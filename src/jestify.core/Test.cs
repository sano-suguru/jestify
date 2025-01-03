using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace jestify.core;

public static class Test
{
    private static readonly TestLogger _testLogger;
    private static readonly TestLifecycleManager _lifecycleManager;
    private static int _defaultTimeout = 5000;
    private static bool _isInitialized;

    static Test()
    {
        _testLogger = new TestLogger();
        _lifecycleManager = new TestLifecycleManager(_testLogger);
    }

    /// <summary>
    /// テストフレームワークの設定を構成します。
    /// </summary>
    public static void Configure(Action<TestConfiguration> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var config = new TestConfiguration();
        configure(config);

        if (config.Logger != null)
        {
            _testLogger.SetLogger(config.Logger);
        }
        else if (config.LoggerConfiguration != null)
        {
            _testLogger.SetLogger(LoggerFactory.Create(config.LoggerConfiguration)
                .CreateLogger(nameof(Test)));
        }

        _defaultTimeout = config.DefaultTimeout;
        _isInitialized = true;
    }

    private static void Initialize()
    {
        if (!_isInitialized)
        {
            Configure(_ => { });
        }
    }

    /// <summary>
    /// ロガーを設定します。
    /// </summary>
    public static void SetLogger(ILogger logger)
    {
        Initialize();
        _testLogger.SetLogger(logger);
    }

    /// <summary>
    /// デフォルトのタイムアウト時間を設定します。
    /// </summary>
    public static void SetTimeout(int timeoutMs)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutMs, 0);
        _defaultTimeout = timeoutMs;
    }

    /// <summary>
    /// テストスイートを定義します。
    /// </summary>
    public static async Task Describe(string title, Func<Task> suite, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentNullException.ThrowIfNull(suite);
        Initialize();

        _testLogger.LogSuiteStart(title);
        var currentSuite = new TestSuite(title);
        _lifecycleManager.PushSuite(currentSuite);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_defaultTimeout);

            await _lifecycleManager.ExecuteBeforeAllHooks(currentSuite, timeoutCts.Token);
            await suite().WaitAsync(timeoutCts.Token);
        }
        finally
        {
            try
            {
                await _lifecycleManager.ExecuteAfterAllHooks(currentSuite, cancellationToken);
            }
            catch (Exception ex)
            {
                _testLogger.LogHookError("AfterAll", currentSuite.Title, ex);
            }
            _lifecycleManager.PopSuite();
        }
    }

    /// <summary>
    /// 同期スイートのオーバーロード
    /// </summary>
    public static Task Describe(string title, Action suite, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(suite);
        return Describe(title, () => { suite(); return Task.CompletedTask; }, cancellationToken);
    }

    /// <summary>
    /// 非同期メソッドのテストを定義します。
    /// </summary>
    public static async Task It(string title, Func<Task> test, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentNullException.ThrowIfNull(test);
        Initialize();

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_defaultTimeout);

            await _lifecycleManager.ExecuteBeforeEachHooks(timeoutCts.Token);

            try
            {
                await test().WaitAsync(timeoutCts.Token);
                stopwatch.Stop();
                _testLogger.LogTestSuccess(title, stopwatch.ElapsedMilliseconds);
            }
            finally
            {
                await _lifecycleManager.ExecuteAfterEachHooks(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _testLogger.LogTestTimeout(title, _defaultTimeout);
            throw new TestTimeoutException(title, TimeSpan.FromMilliseconds(_defaultTimeout));
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _testLogger.LogTestCancelled(title, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _testLogger.LogTestFailure(title, stopwatch.ElapsedMilliseconds, ex);
            throw new TestFailureException(title, TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds), ex);
        }
    }

    /// <summary>
    /// 同期メソッドのテストを定義します。
    /// </summary>
    public static Task It(string title, Action test, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(test);
        return It(title, () => { test(); return Task.CompletedTask; }, cancellationToken);
    }

    /// <summary>
    /// データ駆動テストを実行します。
    /// </summary>
    public static async Task Each<T>(
        string title,
        IEnumerable<T> cases,
        Func<T, Task> test,
        bool parallel = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentNullException.ThrowIfNull(cases);
        ArgumentNullException.ThrowIfNull(test);
        Initialize();

        if (parallel)
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(cases, options, async (item, ct) =>
                await It($"{title} - {item}", async () => await test(item), ct));
        }
        else
        {
            foreach (var item in cases)
            {
                await It($"{title} - {item}", async () => await test(item), cancellationToken);
            }
        }
    }

    /// <summary>
    /// 同期データ駆動テストのオーバーロード
    /// </summary>
    public static Task Each<T>(
        string title,
        IEnumerable<T> cases,
        Action<T> test,
        bool parallel = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(test);
        return Each(title, cases, item => { test(item); return Task.CompletedTask; }, parallel, cancellationToken);
    }

    /// <summary>
    /// スイート実行前に1回だけ実行するセットアップを定義します。
    /// </summary>
    public static void BeforeAll(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddBeforeAllHook(action);
    }

    /// <summary>
    /// スイート実行前に1回だけ実行する非同期セットアップのオーバーロード
    /// </summary>
    public static void BeforeAll(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddBeforeAllHook(ct => action());
    }

    /// <summary>
    /// スイート実行前に1回だけ実行する同期セットアップのオーバーロード
    /// </summary>
    public static void BeforeAll(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddBeforeAllHook(ct => { action(); return Task.CompletedTask; });
    }

    /// <summary>
    /// スイート実行後に1回だけ実行するティアダウンを定義します。
    /// </summary>
    public static void AfterAll(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddAfterAllHook(action);
    }

    /// <summary>
    /// スイート実行後に1回だけ実行する非同期ティアダウンのオーバーロード
    /// </summary>
    public static void AfterAll(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddAfterAllHook(ct => action());
    }

    /// <summary>
    /// スイート実行後に1回だけ実行する同期ティアダウンのオーバーロード
    /// </summary>
    public static void AfterAll(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddAfterAllHook(ct => { action(); return Task.CompletedTask; });
    }

    /// <summary>
    /// 各テスト実行前に実行するセットアップを定義します。
    /// </summary>
    public static void BeforeEach(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddBeforeEachHook(action);
    }

    /// <summary>
    /// 各テスト実行前に実行する非同期セットアップのオーバーロード
    /// </summary>
    public static void BeforeEach(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddBeforeEachHook(ct => action());
    }

    /// <summary>
    /// 各テスト実行前に実行する同期セットアップのオーバーロード
    /// </summary>
    public static void BeforeEach(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddBeforeEachHook(ct => { action(); return Task.CompletedTask; });
    }

    /// <summary>
    /// 各テスト実行後に実行するティアダウンを定義します。
    /// </summary>
    public static void AfterEach(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddAfterEachHook(action);
    }

    /// <summary>
    /// 各テスト実行後に実行する非同期ティアダウンのオーバーロード
    /// </summary>
    public static void AfterEach(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddAfterEachHook(ct => action());
    }

    /// <summary>
    /// 各テスト実行後に実行する同期ティアダウンのオーバーロード
    /// </summary>
    public static void AfterEach(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Initialize();
        _lifecycleManager.AddAfterEachHook(ct => { action(); return Task.CompletedTask; });
    }

    /// <summary>
    /// テストをスキップします。
    /// </summary>
    public static void Skip(string title, string? reason = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        Initialize();
        _testLogger.LogTestSkipped(title, reason);
    }

    /// <summary>
    /// モックを作成します。
    /// </summary>
    public static Mock<T> Mock<T>() where T : class => new();
}
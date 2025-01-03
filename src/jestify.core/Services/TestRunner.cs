using System.Diagnostics;

namespace jestify;

/// <summary>
/// テストの実行を制御するランナークラス
/// </summary>
public class TestRunner
{
    private readonly ITestLogger _logger;
    private readonly ITestLifecycleManager _lifecycleManager;
    private readonly ParallelOptions _defaultParallelOptions;
    private readonly AsyncLocal<TestContext?> _currentContext = new();
    private int _defaultTimeout;

    public TestRunner(
        ITestLogger logger,
        ITestLifecycleManager lifecycleManager,
        int defaultTimeout = 5000)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(lifecycleManager);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(defaultTimeout, 0);

        _logger = logger;
        _lifecycleManager = lifecycleManager;
        _defaultTimeout = defaultTimeout;
        _defaultParallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
    }

    /// <summary>
    /// 現在のテストコンテキストを取得します。
    /// </summary>
    public TestContext? CurrentContext => _currentContext.Value;

    /// <summary>
    /// デフォルトのタイムアウト時間を設定します。
    /// </summary>
    public void SetTimeout(int timeoutMs)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeoutMs, 0);
        _defaultTimeout = timeoutMs;
    }

    /// <summary>
    /// テストスイートを実行します。
    /// </summary>
    public async Task RunSuite(string title, Func<CancellationToken, Task> suite, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentNullException.ThrowIfNull(suite);

        _logger.LogSuiteStart(title);
        var currentSuite = new TestSuite(title);
        _lifecycleManager.PushSuite(currentSuite);

        var previousContext = _currentContext.Value;
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_defaultTimeout);

            _currentContext.Value = new TestContext(
                currentSuite,
                title,
                _defaultTimeout,
                timeoutCts.Token);

            await _lifecycleManager.ExecuteBeforeAllHooks(currentSuite, timeoutCts.Token);
            await suite(timeoutCts.Token);
        }
        finally
        {
            try
            {
                await _lifecycleManager.ExecuteAfterAllHooks(currentSuite, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogHookError("AfterAll", title, ex);
            }
            _lifecycleManager.PopSuite();
            _currentContext.Value = previousContext;
        }
    }

    /// <summary>
    /// 個別のテストケースを実行します。
    /// </summary>
    public async Task RunTest(string title, Func<CancellationToken, Task> test, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentNullException.ThrowIfNull(test);

        var stopwatch = Stopwatch.StartNew();
        var previousContext = _currentContext.Value;

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_defaultTimeout);

            var currentSuite = _lifecycleManager.GetCurrentSuite();
            _currentContext.Value = new TestContext(
                currentSuite,
                title,
                _defaultTimeout,
                timeoutCts.Token);

            await _lifecycleManager.ExecuteBeforeEachHooks(timeoutCts.Token);

            try
            {
                await test(timeoutCts.Token);
                stopwatch.Stop();
                _logger.LogTestSuccess(title, stopwatch.ElapsedMilliseconds);
            }
            finally
            {
                await _lifecycleManager.ExecuteAfterEachHooks(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            _logger.LogTestTimeout(title, _defaultTimeout);
            throw new TestTimeoutException(title, TimeSpan.FromMilliseconds(_defaultTimeout));
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogTestCancelled(title, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogTestFailure(title, stopwatch.ElapsedMilliseconds, ex);
            throw new TestFailureException(title, TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds), ex);
        }
        finally
        {
            _currentContext.Value = previousContext;
        }
    }

    /// <summary>
    /// データ駆動テストを実行します。
    /// </summary>
    public async Task RunEach<T>(
        string titleFormat,
        IEnumerable<T> cases,
        Func<T, CancellationToken, Task> test,
        bool parallel = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(titleFormat);
        ArgumentNullException.ThrowIfNull(cases);
        ArgumentNullException.ThrowIfNull(test);

        if (parallel)
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = _defaultParallelOptions.MaxDegreeOfParallelism,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(cases, options, async (item, ct) =>
            {
                var title = string.Format(titleFormat, item);
                await this.RunTest(title, async innerCt => await test(item, innerCt), ct);
            });
        }
        else
        {
            foreach (var item in cases)
            {
                var title = string.Format(titleFormat, item);
                await this.RunTest(title, async ct => await test(item, ct), cancellationToken);
            }
        }
    }

    /// <summary>
    /// テストをスキップとしてマークします。
    /// </summary>
    public void Skip(string title, string? reason = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        _logger.LogTestSkipped(title, reason);
    }
}

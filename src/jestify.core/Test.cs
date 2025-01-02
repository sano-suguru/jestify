using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace jestify.core;

public static class Test
{
    private static readonly AsyncLocal<ILogger> _logger = new();
    private static readonly AsyncLocal<Func<Task>?> _setup = new();
    private static readonly AsyncLocal<Func<Task>?> _teardown = new();

    /// <summary>
    /// ロガーを設定します。
    /// </summary>
    public static void SetLogger(ILogger logger) => _logger.Value = logger;

    /// <summary>
    /// グローバルセットアップメソッド
    /// </summary>
    public static void Setup(Func<Task> action) => _setup.Value = action;

    /// <summary>
    /// 同期セットアップメソッドのオーバーロード
    /// </summary>
    public static void Setup(Action action) => _setup.Value = () => Task.Run(action);

    /// <summary>
    /// グローバルティアダウンメソッド
    /// </summary>
    public static void Teardown(Func<Task> action) => _teardown.Value = action;

    /// <summary>
    /// 同期ティアダウンメソッドのオーバーロード
    /// </summary>
    public static void Teardown(Action action) => _teardown.Value = () => Task.Run(action);

    /// <summary>
    /// テストスイートを定義します。
    /// </summary>
    public static async Task Describe(string title, Func<Task> suite, CancellationToken cancellationToken = default)
    {
        EnsureLogger();
        _logger.Value!.LogInformation("[Describe] {Title}", title);

        try
        {
            if (_setup.Value != null)
                await _setup.Value().WaitAsync(cancellationToken);

            await suite().WaitAsync(cancellationToken);
        }
        finally
        {
            if (_teardown.Value != null)
                await _teardown.Value().WaitAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 同期スイートのオーバーロード
    /// </summary>
    public static Task Describe(string title, Action suite, CancellationToken cancellationToken = default)
        => Describe(title, () => Task.Run(suite), cancellationToken);

    /// <summary>
    /// 非同期メソッドのテストを定義します。
    /// </summary>
    public static async Task It(string title, Func<Task> test, CancellationToken cancellationToken = default)
    {
        EnsureLogger();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await test().WaitAsync(cancellationToken);
            stopwatch.Stop();
            _logger.Value!.LogInformation("✔ {Title} (Completed in {Elapsed} ms)",
                title, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.Value!.LogWarning("⚠ {Title} (Cancelled after {Elapsed} ms)",
                title, stopwatch.ElapsedMilliseconds);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var duration = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            _logger.Value!.LogError(ex, "✘ {Title} (Failed after {Elapsed} ms)",
                title, stopwatch.ElapsedMilliseconds);
            throw new TestFailureException(title, duration, ex);
        }
    }

    /// <summary>
    /// 同期メソッドのテストを定義します。
    /// </summary>
    public static Task It(string title, Action test, CancellationToken cancellationToken = default)
        => It(title, () => Task.Run(test), cancellationToken);

    /// <summary>
    /// 指定時間内に非同期処理が完了するかを確認します。
    /// </summary>
    public static async Task Timeout(string title, Func<Task> test, int timeoutMs)
    {
        EnsureLogger();
        using var cts = new CancellationTokenSource(timeoutMs);

        try
        {
            await test().WaitAsync(cts.Token);
            _logger.Value!.LogInformation("✔ {Title} (Completed within {Timeout} ms)",
                title, timeoutMs);
        }
        catch (OperationCanceledException)
        {
            _logger.Value!.LogWarning("⚠ {Title} (Timed out after {Timeout} ms)",
                title, timeoutMs);
            throw;
        }
        catch (Exception ex)
        {
            _logger.Value!.LogError(ex, "✘ {Title} (Error during timeout)", title);
            throw;
        }
    }

    /// <summary>
    /// データ駆動テストを実行します。
    /// </summary>
    public static async Task Each<T>(
        string title,
        IEnumerable<T> cases,
        Func<T, Task> test,
        ParallelOptions? parallelOptions = null,
        CancellationToken cancellationToken = default)
    {
        EnsureLogger();

        if (parallelOptions != null)
        {
            await Parallel.ForEachAsync(cases, parallelOptions, async (item, ct)
                => await It($"{title} - {item}", async () => await test(item), ct));
        }
        else
        {
            foreach (T? item in cases)
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
        ParallelOptions? parallelOptions = null,
        CancellationToken cancellationToken = default)
        => Each(title, cases, item => Task.Run(() => test(item)), parallelOptions, cancellationToken);

    /// <summary>
    /// テストをスキップします。
    /// </summary>
    public static void Skip(string title, string reason)
    {
        EnsureLogger();
        _logger.Value!.LogInformation("⏭ {Title} (Skipped: {Reason})", title, reason);
    }

    /// <summary>
    /// モックを作成します。
    /// </summary>
    public static Mock<T> Mock<T>() where T : class => new();

    private static void EnsureLogger()
    {
        _logger.Value ??= LoggerFactory.Create(builder => builder.AddConsole())
                .CreateLogger(nameof(Test));
    }
}

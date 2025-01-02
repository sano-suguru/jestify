using Microsoft.Extensions.Logging;

namespace jestify.core;

public class TestLogger
{
    private readonly AsyncLocal<ILogger> _logger = new();

    /// <summary>
    /// ロガーを設定します。
    /// </summary>
    public void SetLogger(ILogger logger) => _logger.Value = logger;

    /// <summary>
    /// テストスイートの開始をログに記録します。
    /// </summary>
    public void LogSuiteStart(string title) 
        => _logger.Value?.LogInformation("[Describe] {Title}", title);

    /// <summary>
    /// テストの成功をログに記録します。
    /// </summary>
    public void LogTestSuccess(string title, long elapsedMs)
    {
        _logger.Value?.LogInformation("✔ {Title} (Completed in {Elapsed} ms)",
            title, elapsedMs);
    }

    /// <summary>
    /// テストのタイムアウトをログに記録します。
    /// </summary>
    public void LogTestTimeout(string title, int timeoutMs)
    {
        _logger.Value?.LogWarning("⚠ {Title} (Timed out after {Timeout} ms)",
            title, timeoutMs);
    }

    /// <summary>
    /// テストのキャンセルをログに記録します。
    /// </summary>
    public void LogTestCancelled(string title, long elapsedMs)
    {
        _logger.Value?.LogWarning("⚠ {Title} (Cancelled after {Elapsed} ms)",
            title, elapsedMs);
    }

    /// <summary>
    /// テストの失敗をログに記録します。
    /// </summary>
    public void LogTestFailure(string title, long elapsedMs, Exception exception)
    {
        _logger.Value?.LogError(exception, "✘ {Title} (Failed after {Elapsed} ms)",
            title, elapsedMs);
    }

    /// <summary>
    /// テストのスキップをログに記録します。
    /// </summary>
    public void LogTestSkipped(string title, string? reason = null)
    {
        _logger.Value?.LogInformation("⏭ {Title} {Reason}",
            title, reason != null ? $"(Skipped: {reason})" : "(Skipped)");
    }

    /// <summary>
    /// BeforeAll/AfterAllフックのエラーをログに記録します。
    /// </summary>
    public void LogHookError(string hookType, string suiteTitle, Exception exception)
    {
        _logger.Value?.LogError(exception, "Error in {HookType} hook for suite: {SuiteTitle}",
            hookType, suiteTitle);
    }

    /// <summary>
    /// BeforeEach/AfterEachフックのエラーをログに記録します。
    /// </summary>
    public void LogEachHookError(string hookType, Exception exception)
    {
        _logger.Value?.LogError(exception, "Error in {HookType} hook",
            hookType);
    }

    /// <summary>
    /// デバッグ情報をログに記録します。
    /// </summary>
    public void LogDebug(string message, params object[] args)
    {
        _logger.Value?.LogDebug(message, args);
    }

    /// <summary>
    /// ロガーが設定されていない場合にデフォルトのコンソールロガーを設定します。
    /// </summary>
    public void EnsureLogger()
    {
        _logger.Value ??= LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger(nameof(TestLogger));
    }
}

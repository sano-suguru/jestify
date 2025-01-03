using Microsoft.Extensions.Logging;

namespace jestify;

public class TestLogger : ITestLogger
{
    private readonly AsyncLocal<ILogger> _logger = new();

    /// <inheritdoc />
    public void SetLogger(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger.Value = logger;
    }

    /// <inheritdoc />
    public void LogSuiteStart(string title)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        _logger.Value?.LogInformation("[Describe] {Title}", title);
    }

    /// <inheritdoc />
    public void LogTestSuccess(string title, long elapsedMs)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        _logger.Value?.LogInformation("✔ {Title} (Completed in {Elapsed} ms)",
            title, elapsedMs);
    }

    /// <inheritdoc />
    public void LogTestTimeout(string title, int timeoutMs)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        _logger.Value?.LogWarning("⚠ {Title} (Timed out after {Timeout} ms)",
            title, timeoutMs);
    }

    /// <inheritdoc />
    public void LogTestCancelled(string title, long elapsedMs)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        _logger.Value?.LogWarning("⚠ {Title} (Cancelled after {Elapsed} ms)",
            title, elapsedMs);
    }

    /// <inheritdoc />
    public void LogTestFailure(string title, long elapsedMs, Exception exception)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentNullException.ThrowIfNull(exception);
        _logger.Value?.LogError(exception, "✘ {Title} (Failed after {Elapsed} ms)",
            title, elapsedMs);
    }

    /// <inheritdoc />
    public void LogTestSkipped(string title, string? reason = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        _logger.Value?.LogInformation("⏭ {Title} {Reason}",
            title, reason != null ? $"(Skipped: {reason})" : "(Skipped)");
    }

    /// <inheritdoc />
    public void LogHookError(string hookType, string suiteTitle, Exception exception)
    {
        ArgumentException.ThrowIfNullOrEmpty(hookType);
        ArgumentException.ThrowIfNullOrEmpty(suiteTitle);
        ArgumentNullException.ThrowIfNull(exception);
        _logger.Value?.LogError(exception, "Error in {HookType} hook for suite: {SuiteTitle}",
            hookType, suiteTitle);
    }

    /// <inheritdoc />
    public void LogEachHookError(string hookType, Exception exception)
    {
        ArgumentException.ThrowIfNullOrEmpty(hookType);
        ArgumentNullException.ThrowIfNull(exception);
        _logger.Value?.LogError(exception, "Error in {HookType} hook",
            hookType);
    }

    /// <inheritdoc />
    public void LogDebug(string message, params object[] args)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        _logger.Value?.LogDebug(message, args);
    }

    /// <inheritdoc />
    public void EnsureLogger()
    {
        _logger.Value ??= LoggerFactory.Create(builder => builder.AddConsole())
            .CreateLogger(nameof(TestLogger));
    }
}

using Microsoft.Extensions.Logging;

namespace jestify;

/// <summary>
/// テストの実行結果をログに記録するインターフェース
/// </summary>
public interface ITestLogger
{
    /// <summary>
    /// ロガーを設定します。
    /// </summary>
    void SetLogger(ILogger logger);

    /// <summary>
    /// テストスイートの開始をログに記録します。
    /// </summary>
    void LogSuiteStart(string title);

    /// <summary>
    /// テストの成功をログに記録します。
    /// </summary>
    void LogTestSuccess(string title, long elapsedMs);

    /// <summary>
    /// テストのタイムアウトをログに記録します。
    /// </summary>
    void LogTestTimeout(string title, int timeoutMs);

    /// <summary>
    /// テストのキャンセルをログに記録します。
    /// </summary>
    void LogTestCancelled(string title, long elapsedMs);

    /// <summary>
    /// テストの失敗をログに記録します。
    /// </summary>
    void LogTestFailure(string title, long elapsedMs, Exception exception);

    /// <summary>
    /// テストのスキップをログに記録します。
    /// </summary>
    void LogTestSkipped(string title, string? reason = null);

    /// <summary>
    /// BeforeAll/AfterAllフックのエラーをログに記録します。
    /// </summary>
    void LogHookError(string hookType, string suiteTitle, Exception exception);

    /// <summary>
    /// BeforeEach/AfterEachフックのエラーをログに記録します。
    /// </summary>
    void LogEachHookError(string hookType, Exception exception);

    /// <summary>
    /// デバッグ情報をログに記録します。
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// ロガーが設定されていない場合にデフォルトのコンソールロガーを設定します。
    /// </summary>
    void EnsureLogger();
}
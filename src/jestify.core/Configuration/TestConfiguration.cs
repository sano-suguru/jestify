using Microsoft.Extensions.Logging;

namespace jestify;

/// <summary>
/// テストフレームワークの設定を定義します。
/// </summary>
public class TestConfiguration
{
    /// <summary>
    /// カスタムロガーを設定します。
    /// </summary>
    public ILogger? Logger { get; set; } 

    /// <summary>
    /// ロギングの設定を行います。
    /// </summary>
    public Action<ILoggingBuilder>? LoggerConfiguration { get; set; }

    /// <summary>
    /// カスタムライフサイクルマネージャーを設定します。
    /// </summary>
    public ITestLifecycleManager? LifecycleManager { get; set; }

    /// <summary>
    /// デフォルトのタイムアウト時間（ミリ秒）を設定します。
    /// </summary>
    public int DefaultTimeout { get; set; } = 5000;
}

namespace jestify;

/// <summary>
/// テストがタイムアウトした場合にスローされる例外
/// </summary>
/// <remarks>
/// 新しい <see cref="TestTimeoutException"/> インスタンスを初期化します。
/// </remarks>
/// <param name="title">テストのタイトル</param>
/// <param name="timeout">タイムアウトまでの制限時間</param>
public class TestTimeoutException(string title, TimeSpan timeout)
    : TestFailureException(title, timeout,
        new OperationCanceledException($"Test '{title}' timed out after {timeout.TotalMilliseconds:F2}ms"))
{
    /// <summary>3
    /// タイムアウトまでの制限時間
    /// </summary>
    public TimeSpan Timeout { get; } = timeout;

    /// <summary>
    /// タイムアウトした操作の説明メッセージを取得します。
    /// </summary>
    public override string Message =>
        $"Test '{this.TestTitle}' timed out after {this.Timeout.TotalMilliseconds:F2}ms. " +
        $"Consider increasing the timeout using Test.SetTimeout() if the test requires more time.";
}
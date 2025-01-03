namespace jestify;

/// <summary>
/// テストの実行コンテキストを表します。
/// </summary>
public class TestContext
{
    /// <summary>
    /// 現在実行中のテストスイート
    /// </summary>
    public TestSuite CurrentSuite { get; }

    /// <summary>
    /// 現在のテストのタイトル
    /// </summary>
    public string CurrentTest { get; }

    /// <summary>
    /// テストのタイムアウト時間（ミリ秒）
    /// </summary>
    public int Timeout { get; }

    /// <summary>
    /// キャンセレーショントークン
    /// </summary>
    public CancellationToken CancellationToken { get; }

    internal TestContext(
        TestSuite currentSuite,
        string currentTest,
        int timeout,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(currentSuite);
        ArgumentException.ThrowIfNullOrEmpty(currentTest);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, 0);

        this.CurrentSuite = currentSuite;
        this.CurrentTest = currentTest;
        this.Timeout = timeout;
        this.CancellationToken = cancellationToken;
    }
}

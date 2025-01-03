namespace jestify;

/// <summary>
/// テストの実行コンテキストを表します。
/// </summary>
public class TestContext
{
    private readonly Dictionary<string, object> _sharedData = [];

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

    /// <summary>
    /// テストコンテキストにデータを設定します。
    /// </summary>
    public void Set<T>(string key, T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _sharedData[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// テストコンテキストからデータを取得します。
    /// </summary>
    public T? Get<T>(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _sharedData.TryGetValue(key, out var value) ? (T)value : default;
    }

    /// <summary>
    /// 指定したキーのデータを削除します。
    /// </summary>
    public bool Remove(string key)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        return _sharedData.Remove(key);
    }

    /// <summary>
    /// すべてのデータをクリアします。
    /// </summary>
    public void Clear() => _sharedData.Clear();
}
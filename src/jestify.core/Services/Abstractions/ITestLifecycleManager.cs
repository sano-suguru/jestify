namespace jestify;

/// <summary>
/// テストのライフサイクルを管理するインターフェース
/// </summary>
public interface ITestLifecycleManager
{
    /// <summary>
    /// 現在のテストスイートを取得します。
    /// </summary>
    TestSuite GetCurrentSuite();

    /// <summary>
    /// 新しいテストスイートをスタックにプッシュします。
    /// </summary>
    void PushSuite(TestSuite suite);

    /// <summary>
    /// 現在のテストスイートをスタックからポップします。
    /// </summary>
    TestSuite PopSuite();

    /// <summary>
    /// BeforeAllフックを実行します。
    /// </summary>
    Task ExecuteBeforeAllHooks(TestSuite suite, CancellationToken cancellationToken);

    /// <summary>
    /// AfterAllフックを実行します。
    /// </summary>
    Task ExecuteAfterAllHooks(TestSuite suite, CancellationToken cancellationToken);

    /// <summary>
    /// BeforeEachフックを実行します。
    /// </summary>
    Task ExecuteBeforeEachHooks(CancellationToken cancellationToken);

    /// <summary>
    /// AfterEachフックを実行します。
    /// </summary>
    Task ExecuteAfterEachHooks(CancellationToken cancellationToken);

    /// <summary>
    /// スイートにBeforeAllフックを追加します。
    /// </summary>
    void AddBeforeAllHook(Func<CancellationToken, Task> action);

    /// <summary>
    /// スイートにAfterAllフックを追加します。
    /// </summary>
    void AddAfterAllHook(Func<CancellationToken, Task> action);

    /// <summary>
    /// スイートにBeforeEachフックを追加します。
    /// </summary>
    void AddBeforeEachHook(Func<CancellationToken, Task> action);

    /// <summary>
    /// スイートにAfterEachフックを追加します。
    /// </summary>
    void AddAfterEachHook(Func<CancellationToken, Task> action);
}

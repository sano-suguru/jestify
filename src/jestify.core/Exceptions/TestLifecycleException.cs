namespace jestify;

/// <summary>
/// テストライフサイクル管理中に発生した例外を表します。
/// </summary>
public class TestLifecycleException : Exception
{
    /// <summary>
    /// 新しい <see cref="TestLifecycleException"/> インスタンスを初期化します。
    /// </summary>
    public TestLifecycleException() { }

    /// <summary>
    /// 指定されたエラーメッセージを使用して、新しい <see cref="TestLifecycleException"/> インスタンスを初期化します。
    /// </summary>
    /// <param name="message">エラーを説明するメッセージ。</param>
    public TestLifecycleException(string message) : base(message) { }

    /// <summary>
    /// 指定されたエラーメッセージと内部例外を使用して、新しい <see cref="TestLifecycleException"/> インスタンスを初期化します。
    /// </summary>
    /// <param name="message">エラーを説明するメッセージ。</param>
    /// <param name="innerException">現在の例外の原因である例外。</param>
    public TestLifecycleException(string message, Exception innerException)
        : base(message, innerException) { }
}

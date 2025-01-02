namespace jestify.core;

public class TestLifecycleManager
{
    private readonly AsyncLocal<Stack<TestSuite>> _suiteStack = new();
    private readonly TestLogger _logger;

    public TestLifecycleManager(TestLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// 現在のテストスイートを取得します。
    /// </summary>
    public TestSuite GetCurrentSuite()
    {
        _suiteStack.Value ??= new Stack<TestSuite>();
        if (_suiteStack.Value.Count == 0)
        {
            throw new InvalidOperationException("Test hooks must be called within a describe block");
        }
        return _suiteStack.Value.Peek();
    }

    /// <summary>
    /// 新しいテストスイートをスタックにプッシュします。
    /// </summary>
    public void PushSuite(TestSuite suite)
    {
        _suiteStack.Value ??= new Stack<TestSuite>();
        _suiteStack.Value.Push(suite);
    }

    /// <summary>
    /// 現在のテストスイートをスタックからポップします。
    /// </summary>
    public TestSuite PopSuite()
    {
        var stack = _suiteStack.Value;
        if (stack == null || stack.Count == 0)
        {
            throw new InvalidOperationException("No test suite to pop");
        }
        return stack.Pop();
    }

    /// <summary>
    /// BeforeAllフックを実行します。
    /// </summary>
    public async Task ExecuteBeforeAllHooks(TestSuite suite, CancellationToken cancellationToken)
    {
        foreach (var hook in suite.BeforeAll)
        {
            await hook(cancellationToken);
        }
    }

    /// <summary>
    /// AfterAllフックを実行します。
    /// </summary>
    public async Task ExecuteAfterAllHooks(TestSuite suite, CancellationToken cancellationToken)
    {
        foreach (var hook in suite.AfterAll)
        {
            try
            {
                await hook(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogHookError("AfterAll", suite.Title, ex);
            }
        }
    }

    /// <summary>
    /// BeforeEachフックを実行します。
    /// </summary>
    public async Task ExecuteBeforeEachHooks(CancellationToken cancellationToken)
    {
        _suiteStack.Value ??= new Stack<TestSuite>();

        // スタック内の全てのスイートのBeforeEachを親から順に実行
        var suites = _suiteStack.Value.Reverse();
        foreach (var currentSuite in suites)
        {
            foreach (var hook in currentSuite.BeforeEach)
            {
                await hook(cancellationToken);
            }
        }
    }

    /// <summary>
    /// AfterEachフックを実行します。
    /// </summary>
    public async Task ExecuteAfterEachHooks(CancellationToken cancellationToken)
    {
        _suiteStack.Value ??= new Stack<TestSuite>();

        // スタック内の全てのスイートのAfterEachを子から順に実行
        foreach (var currentSuite in _suiteStack.Value)
        {
            foreach (var hook in currentSuite.AfterEach)
            {
                try
                {
                    await hook(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogEachHookError("AfterEach", ex);
                }
            }
        }
    }

    /// <summary>
    /// スイートにBeforeAllフックを追加します。
    /// </summary>
    public void AddBeforeAllHook(Func<CancellationToken, Task> action)
    {
        var suite = this.GetCurrentSuite();
        suite.BeforeAll.Add(action);
    }

    /// <summary>
    /// スイートにAfterAllフックを追加します。
    /// </summary>
    public void AddAfterAllHook(Func<CancellationToken, Task> action)
    {
        var suite = this.GetCurrentSuite();
        suite.AfterAll.Add(action);
    }

    /// <summary>
    /// スイートにBeforeEachフックを追加します。
    /// </summary>
    public void AddBeforeEachHook(Func<CancellationToken, Task> action)
    {
        var suite = this.GetCurrentSuite();
        suite.BeforeEach.Add(action);
    }

    /// <summary>
    /// スイートにAfterEachフックを追加します。
    /// </summary>
    public void AddAfterEachHook(Func<CancellationToken, Task> action)
    {
        var suite = this.GetCurrentSuite();
        suite.AfterEach.Add(action);
    }
}

namespace jestify;

public class TestLifecycleManager : ITestLifecycleManager
{
    private readonly AsyncLocal<Stack<TestSuite>> _suiteStack = new();
    private readonly ITestLogger _logger;

    public TestLifecycleManager(ITestLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public TestSuite GetCurrentSuite()
    {
        _suiteStack.Value ??= new Stack<TestSuite>();
        if (_suiteStack.Value.Count == 0)
        {
            throw new TestLifecycleException("Test hooks must be called within a describe block");
        }
        return _suiteStack.Value.Peek();
    }

    /// <inheritdoc />
    public void PushSuite(TestSuite suite)
    {
        ArgumentNullException.ThrowIfNull(suite);
        _suiteStack.Value ??= new Stack<TestSuite>();
        _suiteStack.Value.Push(suite);
    }

    /// <inheritdoc />
    public TestSuite PopSuite()
    {
        var stack = _suiteStack.Value;
        if (stack == null || stack.Count == 0)
        {
            throw new TestLifecycleException("No test suite to pop");
        }
        return stack.Pop();
    }

    /// <inheritdoc />
    public async Task ExecuteBeforeAllHooks(TestSuite suite, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(suite);

        foreach (var hook in suite.BeforeAll)
        {
            await hook(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task ExecuteAfterAllHooks(TestSuite suite, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(suite);

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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void AddBeforeAllHook(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var suite = this.GetCurrentSuite();
        suite.BeforeAll.Add(action);
    }

    /// <inheritdoc />
    public void AddAfterAllHook(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var suite = this.GetCurrentSuite();
        suite.AfterAll.Add(action);
    }

    /// <inheritdoc />
    public void AddBeforeEachHook(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var suite = this.GetCurrentSuite();
        suite.BeforeEach.Add(action);
    }

    /// <inheritdoc />
    public void AddAfterEachHook(Func<CancellationToken, Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var suite = this.GetCurrentSuite();
        suite.AfterEach.Add(action);
    }
}

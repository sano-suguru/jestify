namespace jestify;

public class TestSuite(string title)
{
    public string Title { get; } = title;
    public List<Func<CancellationToken, Task>> BeforeAll { get; } = [];
    public List<Func<CancellationToken, Task>> AfterAll { get; } = [];
    public List<Func<CancellationToken, Task>> BeforeEach { get; } = [];
    public List<Func<CancellationToken, Task>> AfterEach { get; } = [];
}

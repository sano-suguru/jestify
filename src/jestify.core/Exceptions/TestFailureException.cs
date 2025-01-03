namespace jestify;

public class TestFailureException(
    string title, TimeSpan duration, Exception inner)
    : Exception($"Test '{title}' failed after {duration.TotalMilliseconds:F2}ms", inner)
{
    public string TestTitle { get; } = title;
    public TimeSpan Duration { get; } = duration;
}

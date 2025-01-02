# Jestify

[日本語](../README.md) | [English](README.en.md)

A testing framework that brings Jest's elegance to C#.  
Write smart test code with Jest-like intuitive syntax, free from verbose naming conventions.

## Simple and Intuitive API

```csharp
await Describe("Calculator", async () =>
{
    var calculator = new Calculator();

    BeforeEach(() =>
    {
        calculator.Reset();
    });

    await It("should perform addition", () =>
    {
        var result = calculator.Add(2, 3);
        Assert.Equal(5, result);
    });
});
```

## Features

- **Jest-like Syntax**: Intuitive test writing with `Describe`/`It`
- **Simple API**: Minimal design free from unnecessary decorations
- **Powerful Async Support**: Full async/await support
- **Rich Lifecycle Hooks**: `BeforeAll`/`AfterAll`/`BeforeEach`/`AfterEach`
- **Data-Driven Tests**: Concise iteration tests with `Each`
- **Modern Features**: Timeout control, cancellation, parallel execution
- **Clear Output**: Visually appealing colored console output

## Why Jestify?

Unlike traditional C# testing frameworks, Jestify offers:

- **No Redundant Attributes**: No need for `[TestMethod]` or `[TestClass]`
- **Intuitive Structure**: Easy test grouping with `Describe`
- **Simple Async**: Use async/await without special attributes
- **Flexible Setup**: Flexible test environment setup with hooks

## Installation

```bash
dotnet add package jestify
```

## Practical Examples

### Using Lifecycle Hooks

```csharp
await Describe("Database Tests", async () =>
{
    Database db = null!;

    BeforeAll(async () =>
    {
        db = await Database.CreateAsync();
        await db.MigrateAsync();
    });

    AfterAll(async () =>
    {
        await db.DisposeAsync();
    });

    BeforeEach(async () =>
    {
        await db.ClearAsync();
    });

    await It("should save data", async () =>
    {
        await db.SaveAsync(new TestData());
        Assert.Equal(1, await db.CountAsync());
    });
});
```

### Data-Driven Tests

```csharp
await Each(
    "should smartly test different cases: {0}",
    new[] { 1, 2, 3, 4, 5 },
    async (number, ct) => 
    {
        var result = await Calculate(number);
        Assert.True(result > 0);
    }
);
```

### Parallel Tests

```csharp
await Each(
    "parallel test {0}",
    Enumerable.Range(1, 100),
    async (number, ct) => 
    {
        await Task.Delay(100, ct);
        Assert.True(number > 0);
    },
    parallel: true  // Simple parallel execution
);
```

### Timeout and Skip

```csharp
// Set global timeout
SetTimeout(10000); // 10 seconds

// Conditional skip
if (!isDatabaseAvailable)
{
    Skip("Database Tests", "DB connection not available");
}
```

## Best Practices

1. **Clear Test Titles**
   - Use titles that clearly indicate the test's purpose
   - Include expected results in the title

2. **Test Independence**
   - Each test should be independent
   - Create a clean state with BeforeEach

3. **Resource Management**
   - Clean up reliably with AfterAll/AfterEach
   - Utilize using statements and Dispose pattern

4. **Async Handling**
   - Use await appropriately
   - Leverage cancellation tokens

5. **Performance**
   - Set appropriate timeouts for long-running tests
   - Use parallel execution when needed

## Contributing

Contributions are welcome! Feel free to submit Pull Requests.

## License

MIT License - See [LICENSE](../LICENSE) file for details.
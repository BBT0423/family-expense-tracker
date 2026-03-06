using Xunit;

namespace SummaryYearSalary.Tests.Fixtures;

/// <summary>
/// Base class for test classes providing common functionality.
/// </summary>
public abstract class BaseTest
{
    /// <summary>
    /// Asserts that a string is not null or empty.
    /// </summary>
    protected static void AssertNotNullOrEmpty(string? value, string message)
    {
        Assert.False(string.IsNullOrEmpty(value), message);
    }

    /// <summary>
    /// Asserts that a string is null or empty.
    /// </summary>
    protected static void AssertNullOrEmpty(string? value, string message)
    {
        Assert.True(string.IsNullOrEmpty(value), message);
    }

    /// <summary>
    /// Asserts that a collection is not empty.
    /// </summary>
    protected static void AssertNotEmpty<T>(IEnumerable<T> collection, string message)
    {
        Assert.NotEmpty(collection);
    }
}

/// <summary>
/// Fixture collection for grouping related tests.
/// </summary>
[CollectionDefinition("API Tests")]
public class ApiTestCollection : ICollectionFixture<TestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to define the traits on the collection.
}

/// <summary>
/// Reusable test fixture for API tests.
/// Can include shared resources like database connections, mocks, etc.
/// </summary>
public class TestFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        // Initialize shared resources
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        // Clean up shared resources
        await Task.CompletedTask;
    }
}

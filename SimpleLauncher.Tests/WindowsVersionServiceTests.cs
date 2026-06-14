using SimpleLauncher.Services.DebugAndBugReport;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="WindowsVersionService"/> which returns a human-readable Windows version string.
/// </summary>
public class WindowsVersionServiceTests
{
    private readonly WindowsVersionService _service = new();

    [Fact]
    public void GetVersionReturnsNonEmptyString()
    {
        var result = _service.GetVersion();
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void GetVersionReturnsKnownVersionString()
    {
        var result = _service.GetVersion();
        // On modern Windows, should return one of the known strings
        var knownVersions = new[]
        {
            "Windows 10 or Windows 11",
            "Windows 8.1",
            "Windows 8",
            "Windows 7"
        };

        // Either it's a known version or it starts with "Unknown Windows Version"
        Assert.True(
            knownVersions.Contains(result) || result.StartsWith("Unknown Windows Version", StringComparison.Ordinal),
            $"Unexpected version string: {result}");
    }

    [Fact]
    public void GetVersionDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.GetVersion());
        Assert.Null(ex);
    }

    [Fact]
    public void GetVersionReturnsConsistentResults()
    {
        var result1 = _service.GetVersion();
        var result2 = _service.GetVersion();
        Assert.Equal(result1, result2);
    }
}

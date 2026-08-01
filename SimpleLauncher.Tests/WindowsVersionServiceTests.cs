using SimpleLauncher.Services.DebugAndBugReport;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="WindowsVersionService"/> which returns a human-readable Windows version string.
/// </summary>
public class WindowsVersionServiceTests
{
    private readonly WindowsVersionService _service = new();

    /// <summary>
    /// Verifies that <see cref="WindowsVersionService.GetVersion"/> returns a non-empty, non-whitespace string.
    /// </summary>
    [Fact]
    public void GetVersionReturnsNonEmptyString()
    {
        var result = _service.GetVersion();
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    /// <summary>
    /// Verifies that <see cref="WindowsVersionService.GetVersion"/> returns a recognized Windows version string.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="WindowsVersionService.GetVersion"/> does not throw any exceptions.
    /// </summary>
    [Fact]
    public void GetVersionDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.GetVersion());
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that <see cref="WindowsVersionService.GetVersion"/> returns the same result on repeated calls.
    /// </summary>
    [Fact]
    public void GetVersionReturnsConsistentResults()
    {
        var result1 = _service.GetVersion();
        var result2 = _service.GetVersion();
        Assert.Equal(result1, result2);
    }
}

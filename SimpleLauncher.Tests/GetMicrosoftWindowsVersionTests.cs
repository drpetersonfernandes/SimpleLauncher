using SimpleLauncher.Services.DebugAndBugReport;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="GetMicrosoftWindowsVersion"/> utility class.
/// </summary>
public class GetMicrosoftWindowsVersionTests
{
    /// <summary>
    /// Verifies that GetVersion returns a non-empty string.
    /// </summary>
    [Fact]
    public void GetVersionReturnsNonEmptyString()
    {
        var result = GetMicrosoftWindowsVersion.GetVersion();
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    /// <summary>
    /// Verifies that GetVersion returns a string starting with the expected prefix.
    /// </summary>
    [Fact]
    public void GetVersionContainsExpectedPrefix()
    {
        var result = GetMicrosoftWindowsVersion.GetVersion();
        Assert.True(
            result.StartsWith("Windows", StringComparison.OrdinalIgnoreCase) ||
            result.StartsWith("Unknown Windows Version", StringComparison.OrdinalIgnoreCase),
            $"Expected result to start with 'Windows' or 'Unknown Windows Version', but was: {result}");
    }
}

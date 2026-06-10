using SimpleLauncher.Services.DebugAndBugReport;
using Xunit;

namespace SimpleLauncher.Tests;

public class GetMicrosoftWindowsVersionTests
{
    [Fact]
    public void GetVersionReturnsNonEmptyString()
    {
        var result = GetMicrosoftWindowsVersion.GetVersion();
        Assert.False(string.IsNullOrWhiteSpace(result));
    }

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

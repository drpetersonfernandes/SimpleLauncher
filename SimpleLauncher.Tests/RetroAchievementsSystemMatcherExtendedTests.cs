using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.RetroAchievements;
using Xunit;

namespace SimpleLauncher.Tests;

public class RetroAchievementsSystemMatcherExtendedTests
{
    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpDebugLogger : IDebugLogger
    {
        public void Log(string message)
        {
        }

        public void LogException(Exception ex, string? contextMessage = null)
        {
        }
    }

    private readonly RetroAchievementsSystemMatcher _matcher = new(new NoOpLogErrors(), new NoOpDebugLogger());

    [Fact]
    public void GetBestMatchSystemNameWithExactMatch()
    {
        var result = _matcher.GetBestMatchSystemName("NES");
        Assert.Equal("nintendo entertainment system", result);
    }

    [Fact]
    public void GetBestMatchSystemNameWithAlias()
    {
        var result = _matcher.GetBestMatchSystemName("Nintendo");
        Assert.NotNull(result);
    }

    [Fact]
    public void GetBestMatchSystemNameWithUnknownReturnsNormalizedInput()
    {
        var result = _matcher.GetBestMatchSystemName("UnknownSystem12345");
        Assert.Equal("unknownsystem12345", result);
    }

    [Fact]
    public void IsOfficialSystemNameWithValidName()
    {
        var result = _matcher.IsOfficialSystemName("nintendo entertainment system");
        Assert.True(result);
    }

    [Fact]
    public void IsOfficialSystemNameWithInvalidName()
    {
        var result = _matcher.IsOfficialSystemName("UnknownSystem12345");
        Assert.False(result);
    }

    [Fact]
    public void GetSupportedSystemNamesReturnsNonEmpty()
    {
        var names = _matcher.GetSupportedSystemNames();
        Assert.NotNull(names);
        Assert.NotEmpty(names);
    }

    [Fact]
    public void GetSupportedSystemNamesContainsKnownSystems()
    {
        var names = _matcher.GetSupportedSystemNames();
        Assert.Contains("nintendo entertainment system", names);
    }

    [Fact]
    public void GetSystemIdWithValidSystem()
    {
        var id = _matcher.GetSystemId("NES");
        Assert.True(id > 0);
    }

    [Fact]
    public void GetSystemIdWithInvalidSystemReturnsZero()
    {
        var id = _matcher.GetSystemId("UnknownSystem12345");
        Assert.Equal(-1, id);
    }

    [Fact]
    public void IsSystemInMappingsWithValidSystem()
    {
        var result = _matcher.IsSystemInMappings("NES");
        Assert.True(result);
    }

    [Fact]
    public void IsSystemInMappingsWithInvalidSystem()
    {
        var result = _matcher.IsSystemInMappings("UnknownSystem12345");
        Assert.False(result);
    }

    [Fact]
    public void GetExactAliasMatchReturnsNullForUnknown()
    {
        var result = _matcher.GetExactAliasMatch("UnknownSystem12345");
        Assert.Null(result);
    }

    [Fact]
    public void SupportedSystemsCountIsReasonable()
    {
        var names = _matcher.GetSupportedSystemNames();
        Assert.True(names.Count >= 10, $"Expected at least 10 supported systems, got {names.Count}");
    }
}

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

    [Fact]
    public void GetBestMatchSystemNameWithExactMatch()
    {
        var result = RetroAchievementsSystemMatcher.GetBestMatchSystemName("NES", new NoOpLogErrors(), new NoOpDebugLogger());
        Assert.Equal("nintendo entertainment system", result);
    }

    [Fact]
    public void GetBestMatchSystemNameWithAlias()
    {
        var result = RetroAchievementsSystemMatcher.GetBestMatchSystemName("Nintendo", new NoOpLogErrors(), new NoOpDebugLogger());
        Assert.NotNull(result);
    }

    [Fact]
    public void GetBestMatchSystemNameWithUnknownReturnsNormalizedInput()
    {
        var result = RetroAchievementsSystemMatcher.GetBestMatchSystemName("UnknownSystem12345", new NoOpLogErrors(), new NoOpDebugLogger());
        Assert.Equal("unknownsystem12345", result);
    }

    [Fact]
    public void IsOfficialSystemNameWithValidName()
    {
        var result = RetroAchievementsSystemMatcher.IsOfficialSystemName("nintendo entertainment system");
        Assert.True(result);
    }

    [Fact]
    public void IsOfficialSystemNameWithInvalidName()
    {
        var result = RetroAchievementsSystemMatcher.IsOfficialSystemName("UnknownSystem12345");
        Assert.False(result);
    }

    [Fact]
    public void GetSupportedSystemNamesReturnsNonEmpty()
    {
        var names = RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Assert.NotNull(names);
        Assert.NotEmpty(names);
    }

    [Fact]
    public void GetSupportedSystemNamesContainsKnownSystems()
    {
        var names = RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Assert.Contains("nintendo entertainment system", names);
    }

    [Fact]
    public void GetSystemIdWithValidSystem()
    {
        var id = RetroAchievementsSystemMatcher.GetSystemId("NES", new NoOpDebugLogger());
        Assert.True(id > 0);
    }

    [Fact]
    public void GetSystemIdWithInvalidSystemReturnsZero()
    {
        var id = RetroAchievementsSystemMatcher.GetSystemId("UnknownSystem12345", new NoOpDebugLogger());
        Assert.Equal(-1, id);
    }

    [Fact]
    public void IsSystemInMappingsWithValidSystem()
    {
        var result = RetroAchievementsSystemMatcher.IsSystemInMappings("NES");
        Assert.True(result);
    }

    [Fact]
    public void IsSystemInMappingsWithInvalidSystem()
    {
        var result = RetroAchievementsSystemMatcher.IsSystemInMappings("UnknownSystem12345");
        Assert.False(result);
    }

    [Fact]
    public void GetExactAliasMatchReturnsNullForUnknown()
    {
        var result = RetroAchievementsSystemMatcher.GetExactAliasMatch("UnknownSystem12345");
        Assert.Null(result);
    }

    [Fact]
    public void SupportedSystemsCountIsReasonable()
    {
        var names = RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Assert.True(names.Count >= 10, $"Expected at least 10 supported systems, got {names.Count}");
    }
}

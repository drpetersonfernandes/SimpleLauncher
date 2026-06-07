using SimpleLauncher.Services.DebugAndBugReport;
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

    [Fact]
    public void GetBestMatchSystemNameWithExactMatch()
    {
        var result = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetBestMatchSystemName("NES", new NoOpLogErrors());
        Assert.Equal("nintendo entertainment system", result);
    }

    [Fact]
    public void GetBestMatchSystemNameWithAlias()
    {
        var result = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetBestMatchSystemName("Nintendo", new NoOpLogErrors());
        Assert.NotNull(result);
    }

    [Fact]
    public void GetBestMatchSystemNameWithUnknownReturnsNormalizedInput()
    {
        var result = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetBestMatchSystemName("UnknownSystem12345", new NoOpLogErrors());
        Assert.Equal("unknownsystem12345", result);
    }

    [Fact]
    public void IsOfficialSystemNameWithValidName()
    {
        var result = Services.RetroAchievements.RetroAchievementsSystemMatcher.IsOfficialSystemName("nintendo entertainment system");
        Assert.True(result);
    }

    [Fact]
    public void IsOfficialSystemNameWithInvalidName()
    {
        var result = Services.RetroAchievements.RetroAchievementsSystemMatcher.IsOfficialSystemName("UnknownSystem12345");
        Assert.False(result);
    }

    [Fact]
    public void GetSupportedSystemNamesReturnsNonEmpty()
    {
        var names = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Assert.NotNull(names);
        Assert.NotEmpty(names);
    }

    [Fact]
    public void GetSupportedSystemNamesContainsKnownSystems()
    {
        var names = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Assert.Contains("nintendo entertainment system", names);
    }

    [Fact]
    public void GetSystemIdWithValidSystem()
    {
        var id = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetSystemId("NES");
        Assert.True(id > 0);
    }

    [Fact]
    public void GetSystemIdWithInvalidSystemReturnsZero()
    {
        var id = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetSystemId("UnknownSystem12345");
        Assert.Equal(-1, id);
    }

    [Fact]
    public void IsSystemInMappingsWithValidSystem()
    {
        var result = Services.RetroAchievements.RetroAchievementsSystemMatcher.IsSystemInMappings("NES");
        Assert.True(result);
    }

    [Fact]
    public void IsSystemInMappingsWithInvalidSystem()
    {
        var result = Services.RetroAchievements.RetroAchievementsSystemMatcher.IsSystemInMappings("UnknownSystem12345");
        Assert.False(result);
    }

    [Fact]
    public void GetExactAliasMatchReturnsNullForUnknown()
    {
        var result = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetExactAliasMatch("UnknownSystem12345");
        Assert.Null(result);
    }

    [Fact]
    public void SupportedSystemsCountIsReasonable()
    {
        var names = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Assert.True(names.Count >= 10, $"Expected at least 10 supported systems, got {names.Count}");
    }
}

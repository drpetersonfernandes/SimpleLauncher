using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.RetroAchievements;
using Xunit;

namespace SimpleLauncher.Tests;

public class RetroAchievementsSystemMatcherTests
{
    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    [Theory]
    [InlineData("snes", "super nintendo entertainment system")]
    [InlineData("SNES", "super nintendo entertainment system")]
    [InlineData("Super Nintendo", "super nintendo entertainment system")]
    [InlineData("n64", "nintendo 64")]
    [InlineData("Nintendo 64", "nintendo 64")]
    [InlineData("gba", "game boy advance")]
    [InlineData("genesis", "genesis/mega drive")]
    [InlineData("sega genesis", "genesis/mega drive")]
    [InlineData("ps1", "playstation")]
    [InlineData("playstation", "playstation")]
    [InlineData("arcade", "arcade")]
    [InlineData("mame", "arcade")]
    [InlineData("dreamcast", "dreamcast")]
    [InlineData("sega dreamcast", "dreamcast")]
    public void GetBestMatchSystemNameKnownAliasReturnsExpectedKey(string input, string expected)
    {
        var result = RetroAchievementsSystemMatcher.GetBestMatchSystemName(input, new NoOpLogErrors());
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetBestMatchSystemNameNullOrWhitespaceReturnsOriginal()
    {
        Assert.Null(RetroAchievementsSystemMatcher.GetBestMatchSystemName(null, new NoOpLogErrors()));
        Assert.Equal("", RetroAchievementsSystemMatcher.GetBestMatchSystemName("", new NoOpLogErrors()));
        Assert.Equal("   ", RetroAchievementsSystemMatcher.GetBestMatchSystemName("   ", new NoOpLogErrors()));
    }

    [Theory]
    [InlineData("super nintendo entertainment system", true)]
    [InlineData("playstation", true)]
    [InlineData("arcade", true)]
    [InlineData("unknown console xyz", false)]
    public void IsOfficialSystemNameReturnsExpected(string name, bool expected)
    {
        var result = RetroAchievementsSystemMatcher.IsOfficialSystemName(name);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetSupportedSystemNamesReturnsNonEmptyList()
    {
        var result = RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Assert.NotEmpty(result);
        Assert.Equal(result.Count, result.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Theory]
    [InlineData("snes", "super nintendo entertainment system")]
    [InlineData("gb", "game boy")]
    [InlineData("n64", "nintendo 64")]
    public void GetExactAliasMatchReturnsExpected(string input, string expected)
    {
        var result = RetroAchievementsSystemMatcher.GetExactAliasMatch(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetExactAliasMatchUnknownReturnsNull()
    {
        var result = RetroAchievementsSystemMatcher.GetExactAliasMatch("unknown");
        Assert.Null(result);
    }

    [Theory]
    [InlineData("snes", true)]
    [InlineData("super nintendo", true)]
    [InlineData("nintendo 64", true)]
    public void IsSystemInMappingsReturnsExpected(string input, bool expected)
    {
        var result = RetroAchievementsSystemMatcher.IsSystemInMappings(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("snes", 3)]
    [InlineData("n64", 2)]
    [InlineData("playstation", 12)]
    public void GetSystemIdKnownSystemReturnsExpectedId(string input, int expected)
    {
        var result = RetroAchievementsSystemMatcher.GetSystemId(input);
        Assert.Equal(expected, result);
    }
}

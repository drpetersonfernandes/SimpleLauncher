using SimpleLauncher.Services.RetroAchievements;
using Xunit;

namespace SimpleLauncher.Tests;

public class RetroAchievementsSystemMatcherTests
{
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
    public void GetBestMatchSystemName_KnownAlias_ReturnsExpectedKey(string input, string expected)
    {
        var result = RetroAchievementsSystemMatcher.GetBestMatchSystemName(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetBestMatchSystemName_NullOrWhitespace_ReturnsOriginal()
    {
        Assert.Null(RetroAchievementsSystemMatcher.GetBestMatchSystemName(null));
        Assert.Equal("", RetroAchievementsSystemMatcher.GetBestMatchSystemName(""));
        Assert.Equal("   ", RetroAchievementsSystemMatcher.GetBestMatchSystemName("   "));
    }

    [Theory]
    [InlineData("super nintendo entertainment system", true)]
    [InlineData("playstation", true)]
    [InlineData("arcade", true)]
    [InlineData("unknown console xyz", false)]
    public void IsOfficialSystemName_ReturnsExpected(string name, bool expected)
    {
        var result = RetroAchievementsSystemMatcher.IsOfficialSystemName(name);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetSupportedSystemNames_ReturnsNonEmptyList()
    {
        var result = RetroAchievementsSystemMatcher.GetSupportedSystemNames();
        Assert.NotEmpty(result);
        Assert.Equal(result.Count, result.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Theory]
    [InlineData("snes", "super nintendo entertainment system")]
    [InlineData("gb", "game boy")]
    [InlineData("n64", "nintendo 64")]
    public void GetExactAliasMatch_ReturnsExpected(string input, string expected)
    {
        var result = RetroAchievementsSystemMatcher.GetExactAliasMatch(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetExactAliasMatch_Unknown_ReturnsNull()
    {
        var result = RetroAchievementsSystemMatcher.GetExactAliasMatch("unknown");
        Assert.Null(result);
    }

    [Theory]
    [InlineData("snes", true)]
    [InlineData("super nintendo", true)]
    [InlineData("nintendo 64", true)]
    public void IsSystemInMappings_ReturnsExpected(string input, bool expected)
    {
        var result = RetroAchievementsSystemMatcher.IsSystemInMappings(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("snes", 3)]
    [InlineData("n64", 2)]
    [InlineData("playstation", 12)]
    public void GetSystemId_KnownSystem_ReturnsExpectedId(string input, int expected)
    {
        var result = RetroAchievementsSystemMatcher.GetSystemId(input);
        Assert.Equal(expected, result);
    }
}

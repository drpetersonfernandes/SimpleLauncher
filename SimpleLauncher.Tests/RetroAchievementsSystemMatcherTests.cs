using SimpleLauncher.Services.RetroAchievements;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Tests for <see cref="RetroAchievementsSystemMatcher"/> covering system name aliasing,
/// official name validation, supported system enumeration, and system ID lookups.
/// </summary>
public class RetroAchievementsSystemMatcherTests
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

    /// <summary>
    /// Verifies that GetBestMatchSystemName maps known system aliases to their canonical RetroAchievements names.
    /// </summary>
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
        var result = _matcher.GetBestMatchSystemName(input);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that GetBestMatchSystemName handles null, empty, and whitespace inputs gracefully.
    /// </summary>
    [Fact]
    public void GetBestMatchSystemNameNullOrWhitespaceReturnsOriginal()
    {
        Assert.Null(_matcher.GetBestMatchSystemName(null));
        Assert.Equal("", _matcher.GetBestMatchSystemName(""));
        Assert.Equal("   ", _matcher.GetBestMatchSystemName("   "));
    }

    /// <summary>
    /// Verifies that IsOfficialSystemName correctly identifies known official system names.
    /// </summary>
    [Theory]
    [InlineData("super nintendo entertainment system", true)]
    [InlineData("playstation", true)]
    [InlineData("arcade", true)]
    [InlineData("unknown console xyz", false)]
    public void IsOfficialSystemNameReturnsExpected(string name, bool expected)
    {
        var result = _matcher.IsOfficialSystemName(name);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that GetSupportedSystemNames returns a non-empty list with no duplicates.
    /// </summary>
    [Fact]
    public void GetSupportedSystemNamesReturnsNonEmptyList()
    {
        var result = _matcher.GetSupportedSystemNames();
        Assert.NotEmpty(result);
        Assert.Equal(result.Count, result.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    /// <summary>
    /// Verifies that GetExactAliasMatch returns the expected canonical name for known aliases.
    /// </summary>
    [Theory]
    [InlineData("snes", "super nintendo entertainment system")]
    [InlineData("gb", "game boy")]
    [InlineData("n64", "nintendo 64")]
    public void GetExactAliasMatchReturnsExpected(string input, string expected)
    {
        var result = _matcher.GetExactAliasMatch(input);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that GetExactAliasMatch returns null for an unknown alias.
    /// </summary>
    [Fact]
    public void GetExactAliasMatchUnknownReturnsNull()
    {
        var result = _matcher.GetExactAliasMatch("unknown");
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that IsSystemInMappings correctly identifies system names present in the alias mappings.
    /// </summary>
    [Theory]
    [InlineData("snes", true)]
    [InlineData("super nintendo", true)]
    [InlineData("nintendo 64", true)]
    public void IsSystemInMappingsReturnsExpected(string input, bool expected)
    {
        var result = _matcher.IsSystemInMappings(input);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verifies that GetSystemId returns the correct RetroAchievements system ID for known systems.
    /// </summary>
    [Theory]
    [InlineData("snes", 3)]
    [InlineData("n64", 2)]
    [InlineData("playstation", 12)]
    public void GetSystemIdKnownSystemReturnsExpectedId(string input, int expected)
    {
        var result = _matcher.GetSystemId(input);
        Assert.Equal(expected, result);
    }
}

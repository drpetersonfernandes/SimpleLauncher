using SimpleLauncher.Services.RetroAchievements;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Extended tests for <see cref="RetroAchievementsSystemMatcher"/> covering exact matches,
/// unknown system fallback, supported system count validation, and mapping checks.
/// </summary>
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

    /// <summary>
    /// Verifies that GetBestMatchSystemName returns the correct canonical name for an exact alias match.
    /// </summary>
    [Fact]
    public void GetBestMatchSystemNameWithExactMatch()
    {
        var result = _matcher.GetBestMatchSystemName("NES");
        Assert.Equal("nintendo entertainment system", result);
    }

    /// <summary>
    /// Verifies that GetBestMatchSystemName returns a non-null result for a known alias.
    /// </summary>
    [Fact]
    public void GetBestMatchSystemNameWithAlias()
    {
        var result = _matcher.GetBestMatchSystemName("Nintendo");
        Assert.NotNull(result);
    }

    /// <summary>
    /// Verifies that GetBestMatchSystemName returns a normalized lowercase version of an unknown system name.
    /// </summary>
    [Fact]
    public void GetBestMatchSystemNameWithUnknownReturnsNormalizedInput()
    {
        var result = _matcher.GetBestMatchSystemName("UnknownSystem12345");
        Assert.Equal("unknownsystem12345", result);
    }

    /// <summary>
    /// Verifies that IsOfficialSystemName returns true for a valid official system name.
    /// </summary>
    [Fact]
    public void IsOfficialSystemNameWithValidName()
    {
        var result = _matcher.IsOfficialSystemName("nintendo entertainment system");
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that IsOfficialSystemName returns false for an unknown system name.
    /// </summary>
    [Fact]
    public void IsOfficialSystemNameWithInvalidName()
    {
        var result = _matcher.IsOfficialSystemName("UnknownSystem12345");
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that GetSupportedSystemNames returns a non-null, non-empty list.
    /// </summary>
    [Fact]
    public void GetSupportedSystemNamesReturnsNonEmpty()
    {
        var names = _matcher.GetSupportedSystemNames();
        Assert.NotNull(names);
        Assert.NotEmpty(names);
    }

    /// <summary>
    /// Verifies that GetSupportedSystemNames includes well-known systems like NES.
    /// </summary>
    [Fact]
    public void GetSupportedSystemNamesContainsKnownSystems()
    {
        var names = _matcher.GetSupportedSystemNames();
        Assert.Contains("nintendo entertainment system", names);
    }

    /// <summary>
    /// Verifies that GetSystemId returns a positive ID for a valid system alias.
    /// </summary>
    [Fact]
    public void GetSystemIdWithValidSystem()
    {
        var id = _matcher.GetSystemId("NES");
        Assert.True(id > 0);
    }

    /// <summary>
    /// Verifies that GetSystemId returns -1 for an unknown system name.
    /// </summary>
    [Fact]
    public void GetSystemIdWithInvalidSystemReturnsZero()
    {
        var id = _matcher.GetSystemId("UnknownSystem12345");
        Assert.Equal(-1, id);
    }

    /// <summary>
    /// Verifies that IsSystemInMappings returns true for a valid system alias.
    /// </summary>
    [Fact]
    public void IsSystemInMappingsWithValidSystem()
    {
        var result = _matcher.IsSystemInMappings("NES");
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that IsSystemInMappings returns false for an unknown system name.
    /// </summary>
    [Fact]
    public void IsSystemInMappingsWithInvalidSystem()
    {
        var result = _matcher.IsSystemInMappings("UnknownSystem12345");
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that GetExactAliasMatch returns null for an unknown system name.
    /// </summary>
    [Fact]
    public void GetExactAliasMatchReturnsNullForUnknown()
    {
        var result = _matcher.GetExactAliasMatch("UnknownSystem12345");
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that the supported systems list contains at least 10 entries.
    /// </summary>
    [Fact]
    public void SupportedSystemsCountIsReasonable()
    {
        var names = _matcher.GetSupportedSystemNames();
        Assert.True(names.Count >= 10, $"Expected at least 10 supported systems, got {names.Count}");
    }
}

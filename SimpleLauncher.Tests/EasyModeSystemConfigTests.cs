using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="EasyModeSystemConfig"/> validation logic and property behavior.
/// </summary>
public class EasyModeSystemConfigTests
{
    /// <summary>
    /// Verifies that IsValid returns true when SystemName is set to a valid non-empty string.
    /// </summary>
    [Fact]
    public void IsValidWithSystemNameReturnsTrue()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "Arcade"
        };

        Assert.True(config.IsValid());
    }

    /// <summary>
    /// Verifies that IsValid returns false when SystemName is null.
    /// </summary>
    [Fact]
    public void IsValidWithNullSystemNameReturnsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = null
        };

        Assert.False(config.IsValid());
    }

    /// <summary>
    /// Verifies that IsValid returns false when SystemName is an empty string.
    /// </summary>
    [Fact]
    public void IsValidWithEmptySystemNameReturnsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = ""
        };

        Assert.False(config.IsValid());
    }

    /// <summary>
    /// Verifies that IsValid returns false when SystemName consists only of whitespace.
    /// </summary>
    [Fact]
    public void IsValidWithWhitespaceSystemNameReturnsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "   "
        };

        Assert.False(config.IsValid());
    }

    /// <summary>
    /// Verifies that ShouldSerializeExtractFileBeforeLaunch returns false when the property is set to false.
    /// </summary>
    [Fact]
    public void ShouldSerializeExtractFileBeforeLaunchFalseReturnsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "Test",
            ExtractFileBeforeLaunch = false
        };

        Assert.False(config.ShouldSerializeExtractFileBeforeLaunch());
    }

    /// <summary>
    /// Verifies that ShouldSerializeExtractFileBeforeLaunch returns true when the property is set to true.
    /// </summary>
    [Fact]
    public void ShouldSerializeExtractFileBeforeLaunchTrueReturnsTrue()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "Test",
            ExtractFileBeforeLaunch = true
        };

        Assert.True(config.ShouldSerializeExtractFileBeforeLaunch());
    }

    /// <summary>
    /// Verifies that the default value of ExtractFileBeforeLaunch is false.
    /// </summary>
    [Fact]
    public void DefaultExtractFileBeforeLaunchIsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "Test"
        };

        Assert.False(config.ExtractFileBeforeLaunch);
    }

    /// <summary>
    /// Verifies that all properties on EasyModeSystemConfig can be set via object initializer and retrieved correctly.
    /// </summary>
    [Fact]
    public void PropertiesCanBeSetAndRetrieved()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "NES",
            SystemFolder = "C:\\roms\\nes",
            SystemImageFolder = "C:\\images\\nes",
            FileFormatsToSearch = [".nes", ".zip"],
            FileFormatsToLaunch = [".nes"],
            ExtractFileBeforeLaunch = false
        };

        Assert.Equal("NES", config.SystemName);
        Assert.Equal("C:\\roms\\nes", config.SystemFolder);
        Assert.Equal("C:\\images\\nes", config.SystemImageFolder);
        Assert.Equal(2, config.FileFormatsToSearch.Count);
        Assert.Single(config.FileFormatsToLaunch);
    }
}

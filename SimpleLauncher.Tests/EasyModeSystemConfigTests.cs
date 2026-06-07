using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class EasyModeSystemConfigTests
{
    [Fact]
    public void IsValidWithSystemNameReturnsTrue()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "Arcade"
        };

        Assert.True(config.IsValid());
    }

    [Fact]
    public void IsValidWithNullSystemNameReturnsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = null
        };

        Assert.False(config.IsValid());
    }

    [Fact]
    public void IsValidWithEmptySystemNameReturnsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = ""
        };

        Assert.False(config.IsValid());
    }

    [Fact]
    public void IsValidWithWhitespaceSystemNameReturnsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "   "
        };

        Assert.False(config.IsValid());
    }

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

    [Fact]
    public void DefaultExtractFileBeforeLaunchIsFalse()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "Test"
        };

        Assert.False(config.ExtractFileBeforeLaunch);
    }

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

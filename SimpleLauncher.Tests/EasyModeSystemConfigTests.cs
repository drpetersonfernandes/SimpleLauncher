using SimpleLauncher.Core.Models;
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
            SystemName = null!
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

    /// <summary>
    /// Verifies that the default value of SystemName is null.
    /// </summary>
    [Fact]
    public void DefaultSystemNameIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemName);
    }

    /// <summary>
    /// Verifies that the default value of SystemFolder is null.
    /// </summary>
    [Fact]
    public void DefaultSystemFolderIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemFolder);
    }

    /// <summary>
    /// Verifies that the default value of SystemImageFolder is null.
    /// </summary>
    [Fact]
    public void DefaultSystemImageFolderIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemImageFolder);
    }

    /// <summary>
    /// Verifies that the default value of FileFormatsToSearch is null.
    /// </summary>
    [Fact]
    public void DefaultFileFormatsToSearchIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.FileFormatsToSearch);
    }

    /// <summary>
    /// Verifies that the default value of FileFormatsToLaunch is null.
    /// </summary>
    [Fact]
    public void DefaultFileFormatsToLaunchIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.FileFormatsToLaunch);
    }

    /// <summary>
    /// Verifies that the default value of Emulators is null.
    /// </summary>
    [Fact]
    public void DefaultEmulatorsIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.Emulators);
    }

    /// <summary>
    /// Verifies that FileFormatsToSearch and FileFormatsToLaunch can be set with multiple values.
    /// </summary>
    [Fact]
    public void FileFormatsCanBeSet()
    {
        var config = new EasyModeSystemConfig
        {
            FileFormatsToSearch = ["nes", "fds", "unf"],
            FileFormatsToLaunch = ["nes", "fds"]
        };

        Assert.Equal(3, config.FileFormatsToSearch.Count);
        Assert.Equal(2, config.FileFormatsToLaunch.Count);
    }

    /// <summary>
    /// Verifies that the Emulators property can be set with an EmulatorsConfig containing an EmulatorConfig.
    /// </summary>
    [Fact]
    public void EmulatorsCanBeSet()
    {
        var config = new EasyModeSystemConfig
        {
            Emulators = new EmulatorsConfig
            {
                Emulator = new EmulatorConfig { EmulatorName = "RetroArch" }
            }
        };

        Assert.NotNull(config.Emulators);
        Assert.NotNull(config.Emulators.Emulator);
        Assert.Equal("RetroArch", config.Emulators.Emulator.EmulatorName);
    }

    /// <summary>
    /// Verifies that IsValid returns true when all required properties are set correctly.
    /// </summary>
    [Fact]
    public void IsValidReturnsTrueWithValidConfig()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "NES",
            SystemFolder = @"C:\roms\NES",
            FileFormatsToSearch = ["nes"],
            FileFormatsToLaunch = ["nes"],
            Emulators = new EmulatorsConfig
            {
                Emulator = new EmulatorConfig { EmulatorName = "RetroArch" }
            }
        };

        Assert.True(config.IsValid());
    }

    /// <summary>
    /// Verifies that IsValid returns true even when SystemFolder is null.
    /// </summary>
    [Fact]
    public void IsValidReturnsTrueWithNullSystemFolder()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "NES",
            FileFormatsToSearch = ["nes"],
            FileFormatsToLaunch = ["nes"],
            Emulators = new EmulatorsConfig
            {
                Emulator = new EmulatorConfig { EmulatorName = "RetroArch" }
            }
        };

        Assert.True(config.IsValid());
    }

    /// <summary>
    /// Verifies that Unicode characters in SystemName are preserved.
    /// </summary>
    [Fact]
    public void UnicodeSystemNameIsPreserved()
    {
        var config = new EasyModeSystemConfig { SystemName = "ゲームボーイ" };
        Assert.Equal("ゲームボーイ", config.SystemName);
    }

    /// <summary>
    /// Verifies that spaces in SystemName and SystemFolder are preserved.
    /// </summary>
    [Fact]
    public void SpacesInPathsArePreserved()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "Nintendo Entertainment System",
            SystemFolder = @"C:\My ROMs\NES Games"
        };

        Assert.Contains(" ", config.SystemName, StringComparison.Ordinal);
        Assert.Contains(" ", config.SystemFolder, StringComparison.Ordinal);
    }
}
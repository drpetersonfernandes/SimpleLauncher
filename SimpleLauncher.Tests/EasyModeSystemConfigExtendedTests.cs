using SimpleLauncher.Models;
using SimpleLauncher.Services.EasyMode.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="EasyModeSystemConfig"/> covering default values, edge cases, and Unicode support.
/// </summary>
public class EasyModeSystemConfigExtendedTests
{
    /// <summary>
    /// Verifies that the default value of SystemName is null.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigDefaultSystemNameIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemName);
    }

    /// <summary>
    /// Verifies that the default value of SystemFolder is null.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigDefaultSystemFolderIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemFolder);
    }

    /// <summary>
    /// Verifies that the default value of SystemImageFolder is null.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigDefaultSystemImageFolderIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemImageFolder);
    }

    /// <summary>
    /// Verifies that the default value of FileFormatsToSearch is null.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigDefaultFileFormatsToSearchIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.FileFormatsToSearch);
    }

    /// <summary>
    /// Verifies that the default value of FileFormatsToLaunch is null.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigDefaultFileFormatsToLaunchIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.FileFormatsToLaunch);
    }

    /// <summary>
    /// Verifies that the default value of ExtractFileBeforeLaunch is false.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigDefaultExtractFileBeforeLaunchIsFalse()
    {
        var config = new EasyModeSystemConfig();
        Assert.False(config.ExtractFileBeforeLaunch);
    }

    /// <summary>
    /// Verifies that the default value of Emulators is null.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigDefaultEmulatorsIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.Emulators);
    }

    /// <summary>
    /// Verifies that basic string and boolean properties can be set and retrieved.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigPropertiesCanBeSet()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "NES",
            SystemFolder = @"C:\roms\NES",
            SystemImageFolder = @"C:\images\NES",
            ExtractFileBeforeLaunch = true
        };

        Assert.Equal("NES", config.SystemName);
        Assert.Equal(@"C:\roms\NES", config.SystemFolder);
        Assert.Equal(@"C:\images\NES", config.SystemImageFolder);
        Assert.True(config.ExtractFileBeforeLaunch);
    }

    /// <summary>
    /// Verifies that file format lists can be set and their counts are correct.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigFileFormatsCanBeSet()
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
    /// Verifies that the Emulators nested configuration object can be set and retrieved.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigEmulatorsCanBeSet()
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
    /// Verifies that IsValid returns true when all required properties are populated.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigIsValidReturnsTrueWithValidConfig()
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
    /// Verifies that IsValid returns false when SystemName is null even if other properties are set.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigIsValidReturnsFalseWithNullSystemName()
    {
        var config = new EasyModeSystemConfig
        {
            SystemFolder = @"C:\roms\NES",
            FileFormatsToSearch = ["nes"],
            FileFormatsToLaunch = ["nes"],
            Emulators = new EmulatorsConfig
            {
                Emulator = new EmulatorConfig { EmulatorName = "RetroArch" }
            }
        };

        Assert.False(config.IsValid());
    }

    /// <summary>
    /// Verifies that IsValid returns false when SystemName is empty even if other properties are set.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigIsValidReturnsFalseWithEmptySystemName()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "",
            SystemFolder = @"C:\roms\NES",
            FileFormatsToSearch = ["nes"],
            FileFormatsToLaunch = ["nes"],
            Emulators = new EmulatorsConfig
            {
                Emulator = new EmulatorConfig { EmulatorName = "RetroArch" }
            }
        };

        Assert.False(config.IsValid());
    }

    /// <summary>
    /// Verifies that IsValid still returns true when SystemFolder is null (only SystemName is required).
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigIsValidReturnsFalseWithNullSystemFolder()
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
    /// Verifies that ShouldSerializeExtractFileBeforeLaunch returns false by default.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigShouldSerializeExtractFileBeforeLaunchFalseByDefault()
    {
        var config = new EasyModeSystemConfig();
        Assert.False(config.ShouldSerializeExtractFileBeforeLaunch());
    }

    /// <summary>
    /// Verifies that ShouldSerializeExtractFileBeforeLaunch returns true when set to true.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigShouldSerializeExtractFileBeforeLaunchTrueWhenSet()
    {
        var config = new EasyModeSystemConfig { ExtractFileBeforeLaunch = true };
        Assert.True(config.ShouldSerializeExtractFileBeforeLaunch());
    }

    /// <summary>
    /// Verifies that Unicode characters in SystemName are preserved correctly.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigWithUnicodeSystemName()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "ゲームボーイ"
        };

        Assert.Equal("ゲームボーイ", config.SystemName);
    }

    /// <summary>
    /// Verifies that paths and names containing spaces are handled correctly.
    /// </summary>
    [Fact]
    public void EasyModeSystemConfigWithSpacesInPaths()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "Nintendo Entertainment System",
            SystemFolder = @"C:\My ROMs\NES Games"
        };

        Assert.Contains(" ", config.SystemName);
        Assert.Contains(" ", config.SystemFolder);
    }
}

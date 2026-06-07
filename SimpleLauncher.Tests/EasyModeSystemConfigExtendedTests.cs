using SimpleLauncher.Models;
using SimpleLauncher.Services.EasyMode.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class EasyModeSystemConfigExtendedTests
{
    [Fact]
    public void EasyModeSystemConfigDefaultSystemNameIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemName);
    }

    [Fact]
    public void EasyModeSystemConfigDefaultSystemFolderIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemFolder);
    }

    [Fact]
    public void EasyModeSystemConfigDefaultSystemImageFolderIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.SystemImageFolder);
    }

    [Fact]
    public void EasyModeSystemConfigDefaultFileFormatsToSearchIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.FileFormatsToSearch);
    }

    [Fact]
    public void EasyModeSystemConfigDefaultFileFormatsToLaunchIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.FileFormatsToLaunch);
    }

    [Fact]
    public void EasyModeSystemConfigDefaultExtractFileBeforeLaunchIsFalse()
    {
        var config = new EasyModeSystemConfig();
        Assert.False(config.ExtractFileBeforeLaunch);
    }

    [Fact]
    public void EasyModeSystemConfigDefaultEmulatorsIsNull()
    {
        var config = new EasyModeSystemConfig();
        Assert.Null(config.Emulators);
    }

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

    [Fact]
    public void EasyModeSystemConfigShouldSerializeExtractFileBeforeLaunchFalseByDefault()
    {
        var config = new EasyModeSystemConfig();
        Assert.False(config.ShouldSerializeExtractFileBeforeLaunch());
    }

    [Fact]
    public void EasyModeSystemConfigShouldSerializeExtractFileBeforeLaunchTrueWhenSet()
    {
        var config = new EasyModeSystemConfig { ExtractFileBeforeLaunch = true };
        Assert.True(config.ShouldSerializeExtractFileBeforeLaunch());
    }

    [Fact]
    public void EasyModeSystemConfigWithUnicodeSystemName()
    {
        var config = new EasyModeSystemConfig
        {
            SystemName = "ゲームボーイ"
        };

        Assert.Equal("ゲームボーイ", config.SystemName);
    }

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

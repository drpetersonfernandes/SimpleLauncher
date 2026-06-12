using SimpleLauncher.Services.SystemManager;
using Xunit;

namespace SimpleLauncher.Tests;

public class SystemManagerConfigTests
{
    [Fact]
    public void DefaultPropertiesAreNull()
    {
        var config = new SystemManagerConfig();

        Assert.Null(config.SystemName);
        Assert.Null(config.SystemFolders);
        Assert.Null(config.SystemImageFolder);
        Assert.Null(config.FileFormatsToSearch);
        Assert.Null(config.FileFormatsToLaunch);
        Assert.Null(config.Emulators);
        Assert.Null(config.PrimarySystemFolder);
        Assert.False(config.ExtractFileBeforeLaunch);
        Assert.False(config.GroupByFolder);
        Assert.False(config.DisableRecursiveSearch);
    }

    [Fact]
    public void InitPropertiesCanBeSet()
    {
        var config = new SystemManagerConfig
        {
            SystemName = "NES",
            SystemFolders = [@"C:\roms\nes"],
            SystemImageFolder = @"C:\images\nes",
            FileFormatsToSearch = [".nes", ".zip"],
            FileFormatsToLaunch = [".nes"],
            ExtractFileBeforeLaunch = true,
            GroupByFolder = true,
            DisableRecursiveSearch = true,
            Emulators = []
        };

        Assert.Equal("NES", config.SystemName);
        Assert.Single(config.SystemFolders);
        Assert.Equal(@"C:\roms\nes", config.SystemFolders[0]);
        Assert.Equal(@"C:\images\nes", config.SystemImageFolder);
        Assert.Equal(2, config.FileFormatsToSearch.Count);
        Assert.Single(config.FileFormatsToLaunch);
        Assert.True(config.ExtractFileBeforeLaunch);
        Assert.True(config.GroupByFolder);
        Assert.True(config.DisableRecursiveSearch);
    }

    [Fact]
    public void PrimarySystemFolderReturnsFirstFolder()
    {
        var config = new SystemManagerConfig
        {
            SystemFolders = ["first", "second", "third"]
        };

        Assert.Equal("first", config.PrimarySystemFolder);
    }

    [Fact]
    public void PrimarySystemFolderReturnsNullWhenFoldersAreNull()
    {
        var config = new SystemManagerConfig();
        Assert.Null(config.PrimarySystemFolder);
    }

    [Fact]
    public void PrimarySystemFolderReturnsNullWhenFoldersAreEmpty()
    {
        var config = new SystemManagerConfig
        {
            SystemFolders = []
        };

        Assert.Null(config.PrimarySystemFolder);
    }

    [Fact]
    public void EmulatorsCanBeSet()
    {
        var emulators = new List<Emulator>
        {
            new() { EmulatorName = "Mesen", EmulatorLocation = @"C:\emu\mesen.exe" },
            new() { EmulatorName = "RetroArch", EmulatorLocation = @"C:\emu\retroarch.exe" }
        };

        var config = new SystemManagerConfig { Emulators = emulators };

        Assert.Equal(2, config.Emulators.Count);
        Assert.Equal("Mesen", config.Emulators[0].EmulatorName);
        Assert.Equal("RetroArch", config.Emulators[1].EmulatorName);
    }
}

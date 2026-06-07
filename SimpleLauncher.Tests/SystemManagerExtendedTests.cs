using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class SystemManagerExtendedTests : IDisposable
{
    private readonly string _testDirectory;

    public SystemManagerExtendedTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_SysMgrExt_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // ignored
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void SystemManagerDefaultProperties()
    {
        var sm = new Services.SystemManager.SystemManager();
        Assert.Null(sm.SystemName);
        Assert.Null(sm.SystemImageFolder);
        Assert.Null(sm.SystemFolders);
        Assert.Null(sm.FileFormatsToSearch);
        Assert.Null(sm.FileFormatsToLaunch);
    }

    [Fact]
    public void SystemManagerPropertiesCanBeSet()
    {
        var sm = new Services.SystemManager.SystemManager
        {
            SystemName = "NES",
            SystemFolders = [@"C:\roms\NES"],
            SystemImageFolder = @"C:\images\NES",
            ExtractFileBeforeLaunch = true
        };

        Assert.Equal("NES", sm.SystemName);
        Assert.Single(sm.SystemFolders);
        Assert.Equal(@"C:\roms\NES", sm.SystemFolders[0]);
        Assert.Equal(@"C:\images\NES", sm.SystemImageFolder);
        Assert.True(sm.ExtractFileBeforeLaunch);
    }

    [Fact]
    public void SystemManagerEmulatorsListDefaultIsNull()
    {
        var sm = new Services.SystemManager.SystemManager();
        Assert.Null(sm.Emulators);
    }

    [Fact]
    public void SystemManagerEmulatorsCanBeSet()
    {
        var sm = new Services.SystemManager.SystemManager
        {
            Emulators =
            [
                new Services.SystemManager.Emulator { EmulatorName = "RetroArch" },
                new Services.SystemManager.Emulator { EmulatorName = "Mesen" }
            ]
        };

        Assert.Equal(2, sm.Emulators.Count);
        Assert.Contains(sm.Emulators, static e => e.EmulatorName == "RetroArch");
        Assert.Contains(sm.Emulators, static e => e.EmulatorName == "Mesen");
    }

    [Fact]
    public void SystemManagerFileFormatsCanBeSet()
    {
        var sm = new Services.SystemManager.SystemManager
        {
            FileFormatsToSearch = ["nes", "fds", "unf"],
            FileFormatsToLaunch = ["nes", "fds"]
        };

        Assert.Equal(3, sm.FileFormatsToSearch.Count);
        Assert.Equal(2, sm.FileFormatsToLaunch.Count);
    }

    [Fact]
    public void SystemManagerDisableRecursiveSearch()
    {
        var sm = new Services.SystemManager.SystemManager
        {
            DisableRecursiveSearch = true
        };

        Assert.True(sm.DisableRecursiveSearch);
    }

    [Fact]
    public void SystemManagerGroupByFolder()
    {
        var sm = new Services.SystemManager.SystemManager
        {
            GroupByFolder = true
        };

        Assert.True(sm.GroupByFolder);
    }

    [Fact]
    public void SystemManagerSystemFoldersCanBeSet()
    {
        var sm = new Services.SystemManager.SystemManager
        {
            SystemFolders = ["C:\\roms\\NES", "D:\\roms\\NES"]
        };

        Assert.Equal(2, sm.SystemFolders.Count);
    }

    [Fact]
    public void EmulatorDefaultProperties()
    {
        var emu = new Services.SystemManager.Emulator();
        Assert.Null(emu.EmulatorName);
        Assert.Null(emu.EmulatorLocation);
        Assert.Null(emu.EmulatorParameters);
    }

    [Fact]
    public void EmulatorPropertiesCanBeSet()
    {
        var emu = new Services.SystemManager.Emulator
        {
            EmulatorName = "RetroArch",
            EmulatorLocation = @"C:\emulators\retroarch.exe",
            EmulatorParameters = "-L core.dll"
        };

        Assert.Equal("RetroArch", emu.EmulatorName);
        Assert.Equal(@"C:\emulators\retroarch.exe", emu.EmulatorLocation);
        Assert.Equal("-L core.dll", emu.EmulatorParameters);
    }

    [Fact]
    public void EmulatorWithSpecialCharactersInPath()
    {
        var emu = new Services.SystemManager.Emulator
        {
            EmulatorLocation = @"C:\Program Files (x86)\RetroArch\retroarch.exe"
        };

        Assert.Contains("(", emu.EmulatorLocation);
        Assert.Contains(")", emu.EmulatorLocation);
    }

    [Fact]
    public void EmulatorWithSpacesInPath()
    {
        var emu = new Services.SystemManager.Emulator
        {
            EmulatorLocation = @"C:\My Emulators\RetroArch\retroarch.exe"
        };

        Assert.Contains(" ", emu.EmulatorLocation);
    }
}

using SimpleLauncher.Services.SystemManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Provides extended test coverage for SystemManager and Emulator models, covering default states, property assignment, and edge cases.
/// </summary>
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

    /// <summary>
    /// Verifies that a new SystemManager has null defaults for all reference-type properties.
    /// </summary>
    [Fact]
    public void SystemManagerDefaultProperties()
    {
        var sm = new SystemManager();
        Assert.Null(sm.SystemName);
        Assert.Null(sm.SystemImageFolder);
        Assert.Null(sm.SystemFolders);
        Assert.Null(sm.FileFormatsToSearch);
        Assert.Null(sm.FileFormatsToLaunch);
    }

    /// <summary>
    /// Verifies that SystemManager properties can be assigned and retrieved.
    /// </summary>
    [Fact]
    public void SystemManagerPropertiesCanBeSet()
    {
        var sm = new SystemManager
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

    /// <summary>
    /// Verifies that the Emulators list defaults to null.
    /// </summary>
    [Fact]
    public void SystemManagerEmulatorsListDefaultIsNull()
    {
        var sm = new SystemManager();
        Assert.Null(sm.Emulators);
    }

    /// <summary>
    /// Verifies that emulators can be added to the Emulators list.
    /// </summary>
    [Fact]
    public void SystemManagerEmulatorsCanBeSet()
    {
        var sm = new SystemManager
        {
            Emulators =
            [
                new Emulator { EmulatorName = "RetroArch" },
                new Emulator { EmulatorName = "Mesen" }
            ]
        };

        Assert.Equal(2, sm.Emulators.Count);
        Assert.Contains(sm.Emulators, static e => e.EmulatorName == "RetroArch");
        Assert.Contains(sm.Emulators, static e => e.EmulatorName == "Mesen");
    }

    /// <summary>
    /// Verifies that file format lists can be populated correctly.
    /// </summary>
    [Fact]
    public void SystemManagerFileFormatsCanBeSet()
    {
        var sm = new SystemManager
        {
            FileFormatsToSearch = ["nes", "fds", "unf"],
            FileFormatsToLaunch = ["nes", "fds"]
        };

        Assert.Equal(3, sm.FileFormatsToSearch.Count);
        Assert.Equal(2, sm.FileFormatsToLaunch.Count);
    }

    /// <summary>
    /// Verifies that the DisableRecursiveSearch flag can be set.
    /// </summary>
    [Fact]
    public void SystemManagerDisableRecursiveSearch()
    {
        var sm = new SystemManager
        {
            DisableRecursiveSearch = true
        };

        Assert.True(sm.DisableRecursiveSearch);
    }

    /// <summary>
    /// Verifies that the GroupByFolder flag can be set.
    /// </summary>
    [Fact]
    public void SystemManagerGroupByFolder()
    {
        var sm = new SystemManager
        {
            GroupByFolder = true
        };

        Assert.True(sm.GroupByFolder);
    }

    /// <summary>
    /// Verifies that system folders can be assigned.
    /// </summary>
    [Fact]
    public void SystemManagerSystemFoldersCanBeSet()
    {
        var sm = new SystemManager
        {
            SystemFolders = ["C:\\roms\\NES", "D:\\roms\\NES"]
        };

        Assert.Equal(2, sm.SystemFolders.Count);
    }

    /// <summary>
    /// Verifies that a new Emulator has null defaults for string properties.
    /// </summary>
    [Fact]
    public void EmulatorDefaultProperties()
    {
        var emu = new Emulator();
        Assert.Null(emu.EmulatorName);
        Assert.Null(emu.EmulatorLocation);
        Assert.Null(emu.EmulatorParameters);
    }

    /// <summary>
    /// Verifies that Emulator properties can be assigned and retrieved.
    /// </summary>
    [Fact]
    public void EmulatorPropertiesCanBeSet()
    {
        var emu = new Emulator
        {
            EmulatorName = "RetroArch",
            EmulatorLocation = @"C:\emulators\retroarch.exe",
            EmulatorParameters = "-L core.dll"
        };

        Assert.Equal("RetroArch", emu.EmulatorName);
        Assert.Equal(@"C:\emulators\retroarch.exe", emu.EmulatorLocation);
        Assert.Equal("-L core.dll", emu.EmulatorParameters);
    }

    /// <summary>
    /// Verifies that emulator paths with parentheses are handled correctly.
    /// </summary>
    [Fact]
    public void EmulatorWithSpecialCharactersInPath()
    {
        var emu = new Emulator
        {
            EmulatorLocation = @"C:\Program Files (x86)\RetroArch\retroarch.exe"
        };

        Assert.Contains("(", emu.EmulatorLocation);
        Assert.Contains(")", emu.EmulatorLocation);
    }

    /// <summary>
    /// Verifies that emulator paths with spaces are handled correctly.
    /// </summary>
    [Fact]
    public void EmulatorWithSpacesInPath()
    {
        var emu = new Emulator
        {
            EmulatorLocation = @"C:\My Emulators\RetroArch\retroarch.exe"
        };

        Assert.Contains(" ", emu.EmulatorLocation);
    }
}

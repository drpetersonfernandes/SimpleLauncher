using SimpleLauncher.Services.SystemManager;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the SystemManager and Emulator model classes for property assignment, defaults, and collection behavior.
/// </summary>
public class SystemManagerTests
{
    /// <summary>
    /// Verifies that all SystemManager properties can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void SystemManagerPropertiesCanBeSetAndRetrieved()
    {
        var system = new SystemManager
        {
            SystemName = "Arcade",
            SystemFolders = ["C:\\roms\\Arcade", "D:\\roms\\Arcade"],
            SystemImageFolder = "C:\\images\\Arcade",
            FileFormatsToSearch = [".zip", ".7z"],
            FileFormatsToLaunch = [".zip"],
            ExtractFileBeforeLaunch = false,
            GroupByFolder = false,
            DisableRecursiveSearch = false,
            Emulators =
            [
                new Emulator
                {
                    EmulatorName = "MAME",
                    EmulatorLocation = "C:\\emulators\\mame\\mame.exe",
                    EmulatorParameters = "-rompath %SYSTEMFOLDER%"
                }
            ]
        };

        Assert.Equal("Arcade", system.SystemName);
        Assert.Equal(2, system.SystemFolders.Count);
        Assert.Equal("C:\\images\\Arcade", system.SystemImageFolder);
        Assert.Equal(2, system.FileFormatsToSearch.Count);
        Assert.Single(system.FileFormatsToLaunch);
        Assert.False(system.ExtractFileBeforeLaunch);
        Assert.Single(system.Emulators);
        Assert.Equal("MAME", system.Emulators[0].EmulatorName);
    }

    /// <summary>
    /// Verifies that PrimarySystemFolder returns the first folder in the list.
    /// </summary>
    [Fact]
    public void PrimarySystemFolderReturnsFirstFolder()
    {
        var system = new SystemManager
        {
            SystemName = "NES",
            SystemFolders = ["C:\\roms\\NES", "D:\\backup\\NES"]
        };

        Assert.Equal("C:\\roms\\NES", system.PrimarySystemFolder);
    }

    /// <summary>
    /// Verifies that PrimarySystemFolder returns null when SystemFolders is null.
    /// </summary>
    [Fact]
    public void PrimarySystemFolderWithNullFoldersReturnsNull()
    {
        var system = new SystemManager
        {
            SystemName = "NES",
            SystemFolders = null
        };

        Assert.Null(system.PrimarySystemFolder);
    }

    /// <summary>
    /// Verifies that PrimarySystemFolder returns null when SystemFolders is empty.
    /// </summary>
    [Fact]
    public void PrimarySystemFolderWithEmptyFoldersReturnsNull()
    {
        var system = new SystemManager
        {
            SystemName = "NES",
            SystemFolders = []
        };

        Assert.Null(system.PrimarySystemFolder);
    }

    /// <summary>
    /// Verifies that all Emulator properties can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void EmulatorPropertiesCanBeSetAndRetrieved()
    {
        var emulator = new Emulator
        {
            EmulatorName = "RetroArch",
            EmulatorLocation = "C:\\emulators\\retroarch\\retroarch.exe",
            EmulatorParameters = "-L core.dll %ROM%",
            ReceiveANotificationOnEmulatorError = true,
            ImagePackDownloadLink = "https://example.com/pack.zip",
            ImagePackDownloadExtractPath = "C:\\images"
        };

        Assert.Equal("RetroArch", emulator.EmulatorName);
        Assert.Equal("C:\\emulators\\retroarch\\retroarch.exe", emulator.EmulatorLocation);
        Assert.Equal("-L core.dll %ROM%", emulator.EmulatorParameters);
        Assert.True(emulator.ReceiveANotificationOnEmulatorError);
        Assert.Equal("https://example.com/pack.zip", emulator.ImagePackDownloadLink);
        Assert.Equal("C:\\images", emulator.ImagePackDownloadExtractPath);
    }

    /// <summary>
    /// Verifies that Emulator properties default to null or false.
    /// </summary>
    [Fact]
    public void EmulatorDefaultValuesAreNull()
    {
        var emulator = new Emulator();

        Assert.Null(emulator.EmulatorName);
        Assert.Null(emulator.EmulatorLocation);
        Assert.Null(emulator.EmulatorParameters);
        Assert.False(emulator.ReceiveANotificationOnEmulatorError);
        Assert.Null(emulator.ImagePackDownloadLink);
        Assert.Null(emulator.ImagePackDownloadLink2);
        Assert.Null(emulator.ImagePackDownloadLink3);
        Assert.Null(emulator.ImagePackDownloadLink4);
        Assert.Null(emulator.ImagePackDownloadLink5);
        Assert.Null(emulator.ImagePackDownloadExtractPath);
    }

    /// <summary>
    /// Verifies that multiple emulators maintain their insertion order.
    /// </summary>
    [Fact]
    public void SystemManagerWithMultipleEmulatorsPreservesOrder()
    {
        var system = new SystemManager
        {
            SystemName = "PlayStation",
            SystemFolders = ["C:\\roms\\PS1"],
            Emulators =
            [
                new Emulator { EmulatorName = "DuckStation" },
                new Emulator { EmulatorName = "RetroArch" },
                new Emulator { EmulatorName = "Mednafen" }
            ]
        };

        Assert.Equal(3, system.Emulators.Count);
        Assert.Equal("DuckStation", system.Emulators[0].EmulatorName);
        Assert.Equal("RetroArch", system.Emulators[1].EmulatorName);
        Assert.Equal("Mednafen", system.Emulators[2].EmulatorName);
    }

    /// <summary>
    /// Verifies that boolean properties on SystemManager default to false.
    /// </summary>
    [Fact]
    public void SystemManagerDefaultBoolPropertiesAreFalse()
    {
        var system = new SystemManager
        {
            SystemName = "Test"
        };

        Assert.False(system.ExtractFileBeforeLaunch);
        Assert.False(system.GroupByFolder);
        Assert.False(system.DisableRecursiveSearch);
    }
}

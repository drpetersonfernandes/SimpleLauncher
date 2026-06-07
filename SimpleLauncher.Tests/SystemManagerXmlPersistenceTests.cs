using System.Reflection;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.SystemManager;
using SimpleLauncher.Services.SystemManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class SystemManagerXmlPersistenceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _systemXmlPath;
    private readonly IConfiguration _configuration;

    public SystemManagerXmlPersistenceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_SystemXmlTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
        _systemXmlPath = Path.Combine(_testDirectory, "system.xml");

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SystemXmlPath"] = _systemXmlPath
            })
            .Build();

        ServiceProviderMock.Install(_configuration);
        ResetSystemXmlStaticState();
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
            // Best-effort cleanup
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    private static void ResetSystemXmlStaticState()
    {
        var type = typeof(SystemManager);
        type.GetField("_fileLocation", BindingFlags.NonPublic | BindingFlags.Static)?.SetValue(null, null);

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var localFile = Path.Combine(localAppData, "SimpleLauncher", "system.xml");
        try
        {
            if (File.Exists(localFile)) File.Delete(localFile);
        }
        catch
        {
            // ignored
        }
    }

    private static string BuildValidSystemXml()
    {
        var doc = new XDocument(
            new XElement("SystemConfigs",
                new XElement("SystemConfig",
                    new XElement("SystemName", "Arcade"),
                    new XElement("SystemFolders",
                        new XElement("SystemFolder", @"C:\roms\Arcade")),
                    new XElement("SystemImageFolder", @"C:\images\Arcade"),
                    new XElement("FileFormatsToSearch",
                        new XElement("FormatToSearch", ".zip")),
                    new XElement("GroupByFolder", false),
                    new XElement("DisableRecursiveSearch", false),
                    new XElement("FileFormatsToLaunch",
                        new XElement("FormatToLaunch", ".zip")),
                    new XElement("Emulators",
                        new XElement("Emulator",
                            new XElement("EmulatorName", "MAME"),
                            new XElement("EmulatorLocation", @"C:\emu\mame.exe"),
                            new XElement("EmulatorParameters", "-rompath %SYSTEMFOLDER%"),
                            new XElement("ReceiveANotificationOnEmulatorError", true))
                    )
                )
            )
        );
        return doc.ToString();
    }

    private static string BuildMultiSystemXml()
    {
        var doc = new XDocument(
            new XElement("SystemConfigs",
                new XElement("SystemConfig",
                    new XElement("SystemName", "Arcade"),
                    new XElement("SystemFolders",
                        new XElement("SystemFolder", @"C:\roms\Arcade")),
                    new XElement("SystemImageFolder", @"C:\images\Arcade"),
                    new XElement("FileFormatsToSearch",
                        new XElement("FormatToSearch", ".zip")),
                    new XElement("GroupByFolder", false),
                    new XElement("DisableRecursiveSearch", false),
                    new XElement("FileFormatsToLaunch",
                        new XElement("FormatToLaunch", ".zip")),
                    new XElement("Emulators",
                        new XElement("Emulator",
                            new XElement("EmulatorName", "MAME"),
                            new XElement("EmulatorLocation", @"C:\emu\mame.exe"),
                            new XElement("EmulatorParameters", ""),
                            new XElement("ReceiveANotificationOnEmulatorError", true))
                    )
                ),
                new XElement("SystemConfig",
                    new XElement("SystemName", "NES"),
                    new XElement("SystemFolders",
                        new XElement("SystemFolder", @"C:\roms\NES")),
                    new XElement("SystemImageFolder", @"C:\images\NES"),
                    new XElement("FileFormatsToSearch",
                        new XElement("FormatToSearch", ".nes"),
                        new XElement("FormatToSearch", ".zip")),
                    new XElement("GroupByFolder", true),
                    new XElement("DisableRecursiveSearch", true),
                    new XElement("FileFormatsToLaunch",
                        new XElement("FormatToLaunch", ".nes")),
                    new XElement("Emulators",
                        new XElement("Emulator",
                            new XElement("EmulatorName", "RetroArch"),
                            new XElement("EmulatorLocation", @"C:\emu\retroarch.exe"),
                            new XElement("EmulatorParameters", "-L nestopia %ROM%"),
                            new XElement("ReceiveANotificationOnEmulatorError", false)),
                        new XElement("Emulator",
                            new XElement("EmulatorName", "Mesen"),
                            new XElement("EmulatorLocation", @"C:\emu\mesen.exe"),
                            new XElement("EmulatorParameters", "%ROM%"),
                            new XElement("ReceiveANotificationOnEmulatorError", true))
                    )
                )
            )
        );
        return doc.ToString();
    }

    [Fact]
    public void SystemExists_ReturnsFalse_WhenXmlFileDoesNotExist()
    {
        var result = SystemManager.SystemExists("Arcade", _configuration);
        Assert.False(result);
    }

    [Fact]
    public void SystemExists_ReturnsTrue_WhenSystemIsInXml()
    {
        File.WriteAllText(_systemXmlPath, BuildValidSystemXml());
        ResetSystemXmlStaticState();

        var result = SystemManager.SystemExists("Arcade", _configuration);
        Assert.True(result);
    }

    [Fact]
    public void SystemExists_ReturnsFalse_WhenSystemIsNotInXml()
    {
        File.WriteAllText(_systemXmlPath, BuildValidSystemXml());
        ResetSystemXmlStaticState();

        var result = SystemManager.SystemExists("NES", _configuration);
        Assert.False(result);
    }

    [Fact]
    public void SystemExists_IsCaseInsensitive()
    {
        File.WriteAllText(_systemXmlPath, BuildValidSystemXml());
        ResetSystemXmlStaticState();

        var result = SystemManager.SystemExists("aRcAdE", _configuration);
        Assert.True(result);
    }

    [Fact]
    public void LoadSystemManagers_ReturnsEmptyList_WhenXmlFileDoesNotExist()
    {
        var result = SystemManager.LoadSystemManagers(_configuration);
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void LoadSystemManagers_CreatesXmlFile_WhenFileDoesNotExist()
    {
        SystemManager.LoadSystemManagers(_configuration);
        Assert.True(File.Exists(_systemXmlPath));
    }

    [Fact]
    public void LoadSystemManagers_LoadsSingleSystem_Correctly()
    {
        ResetSystemXmlStaticState();
        File.WriteAllText(_systemXmlPath, BuildValidSystemXml());

        var result = SystemManager.LoadSystemManagers(_configuration);

        Assert.Single(result);
        var system = result[0];
        Assert.Equal("Arcade", system.SystemName);
        Assert.Single(system.SystemFolders);
        Assert.Equal(@"C:\roms\Arcade", system.SystemFolders[0]);
        Assert.Equal(@"C:\images\Arcade", system.SystemImageFolder);
        Assert.Single(system.FileFormatsToSearch);
        Assert.Equal(".zip", system.FileFormatsToSearch[0]);
        Assert.False(system.ExtractFileBeforeLaunch);
        Assert.False(system.GroupByFolder);
        Assert.False(system.DisableRecursiveSearch);
        Assert.Single(system.FileFormatsToLaunch);
        Assert.Equal(".zip", system.FileFormatsToLaunch[0]);
        Assert.Single(system.Emulators);
        Assert.Equal("MAME", system.Emulators[0].EmulatorName);
    }

    [Fact]
    public void LoadSystemManagers_LoadsMultipleSystems_Correctly()
    {
        ResetSystemXmlStaticState();
        File.WriteAllText(_systemXmlPath, BuildMultiSystemXml());

        var result = SystemManager.LoadSystemManagers(_configuration);

        Assert.Equal(2, result.Count);

        var arcade = result[0];
        Assert.Equal("Arcade", arcade.SystemName);
        Assert.Single(arcade.Emulators);

        var nes = result[1];
        Assert.Equal("NES", nes.SystemName);
        Assert.Equal(2, nes.Emulators.Count);
        Assert.Equal("RetroArch", nes.Emulators[0].EmulatorName);
        Assert.Equal("Mesen", nes.Emulators[1].EmulatorName);
        Assert.True(nes.GroupByFolder);
        Assert.True(nes.DisableRecursiveSearch);
        Assert.False(nes.Emulators[0].ReceiveANotificationOnEmulatorError);
        Assert.Equal(".nes", nes.FileFormatsToLaunch[0]);
        Assert.Equal(2, nes.FileFormatsToSearch.Count);
    }

    [Fact]
    public async Task SaveSystemConfigurationAsync_AddsNewSystem()
    {
        var system = new SystemManager
        {
            SystemName = "Genesis",
            SystemFolders = [@"C:\roms\Genesis"],
            SystemImageFolder = @"C:\images\Genesis",
            FileFormatsToSearch = [".md", ".bin"],
            FileFormatsToLaunch = [".md"],
            Emulators =
            [
                new Emulator
                {
                    EmulatorName = "Blastem",
                    EmulatorLocation = @"C:\emu\blastem.exe",
                    EmulatorParameters = "%ROM%",
                    ReceiveANotificationOnEmulatorError = true
                }
            ]
        };

        await SystemManager.SaveSystemConfigurationAsync(system);

        Assert.True(File.Exists(_systemXmlPath));

        // Verify the saved content
        ResetSystemXmlStaticState();
        var loaded = SystemManager.LoadSystemManagers(_configuration);

        Assert.Single(loaded);
        Assert.Equal("Genesis", loaded[0].SystemName);
        Assert.Single(loaded[0].SystemFolders);
        Assert.Equal(@"C:\roms\Genesis", loaded[0].SystemFolders[0]);
        Assert.Equal(@"C:\images\Genesis", loaded[0].SystemImageFolder);
        Assert.Equal(2, loaded[0].FileFormatsToSearch.Count);
        Assert.Single(loaded[0].FileFormatsToLaunch);
        Assert.Single(loaded[0].Emulators);
        Assert.Equal("Blastem", loaded[0].Emulators[0].EmulatorName);
    }

    [Fact]
    public async Task SaveSystemConfigurationAsync_AddsSystemAlongsideExisting()
    {
        File.WriteAllText(_systemXmlPath, BuildValidSystemXml());
        ResetSystemXmlStaticState();

        var newSystem = new SystemManager
        {
            SystemName = "Genesis",
            SystemFolders = [@"C:\roms\Genesis"],
            SystemImageFolder = @"C:\images\Genesis",
            FileFormatsToSearch = [".md"],
            FileFormatsToLaunch = [".md"],
            Emulators =
            [
                new Emulator
                {
                    EmulatorName = "Blastem",
                    EmulatorLocation = @"C:\emu\blastem.exe",
                    EmulatorParameters = "%ROM%",
                    ReceiveANotificationOnEmulatorError = true
                }
            ]
        };

        await SystemManager.SaveSystemConfigurationAsync(newSystem);

        ResetSystemXmlStaticState();
        var loaded = SystemManager.LoadSystemManagers(_configuration);

        Assert.Equal(2, loaded.Count);
        Assert.Contains(loaded, static s => s.SystemName == "Arcade");
        Assert.Contains(loaded, static s => s.SystemName == "Genesis");
    }

    [Fact]
    public async Task SaveSystemConfigurationAsync_UpdatesExistingSystem()
    {
        File.WriteAllText(_systemXmlPath, BuildValidSystemXml());
        ResetSystemXmlStaticState();

        var updatedSystem = new SystemManager
        {
            SystemName = "Arcade",
            SystemFolders = [@"C:\roms\Arcade", @"D:\backup\Arcade"],
            SystemImageFolder = @"C:\images\ArcadeUpdated",
            FileFormatsToSearch = [".zip", ".7z"],
            FileFormatsToLaunch = [".zip"],
            Emulators =
            [
                new Emulator
                {
                    EmulatorName = "RetroArch",
                    EmulatorLocation = @"C:\emu\retroarch.exe",
                    EmulatorParameters = "-L mame %ROM%",
                    ReceiveANotificationOnEmulatorError = false
                }
            ]
        };

        await SystemManager.SaveSystemConfigurationAsync(updatedSystem);

        ResetSystemXmlStaticState();
        var loaded = SystemManager.LoadSystemManagers(_configuration);

        Assert.Single(loaded);
        Assert.Equal("Arcade", loaded[0].SystemName);
        Assert.Equal(2, loaded[0].SystemFolders.Count);
        Assert.Equal(@"C:\images\ArcadeUpdated", loaded[0].SystemImageFolder);
        Assert.Single(loaded[0].Emulators);
        Assert.Equal("RetroArch", loaded[0].Emulators[0].EmulatorName);
        Assert.False(loaded[0].Emulators[0].ReceiveANotificationOnEmulatorError);
    }

    [Fact]
    public async Task SaveSystemConfigurationAsync_RenamesSystem()
    {
        File.WriteAllText(_systemXmlPath, BuildValidSystemXml());
        ResetSystemXmlStaticState();

        var renamedSystem = new SystemManager
        {
            SystemName = "ArcadeRenamed",
            SystemFolders = [@"C:\roms\Arcade"],
            SystemImageFolder = @"C:\images\Arcade",
            FileFormatsToSearch = [".zip"],
            FileFormatsToLaunch = [".zip"],
            Emulators =
            [
                new Emulator
                {
                    EmulatorName = "MAME",
                    EmulatorLocation = @"C:\emu\mame.exe",
                    EmulatorParameters = "",
                    ReceiveANotificationOnEmulatorError = true
                }
            ]
        };

        // originalSystemName = "Arcade" tells SaveSystemConfigurationAsync to update the old name
        await SystemManager.SaveSystemConfigurationAsync(renamedSystem, "Arcade");

        ResetSystemXmlStaticState();
        var loaded = SystemManager.LoadSystemManagers(_configuration);

        Assert.Single(loaded);
        Assert.Equal("ArcadeRenamed", loaded[0].SystemName);
    }

    [Fact]
    public async Task DeleteSystemAsync_RemovesSystem()
    {
        File.WriteAllText(_systemXmlPath, BuildMultiSystemXml());
        ResetSystemXmlStaticState();

        SystemManager.DeleteSystemAsync("Arcade");

        // Give it a moment since it's async void
        await Task.Delay(200);

        ResetSystemXmlStaticState();
        var loaded = SystemManager.LoadSystemManagers(_configuration);

        Assert.Single(loaded);
        Assert.Equal("NES", loaded[0].SystemName);
    }

    [Fact]
    public async Task DeleteSystemAsync_DoesNothing_WhenSystemNotFound()
    {
        File.WriteAllText(_systemXmlPath, BuildValidSystemXml());
        ResetSystemXmlStaticState();

        SystemManager.DeleteSystemAsync("NonExistent");

        await Task.Delay(200);

        ResetSystemXmlStaticState();
        var loaded = SystemManager.LoadSystemManagers(_configuration);

        Assert.Single(loaded);
        Assert.Equal("Arcade", loaded[0].SystemName);
    }
}

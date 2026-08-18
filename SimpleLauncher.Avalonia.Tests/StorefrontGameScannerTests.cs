using System.Xml.Linq;
using SimpleLauncher.Avalonia.Services.GameScan;
using SimpleLauncher.Avalonia.Services.SystemManager;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for <see cref="StorefrontGameScanner"/> — "Microsoft Windows" system creation,
/// .url shortcut materialization, idempotence, and ignored-name filtering.
/// All I/O is isolated to a temp system.xml and the test output directory.
/// </summary>
public class StorefrontGameScannerTests
{
    private static string DefaultRomsPath =>
        PathHelper.ResolveRelativeToAppDirectory($"%BASEFOLDER%\\roms\\{StorefrontGameScanner.WindowsSystemName}")!;

    private static string DefaultImagesPath =>
        PathHelper.ResolveRelativeToAppDirectory($"%BASEFOLDER%\\images\\{StorefrontGameScanner.WindowsSystemName}")!;

    private static StorefrontGameScanner ScannerWithTempSystemXml(string systemXmlPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(systemXmlPath)!);
        File.WriteAllText(systemXmlPath, "<SystemConfigs />");

        var json = $$"""{"SystemXmlPath": "{{systemXmlPath.Replace("\\", "\\\\")}}"}""";
        var config = TestEnvironment.ConfigurationFromJson(json);
        return new StorefrontGameScanner(new SystemManagerService(config), config);
    }

    private static List<(string Name, string ExePath, string Storefront)> Games(params (string Name, string ExePath, string Storefront)[] games)
    {
        return [.. games];
    }

    /// <summary>
    /// Removes the default %BASEFOLDER% roms/images folders left by previous test runs
    /// (same test-bin location every run, so shortcuts would otherwise be skipped).
    /// </summary>
    private static void CleanDefaultFolders()
    {
        try
        {
            if (Directory.Exists(DefaultRomsPath)) Directory.Delete(DefaultRomsPath, true);
            if (Directory.Exists(DefaultImagesPath)) Directory.Delete(DefaultImagesPath, true);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to clean default scanner test folders");
        }
    }

    public StorefrontGameScannerTests()
    {
        CleanDefaultFolders();
    }

    [Fact]
    public void CreateShortcutsForGames_CreatesSystemEntryAndShortcuts_OnFirstRun()
    {
        var systemXml = Path.Combine(Path.GetTempPath(), "SimpleLauncherScannerTests", Guid.NewGuid().ToString("N"), "system.xml");
        var scanner = ScannerWithTempSystemXml(systemXml);
        var games = Games(
            ("Hollow Knight", @"C:\Games\HollowKnight\hollow.exe", "Steam"),
            ("Doom", @"C:\Games\Doom\doom.exe", "GOG"));

        var result = scanner.CreateShortcutsForGames(games);

        Assert.Equal(2, result.GamesFound);
        Assert.Equal(2, result.ShortcutsCreated);
        Assert.True(result.SystemWasCreated);
        Assert.True(Directory.Exists(DefaultImagesPath));

        var hollowShortcut = Path.Combine(DefaultRomsPath, "Hollow Knight.url");
        Assert.True(File.Exists(hollowShortcut));
        var content = File.ReadAllText(hollowShortcut);
        Assert.StartsWith("[InternetShortcut]", content);
        Assert.Contains("URL=file:///C:\\Games\\HollowKnight\\hollow.exe", content);

        var doc = XDocument.Load(systemXml);
        var config = Assert.Single(doc.Root!.Elements("SystemConfig"),
            static e => e.Element("SystemName")?.Value == StorefrontGameScanner.WindowsSystemName);
        Assert.Equal(StorefrontGameScanner.WindowsSystemName, config.Element("SystemName")?.Value);
        Assert.Equal(
            ["url", "lnk", "bat"],
            config.Element("FileFormatsToSearch")?.Elements("FormatToSearch").Select(static e => e.Value).ToList());
        Assert.Equal("Direct Launch", config.Element("Emulators")?.Element("Emulator")?.Element("EmulatorName")?.Value);
        Assert.Equal(
            $"%BASEFOLDER%\\roms\\{StorefrontGameScanner.WindowsSystemName}",
            config.Element("SystemFolders")?.Element("SystemFolder")?.Value);
    }

    [Fact]
    public void CreateShortcutsForGames_SecondRun_AddsOnlyNewGames()
    {
        var systemXml = Path.Combine(Path.GetTempPath(), "SimpleLauncherScannerTests", Guid.NewGuid().ToString("N"), "system.xml");
        var scanner = ScannerWithTempSystemXml(systemXml);
        var firstRun = Games(
            ("Hollow Knight", @"C:\Games\HollowKnight\hollow.exe", "Steam"),
            ("Doom", @"C:\Games\Doom\doom.exe", "GOG"));

        var first = scanner.CreateShortcutsForGames(firstRun);
        var second = scanner.CreateShortcutsForGames(firstRun);
        var third = scanner.CreateShortcutsForGames(Games(("Doom", @"C:\Games\Doom\doom.exe", "GOG"), ("Quake", @"C:\Games\Quake\quake.exe", "Steam")));

        Assert.Equal(2, first.ShortcutsCreated);
        Assert.True(first.SystemWasCreated);

        Assert.Equal(0, second.ShortcutsCreated);
        Assert.False(second.SystemWasCreated);

        Assert.Equal(1, third.ShortcutsCreated); // only the new game
        Assert.False(third.SystemWasCreated);

        var doc = XDocument.Load(systemXml);
        var config = Assert.Single(doc.Root!.Elements("SystemConfig"),
            static e => e.Element("SystemName")?.Value == StorefrontGameScanner.WindowsSystemName);

        // Existing shortcut content is never overwritten
        var doomShortcut = Path.Combine(DefaultRomsPath, "Doom.url");
        var before = File.ReadAllText(doomShortcut);
        scanner.CreateShortcutsForGames(firstRun);
        Assert.Equal(before, File.ReadAllText(doomShortcut));
    }

    [Fact]
    public void CreateShortcutsForGames_SkipsIgnoredNamesAndEmptyExePaths()
    {
        var systemXml = Path.Combine(Path.GetTempPath(), "SimpleLauncherScannerTests", Guid.NewGuid().ToString("N"), "system.xml");
        var scanner = ScannerWithTempSystemXml(systemXml);
        var games = Games(
            ("Spacewar", @"C:\Games\steam\spacewar.exe", "Steam"), // launcher meta title
            ("Battle.net", @"C:\BattleNet\battle.net.exe", "Battle.net"),
            ("NoExe", "", "Steam"), // no executable resolved
            ("Real Game", @"C:\Games\Real\game.exe", "Steam"));

        var result = scanner.CreateShortcutsForGames(games);

        Assert.Equal(1, result.ShortcutsCreated);
        Assert.False(File.Exists(Path.Combine(DefaultRomsPath, "Spacewar.url")));
        Assert.False(File.Exists(Path.Combine(DefaultRomsPath, "Battle.net.url")));
        Assert.True(File.Exists(Path.Combine(DefaultRomsPath, "Real Game.url")));
    }

    [Fact]
    public void CreateShortcutsForGames_SanitizesGameNamesForFileNames()
    {
        var systemXml = Path.Combine(Path.GetTempPath(), "SimpleLauncherScannerTests", Guid.NewGuid().ToString("N"), "system.xml");
        var scanner = ScannerWithTempSystemXml(systemXml);
        var games = Games(
            ("Halo: Combat Evolved (Anniversary)", @"C:\Games\Halo\halo.exe", "Steam"),
            ("Game..with..dots", @"C:\Games\Dots\game.exe", "GOG"));

        var result = scanner.CreateShortcutsForGames(games);

        Assert.Equal(2, result.ShortcutsCreated);
        var sanitizedHalo = Core.Services.SanitizeInputString.SanitizeInputSystemName.SanitizeFolderName("Halo: Combat Evolved (Anniversary)");
        var sanitizedDots = Core.Services.SanitizeInputString.SanitizeInputSystemName.SanitizeFolderName("Game..with..dots");
        Assert.True(File.Exists(Path.Combine(DefaultRomsPath, $"{sanitizedHalo}.url")));
        Assert.True(File.Exists(Path.Combine(DefaultRomsPath, $"{sanitizedDots}.url")));
    }

    [Fact]
    public void CreateShortcutsForGames_ExistingSystem_UsesItsConfiguredFolder()
    {
        var systemXml = Path.Combine(Path.GetTempPath(), "SimpleLauncherScannerTests", Guid.NewGuid().ToString("N"), "system.xml");
        var customRoms = Path.Combine(Path.GetTempPath(), "SimpleLauncherScannerTests", Guid.NewGuid().ToString("N"), "custom roms");
        Directory.CreateDirectory(Path.GetDirectoryName(systemXml)!);
        Directory.CreateDirectory(customRoms);
        File.WriteAllText(systemXml, $"""
            <SystemConfigs>
              <SystemConfig>
                <SystemName>Microsoft Windows</SystemName>
                <SystemFolders>
                  <SystemFolder>{customRoms}</SystemFolder>
                </SystemFolders>
                <SystemImageFolder />
                <FileFormatsToSearch>
                  <FormatToSearch>url</FormatToSearch>
                </FileFormatsToSearch>
                <GroupByFolder>false</GroupByFolder>
                <DisableRecursiveSearch>false</DisableRecursiveSearch>
              </SystemConfig>
            </SystemConfigs>
            """);
        var config = TestEnvironment.ConfigurationFromJson(
            $$"""{"SystemXmlPath": "{{systemXml.Replace("\\", "\\\\")}}"}""");
        var scanner = new StorefrontGameScanner(new SystemManagerService(config), config);

        var result = scanner.CreateShortcutsForGames(Games(("Skyrim", @"C:\Games\Skyrim\skyrim.exe", "Steam")));

        Assert.False(result.SystemWasCreated);
        Assert.Equal(1, result.ShortcutsCreated);
        Assert.True(File.Exists(Path.Combine(customRoms, "Skyrim.url")));
        Assert.False(File.Exists(Path.Combine(DefaultRomsPath, "Skyrim.url")));
    }

    [Fact]
    public void IgnoredGameNames_MatchesWpfScannerSet()
    {
        var expected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Steamworks Common Redistributables",
            "Unreal Engine",
            "Fab UE Plugin",
            "Quixel Bridge",
            "DirectX",
            "Google Earth VR",
            "Spacewar",
            "PC Health Check",
            "Rockstar Games Launcher",
            "Battle.net",
            "Ubisoft Connect"
        };

        Assert.Equal(expected, StorefrontGameScanner.IgnoredGameNames);
    }
}

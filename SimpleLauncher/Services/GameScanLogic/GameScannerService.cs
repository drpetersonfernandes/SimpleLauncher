using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class GameScannerService
{
    private readonly ILogErrors _logErrors;
    private const string WindowsSystemName = "Microsoft Windows";

    internal static readonly HashSet<string> IgnoredGameNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Steamworks Common Redistributables",
        "Unreal Engine",
        "Fab UE Plugin",
        "Quixel Bridge",
        "DirectX",
        "Google Earth VR",
        "Spacewar",
        "PC Health Check"
    };

    // Whitelist for Microsoft Store games to avoid adding Calculator/Photos etc.
    public static readonly string[] MicrosoftStoreKeywords =
    {
        "Minecraft", "Solitaire", "Forza", "Halo", "Gears of War", "Sea of Thieves", "Flight Simulator", "Age of Empires", "Among Us", "Roblox"
    };

    private readonly string _windowsRomsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", "Microsoft Windows");
    private readonly string _windowsImagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "Microsoft Windows");
    private readonly string _systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

    public bool WasNewSystemCreated { get; private set; }

    public GameScannerService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public async Task ScanForStoreGamesAsync()
    {
        try
        {
            WasNewSystemCreated = await EnsureWindowsSystemExistsAsync();

            var tasks = new List<Task>
            {
                ScanSteamGames.ScanSteamGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanEpicGames.ScanEpicGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                // ScanAmazonGamesAsync(),
                // ScanBattleNetGamesAsync(),
                ScanGogGames.ScanGogGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                // ScanHumbleGamesAsync(),
                ScanItchioGames.ScanItchioGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames), // Added Itch
                // ScanRockstarGamesAsync(),
                // ScanXboxGamesAsync(),
                ScanUplayGames.ScanUplayGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanEaGames.ScanEaGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanMicrosoftStoreGames.ScanMicrosoftStoreGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath) // Added images path arg
            };

            await Task.WhenAll(tasks);

            DebugLogger.Log("[GameScannerService] All store game scans completed.");
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred during the game scanning process.");
        }
    }

    private async Task<bool> EnsureWindowsSystemExistsAsync()
    {
        try
        {
            XDocument xmlDoc;
            if (File.Exists(_systemXmlPath))
            {
                var xmlContent = await File.ReadAllTextAsync(_systemXmlPath);
                xmlDoc = string.IsNullOrWhiteSpace(xmlContent)
                    ? new XDocument(new XElement("SystemConfigs"))
                    : XDocument.Parse(xmlContent);
            }
            else
            {
                xmlDoc = new XDocument(new XElement("SystemConfigs"));
            }

            var systemExists = xmlDoc.Root?.Elements("SystemConfig")
                .Any(static el => el.Element("SystemName")?.Value.Equals(WindowsSystemName, StringComparison.OrdinalIgnoreCase) ?? false) ?? false;

            if (systemExists) return false;

            DebugLogger.Log($"[GameScannerService] '{WindowsSystemName}' system not found. Creating it now.");

            var newSystemElement = new XElement("SystemConfig",
                new XElement("SystemName", WindowsSystemName),
                new XElement("SystemFolders", new XElement("SystemFolder", "%BASEFOLDER%\\roms\\Microsoft Windows")),
                new XElement("SystemImageFolder", "%BASEFOLDER%\\images\\Microsoft Windows"),
                new XElement("SystemIsMAME", "false"),
                new XElement("FileFormatsToSearch",
                    new XElement("FormatToSearch", "url"),
                    new XElement("FormatToSearch", "lnk"),
                    new XElement("FormatToSearch", "bat")
                ),
                new XElement("GroupByFolder", "false"),
                new XElement("ExtractFileBeforeLaunch", "false"),
                new XElement("FileFormatsToLaunch"),
                new XElement("Emulators",
                    new XElement("Emulator",
                        new XElement("EmulatorName", "Direct Launch"),
                        new XElement("EmulatorLocation", ""),
                        new XElement("EmulatorParameters", ""),
                        new XElement("ReceiveANotificationOnEmulatorError", "true")
                    )
                )
            );

            xmlDoc.Root?.Add(newSystemElement);

            var settings = new XmlWriterSettings { Indent = true, Async = true };
            await using (var writer = XmlWriter.Create(_systemXmlPath, settings))
            {
                await xmlDoc.SaveAsync(writer, CancellationToken.None);
            }

            Directory.CreateDirectory(_windowsRomsPath);
            Directory.CreateDirectory(_windowsImagesPath);

            return true;
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "Failed to create 'Microsoft Windows' system in system.xml.");
            return false;
        }
    }

    internal static Task ExtractIconFromGameFolder(string gameInstallPath, string sanitizedGameName, string windowsImagesPath, string specificExePath = null)
    {
        var iconPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
        if (File.Exists(iconPath) || !Directory.Exists(gameInstallPath)) return Task.CompletedTask;

        var mainExe = specificExePath;

        if (string.IsNullOrEmpty(mainExe) || !File.Exists(mainExe))
        {
            // Heuristics to find the main EXE
            var exeFiles = Directory.GetFiles(gameInstallPath, "*.exe", SearchOption.TopDirectoryOnly);

            // 1. Name match
            // 2. Contains name
            mainExe = exeFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(sanitizedGameName, StringComparison.OrdinalIgnoreCase)) ?? exeFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Contains(sanitizedGameName, StringComparison.OrdinalIgnoreCase));

            // 3. Largest EXE (ignoring uninstallers/unity crash handlers)
            if (mainExe == null && exeFiles.Length > 0)
            {
                mainExe = exeFiles
                    .Where(static f => !f.Contains("unins", StringComparison.OrdinalIgnoreCase) &&
                                       !f.Contains("setup", StringComparison.OrdinalIgnoreCase) &&
                                       !f.Contains("crash", StringComparison.OrdinalIgnoreCase) &&
                                       !f.Contains("unity", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(static f => new FileInfo(f).Length)
                    .FirstOrDefault();
            }
        }

        if (mainExe != null && File.Exists(mainExe))
        {
            try
            {
                IconExtractor.SaveIconFromExe(mainExe, iconPath);
            }
            catch
            {
                /* Ignore icon extraction failure */
            }
        }

        return Task.CompletedTask;
    }
}
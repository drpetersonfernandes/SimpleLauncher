using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;
using System.Xml;

namespace SimpleLauncher.Services;

/// <summary>
/// Scans for games installed via Steam and Epic Games Launcher and integrates them into Simple Launcher.
/// </summary>
public class GameScannerService
{
    private readonly ILogErrors _logErrors;
    private const string WindowsSystemName = "Microsoft Windows";

    private static readonly HashSet<string> IgnoredGameNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Steamworks Common Redistributables",
        "Unreal Engine",
        "Fab UE Plugin",
        "Quixel Bridge"
    };

    private readonly string _windowsRomsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", "Microsoft Windows");
    private readonly string _windowsImagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "Microsoft Windows");
    private readonly string _systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

    public bool WasNewSystemCreated { get; private set; }

    public GameScannerService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    /// <summary>
    /// Starts the background scan for games from external launchers.
    /// </summary>
    public async Task ScanForStoreGamesAsync()
    {
        try
        {
            WasNewSystemCreated = await EnsureWindowsSystemExistsAsync();

            // Run scans in parallel
            await Task.WhenAll(ScanSteamGamesAsync(), ScanEpicGamesAsync());

            DebugLogger.Log("[GameScannerService] Steam and Epic Games scan completed.");
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred during the game scanning process.");
        }
    }

    /// <summary>
    /// Ensures that a "Microsoft Windows" system is configured in system.xml.
    /// If not present, it creates one.
    /// </summary>
    private async Task<bool> EnsureWindowsSystemExistsAsync()
    {
        try
        {
            XDocument xmlDoc;
            if (File.Exists(_systemXmlPath))
            {
                var xmlContent = await File.ReadAllTextAsync(_systemXmlPath);
                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    xmlDoc = new XDocument(new XElement("SystemConfigs"));
                }
                else
                {
                    xmlDoc = XDocument.Parse(xmlContent);
                }
            }
            else
            {
                xmlDoc = new XDocument(new XElement("SystemConfigs"));
            }

            var systemExists = xmlDoc.Root?.Elements("SystemConfig")
                .Any(el => el.Element("SystemName")?.Value.Equals(WindowsSystemName, StringComparison.OrdinalIgnoreCase) ?? false) ?? false;

            if (systemExists)
            {
                return false; // System already exists, nothing to do.
            }

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

            // Create the necessary directories
            Directory.CreateDirectory(_windowsRomsPath);
            Directory.CreateDirectory(_windowsImagesPath);

            DebugLogger.Log($"[GameScannerService] Successfully created and configured '{WindowsSystemName}' system.");
            return true;
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "Failed to create 'Microsoft Windows' system in system.xml.");
            return false;
        }
    }

    /// <summary>
    /// Scans for installed Steam games and creates launchers and artwork.
    /// </summary>
    private async Task ScanSteamGamesAsync()
    {
        try
        {
            var steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string;
            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
            {
                DebugLogger.Log("[GameScannerService] Steam installation not found.");
                return;
            }

            var libraryFoldersVdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersVdf))
            {
                DebugLogger.Log("[GameScannerService] libraryfolders.vdf not found.");
                return;
            }

            var libraryPaths = new List<string> { Path.Combine(steamPath, "steamapps") }; // Add default library
            try
            {
                var vdfData = VdfParser.Parse(libraryFoldersVdf);
                if (vdfData.TryGetValue("libraryfolders", out var folders) && folders is Dictionary<string, object> folderDict)
                {
                    foreach (var folderNode in folderDict.Values)
                    {
                        if (folderNode is Dictionary<string, object> pathDict && pathDict.TryGetValue("path", out var pathObj) && pathObj is string path)
                        {
                            libraryPaths.Add(Path.Combine(path, "steamapps"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logErrors.LogErrorAsync(ex, $"Failed to parse Steam's libraryfolders.vdf file at {libraryFoldersVdf}");
                // Continue with just the default library path
            }

            foreach (var libraryPath in libraryPaths.Distinct())
            {
                if (!Directory.Exists(libraryPath)) continue;

                var manifestFiles = Directory.GetFiles(libraryPath, "appmanifest_*.acf");
                foreach (var manifestFile in manifestFiles)
                {
                    try
                    {
                        var appData = VdfParser.Parse(manifestFile);
                        if (appData.TryGetValue("AppState", out var appState) && appState is Dictionary<string, object> appStateDict)
                        {
                            if (appStateDict.TryGetValue("name", out var nameObj) && nameObj is string gameName &&
                                appStateDict.TryGetValue("appid", out var appIdObj) && appIdObj is string appId)
                            {
                                // Check against the ignore list
                                if (IgnoredGameNames.Contains(gameName))
                                {
                                    DebugLogger.Log($"[GameScannerService] Ignoring Steam app: {gameName}");
                                    continue;
                                }

                                var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                                var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                                // Create .url shortcut
                                var shortcutContent = $"[InternetShortcut]\nURL=steam://run/{appId}";
                                await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                                // Copy artwork
                                var artworkPath = Path.Combine(steamPath, "appcache", "librarycache", $"{appId}_library_600x900.jpg");
                                if (File.Exists(artworkPath))
                                {
                                    var destArtworkPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.jpg");
                                    File.Copy(artworkPath, destArtworkPath, true);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await _logErrors.LogErrorAsync(ex, $"Failed to process Steam manifest: {manifestFile}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for Steam games.");
        }
    }

    /// <summary>
    /// Scans for installed Epic Games and creates launchers and artwork.
    /// </summary>
    private async Task ScanEpicGamesAsync()
    {
        try
        {
            const string manifestsPath = @"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests";
            if (!Directory.Exists(manifestsPath))
            {
                DebugLogger.Log("[GameScannerService] Epic Games manifests directory not found.");
                return;
            }

            var manifestFiles = Directory.GetFiles(manifestsPath, "*.item");
            foreach (var manifestFile in manifestFiles)
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(manifestFile);
                    using var doc = JsonDocument.Parse(jsonContent);
                    var root = doc.RootElement;

                    var displayName = root.GetProperty("DisplayName").GetString();
                    var appName = root.GetProperty("AppName").GetString();
                    var installLocation = root.GetProperty("InstallLocation").GetString();
                    var launchExecutable = root.GetProperty("LaunchExecutable").GetString();

                    if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(appName)) continue;

                    // Check against the ignore list
                    if (IgnoredGameNames.Contains(displayName))
                    {
                        DebugLogger.Log($"[GameScannerService] Ignoring Epic Games app: {displayName}");
                        continue;
                    }

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(displayName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                    // Create .url shortcut
                    var shortcutContent = $"[InternetShortcut]\nURL=com.epicgames.launcher://apps/{appName}?action=launch&silent=true";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    // Extract and save icon from executable
                    if (installLocation != null)
                    {
                        if (launchExecutable != null)
                        {
                            var exePath = Path.Combine(installLocation, launchExecutable);
                            if (File.Exists(exePath))
                            {
                                var iconPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.png");
                                IconExtractor.SaveIconFromExe(exePath, iconPath);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _logErrors.LogErrorAsync(ex, $"Failed to process Epic Games manifest: {manifestFile}");
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for Epic games.");
        }
    }
}
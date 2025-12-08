using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services;

public class GameScannerService
{
    private readonly ILogErrors _logErrors;
    private const string WindowsSystemName = "Microsoft Windows";

    private static readonly HashSet<string> IgnoredGameNames = new(StringComparer.OrdinalIgnoreCase)
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
    private static readonly string[] MicrosoftStoreKeywords =
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
                ScanSteamGamesAsync(),
                ScanEpicGamesAsync(),
                ScanGogGamesAsync(),
                ScanUbisoftGamesAsync(),
                ScanEaGamesAsync(),
                ScanMicrosoftStoreGamesAsync()
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

    #region Steam

    private async Task ScanSteamGamesAsync()
    {
        var libraryPaths = new List<string>();

        try
        {
            // Prioritize HKCU as it reflects the current user's installation
            var steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;

            if (string.IsNullOrEmpty(steamPath))
            {
                steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string;
            }

            if (string.IsNullOrEmpty(steamPath))
            {
                steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string;
            }

            if (string.IsNullOrEmpty(steamPath))
            {
                steamPath = GetSteamPathFromProcess();
            }

            if (string.IsNullOrEmpty(steamPath))
            {
                DebugLogger.Log("[GameScannerService] Steam installation not found.");
                return;
            }

            // Fix separators
            steamPath = steamPath.Replace('/', '\\');

            // 1. Add Default Library
            libraryPaths.Add(Path.Combine(steamPath, "steamapps"));

            // 2. Parse libraryfolders.vdf for external libraries
            var libraryFoldersVdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (File.Exists(libraryFoldersVdf))
            {
                try
                {
                    var vdfData = SteamVdfParser.Parse(libraryFoldersVdf);

                    // Handle new VDF format (numeric keys at root or inside "libraryfolders")
                    var rootNode = vdfData.TryGetValue("libraryfolders", out var value)
                        ? value as Dictionary<string, object>
                        : vdfData;

                    if (rootNode != null)
                    {
                        foreach (var kvp in rootNode)
                        {
                            // Check if value is a dictionary containing "path"
                            if (kvp.Value is Dictionary<string, object> libData &&
                                libData.TryGetValue("path", out var pathObj) &&
                                pathObj is string pathStr)
                            {
                                if (!string.Equals(pathStr, steamPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    libraryPaths.Add(Path.Combine(pathStr, "steamapps"));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[GameScannerService] Error parsing libraryfolders.vdf: {ex.Message}");
                }
            }

            // 3. Scan for Games in all libraries
            foreach (var libraryPath in libraryPaths.Distinct())
            {
                if (!Directory.Exists(libraryPath)) continue;

                // Standard AppManifests
                var manifestFiles = Directory.GetFiles(libraryPath, "appmanifest_*.acf");
                foreach (var manifestFile in manifestFiles)
                {
                    await ProcessSteamManifest(manifestFile, libraryPath, steamPath);
                }
            }

            // 4. Scan for Source Mods
            // Mods are usually in Steam\steamapps\sourcemods
            var sourceModsPath = Path.Combine(steamPath, "steamapps", "sourcemods");
            if (Directory.Exists(sourceModsPath))
            {
                var modDirectories = Directory.GetDirectories(sourceModsPath);
                foreach (var modDir in modDirectories)
                {
                    await ProcessSourceMod(modDir);
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for Steam games.");
        }
    }

    private async Task ProcessSteamManifest(string manifestFile, string libraryPath, string steamPath)
    {
        try
        {
            var appData = SteamVdfParser.Parse(manifestFile);
            if (appData.TryGetValue("AppState", out var appState) && appState is Dictionary<string, object> appStateDict)
            {
                if (appStateDict.TryGetValue("name", out var nameObj) && nameObj is string gameName &&
                    appStateDict.TryGetValue("appid", out var appIdObj) && appIdObj is string appId &&
                    appStateDict.TryGetValue("installdir", out var installDirObj) && installDirObj is string installDir)
                {
                    if (IgnoredGameNames.Contains(gameName)) return;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");
                    var gameInstallPath = Path.Combine(libraryPath, "common", installDir);

                    var shortcutContent = $"[InternetShortcut]\nURL=steam://run/{appId}";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    await TryCopySteamArtworkAsync(steamPath, appId, sanitizedGameName, gameInstallPath);
                }
            }
        }
        catch
        {
            // Ignore individual manifest errors
        }
    }

    private async Task ProcessSourceMod(string modDir)
    {
        try
        {
            var gameInfoPath = Path.Combine(modDir, "gameinfo.txt");
            if (!File.Exists(gameInfoPath)) return;

            // Simple parsing for game name in gameinfo.txt
            var lines = await File.ReadAllLinesAsync(gameInfoPath);
            string gameName = null;
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("game", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = trimmed.Split('"', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        gameName = parts[1];
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(gameName))
            {
                gameName = new DirectoryInfo(modDir).Name;
            }

            var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
            Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

            // Steam launches mods via AppID. Mods usually don't have a numeric AppID in the traditional sense
            // accessible easily without calculating CRC, but we can launch via steam.exe -applaunch <AppID> <ModDirName>
            // However, for SimpleLauncher, we might need to point to the bat or exe if it exists,
            // or rely on Steam recognizing the mod ID if we can calculate it.
            // Simplified approach: Create a BAT file to launch steam with mod parameters.

            // Note: Playnite calculates ModID using CRC of folder name + 0x80000000.
            // For simplicity here, we will try to find a direct executable or skip complex mod ID calculation
            // to avoid external dependencies like SteamKit2 logic in this snippet.
            // Alternative: Launch via steam.exe -applaunch <BaseAppID> -game <ModDir>
            // Base AppID for Source SDK Base 2013 Singleplayer is 243730, etc. This is complex.
            // Let's skip creating shortcuts for mods if we can't easily determine the AppID,
            // or just log it.

            DebugLogger.Log($"[GameScannerService] Found Source Mod: {gameName}. Skipping shortcut creation (requires base AppID resolution).");
        }
        catch
        {
            // Ignore mod errors
        }
    }

    private static string GetSteamPathFromProcess()
    {
        try
        {
            var steamProcess = Process.GetProcessesByName("steam").FirstOrDefault();
            return steamProcess?.MainModule?.FileName != null
                ? Path.GetDirectoryName(steamProcess.MainModule.FileName)
                : null;
        }
        catch
        {
            return null;
        }
    }

    private Task TryCopySteamArtworkAsync(string steamPath, string appId, string sanitizedGameName, string gameInstallPath)
    {
        var destArtworkPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.jpg");
        if (File.Exists(destArtworkPath)) return Task.CompletedTask;

        var cachePath = Path.Combine(steamPath, "appcache", "librarycache");
        if (!Directory.Exists(cachePath)) return Task.CompletedTask;

        string[] searchPatterns =
        {
            $"{appId}_library_600x900.jpg",
            $"{appId}_header.jpg",
            $"{appId}_library_hero.jpg"
        };

        foreach (var pattern in searchPatterns)
        {
            var sourcePath = Path.Combine(cachePath, pattern);
            if (File.Exists(sourcePath))
            {
                try
                {
                    File.Copy(sourcePath, destArtworkPath, true);
                    return Task.CompletedTask;
                }
                catch
                {
                    /* Ignore */
                }
            }
        }

        // Fallback to EXE icon
        return ExtractIconFromGameFolder(gameInstallPath, sanitizedGameName);
    }

    #endregion

    #region Epic Games

    private async Task ScanEpicGamesAsync()
    {
        try
        {
            // Method 1: LauncherInstalled.dat (Preferred/Faster)
            var allUsersPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic");
            var installedDatPath = Path.Combine(allUsersPath, "UnrealEngineLauncher", "LauncherInstalled.dat");

            if (File.Exists(installedDatPath))
            {
                try
                {
                    var jsonString = await File.ReadAllTextAsync(installedDatPath);
                    var installedApps = JsonSerializer.Deserialize<EpicInstalledAppList>(jsonString);

                    if (installedApps?.InstallationList != null)
                    {
                        foreach (var app in installedApps.InstallationList)
                        {
                            // Filter out Unreal Engine tools (UE_...) and other non-games
                            if (string.IsNullOrEmpty(app.AppName) || app.AppName.StartsWith("UE_", StringComparison.Ordinal) || app.AppName.StartsWith("Falcon", StringComparison.Ordinal))
                                continue;

                            // We need the display name. LauncherInstalled.dat usually has AppName as the ID (e.g., "Codename").
                            // We might need to cross-reference with Manifests to get the pretty DisplayName.
                            // So we use the InstallLocation to find the manifest or just use the folder name as fallback.

                            string displayName = null;
                            string launchExe = null;

                            // Try to find matching manifest to get DisplayName and Executable
                            var manifestsPath = Path.Combine(allUsersPath, "EpicGamesLauncher", "Data", "Manifests");
                            if (Directory.Exists(manifestsPath))
                            {
                                var manifestFiles = Directory.GetFiles(manifestsPath, "*.item");
                                foreach (var mFile in manifestFiles)
                                {
                                    // Simple check to avoid full parse if possible, or just parse
                                    var content = await File.ReadAllTextAsync(mFile);
                                    if (content.Contains($"\"AppName\": \"{app.AppName}\""))
                                    {
                                        using var doc = JsonDocument.Parse(content);
                                        if (doc.RootElement.TryGetProperty("DisplayName", out var dn))
                                        {
                                            displayName = dn.GetString();
                                        }

                                        if (doc.RootElement.TryGetProperty("LaunchExecutable", out var le))
                                        {
                                            launchExe = le.GetString();
                                        }

                                        break;
                                    }
                                }
                            }

                            if (string.IsNullOrEmpty(displayName))
                            {
                                displayName = new DirectoryInfo(app.InstallLocation).Name;
                            }

                            if (IgnoredGameNames.Contains(displayName)) continue;

                            await CreateEpicShortcut(displayName, app.AppName, app.InstallLocation, launchExe);
                        }

                        return; // Successfully processed via DAT file
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[GameScannerService] Error reading Epic LauncherInstalled.dat: {ex.Message}. Falling back to manifests.");
                }
            }

            // Method 2: Fallback to scanning Manifests directly
            var manifestsDir = Path.Combine(allUsersPath, "EpicGamesLauncher", "Data", "Manifests");
            if (Directory.Exists(manifestsDir))
            {
                var manifestFiles = Directory.GetFiles(manifestsDir, "*.item");
                foreach (var manifestFile in manifestFiles)
                {
                    try
                    {
                        var jsonContent = await File.ReadAllTextAsync(manifestFile);
                        using var doc = JsonDocument.Parse(jsonContent);
                        var root = doc.RootElement;

                        if (!root.TryGetProperty("DisplayName", out var nameProp) ||
                            !root.TryGetProperty("AppName", out var appNameProp)) continue;

                        var displayName = nameProp.GetString();
                        var appName = appNameProp.GetString();

                        // Filter UE stuff
                        if (appName != null && (string.IsNullOrEmpty(displayName) || appName.StartsWith("UE_", StringComparison.InvariantCulture) || IgnoredGameNames.Contains(displayName))) continue;

                        var installLoc = root.TryGetProperty("InstallLocation", out var il) ? il.GetString() : "";
                        var launchExe = root.TryGetProperty("LaunchExecutable", out var le) ? le.GetString() : "";

                        await CreateEpicShortcut(displayName, appName, installLoc, launchExe);
                    }
                    catch
                    {
                        /* Ignore */
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for Epic games.");
        }
    }

    private async Task CreateEpicShortcut(string displayName, string appName, string installLocation, string launchExecutable)
    {
        var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(displayName);
        var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

        var shortcutContent = $"[InternetShortcut]\nURL=com.epicgames.launcher://apps/{appName}?action=launch&silent=true";
        await File.WriteAllTextAsync(shortcutPath, shortcutContent);

        // Extract icon
        if (!string.IsNullOrEmpty(installLocation) && !string.IsNullOrEmpty(launchExecutable))
        {
            var fullExePath = Path.Combine(installLocation, launchExecutable);
            await ExtractIconFromGameFolder(installLocation, sanitizedGameName, fullExePath);
        }
    }

    #endregion

    #region GOG Galaxy

    private async Task ScanGogGamesAsync()
    {
        try
        {
            const string gogRegKey = @"SOFTWARE\WOW6432Node\GOG.com\Games";
            using var baseKey = Registry.LocalMachine.OpenSubKey(gogRegKey);
            if (baseKey == null) return;

            foreach (var gameId in baseKey.GetSubKeyNames())
            {
                try
                {
                    using var gameKey = baseKey.OpenSubKey(gameId);
                    if (gameKey == null) continue;

                    var gameName = gameKey.GetValue("gameName") as string;
                    var exePath = gameKey.GetValue("exe") as string;
                    var workingDir = gameKey.GetValue("workingDir") as string;

                    if (string.IsNullOrEmpty(gameName) || IgnoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                    var shortcutContent = $"[InternetShortcut]\nURL=goggalaxy://openGameView/{gameId}";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    if (!string.IsNullOrEmpty(exePath) && !string.IsNullOrEmpty(workingDir))
                    {
                        var fullExePath = Path.Combine(workingDir, exePath);
                        await ExtractIconFromGameFolder(workingDir, sanitizedGameName, fullExePath);
                    }
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for GOG games.");
        }
    }

    #endregion

    #region Ubisoft Connect

    private async Task ScanUbisoftGamesAsync()
    {
        try
        {
            const string ubiRegKey = @"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs";
            using var baseKey = Registry.LocalMachine.OpenSubKey(ubiRegKey);
            if (baseKey == null) return;

            foreach (var gameId in baseKey.GetSubKeyNames())
            {
                try
                {
                    using var gameKey = baseKey.OpenSubKey(gameId);
                    if (gameKey == null) continue;

                    var installDir = gameKey.GetValue("InstallDir") as string;
                    if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir)) continue;

                    var gameName = new DirectoryInfo(installDir).Name.Replace(" Edition", "").Trim();
                    if (IgnoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                    var shortcutContent = $"[InternetShortcut]\nURL=uplay://launch/{gameId}/0";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    await ExtractIconFromGameFolder(installDir, sanitizedGameName);
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for Ubisoft games.");
        }
    }

    #endregion

    #region EA App

    private async Task ScanEaGamesAsync()
    {
        try
        {
            const string eaRegKey = @"SOFTWARE\WOW6432Node\Electronic Arts\EA Core\Installed Games";
            using var baseKey = Registry.LocalMachine.OpenSubKey(eaRegKey);
            if (baseKey == null) return;

            foreach (var contentId in baseKey.GetSubKeyNames())
            {
                try
                {
                    using var gameKey = baseKey.OpenSubKey(contentId);
                    if (gameKey == null) continue;

                    var installDir = gameKey.GetValue("Install Dir") as string;
                    if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir)) continue;

                    var gameName = new DirectoryInfo(installDir).Name;
                    if (IgnoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                    var shortcutContent = $"[InternetShortcut]\nURL=origin2://game/launch?offerIds={contentId}";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    await ExtractIconFromGameFolder(installDir, sanitizedGameName);
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for EA games.");
        }
    }

    #endregion

    #region Microsoft Store

    private async Task ScanMicrosoftStoreGamesAsync()
    {
        try
        {
            // Sticking to PowerShell for portability in this context, but refining the filter.
            const string script = "Get-StartApps | ConvertTo-Json";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (string.IsNullOrWhiteSpace(output)) return;

            using var doc = JsonDocument.Parse(output);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return;

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                try
                {
                    var name = element.GetProperty("Name").GetString();
                    var appId = element.GetProperty("AppID").GetString();

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(appId)) continue;

                    // Filter: Must match whitelist OR not be a system app (heuristic)
                    // Since we can't easily detect "Game" type via PS Get-StartApps without more complex scripts,
                    // we rely on the whitelist for high-quality detection.
                    var isMatch = MicrosoftStoreKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));

                    if (!isMatch) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(name);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.bat");

                    // Use 'start shell:AppsFolder\...' for reliable launching
                    var batchContent = $"@echo off\r\nstart \"\" \"shell:AppsFolder\\{appId}\"";
                    await File.WriteAllTextAsync(shortcutPath, batchContent);

                    // Note: Extracting icons for UWP apps via simple file IO is difficult because
                    // assets are inside WindowsApps folder (restricted) or resources.pri.
                    // We skip icon extraction for UWP here.
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for Microsoft Store games.");
        }
    }

    #endregion

    #region Helpers

    private Task ExtractIconFromGameFolder(string gameInstallPath, string sanitizedGameName, string specificExePath = null)
    {
        var iconPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.png");
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

    #endregion
}
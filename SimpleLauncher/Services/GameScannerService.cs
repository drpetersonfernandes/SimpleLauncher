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

namespace SimpleLauncher.Services;

/// <summary>
/// Scans for games installed via Steam, Epic, GOG, Ubisoft, EA, and Microsoft Store,
/// integrating them into Simple Launcher.
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
        "Quixel Bridge",
        "DirectX",
        "Google Earth VR",
        "Spacewar"
    };

    // Whitelist for Microsoft Store games to avoid adding Calculator/Photos etc.
    private static readonly string[] MicrosoftStoreKeywords =
    {
        "Minecraft", "Solitaire", "Forza", "Halo", "Gears of War", "Sea of Thieves", "Flight Simulator", "Age of Empires"
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

            // Run scans in parallel where safe, or sequentially to avoid disk thrashing
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

    /// <summary>
    /// Ensures that a "Microsoft Windows" system is configured in system.xml.
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
                .Any(static el => el.Element("SystemName")?.Value.Equals(WindowsSystemName, StringComparison.OrdinalIgnoreCase) ?? false) ?? false;

            if (systemExists)
            {
                return false; // System already exists
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

            Directory.CreateDirectory(_windowsRomsPath);
            Directory.CreateDirectory(_windowsImagesPath);

            DebugLogger.Log($"[GameScannerService] Successfully created '{WindowsSystemName}' system.");

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
        var steamPath = "";
        var libraryPaths = new List<string>();
        try
        {
            // 1. Find Steam Path (Check both 64-bit and 32-bit keys, and HKCU)
            steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null) as string;
            DebugLogger.Log($"[GameScannerService] Steam installation path (1st check): {steamPath}");

            if (string.IsNullOrEmpty(steamPath))
            {
                steamPath = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string;
                DebugLogger.Log($"[GameScannerService] Steam installation path (2nd check): {steamPath}");
            }

            if (string.IsNullOrEmpty(steamPath))
            {
                steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
                DebugLogger.Log($"[GameScannerService] Steam installation path (3rd check): {steamPath}");
            }

            if (string.IsNullOrEmpty(steamPath))
            {
                steamPath = GetSteamPathFromProcess();
            }

            if (string.IsNullOrEmpty(steamPath) || !Directory.Exists(steamPath))
            {
                DebugLogger.Log("[GameScannerService] Steam installation not found.");
                return;
            }

            // 2. Identify Library Folders
            libraryPaths = new List<string> { Path.Combine(steamPath, "steamapps") };
            DebugLogger.Log($"[GameScannerService] Steam library path: {libraryPaths[0]}");

            var libraryFoldersVdf = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

            if (File.Exists(libraryFoldersVdf))
            {
                try
                {
                    var vdfData = SteamVdfParser.Parse(libraryFoldersVdf);

                    // The root should contain "libraryfolders" key
                    if (vdfData.TryGetValue("libraryfolders", out var libraryFoldersObj) &&
                        libraryFoldersObj is Dictionary<string, object> libraryFoldersDict)
                    {
                        // Each key in libraryfolders is a library index ("0", "1", etc.)
                        foreach (var libraryKvp in libraryFoldersDict)
                        {
                            if (libraryKvp.Value is Dictionary<string, object> libraryInfo &&
                                libraryInfo.TryGetValue("path", out var pathObj) &&
                                pathObj is string pathStr)
                            {
                                // Only add if it's not the main Steam directory (already added above)
                                if (!string.Equals(pathStr, steamPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    libraryPaths.Add(Path.Combine(pathStr, "steamapps"));
                                    DebugLogger.Log($"[GameScannerService] Found additional Steam library: {pathStr}");
                                }
                            }
                        }
                    }

                    // Fallback: Check if the root itself contains numeric keys with paths (some VDF parser results might flatten the structure)
                    // or if the structure is older.
                    foreach (var kvp in vdfData)
                    {
                        // Check if this KVP is actually a library definition (e.g. Key "1", Value Dict containing "path")
                        if (kvp.Value is Dictionary<string, object> potentialLib &&
                            potentialLib.TryGetValue("path", out var pathObj) &&
                            pathObj is string pathStr)
                        {
                            if (!string.Equals(pathStr, steamPath, StringComparison.OrdinalIgnoreCase))
                            {
                                libraryPaths.Add(Path.Combine(pathStr, "steamapps"));
                                DebugLogger.Log($"[GameScannerService] Found additional Steam library (root scan): {pathStr}");
                            }
                        }
                    }

                    if (libraryPaths.Count == 1) // Only default path found so far
                    {
                        // Fallback: try to parse as flat structure (older Steam versions)
                        foreach (var kvp in vdfData)
                        {
                            if (kvp.Value is Dictionary<string, object> folderInfo &&
                                folderInfo.TryGetValue("path", out var pathObj) &&
                                pathObj is string pathStr)
                            {
                                if (!string.Equals(pathStr, steamPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    libraryPaths.Add(Path.Combine(pathStr, "steamapps"));
                                    DebugLogger.Log($"[GameScannerService] Found additional Steam library (fallback): {pathStr}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await _logErrors.LogErrorAsync(ex, "Failed to parse Steam's libraryfolders.vdf. Using default path only.");
                    DebugLogger.Log($"[GameScannerService] Error parsing Steam's libraryfolders.vdf: {ex.Message}");
                }
            }

            // 3. Scan Manifests
            foreach (var libraryPath in libraryPaths.Distinct())
            {
                if (!Directory.Exists(libraryPath)) continue;

                var manifestFiles = Directory.GetFiles(libraryPath, "appmanifest_*.acf");
                DebugLogger.Log($"[GameScannerService] Found {manifestFiles.Length} Steam manifests in {libraryPath}");

                foreach (var manifestFile in manifestFiles)
                {
                    try
                    {
                        var appData = SteamVdfParser.Parse(manifestFile);
                        if (appData.TryGetValue("AppState", out var appState) && appState is Dictionary<string, object> appStateDict)
                        {
                            // Ensure we have the required fields: name, appid, and installdir for the fallback
                            if (appStateDict.TryGetValue("name", out var nameObj) && nameObj is string gameName &&
                                appStateDict.TryGetValue("appid", out var appIdObj) && appIdObj is string appId &&
                                appStateDict.TryGetValue("installdir", out var installDirObj) && installDirObj is string installDir)
                            {
                                if (IgnoredGameNames.Contains(gameName)) continue;

                                var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                                var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                                // Construct the full path to the game's installation directory for the icon fallback
                                // libraryPath is like "D:\SteamLibrary\steamapps", so we go up one level to get to the library root.
                                var libraryRoot = Path.GetDirectoryName(libraryPath);
                                if (libraryRoot != null)
                                {
                                    var gameInstallPath = Path.Combine(libraryRoot, "common", installDir);

                                    // Create .url shortcut
                                    var shortcutContent = $"[InternetShortcut]\nURL=steam://run/{appId}";
                                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                                    // Copy artwork (Try multiple variations)
                                    await TryCopySteamArtworkAsync(steamPath, appId, sanitizedGameName, gameInstallPath);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log but continue
                        DebugLogger.Log($"[GameScannerService] Error processing Steam manifest {manifestFile}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, $"An error occurred while scanning for Steam games. SteamPath: {steamPath}, LibraryPaths: {string.Join(", ", libraryPaths)}");
            DebugLogger.Log($"[GameScannerService] Steam scan failed: {ex.Message}\nStack: {ex.StackTrace}");
        }
    }

    private static string GetSteamPathFromProcess()
    {
        try
        {
            var steamProcess = Process.GetProcessesByName("steam").FirstOrDefault();
            if (steamProcess != null)
            {
                var path = steamProcess.MainModule?.FileName;
                if (!string.IsNullOrEmpty(path))
                {
                    return Path.GetDirectoryName(path);
                }
            }
        }
        catch
        {
            /* Ignore errors */
        }

        return null;
    }

    private Task TryCopySteamArtworkAsync(string steamPath, string appId, string sanitizedGameName, string gameInstallPath)
    {
        var destArtworkPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.jpg");
        if (File.Exists(destArtworkPath)) return Task.CompletedTask;

        var cachePath = Path.Combine(steamPath, "appcache", "librarycache");
        if (!Directory.Exists(cachePath)) return Task.CompletedTask;

        // Priority list of images to look for
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
                    /* Ignore copy errors */
                }
            }
        }

        // --- FALLBACK LOGIC ---
        // If we are here, it means no cached artwork was found. Try to get the icon from the exe.
        DebugLogger.Log($"[GameScannerService] Steam cache art not found for '{sanitizedGameName}' (AppID: {appId}). Attempting fallback to exe icon.");

        if (!Directory.Exists(gameInstallPath))
        {
            DebugLogger.Log($"[GameScannerService] Fallback failed: Install directory does not exist at '{gameInstallPath}'.");
            return Task.CompletedTask;
        }

        // Try to find a suitable executable. Heuristics:
        // 1. Exe name contains the sanitized game name (e.g. "GameName.exe" inside "GameName").
        // 2. Sanitized game name contains the exe name (e.g. "Pizzeria Simulator.exe" inside "Freddy Fazbear's Pizzeria Simulator").
        // 3. The largest executable file (avoids small launchers, crash handlers, or config tools).
        // 4. Any exe that isn't an uninstaller/setup utility.

        var exeFiles = Directory.GetFiles(gameInstallPath, "*.exe", SearchOption.TopDirectoryOnly);

        var selectionMethod = "None";

        // Heuristic 1: Exe contains Game Name
        var mainExe = exeFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Contains(sanitizedGameName, StringComparison.OrdinalIgnoreCase));

        // Heuristic 2: Game Name contains Exe Name (useful for long titles with short exes)
        if (mainExe == null)
        {
            mainExe = exeFiles.FirstOrDefault(f =>
            {
                var name = Path.GetFileNameWithoutExtension(f);
                return name.Length > 3 && sanitizedGameName.Contains(name, StringComparison.OrdinalIgnoreCase);
            });
            if (mainExe != null)
            {
                selectionMethod = "GameNameContainsExe";
            }
        }
        else
        {
            selectionMethod = "ExeContainsGameName";
        }

        // Heuristic 3: Largest EXE (Fallback)
        if (mainExe == null && exeFiles.Length > 0)
        {
            // Filter out obvious junk first
            var candidates = exeFiles
                .Where(f => !f.Contains("unins", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains("setup", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains("crash", StringComparison.OrdinalIgnoreCase) &&
                            !f.Contains("unity", StringComparison.OrdinalIgnoreCase)) // UnityCrashHandler
                .ToList();

            if (candidates.Count != 0)
            {
                mainExe = candidates.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                selectionMethod = "LargestFile";
            }
            else
            {
                // Absolute fallback
                mainExe = exeFiles.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                selectionMethod = "LargestFile_NoFilter";
            }
        }

        if (mainExe != null)
        {
            var iconPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.png");
            if (!File.Exists(iconPath))
            {
                DebugLogger.Log($"[GameScannerService] Fallback ({selectionMethod}): Selected '{Path.GetFileName(mainExe)}' for '{sanitizedGameName}'. Extracting icon...");
                IconExtractor.SaveIconFromExe(mainExe, iconPath);
            }
        }
        else
        {
            DebugLogger.Log($"[GameScannerService] Fallback failed: No suitable executable found in '{gameInstallPath}'.");
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Epic Games

    private async Task ScanEpicGamesAsync()
    {
        try
        {
            const string manifestsPath = @"C:\ProgramData\Epic\EpicGamesLauncher\Data\Manifests";
            if (!Directory.Exists(manifestsPath)) return;

            var manifestFiles = Directory.GetFiles(manifestsPath, "*.item");
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

                    if (string.IsNullOrEmpty(displayName) || IgnoredGameNames.Contains(displayName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(displayName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                    var shortcutContent = $"[InternetShortcut]\nURL=com.epicgames.launcher://apps/{appName}?action=launch&silent=true";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    // Extract icon
                    if (root.TryGetProperty("InstallLocation", out var installLocProp) &&
                        root.TryGetProperty("LaunchExecutable", out var exeProp))
                    {
                        var exePath = Path.Combine(installLocProp.GetString() ?? "", exeProp.GetString() ?? "");
                        if (File.Exists(exePath))
                        {
                            var iconPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.png");
                            if (!File.Exists(iconPath))
                                IconExtractor.SaveIconFromExe(exePath, iconPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[GameScannerService] Error processing Epic manifest {manifestFile}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for Epic games.");
        }
    }

    #endregion

    #region GOG Galaxy

    private async Task ScanGogGamesAsync()
    {
        try
        {
            // GOG usually stores game info in HKLM\SOFTWARE\WOW6432Node\GOG.com\Games
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

                    if (string.IsNullOrEmpty(gameName)) continue;
                    if (IgnoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                    // Create shortcut using Galaxy protocol
                    var shortcutContent = $"[InternetShortcut]\nURL=goggalaxy://openGameView/{gameId}";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    // Extract icon from the actual executable if available
                    if (!string.IsNullOrEmpty(exePath) && !string.IsNullOrEmpty(workingDir))
                    {
                        var fullExePath = Path.Combine(workingDir, exePath);
                        if (File.Exists(fullExePath))
                        {
                            var iconPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.png");
                            if (!File.Exists(iconPath))
                                IconExtractor.SaveIconFromExe(fullExePath, iconPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[GameScannerService] Error processing GOG game {gameId}: {ex.Message}");
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

                    // Ubisoft registry doesn't always have the "Name". We might need to infer it from the folder name.
                    // Or check "GameExplorer" entry if it exists.
                    var gameName = new DirectoryInfo(installDir).Name;

                    // Clean up common folder suffixes
                    gameName = gameName.Replace(" Edition", "").Trim();

                    if (IgnoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                    // Create shortcut using Uplay protocol
                    var shortcutContent = $"[InternetShortcut]\nURL=uplay://launch/{gameId}/0";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    // Try to find an executable for the icon
                    var exeFiles = Directory.GetFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly);
                    var mainExe = exeFiles.FirstOrDefault(f => f.Contains(gameName, StringComparison.OrdinalIgnoreCase)) ?? exeFiles.FirstOrDefault();

                    if (mainExe != null)
                    {
                        var iconPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.png");
                        if (!File.Exists(iconPath))
                            IconExtractor.SaveIconFromExe(mainExe, iconPath);
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[GameScannerService] Error processing Ubisoft game {gameId}: {ex.Message}");
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

                    // EA usually has "Install Dir"
                    var installDir = gameKey.GetValue("Install Dir") as string;
                    if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir)) continue;

                    // Try to get name from directory or registry if available
                    var gameName = new DirectoryInfo(installDir).Name;

                    if (IgnoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.url");

                    // EA App protocol
                    var shortcutContent = $"[InternetShortcut]\nURL=origin2://game/launch?offerIds={contentId}";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    // Icon
                    var exeFiles = Directory.GetFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly);
                    var mainExe = exeFiles.FirstOrDefault(static f => !f.Contains("Cleanup", StringComparison.OrdinalIgnoreCase) && !f.Contains("Touchup", StringComparison.OrdinalIgnoreCase));

                    if (mainExe != null)
                    {
                        var iconPath = Path.Combine(_windowsImagesPath, $"{sanitizedGameName}.png");
                        if (!File.Exists(iconPath))
                            IconExtractor.SaveIconFromExe(mainExe, iconPath);
                    }
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[GameScannerService] Error processing EA game {contentId}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for EA games.");
        }
    }

    #endregion

    #region Microsoft Store / Default Games

    private async Task ScanMicrosoftStoreGamesAsync()
    {
        try
        {
            // Using PowerShell to get AppxPackages is the cleanest way without heavy API dependencies.
            // We filter by a whitelist to avoid adding "Calculator", "Photos", etc.

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

                    // Check against whitelist
                    var isMatch = MicrosoftStoreKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));

                    if (!isMatch) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(name);
                    // Create a .bat file instead of a .url file for reliability with shell:AppsFolder
                    var shortcutPath = Path.Combine(_windowsRomsPath, $"{sanitizedGameName}.bat");

                    // Create a batch file that uses the 'start' command. This is the most reliable
                    // way to launch UWP apps via their shell protocol from an external application,
                    // avoiding the working directory issues seen with Process.Start on .url files.
                    var batchContent = $"@echo off\r\nstart \"\" \"shell:AppsFolder\\{appId}\"";
                    await File.WriteAllTextAsync(shortcutPath, batchContent);

                    // Note: Extracting icons from UWP apps programmatically is challenging without UWP APIs.
                    // We rely on the user or a scraper to fill in the image later, or use a default.
                }
                catch
                {
                    // Ignore individual parsing errors
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred while scanning for Microsoft Store games.");
        }
    }

    #endregion
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.SanitizeInputString;

namespace SimpleLauncher.Services.GameScan;

internal static class ScanSteamGames
{
    internal static async Task ScanSteamGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
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
                    var vdfData = SteamVdfParser.Parse(libraryFoldersVdf, logErrors);

                    // Handle new VDF format (numeric keys at root or inside "libraryfolders")
                    var rootNode = vdfData.TryGetValue("libraryfolders", out var value)
                        ? value as Dictionary<string, object>
                        : vdfData;

                    if (rootNode != null)
                    {
                        foreach (var kvp in rootNode)
                        {
                            switch (kvp.Value)
                            {
                                // Modern format: "0" { "path" "C:\\Games" ... }
                                case Dictionary<string, object> libData when
                                    libData.TryGetValue("path", out var pathObj) &&
                                    pathObj is string pathStr:
                                {
                                    if (!string.Equals(pathStr, steamPath, StringComparison.OrdinalIgnoreCase))
                                    {
                                        libraryPaths.Add(Path.Combine(pathStr, "steamapps"));
                                    }

                                    break;
                                }
                                // Legacy format: "1" "C:\\Games"
                                case string legacyPath:
                                {
                                    if (!string.Equals(legacyPath, steamPath, StringComparison.OrdinalIgnoreCase))
                                    {
                                        libraryPaths.Add(Path.Combine(legacyPath, "steamapps"));
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, "Error parsing libraryfolders.vdf");
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
                    await ProcessSteamManifest(manifestFile, libraryPath, steamPath, logErrors, windowsRomsPath, windowsImagesPath, ignoredGameNames);
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
                    // Pass windowsImagesPath here
                    await ProcessSourceMod(modDir, windowsRomsPath, windowsImagesPath, logErrors);
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Steam games.");
        }
    }

    private static async Task ProcessSteamManifest(string manifestFile, string libraryPath, string steamPath, ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
    {
        try
        {
            var appData = SteamVdfParser.Parse(manifestFile, logErrors);
            if (appData.TryGetValue("AppState", out var appState) && appState is Dictionary<string, object> appStateDict)
            {
                if (appStateDict.TryGetValue("name", out var nameObj) && nameObj is string gameName &&
                    appStateDict.TryGetValue("appid", out var appIdObj) && appIdObj is string appId &&
                    appStateDict.TryGetValue("installdir", out var installDirObj) && installDirObj is string installDir)
                {
                    if (ignoredGameNames.Contains(gameName)) return;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");
                    var gameInstallPath = Path.Combine(libraryPath, "common", installDir);

                    var shortcutContent = $"[InternetShortcut]\nURL=steam://run/{appId}";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    await TryCopySteamArtworkAsync(logErrors, steamPath, appId, gameName, sanitizedGameName, gameInstallPath, windowsImagesPath);
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Error processing Steam manifest: {manifestFile}");
        }
    }

    private static async Task ProcessSourceMod(string modDir, string windowsRomsPath, string windowsImagesPath, ILogErrors logErrors)
    {
        try
        {
            var gameInfoPath = Path.Combine(modDir, "gameinfo.txt");
            if (!File.Exists(gameInfoPath)) return;

            // 1. Parse gameinfo.txt using the existing VDF parser
            var vdfData = SteamVdfParser.Parse(gameInfoPath, logErrors);

            string gameName = null;
            string baseAppId = null;

            // Source mods store info under a "GameInfo" root key
            if (vdfData.TryGetValue("GameInfo", out var gi) && gi is Dictionary<string, object> gameInfo)
            {
                // Get the Display Name
                if (gameInfo.TryGetValue("game", out var nameObj))
                {
                    gameName = nameObj.ToString();
                }

                // Get the Base AppID (e.g., 243730 for Source SDK 2013)
                if (gameInfo.TryGetValue("FileSystem", out var fs) && fs is Dictionary<string, object> fileSystem)
                {
                    if (fileSystem.TryGetValue("SteamAppId", out var appIdObj))
                    {
                        baseAppId = appIdObj.ToString();
                    }
                }
            }

            // Fallback for name if not found in VDF
            if (string.IsNullOrEmpty(gameName))
            {
                gameName = new DirectoryInfo(modDir).Name;
            }

            if (string.IsNullOrEmpty(baseAppId))
            {
                DebugLogger.Log($"[GameScannerService] Could not resolve Base AppID for mod: {gameName}. Skipping.");
                return;
            }

            var modFolderName = new DirectoryInfo(modDir).Name;
            var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
            var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

            // 2. Create the Shortcut
            // The protocol for mods is: steam://run/<BaseAppID>//-game <ModFolderName>
            var shortcutContent = $"[InternetShortcut]\nURL=steam://run/{baseAppId}//-game {modFolderName}";
            await File.WriteAllTextAsync(shortcutPath, shortcutContent);

            // 3. Handle Icon/Image
            var destArtworkPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
            if (!File.Exists(destArtworkPath))
            {
                // Source mods usually have a game.ico in the root folder
                var modIcon = Path.Combine(modDir, "game.ico");
                if (File.Exists(modIcon))
                {
                    try
                    {
                        using var icon = new Icon(modIcon, 256, 256);
                        using var bmp = icon.ToBitmap();
                        bmp.Save(destArtworkPath, ImageFormat.Png);
                    }
                    catch
                    {
                        /* Fallback to generic scan */
                    }
                }

                if (!File.Exists(destArtworkPath))
                {
                    await GameScannerService.FindAndSaveGameImageAsync(logErrors, gameName, modDir, sanitizedGameName, windowsImagesPath);
                }
            }

            DebugLogger.Log($"[GameScannerService] Created shortcut for Source Mod: {gameName}");
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Error processing Source Mod in {modDir}");
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

    private static async Task TryCopySteamArtworkAsync(ILogErrors logErrors, string steamPath, string appId, string gameName, string sanitizedGameName, string gameInstallPath, string windowsImagesPath)
    {
        var destArtworkPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
        if (File.Exists(destArtworkPath)) return;

        // 1. Try API first
        if (await GameScannerService.TryDownloadImageFromApiAsync(gameName, destArtworkPath, logErrors))
        {
            return;
        }

        // 2. Try Steam's local artwork cache
        var cachePath = Path.Combine(steamPath, "appcache", "librarycache");
        if (Directory.Exists(cachePath))
        {
            string[] searchPatterns =
            [
                $"{appId}_library_600x900.jpg",
                $"{appId}_header.jpg",
                $"{appId}_library_hero.jpg"
            ];

            foreach (var pattern in searchPatterns)
            {
                var sourcePath = Path.Combine(cachePath, pattern);
                if (File.Exists(sourcePath))
                {
                    try
                    {
                        // Convert JPG to PNG
                        using var image = Image.FromFile(sourcePath);
                        image.Save(destArtworkPath, ImageFormat.Png);
                        return; // Successfully converted and saved
                    }
                    catch (Exception ex)
                    {
                        await logErrors.LogErrorAsync(ex, $"Error converting Steam artwork from JPG to PNG for {sanitizedGameName} (Source: {sourcePath})");
                    }
                }
            }
        }

        // 3. Fallback to EXE icon if no artwork was found
        await GameScannerService.ExtractIconFromGameFolder(logErrors, gameInstallPath, sanitizedGameName, windowsImagesPath);
    }
}
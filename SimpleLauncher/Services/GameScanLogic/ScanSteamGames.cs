using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanSteamGames
{
    public static async Task ScanSteamGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
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
                            // Modern format: "0" { "path" "C:\\Games" ... }
                            if (kvp.Value is Dictionary<string, object> libData &&
                                libData.TryGetValue("path", out var pathObj) &&
                                pathObj is string pathStr)
                            {
                                if (!string.Equals(pathStr, steamPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    libraryPaths.Add(Path.Combine(pathStr, "steamapps"));
                                }
                            }
                            // Legacy format: "1" "C:\\Games"
                            else if (kvp.Value is string legacyPath)
                            {
                                if (!string.Equals(legacyPath, steamPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    libraryPaths.Add(Path.Combine(legacyPath, "steamapps"));
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
                    await ProcessSourceMod(modDir, windowsRomsPath, logErrors);
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

                    await TryCopySteamArtworkAsync(logErrors, steamPath, appId, sanitizedGameName, gameInstallPath, windowsImagesPath);
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Error processing Steam manifest: {manifestFile}");
        }
    }

    private static async Task ProcessSourceMod(string modDir, string windowsRomsPath, ILogErrors logErrors)
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
            Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

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

    private static async Task TryCopySteamArtworkAsync(ILogErrors logErrors, string steamPath, string appId, string sanitizedGameName, string gameInstallPath, string windowsImagesPath)
    {
        var destArtworkPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
        if (File.Exists(destArtworkPath)) return;

        var cachePath = Path.Combine(steamPath, "appcache", "librarycache");
        if (!Directory.Exists(cachePath)) return;

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
                    // Convert JPG to PNG
                    using var image = Image.FromFile(sourcePath);
                    image.Save(destArtworkPath, ImageFormat.Png);

                    return; // Successfully converted and saved
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, $"Error converting Steam artwork from JPG to PNG for {sanitizedGameName} (Source: {sourcePath})");
                    // Continue to the next pattern or fallback if conversion fails
                }
            }
        }

        // Fallback to EXE icon if no artwork was found or successfully converted
        await GameScannerService.ExtractIconFromGameFolder(logErrors, gameInstallPath, sanitizedGameName, windowsImagesPath);
    }
}
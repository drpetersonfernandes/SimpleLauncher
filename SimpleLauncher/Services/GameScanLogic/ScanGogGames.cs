using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanGogGames
{
    public static async Task ScanGogGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
    {
        try
        {
            // Playnite Logic: Scan Uninstall keys for Publisher "GOG.com"
            // This covers both legacy installers and Galaxy installs.
            var uninstallKeys = new[]
            {
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var keyPath in uninstallKeys)
            {
                using var baseKey = Registry.LocalMachine.OpenSubKey(keyPath);
                if (baseKey == null) continue;

                foreach (var subKeyName in baseKey.GetSubKeyNames())
                {
                    try
                    {
                        using var subKey = baseKey.OpenSubKey(subKeyName);
                        if (subKey == null) continue;

                        var publisher = subKey.GetValue("Publisher") as string;
                        if (publisher != "GOG.com") continue;

                        var gameId = subKeyName.Replace("_is1", ""); // GOG keys are usually "123456_is1"
                        if (!long.TryParse(gameId, out _)) continue;

                        var installLocation = subKey.GetValue("InstallLocation") as string;
                        var displayName = subKey.GetValue("DisplayName") as string;

                        if (string.IsNullOrEmpty(installLocation) || !Directory.Exists(installLocation)) continue;
                        if (string.IsNullOrEmpty(displayName)) continue;

                        // Cleanup Name (remove trademarks etc if needed, Playnite does this)
                        displayName = displayName.Replace("™", "").Replace("®", "").Trim();

                        if (ignoredGameNames.Contains(displayName)) continue;

                        var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(displayName);
                        var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                        // Create Shortcut (Launch via Galaxy)
                        var shortcutContent = $"[InternetShortcut]\nURL=goggalaxy://openGameView/{gameId}";
                        await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                        // Extract Icon
                        // Playnite Logic: Read goggame-{id}.info to find the primary executable
                        string mainExePath = null;
                        var infoFile = Path.Combine(installLocation, $"goggame-{gameId}.info");

                        if (File.Exists(infoFile))
                        {
                            try
                            {
                                var json = await File.ReadAllTextAsync(infoFile);
                                var gameInfo = JsonSerializer.Deserialize<GogGameInfo>(json);

                                var primaryTask = gameInfo?.PlayTasks?.FirstOrDefault(t => t.IsPrimary && t.Type == "FileTask");
                                if (primaryTask != null && !string.IsNullOrEmpty(primaryTask.Path))
                                {
                                    mainExePath = Path.Combine(installLocation, primaryTask.Path);
                                }
                            }
                            catch
                            {
                                // Fallback to heuristics if JSON parsing fails
                            }
                        }

                        // Fallback: Look for the executable defined in registry if .info failed
                        if (string.IsNullOrEmpty(mainExePath))
                        {
                            _ = subKey.GetValue("QuietUninstallString") as string;
                            // Sometimes the uninstaller is in the same folder, not useful for icon,
                            // but we fallback to the heuristic scanner in ExtractIconFromGameFolder
                        }

                        await GameScannerService.ExtractIconFromGameFolder(installLocation, sanitizedGameName, windowsImagesPath, mainExePath);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for GOG games.");
        }
    }
}

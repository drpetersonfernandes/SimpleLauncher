using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models.GameScanLogic;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanGogGames
{
    public static async Task ScanGogGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
    {
        try
        {
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
                        // GOG entries usually have "GOG.com" as publisher
                        if (publisher != "GOG.com") continue;

                        var gameId = subKeyName.Replace("_is1", "");
                        if (!long.TryParse(gameId, out _)) continue;

                        var installLocation = subKey.GetValue("InstallLocation") as string;
                        var displayName = subKey.GetValue("DisplayName") as string;

                        if (string.IsNullOrEmpty(installLocation) || !Directory.Exists(installLocation)) continue;
                        if (string.IsNullOrEmpty(displayName)) continue;

                        displayName = displayName.Replace("™", "").Replace("®", "").Trim();

                        if (ignoredGameNames.Contains(displayName)) continue;

                        // --- FIX: Check for DLC via goggame-*.info ---
                        string mainExePath = null;
                        var infoFile = Path.Combine(installLocation, $"goggame-{gameId}.info");
                        var isDlc = false;

                        if (File.Exists(infoFile))
                        {
                            try
                            {
                                var json = await File.ReadAllTextAsync(infoFile);
                                var gameInfo = JsonSerializer.Deserialize<GogGameInfo>(json);

                                // If RootGameId exists and is different from GameId, this is a DLC
                                if (gameInfo != null && !string.IsNullOrEmpty(gameInfo.RootGameId) && gameInfo.RootGameId != gameInfo.GameId)
                                {
                                    isDlc = true;
                                }

                                if (!isDlc)
                                {
                                    var primaryTask = gameInfo?.PlayTasks?.FirstOrDefault(t => t.IsPrimary && t.Type == "FileTask");
                                    if (primaryTask != null && !string.IsNullOrEmpty(primaryTask.Path))
                                    {
                                        mainExePath = Path.Combine(installLocation, primaryTask.Path);
                                    }
                                }
                            }
                            catch
                            {
                                // Fallback to heuristics if JSON parsing fails
                            }
                        }

                        if (isDlc) continue;
                        // ---------------------------------------------

                        var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(displayName);
                        var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                        var shortcutContent = $"[InternetShortcut]\nURL=goggalaxy://launch/{gameId}";
                        await File.WriteAllTextAsync(shortcutPath, shortcutContent);

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

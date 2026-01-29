using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.SanitizeInputString;

namespace SimpleLauncher.Services.GameScan;

public class ScanHumbleGames
{
    public static async Task ScanHumbleGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
    {
        try
        {
            var configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Humble App", "config.json");
            if (!File.Exists(configPath)) return;

            var jsonContent = await File.ReadAllTextAsync(configPath);
            using var doc = JsonDocument.Parse(jsonContent);

            if (doc.RootElement.TryGetProperty("game-collection-4", out var collection))
            {
                foreach (var game in collection.EnumerateArray())
                {
                    try
                    {
                        var status = game.GetProperty("status").GetString();
                        if (status != "installed" && status != "downloaded") continue;

                        if (!game.TryGetProperty("machineName", out var machineNameProp)) continue;

                        var machineName = machineNameProp.GetString();
                        var gameName = game.GetProperty("gameName").GetString();

                        if (string.IsNullOrEmpty(gameName) || ignoredGameNames.Contains(gameName)) continue;

                        // Determine install path
                        string installDir = null;
                        string exePath = null;

                        // Try 'filePath' first
                        if (game.TryGetProperty("filePath", out var fp) && !string.IsNullOrEmpty(fp.GetString()))
                        {
                            installDir = fp.GetString();
                        }
                        // Fallback to downloadFilePath + machineName
                        else if (game.TryGetProperty("downloadFilePath", out var dfp) && !string.IsNullOrEmpty(dfp.GetString()))
                        {
                            var downloadPath = dfp.GetString();
                            if (!string.IsNullOrEmpty(downloadPath) && !string.IsNullOrEmpty(machineName))
                            {
                                installDir = Path.Combine(downloadPath, machineName);
                            }
                        }

                        if (game.TryGetProperty("executablePath", out var ep))
                        {
                            exePath = ep.GetString();
                        }

                        if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir)) continue;

                        var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                        var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                        // Humble Protocol
                        var shortcutContent = $"[InternetShortcut]\nURL=humble://launch/{machineName}";
                        await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                        string fullExePath = null;
                        if (!string.IsNullOrEmpty(installDir) && !string.IsNullOrEmpty(exePath))
                        {
                            fullExePath = Path.Combine(installDir, exePath);
                        }

                        await GameScannerService.FindAndSaveGameImageAsync(logErrors, gameName, installDir, sanitizedGameName, windowsImagesPath, fullExePath);
                    }
                    catch (Exception ex)
                    {
                        await logErrors.LogErrorAsync(ex, "Error processing Humble game entry.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Humble games.");
        }
    }
}
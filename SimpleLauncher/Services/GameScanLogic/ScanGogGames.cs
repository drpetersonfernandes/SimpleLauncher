using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanGogGames
{
    public static async Task ScanGogGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
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

                    if (string.IsNullOrEmpty(gameName) || ignoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                    var shortcutContent = $"[InternetShortcut]\nURL=goggalaxy://openGameView/{gameId}";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    if (!string.IsNullOrEmpty(exePath) && !string.IsNullOrEmpty(workingDir))
                    {
                        var fullExePath = Path.Combine(workingDir, exePath);
                        await GameScannerService.ExtractIconFromGameFolder(workingDir, sanitizedGameName, windowsImagesPath, fullExePath);
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
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for GOG games.");
        }
    }
}
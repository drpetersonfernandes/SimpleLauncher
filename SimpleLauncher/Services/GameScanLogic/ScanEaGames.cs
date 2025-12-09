using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanEaGames
{
    public static async Task ScanEaGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
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
                    if (ignoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                    var shortcutContent = $"[InternetShortcut]\nURL=origin2://game/launch?offerIds={contentId}";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    await GameScannerService.ExtractIconFromGameFolder(logErrors, installDir, sanitizedGameName, windowsImagesPath);
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, $"Error processing EA game: {contentId}");
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for EA games.");
        }
    }
}
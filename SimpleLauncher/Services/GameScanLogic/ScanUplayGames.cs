using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanUplayGames
{
    public static async Task ScanUplayGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
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
                    if (ignoredGameNames.Contains(gameName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                    var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                    var shortcutContent = $"[InternetShortcut]\nURL=uplay://launch/{gameId}/0";
                    await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                    await GameScannerService.ExtractIconFromGameFolder(installDir, sanitizedGameName, windowsImagesPath);
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Ubisoft games.");
        }
    }
}
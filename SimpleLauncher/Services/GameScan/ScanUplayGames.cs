using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Utils;

namespace SimpleLauncher.Services.GameScan;

public class ScanUplayGames
{
    public static async Task ScanUplayGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
    {
        try
        {
            // Most apps checks both registry views
            var registryViews = new[] { RegistryView.Registry32, RegistryView.Registry64 };
            const string ubiRegKey = @"SOFTWARE\Ubisoft\Launcher\Installs";

            foreach (var view in registryViews)
            {
                using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view).OpenSubKey(ubiRegKey);
                if (baseKey == null) continue;

                foreach (var gameId in baseKey.GetSubKeyNames())
                {
                    try
                    {
                        using var gameKey = baseKey.OpenSubKey(gameId);
                        if (gameKey == null) continue;

                        var installDir = gameKey.GetValue("InstallDir") as string;
                        if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir)) continue;

                        var gameExe = gameKey.GetValue("ExecPath") as string;
                        // Clean up path separators
                        installDir = installDir.Replace('/', Path.DirectorySeparatorChar);

                        var gameName = new DirectoryInfo(installDir).Name.Replace(" Edition", "").Trim();
                        if (ignoredGameNames.Contains(gameName)) continue;

                        var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameName);
                        var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                        var shortcutContent = $"[InternetShortcut]\nURL=uplay://launch/{gameId}";
                        await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                        string fullExePath = null;
                        if (!string.IsNullOrEmpty(gameExe) && File.Exists(gameExe))
                        {
                            fullExePath = gameExe;
                        }

                        await GameScannerService.FindAndSaveGameImageAsync(logErrors, gameName, installDir, sanitizedGameName, windowsImagesPath, fullExePath);
                    }
                    catch (Exception ex)
                    {
                        await logErrors.LogErrorAsync(ex, $"Error processing Ubisoft game registry key: {gameId}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Ubisoft games.");
        }
    }
}
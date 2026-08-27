using Microsoft.Win32;
using SimpleLauncher.Core.Services.SanitizeInputString;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScan;

/// <summary>
/// Scans for installed EA (Electronic Arts) games and creates shortcuts for them.
/// </summary>
public class ScanEaGames : IGamePlatformScanner
{
    /// <inheritdoc />
    public async Task ScanAsync(GameScannerService gameScannerService, ILogger logErrors, string windowsRomsPath,
        string windowsImagesPath, ISet<string> ignoredGameNames)
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

                    await gameScannerService.FindAndSaveGameImageAsync(logErrors, gameName, installDir,
                        sanitizedGameName, windowsImagesPath);
                }
                catch (Exception ex)
                {
                    logErrors.Error(ex, $"Error processing EA game: {contentId}");
                }
            }
        }
        catch (Exception ex)
        {
            logErrors.Error(ex, "An error occurred while scanning for EA games.");
        }
    }
}
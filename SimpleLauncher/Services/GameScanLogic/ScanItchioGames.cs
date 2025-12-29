using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanItchioGames
{
    private static readonly char[] Separator = new[] { '=' };

    public static async Task ScanItchioGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var itchUserData = Path.Combine(appData, "itch");
            var defaultLibraryPath = Path.Combine(itchUserData, "apps");

            if (!Directory.Exists(defaultLibraryPath)) return;

            var gameDirs = Directory.GetDirectories(defaultLibraryPath);

            foreach (var gameDir in gameDirs)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(gameDir);
                    var gameName = dirInfo.Name;
                    var manifestPath = Path.Combine(gameDir, ".itch.toml");
                    string prettyName = null;
                    string launchExe = null;

                    if (File.Exists(manifestPath))
                    {
                        try
                        {
                            var lines = await File.ReadAllLinesAsync(manifestPath);
                            var inActions = false;

                            // Simple state machine to find the first 'path' inside [[actions]]
                            foreach (var line in lines)
                            {
                                var trimmed = line.Trim();
                                if (trimmed.StartsWith("[[actions]]", StringComparison.Ordinal))
                                {
                                    inActions = true;
                                    continue;
                                }

                                // If we hit another section, stop if we were in actions
                                if (trimmed.StartsWith('[') && inActions)
                                {
                                    // If we found an exe, we are done. If not, maybe next action?
                                    // For simplicity, we take the first action found.
                                    if (launchExe != null) break;

                                    inActions = false;
                                }

                                if (inActions && trimmed.StartsWith("path", StringComparison.Ordinal))
                                {
                                    // Handle: path = "bin/game.exe" or path="game.exe"
                                    var parts = trimmed.Split(Separator, 2);
                                    if (parts.Length > 1)
                                    {
                                        var val = parts[1].Trim().Trim('"', '\'');
                                        launchExe = Path.Combine(gameDir, val.Replace("/", "\\"));
                                        // We found the executable for the first action, stop parsing
                                        break;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            /* ignore parse error */
                        }
                    }

                    // Attempt to get a prettier name from the executable if we found one, or directory name
                    if (!string.IsNullOrEmpty(launchExe) && File.Exists(launchExe))
                    {
                        var versionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(launchExe);
                        if (!string.IsNullOrEmpty(versionInfo.ProductName))
                        {
                            prettyName = versionInfo.ProductName;
                        }
                    }

                    if (string.IsNullOrEmpty(prettyName))
                    {
                        // Capitalize slug
                        prettyName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(gameName.Replace("-", " "));
                    }

                    if (ignoredGameNames.Contains(prettyName)) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(prettyName);

                    // Create .bat launch file
                    if (!string.IsNullOrEmpty(launchExe) && File.Exists(launchExe))
                    {
                        var batPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.bat");
                        var batContent = $"@echo off\r\ncd /d \"{Path.GetDirectoryName(launchExe)}\"\r\nstart \"\" \"{Path.GetFileName(launchExe)}\"";
                        await File.WriteAllTextAsync(batPath, batContent);

                        await GameScannerService.FindAndSaveGameImageAsync(logErrors, prettyName, gameDir, sanitizedGameName, windowsImagesPath, launchExe);
                    }
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, $"Error processing Itch.io game directory: {gameDir}");
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Itch.io games.");
        }
    }
}
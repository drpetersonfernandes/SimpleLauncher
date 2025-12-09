using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanItchioGames
{
    public static async Task ScanItchioGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
    {
        try
        {
            // Playnite Logic: Find install path via %APPDATA%\itch\state.json or default locations
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var itchUserData = Path.Combine(appData, "itch");

            // Default library location
            var defaultLibraryPath = Path.Combine(itchUserData, "apps");

            // We could parse %APPDATA%\itch\db\butler.db, but that requires a sqlite reader.
            // Scanning the default apps folder is a safe 90% solution for a simple launcher.

            if (!Directory.Exists(defaultLibraryPath)) return;

            var gameDirs = Directory.GetDirectories(defaultLibraryPath);

            foreach (var gameDir in gameDirs)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(gameDir);
                    var gameName = dirInfo.Name; // Itch folder names are usually slugs, e.g., "celeste"

                    // Check for .itch.toml manifest (Playnite uses this for launch actions)
                    var manifestPath = Path.Combine(gameDir, ".itch.toml");
                    string prettyName = null;
                    string launchExe = null;

                    if (File.Exists(manifestPath))
                    {
                        try
                        {
                            // Itch uses TOML, but simple JSON parser might fail.
                            // However, PlayniteExtensions uses a TOML deserializer.
                            // For SimpleLauncher, we might need to do basic text parsing if we don't want a TOML lib dependency.
                            // Or assume the user might have a receipt.json.gz (which is compressed).

                            // Let's try to find the executable via heuristics if we can't parse TOML easily without deps.
                            // But let's try to read the file content to see if we can regex the exe.
                            var lines = await File.ReadAllLinesAsync(manifestPath);
                            foreach (var line in lines)
                            {
                                if (line.Contains("path =") || line.Contains("path="))
                                {
                                    // very rough parsing
                                    var parts = line.Split('=');
                                    if (parts.Length > 1)
                                    {
                                        var val = parts[1].Trim().Trim('"', '\'');
                                        if (val.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                                        {
                                            launchExe = Path.Combine(gameDir, val);
                                            break;
                                        }
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
                    Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                    // Itch protocol launch: itch://apps/{id} is ideal, but we don't easily have the ID without the DB.
                    // Fallback: Create a direct shortcut to the EXE or use the itch://launch protocol if we knew the cave ID.
                    // Since we don't have the Cave ID (stored in binary DB), we will create a direct EXE launch shortcut
                    // OR a batch file that launches the exe.

                    if (!string.IsNullOrEmpty(launchExe) && File.Exists(launchExe))
                    {
                        // Create a .bat to launch it (SimpleLauncher supports .bat)
                        // This bypasses the Itch client but works for DRM-free games (most itch games).
                        var batPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.bat");
                        var batContent = $"@echo off\r\ncd /d \"{Path.GetDirectoryName(launchExe)}\"\r\nstart \"\" \"{Path.GetFileName(launchExe)}\"";
                        await File.WriteAllTextAsync(batPath, batContent);

                        await GameScannerService.ExtractIconFromGameFolder(gameDir, sanitizedGameName, windowsImagesPath, launchExe);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Itch.io games.");
        }
    }
}

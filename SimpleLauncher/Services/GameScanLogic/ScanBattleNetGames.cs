using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models.GameScanLogic;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanBattleNetGames
{
    // Mapping from InternalId (found in registry) to Readable Name and Executable (for icons)
    // Derived from PlayniteExtensions BattleNetGames.cs
    private static readonly List<BNetAppDef> AppDefinitions = new()
    {
        new BNetAppDef { InternalId = "wow", Name = "World of Warcraft" },
        new BNetAppDef { InternalId = "diablo3", Name = "Diablo III" },
        new BNetAppDef { InternalId = "s2", Name = "StarCraft II" },
        new BNetAppDef { InternalId = "s1", Name = "StarCraft" },
        new BNetAppDef { InternalId = "hs_beta", Name = "Hearthstone" },
        new BNetAppDef { InternalId = "heroes", Name = "Heroes of the Storm" },
        new BNetAppDef { InternalId = "prometheus", Name = "Overwatch 2" },
        new BNetAppDef { InternalId = "viper", Name = "Call of Duty Black Ops 4" },
        new BNetAppDef { InternalId = "odin", Name = "Call of Duty Modern Warfare" },
        new BNetAppDef { InternalId = "w3", Name = "Warcraft III Reforged" },
        new BNetAppDef { InternalId = "lazarus", Name = "Call of Duty MW2 Campaign Remastered" },
        new BNetAppDef { InternalId = "zeus", Name = "Call of Duty Black Ops Cold War" },
        new BNetAppDef { InternalId = "wlby", Name = "Crash Bandicoot 4" },
        new BNetAppDef { InternalId = "osi", Name = "Diablo II Resurrected" },
        new BNetAppDef { InternalId = "rtro", Name = "Blizzard Arcade Collection" },
        new BNetAppDef { InternalId = "fore", Name = "Call of Duty Vanguard" },
        new BNetAppDef { InternalId = "anbs", Name = "Diablo Immortal" },
        new BNetAppDef { InternalId = "auks", Name = "Call of Duty Modern Warfare II" },
        new BNetAppDef { InternalId = "Fen", Name = "Diablo IV" },
        new BNetAppDef { InternalId = "gryphon", Name = "Warcraft Rumble" },
        // Classics
        new BNetAppDef { InternalId = "Diablo II", Name = "Diablo II", IsClassic = true, Exe = "Diablo II.exe" },
        new BNetAppDef { InternalId = "Warcraft III", Name = "Warcraft III", IsClassic = true, Exe = "Warcraft III.exe" }
    };

    public static async Task ScanBattleNetGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
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

                        var displayName = subKey.GetValue("DisplayName") as string;
                        var uninstallString = subKey.GetValue("UninstallString") as string;
                        var installLocation = subKey.GetValue("InstallLocation") as string;

                        if (string.IsNullOrEmpty(uninstallString)) continue;

                        // Check for Battle.net UID in uninstall string
                        // Regex from Playnite: Battle\.net.*--uid=(.*?)\s
                        var match = Regex.Match(uninstallString, @"Battle\.net.*--uid=(.*?)\s");

                        if (match.Success)
                        {
                            var uid = match.Groups[1].Value;
                            var def = AppDefinitions.FirstOrDefault(a => uid.StartsWith(a.InternalId, StringComparison.OrdinalIgnoreCase));

                            if (def != null)
                            {
                                if (ignoredGameNames.Contains(def.Name)) continue;

                                var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(def.Name);
                                var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                                // Launch via Battle.net executable arguments to ensure login
                                // Playnite uses: Battle.net.exe --exec="launch {uid}"
                                // We can try protocol: battlenet://{uid} (works for some, but exec is safer for login)
                                // Let's use protocol for simplicity in .url file, or create .bat if needed.
                                // Protocol: battlenet://{InternalId} usually works.

                                var shortcutContent = $"[InternetShortcut]\nURL=battlenet://{def.InternalId}";
                                await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                                if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                {
                                    await GameScannerService.ExtractIconFromGameFolder(installLocation, sanitizedGameName, windowsImagesPath);
                                }
                            }
                        }
                        else if (displayName != null && AppDefinitions.Any(a => a.IsClassic && displayName.Equals(a.InternalId, StringComparison.OrdinalIgnoreCase)))
                        {
                            // Classic Games
                            var def = AppDefinitions.First(a => a.IsClassic && displayName.Equals(a.InternalId, StringComparison.OrdinalIgnoreCase));
                            if (ignoredGameNames.Contains(def.Name)) continue;

                            var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(def.Name);
                            var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.bat");

                            if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                            {
                                var exePath = Path.Combine(installLocation, def.Exe);
                                var batContent = $"@echo off\r\ncd /d \"{installLocation}\"\r\nstart \"\" \"{def.Exe}\"";
                                await File.WriteAllTextAsync(shortcutPath, batContent);
                                await GameScannerService.ExtractIconFromGameFolder(installLocation, sanitizedGameName, windowsImagesPath, exePath);
                            }
                        }
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
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Battle.net games.");
        }
    }
}

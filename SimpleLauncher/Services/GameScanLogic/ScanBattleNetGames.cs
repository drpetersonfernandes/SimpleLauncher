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
        new BNetAppDef { InternalId = "w1", Name = "Warcraft: Orcs & Humans" },
        new BNetAppDef { InternalId = "w2", Name = "Warcraft II: Battle.net Edition" },
        new BNetAppDef { InternalId = "w1r", Name = "Warcraft: Remastered" },
        new BNetAppDef { InternalId = "w2r", Name = "Warcraft II: Remastered" },
        new BNetAppDef { InternalId = "d1", Name = "Diablo" },
        // Classics
        new BNetAppDef { InternalId = "Diablo II", Name = "Diablo II", IsClassic = true, Exe = "Diablo II.exe", ProductId = "D2" },
        new BNetAppDef { InternalId = "Warcraft III", Name = "Warcraft III", IsClassic = true, Exe = "Warcraft III.exe", ProductId = "W3" }
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
                                // Most apps uses: Battle.net.exe --exec="launch {uid}"
                                // We can try protocol: battlenet://{uid} (works for some, but exec is safer for login)
                                // Let's use protocol for simplicity in .url file, or create .bat if needed.
                                // Protocol: battlenet://{InternalId} usually works.

                                var launchId = !string.IsNullOrEmpty(def.ProductId) ? def.ProductId : def.InternalId;
                                var shortcutContent = $"[InternetShortcut]\nURL=battlenet://{launchId}";
                                await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                                if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                {
                                    await GameScannerService.FindAndSaveGameImageAsync(logErrors, def.Name, installLocation, sanitizedGameName, windowsImagesPath);
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
                                await GameScannerService.FindAndSaveGameImageAsync(logErrors, def.Name, installLocation, sanitizedGameName, windowsImagesPath, exePath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await logErrors.LogErrorAsync(ex, $"Error processing Battle.net game registry key: {subKeyName}");
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
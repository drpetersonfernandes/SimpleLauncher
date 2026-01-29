using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameScan.Models;
using SimpleLauncher.Services.SanitizeInputString;

namespace SimpleLauncher.Services.GameScan;

public static partial class ScanRockstarGames
{
    // Mapping from TitleId to Name and Executable
    private static readonly List<RockstarGameDef> Games =
    [
        new() { TitleId = "gta5", Name = "Grand Theft Auto V", Exe = "PlayGTAV.exe" },
        new() { TitleId = "gta5_gen9", Name = "Grand Theft Auto V Enhanced", Exe = "GTA5_Enhanced_BE.exe" },
        new() { TitleId = "rdr2", Name = "Red Dead Redemption 2", Exe = "RDR2.exe" },
        new() { TitleId = "rdr", Name = "Red Dead Redemption", Exe = "RDR.exe" },
        new() { TitleId = "lanoire", Name = "L.A. Noire", Exe = "LANoire.exe" },
        new() { TitleId = "lanoirevr", Name = "L.A. Noire: The VR Case Files", Exe = "LANoireVR.exe" },
        new() { TitleId = "mp3", Name = "Max Payne 3", Exe = "MaxPayne3.exe" },
        new() { TitleId = "gtasa", Name = "Grand Theft Auto San Andreas", Exe = "gta_sa.exe" },
        new() { TitleId = "gta3", Name = "Grand Theft Auto III", Exe = "gta3.exe" },
        new() { TitleId = "gtavc", Name = "Grand Theft Auto Vice City", Exe = "gta-vc.exe" },
        new() { TitleId = "bully", Name = "Bully Scholarship Edition", Exe = "Bully.exe" },
        new() { TitleId = "gta4", Name = "Grand Theft Auto IV", Exe = "GTAIV.exe" },
        new() { TitleId = "gta3unreal", Name = "GTA III Definitive Edition", Exe = "Gameface/Binaries/Win64/LibertyCity.exe" },
        new() { TitleId = "gtavcunreal", Name = "GTA Vice City Definitive Edition", Exe = "Gameface/Binaries/Win64/ViceCity.exe" },
        new() { TitleId = "gtasaunreal", Name = "GTA San Andreas Definitive Edition", Exe = "Gameface/Binaries/Win64/SanAndreas.exe" }
    ];

    public static async Task ScanRockstarGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
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

                        var uninstallString = subKey.GetValue("UninstallString") as string;
                        var installLocation = subKey.GetValue("InstallLocation") as string;

                        if (string.IsNullOrEmpty(uninstallString)) continue;

                        var match = MyRegex().Match(uninstallString);

                        if (match.Success)
                        {
                            var titleId = match.Groups[1].Value.Trim();
                            var gameDef = Games.FirstOrDefault(g => g.TitleId.Equals(titleId, StringComparison.OrdinalIgnoreCase));

                            if (gameDef != null)
                            {
                                if (ignoredGameNames.Contains(gameDef.Name)) continue;

                                var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(gameDef.Name);
                                var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                                // Rockstar Launcher Protocol
                                // rockstargames://launch/{titleId} usually works.
                                // We'll use the protocol for the .url file.
                                var shortcutContent = $"[InternetShortcut]\nURL=rockstargames://launch/{titleId}";
                                await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                                if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                                {
                                    var exePath = Path.Combine(installLocation, gameDef.Exe);
                                    await GameScannerService.FindAndSaveGameImageAsync(logErrors, gameDef.Name, installLocation, sanitizedGameName, windowsImagesPath, exePath);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await logErrors.LogErrorAsync(ex, $"Error processing Rockstar game registry key: {subKeyName}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Rockstar games.");
        }
    }

    [GeneratedRegex(@"(?:Launcher|uninstall)\.exe.+uninstall=(.+)$", RegexOptions.IgnoreCase, "pt-BR")]
    private static partial Regex MyRegex();
}
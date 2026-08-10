using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.Win32;

namespace SimpleLauncher.Avalonia.Services.GameScan;

/// <summary>
/// Scans installed PC games from digital storefronts via registry, known paths, and manifest files.
/// Simplified replacement for the old 11-scanner system. No image download or shortcut creation.
/// </summary>
public class StorefrontGameScanner
{
    // Steam libraryfolders.vdf / appmanifest_*.acf parsing (VDF key = "value" lines)
    private static readonly System.Text.RegularExpressions.Regex SteamPathRegex =
        new("""
            "path"\s+"([^"]+)"
            """, System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex SteamNameRegex =
        new("""
            "name"\s+"([^"]+)"
            """, System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex SteamInstallDirRegex =
        new("""
            "installdir"\s+"([^"]+)"
            """, System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>
    /// Scans all supported storefronts and returns discovered games as (name, exePath) pairs.
    /// </summary>
    public async Task<List<(string Name, string ExePath, string Storefront)>> ScanAllAsync()
    {
        var results = new List<(string, string, string)>();

        // Storefront scanning reads the Windows registry (Steam/Epic/GOG install paths);
        // not available on Linux.
        await Task.Run(() =>
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            results.AddRange(ScanSteam());
            results.AddRange(ScanEpic());
            results.AddRange(ScanGog());
            results.AddRange(ScanAmazon());
            results.AddRange(ScanBattleNet());
            results.AddRange(ScanRockstar());
            results.AddRange(ScanUbisoft());
            results.AddRange(ScanMicrosoftStore());
        });

        // Deduplicate by name (keep first found)
        return results
            .GroupBy(r => r.Item1, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(r => r.Item1)
            .ToList();
    }

    #region Steam

    [SupportedOSPlatform("windows")]
    private static List<(string, string, string)> ScanSteam()
    {
        var results = new List<(string, string, string)>();
        try
        {
            var steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;
            if (string.IsNullOrEmpty(steamPath)) return results;

            var libraryFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFile)) return results;

            var content = File.ReadAllText(libraryFile);
            // Parse VDF: extract "path" values
            var paths = new List<string> { Path.Combine(steamPath, "steamapps") };
            foreach (System.Text.RegularExpressions.Match match in SteamPathRegex.Matches(content))
                paths.Add(match.Groups[1].Value.Replace(@"\\", "\\"));

            foreach (var libPath in paths)
            {
                var appsDir = Path.Combine(libPath, "steamapps");
                if (!Directory.Exists(appsDir)) continue;

                foreach (var manifest in Directory.GetFiles(appsDir, "appmanifest_*.acf"))
                {
                    try
                    {
                        var acf = File.ReadAllText(manifest);
                        var nameMatch = SteamNameRegex.Match(acf);
                        var installMatch = SteamInstallDirRegex.Match(acf);
                        if (!nameMatch.Success || !installMatch.Success) continue;

                        var gameName = nameMatch.Groups[1].Value;
                        var installDir = installMatch.Groups[1].Value;
                        var commonDir = Path.Combine(libPath, "common", installDir);

                        if (Directory.Exists(commonDir))
                        {
                            var exe = FindMainExe(commonDir);
                            if (exe is not null)
                                results.Add((gameName, exe, "Steam"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, "Storefront scan error");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Storefront scan error");
        }

        return results;
    }

    #endregion

    #region Epic Games

    [SupportedOSPlatform("windows")]
    private static List<(string, string, string)> ScanEpic()
    {
        var results = new List<(string, string, string)>();
        try
        {
            var epicPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Epic", "UnrealEngineLauncher", "LauncherInstalled.dat");

            if (!File.Exists(epicPath))
            {
                epicPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "EpicGamesLauncher", "Saved", "Config", "Windows", "LauncherInstalled.dat");
            }

            if (!File.Exists(epicPath)) return results;

            var json = File.ReadAllText(epicPath);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("InstallationList", out var list)) return results;

            foreach (var item in list.EnumerateArray())
            {
                try
                {
                    var name = item.GetProperty("AppName").GetString() ?? "";
                    var installPath = item.GetProperty("InstallLocation").GetString() ?? "";
                    if (Directory.Exists(installPath))
                    {
                        var exe = FindMainExe(installPath);
                        if (exe is not null)
                            results.Add((name, exe, "Epic Games"));
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Storefront scan error");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Storefront scan error");
        }

        return results;
    }

    #endregion

    #region GOG

    [SupportedOSPlatform("windows")]
    private static List<(string, string, string)> ScanGog()
    {
        var results = new List<(string, string, string)>();
        try
        {
            var gogKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\Games");
            if (gogKey is null) return results;

            foreach (var gameId in gogKey.GetSubKeyNames())
            {
                try
                {
                    using var gameKey = gogKey.OpenSubKey(gameId);
                    if (gameKey is null) continue;

                    var name = gameKey.GetValue("GAMENAME") as string ?? gameKey.GetValue("GameName") as string;
                    var path = gameKey.GetValue("PATH") as string ?? gameKey.GetValue("Path") as string;

                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(path) && Directory.Exists(path))
                    {
                        var exe = gameKey.GetValue("EXE") as string;
                        if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
                        {
                            exe = FindMainExe(path);
                        }

                        if (exe is not null)
                            results.Add((name, exe, "GOG"));
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Storefront scan error");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Storefront scan error");
        }

        return results;
    }

    #endregion

    #region Amazon Games

    [SupportedOSPlatform("windows")]
    private static List<(string, string, string)> ScanAmazon()
    {
        var results = new List<(string, string, string)>();
        try
        {
            var amazonPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Amazon Games", "Data", "Games", "Sql", "GameInstallInfo.sqlite");

            if (!File.Exists(amazonPath)) return results;

            // Amazon uses SQLite — try simple JSON in the AppData folder
            var dataDir = Path.GetDirectoryName(amazonPath);
            if (dataDir is null || !Directory.Exists(dataDir)) return results;

            // Some versions store in a JSON manifest
            foreach (var dir in Directory.GetDirectories(
                         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Amazon Games"), "Library"))
            {
                var exe = FindMainExe(dir);
                if (exe is not null)
                    results.Add((Path.GetFileName(dir), exe, "Amazon Games"));
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Storefront scan error");
        }

        return results;
    }

    #endregion

    #region Battle.net

    [SupportedOSPlatform("windows")]
    private static List<(string, string, string)> ScanBattleNet()
    {
        var results = new List<(string, string, string)>();
        var knownGames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Call of Duty"] = "Call of Duty",
            ["World of Warcraft"] = "World of Warcraft",
            ["Diablo IV"] = "Diablo IV",
            ["Diablo III"] = "Diablo III",
            ["Hearthstone"] = "Hearthstone",
            ["Overwatch"] = "Overwatch",
            ["StarCraft II"] = "StarCraft II",
            ["Heroes of the Storm"] = "Heroes of the Storm",
            ["Warcraft III"] = "Warcraft III"
        };

        try
        {
            var bnPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Battle.net");

            if (!Directory.Exists(bnPath)) return results;

            // Scan product directories
            var agentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Battle.net", "Agent", "product.db");
            if (File.Exists(agentPath))
            {
                var content = File.ReadAllText(agentPath);
                foreach (var kvp in knownGames)
                {
                    if (content.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        var dirs = Directory.GetDirectories(bnPath, kvp.Key + "*", SearchOption.TopDirectoryOnly);
                        foreach (var dir in dirs)
                        {
                            var exe = FindMainExe(dir);
                            if (exe is not null)
                            {
                                results.Add((kvp.Value, exe, "Battle.net"));
                                break;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Storefront scan error");
        }

        return results;
    }

    #endregion

    #region Rockstar

    [SupportedOSPlatform("windows")]
    private static List<(string, string, string)> ScanRockstar()
    {
        var results = new List<(string, string, string)>();
        try
        {
            var rsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Rockstar Games", "Launcher");

            if (Directory.Exists(rsPath))
            {
                foreach (var dir in Directory.GetDirectories(rsPath))
                {
                    if (dir.Contains("Launcher", StringComparison.OrdinalIgnoreCase)) continue;

                    var exe = FindMainExe(dir);
                    if (exe is not null)
                        results.Add((Path.GetFileName(dir), exe, "Rockstar"));
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Storefront scan error");
        }

        return results;
    }

    #endregion

    #region Ubisoft

    [SupportedOSPlatform("windows")]
    private static List<(string, string, string)> ScanUbisoft()
    {
        var results = new List<(string, string, string)>();
        try
        {
            var ubiPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "Ubisoft");

            if (Directory.Exists(ubiPath))
            {
                foreach (var dir in Directory.GetDirectories(ubiPath))
                {
                    var exe = FindMainExe(dir);
                    if (exe is not null)
                        results.Add((Path.GetFileName(dir), exe, "Ubisoft"));
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Storefront scan error");
        }

        return results;
    }

    #endregion

    #region Microsoft Store

    [SupportedOSPlatform("windows")]
    private static List<(string, string, string)> ScanMicrosoftStore()
    {
        var results = new List<(string, string, string)>();
        try
        {
            var packagesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Packages");

            if (!Directory.Exists(packagesPath)) return results;

            // Look for Xbox/GamePass games
            foreach (var dir in Directory.GetDirectories(packagesPath))
            {
                var dirName = Path.GetFileName(dir);
                // Skip system packages
                if (dirName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
                    dirName.StartsWith("windows", StringComparison.OrdinalIgnoreCase))
                    continue;

                var exe = FindMainExe(dir);
                if (exe is not null)
                {
                    var name = dirName.Contains('_') ? dirName[..dirName.IndexOf('_')] : dirName;
                    results.Add((name, exe, "Microsoft Store"));
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Storefront scan error");
        }

        return results;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Finds the main executable in a game directory.
    /// Looks for a single .exe at the root, or the largest .exe.
    /// </summary>
    private static string? FindMainExe(string directory)
    {
        try
        {
            if (!Directory.Exists(directory)) return null;

            // Skip known launchers and uninstallers
            var skipPatterns = new[] { "unins", "UnityCrashHandler", "dxsetup", "vcredist", "dotnet" };

            var exes = Directory.GetFiles(directory, "*.exe", SearchOption.TopDirectoryOnly)
                .Where(e => !skipPatterns.Any(s => Path.GetFileName(e).Contains(s, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (exes.Count == 0)
            {
                // Try one level deeper
                exes = Directory.GetFiles(directory, "*.exe", SearchOption.AllDirectories)
                    .Where(e => !skipPatterns.Any(s => Path.GetFileName(e).Contains(s, StringComparison.OrdinalIgnoreCase)))
                    .Take(20)
                    .ToList();
            }

            return exes.Count switch
            {
                1 => exes[0],
                0 => null,
                _ => exes.OrderByDescending(e => new FileInfo(e).Length).First()
            };

            // Return the largest .exe (most likely the game)
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to find main executable in {Directory}", directory);
            return null;
        }
    }

    #endregion
}

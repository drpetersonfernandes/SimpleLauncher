using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.RetroAchievements;

/// <summary>
/// Provides fuzzy matching for RetroAchievements system names to ensure proper hash method selection.
/// </summary>
public static class RetroAchievementsSystemMatcher
{
    /// <summary>
    /// Holds information about a RetroAchievements system, including its ID and name aliases.
    /// </summary>
    public class RaSystemInfo
    {
        public int Id { get; }
        public string[] Aliases { get; }

        public RaSystemInfo(int id, string[] aliases)
        {
            Id = id;
            Aliases = aliases;
        }
    }

    // Define system name mappings with their official RA Console ID and fuzzy matching patterns.
    private static readonly Dictionary<string, RaSystemInfo> SystemMappings = new()
    {
        // Nintendo systems
        ["nintendo 64"] = new RaSystemInfo(2, ["nintendo 64", "n64", "nintendo64"]),
        ["super nintendo entertainment system"] = new RaSystemInfo(3, ["super nintendo entertainment system", "snes", "super nintendo", "super famicom"]),
        ["game boy"] = new RaSystemInfo(4, ["game boy", "gb", "gameboy"]),
        ["game boy advance"] = new RaSystemInfo(5, ["game boy advance", "gba", "gameboy advance"]),
        ["game boy color"] = new RaSystemInfo(6, ["game boy color", "gbc", "gameboy color"]),
        ["nintendo entertainment system"] = new RaSystemInfo(7, ["nintendo entertainment system", "nes", "famicom"]),
        ["gamecube"] = new RaSystemInfo(16, ["gamecube", "game cube", "game-cube", "gc", "nintendo gamecube", "nintendo game cube", "nintendo game-cube", "nintendo gc"]),
        ["nintendo ds"] = new RaSystemInfo(18, ["nintendo ds", "nintendo-ds", "nintendods", "nds", "ds", "nintendo ds"]),
        ["virtual boy"] = new RaSystemInfo(28, ["virtual boy", "virtualboy", "vb", "nintendo virtual boy", "nintendo virtualboy", "nintendo vb"]),
        ["pokemon mini"] = new RaSystemInfo(24, ["pokemon mini", "pokémon mini"]),
        ["nintendo dsi"] = new RaSystemInfo(78, ["nintendo dsi", "nintendo ds-i", "ndsi", "dsi", "nintendo ndsi", "nintendo dsi"]),
        ["famicom disk system"] = new RaSystemInfo(81, ["famicom disk system", "fds", "nintendo famicom disk system", "nintendo fds"]),
        ["wii"] = new RaSystemInfo(19, ["wii", "nintendo wii"]),
        ["wii u"] = new RaSystemInfo(20, ["wii u", "wiiu", "wii-u", "nintendo wii u", "nintendo wiiu", "nintendo wii-u"]),
        ["nintendo 3ds"] = new RaSystemInfo(62, ["nintendo 3ds", "3ds", "nintendo3ds", "nintendo 3ds"]),

        // Sega systems
        ["genesis/mega drive"] = new RaSystemInfo(1, ["genesis/mega drive", "genesis", "mega drive", "megadrive", "sega genesis", "sega megadrive", "sega mega drive"]),
        ["sega cd"] = new RaSystemInfo(9, ["sega cd", "segacd", "sega genesis cd", "genesis cd", "sega megadrive cd", "megadrive cd", "sega mega drive cd", "mega drive cd"]),
        ["32x"] = new RaSystemInfo(10, ["32x", "sega 32x", "sega genesis 32x", "genesis 32x", "megadrive 32x", "mega drive 32x", "sega megadrive 32x", "mega drive 32x", "sega mega drive 32x"]),
        ["master system"] = new RaSystemInfo(11, ["master system", "mastersystem", "mark3"]),
        ["game gear"] = new RaSystemInfo(15, ["game gear", "gamegear", "sega game gear", "sega gamegear"]),
        ["saturn"] = new RaSystemInfo(39, ["saturn", "sega saturn"]),
        ["dreamcast"] = new RaSystemInfo(40, ["dreamcast", "sega dreamcast"]),
        ["sg-1000"] = new RaSystemInfo(33, ["sg-1000", "sg1000", "sega sg-1000", "sega sg1000"]),
        ["sega pico"] = new RaSystemInfo(68, ["sega pico", "pico"]),

        // Sony systems
        ["playstation"] = new RaSystemInfo(12, ["playstation", "ps1", "psx", "playstation 1", "sony playstation 1", "sony playstation"]),
        ["playstation 2"] = new RaSystemInfo(21, ["playstation 2", "ps2", "sony playstation 2"]),
        ["playstation portable"] = new RaSystemInfo(41, ["playstation portable", "psp"]),

        // NEC systems
        ["pc engine/turbografx-16"] = new RaSystemInfo(8, ["pc engine/turbografx-16", "pc engine", "pcengine", "pc-engine", "turbografx-16", "turbografx 16", "turbografx", "turbografx16", "pce", "tg16"]),
        ["pc engine cd/turbografx-cd"] = new RaSystemInfo(76, [
            "pc engine cd/turbografx-cd", "pc engine cd", "pcengine cd", "pcenginecd", "pcecd", "pce-cd", "pc-engine cd", "turbografx-cd", "turbografx cd",
            "nec pc engine cd", "nec pcengine cd", "nec pcenginecd", "nec pcecd", "nec pce-cd", "nec pc-engine cd", "nec turbografx-cd", "nec turbografx cd"
        ]),
        ["supergrafx"] = new RaSystemInfo(8, ["supergrafx", "sgx"]), // SuperGrafx uses the same core as PC Engine

        // Atari systems
        ["atari lynx"] = new RaSystemInfo(13, ["atari lynx", "lynx"]),
        ["atari jaguar"] = new RaSystemInfo(17, ["atari jaguar", "jaguar"]),
        ["atari 2600"] = new RaSystemInfo(25, ["atari 2600", "atari2600", "atari vcs"]),
        ["atari 7800"] = new RaSystemInfo(51, ["atari 7800", "atari7800"]),
        ["atari jaguar cd"] = new RaSystemInfo(77, ["atari jaguar cd", "jaguar cd", "jaguarcd"]),
        ["atari 5200"] = new RaSystemInfo(50, ["atari 5200", "atari5200"]),
        ["atari st"] = new RaSystemInfo(36, ["atari st", "atari ste", "atarist"]),

        // Other systems
        ["arcade"] = new RaSystemInfo(27, [
            "arcade", "mame", "m.a.m.e.", "arcade games", "arcade classics", "fliperama",
            "neogeo", "neo geo", "neo-geo", "snk neo geo", "snk neogeo"
        ]),
        ["neo geo pocket"] = new RaSystemInfo(14, ["neo geo pocket", "neo geo pocket color", "neogeo pocket", "neogeo pocket color", "ngp", "ngpc"]),
        ["magnavox odyssey 2"] = new RaSystemInfo(23, ["magnavox odyssey 2", "odyssey 2", "odyssey2", "videopac g7000"]),
        ["msx"] = new RaSystemInfo(29, ["msx", "msx1", "msx2"]),
        ["amstrad cpc"] = new RaSystemInfo(37, ["amstrad cpc", "cpc", "amstrad"]),
        ["apple ii"] = new RaSystemInfo(38, ["apple ii", "apple //", "apple2"]),
        ["3do interactive multiplayer"] = new RaSystemInfo(43, ["3do interactive multiplayer", "3do"]),
        ["colecovision"] = new RaSystemInfo(44, ["colecovision"]),
        ["intellivision"] = new RaSystemInfo(45, ["intellivision", "intv"]),
        ["vectrex"] = new RaSystemInfo(46, ["vectrex"]),
        ["pc-fx"] = new RaSystemInfo(49, ["pc-fx", "pcfx"]),
        ["wonderswan"] = new RaSystemInfo(53, ["wonderswan", "wonderswan color"]),
        ["neo geo cd"] = new RaSystemInfo(56, ["neo geo cd", "neogeo cd", "neo geo compact disc"]),
        ["watara supervision"] = new RaSystemInfo(63, ["watara supervision", "supervision"]),
        ["mega duck"] = new RaSystemInfo(69, ["mega duck", "creativision"]),
        ["arduboy"] = new RaSystemInfo(71, ["arduboy"]),
        ["wasm-4"] = new RaSystemInfo(72, ["wasm-4"]),
        ["pc-8000/8800"] = new RaSystemInfo(47, ["pc-8000/8800", "pc-8000", "pc-8800", "pc8000", "pc8800"]),
        ["commodore 64"] = new RaSystemInfo(30, ["commodore 64", "c64"]),
        ["amiga"] = new RaSystemInfo(35, ["amiga", "commodore amiga"]),
        ["zx spectrum"] = new RaSystemInfo(59, ["zx spectrum", "zxspectrum", "spectrum"]),
        ["fairchild channel f"] = new RaSystemInfo(57, ["fairchild channel f", "channel f"]),
        ["philips cd-i"] = new RaSystemInfo(42, ["philips cd-i", "cd-i"]),
        ["sharp x68000"] = new RaSystemInfo(52, ["sharp x68000", "x68000"]),
        ["sharp x1"] = new RaSystemInfo(64, ["sharp x1", "x1"]),
        ["oric"] = new RaSystemInfo(32, ["oric"]),
        ["thomson to8"] = new RaSystemInfo(66, ["thomson to8", "to8"]),
        ["cassette vision"] = new RaSystemInfo(54, ["cassette vision"]),
        ["super cassette vision"] = new RaSystemInfo(55, ["super cassette vision"]),
        ["uzebox"] = new RaSystemInfo(80, ["uzebox"]),
        ["tic-80"] = new RaSystemInfo(65, ["tic-80"]),
        ["ti-83"] = new RaSystemInfo(79, ["ti-83"]),
        ["nokia n-gage"] = new RaSystemInfo(61, ["nokia n-gage", "n-gage"]),
        ["vic-20"] = new RaSystemInfo(34, ["vic-20", "vic20"]),
        ["zx81"] = new RaSystemInfo(31, ["zx81"]),
        ["pc-6000"] = new RaSystemInfo(67, ["pc-6000", "pc6000"]),
        ["game & watch"] = new RaSystemInfo(60, ["game & watch", "game and watch"]),
        ["elektor tv games computer"] = new RaSystemInfo(75, ["elektor tv games computer"]),
        ["interton vc 4000"] = new RaSystemInfo(74, ["interton vc 4000"]),
        ["arcadia 2001"] = new RaSystemInfo(73, ["arcadia 2001"]),
        ["fm towns"] = new RaSystemInfo(58, ["fm towns"]),
        ["hubs"] = new RaSystemInfo(100, ["hubs"]),
        ["events"] = new RaSystemInfo(101, ["events"]),
        ["standalone"] = new RaSystemInfo(102, ["standalone"]),
        ["Xbox"] = new RaSystemInfo(102, ["xbox", "x-box"]),
        ["DOS"] = new RaSystemInfo(102, ["dos", "microsoft dos"]),
        ["PC-9800"] = new RaSystemInfo(102, ["pc-9800", "pc-9800", "pc9800"]),
        ["Zeebo"] = new RaSystemInfo(102, ["Zeebo"])
    };

    /// <summary>
    /// Finds the best matching RetroAchievements system name using fuzzy matching.
    /// </summary>
    /// <param name="inputSystemName">The system name to match</param>
    /// <returns>The normalized RetroAchievements system name, or the original if no match found</returns>
    public static string GetBestMatchSystemName(string inputSystemName)
    {
        if (string.IsNullOrWhiteSpace(inputSystemName))
            return inputSystemName;

        var normalizedInput = inputSystemName.Trim().ToLowerInvariant();

        // Direct exact match (faster for common cases)
        foreach (var kvp in SystemMappings)
        {
            if (kvp.Value.Aliases.Any(pattern => pattern.Equals(normalizedInput, StringComparison.OrdinalIgnoreCase)))
            {
                return kvp.Key;
            }
        }

        // Fuzzy matching - check if any pattern is contained within the input
        foreach (var kvp in SystemMappings)
        {
            if (kvp.Value.Aliases.Any(pattern => normalizedInput.Contains(pattern) || pattern.Contains(normalizedInput)))
            {
                return kvp.Key;
            }
        }

        // Check for partial matches with higher similarity
        foreach (var kvp in SystemMappings)
        {
            foreach (var pattern in kvp.Value.Aliases)
            {
                if (IsFuzzyMatch(normalizedInput, pattern))
                {
                    return kvp.Key;
                }
            }
        }

        // No match found, log it for future improvement and return original.
        DebugLogger.Log($"[RA System Matcher] No match found for system name: '{inputSystemName}'. Consider adding it as an alias.");
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RA System Matcher] No match found for system name: '{inputSystemName}'. Consider adding it as an alias.");

        return normalizedInput;
    }

    public static bool IsOfficialSystemName(string systemName)
    {
        return SystemMappings.ContainsKey(systemName.ToLowerInvariant());
    }

    public static List<string> GetSupportedSystemNames()
    {
        return SystemMappings.Keys.OrderBy(static s => s).ToList();
    }

    /// <summary>
    /// Gets the RetroAchievements Console ID for a given system name.
    /// </summary>
    /// <param name="inputSystemName">The system name to look up.</param>
    /// <returns>The console ID, or -1 if not found.</returns>
    public static int GetSystemId(string inputSystemName)
    {
        DebugLogger.Log($"[GetSystemId] Looking up system ID for '{inputSystemName}'");
        var bestMatch = GetBestMatchSystemName(inputSystemName);
        DebugLogger.Log($"[GetSystemId] Best match: '{bestMatch}'");
        return SystemMappings.TryGetValue(bestMatch, out var systemInfo) ? systemInfo.Id : -1;
    }

    public static string GetExactAliasMatch(string inputSystemName)
    {
        if (string.IsNullOrWhiteSpace(inputSystemName)) return null;

        var normalizedInput = inputSystemName.Trim().ToLowerInvariant();

        foreach (var kvp in SystemMappings)
        {
            if (kvp.Value.Aliases.Any(pattern => pattern.Equals(normalizedInput, StringComparison.OrdinalIgnoreCase)))
            {
                return kvp.Key; // Found 100% match
            }
        }

        return null; // No exact match
    }

    /// <summary>
    /// Performs fuzzy matching between two strings using a combination of techniques.
    /// </summary>
    private static bool IsFuzzyMatch(string input, string pattern)
    {
        // Remove common separators and normalize
        var cleanInput = RemoveSeparators(input);
        var cleanPattern = RemoveSeparators(pattern);

        // Exact match
        if (cleanInput.Equals(cleanPattern, StringComparison.OrdinalIgnoreCase))
            return true;

        // Contains check
        if (cleanInput.Contains(cleanPattern) || cleanPattern.Contains(cleanInput))
            return true;

        // Levenshtein distance for very similar strings (only for shorter strings)
        if (cleanInput.Length <= 30 && cleanPattern.Length <= 30)
        {
            var distance = CalculateLevenshteinDistance(cleanInput, cleanPattern);
            var maxLength = Math.Max(cleanInput.Length, cleanPattern.Length);
            var similarity = 1.0 - (double)distance / maxLength;
            return similarity >= 0.8; // 80% similarity threshold
        }

        return false;
    }

    /// <summary>
    /// Removes common separators and normalizes the string.
    /// </summary>
    private static string RemoveSeparators(string input)
    {
        return input.Replace("-", "")
            .Replace("/", "")
            .Replace("&", "")
            .Replace(" ", "")
            .Replace(".", "")
            .Replace("+", "")
            .ToLowerInvariant();
    }

    /// <summary>
    /// Calculates the Levenshtein distance between two strings.
    /// </summary>
    private static int CalculateLevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s))
            return string.IsNullOrEmpty(t) ? 0 : t.Length;

        if (string.IsNullOrEmpty(t))
            return s.Length;

        var n = s.Length;
        var m = t.Length;
        var d = new int[n + 1, m + 1];

        // Initialize the first row and column
        for (var i = 0; i <= n; d[i, 0] = i++)
        {
        }

        for (var j = 0; j <= m; d[0, j] = j++)
        {
        }

        // Calculate distances
        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                var cost = t[j - 1] == s[i - 1] ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }
}

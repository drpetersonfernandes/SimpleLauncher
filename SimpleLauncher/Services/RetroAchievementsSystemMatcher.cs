using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleLauncher.Services;

/// <summary>
/// Provides fuzzy matching for RetroAchievements system names to ensure proper hash method selection.
/// </summary>
public static class RetroAchievementsSystemMatcher
{
    // Define system name mappings with fuzzy matching patterns
    private static readonly Dictionary<string, string[]> SystemNameMappings = new()
    {
        // Nintendo systems
        ["nintendo 64"] = ["nintendo 64", "n64"],
        ["nintendo entertainment system"] = ["nintendo entertainment system", "nes", "famicom"],
        ["famicom disk system"] = ["famicom disk system", "fds"],
        ["super nintendo entertainment system"] = ["super nintendo entertainment system", "snes", "super nintendo", "super famicom"],
        ["game boy"] = ["game boy", "gb"],
        ["game boy color"] = ["game boy color", "gbc"],
        ["game boy advance"] = ["game boy advance", "gba"],
        ["gamecube"] = ["gamecube", "gc"],
        ["wii"] = ["wii"],
        ["wii u"] = ["wii u", "wiiu"],
        ["nintendo 3ds"] = ["nintendo 3ds", "3ds"],
        ["nintendo dsi"] = ["nintendo dsi", "dsi"],
        ["nintendo ds"] = ["nintendo ds", "nds", "ds"],
        ["virtual boy"] = ["virtual boy", "vb"],
        ["pokemon mini"] = ["pokemon mini", "pokémon mini"],

        // Sega systems
        ["genesis/mega drive"] = ["genesis/mega drive", "genesis", "mega drive", "megadrive"],
        ["master system"] = ["master system", "mastersystem"],
        ["game gear"] = ["game gear", "gamegear"],
        ["sega cd"] = ["sega cd", "segacd"],
        ["32x"] = ["32x", "sega 32x"],
        ["saturn"] = ["saturn"],
        ["dreamcast"] = ["dreamcast"],
        ["sega pico"] = ["sega pico", "pico"],

        // Sony systems
        ["playstation"] = ["playstation", "ps1", "psx"],
        ["playstation 2"] = ["playstation 2", "ps2"],
        ["playstation portable"] = ["playstation portable", "psp"],

        // NEC systems
        ["pc engine/turbografx-16"] = ["pc engine/turbografx-16", "pc engine", "turbografx-16", "turbografx 16", "pce", "tg16"],
        ["pc engine cd/turbografx-cd"] = ["pc engine cd/turbografx-cd", "pc engine cd", "turbografx-cd", "turbografx cd", "pce-cd"],
        ["supergrafx"] = ["supergrafx", "sgx"],

        // Atari systems
        ["atari 2600"] = ["atari 2600", "atari vcs"],
        ["atari 7800"] = ["atari 7800"],
        ["atari lynx"] = ["atari lynx", "lynx"],
        ["atari jaguar"] = ["atari jaguar", "jaguar"],
        ["atari jaguar cd"] = ["atari jaguar cd", "jaguar cd"],
        ["atari 5200"] = ["atari 5200"],
        ["atari st"] = ["atari st", "atari ste"],

        // Other systems
        ["arcade"] = ["arcade", "mame"],
        ["amstrad cpc"] = ["amstrad cpc", "cpc"],
        ["apple ii"] = ["apple ii", "apple //", "apple2"],
        ["colecovision"] = ["colecovision", "colecovision"],
        ["intellivision"] = ["intellivision", "intv"],
        ["msx"] = ["msx", "msx1", "msx2"],
        ["pc-8000/8800"] = ["pc-8000/8800", "pc-8000", "pc-8800", "pc8000", "pc8800"],
        ["wonderswan"] = ["wonderswan", "wonderswan color"],
        ["neo geo pocket"] = ["neo geo pocket", "neo geo pocket color", "ngp", "ngpc"],
        ["neo geo cd"] = ["neo geo cd", "neo geo compact disc"],
        ["commodore 64"] = ["commodore 64", "c64"],
        ["amiga"] = ["amiga"],
        ["zx spectrum"] = ["zx spectrum", "spectrum"],
        ["vectrex"] = ["vectrex"],
        ["magnavox odyssey 2"] = ["magnavox odyssey 2", "odyssey 2", "videopac g7000"],
        ["fairchild channel f"] = ["fairchild channel f", "channel f"],
        ["3do interactive multiplayer"] = ["3do interactive multiplayer", "3do"],
        ["philips cd-i"] = ["philips cd-i", "cd-i"],
        ["pc-fx"] = ["pc-fx", "pcfx"],
        ["sharp x68000"] = ["sharp x68000", "x68000"],
        ["sharp x1"] = ["sharp x1", "x1"],
        ["oric"] = ["oric"],
        ["thomson to8"] = ["thomson to8", "to8"],
        ["cassette vision"] = ["cassette vision"],
        ["super cassette vision"] = ["super cassette vision"],
        ["watara supervision"] = ["watara supervision", "supervision"],
        ["arduboy"] = ["arduboy"],
        ["uzebox"] = ["uzebox"],
        ["tic-80"] = ["tic-80"],
        ["ti-83"] = ["ti-83"],
        ["nokia n-gage"] = ["nokia n-gage", "n-gage"],
        ["wasm-4"] = ["wasm-4"],
        ["mega duck"] = ["mega duck", "creativision"],
        ["sg-1000"] = ["sg-1000", "sg1000"],
        ["vic-20"] = ["vic-20", "vic20"],
        ["zx81"] = ["zx81"],
        ["pc-6000"] = ["pc-6000", "pc6000"],
        ["game & watch"] = ["game & watch", "game and watch"],
        ["elektor tv games computer"] = ["elektor tv games computer"],
        ["interton vc 4000"] = ["interton vc 4000"],
        ["arcadia 2001"] = ["arcadia 2001"],
        ["fm towns"] = ["fm towns"],
        ["hubs"] = ["hubs"],
        ["events"] = ["events"],
        ["standalone"] = ["standalone"]
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
        foreach (var kvp in SystemNameMappings)
        {
            if (kvp.Value.Any(pattern => pattern.Equals(normalizedInput, StringComparison.OrdinalIgnoreCase)))
            {
                return kvp.Key;
            }
        }

        // Fuzzy matching - check if any pattern is contained within the input
        foreach (var kvp in SystemNameMappings)
        {
            if (kvp.Value.Any(pattern => normalizedInput.Contains(pattern) || pattern.Contains(normalizedInput)))
            {
                return kvp.Key;
            }
        }

        // Check for partial matches with higher similarity
        foreach (var kvp in SystemNameMappings)
        {
            foreach (var pattern in kvp.Value)
            {
                if (IsFuzzyMatch(normalizedInput, pattern))
                {
                    return kvp.Key;
                }
            }
        }

        // No match found, return original
        return normalizedInput;
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

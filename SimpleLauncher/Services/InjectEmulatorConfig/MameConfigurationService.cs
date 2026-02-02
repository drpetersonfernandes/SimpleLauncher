using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static partial class MameConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, string systemRomPath = null)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory is null or empty.");

        var configPath = Path.Combine(emuDir, "mame.ini");

        // Backup logic: Create from sample if missing
        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "MAME", "mame.ini");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[MameConfig] Created new mame.ini from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException($"mame.ini not found in {emuDir} and sample not available at {samplePath}");
            }
        }

        DebugLogger.Log($"[MameConfig] Injecting configuration into: {configPath}");

        // Prepare the settings dictionary
        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "video", settings.MameVideo },
            { "window", settings.MameWindow ? "1" : "0" },
            { "maximize", settings.MameMaximize ? "1" : "0" },
            { "keepaspect", settings.MameKeepAspect ? "1" : "0" },
            { "skip_gameinfo", settings.MameSkipGameInfo ? "1" : "0" },
            { "autosave", settings.MameAutosave ? "1" : "0" },
            { "confirm_quit", settings.MameConfirmQuit ? "1" : "0" },
            { "joystick", settings.MameJoystick ? "1" : "0" },
            { "autoframeskip", settings.MameAutoframeskip ? "1" : "0" },
            { "bgfx_backend", settings.MameBgfxBackend },
            { "bgfx_screen_chains", settings.MameBgfxScreenChains },
            { "filter", settings.MameFilter ? "1" : "0" },
            { "cheat", settings.MameCheat ? "1" : "0" },
            { "rewind", settings.MameRewind ? "1" : "0" },
            { "nvram_save", settings.MameNvramSave ? "1" : "0" }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;
        var keysFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmedLine = line.TrimStart();
            if (string.IsNullOrWhiteSpace(line) || trimmedLine.StartsWith('#')) continue;

            // MAME INI format: key <whitespace> value
            var match = MyRegex().Match(trimmedLine);
            if (!match.Success) continue;

            var key = match.Groups[1].Value;
            var whitespaceBetween = match.Groups[2].Value;
            var trailingPart = match.Groups[4].Value; // Includes any trailing comment

            // Handle standard settings
            if (updates.TryGetValue(key, out var newValue))
            {
                // Preserve leading whitespace and trailing comments
                var leadingWhitespace = line.TakeWhile(char.IsWhiteSpace).ToArray();
                lines[i] = new string(leadingWhitespace) + key + whitespaceBetween + newValue + trailingPart;
                keysFound.Add(key);
                modified = true;
            }
            // Handle rompath specifically to append/inject the system folder
            else if (key.Equals("rompath", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(systemRomPath))
            {
                // Get full paths of existing entries for comparison
                var existingPaths = SplitRomPath(trimmedLine.Substring(key.Length).Trim())
                    .Select(p => NormalizePath(GetFullPathSafe(p, emuDir)))
                    .Where(static p => !string.IsNullOrEmpty(p))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var newFullPath = NormalizePath(GetFullPathSafe(systemRomPath, emuDir));

                if (!string.IsNullOrEmpty(newFullPath) && !existingPaths.Contains(newFullPath))
                {
                    // Quote the new path if it contains semicolons or spaces
                    var pathToAdd = systemRomPath.Trim();
                    if (pathToAdd.Contains(';') || pathToAdd.Contains(' '))
                    {
                        pathToAdd = $"\"{pathToAdd}\"";
                    }

                    // Reconstruct the value preserving the original format where possible
                    var currentRomPathValue = trimmedLine.Substring(key.Length).Trim();
                    var newRomPathValue = string.IsNullOrEmpty(currentRomPathValue)
                        ? pathToAdd
                        : $"{currentRomPathValue};{pathToAdd}";

                    var leadingWhitespace = line.TakeWhile(char.IsWhiteSpace).ToArray();
                    lines[i] = new string(leadingWhitespace) + key + whitespaceBetween + newRomPathValue + trailingPart;
                    modified = true;
                }

                keysFound.Add(key);
            }
        }

        // Add missing keys at the end
        foreach (var kvp in updates)
        {
            if (!keysFound.Contains(kvp.Key))
            {
                lines.Add($"{kvp.Key} {kvp.Value}");
                modified = true;
            }
        }

        // Atomic write to prevent corruption during concurrent access
        if (modified)
        {
            var tempPath = configPath + ".tmp";
            try
            {
                File.WriteAllLines(tempPath, lines, new UTF8Encoding(false));
                File.Move(tempPath, configPath, true);
                DebugLogger.Log("[MameConfig] Injection successful.");
            }
            catch
            {
                // Clean up temp file if it exists
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        /* Ignore cleanup errors */
                    }
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Splits ROM path respecting quoted strings. MAME uses semicolons as separators
    /// but paths containing semicolons or spaces should be quoted.
    /// </summary>
    private static List<string> SplitRomPath(string value)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var c in value)
        {
            switch (c)
            {
                case '"':
                    inQuotes = !inQuotes;
                    // Don't include quotes in the path string
                    break;
                case ';' when !inQuotes:
                {
                    var part = current.ToString().Trim();
                    if (!string.IsNullOrEmpty(part))
                        result.Add(part);
                    current.Clear();
                    break;
                }
                default:
                    current.Append(c);
                    break;
            }
        }

        var lastPart = current.ToString().Trim();
        if (!string.IsNullOrEmpty(lastPart))
            result.Add(lastPart);

        return result;
    }

    /// <summary>
    /// Normalizes path for comparison (trims trailing separators).
    /// Assumes Path.GetFullPath has already been called to resolve relative paths.
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Safely gets full path, returning null if the path is invalid.
    /// </summary>
    private static string GetFullPathSafe(string path, string basePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // Remove quotes if present
            path = path.Trim().Trim('"');

            if (string.IsNullOrWhiteSpace(path))
                return null;

            return Path.GetFullPath(path, basePath);
        }
        catch
        {
            return null;
        }
    }

    [GeneratedRegex(@"^(\S+)(\s+)(\S+)(.*)$")]
    private static partial Regex MyRegex();
}

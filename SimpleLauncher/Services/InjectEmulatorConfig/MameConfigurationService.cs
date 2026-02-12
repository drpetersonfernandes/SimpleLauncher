using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static partial class MameConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, string systemRomPath = null, string[] listOfSecondaryRomPath = null)
    {
        if (string.IsNullOrWhiteSpace(emulatorPath))
            throw new ArgumentException(@"Emulator path cannot be null or empty.", nameof(emulatorPath));

        ArgumentNullException.ThrowIfNull(settings);

        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
        {
            throw new InvalidOperationException("Emulator directory is null or empty.");
        }

        var configPath = Path.Combine(emuDir, "mame.ini");

        // Backup logic: Create from sample if missing
        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "MAME", "mame.ini");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[MameConfig] Created new mame.ini from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[MameConfig] Failed to create mame.ini from sample: {ex.Message}");
                    _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[MameConfig] Failed to create mame.ini from sample: {ex.Message}");
                    throw;
                }
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
            var match = IniLineRegex().Match(trimmedLine);
            if (!match.Success) continue;

            var key = match.Groups[1].Value;
            var whitespaceBetween = match.Groups[2].Value;
            var commentPart = match.Groups[4].Value; // Captures only the #comment

            // Handle standard settings
            if (updates.TryGetValue(key, out var newValue))
            {
                // Preserve leading whitespace and trailing comments
                var leadingWhitespace = line.TakeWhile(char.IsWhiteSpace).ToArray();
                lines[i] = new string(leadingWhitespace) + key + whitespaceBetween + newValue + commentPart;
                keysFound.Add(key);
                modified = true;
            }

            /////////////////////////////////////////////////////////////////////
            // Handle rompath specifically to append/inject the system folder ///
            ////////////////////////////////////////////////////////////////////
            else if (key.Equals("rompath", StringComparison.OrdinalIgnoreCase))
            {
                var existingValue = match.Groups[3].Value;
                var uniqueFullPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var finalPathList = new List<string>();

                // 1. Process existing paths from the INI file, ensuring they exist, are unique and unquoted
                var existingPathsRaw = SplitRomPath(existingValue);
                foreach (var path in existingPathsRaw)
                {
                    var fullPath = NormalizePath(GetFullPathSafe(path, emuDir));
                    if (!string.IsNullOrEmpty(fullPath) && Directory.Exists(fullPath) && uniqueFullPaths.Add(fullPath))
                    {
                        finalPathList.Add(path); // Add the original (but unquoted) path
                    }
                }

                // 2. Process the primary system ROM path
                if (!string.IsNullOrEmpty(systemRomPath))
                {
                    var fullPath = NormalizePath(GetFullPathSafe(systemRomPath, emuDir));
                    if (!string.IsNullOrEmpty(fullPath) && Directory.Exists(fullPath) && uniqueFullPaths.Add(fullPath))
                    {
                        finalPathList.Add(RemoveQuotes(systemRomPath));
                    }
                }

                // 3. Process secondary ROM paths
                if (listOfSecondaryRomPath != null)
                {
                    foreach (var secondaryPath in listOfSecondaryRomPath)
                    {
                        if (string.IsNullOrWhiteSpace(secondaryPath))
                            continue;

                        var resolvedSecondaryPath = CheckPath.IsValidPath(secondaryPath)
                            ? PathHelper.ResolveRelativeToAppDirectory(secondaryPath)
                            : null;

                        if (string.IsNullOrEmpty(resolvedSecondaryPath))
                            continue;

                        var fullPath = NormalizePath(GetFullPathSafe(resolvedSecondaryPath, emuDir));
                        if (!string.IsNullOrEmpty(fullPath) && Directory.Exists(fullPath) && uniqueFullPaths.Add(fullPath))
                        {
                            finalPathList.Add(RemoveQuotes(resolvedSecondaryPath));
                        }
                    }
                }

                // Reconstruct the value with unique, unquoted paths
                var newRomPathValue = string.Join(";", finalPathList);

                var leadingWhitespace = line.TakeWhile(char.IsWhiteSpace).ToArray();
                lines[i] = new string(leadingWhitespace) + key + whitespaceBetween + newRomPathValue + commentPart;
                modified = true;
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

        // Add missing rompath key if not present in the original file
        if (!keysFound.Contains("rompath"))
        {
            var uniqueFullPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var finalPathList = new List<string>();

            // Process the primary system ROM path if it exists
            if (!string.IsNullOrEmpty(systemRomPath))
            {
                var fullPath = NormalizePath(GetFullPathSafe(systemRomPath, emuDir));
                if (!string.IsNullOrEmpty(fullPath) && Directory.Exists(fullPath) && uniqueFullPaths.Add(fullPath))
                {
                    finalPathList.Add(RemoveQuotes(systemRomPath));
                }
            }

            // Process secondary ROM paths
            if (listOfSecondaryRomPath != null)
            {
                foreach (var secondaryPath in listOfSecondaryRomPath)
                {
                    if (string.IsNullOrWhiteSpace(secondaryPath))
                        continue;

                    var resolvedSecondaryPath = CheckPath.IsValidPath(secondaryPath)
                        ? PathHelper.ResolveRelativeToAppDirectory(secondaryPath)
                        : null;

                    if (string.IsNullOrEmpty(resolvedSecondaryPath))
                        continue;

                    var fullPath = NormalizePath(GetFullPathSafe(resolvedSecondaryPath, emuDir));
                    if (!string.IsNullOrEmpty(fullPath) && Directory.Exists(fullPath) && uniqueFullPaths.Add(fullPath))
                    {
                        finalPathList.Add(RemoveQuotes(resolvedSecondaryPath));
                    }
                }
            }

            if (finalPathList.Count > 0)
            {
                var newRomPathValue = string.Join(";", finalPathList);
                lines.Add($"rompath {newRomPathValue}");
                modified = true;
            }
        }
        //////////////////////
        // Finish rom edit ///
        //////////////////////

        // Atomic write to prevent corruption during concurrent access
        if (modified)
        {
            var tempPath = configPath + ".tmp";
            try
            {
                File.WriteAllLines(tempPath, lines, new UTF8Encoding(false));
                File.Move(tempPath, configPath, true);
                DebugLogger.Log("[MameConfig] Injected configuration changes.");
            }
            catch (Exception ex)
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

                DebugLogger.Log("[MameConfig] Failed to inject configuration changes.");
                _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, "[MameConfig] Failed to inject configuration changes.");
                throw;
            }
        }
    }

    /// <summary>
    /// Splits ROM path and removes any existing quotes.
    /// MAME uses semicolons as separators.
    /// </summary>
    private static List<string> SplitRomPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];

        // Split by semicolon and then aggressively remove all quotes from each resulting segment
        return value.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(static p => RemoveQuotes(p))
            .Where(static p => !string.IsNullOrEmpty(p))
            .ToList();
    }

    private static string RemoveQuotes(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Replace("\"", string.Empty).Trim();
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

    [GeneratedRegex(@"^(\S+)(\s+)([^#\r\n]*)(#.*)?$")]
    private static partial Regex IniLineRegex();
}
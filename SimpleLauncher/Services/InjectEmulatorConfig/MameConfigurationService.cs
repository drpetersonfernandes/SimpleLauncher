using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static partial class MameConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, ILogErrors logErrors, string systemRomPath = null, string[] listOfSecondaryRomPath = null)
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
                    logErrors.LogAndForget(ex, $"[MameConfig] Failed to create mame.ini from sample: {ex.Message}");
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
            { "video", settings.Mame.Video },
            { "window", settings.Mame.Window ? "1" : "0" },
            { "maximize", settings.Mame.Maximize ? "1" : "0" },
            { "keepaspect", settings.Mame.KeepAspect ? "1" : "0" },
            { "skip_gameinfo", settings.Mame.SkipGameInfo ? "1" : "0" },
            { "autosave", settings.Mame.Autosave ? "1" : "0" },
            { "confirm_quit", settings.Mame.ConfirmQuit ? "1" : "0" },
            { "joystick", settings.Mame.Joystick ? "1" : "0" },
            { "autoframeskip", settings.Mame.Autoframeskip ? "1" : "0" },
            { "bgfx_backend", settings.Mame.BgfxBackend },
            { "bgfx_screen_chains", settings.Mame.BgfxScreenChains },
            { "filter", settings.Mame.Filter ? "1" : "0" },
            { "cheat", settings.Mame.Cheat ? "1" : "0" },
            { "rewind", settings.Mame.Rewind ? "1" : "0" },
            { "nvram_save", settings.Mame.NvramSave ? "1" : "0" }
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
                logErrors.LogAndForget(ex, "[MameConfig] Failed to inject configuration changes.");
                throw;
            }
        }
    }

    /// <summary>
    /// Restores mame.ini from the bundled sample.
    /// Backs up the existing file to mame.ini.bak before overwriting.
    /// </summary>
    public static bool RestoreMameIniFromSample(string emulatorPath, ILogErrors logErrors)
    {
        if (string.IsNullOrWhiteSpace(emulatorPath))
            return false;

        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            return false;

        var configPath = Path.Combine(emuDir, "mame.ini");
        var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "MAME", "mame.ini");

        if (!File.Exists(samplePath))
        {
            DebugLogger.Log($"[MameConfig] Sample mame.ini not found at: {samplePath}");
            return false;
        }

        try
        {
            if (File.Exists(configPath))
            {
                var backupPath = configPath + ".bak";
                File.Move(configPath, backupPath, true);
                DebugLogger.Log($"[MameConfig] Backed up existing mame.ini to: {backupPath}");
            }

            File.Copy(samplePath, configPath, true);
            DebugLogger.Log($"[MameConfig] Restored mame.ini from sample: {configPath}");
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MameConfig] Failed to restore mame.ini from sample: {ex.Message}");
            logErrors.LogAndForget(ex, $"[MameConfig] Failed to restore mame.ini from sample: {ex.Message}");
            return false;
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
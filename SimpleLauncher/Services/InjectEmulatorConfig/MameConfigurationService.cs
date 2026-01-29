using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Managers;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class MameConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager settings, string systemRomPath = null)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory is null or empty.");

        var configPath = Path.Combine(emuDir, "mame.ini");

        if (!File.Exists(configPath))
            throw new FileNotFoundException($"mame.ini not found in {emuDir}");

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
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

            // MAME INI format: key <whitespace> value
            var parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 1) continue;

            var key = parts[0];

            // Handle standard settings
            if (updates.TryGetValue(key, out var newValue))
            {
                // Reconstruct line preserving key, adding spacing, and new value
                lines[i] = $"{key,-25} {newValue}";
                keysFound.Add(key);
                modified = true;
            }
            // Handle rompath specifically to append/inject the system folder
            else if (key.Equals("rompath", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(systemRomPath))
            {
                // Get current value (everything after the key)
                var currentPaths = parts.Length > 1 ? line.Substring(key.Length).Trim() : "";

                // Check if our system path is already in there
                if (!currentPaths.Contains(systemRomPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Append the new path. MAME uses semicolon for separation on Windows.
                    var newPaths = string.IsNullOrEmpty(currentPaths) ? systemRomPath : $"{currentPaths};{systemRomPath}";
                    lines[i] = $"{key,-25} {newPaths}";
                    modified = true;
                }

                keysFound.Add(key);
            }
        }

        foreach (var kvp in updates)
        {
            if (!keysFound.Contains(kvp.Key))
            {
                lines.Add($"{kvp.Key,-25} {kvp.Value}");
                modified = true;
            }
        }

        // If we modified the file, write it back
        if (modified)
        {
            File.WriteAllLines(configPath, lines, Encoding.UTF8);
            DebugLogger.Log("[MameConfig] Injection successful.");
        }
    }
}
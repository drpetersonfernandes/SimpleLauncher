using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class RetroArchConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory is null or empty.");

        var configPath = Path.Combine(emuDir, "retroarch.cfg");

        // Backup logic: Create from sample if missing
        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Retroarch", "retroarch.cfg");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[RetroArchConfig] Created new retroarch.cfg from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException($"retroarch.cfg not found in {emuDir} and sample not available at {samplePath}");
            }
        }

        DebugLogger.Log($"[RetroArchConfig] Injecting configuration into: {configPath}");

        // Define base directories for portability (relative to Simple Launcher)
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var biosDir = Path.Combine(baseDir, "bios");
        var savesDir = Path.Combine(baseDir, "saves");
        var statesDir = Path.Combine(baseDir, "states");
        var screenshotsDir = Path.Combine(baseDir, "screenshots");

        // Ensure directories exist
        Directory.CreateDirectory(biosDir);
        Directory.CreateDirectory(savesDir);
        Directory.CreateDirectory(statesDir);
        Directory.CreateDirectory(screenshotsDir);

        // Prepare settings dictionary
        var updates = new Dictionary<string, string>
        {
            // --- Video ---
            { "video_fullscreen", FormatBool(settings.RetroArchFullscreen) },
            { "video_vsync", FormatBool(settings.RetroArchVsync) },
            { "video_driver", FormatString(settings.RetroArchVideoDriver) },
            { "video_threaded", FormatBool(settings.RetroArchThreadedVideo) },
            { "video_smooth", FormatBool(settings.RetroArchBilinear) },
            { "video_aspect_ratio_index", FormatString(settings.RetroArchAspectRatioIndex) },
            { "video_scale_integer", FormatBool(settings.RetroArchScaleInteger) },
            { "video_shader_enable", FormatBool(settings.RetroArchShaderEnable) },
            { "video_hard_sync", FormatBool(settings.RetroArchHardSync) },

            // --- Audio ---
            { "audio_enable", FormatBool(settings.RetroArchAudioEnable) },
            { "audio_mute_enable", FormatBool(settings.RetroArchAudioMute) },

            // --- Directories (Portability) ---
            { "system_directory", FormatPath(biosDir) },
            { "savefile_directory", FormatPath(savesDir) },
            { "savestate_directory", FormatPath(statesDir) },
            { "screenshot_directory", FormatPath(screenshotsDir) },

            // --- Automation / Misc ---
            { "pause_nonactive", FormatBool(settings.RetroArchPauseNonActive) },
            { "config_save_on_exit", FormatBool(settings.RetroArchSaveOnExit) },
            { "savestate_auto_save", FormatBool(settings.RetroArchAutoSaveState) },
            { "savestate_auto_load", FormatBool(settings.RetroArchAutoLoadState) },
            { "rewind_enable", FormatBool(settings.RetroArchRewind) },
            { "run_ahead_enabled", FormatBool(settings.RetroArchRunAhead) },
            { "discord_allow", FormatBool(settings.RetroArchDiscordAllow) },

            // --- UI ---
            { "menu_driver", FormatString(settings.RetroArchMenuDriver) },
            { "menu_show_advanced_settings", FormatBool(settings.RetroArchShowAdvancedSettings) },

            // --- RetroAchievements ---
            { "cheevos_enable", FormatBool(settings.RetroArchCheevosEnable) },
            { "cheevos_hardcore_mode_enable", FormatBool(settings.RetroArchCheevosHardcore) }
        };

        // --- Directories (Conditional Portability) ---
        if (settings.RetroArchOverrideSystemDir)
        {
            updates.Add("system_directory", FormatPath(Path.Combine(baseDir, "bios")));
        }

        if (settings.RetroArchOverrideSaveDir)
        {
            updates.Add("savefile_directory", FormatPath(Path.Combine(baseDir, "saves")));
        }

        if (settings.RetroArchOverrideStateDir)
        {
            updates.Add("savestate_directory", FormatPath(Path.Combine(baseDir, "states")));
        }

        if (settings.RetroArchOverrideScreenshotDir)
        {
            updates.Add("screenshot_directory", FormatPath(Path.Combine(baseDir, "screenshots")));
        }

        // Read and Update
        var lines = File.ReadAllLines(configPath).ToList();
        var keysFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;

            // RetroArch config is "key = value"
            var parts = line.Split('=', 2);
            if (parts.Length < 1) continue;

            var key = parts[0].Trim();

            if (updates.TryGetValue(key, out var newValue))
            {
                lines[i] = $"{key} = {newValue}";
                keysFound.Add(key);
            }
        }

        // Append missing keys
        foreach (var kvp in updates)
        {
            if (!keysFound.Contains(kvp.Key))
            {
                lines.Add($"{kvp.Key} = {kvp.Value}");
            }
        }

        File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
        DebugLogger.Log("[RetroArchConfig] Injection successful.");
        return;

        // Helper methods to properly format values for RetroArch config
        // RetroArch requires string values to be wrapped in double quotes
        // These methods prevent double-quoting by stripping existing quotes first

        static string FormatString(string val)
        {
            if (string.IsNullOrEmpty(val))
                return "\"\"";

            // Strip existing surrounding quotes to prevent double-quoting
            val = val.Trim();
            if (val.Length >= 2 && val.StartsWith('"') && val.EndsWith('"'))
            {
                val = val.Substring(1, val.Length - 2);
            }

            // Escape any internal quotes and wrap in quotes
            val = val.Replace("\"", "\\\"");
            return $"\"{val}\"";
        }

        static string FormatBool(bool val)
        {
            // Booleans in RetroArch are quoted strings: "true" or "false"
            return val ? "\"true\"" : "\"false\"";
        }

        static string FormatPath(string path)
        {
            // Normalize path separators and format as string
            return FormatString(path.Replace("\\", "/"));
        }
    }
}
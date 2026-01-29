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

        if (!File.Exists(configPath))
            throw new FileNotFoundException($"retroarch.cfg not found in {emuDir}");

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
            { "video_fullscreen", BoolQuote(settings.RetroArchFullscreen) },
            { "video_vsync", BoolQuote(settings.RetroArchVsync) },
            { "video_driver", Quote(settings.RetroArchVideoDriver) },
            { "video_threaded", BoolQuote(settings.RetroArchThreadedVideo) },
            { "video_smooth", BoolQuote(settings.RetroArchBilinear) },
            { "video_aspect_ratio_index", Quote(settings.RetroArchAspectRatioIndex) },
            { "video_scale_integer", BoolQuote(settings.RetroArchScaleInteger) },
            { "video_shader_enable", BoolQuote(settings.RetroArchShaderEnable) },
            { "video_hard_sync", BoolQuote(settings.RetroArchHardSync) },

            // --- Audio ---
            { "audio_enable", BoolQuote(settings.RetroArchAudioEnable) },
            { "audio_mute_enable", BoolQuote(settings.RetroArchAudioMute) },

            // --- Directories (Portability) ---
            { "system_directory", Quote(biosDir.Replace("\\", "/")) },
            { "savefile_directory", Quote(savesDir.Replace("\\", "/")) },
            { "savestate_directory", Quote(statesDir.Replace("\\", "/")) },
            { "screenshot_directory", Quote(screenshotsDir.Replace("\\", "/")) },

            // --- Automation / Misc ---
            { "pause_nonactive", BoolQuote(settings.RetroArchPauseNonActive) },
            { "config_save_on_exit", BoolQuote(settings.RetroArchSaveOnExit) },
            { "savestate_auto_save", BoolQuote(settings.RetroArchAutoSaveState) },
            { "savestate_auto_load", BoolQuote(settings.RetroArchAutoLoadState) },
            { "rewind_enable", BoolQuote(settings.RetroArchRewind) },
            { "run_ahead_enabled", BoolQuote(settings.RetroArchRunAhead) },
            { "discord_allow", BoolQuote(settings.RetroArchDiscordAllow) },

            // --- UI ---
            { "menu_driver", Quote(settings.RetroArchMenuDriver) },
            { "menu_show_advanced_settings", BoolQuote(settings.RetroArchShowAdvancedSettings) },

            // --- RetroAchievements ---
            { "cheevos_enable", BoolQuote(settings.RetroArchCheevosEnable) },
            { "cheevos_hardcore_mode_enable", BoolQuote(settings.RetroArchCheevosHardcore) }
        };

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

        // BUG FIX: RetroArch requires values to be wrapped in double quotes
        static string Quote(string val)
        {
            return $"\"{val}\"";
        }

        static string BoolQuote(bool val)
        {
            return Quote(val ? "true" : "false");
        }
    }
}
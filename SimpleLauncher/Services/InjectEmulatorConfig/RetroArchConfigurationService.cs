using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class RetroArchConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager settings)
    {
        try
        {
            var emuDir = Path.GetDirectoryName(emulatorPath);
            if (string.IsNullOrEmpty(emuDir)) return;

            var configPath = Path.Combine(emuDir, "retroarch.cfg");

            // If config doesn't exist, we can't edit it. RA usually creates it on first run.
            if (!File.Exists(configPath))
            {
                DebugLogger.Log("[RetroArchConfig] retroarch.cfg not found. Skipping injection.");
                return;
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
            // Note: RetroArch expects strings in quotes, bools/numbers without.
            var updates = new Dictionary<string, string>
            {
                // --- Video ---
                { "video_fullscreen", settings.RetroArchFullscreen ? "true" : "false" },
                { "video_vsync", settings.RetroArchVsync ? "true" : "false" },
                { "video_driver", $"\"{settings.RetroArchVideoDriver}\"" },
                { "video_aspect_ratio_auto", "true" },
                { "video_threaded", settings.RetroArchThreadedVideo ? "true" : "false" },
                { "video_smooth", settings.RetroArchBilinear ? "true" : "false" },
                { "video_crop_overscan", "true" },
                { "video_shader_enable", "true" }, // Generally enable to allow per-core overrides to work

                // --- Audio ---
                { "audio_enable", settings.RetroArchAudioEnable ? "true" : "false" },
                { "audio_mute_enable", settings.RetroArchAudioMute ? "true" : "false" },

                // --- Directories (Portability) ---
                // We use forward slashes for compatibility or escaped backslashes
                { "system_directory", $"\"{biosDir.Replace("\\", "/")}\"" },
                { "savefile_directory", $"\"{savesDir.Replace("\\", "/")}\"" },
                { "savestate_directory", $"\"{statesDir.Replace("\\", "/")}\"" },
                { "screenshot_directory", $"\"{screenshotsDir.Replace("\\", "/")}\"" },

                // --- Automation / Misc ---
                { "pause_nonactive", settings.RetroArchPauseNonActive ? "true" : "false" },
                { "config_save_on_exit", settings.RetroArchSaveOnExit ? "true" : "false" },
                { "savestate_auto_save", settings.RetroArchAutoSaveState ? "true" : "false" },
                { "savestate_auto_load", settings.RetroArchAutoLoadState ? "true" : "false" },
                { "rewind_enable", settings.RetroArchRewind ? "true" : "false" },
                { "content_runtime_log", "true" },

                // --- UI ---
                { "menu_driver", $"\"{settings.RetroArchMenuDriver}\"" },

                // --- RetroAchievements ---
                // We inject credentials from the main SettingsManager if enabled
                { "cheevos_enable", settings.RetroArchCheevosEnable ? "true" : "false" },
                { "cheevos_hardcore_mode_enable", settings.RetroArchCheevosHardcore ? "true" : "false" }
            };

            // Only inject credentials if RA is enabled to avoid clearing them if user disabled it temporarily
            if (settings.RetroArchCheevosEnable)
            {
                updates["cheevos_username"] = $"\"{settings.RaUsername}\"";
                // Prefer Token, fallback to Password
                var secret = !string.IsNullOrEmpty(settings.RaToken) ? settings.RaToken : settings.RaPassword;
                updates["cheevos_password"] = $"\"{secret}\"";
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
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[RetroArchConfig] Failed to inject settings: {ex.Message}");
        }
    }
}

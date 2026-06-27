using System.Text;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

using Interfaces;

public static class RetroArchConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, ILogErrors logErrors, IDebugLogger debugLogger)
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
                try
                {
                    File.Copy(samplePath, configPath);
                    debugLogger.Log($"[RetroArchConfig] Created new retroarch.cfg from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    debugLogger.Log($"[RetroArchConfig] Failed to create retroarch.cfg from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[RetroArchConfig] Failed to create retroarch.cfg from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException($"retroarch.cfg not found in {emuDir} and sample not available at {samplePath}");
            }
        }

        debugLogger.Log($"[RetroArchConfig] Injecting configuration into: {configPath}");

        // Prepare settings dictionary
        var updates = new Dictionary<string, string>
        {
            // --- Video ---
            { "video_fullscreen", FormatBool(settings.RetroArch.Fullscreen) },
            { "video_vsync", FormatBool(settings.RetroArch.Vsync) },
            { "video_driver", FormatString(settings.RetroArch.VideoDriver) },
            { "video_threaded", FormatBool(settings.RetroArch.ThreadedVideo) },
            { "video_smooth", FormatBool(settings.RetroArch.Bilinear) },
            { "video_aspect_ratio_index", FormatString(settings.RetroArch.AspectRatioIndex) },
            { "video_scale_integer", FormatBool(settings.RetroArch.ScaleInteger) },
            { "video_shader_enable", FormatBool(settings.RetroArch.ShaderEnable) },
            { "video_hard_sync", FormatBool(settings.RetroArch.HardSync) },

            // --- Audio ---
            { "audio_enable", FormatBool(settings.RetroArch.AudioEnable) },
            { "audio_mute_enable", FormatBool(settings.RetroArch.AudioMute) },

            // --- Automation / Misc ---
            { "pause_nonactive", FormatBool(settings.RetroArch.PauseNonActive) },
            { "config_save_on_exit", FormatBool(settings.RetroArch.SaveOnExit) },
            { "savestate_auto_save", FormatBool(settings.RetroArch.AutoSaveState) },
            { "savestate_auto_load", FormatBool(settings.RetroArch.AutoLoadState) },
            { "rewind_enable", FormatBool(settings.RetroArch.Rewind) },
            { "run_ahead_enabled", FormatBool(settings.RetroArch.RunAhead) },
            { "discord_allow", FormatBool(settings.RetroArch.DiscordAllow) },

            // --- UI ---
            { "menu_driver", FormatString(settings.RetroArch.MenuDriver) },
            { "menu_show_advanced_settings", FormatBool(settings.RetroArch.ShowAdvancedSettings) },

            // --- RetroAchievements ---
            { "cheevos_enable", FormatBool(settings.RetroArch.CheevosEnable) },
            { "cheevos_hardcore_mode_enable", FormatBool(settings.RetroArch.CheevosHardcore) }
        };

        // Read and Update
        List<string> lines;
        try
        {
            lines = File.ReadAllLines(configPath).ToList();
        }
        catch (UnauthorizedAccessException ex)
        {
            debugLogger.Log($"[RetroArchConfig] Access denied reading config: {configPath}");
            logErrors.LogAndForget(ex, $"[RetroArchConfig] Access denied reading config: {configPath}");
            throw;
        }
        catch (IOException ex)
        {
            debugLogger.Log($"[RetroArchConfig] I/O error reading config: {configPath}");
            logErrors.LogAndForget(ex, $"[RetroArchConfig] I/O error reading config: {configPath}");
            throw;
        }

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

        try
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            debugLogger.Log("[RetroArchConfig] Injected configuration changes..");
        }
        catch (Exception ex)
        {
            debugLogger.Log($"[RetroArchConfig] Failed to inject configuration changes: {ex.Message}");
            logErrors.LogAndForget(ex, $"[RetroArchConfig] Failed to inject configuration changes: {ex.Message}");
            throw;
        }

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
    }
}

using System.Text;

namespace SimpleLauncher.Core.Services.InjectEmulatorConfig;

/// <summary>
/// Provides functionality to inject Simple Launcher settings into the RetroArch emulator configuration file (retroarch.cfg).
/// </summary>
public static class RetroArchConfigurationService
{
    /// <summary>
    /// Injects Simple Launcher configuration settings into the RetroArch emulator's retroarch.cfg file.
    /// Creates the config from a sample if it does not exist, then updates video, audio, automation, UI, and RetroAchievements settings.
    /// </summary>
    /// <param name="emulatorPath">The full path to the RetroArch emulator executable.</param>
    /// <param name="settings">The settings manager containing RetroArch configuration values.</param>
    /// <param name="logger">The logger instance for diagnostic output.</param>
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManagerService settings,
        ILogger logger)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory is null or empty.");

        var configPath = Path.Combine(emuDir, "retroarch.cfg");

        // Backup logic: Create from sample if missing
        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Retroarch",
                "retroarch.cfg");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    logger.Debug($"[RetroArchConfig] Created new retroarch.cfg from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    logger.Debug($"[RetroArchConfig] Failed to create retroarch.cfg from sample: {ex.Message}");
                    logger.Error(ex, $"[RetroArchConfig] Failed to create retroarch.cfg from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException(
                    $"retroarch.cfg not found in {emuDir} and sample not available at {samplePath}");
            }
        }

        logger.Debug($"[RetroArchConfig] Injecting configuration into: {configPath}");

        // Prepare settings dictionary
        var updates = new Dictionary<string, string>(StringComparer.Ordinal)
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
            logger.Debug($"[RetroArchConfig] Access denied reading config: {configPath}");
            logger.Error(ex, $"[RetroArchConfig] Access denied reading config: {configPath}");
            throw;
        }
        catch (IOException ex)
        {
            logger.Debug($"[RetroArchConfig] I/O error reading config: {configPath}");
            logger.Error(ex, $"[RetroArchConfig] I/O error reading config: {configPath}");
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
            logger.Debug("[RetroArchConfig] Injected configuration changes..");
        }
        catch (Exception ex)
        {
            logger.Debug($"[RetroArchConfig] Failed to inject configuration changes: {ex.Message}");
            logger.Error(ex, $"[RetroArchConfig] Failed to inject configuration changes: {ex.Message}");
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
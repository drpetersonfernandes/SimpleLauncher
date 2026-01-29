using System;
using System.IO;
using SimpleLauncher.Managers;
using Tomlyn;
using Tomlyn.Model;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class XeniaConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager settings)
    {
        try
        {
            var emuDir = Path.GetDirectoryName(emulatorPath);
            if (string.IsNullOrEmpty(emuDir)) return;

            // Define all possible config filenames
            string[] configFiles = ["xenia-canary.config.toml", "xenia.config.toml"];
            var foundAny = false;

            foreach (var fileName in configFiles)
            {
                var configPath = Path.Combine(emuDir, fileName);
                if (!File.Exists(configPath)) continue;

                foundAny = true;
                UpdateSingleConfigFile(configPath, settings);
            }

            if (!foundAny)
            {
                DebugLogger.Log("[XeniaConfig] No configuration file found to inject settings into.");
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[XeniaConfig] Critical failure in injection service: {ex.Message}");
        }
    }

    private static void UpdateSingleConfigFile(string configPath, SettingsManager settings)
    {
        try
        {
            DebugLogger.Log($"[XeniaConfig] Injecting into: {Path.GetFileName(configPath)}");

            var tomlContent = File.ReadAllText(configPath);
            var model = Toml.ToModel(tomlContent);

            // Helper to get or create a table (section)
            TomlTable GetOrCreateTable(string key)
            {
                if (model.ContainsKey(key) && model[key] is TomlTable table)
                    return table;

                var newTable = new TomlTable();
                model[key] = newTable;
                return newTable;
            }

            // [APU]
            var apu = GetOrCreateTable("APU");
            apu["apu"] = settings.XeniaApu;
            apu["mute"] = settings.XeniaMute;

            // [GPU]
            var gpu = GetOrCreateTable("GPU");
            gpu["gpu"] = settings.XeniaGpu;
            gpu["vsync"] = settings.XeniaVsync;
            gpu["draw_resolution_scale_x"] = settings.XeniaResScaleX;
            gpu["draw_resolution_scale_y"] = settings.XeniaResScaleY;
            gpu["readback_resolve"] = settings.XeniaReadbackResolve; // New
            gpu["gamma_render_target_as_srgb"] = settings.XeniaGammaSrgb; // New

            // [Display]
            var display = GetOrCreateTable("Display");
            display["fullscreen"] = settings.XeniaFullscreen;
            display["postprocess_antialiasing"] = settings.XeniaAa;
            display["postprocess_scaling_and_sharpening"] = settings.XeniaScaling;

            // [HID]
            var hid = GetOrCreateTable("HID");
            hid["hid"] = settings.XeniaHid;
            hid["vibration"] = settings.XeniaVibration; // New

            // [Kernel]
            var kernel = GetOrCreateTable("Kernel");
            kernel["apply_patches"] = settings.XeniaApplyPatches;

            // [General]
            var general = GetOrCreateTable("General");
            general["discord"] = settings.XeniaDiscordPresence;

            // [Logging]
            var logging = GetOrCreateTable("Logging");
            logging["enable_console"] = false;

            // [Storage]
            var storage = GetOrCreateTable("Storage");
            storage["mount_cache"] = settings.XeniaMountCache; // New

            // [XConfig]
            var xconfig = GetOrCreateTable("XConfig");
            xconfig["user_language"] = settings.XeniaUserLanguage;

            // Write back
            var updatedToml = Toml.FromModel(model);
            File.WriteAllText(configPath, updatedToml);
            DebugLogger.Log($"[XeniaConfig] Successfully updated {Path.GetFileName(configPath)}");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[XeniaConfig] Failed to update {configPath}: {ex.Message}");
        }
    }
}
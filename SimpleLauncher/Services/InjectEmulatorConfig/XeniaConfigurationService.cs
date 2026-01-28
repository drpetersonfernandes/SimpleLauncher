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

            // Xenia Manager logic: Check for canary config first, then standard config
            var configPath = Path.Combine(emuDir, "xenia-canary.config.toml");
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(emuDir, "xenia.config.toml");
            }

            if (!File.Exists(configPath))
            {
                DebugLogger.Log("[XeniaConfig] No configuration file found to inject settings into.");
                return;
            }

            DebugLogger.Log($"[XeniaConfig] Injecting configuration into: {configPath}");

            // 1. Parse the existing TOML file
            var tomlContent = File.ReadAllText(configPath);
            var model = Toml.ToModel(tomlContent);

            // 2. Update values based on SettingsManager
            // Helper to safely get a table (section)
            TomlTable GetTable(string key)
            {
                if (model.ContainsKey(key) && model[key] is TomlTable table)
                    return table;

                return null;
            }

            // [APU]
            var apu = GetTable("APU");
            if (apu != null)
            {
                apu["apu"] = settings.XeniaApu;
                apu["mute"] = settings.XeniaMute;
            }

            // [GPU]
            var gpu = GetTable("GPU");
            if (gpu != null)
            {
                gpu["gpu"] = settings.XeniaGpu;
                gpu["vsync"] = settings.XeniaVsync;
                gpu["draw_resolution_scale_x"] = settings.XeniaResScaleX;
                gpu["draw_resolution_scale_y"] = settings.XeniaResScaleY;
            }

            // [Display]
            var display = GetTable("Display");
            if (display != null)
            {
                display["fullscreen"] = settings.XeniaFullscreen;
                display["postprocess_antialiasing"] = settings.XeniaAa;
                display["postprocess_scaling_and_sharpening"] = settings.XeniaScaling;
            }

            // [HID]
            var hid = GetTable("HID");
            if (hid != null)
            {
                hid["hid"] = settings.XeniaHid;
            }

            // [Kernel]
            var kernel = GetTable("Kernel");
            if (kernel != null)
            {
                kernel["apply_patches"] = settings.XeniaApplyPatches;
            }

            // [Logging]
            var logging = GetTable("Logging");
            if (logging != null)
            {
                logging["show_console"] = false;
            }

            // [XConfig]
            var xconfig = GetTable("XConfig");
            if (xconfig != null)
            {
                xconfig["user_language"] = settings.XeniaUserLanguage;
            }

            // 3. Write back to disk
            var updatedToml = Toml.FromModel(model);
            File.WriteAllText(configPath, updatedToml);

            DebugLogger.Log("[XeniaConfig] Injection successful.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[XeniaConfig] Failed to inject settings: {ex.Message}");
            // We do not throw here to allow the game to attempt launch even if config fails
        }
    }
}

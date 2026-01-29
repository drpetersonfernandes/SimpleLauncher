using System;
using System.IO;
using SimpleLauncher.Services.DebugAndBugReport;
using Tomlyn;
using Tomlyn.Model;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class XeniaConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory is null or empty.");

        // Define all possible config filenames
        string[] configFiles = ["xenia-canary.config.toml", "xenia.config.toml"];
        var processedCount = 0;

        foreach (var fileName in configFiles)
        {
            var configPath = Path.Combine(emuDir, fileName);
            if (!File.Exists(configPath)) continue;

            if (UpdateSingleConfigFile(configPath, settings))
            {
                processedCount++;
            }
        }

        if (processedCount == 0)
            throw new FileNotFoundException("No xenia configuration files found to inject into.");
    }

    private static bool UpdateSingleConfigFile(string configPath, SettingsManager.SettingsManager settings)
    {
        DebugLogger.Log($"[XeniaConfig] Injecting into: {Path.GetFileName(configPath)}");

        var tomlContent = File.ReadAllText(configPath);
        var model = Toml.ToModel(tomlContent);

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
        return true;

        // Helper to get or create a table (section)
        TomlTable GetOrCreateTable(string key)
        {
            if (model.ContainsKey(key) && model[key] is TomlTable table)
                return table;

            var newTable = new TomlTable();
            model[key] = newTable;
            return newTable;
        }
    }
}
using System.IO;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.DebugAndBugReport;
using Tomlyn;
using Tomlyn.Model;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class XeniaConfigurationService
{
    public static void InjectSettings(string emulatorPath, Core.Services.SettingsManager.SettingsManager settings, ILogErrors logErrors)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory is null or empty.");

        // Define all possible config filenames
        string[] configFiles = ["xenia-canary.config.toml", "xenia.config.toml"];
        var processedCount = 0;

        foreach (var fileName in configFiles)
        {
            // 1. Try portable path first (emulator directory)
            var configPath = Path.Combine(emuDir, fileName);

            // 2. If not found in portable location, try Documents folder (standard Xenia location)
            if (!File.Exists(configPath))
            {
                var documentsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Xenia",
                    fileName);
                if (File.Exists(documentsPath))
                {
                    configPath = documentsPath;
                }
            }

            // The UpdateSingleConfigFile now handles creation from sample if missing.
            // So we don't need to check File.Exists(configPath) here anymore,
            // as it will attempt to create it if not found.
            if (UpdateSingleConfigFile(configPath, settings, logErrors))
            {
                processedCount++;
            }
        }

        if (processedCount == 0)
        {
            // Log the issue instead of throwing to prevent crash when samples are missing
            // or no config files exist. Xenia will use its default settings.
            DebugLogger.Log("[XeniaConfig] WARNING: No configuration files found to inject into. Expected xenia.config.toml or xenia-canary.config.toml in emulator directory or Documents\\Xenia. Xenia will use default settings.");
        }
    }

    private static bool UpdateSingleConfigFile(string configPath, Core.Services.SettingsManager.SettingsManager settings, ILogErrors logErrors)
    {
        // Backup logic: Create from sample if missing
        if (!File.Exists(configPath))
        {
            var fileName = Path.GetFileName(configPath);
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", fileName);
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[XeniaConfig] Created new {fileName} from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[XeniaConfig] Failed to create {fileName} from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[XeniaConfig] Failed to create {fileName} from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                DebugLogger.Log($"[XeniaConfig] Sample not found for {fileName}, skipping: {samplePath}");
                return false;
            }
        }

        DebugLogger.Log($"[XeniaConfig] Injecting into: {Path.GetFileName(configPath)}");

        var tomlContent = File.ReadAllText(configPath);
        var model = TomlSerializer.Deserialize<TomlTable>(tomlContent) ?? new TomlTable();

        // [APU]
        var apu = GetOrCreateTable("APU");
        apu["apu"] = settings.Xenia.Apu;
        apu["mute"] = settings.Xenia.Mute;

        // [GPU]
        var gpu = GetOrCreateTable("GPU");
        gpu["gpu"] = settings.Xenia.Gpu;
        gpu["vsync"] = settings.Xenia.Vsync;
        gpu["draw_resolution_scale_x"] = settings.Xenia.ResScaleX;
        gpu["draw_resolution_scale_y"] = settings.Xenia.ResScaleY;
        gpu["readback_resolve"] = settings.Xenia.ReadbackResolve;
        gpu["gamma_render_target_as_srgb"] = settings.Xenia.GammaSrgb;

        // [Display]
        var display = GetOrCreateTable("Display");
        display["fullscreen"] = settings.Xenia.Fullscreen;
        display["postprocess_antialiasing"] = settings.Xenia.Aa;
        display["postprocess_scaling_and_sharpening"] = settings.Xenia.Scaling;

        // [HID]
        var hid = GetOrCreateTable("HID");
        hid["hid"] = settings.Xenia.Hid;
        hid["vibration"] = settings.Xenia.Vibration;

        // [General]
        var general = GetOrCreateTable("General");
        general["discord"] = settings.Xenia.DiscordPresence;
        general["apply_patches"] = settings.Xenia.ApplyPatches;

        // [Logging]
        var logging = GetOrCreateTable("Logging");
        logging["enable_console"] = false;

        // [Storage]
        var storage = GetOrCreateTable("Storage");
        storage["mount_cache"] = settings.Xenia.MountCache;

        // [XConfig]
        var xconfig = GetOrCreateTable("XConfig");
        xconfig["user_language"] = settings.Xenia.UserLanguage;

        // Write back
        var updatedToml = TomlSerializer.Serialize(model);
        try
        {
            File.WriteAllText(configPath, updatedToml);
            DebugLogger.Log("[XeniaConfig] Injected configuration changes.");
            return true;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[XeniaConfig] Failed to inject configuration changes: {ex.Message}");
            logErrors.LogAndForget(ex, $"[XeniaConfig] Failed to inject configuration changes: {ex.Message}");
            throw;
        }

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
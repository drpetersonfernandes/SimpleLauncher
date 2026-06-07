using System.IO;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.DebugAndBugReport;
using Tomlyn;
using Tomlyn.Model;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class YumirConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, ILogErrors logErrors)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "Ymir.toml");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Yumir", "Ymir.toml");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[YumirConfig] Created new Ymir.toml from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[YumirConfig] Failed to create Ymir.toml from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[YumirConfig] Failed to create Ymir.toml from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("Ymir.toml not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[YumirConfig] Injecting configuration into: {configPath}");

        var tomlContent = File.ReadAllText(configPath);
        var model = TomlSerializer.Deserialize<TomlTable>(tomlContent) ?? new TomlTable();

        // [Video]
        var video = GetOrCreateTable(model, "Video");
        video["FullScreen"] = settings.Yumir.Fullscreen;
        video["ForceAspectRatio"] = settings.Yumir.ForceAspectRatio;
        video["ForcedAspect"] = settings.Yumir.ForcedAspect;
        video["ReduceLatency"] = settings.Yumir.ReduceLatency;

        // [Audio]
        var audio = GetOrCreateTable(model, "Audio");
        audio["Volume"] = settings.Yumir.Volume;
        audio["Mute"] = settings.Yumir.Mute;

        // [System]
        var system = GetOrCreateTable(model, "System");
        system["VideoStandard"] = settings.Yumir.VideoStandard;
        system["AutoDetectRegion"] = settings.Yumir.AutoDetectRegion;

        // [General]
        var general = GetOrCreateTable(model, "General");
        general["PauseWhenUnfocused"] = settings.Yumir.PauseWhenUnfocused;

        var updatedToml = TomlSerializer.Serialize(model);
        try
        {
            File.WriteAllText(configPath, updatedToml);
            DebugLogger.Log("[YumirConfig] Injected configuration changes.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[YumirConfig] Failed to inject configuration changes: {ex.Message}");
            logErrors.LogAndForget(ex, $"[YumirConfig] Failed to inject configuration changes: {ex.Message}");
            throw;
        }
    }

    private static TomlTable GetOrCreateTable(TomlTable model, string key)
    {
        if (model.ContainsKey(key) && model[key] is TomlTable table)
            return table;

        var newTable = new TomlTable();
        model[key] = newTable;
        return newTable;
    }
}
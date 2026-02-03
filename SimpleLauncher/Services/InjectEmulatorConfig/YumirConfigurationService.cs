using System;
using System.IO;
using SimpleLauncher.Services.DebugAndBugReport;
using Tomlyn;
using Tomlyn.Model;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class YumirConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
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
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[YumirConfig] Created new Ymir.toml from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("Ymir.toml not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[YumirConfig] Injecting configuration into: {configPath}");

        var tomlContent = File.ReadAllText(configPath);
        var model = Toml.ToModel(tomlContent);

        // [Video]
        var video = GetOrCreateTable(model, "Video");
        video["FullScreen"] = settings.YumirFullscreen;
        video["ForceAspectRatio"] = settings.YumirForceAspectRatio;
        video["ForcedAspect"] = settings.YumirForcedAspect;
        video["ForceAspectRatio"] = settings.YumirForceAspectRatio;
        video["ReduceLatency"] = settings.YumirReduceLatency;

        // [Audio]
        var audio = GetOrCreateTable(model, "Audio");
        audio["Volume"] = settings.YumirVolume;
        audio["Mute"] = settings.YumirMute;

        // [System]
        var system = GetOrCreateTable(model, "System");
        system["VideoStandard"] = settings.YumirVideoStandard;
        system["AutoDetectRegion"] = settings.YumirAutoDetectRegion;

        // [General]
        var general = GetOrCreateTable(model, "General");
        general["PauseWhenUnfocused"] = settings.YumirPauseWhenUnfocused;

        var updatedToml = Toml.FromModel(model);
        File.WriteAllText(configPath, updatedToml);
        DebugLogger.Log("[YumirConfig] Injection successful.");
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

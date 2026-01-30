using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class AresConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "settings.bml");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Ares", "settings.bml");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[AresConfig] Created new settings.bml from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("settings.bml not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[AresConfig] Injecting configuration into: {configPath}");

        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Video
            { "Driver:", settings.AresVideoDriver },
            { "Exclusive:", settings.AresExclusive.ToString().ToLowerInvariant() },
            { "Shader:", settings.AresShader },
            { "Multiplier:", settings.AresMultiplier.ToString(CultureInfo.InvariantCulture) },
            { "AspectCorrectionMode:", settings.AresAspectCorrection },
            // Audio
            { "Mute:", settings.AresMute.ToString().ToLowerInvariant() },
            { "Volume:", settings.AresVolume.ToString("F1", CultureInfo.InvariantCulture) },
            // Boot
            { "Fast:", settings.AresFastBoot.ToString().ToLowerInvariant() },
            // General
            { "Rewind:", settings.AresRewind.ToString().ToLowerInvariant() },
            { "RunAhead:", settings.AresRunAhead.ToString().ToLowerInvariant() },
            { "AutoSaveMemory:", settings.AresAutoSaveMemory.ToString().ToLowerInvariant() }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmedLine = line.TrimStart();

            var key = updates.Keys.FirstOrDefault(k => trimmedLine.StartsWith(k, StringComparison.OrdinalIgnoreCase));

            if (key != null)
            {
                var indent = line[..^trimmedLine.Length];
                var newValue = $"{indent}{key} {updates[key]}";
                if (lines[i] != newValue)
                {
                    lines[i] = newValue;
                    modified = true;
                }
            }
        }

        if (modified)
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            DebugLogger.Log("[AresConfig] Injection successful.");
        }
        else
        {
            DebugLogger.Log("[AresConfig] No changes needed.");
        }
    }
}

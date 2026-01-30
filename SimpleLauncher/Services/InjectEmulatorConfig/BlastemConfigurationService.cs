using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class BlastemConfigurationService
{
    private static readonly char[] Separator = new[] { ' ', '\t' };

    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "default.cfg");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Blastem", "default.cfg");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[BlastemConfig] Created new default.cfg from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("default.cfg not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[BlastemConfig] Injecting configuration into: {configPath}");

        var updates = new Dictionary<string, string>
        {
            { "fullscreen", settings.BlastemFullscreen ? "on" : "off" },
            { "vsync", settings.BlastemVsync ? "on" : "off" },
            { "aspect", settings.BlastemAspect },
            { "scaling", settings.BlastemScaling },
            { "scanlines", settings.BlastemScanlines ? "on" : "off" },
            { "rate", settings.BlastemAudioRate.ToString(CultureInfo.InvariantCulture) },
            { "sync_source", settings.BlastemSyncSource }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || line.StartsWith('{') || line.StartsWith('}')) continue;

            var parts = line.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            var key = parts[0];
            if (!updates.TryGetValue(key, out var newValue)) continue;

            // Reconstruct the line preserving indentation (assuming a single tab)
            var newLine = $"\t{key} {newValue}";
            if (lines[i] == newLine) continue;

            lines[i] = newLine;
            modified = true;
        }

        if (modified)
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            DebugLogger.Log("[BlastemConfig] Injection successful.");
        }
        else
        {
            DebugLogger.Log("[BlastemConfig] No changes needed for Blastem configuration.");
        }
    }
}

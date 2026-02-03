using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class MednafenConfigurationService
{
    // List of common system prefixes used by Mednafen for per-system settings
    private static readonly string[] SystemPrefixes =
    [
        "apple2", "gb", "gba", "gg", "lynx", "md", "nes", "ngp", "pce", "pce_fast",
        "pcfx", "psx", "sms", "snes", "snes_faust", "ss", "vb", "wswan"
    ];

    private static readonly char[] Separator = [' '];

    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "mednafen.cfg");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Mednafen", "mednafen.cfg");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[MednafenConfig] Created new mednafen.cfg from sample: {configPath}");
            }
            else
            {
                // If no config and no sample, we can't proceed.
                throw new FileNotFoundException("mednafen.cfg not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[MednafenConfig] Injecting configuration into: {configPath}");

        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Global settings
            { "video.driver", settings.MednafenVideoDriver },
            { "video.fs", settings.MednafenFullscreen ? "1" : "0" },
            { "video.glvsync", settings.MednafenVsync ? "1" : "0" },
            { "sound.volume", settings.MednafenVolume.ToString(CultureInfo.InvariantCulture) },
            { "cheats", settings.MednafenCheats ? "1" : "0" },
            { "state_rewind", settings.MednafenRewind ? "1" : "0" }
        };

        // Add per-system settings for all common systems
        foreach (var prefix in SystemPrefixes)
        {
            updates[$"{prefix}.stretch"] = settings.MednafenStretch;
            updates[$"{prefix}.videoip"] = settings.MednafenBilinear ? "1" : "0";
            updates[$"{prefix}.scanlines"] = settings.MednafenScanlines.ToString(CultureInfo.InvariantCulture);
            updates[$"{prefix}.shader"] = settings.MednafenShader;
            updates[$"{prefix}.special"] = settings.MednafenSpecial;
        }

        var lines = File.ReadAllLines(configPath).ToList();
        var keysFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var modified = false;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';')) continue;

            var parts = line.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            var key = parts[0];
            if (updates.TryGetValue(key, out var newValue))
            {
                var newLine = $"{key} {newValue}";
                if (lines[i] != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }

                keysFound.Add(key);
            }
        }

        // Append any keys that were not found in the file
        foreach (var kvp in updates)
        {
            if (!keysFound.Contains(kvp.Key))
            {
                lines.Add($"{kvp.Key} {kvp.Value}");
                modified = true;
            }
        }

        if (modified)
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            DebugLogger.Log("[MednafenConfig] Injection successful.");
        }
        else
        {
            DebugLogger.Log("[MednafenConfig] No changes needed.");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class Pcsx2ConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        // PCSX2 usually stores config in 'inis' subfolder or root
        var configPath = Path.Combine(emuDir, "inis", "PCSX2.ini");
        if (!File.Exists(configPath))
        {
            configPath = Path.Combine(emuDir, "PCSX2.ini");
        }

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "PCSX2", "PCSX2.ini");
            if (File.Exists(samplePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? throw new InvalidOperationException("Could not create directory for PCSX2.ini"));
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[PCSX2Config] Created new PCSX2.ini from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("PCSX2.ini not found and sample is missing.", samplePath);
            }
        }

        var uiUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "StartFullscreen", settings.Pcsx2StartFullscreen.ToString().ToLowerInvariant() }
        };

        var emuCoreUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "EnableCheats", settings.Pcsx2EnableCheats.ToString().ToLowerInvariant() },
            { "EnableWideScreenPatches", settings.Pcsx2EnableWidescreenPatches.ToString().ToLowerInvariant() }
        };

        var gsUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Renderer", settings.Pcsx2Renderer.ToString(CultureInfo.InvariantCulture) },
            { "upscale_multiplier", settings.Pcsx2UpscaleMultiplier.ToString(CultureInfo.InvariantCulture) },
            { "AspectRatio", settings.Pcsx2AspectRatio },
            { "VsyncEnable", settings.Pcsx2Vsync.ToString(CultureInfo.InvariantCulture) }
        };

        var audioUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "FinalVolume", settings.Pcsx2Volume.ToString(CultureInfo.InvariantCulture) }
        };

        var achUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Enabled", settings.Pcsx2AchievementsEnabled.ToString().ToLowerInvariant() },
            { "Hardcore", settings.Pcsx2AchievementsHardcore.ToString().ToLowerInvariant() }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;
        var currentSection = "";

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';')) continue;

            var parts = line.Split('=', 2);
            if (parts.Length < 2) continue;

            var key = parts[0].Trim();
            var currentUpdates = currentSection switch
            {
                "[UI]" => uiUpdates,
                "[EmuCore]" => emuCoreUpdates,
                "[EmuCore/GS]" => gsUpdates,
                "[SPU2/Mixing]" => audioUpdates,
                "[Achievements]" => achUpdates,
                _ => null
            };

            if (currentUpdates != null && currentUpdates.Remove(key, out var newValue))
            {
                var newLine = $"{key} = {newValue}";
                if (lines[i] != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }
            }
        }

        // Add missing keys/sections
        if (uiUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[UI]", uiUpdates, ref modified);
        }

        if (emuCoreUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[EmuCore]", emuCoreUpdates, ref modified);
        }

        if (gsUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[EmuCore/GS]", gsUpdates, ref modified);
        }

        if (audioUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[SPU2/Mixing]", audioUpdates, ref modified);
        }

        if (achUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[Achievements]", achUpdates, ref modified);
        }

        if (modified)
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            DebugLogger.Log("[PCSX2Config] Injection successful.");
        }
    }

    private static void ApplyUpdatesToSection(List<string> lines, string sectionName, Dictionary<string, string> updates, ref bool modified)
    {
        var sectionIndex = lines.FindIndex(l => l.Trim().Equals(sectionName, StringComparison.OrdinalIgnoreCase));

        if (sectionIndex == -1)
        {
            lines.Add("");
            lines.Add(sectionName);
            sectionIndex = lines.Count - 1;
        }

        var insertIndex = sectionIndex + 1;
        foreach (var kvp in updates)
        {
            lines.Insert(insertIndex++, $"{kvp.Key} = {kvp.Value}");
        }

        if (updates.Count > 0)
        {
            modified = true;
        }
    }
}
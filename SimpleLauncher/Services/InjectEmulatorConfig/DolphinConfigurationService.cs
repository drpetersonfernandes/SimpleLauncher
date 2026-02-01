using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class DolphinConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        // For portable setups, Dolphin uses a "User/Config" subfolder. Fallback to the emulator's root.
        var configDir = Path.Combine(emuDir, "User", "Config");
        if (!Directory.Exists(configDir))
        {
            configDir = emuDir;
        }

        var configPath = Path.Combine(configDir, "Dolphin.ini");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Dolphin", "Dolphin.ini");
            if (File.Exists(samplePath))
            {
                Directory.CreateDirectory(configDir); // Ensure the target directory exists
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[DolphinConfig] Created new Dolphin.ini from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("Dolphin.ini not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[DolphinConfig] Injecting configuration into: {configPath}");

        var coreUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "GFXBackend", settings.DolphinGfxBackend },
            { "WiimoteContinuousScanning", settings.DolphinWiimoteContinuousScanning.ToString() },
            { "WiimoteEnableSpeaker", settings.DolphinWiimoteEnableSpeaker.ToString() }
        };

        var dspUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "DSPThread", settings.DolphinDspThread.ToString() }
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

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || line.StartsWith('$')) continue;

            var parts = line.Split('=', 2);
            if (parts.Length < 2) continue;

            var key = parts[0].Trim();
            Dictionary<string, string> currentUpdates = null;

            if (currentSection.Equals("[Core]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = coreUpdates;
            }
            else if (currentSection.Equals("[DSP]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = dspUpdates;
            }

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
        if (coreUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[Core]", coreUpdates, ref modified);
        }

        if (dspUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[DSP]", dspUpdates, ref modified);
        }

        if (modified)
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            DebugLogger.Log("[DolphinConfig] Injection successful.");
        }
        else
        {
            DebugLogger.Log("[DolphinConfig] No changes needed.");
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

        modified = true;
    }
}
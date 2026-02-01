using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class DuckStationConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "settings.ini");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "DuckStation", "settings.ini");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[DuckStationConfig] Created new settings.ini from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("settings.ini not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[DuckStationConfig] Injecting configuration into: {configPath}");

        var mainUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "StartFullscreen", settings.DuckStationStartFullscreen.ToString().ToLowerInvariant() },
            { "PauseOnFocusLoss", settings.DuckStationPauseOnFocusLoss.ToString().ToLowerInvariant() },
            { "SaveStateOnExit", settings.DuckStationSaveStateOnExit.ToString().ToLowerInvariant() },
            { "RewindEnable", settings.DuckStationRewindEnable.ToString().ToLowerInvariant() },
            { "RunaheadFrameCount", settings.DuckStationRunaheadFrameCount.ToString(CultureInfo.InvariantCulture) }
        };

        var gpuUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Renderer", settings.DuckStationRenderer },
            { "ResolutionScale", settings.DuckStationResolutionScale.ToString(CultureInfo.InvariantCulture) },
            { "TextureFilter", settings.DuckStationTextureFilter },
            { "WidescreenHack", settings.DuckStationWidescreenHack.ToString().ToLowerInvariant() },
            { "PGXPEnable", settings.DuckStationPgxpEnable.ToString().ToLowerInvariant() }
        };

        var displayUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "AspectRatio", settings.DuckStationAspectRatio },
            { "VSync", settings.DuckStationVsync.ToString().ToLowerInvariant() }
        };

        var audioUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "OutputVolume", settings.DuckStationOutputVolume.ToString(CultureInfo.InvariantCulture) },
            { "OutputMuted", settings.DuckStationOutputMuted.ToString().ToLowerInvariant() }
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
            Dictionary<string, string> currentUpdates = null;

            if (currentSection.Equals("[Main]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = mainUpdates;
            }
            else if (currentSection.Equals("[GPU]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = gpuUpdates;
            }
            else if (currentSection.Equals("[Display]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = displayUpdates;
            }
            else if (currentSection.Equals("[Audio]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = audioUpdates;
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
        if (mainUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[Main]", mainUpdates, out var sectionModified);
            modified |= sectionModified;
        }

        if (gpuUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[GPU]", gpuUpdates, out var sectionModified);
            modified |= sectionModified;
        }

        if (displayUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[Display]", displayUpdates, out var sectionModified);
            modified |= sectionModified;
        }

        if (audioUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[Audio]", audioUpdates, out var sectionModified);
            modified |= sectionModified;
        }

        if (modified)
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            DebugLogger.Log("[DuckStationConfig] Injection successful.");
        }
        else
        {
            DebugLogger.Log("[DuckStationConfig] No changes needed.");
        }
    }

    private static void ApplyUpdatesToSection(List<string> lines, string sectionName, Dictionary<string, string> updates, out bool modified)
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
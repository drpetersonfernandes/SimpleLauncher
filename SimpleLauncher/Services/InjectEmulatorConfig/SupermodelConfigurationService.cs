using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class SupermodelConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir)) throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "Config", "Supermodel.ini");

        // Supermodel often puts the ini in a 'Config' subfolder, but check root too
        if (!File.Exists(configPath))
        {
            var rootPath = Path.Combine(emuDir, "Supermodel.ini");
            if (File.Exists(rootPath))
            {
                configPath = rootPath;
            }
        }

        // Backup logic: Create from your sample if missing
        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Supermodel", "Supermodel.ini");
            if (File.Exists(samplePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? throw new InvalidOperationException("Could not create directory for Supermodel.ini"));
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[SupermodelConfig] Created from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("Supermodel.ini not found and sample missing.");
            }
        }

        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "New3DEngine", settings.SupermodelNew3DEngine ? "1" : "0" },
            { "QuadRendering", settings.SupermodelQuadRendering ? "1" : "0" },
            { "FullScreen", settings.SupermodelFullscreen ? "1" : "0" },
            { "XResolution", settings.SupermodelResX.ToString(CultureInfo.InvariantCulture) },
            { "YResolution", settings.SupermodelResY.ToString(CultureInfo.InvariantCulture) },
            { "WideScreen", settings.SupermodelWideScreen ? "1" : "0" },
            { "Stretch", settings.SupermodelStretch ? "1" : "0" },
            { "VSync", settings.SupermodelVsync ? "1" : "0" },
            { "Throttle", settings.SupermodelThrottle ? "1" : "0" },
            { "MusicVolume", settings.SupermodelMusicVolume.ToString(CultureInfo.InvariantCulture) },
            { "SoundVolume", settings.SupermodelSoundVolume.ToString(CultureInfo.InvariantCulture) },
            { "InputSystem", settings.SupermodelInputSystem }, // Simple values don't need quotes in Supermodel INI
            { "MultiThreaded", settings.SupermodelMultiThreaded ? "1" : "0" },
            { "PowerPCFrequency", settings.SupermodelPowerPcFrequency.ToString(CultureInfo.InvariantCulture) }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var keysFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var inGlobalSection = false;
        var globalSectionIndex = -1;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();

            if (line.Equals("[Global]", StringComparison.OrdinalIgnoreCase))
            {
                inGlobalSection = true;
                globalSectionIndex = i;
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                inGlobalSection = false;
                continue;
            }

            if (!inGlobalSection || string.IsNullOrWhiteSpace(line) || line.StartsWith(';')) continue;

            var parts = line.Split('=', 2);
            if (parts.Length < 2) continue;

            var key = parts[0].Trim();
            if (updates.TryGetValue(key, out var newValue))
            {
                lines[i] = $"{key} = {newValue}";
                keysFound.Add(key);
            }
        }

        // If keys were not found, add them to the [Global] section
        var keysToAdd = updates.Keys.Where(k => !keysFound.Contains(k)).ToList();
        if (keysToAdd.Count != 0)
        {
            if (globalSectionIndex != -1)
            {
                // Find the end of the [Global] section or end of file
                var insertIndex = globalSectionIndex + 1;
                while (insertIndex < lines.Count && !lines[insertIndex].Trim().StartsWith('['))
                {
                    insertIndex++;
                }

                foreach (var key in keysToAdd)
                {
                    lines.Insert(insertIndex++, $"{key} = {updates[key]}");
                }
            }
            else // If [Global] section doesn't exist, add it with the missing keys
            {
                lines.Add("");
                lines.Add("[Global]");
                foreach (var key in keysToAdd)
                {
                    lines.Add($"{key} = {updates[key]}");
                }
            }
        }

        File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
        DebugLogger.Log("[SupermodelConfig] Injection successful.");
    }
}
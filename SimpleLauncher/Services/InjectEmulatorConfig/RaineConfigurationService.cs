using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class RaineConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir)) throw new InvalidOperationException("Emulator directory not found.");

        // Raine uses raine32_sdl.cfg or raine64_sdl.cfg.
        var configPath = Path.Combine(emuDir, "config", "raine32_sdl.cfg");

        // If config is missing, copy from sample
        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Raine", "raine32_sdl.cfg");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[RaineConfig] Created new config from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, "Failed to create Raine config from sample.");
                    throw;
                }
            }
            else throw new FileNotFoundException("Raine configuration file not found and sample is missing.", samplePath);
        }

        var updates = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Display"] = new(StringComparer.OrdinalIgnoreCase)
            {
                { "fullscreen", settings.RaineFullscreen ? "1" : "0" },
                { "screen_x", settings.RaineResX.ToString(CultureInfo.InvariantCulture) },
                { "screen_y", settings.RaineResY.ToString(CultureInfo.InvariantCulture) },
                { "fix_aspect_ratio", settings.RaineFixAspectRatio ? "1" : "0" },
                { "ogl_dbuf", settings.RaineVsync ? "2" : "0" }
            },
            ["Sound"] = new(StringComparer.OrdinalIgnoreCase)
            {
                { "driver", settings.RaineSoundDriver },
                { "sample_rate", settings.RaineSampleRate.ToString(CultureInfo.InvariantCulture) }
            }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;
        string currentSection = null;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmedLine = line.Trim(); // Trim once for logic checks
            if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

            // Robust section header detection
            if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
            {
                currentSection = trimmedLine.Trim('[', ']').Trim();
                continue;
            }

            if (currentSection != null && updates.TryGetValue(currentSection, out var sectionUpdates))
            {
                var parts = trimmedLine.Split('=', 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    if (sectionUpdates.TryGetValue(key, out var newValue))
                    {
                        var newLine = $"{key} = {newValue}";
                        if (lines[i].Trim() != newLine) // Compare trimmed to avoid false positives on indentation
                        {
                            lines[i] = newLine;
                            modified = true;
                        }

                        sectionUpdates.Remove(key);
                    }
                }
            }
        }

        // Add missing keys/sections
        foreach (var section in updates)
        {
            if (section.Value.Count > 0)
            {
                modified = true;
                var sectionHeader = $"[{section.Key}]";
                var sectionIndex = lines.FindIndex(l => l.Trim().Equals(sectionHeader, StringComparison.OrdinalIgnoreCase));
                if (sectionIndex == -1)
                {
                    lines.Add("");
                    lines.Add(sectionHeader);
                    sectionIndex = lines.Count - 1;
                }

                foreach (var kvp in section.Value)
                {
                    lines.Insert(sectionIndex + 1, $"{kvp.Key} = {kvp.Value}");
                }
            }
        }

        if (modified)
        {
            try
            {
                File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
                DebugLogger.Log("[RaineConfig] Configuration injected.");
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, "Failed to inject Raine configuration.");
                throw;
            }
        }
    }
}
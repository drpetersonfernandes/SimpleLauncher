using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class FlycastConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "emu.cfg");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Flycast", "emu.cfg");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[FlycastConfig] Created new emu.cfg from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[FlycastConfig] Failed to create emu.cfg from sample: {ex.Message}");
                    _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[FlycastConfig] Failed to create emu.cfg from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("emu.cfg not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[FlycastConfig] Injecting configuration into: {configPath}");

        var windowUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "fullscreen", settings.FlycastFullscreen ? "yes" : "no" },
            { "width", settings.FlycastWidth.ToString(CultureInfo.InvariantCulture) },
            { "height", settings.FlycastHeight.ToString(CultureInfo.InvariantCulture) },
            { "maximized", settings.FlycastMaximized ? "yes" : "no" }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;
        var inWindowSection = false;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                inWindowSection = line.Equals("[window]", StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inWindowSection || string.IsNullOrWhiteSpace(line) || line.StartsWith(';')) continue;

            var parts = line.Split('=', 2);
            if (parts.Length < 2) continue;

            var key = parts[0].Trim();
            if (windowUpdates.TryGetValue(key, out var newValue))
            {
                var newLine = $"{key} = {newValue}";
                if (lines[i] != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }

                windowUpdates.Remove(key); // Mark as found
            }
        }

        // Add any missing keys to the [window] section
        if (windowUpdates.Count > 0)
        {
            modified = true;
            var windowIndex = lines.FindIndex(static l => l.Trim().Equals("[window]", StringComparison.OrdinalIgnoreCase));
            if (windowIndex != -1)
            {
                var insertIndex = windowIndex + 1;
                while (insertIndex < lines.Count && !string.IsNullOrWhiteSpace(lines[insertIndex]) && !lines[insertIndex].Trim().StartsWith('['))
                {
                    insertIndex++;
                }

                foreach (var kvp in windowUpdates)
                {
                    lines.Insert(insertIndex++, $"{kvp.Key} = {kvp.Value}");
                }
            }
            else // If [window] section doesn't exist, add it
            {
                lines.Add("");
                lines.Add("[window]");
                foreach (var kvp in windowUpdates)
                {
                    lines.Add($"{kvp.Key} = {kvp.Value}");
                }
            }
        }

        if (modified)
        {
            try
            {
                File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
                DebugLogger.Log("[FlycastConfig] Injected configuration changes..");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[FlycastConfig] Failed to inject configuration changes: {ex.Message}");
                _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[FlycastConfig] Failed to inject configuration changes: {ex.Message}");
                throw;
            }
        }
        else
        {
            DebugLogger.Log("[FlycastConfig] No changes needed.");
        }
    }
}
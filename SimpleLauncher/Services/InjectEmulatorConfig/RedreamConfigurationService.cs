using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class RedreamConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "redream.cfg");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Redream", "redream.cfg");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[RedreamConfig] Created new redream.cfg from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[RedreamConfig] Failed to create redream.cfg from sample: {ex.Message}");
                    _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[RedreamConfig] Failed to create redream.cfg from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("redream.cfg not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[RedreamConfig] Injecting configuration into: {configPath}");

        var isWindowed = IsWindowedMode(settings.RedreamFullmode);

        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "cable", settings.RedreamCable },
            { "broadcast", settings.RedreamBroadcast },
            { "language", settings.RedreamLanguage },
            { "region", settings.RedreamRegion },
            { "vsync", settings.RedreamVsync ? "1" : "0" },
            { "frameskip", settings.RedreamFrameskip ? "1" : "0" },
            { "aspect", settings.RedreamAspect },
            { "res", settings.RedreamRes.ToString(CultureInfo.InvariantCulture) },
            { "renderer", settings.RedreamRenderer },
            { "volume", settings.RedreamVolume.ToString(CultureInfo.InvariantCulture) },
            { "latency", settings.RedreamLatency.ToString(CultureInfo.InvariantCulture) },
            { "framerate", settings.RedreamFramerate ? "1" : "0" }
        };

        // Handle window/fullscreen mode correctly
        if (isWindowed) // Changed from: if (settings.RedreamFullmode == "windowed")
        {
            updates["mode"] = "windowed";
            updates["width"] = settings.RedreamWidth.ToString(CultureInfo.InvariantCulture);
            updates["height"] = settings.RedreamHeight.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            updates["mode"] = "fullscreen";
            updates["fullmode"] = settings.RedreamFullmode;
            updates["fullwidth"] = settings.RedreamWidth.ToString(CultureInfo.InvariantCulture);
            updates["fullheight"] = settings.RedreamHeight.ToString(CultureInfo.InvariantCulture);
        }

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;
        var keysFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            var parts = line.Split('=', 2);
            if (parts.Length < 2) continue;

            var key = parts[0].Trim();
            if (updates.TryGetValue(key, out var newValue))
            {
                var newLine = $"{key}={newValue}";
                if (line != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }

                keysFound.Add(key);
            }
        }

        // Add missing keys
        foreach (var kvp in updates)
        {
            if (!keysFound.Contains(kvp.Key))
            {
                lines.Add($"{kvp.Key}={kvp.Value}");
                modified = true;
            }
        }

        if (modified)
        {
            try
            {
                File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
                DebugLogger.Log("[RedreamConfig] Injected configuration changes..");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[RedreamConfig] Failed to inject configuration changes: {ex.Message}");
                _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[RedreamConfig] Failed to inject configuration changes: {ex.Message}");
                throw;
            }
        }
    }

    private static bool IsWindowedMode(string fullmode)
    {
        return string.IsNullOrWhiteSpace(fullmode) ||
               fullmode.Equals("windowed", StringComparison.OrdinalIgnoreCase);
    }
}
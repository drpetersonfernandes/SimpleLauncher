using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class AzaharConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir)) throw new InvalidOperationException("Emulator directory not found.");

        // Azahar usually looks for qt-config.ini in the same folder or AppData.
        // We assume portable mode/local config first.
        var configPath = Path.Combine(emuDir, "qt-config.ini");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Azahar", "qt-config.ini");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[AzaharConfig] Created new qt-config.ini from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("qt-config.ini not found and sample is missing.");
            }
        }

        var updates = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Renderer"] = new()
            {
                { "graphics_api", settings.AzaharGraphicsApi.ToString(CultureInfo.InvariantCulture) },
                { "resolution_factor", settings.AzaharResolutionFactor.ToString(CultureInfo.InvariantCulture) },
                { "use_vsync_new", settings.AzaharUseVsync.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture) },
                { "async_shader_compilation", settings.AzaharAsyncShaderCompilation.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture) }
            },
            ["UI"] = new()
            {
                { "fullscreen", settings.AzaharFullscreen.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture) }
            },
            ["Audio"] = new()
            {
                { "volume", (settings.AzaharVolume / 100.0).ToString("F2", CultureInfo.InvariantCulture) }
            },
            ["System"] = new()
            {
                { "is_new_3ds", settings.AzaharIsNew3ds.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture) }
            },
            ["Layout"] = new()
            {
                { "layout_option", settings.AzaharLayoutOption.ToString(CultureInfo.InvariantCulture) }
            }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;
        string currentSection = null;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line.Substring(1, line.Length - 2);
                continue;
            }

            if (currentSection != null && updates.TryGetValue(currentSection, out var sectionUpdates))
            {
                var parts = line.Split('=', 2);
                if (parts.Length < 2) continue;

                var key = parts[0].Trim();

                // Check if this is a standard key or a \default key
                var baseKey = key.EndsWith("\\default", StringComparison.Ordinal) ? key.Replace("\\default", "") : key;

                if (sectionUpdates.TryGetValue(baseKey, out var newValue))
                {
                    string newLine;
                    if (key.EndsWith("\\default", StringComparison.Ordinal))
                    {
                        newLine = $"{key}=false"; // We are providing a custom value, so default is false
                    }
                    else
                    {
                        newLine = $"{key}={newValue}";
                    }

                    if (lines[i] != newLine)
                    {
                        lines[i] = newLine;
                        modified = true;
                    }
                }
            }
        }

        if (modified)
        {
            File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
            DebugLogger.Log("[AzaharConfig] Injection successful.");
        }
    }
}

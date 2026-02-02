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
                { "use_vsync_new", BoolToString(settings.AzaharUseVsync) },
                { "async_shader_compilation", BoolToString(settings.AzaharAsyncShaderCompilation) }
            },
            ["UI"] = new()
            {
                { "fullscreen", BoolToString(settings.AzaharFullscreen) }
            },
            ["Audio"] = new()
            {
                { "volume", (settings.AzaharVolume / 100.0).ToString("F2", CultureInfo.InvariantCulture) },
                { "enable_audio_stretching", BoolToString(settings.AzaharEnableAudioStretching) }
            },
            ["System"] = new()
            {
                { "is_new_3ds", BoolToString(settings.AzaharIsNew3ds) }
            },
            ["Layout"] = new()
            {
                { "layout_option", settings.AzaharLayoutOption.ToString(CultureInfo.InvariantCulture) }
            }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;
        var keysProcessed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string currentSection = null;

        // Track which keys need their \default counterpart updated
        var defaultKeyUpdates = new Dictionary<string, bool>(); // key -> isDefault

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line.Substring(1, line.Length - 2);
                continue;
            }

            if (currentSection == null) continue;

            var parts = line.Split('=', 2);
            if (parts.Length < 2) continue;

            var key = parts[0].Trim();

            // Handle \default keys
            if (key.EndsWith("\\default", StringComparison.OrdinalIgnoreCase))
            {
                var baseKey = key.Substring(0, key.Length - 8); // Remove "\default"

                if (updates.TryGetValue(currentSection, out var sectionUpdateList) &&
                    sectionUpdateList.ContainsKey(baseKey))
                {
                    // We're providing a custom value, so default should be false
                    var newLine = $"{key}=false";
                    if (lines[i] != newLine)
                    {
                        lines[i] = newLine;
                        modified = true;
                    }

                    defaultKeyUpdates[baseKey] = false;
                }

                continue;
            }

            // Handle regular value keys
            if (updates.TryGetValue(currentSection, out var sectionUpdateList2) &&
                sectionUpdateList2.TryGetValue(key, out var newValue))
            {
                var newLine = $"{key}={newValue}";
                if (lines[i] != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }

                keysProcessed.Add($"{currentSection}:{key}");
            }
        }

        // Second pass: Add missing \default=false lines for keys we updated
        // and add missing keys entirely
        foreach (var (sectionName, sectionDict) in updates)
        {
            foreach (var (key, value) in sectionDict)
            {
                var fullKey = $"{sectionName}:{key}";

                // Find section in lines
                var sectionIndex = lines.FindIndex(l =>
                    l.Trim().Equals($"[{sectionName}]", StringComparison.OrdinalIgnoreCase));
                if (sectionIndex == -1) continue;

                // Check if we processed this key
                if (!keysProcessed.Contains(fullKey))
                {
                    // Need to add the key and its \default line
                    var insertIndex = sectionIndex + 1;
                    while (insertIndex < lines.Count &&
                           !string.IsNullOrWhiteSpace(lines[insertIndex]) &&
                           !lines[insertIndex].Trim().StartsWith('['))
                    {
                        insertIndex++;
                    }

                    // Insert value line first, then \default line
                    lines.Insert(insertIndex, $"{key}={value}");
                    lines.Insert(insertIndex + 1, $"{key}\\default=false");
                    modified = true;
                }
                else if (!defaultKeyUpdates.ContainsKey(key))
                {
                    // Key existed but we didn't see a \default line, add one
                    var keyIndex = lines.FindIndex(sectionIndex, l =>
                    {
                        var trimmed = l.Trim();
                        return trimmed.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase) &&
                               !trimmed.Contains("\\default");
                    });

                    if (keyIndex != -1)
                    {
                        lines.Insert(keyIndex + 1, $"{key}\\default=false");
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

    private static string BoolToString(bool value)
    {
        return value ? "true" : "false";
    }
}
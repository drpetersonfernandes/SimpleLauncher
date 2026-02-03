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

        foreach (var (sectionName, dictionary) in updates)
        {
            var sectionHeader = $"[{sectionName}]";

            // 1. Find or Create Section
            var sectionStartIndex = lines.FindIndex(l => l.Trim().Equals(sectionHeader, StringComparison.OrdinalIgnoreCase));
            if (sectionStartIndex == -1)
            {
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1])) lines.Add("");
                lines.Add(sectionHeader);
                sectionStartIndex = lines.Count - 1;
                modified = true;
            }

            // 2. Determine Section Range
            var sectionEndIndex = lines.FindIndex(sectionStartIndex + 1, static l => l.Trim().StartsWith('['));
            if (sectionEndIndex == -1)
            {
                sectionEndIndex = lines.Count;
            }

            foreach (var (key, value) in dictionary)
            {
                var defaultKey = $"{key}\\default";
                var keyIndex = -1;
                var defaultKeyIndex = -1;

                // 3. Search for existing keys within this section only
                for (var i = sectionStartIndex + 1; i < sectionEndIndex; i++)
                {
                    var trimmed = lines[i].Trim();
                    if (trimmed.StartsWith($"{key}=", StringComparison.OrdinalIgnoreCase))
                    {
                        keyIndex = i;
                    }
                    else if (trimmed.StartsWith($"{defaultKey}=", StringComparison.OrdinalIgnoreCase))
                    {
                        defaultKeyIndex = i;
                    }
                }

                // 4. Update or Insert Main Key
                var keyLine = $"{key}={value}";
                if (keyIndex != -1)
                {
                    if (lines[keyIndex] != keyLine)
                    {
                        lines[keyIndex] = keyLine;
                        modified = true;
                    }
                }
                else
                {
                    lines.Insert(sectionEndIndex, keyLine);
                    keyIndex = sectionEndIndex;
                    sectionEndIndex++;
                    modified = true;
                }

                // 5. Update or Insert \default Key (always false when injected)
                var defaultLine = $"{defaultKey}=false";
                if (defaultKeyIndex != -1)
                {
                    if (lines[defaultKeyIndex] != defaultLine)
                    {
                        lines[defaultKeyIndex] = defaultLine;
                        modified = true;
                    }
                }
                else
                {
                    lines.Insert(keyIndex + 1, defaultLine);
                    sectionEndIndex++;
                    modified = true;
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
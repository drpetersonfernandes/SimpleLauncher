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
    public static void InjectSettings(
        string emulatorPath,
        SettingsManager.SettingsManager settings,
        string gameFilePath = null,
        string systemRomPath = null,
        string raineCustomRomDirectory = null)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir)) throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "config", "raine32_sdl.cfg");
        var configDir = Path.GetDirectoryName(configPath);
        if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

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

        // Determine if we are in NeoGeo CD mode
        var ext = !string.IsNullOrEmpty(gameFilePath) ? Path.GetExtension(gameFilePath).ToLowerInvariant() : "";
        var isNeoGeoCd = ext is ".cue" or ".iso" or ".bin" or ".chd";
        var gameDir = !string.IsNullOrEmpty(gameFilePath) ? Path.GetDirectoryName(gameFilePath) : null;

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
            },
            ["General"] = new(StringComparer.OrdinalIgnoreCase)
            {
                { "frame_skip", settings.RaineFrameSkip.ToString(CultureInfo.InvariantCulture) },
                { "ShowFPS", settings.RaineShowFps ? "1" : "0" }
            },
            ["Directories"] = new(StringComparer.OrdinalIgnoreCase),
            ["neocd"] = new(StringComparer.OrdinalIgnoreCase)
        };

        // Inject rom_dir_0
        // Priority: 1. Custom RaineRomDirectory from settings, 2. Game directory (if arcade), 3. System PrimarySystemFolder
        string effectiveRomDir = null;
        if (!string.IsNullOrEmpty(raineCustomRomDirectory) && Directory.Exists(raineCustomRomDirectory))
        {
            effectiveRomDir = raineCustomRomDirectory;
        }
        else if (!isNeoGeoCd && !string.IsNullOrEmpty(gameDir))
        {
            effectiveRomDir = gameDir;
        }
        else if (!string.IsNullOrEmpty(systemRomPath))
        {
            effectiveRomDir = systemRomPath;
        }

        if (!string.IsNullOrEmpty(effectiveRomDir))
        {
            updates["Directories"]["rom_dir_0"] = effectiveRomDir.EndsWith(Path.DirectorySeparatorChar) ? effectiveRomDir : effectiveRomDir + Path.DirectorySeparatorChar;
        }

        // Inject NeoGeo CD specific settings
        if (isNeoGeoCd)
        {
            if (!string.IsNullOrEmpty(gameDir))
            {
                updates["neocd"]["neocd_dir"] = gameDir.EndsWith(Path.DirectorySeparatorChar) ? gameDir : gameDir + Path.DirectorySeparatorChar;
            }

            updates["neocd"]["neocd_bios"] = settings.RaineNeoCdBios;
            updates["neocd"]["music_volume"] = settings.RaineMusicVolume.ToString(CultureInfo.InvariantCulture);
            updates["neocd"]["sfx_volume"] = settings.RaineSfxVolume.ToString(CultureInfo.InvariantCulture);
            updates["neocd"]["mute_sfx"] = settings.RaineMuteSfx ? "1" : "0";
            updates["neocd"]["mute_music"] = settings.RaineMuteMusic ? "1" : "0";
        }

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
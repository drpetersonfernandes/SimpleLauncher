using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class AresConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "settings.bml");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Ares", "settings.bml");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[AresConfig] Trying to create new settings.bml from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[AresConfig] Failed to create settings.bml from sample: {ex.Message}");
                    _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[AresConfig] Failed to create settings.bml from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("settings.bml not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[AresConfig] Injecting configuration into: {configPath}");

        // Section-aware updates to prevent cross-section key collisions (e.g., Driver: appears in Video, Audio, Input)
        var updates = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Video"] = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Driver:", settings.AresVideoDriver },
                { "Exclusive:", settings.AresExclusive.ToString().ToLowerInvariant() },
                { "Shader:", settings.AresShader },
                { "Multiplier:", settings.AresMultiplier.ToString(CultureInfo.InvariantCulture) },
                { "Output:", settings.AresAspectCorrection }
            },
            ["Audio"] = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Mute:", settings.AresMute.ToString().ToLowerInvariant() },
                { "Volume:", settings.AresVolume.ToString("F1", CultureInfo.InvariantCulture) }
            },
            ["Boot"] = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Fast:", settings.AresFastBoot.ToString().ToLowerInvariant() }
            },
            ["General"] = new(StringComparer.OrdinalIgnoreCase)
            {
                { "Rewind:", settings.AresRewind.ToString().ToLowerInvariant() },
                { "RunAhead:", settings.AresRunAhead.ToString().ToLowerInvariant() },
                { "AutoSaveMemory:", settings.AresAutoSaveMemory.ToString().ToLowerInvariant() }
            }
        };

        var lines = File.ReadAllLines(configPath).ToList();
        var modified = false;
        string currentSection = null;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            var trimmedLine = line.TrimStart();

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(trimmedLine))
                continue;

            // Detect section headers: not indented and contains no colon (e.g., "Video", "Audio", "Input")
            if (!char.IsWhiteSpace(line[0]) && !trimmedLine.Contains(':'))
            {
                currentSection = trimmedLine;
                continue;
            }

            // Only process if we're in a section with pending updates
            if (currentSection == null || !updates.TryGetValue(currentSection, out var sectionUpdates))
                continue;

            // Parse property lines (e.g., "  Driver: OpenGL 3.2")
            var lineParts = trimmedLine.Split(':', 2);
            if (lineParts.Length < 1) continue;

            var lineKey = lineParts[0] + ":";

            if (sectionUpdates.TryGetValue(lineKey, out var newValue))
            {
                var indent = line[..^trimmedLine.Length];
                var newLine = $"{indent}{lineKey} {newValue}";
                if (lines[i] != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }
            }
        }

        if (modified)
        {
            try
            {
                File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
                DebugLogger.Log("[AresConfig] Trying to inject configuration changes..");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[AresConfig] Failed to inject configuration changes: {ex.Message}");
                _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[AresConfig] Failed to inject configuration changes: {ex.Message}");
                throw;
            }
        }
        else
        {
            DebugLogger.Log("[AresConfig] No changes needed.");
        }
    }
}
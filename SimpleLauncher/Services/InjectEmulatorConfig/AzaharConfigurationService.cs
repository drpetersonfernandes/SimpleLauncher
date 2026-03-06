using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class AzaharConfigurationService
{
    /// <summary>
    /// Injects settings into Azahar's qt-config.ini file.
    /// </summary>
    /// <param name="emulatorPath">Path to the Azahar executable.</param>
    /// <param name="settings">The settings manager containing Azahar configuration.</param>
    /// <returns>True if injection was successful, false if it failed due to permissions but the game can still launch.</returns>
    /// <exception cref="InvalidOperationException">Thrown when emulator directory is not found.</exception>
    /// <exception cref="FileNotFoundException">Thrown when config file and sample are both missing.</exception>
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir)) throw new InvalidOperationException("Emulator directory not found.");

        // Azahar usually looks for qt-config.ini in the same folder or AppData.
        // We assume portable mode/local config first.
        var configPath = Path.Combine(emuDir, "qt-config.ini");

        // Check if we have write access to the emulator directory
        if (!IsDirectoryWritable(emuDir))
        {
            DebugLogger.Log($"[AzaharConfig] Directory is not writable: {emuDir}");
            throw new AzaharPermissionException($"Cannot write to emulator directory: {emuDir}");
        }

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Azahar", "qt-config.ini");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[AzaharConfig] Created new qt-config.ini from sample: {configPath}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    DebugLogger.Log($"[AzaharConfig] Failed to create qt-config.ini from sample due to permissions: {ex.Message}");
                    _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[AzaharConfig] Failed to create qt-config.ini from sample: {ex.Message}");
                    throw new AzaharPermissionException($"Cannot write to emulator directory: {emuDir}", ex);
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[AzaharConfig] Failed to create qt-config.ini from sample: {ex.Message}");
                    _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[AzaharConfig] Failed to create qt-config.ini from sample: {ex.Message}");
                    throw;
                }
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

        List<string> lines;
        try
        {
            lines = File.ReadAllLines(configPath).ToList();
        }
        catch (UnauthorizedAccessException ex)
        {
            DebugLogger.Log($"[AzaharConfig] Failed to read qt-config.ini due to permissions: {ex.Message}");
            _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[AzaharConfig] Failed to read qt-config.ini: {ex.Message}");
            throw new AzaharPermissionException($"Cannot read configuration file: {configPath}", ex);
        }

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
            try
            {
                File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
                DebugLogger.Log("[AzaharConfig] Injected configuration changes..");
            }
            catch (UnauthorizedAccessException ex)
            {
                DebugLogger.Log($"[AzaharConfig] Failed to inject configuration changes due to permissions: {ex.Message}");
                _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[AzaharConfig] Failed to inject configuration changes: {ex.Message}");
                throw new AzaharPermissionException($"Cannot write to configuration file: {configPath}", ex);
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[AzaharConfig] Failed to inject configuration changes: {ex.Message}");
                _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[AzaharConfig] Failed to inject configuration changes: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Checks if a directory is writable by attempting to create a temporary file.
    /// </summary>
    private static bool IsDirectoryWritable(string dirPath)
    {
        try
        {
            var testFile = Path.Combine(dirPath, $".write_test_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[AzaharConfig] Error checking directory writability: {ex.Message}");
            return false;
        }
    }

    private static string BoolToString(bool value)
    {
        return value ? "true" : "false";
    }
}

/// <summary>
/// Exception thrown when Azahar configuration cannot be modified due to file permission issues.
/// </summary>
public class AzaharPermissionException : Exception
{
    public AzaharPermissionException(string message) : base(message)
    {
    }

    public AzaharPermissionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
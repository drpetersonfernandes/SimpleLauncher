using System.Globalization;
using System.Text;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

using Interfaces;

public static class DuckStationConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, ILogErrors logErrors, IDebugLogger debugLogger)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "settings.ini");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "DuckStation", "settings.ini");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    debugLogger.Log($"[DuckStationConfig] Created new settings.ini from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    debugLogger.Log($"[DuckStationConfig] Failed to create settings.ini from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[DuckStationConfig] Failed to create settings.ini from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("settings.ini not found and sample is missing.", samplePath);
            }
        }

        debugLogger.Log($"[DuckStationConfig] Injecting configuration into: {configPath}");

        var mainUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "StartFullscreen", settings.DuckStation.StartFullscreen.ToString().ToLowerInvariant() },
            { "PauseOnFocusLoss", settings.DuckStation.PauseOnFocusLoss.ToString().ToLowerInvariant() },
            { "SaveStateOnExit", settings.DuckStation.SaveStateOnExit.ToString().ToLowerInvariant() },
            { "RewindEnable", settings.DuckStation.RewindEnable.ToString().ToLowerInvariant() },
            { "RunaheadFrameCount", settings.DuckStation.RunaheadFrameCount.ToString(CultureInfo.InvariantCulture) }
        };

        var gpuUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Renderer", settings.DuckStation.Renderer },
            { "ResolutionScale", settings.DuckStation.ResolutionScale.ToString(CultureInfo.InvariantCulture) },
            { "TextureFilter", settings.DuckStation.TextureFilter },
            { "WidescreenHack", settings.DuckStation.WidescreenHack.ToString().ToLowerInvariant() },
            { "PGXPEnable", settings.DuckStation.PgxpEnable.ToString().ToLowerInvariant() }
        };

        var displayUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "AspectRatio", settings.DuckStation.AspectRatio },
            { "VSync", settings.DuckStation.Vsync.ToString().ToLowerInvariant() }
        };

        var audioUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "OutputVolume", settings.DuckStation.OutputVolume.ToString(CultureInfo.InvariantCulture) },
            { "OutputMuted", settings.DuckStation.OutputMuted.ToString().ToLowerInvariant() }
        };

        List<string> lines;
        try
        {
            lines = File.ReadAllLines(configPath).ToList();
        }
        catch (UnauthorizedAccessException ex)
        {
            debugLogger.Log($"[DuckStationConfig] Access denied reading config: {configPath}");
            logErrors.LogAndForget(ex, $"[DuckStationConfig] Access denied reading config: {configPath}");
            throw;
        }
        catch (IOException ex)
        {
            debugLogger.Log($"[DuckStationConfig] I/O error reading config: {configPath}");
            logErrors.LogAndForget(ex, $"[DuckStationConfig] I/O error reading config: {configPath}");
            throw;
        }

        var modified = false;
        var currentSection = "";

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';')) continue;

            var parts = line.Split('=', 2);
            if (parts.Length < 2) continue;

            var key = parts[0].Trim();
            Dictionary<string, string> currentUpdates = null;

            if (currentSection.Equals("[Main]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = mainUpdates;
            }
            else if (currentSection.Equals("[GPU]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = gpuUpdates;
            }
            else if (currentSection.Equals("[Display]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = displayUpdates;
            }
            else if (currentSection.Equals("[Audio]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = audioUpdates;
            }

            if (currentUpdates != null && currentUpdates.Remove(key, out var newValue))
            {
                var newLine = $"{key} = {newValue}";
                if (lines[i] != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }
            }
        }

        // Add missing keys/sections
        if (mainUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[Main]", mainUpdates, out var sectionModified);
            modified |= sectionModified;
        }

        if (gpuUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[GPU]", gpuUpdates, out var sectionModified);
            modified |= sectionModified;
        }

        if (displayUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[Display]", displayUpdates, out var sectionModified);
            modified |= sectionModified;
        }

        if (audioUpdates.Count > 0)
        {
            ApplyUpdatesToSection(lines, "[Audio]", audioUpdates, out var sectionModified);
            modified |= sectionModified;
        }

        if (modified)
        {
            try
            {
                File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
                debugLogger.Log("[DuckStationConfig] Injected configuration changes..");
            }
            catch (Exception ex)
            {
                debugLogger.Log($"[DuckStationConfig] Failed to inject configuration changes: {ex.Message}");
                logErrors.LogAndForget(ex, $"[DuckStationConfig] Failed to inject configuration changes: {ex.Message}");
                throw;
            }
        }
        else
        {
            debugLogger.Log("[DuckStationConfig] No changes needed.");
        }
    }

    private static void ApplyUpdatesToSection(List<string> lines, string sectionName, Dictionary<string, string> updates, out bool modified)
    {
        if (updates.Count == 0)
        {
            modified = false;
            return;
        }

        var sectionIndex = lines.FindIndex(l => l.Trim().Equals(sectionName, StringComparison.OrdinalIgnoreCase));
        if (sectionIndex == -1)
        {
            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add("");
            lines.Add(sectionName);
            sectionIndex = lines.Count - 1;
        }

        // Find the end of this section (start of next section or end of file)
        var insertIndex = sectionIndex + 1;
        for (var i = insertIndex; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                // Found next section, insert before it
                insertIndex = i;
                break;
            }

            insertIndex = i + 1;
        }

        foreach (var kvp in updates)
        {
            lines.Insert(insertIndex++, $"{kvp.Key} = {kvp.Value}");
        }

        modified = true;
    }
}

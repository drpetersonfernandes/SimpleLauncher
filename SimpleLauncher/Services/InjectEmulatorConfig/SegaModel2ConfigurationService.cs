using System.Globalization;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class SegaModel2ConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, ILogErrors logErrors, IDebugLogger debugLogger)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "EMULATOR.INI");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "SEGA Model 2", "EMULATOR.INI");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    debugLogger.Log($"[SegaModel2Config] Created new EMULATOR.INI from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    debugLogger.Log($"[SegaModel2Config] Failed to create EMULATOR.INI from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[SegaModel2Config] Failed to create EMULATOR.INI from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("EMULATOR.INI not found and sample is missing.", samplePath);
            }
        }

        debugLogger.Log($"[SegaModel2Config] Injecting configuration into: {configPath}");

        var rendererUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "FullScreenWidth", settings.SegaModel2.ResX.ToString(CultureInfo.InvariantCulture) },
            { "FullScreenHeight", settings.SegaModel2.ResY.ToString(CultureInfo.InvariantCulture) },
            { "WideScreenWindow", settings.SegaModel2.WideScreen.ToString(CultureInfo.InvariantCulture) },
            { "Bilinear", settings.SegaModel2.Bilinear ? "1" : "0" },
            { "Trilinear", settings.SegaModel2.Trilinear ? "1" : "0" },
            { "FilterTilemaps", settings.SegaModel2.FilterTilemaps ? "1" : "0" },
            { "DrawCross", settings.SegaModel2.DrawCross ? "1" : "0" },
            { "FSAA", settings.SegaModel2.Fsaa.ToString(CultureInfo.InvariantCulture) }
        };

        var inputUpdates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "XInput", settings.SegaModel2.XInput ? "1" : "0" },
            { "EnableFF", settings.SegaModel2.EnableFf ? "1" : "0" },
            { "HoldGears", settings.SegaModel2.HoldGears ? "1" : "0" },
            { "UseRawInput", settings.SegaModel2.UseRawInput ? "1" : "0" }
        };

        var lines = File.ReadAllLines(configPath).ToList();
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

            if (currentSection.Equals("[Renderer]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = rendererUpdates;
            }
            else if (currentSection.Equals("[Input]", StringComparison.OrdinalIgnoreCase))
            {
                currentUpdates = inputUpdates;
            }

            if (currentUpdates != null && currentUpdates.TryGetValue(key, out var newValue))
            {
                var newLine = $"{key}={newValue}";
                if (lines[i] != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }

                currentUpdates.Remove(key); // Mark as found
            }
        }

        // Add any missing keys to their respective sections
        if (rendererUpdates.Count > 0 || inputUpdates.Count > 0)
        {
            modified = true;
            // Try to add to existing sections
            if (rendererUpdates.Count > 0)
            {
                var rendererIndex = lines.FindIndex(static l => l.Trim().Equals("[Renderer]", StringComparison.OrdinalIgnoreCase));
                if (rendererIndex != -1)
                {
                    var insertIndex = rendererIndex + 1;
                    while (insertIndex < lines.Count && !string.IsNullOrWhiteSpace(lines[insertIndex]) && !lines[insertIndex].Trim().StartsWith('['))
                    {
                        insertIndex++;
                    }

                    foreach (var kvp in rendererUpdates)
                    {
                        lines.Insert(insertIndex++, $"{kvp.Key}={kvp.Value}");
                    }
                }
                else
                {
                    // Section doesn't exist, add it at the end
                    lines.Add("[Renderer]");
                    foreach (var kvp in rendererUpdates)
                    {
                        lines.Add($"{kvp.Key}={kvp.Value}");
                    }
                }
            }

            if (inputUpdates.Count > 0)
            {
                var inputIndex = lines.FindIndex(static l => l.Trim().Equals("[Input]", StringComparison.OrdinalIgnoreCase));
                if (inputIndex != -1)
                {
                    var insertIndex = inputIndex + 1;
                    while (insertIndex < lines.Count && !string.IsNullOrWhiteSpace(lines[insertIndex]) && !lines[insertIndex].Trim().StartsWith('['))
                    {
                        insertIndex++;
                    }

                    foreach (var kvp in inputUpdates)
                    {
                        lines.Insert(insertIndex++, $"{kvp.Key}={kvp.Value}");
                    }
                }
                else
                {
                    // Section doesn't exist, add it at the end
                    lines.Add("[Input]");
                    foreach (var kvp in inputUpdates)
                    {
                        lines.Add($"{kvp.Key}={kvp.Value}");
                    }
                }
            }
        }

        if (modified)
        {
            try
            {
                File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
                debugLogger.Log("[SegaModel2Config] Injected configuration changes.");
            }
            catch (Exception ex)
            {
                debugLogger.Log($"[SegaModel2Config] Failed to inject configuration changes: {ex.Message}");
                logErrors.LogAndForget(ex, $"[SegaModel2Config] Failed to inject configuration changes: {ex.Message}");
                throw;
            }
        }
        else
        {
            debugLogger.Log("[SegaModel2Config] No changes needed.");
        }
    }
}
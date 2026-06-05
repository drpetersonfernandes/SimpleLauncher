using System.Globalization;
using System.IO;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class MednafenConfigurationService
{
    // List of common system prefixes used by Mednafen for per-system settings
    private static readonly string[] SystemPrefixes =
    [
        "apple2", "gb", "gba", "gg", "lynx", "md", "nes", "ngp", "pce", "pce_fast",
        "pcfx", "psx", "sms", "snes", "snes_faust", "ss", "vb", "wswan"
    ];

    private static readonly char[] Separator = [' ', '\t'];

    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, ILogErrors logErrors)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "mednafen.cfg");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Mednafen", "mednafen.cfg");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[MednafenConfig] Created new mednafen.cfg from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[MednafenConfig] Failed to create mednafen.cfg from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[MednafenConfig] Failed to create mednafen.cfg from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                // If no config and no sample, we can't proceed.
                throw new FileNotFoundException("mednafen.cfg not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[MednafenConfig] Injecting configuration into: {configPath}");

        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Global settings
            { "video.driver", settings.Mednafen.VideoDriver },
            { "video.fs", settings.Mednafen.Fullscreen ? "1" : "0" },
            { "video.glvsync", settings.Mednafen.Vsync ? "1" : "0" },
            { "video.blit_timesync", settings.Mednafen.Vsync ? "1" : "0" },
            { "sound.volume", settings.Mednafen.Volume.ToString(CultureInfo.InvariantCulture) },
            { "cheats", settings.Mednafen.Cheats ? "1" : "0" },
            { "state_rewind", settings.Mednafen.Rewind ? "1" : "0" }
        };

        // Add per-system settings for all common systems
        foreach (var prefix in SystemPrefixes)
        {
            updates[$"{prefix}.stretch"] = settings.Mednafen.Stretch;
            updates[$"{prefix}.videoip"] = settings.Mednafen.Bilinear ? "1" : "0";
            updates[$"{prefix}.scanlines"] = settings.Mednafen.Scanlines.ToString(CultureInfo.InvariantCulture);
            updates[$"{prefix}.shader"] = settings.Mednafen.Shader;
            updates[$"{prefix}.special"] = settings.Mednafen.Special;
        }

        var lines = File.ReadAllLines(configPath).ToList();
        var keysFound = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var modified = false;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';')) continue;

            var parts = line.Split(Separator, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;

            var key = parts[0];
            if (updates.TryGetValue(key, out var newValue))
            {
                var newLine = $"{key} {newValue}";
                if (lines[i] != newLine)
                {
                    lines[i] = newLine;
                    modified = true;
                }

                keysFound.Add(key);
            }
        }

        // Append any keys that were not found in the file
        foreach (var kvp in updates)
        {
            if (!keysFound.Contains(kvp.Key))
            {
                lines.Add($"{kvp.Key} {kvp.Value}");
                modified = true;
            }
        }

        if (modified)
        {
            try
            {
                File.WriteAllLines(configPath, lines, new UTF8Encoding(false));
                DebugLogger.Log("[MednafenConfig] Injected configuration changes..");
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[MednafenConfig] Failed to inject configuration changes: {ex.Message}");
                logErrors.LogAndForget(ex, $"[MednafenConfig] Failed to inject configuration changes: {ex.Message}");
                throw;
            }
        }
        else
        {
            DebugLogger.Log("[MednafenConfig] No changes needed.");
        }
    }
}
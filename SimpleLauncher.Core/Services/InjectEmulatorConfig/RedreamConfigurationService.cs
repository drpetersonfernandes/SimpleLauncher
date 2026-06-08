using System.Globalization;
using System.IO;
using System.Text;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Core.Services.InjectEmulatorConfig;

public static class RedreamConfigurationService
{
    public static void InjectSettings(string emulatorPath, Core.Services.SettingsManager.SettingsManager settings, ILogErrors logErrors, IDebugLogger debugLogger)
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
                    debugLogger.Log($"[RedreamConfig] Created new redream.cfg from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    debugLogger.Log($"[RedreamConfig] Failed to create redream.cfg from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[RedreamConfig] Failed to create redream.cfg from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("redream.cfg not found and sample is missing.", samplePath);
            }
        }

        debugLogger.Log($"[RedreamConfig] Injecting configuration into: {configPath}");

        var isWindowed = IsWindowedMode(settings.Redream.Fullmode);

        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "cable", settings.Redream.Cable },
            { "broadcast", settings.Redream.Broadcast },
            { "language", settings.Redream.Language },
            { "region", settings.Redream.Region },
            { "vsync", settings.Redream.Vsync ? "1" : "0" },
            { "frameskip", settings.Redream.Frameskip ? "1" : "0" },
            { "aspect", settings.Redream.Aspect },
            { "res", settings.Redream.Res.ToString(CultureInfo.InvariantCulture) },
            { "renderer", settings.Redream.Renderer },
            { "volume", settings.Redream.Volume.ToString(CultureInfo.InvariantCulture) },
            { "latency", settings.Redream.Latency.ToString(CultureInfo.InvariantCulture) },
            { "framerate", settings.Redream.Framerate ? "1" : "0" }
        };

        // Handle window/fullscreen mode correctly
        if (isWindowed) // Changed from: if (settings.Redream.Fullmode == "windowed")
        {
            updates["mode"] = "windowed";
            updates["width"] = settings.Redream.Width.ToString(CultureInfo.InvariantCulture);
            updates["height"] = settings.Redream.Height.ToString(CultureInfo.InvariantCulture);
        }
        else
        {
            updates["mode"] = "fullscreen";
            updates["fullmode"] = settings.Redream.Fullmode;
            updates["fullwidth"] = settings.Redream.Width.ToString(CultureInfo.InvariantCulture);
            updates["fullheight"] = settings.Redream.Height.ToString(CultureInfo.InvariantCulture);
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
                debugLogger.Log("[RedreamConfig] Injected configuration changes..");
            }
            catch (Exception ex)
            {
                debugLogger.Log($"[RedreamConfig] Failed to inject configuration changes: {ex.Message}");
                logErrors.LogAndForget(ex, $"[RedreamConfig] Failed to inject configuration changes: {ex.Message}");
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
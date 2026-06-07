using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class MesenConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings, ILogErrors logErrors)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory is null or empty.");

        var configPath = Path.Combine(emuDir, "settings.json");

        // Create from sample if missing
        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Mesen", "settings.json");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[MesenConfig] Created new settings.json from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[MesenConfig] Failed to create settings.json from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[MesenConfig] Failed to create settings.json from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException($"settings.json not found in {emuDir} and sample not available at {samplePath}");
            }
        }

        DebugLogger.Log($"[MesenConfig] Injecting configuration into: {configPath}");

        try
        {
            var jsonContent = File.ReadAllText(configPath);
            var root = JsonNode.Parse(jsonContent)?.AsObject();

            if (root == null)
            {
                throw new InvalidDataException("Failed to parse Mesen settings.json as a valid JSON object.");
            }

            // [Video]
            var video = GetOrCreateObject(root, "Video");
            video["UseExclusiveFullscreen"] = settings.Mesen.Fullscreen;

            // Map UI aspect ratio values to Mesen enum values
            var aspectRatio = settings.Mesen.AspectRatio switch
            {
                "4:3" => "Standard",
                "16:9" => "Widescreen",
                "Auto" => "Auto",
                "NoStretching" => "NoStretching",
                _ => settings.Mesen.AspectRatio
            };
            video["AspectRatio"] = aspectRatio;
            video["VerticalSync"] = settings.Mesen.Vsync;
            video["UseBilinearInterpolation"] = settings.Mesen.Bilinear;
            video["VideoFilter"] = settings.Mesen.VideoFilter;

            // [Audio]
            var audio = GetOrCreateObject(root, "Audio");
            audio["EnableAudio"] = settings.Mesen.EnableAudio;
            audio["MasterVolume"] = settings.Mesen.MasterVolume;

            // [Preferences]
            var preferences = GetOrCreateObject(root, "Preferences");
            preferences["EnableRewind"] = settings.Mesen.Rewind;
            preferences["PauseWhenInBackground"] = settings.Mesen.PauseInBackground;

            // [Emulation]
            var emulation = GetOrCreateObject(root, "Emulation");
            emulation["RunAheadFrames"] = settings.Mesen.RunAhead;

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(configPath, root.ToJsonString(options));

            DebugLogger.Log("[MesenConfig] Injection successful.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MesenConfig] Error injecting settings: {ex.Message}");
            logErrors.LogAndForget(ex, $"[MesenConfig] Error injecting settings: {ex.Message}");
            throw; // Re-throw to be caught by the caller
        }
    }

    private static JsonObject GetOrCreateObject(JsonObject parent, string key)
    {
        if (parent.ContainsKey(key) && parent[key] is JsonObject existingObject)
        {
            return existingObject;
        }

        var newObject = new JsonObject();
        parent[key] = newObject;
        return newObject;
    }
}
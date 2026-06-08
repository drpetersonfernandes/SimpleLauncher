using System.Text;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using YamlDotNet.Serialization;

namespace SimpleLauncher.Core.Services.InjectEmulatorConfig;

public static class Rpcs3ConfigurationService
{
    public static void InjectSettings(string emulatorPath, Core.Services.SettingsManager.SettingsManager settings, ILogErrors logErrors, IDebugLogger debugLogger)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "config.yml");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "RPCS3", "config.yml");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    debugLogger.Log($"[RPCS3Config] Created new config.yml from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    debugLogger.Log($"[RPCS3Config] Failed to create config.yml from sample: {ex.Message}");
                    logErrors.LogAndForget(ex, $"[RPCS3Config] Failed to create config.yml from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("config.yml not found and sample is missing.", samplePath);
            }
        }

        debugLogger.Log($"[RPCS3Config] Injecting configuration into: {configPath}");

        var deserializer = new DeserializerBuilder().Build();
        var serializer = new SerializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<Dictionary<object, object>>(File.ReadAllText(configPath));

        // Core
        SetValue("Core", "PPU Decoder", settings.Rpcs3.PpuDecoder);
        SetValue("Core", "SPU Decoder", settings.Rpcs3.SpuDecoder);

        // Video
        SetValue("Video", "Renderer", settings.Rpcs3.Renderer);
        SetValue("Video", "Resolution", settings.Rpcs3.Resolution);
        SetValue("Video", "Aspect ratio", settings.Rpcs3.AspectRatio);
        SetValue("Video", "VSync", settings.Rpcs3.Vsync);
        SetValue("Video", "Resolution Scale", settings.Rpcs3.ResolutionScale);
        SetValue("Video", "Anisotropic Filter Override", settings.Rpcs3.AnisotropicFilter);

        // Audio
        SetValue("Audio", "Renderer", settings.Rpcs3.AudioRenderer);
        SetValue("Audio", "Enable Buffering", settings.Rpcs3.AudioBuffering);

        // Miscellaneous
        SetValue("Miscellaneous", "Start games in fullscreen mode", settings.Rpcs3.StartFullscreen);

        var updatedYaml = serializer.Serialize(yamlObject);
        File.WriteAllText(configPath, updatedYaml, new UTF8Encoding(false));
        debugLogger.Log("[RPCS3Config] Injection successful.");
        return;

        // Helper to navigate and set values
        void SetValue(string section, string key, object value)
        {
            if (!yamlObject.TryGetValue(section, out var sectionObj))
            {
                // Create section if it doesn't exist
                sectionObj = new Dictionary<object, object>();
                yamlObject[section] = sectionObj;
            }

            if (sectionObj is not Dictionary<object, object> sectionDict)
            {
                // Replace non-dictionary value with a dictionary
                sectionDict = new Dictionary<object, object>();
                yamlObject[section] = sectionDict;
            }

            sectionDict[key] = value;
        }
    }
}
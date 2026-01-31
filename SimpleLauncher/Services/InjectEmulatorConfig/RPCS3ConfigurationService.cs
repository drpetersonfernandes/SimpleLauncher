using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;
using YamlDotNet.Serialization;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class Rpcs3ConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
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
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[RPCS3Config] Created new config.yml from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("config.yml not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[RPCS3Config] Injecting configuration into: {configPath}");

        var deserializer = new DeserializerBuilder().Build();
        var serializer = new SerializerBuilder().Build();
        var yamlObject = deserializer.Deserialize<Dictionary<object, object>>(File.ReadAllText(configPath));

        // Core
        SetValue("Core", "PPU Decoder", settings.Rpcs3PpuDecoder);
        SetValue("Core", "SPU Decoder", settings.Rpcs3SpuDecoder);

        // Video
        SetValue("Video", "Renderer", settings.Rpcs3Renderer);
        SetValue("Video", "Resolution", settings.Rpcs3Resolution);
        SetValue("Video", "Aspect ratio", settings.Rpcs3AspectRatio);
        SetValue("Video", "VSync", settings.Rpcs3Vsync);
        SetValue("Video", "Resolution Scale", settings.Rpcs3ResolutionScale);
        SetValue("Video", "Anisotropic Filter Override", settings.Rpcs3AnisotropicFilter);

        // Audio
        SetValue("Audio", "Renderer", settings.Rpcs3AudioRenderer);
        SetValue("Audio", "Enable Buffering", settings.Rpcs3AudioBuffering);

        // Miscellaneous
        SetValue("Miscellaneous", "Start games in fullscreen mode", settings.Rpcs3StartFullscreen);

        var updatedYaml = serializer.Serialize(yamlObject);
        File.WriteAllText(configPath, updatedYaml, new UTF8Encoding(false));
        DebugLogger.Log("[RPCS3Config] Injection successful.");
        return;

        // Helper to navigate and set values
        void SetValue(string section, string key, object value)
        {
            if (yamlObject.TryGetValue(section, out var sectionObj) && sectionObj is Dictionary<object, object> sectionDict)
            {
                sectionDict[key] = value;
            }
        }
    }
}

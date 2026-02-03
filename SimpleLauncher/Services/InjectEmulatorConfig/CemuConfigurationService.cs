using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class CemuConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "settings.xml");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Cemu", "settings.xml");
            if (File.Exists(samplePath))
            {
                try
                {
                    File.Copy(samplePath, configPath);
                    DebugLogger.Log($"[CemuConfig] Trying to creat new settings.xml from sample: {configPath}");
                }
                catch (Exception ex)
                {
                    DebugLogger.Log($"[CemuConfig] Failed to create settings.xml from sample: {ex.Message}");
                    _ = App.ServiceProvider.GetService<ILogErrors>()?.LogErrorAsync(ex, $"[CemuConfig] Failed to create settings.xml from sample: {ex.Message}");
                    throw;
                }
            }
            else
            {
                throw new FileNotFoundException("settings.xml not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[CemuConfig] Injecting configuration into: {configPath}");

        try
        {
            var doc = XDocument.Load(configPath);
            var content = doc.Element("content");

            if (content == null) throw new InvalidDataException("Invalid Cemu settings.xml format.");

            // Root level elements
            SetOrUpdateElement(content, "fullscreen", settings.CemuFullscreen.ToString().ToLowerInvariant());
            SetOrUpdateElement(content, "use_discord_presence", settings.CemuDiscordPresence.ToString().ToLowerInvariant());
            SetOrUpdateElement(content, "console_language", settings.CemuConsoleLanguage.ToString(CultureInfo.InvariantCulture));

            // Graphic Section
            var graphic = GetOrCreateElement(content, "Graphic");
            SetOrUpdateElement(graphic, "api", settings.CemuGraphicApi.ToString(CultureInfo.InvariantCulture));
            SetOrUpdateElement(graphic, "VSync", settings.CemuVsync.ToString(CultureInfo.InvariantCulture));
            SetOrUpdateElement(graphic, "AsyncCompile", settings.CemuAsyncCompile.ToString().ToLowerInvariant());

            // Audio Section
            var audio = GetOrCreateElement(content, "Audio");
            SetOrUpdateElement(audio, "TVVolume", settings.CemuTvVolume.ToString(CultureInfo.InvariantCulture));

            // Preserve original formatting by using XmlWriter
            var writerSettings = new System.Xml.XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                Encoding = System.Text.Encoding.UTF8
            };
            using var writer = System.Xml.XmlWriter.Create(configPath, writerSettings);
            doc.Save(writer);

            DebugLogger.Log("[CemuConfig] Injection successful.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[CemuConfig] Error: {ex.Message}");
            throw;
        }
    }

    private static XElement GetOrCreateElement(XElement parent, string name)
    {
        if (name != null)
        {
            var el = parent.Element(name);
            if (el != null) return el;

            el = new XElement(name);
            parent.Add(el);
            return el;
        }

        return null;
    }

    private static void SetOrUpdateElement(XElement parent, string name, string value)
    {
        if (name != null)
        {
            var el = parent.Element(name);
            if (el != null)
            {
                el.Value = value;
            }
            else parent.Add(new XElement(name, value));
        }
    }
}
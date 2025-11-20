using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

[XmlRoot("EasyMode")]
public class EasyModeManager
{
    [XmlElement("EasyModeSystemConfig")]
    public List<EasyModeSystemConfig> Systems { get; set; }

    public static EasyModeManager Load()
    {
        // Determine the XML file based on system architecture
        var xmlFile = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "easymode.xml",
                Architecture.Arm64 => "easymode_arm64.xml",
                _ => "easymode.xml" // Default fallback
            }
            : "easymode.xml"; // Default fallback

        var xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);

        // Check if xmlFile exists before proceeding.
        if (!File.Exists(xmlFilePath))
        {
            // Notify developer
            var contextMessage = $"The file '{xmlFile}' was not found in the application folder.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

            // Notify the user.
            MessageBoxLibrary.ErrorLoadingEasyModeXmlMessageBox();

            // Return an empty config to avoid further null reference issues
            return new EasyModeManager { Systems = [] };
        }

        try
        {
            var serializer = new XmlSerializer(typeof(EasyModeManager));

            // Open the file
            using var fileStream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            // Create XmlReaderSettings to disable DTD processing and set XmlResolver to null
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            // Create XmlReader with the settings
            using var xmlReader = XmlReader.Create(fileStream, settings);

            var config = (EasyModeManager)serializer.Deserialize(xmlReader);

            // Validate configuration if not null.
            if (config != null)
            {
                config.Validate(); // Exclude invalid systems
                return config;
            }
        }
        catch (InvalidOperationException ex)
        {
            // Notify developer
            var contextMessage = $"The file '{xmlFile}' is corrupted or invalid.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            return new EasyModeManager { Systems = [] }; // Return an empty config
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"An unexpected error occurred while loading the file '{xmlFile}'.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            return new EasyModeManager { Systems = [] }; // Return an empty config
        }

        return new EasyModeManager { Systems = [] }; // Return an empty config
    }

    public void Validate()
    {
        Systems = Systems?.Where(static system => system.IsValid()).ToList() ?? [];
    }
}
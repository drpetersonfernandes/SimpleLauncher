using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher;

[XmlRoot("EasyMode")]
public class EasyModeManager
{
    [XmlElement("EasyModeSystemConfig")]
    public List<EasyModeSystemConfig> Systems { get; set; }

    public static EasyModeManager Load()
    {
        const string xmlFile = "easymode.xml";
        var xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);

        // Check if xmlFile exists before proceeding.
        if (!File.Exists(xmlFilePath))
        {
            // Notify developer
            const string contextMessage = "The file 'easymode.xml' was not found in the application folder.";
            var ex = new FileNotFoundException($"File not found: {xmlFilePath}");
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
            const string contextMessage = "The file 'easymode.xml' is corrupted or invalid.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Return an empty config to avoid further null reference issues
            return new EasyModeManager { Systems = [] };
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An unexpected error occurred while loading the file 'easymode.xml'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Return an empty config to avoid further null reference issues
            return new EasyModeManager { Systems = [] };
        }

        // Return an empty config to avoid further null reference issues
        return new EasyModeManager { Systems = [] };
    }

    public void Validate()
    {
        Systems = Systems?.Where(static system => system.IsValid()).ToList() ?? [];
    }
}
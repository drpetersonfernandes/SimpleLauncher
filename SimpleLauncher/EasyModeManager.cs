using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

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

        // Check if the file exists before proceeding.
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
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An unexpected error occurred while loading the file 'easymode.xml'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }

        // Return an empty config to avoid further null reference issues
        return new EasyModeManager { Systems = [] };
    }

    public void Validate()
    {
        Systems = Systems?.Where(system => system.IsValid()).ToList() ?? [];
    }
}

public class EasyModeSystemConfig
{
    public string SystemName { get; set; }
    public string SystemFolder { get; set; }
    public string SystemImageFolder { get; set; }

    [XmlElement("SystemIsMAME")]
    public bool? SystemIsMameNullable { get; set; }

    [XmlIgnore]
    public bool SystemIsMame => SystemIsMameNullable ?? false;

    [XmlArray("FileFormatsToSearch")]
    [XmlArrayItem("FormatToSearch")]
    public List<string> FileFormatsToSearch { get; set; }

    [XmlElement("ExtractFileBeforeLaunch")]
    public bool? ExtractFileBeforeLaunchNullable { get; set; }

    [XmlIgnore]
    public bool ExtractFileBeforeLaunch => ExtractFileBeforeLaunchNullable ?? false;

    [XmlArray("FileFormatsToLaunch")]
    [XmlArrayItem("FormatToLaunch")]
    public List<string> FileFormatsToLaunch { get; set; }

    [XmlElement("Emulators")]
    public EmulatorsConfig Emulators { get; set; }

    public bool IsValid()
    {
        // Validate only SystemName; Emulators and nested Emulator can be null.
        return !string.IsNullOrWhiteSpace(SystemName);
    }
}

public class EmulatorsConfig
{
    [XmlElement("Emulator")]
    public EmulatorConfig Emulator { get; set; }
}

public class EmulatorConfig
{
    public string EmulatorName { get; set; }
    public string EmulatorLocation { get; set; }
    public string EmulatorParameters { get; set; }
    public string EmulatorDownloadPage { get; set; }
    public string EmulatorLatestVersion { get; set; }
    public string EmulatorDownloadLink { get; set; }

    [XmlElement("EmulatorDownloadRename")]
    public bool? EmulatorDownloadRenameNullable { get; set; }

    [XmlIgnore]
    public bool EmulatorDownloadRename => EmulatorDownloadRenameNullable ?? false;

    public string EmulatorDownloadExtractPath { get; set; }
    public string CoreLocation { get; set; }
    public string CoreLatestVersion { get; set; }
    public string CoreDownloadLink { get; set; }
    public string CoreDownloadExtractPath { get; set; }
    public string ExtrasLocation { get; set; }
    public string ExtrasLatestVersion { get; set; }
    public string ExtrasDownloadLink { get; set; }
    public string ExtrasDownloadExtractPath { get; set; }
}
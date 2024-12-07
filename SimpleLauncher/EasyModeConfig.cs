using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleLauncher;

[XmlRoot("EasyMode")]
public class EasyModeConfig
{
    [XmlElement("EasyModeSystemConfig")]
    public List<EasyModeSystemConfig> Systems { get; set; }

    public static EasyModeConfig Load()
    {
        string xmlFile = "easymode.xml";
        string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(EasyModeConfig));
            using FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open);
            var config = (EasyModeConfig)serializer.Deserialize(fileStream);
            if (config != null)
            {
                config.Validate(); // Exclude invalid systems
                return config;
            }
        }
        catch (InvalidOperationException ex)
        {
            ShowErrorMessage("The file 'easymode.xml' is corrupted or invalid. Please reinstall 'Simple Launcher'.", ex);
        }
        catch (FileNotFoundException)
        {
            ShowErrorMessage($"The file 'easymode.xml' was not found. Please reinstall 'Simple Launcher'.");
        }
        catch (Exception ex)
        {
            ShowErrorMessage("An unexpected error occurred while loading the file 'easymode.xml'.\n\n" +
                             "The error was reported to the developer that will try to fix the issue.", ex);
        }

        // Return an empty config to avoid further null reference issues
        return new EasyModeConfig { Systems = [] };
    }

    private static void ShowErrorMessage(string message, Exception ex = null)
    {
        string errorDetails = ex != null ? $"{message}\n\n" +
                                           $"Exception type: {ex.GetType().Name}\n" +
                                           $"Error Details: {ex.Message}" : string.Empty;
        Task logTask = LogErrors.LogErrorAsync(ex, errorDetails);
        logTask.Wait(TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Validates the list of systems and excludes invalid ones.
    /// </summary>
    public void Validate()
    {
        Systems = Systems?.Where(system => system.IsValid()).ToList() ?? new List<EasyModeSystemConfig>();
    }
}

public class EasyModeSystemConfig
{
    public string SystemName { get; set; }
    public string SystemFolder { get; set; }
    public string SystemImageFolder { get; set; }

    [XmlElement("SystemIsMame")]
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

    /// <summary>
    /// Validates the system configuration.
    /// </summary>
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

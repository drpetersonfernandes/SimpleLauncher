using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
            // Notify developer
            string errorMessage = "The file 'easymode.xml' is corrupted or invalid.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Error Details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            ErrorLoadingEasyModeXmlMessageBox();
        }
        catch (FileNotFoundException ex)
        {
            // Notify developer
            string errorMessage = "The file 'easymode.xml' was not found.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Error Details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            ErrorLoadingEasyModeXmlMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = "An unexpected error occurred while loading the file 'easymode.xml'.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Error Details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            ErrorLoadingEasyModeXmlMessageBox();
        }

        // Return an empty config to avoid further null reference issues
        return new EasyModeConfig { Systems = [] };

        void ErrorLoadingEasyModeXmlMessageBox()
        {
            string errorloadingthefileeasymodexml2 = (string)Application.Current.TryFindResource("Errorloadingthefileeasymodexml") ?? "Error loading the file 'easymode.xml'.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix it?";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{errorloadingthefileeasymodexml2}\n\n" +
                                         $"{theerrorwasreportedtothedeveloper2}\n" +
                                         $"{doyouwanttoreinstallSimpleLauncher2}",
                error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                MessageBox.Show(pleasereinstallSimpleLaunchermanually2, 
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

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
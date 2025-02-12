using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace SimpleLauncher;

public class SystemConfig
{
    public string SystemName { get; set; }
    public string SystemFolder { get; set; }
    public string SystemImageFolder { get; set; }
    public bool SystemIsMame { get; set; }
    public List<string> FileFormatsToSearch { get; set; }
    public bool ExtractFileBeforeLaunch { get; set; }
    public List<string> FileFormatsToLaunch { get; set; }
    public List<Emulator> Emulators { get; set; }
        
    public class Emulator
    {
        public string EmulatorName { get; set; }
        public string EmulatorLocation { get; set; }
        public string EmulatorParameters { get; set; }
    }
        
    private static readonly string XmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

    public static List<SystemConfig> LoadSystemConfigs()
    {
        if (string.IsNullOrEmpty(XmlPath))
        {
            throw new ArgumentNullException(nameof(XmlPath), @"The path to the XML file cannot be null or empty.");
        }

        try
        {
            if (!File.Exists(XmlPath))
            {
                // Search for backup files in the application directory
                string directoryPath = Path.GetDirectoryName(XmlPath);
                    
                try
                {
                    if (directoryPath != null)
                    {
                        var backupFiles = Directory.GetFiles(directoryPath, "system_backup*.xml").ToList();
                        if (backupFiles.Count > 0)
                        {
                            // Sort the backup files by their creation time to find the most recent one
                            var mostRecentBackupFile = backupFiles.MaxBy(File.GetCreationTime);

                            // Notify user
                            FileSystemXmlNotFindMessageBox(mostRecentBackupFile);
                        }
                        else
                        {
                            // Create 'system.xml' using a prefilled 'system_model.xml'
                            string systemModel = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system_model.xml");

                            if (File.Exists(systemModel))
                            {
                                File.Copy(systemModel, XmlPath, false);
                            }
                            else
                            {
                                // Notify developer
                                string contextMessage = "'system_model.xml' was not found in the application folder.";
                                Exception ex = new Exception(contextMessage);
                                LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));
                                        
                                // Notify user
                                MessageBoxLibrary.SystemModelXmlIsMissingMessageBox();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    string contextMessage = $"The file 'system.xml' is corrupted or could not be open.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.SystemXmlIsCorruptedMessageBox();

                    return null;
                }
            }
            
            XDocument doc;
            
            try
            {
                doc = XDocument.Load(XmlPath);
            }
            catch (XmlException ex)
            {
                // Notify developer
                string errorDetailsDeveloper = $"The file 'system.xml' is badly corrupted at line {ex.LineNumber}, position {ex.LinePosition}.\n\n" +
                                               $"Exception type: {ex.GetType().Name}\n" +
                                               $"Exception details: {ex.Message}";
                LogErrors.LogErrorAsync(ex, errorDetailsDeveloper).Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                MessageBoxLibrary.FiLeSystemXmlIsCorruptedMessageBox();

                return null;
            }
            
            var systemConfigs = new List<SystemConfig>();
            var invalidConfigs = new Dictionary<XElement, string>();

            if (doc.Root != null)
            {
                foreach (var sysConfigElement in doc.Root.Elements("SystemConfig"))
                {
                    try
                    {
                        // Attempt to parse each system configuration.
                        if (sysConfigElement.Element("SystemName") == null ||
                            string.IsNullOrEmpty(sysConfigElement.Element("SystemName")?.Value))
                            throw new InvalidOperationException("Missing or empty 'System Name' in XML.");

                        if (sysConfigElement.Element("SystemFolder") == null ||
                            string.IsNullOrEmpty(sysConfigElement.Element("SystemFolder")?.Value))
                            throw new InvalidOperationException("Missing or empty 'System Folder' in XML.");

                        if (!bool.TryParse(sysConfigElement.Element("SystemIsMAME")?.Value, out bool systemIsMame))
                            throw new InvalidOperationException("Invalid or missing value for 'System Is MAME'.");

                        // Validate FileFormatsToSearch
                        var formatsToSearch = sysConfigElement.Element("FileFormatsToSearch")
                            ?.Elements("FormatToSearch")
                            .Select(e => e.Value.Trim())
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value)) // Ensure no empty or whitespace-only entries
                            .ToList();
                        if (formatsToSearch == null || formatsToSearch.Count == 0)
                            throw new InvalidOperationException("'File Extension To Search' should have at least one value.");
                        
                        // Validate ExtractFileBeforeLaunch
                        if (!bool.TryParse(sysConfigElement.Element("ExtractFileBeforeLaunch")?.Value,
                                out bool extractFileBeforeLaunch))
                            throw new InvalidOperationException("Invalid or missing value for 'Extract File Before Launch'.");
                        if (extractFileBeforeLaunch && !formatsToSearch.All(f => f == "zip" || f == "7z" || f == "rar"))
                            throw new InvalidOperationException("When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.");
                        
                        // Validate FileFormatsToLaunch
                        var formatsToLaunch = sysConfigElement.Element("FileFormatsToLaunch")
                            ?.Elements("FormatToLaunch")
                            .Select(e => e.Value.Trim())
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value)) // Ensure no empty or whitespace-only entries
                            .ToList();
                        if (extractFileBeforeLaunch && (formatsToLaunch == null || formatsToLaunch.Count == 0))
                            throw new InvalidOperationException("'File Extension To Launch' should have at least one value when 'Extract File Before Launch' is set to true.");

                        // Validate emulator configurations
                        var emulators = sysConfigElement.Element("Emulators")?.Elements("Emulator").Select(
                            emulatorElement =>
                            {
                                if (string.IsNullOrEmpty(emulatorElement.Element("EmulatorName")?.Value))
                                    throw new InvalidOperationException("'Emulator Name' should not be empty or null.");

                                return new Emulator
                                {
                                    EmulatorName = emulatorElement.Element("EmulatorName")?.Value,
                                    EmulatorLocation = emulatorElement.Element("EmulatorLocation")?.Value,
                                    EmulatorParameters =
                                        emulatorElement.Element("EmulatorParameters")
                                            ?.Value // It's okay if this is null or empty
                                };
                            }).ToList();

                        if (emulators == null || emulators.Count == 0)
                            throw new InvalidOperationException("Emulators list should not be empty or null.");

                        systemConfigs.Add(new SystemConfig
                        {
                            SystemName = sysConfigElement.Element("SystemName")?.Value,
                            SystemFolder = sysConfigElement.Element("SystemFolder")?.Value,
                            SystemImageFolder = sysConfigElement.Element("SystemImageFolder")?.Value,
                            SystemIsMame = systemIsMame,
                            ExtractFileBeforeLaunch = extractFileBeforeLaunch,
                            FileFormatsToSearch = formatsToSearch,
                            FileFormatsToLaunch = formatsToLaunch,
                            Emulators = emulators
                        });
                    }
                    catch (Exception ex)
                    {
                        string systemName = sysConfigElement.Element("SystemName")?.Value ?? "Unnamed System";
                        if (!invalidConfigs.ContainsKey(sysConfigElement))
                        {
                            invalidConfigs[sysConfigElement] = $"The system '{systemName}' was removed due to the following error(s):\n";
                        }
                        invalidConfigs[sysConfigElement] += $"- {ex.Message}\n";
                    }
               
                }
            
                // Remove any invalid configurations from the XML document
                foreach (var invalidConfig in invalidConfigs.Keys)
                {
                    invalidConfig.Remove();
                }
            
                // Save the corrected XML back to disk if any invalid configurations were removed
                if (invalidConfigs.Any())
                {
                    doc.Save(XmlPath);
                }
                
                // Notify user about each invalid configuration in a single message per system
                foreach (var error in invalidConfigs.Values)
                {
                    // Notify user
                    InvalidSystemConfigurationMessageBox();
                    void InvalidSystemConfigurationMessageBox()
                    {
                        string invalidSystemConfiguration2 = (string)Application.Current.TryFindResource("InvalidSystemConfiguration") ?? "Invalid System Configuration";
                        MessageBox.Show(error,
                            invalidSystemConfiguration2, MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            return systemConfigs;
        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"Error loading system configurations from 'system.xml'.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.SystemXmlIsCorruptedMessageBox();
            
            return null;
        }

        void FileSystemXmlNotFindMessageBox(string mostRecentBackupFile)
        {
            var restoreResult = MessageBoxLibrary.WouldYouLikeToRestoreTheLastBackupMessageBox();

            if (restoreResult == MessageBoxResult.Yes)
            {
                // Rename the most recent backup file to system.xml
                try
                {
                    File.Copy(mostRecentBackupFile, XmlPath, false);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    string contextMessage = $"'Simple Launcher' was unable to restore the last backup.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.SimpleLauncherWasUnableToRestoreBackupMessageBox();
                }
            }
        }
    }

    
}
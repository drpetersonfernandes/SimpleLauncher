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
        public string EmulatorName { get; init; }
        public string EmulatorLocation { get; init; }
        public string EmulatorParameters { get; init; }
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
            // Check for the existence of the system.xml
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
                            
                            string icouldnotfindthefile2 = (string)Application.Current.TryFindResource("Icouldnotfindthefile") ?? "I could not find the file";
                            string whichisrequiredtostart2 = (string)Application.Current.TryFindResource("whichisrequiredtostart") ?? ", which is required to start the application.";
                            string butIfoundabackupfile2 = (string)Application.Current.TryFindResource("ButIfoundabackupfile") ?? "But I found a backup file.";
                            string wouldyouliketorestore2 = (string)Application.Current.TryFindResource("Wouldyouliketorestore") ?? "Would you like to restore the last backup?";
                            string restoreBackup2 = (string)Application.Current.TryFindResource("RestoreBackup") ?? "Restore Backup?";
                            var restoreResult = MessageBox.Show($"{icouldnotfindthefile2} 'system.xml'{whichisrequiredtostart2}\n\n" +
                                                                $"{butIfoundabackupfile2}\n\n" +
                                                                $"{wouldyouliketorestore2}",
                                restoreBackup2, MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if (restoreResult == MessageBoxResult.Yes)
                            {
                                // Rename the most recent backup file to system.xml
                                File.Copy(mostRecentBackupFile, XmlPath, false);
                            }
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
                                string contextMessage = $"'system_model.xml' was not found in the application folder.";
                                Exception ex = new Exception(contextMessage);
                                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                                logTask.Wait(TimeSpan.FromSeconds(2));
                                        
                                // Ask the user if they want to automatically reinstall Simple Launcher
                                var messageBoxResult = MessageBox.Show(
                                    "The file 'system_model.xml' is missing.\n\n" +
                                    "'Simple Launcher' cannot work properly without this file.\n\n" +
                                    "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?",
                                    "Missing File", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                                if (messageBoxResult == MessageBoxResult.Yes)
                                {
                                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                                }
                                else
                                {
                                    MessageBox.Show("Please reinstall 'Simple Launcher' manually to fix the problem.\n\n" +
                                                    "The application will shut down.",
                                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                                    // Shutdown the application and exit
                                    Application.Current.Shutdown();
                                    Environment.Exit(0);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string contextMessage = $"The file 'system.xml' is corrupted or could not be open.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    MessageBox.Show($"'system.xml' is corrupted or could not be opened.\n" +
                                    $"Please fix it manually or delete it.\n" +
                                    $"If you choose to delete it, 'Simple Launcher' will create a new one for you.\n\n" +
                                    $"If you want to debug the error yourself, check the 'error_user.log' file inside the 'Simple Launcher' folder.\n\n" +
                                    $"The application will shut down.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    // Shutdown the application and exit
                    Application.Current.Shutdown();
                    Environment.Exit(0);
                    
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
                string errorDetails = $"The file 'system.xml' is badly corrupted at line {ex.LineNumber}, position {ex.LinePosition}.\n\n" +
                                      $"To see the details, check the 'error_user.log' file inside the 'Simple Launcher' folder.";
                MessageBox.Show(errorDetails,
                    "XML Load Error", MessageBoxButton.OK, MessageBoxImage.Error);

                string errorDetailsDeveloper = $"The file 'system.xml' is badly corrupted at line {ex.LineNumber}, position {ex.LinePosition}.\n\n" +
                                               $"Exception type: {ex.GetType().Name}\n" +
                                               $"Exception details: {ex.Message}";
                
                Task logTask = LogErrors.LogErrorAsync(ex, errorDetailsDeveloper);
                logTask.Wait(TimeSpan.FromSeconds(2));

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
                    MessageBox.Show(error, "Invalid System Configuration", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            return systemConfigs;
        }
        catch (Exception ex)
        {
            string contextMessage = $"Error loading system configurations from 'system.xml'.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            MessageBox.Show("'system.xml' is corrupted or could not be opened.\n\n" +
                            "Please fix it manually or delete it.\n\n" +
                            "If you choose to delete it, 'Simple Launcher' will create a new one for you.\n\n" +
                            "If you want to debug the error yourself, check the file 'error_user.log' inside the 'Simple Launcher' folder.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }
}
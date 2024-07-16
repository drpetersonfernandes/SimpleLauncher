using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public class SystemConfig
    {
        public string SystemName { get; private set; }
        public string SystemFolder { get; private set; }
        public string SystemImageFolder { get; private set; }
        public bool SystemIsMame { get; private set; }
        public List<string> FileFormatsToSearch { get; private set; }
        public bool ExtractFileBeforeLaunch { get; private set; }
        public List<string> FileFormatsToLaunch { get; private set; }
        public List<Emulator> Emulators { get; private set; }
        
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
                                MessageBoxResult restoreResult = MessageBox.Show("I could not find the file system.xml, which is required to start the application.\n\nBut I found a backup system file.\n\nWould you like to restore the last backup?", "Restore Backup?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (restoreResult == MessageBoxResult.Yes)
                                {
                                    // Rename the most recent backup file to system.xml
                                    File.Copy(mostRecentBackupFile, XmlPath, false); // Does not overwrite the file if it already exists
                                }
                            }
                            else
                            {
                                // Ask the user whether to create an empty system.xml or use a prefilled system_model.xml
                                MessageBoxResult createNewFileResult = MessageBox.Show("The file system.xml is missing. Would you like to create an empty system.xml or use a prefilled system_model.xml?\n\nClick Yes to create an empty system.xml.\nClick No to use the prefilled system_model.xml.", "Create System.xml", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (createNewFileResult == MessageBoxResult.Yes)
                                {
                                    // Create an empty system.xml
                                    var emptyDoc = new XDocument(new XElement("Systems"));
                                    emptyDoc.Save(XmlPath);
                                }
                                else
                                {
                                    try
                                    {
                                        // Location of system_model.xml
                                        string systemModel = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system_model.xml");

                                        // Rename system.model to system.xml
                                        File.Copy(systemModel, XmlPath, false); // Does not overwrite the file if it already exists
                                    }
                                    catch (Exception)
                                    {
                                        string contextMessage = $"The file system_model.xml is missing.\n\nThe application will be shutdown.\n\nPlease reinstall Simple Launcher to restore this file.";
                                        MessageBox.Show(contextMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

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
                        string contextMessage = $"The file system.xml is corrupted.\n\nException details: {ex}";
                        Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                        MessageBox.Show($"The system.xml is corrupted: {ex.Message}\n\nPlease fix it manually or delete it.\n\nIf you choose to delete it the application will create one for you.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                        return null;
                    }
                }

                var doc = XDocument.Load(XmlPath);
                var systemConfigs = new List<SystemConfig>();

                if (doc.Root != null)
                {
                    foreach (var sysConfigElement in doc.Root.Elements("SystemConfig"))
                    {
                        if (sysConfigElement.Element("SystemName") == null ||
                            string.IsNullOrEmpty(sysConfigElement.Element("SystemName")?.Value))
                            throw new InvalidOperationException("Missing or empty SystemName in XML.");

                        if (sysConfigElement.Element("SystemFolder") == null ||
                            string.IsNullOrEmpty(sysConfigElement.Element("SystemFolder")?.Value))
                            throw new InvalidOperationException("Missing or empty SystemFolder in XML.");

                        if (!bool.TryParse(sysConfigElement.Element("SystemIsMAME")?.Value, out bool systemIsMame))
                            throw new InvalidOperationException("Invalid or missing value for SystemIsMAME.");

                        var formatsToSearch = sysConfigElement.Element("FileFormatsToSearch")
                            ?.Elements("FormatToSearch").Select(e => e.Value).ToList();
                        if (formatsToSearch == null || formatsToSearch.Count == 0)
                            throw new InvalidOperationException("FileFormatsToSearch should have at least one value.");

                        if (!bool.TryParse(sysConfigElement.Element("ExtractFileBeforeLaunch")?.Value,
                                out bool extractFileBeforeLaunch))
                            throw new InvalidOperationException(
                                "Invalid or missing value for ExtractFileBeforeLaunch.");

                        var formatsToLaunch = sysConfigElement.Element("FileFormatsToLaunch")
                            ?.Elements("FormatToLaunch").Select(e => e.Value).ToList();
                        if (extractFileBeforeLaunch && (formatsToLaunch == null || formatsToLaunch.Count == 0))
                            throw new InvalidOperationException(
                                "FileFormatsToLaunch should have at least one value when ExtractFileBeforeLaunch is true.");

                        var emulators = sysConfigElement.Element("Emulators")?.Elements("Emulator").Select(
                            emulatorElement =>
                            {
                                if (string.IsNullOrEmpty(emulatorElement.Element("EmulatorName")?.Value))
                                    throw new InvalidOperationException("EmulatorName should not be empty or null.");

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
                }

                return systemConfigs;
            }
            catch (Exception ex2)
            {
                string contextMessage = $"Error loading system configurations from system.xml.\n\nException details: {ex2}";
                Task logTask = LogErrors.LogErrorAsync(ex2, contextMessage);
                MessageBox.Show($"The system.xml is broken: {ex2.Message}\n\nPlease fix it manually or delete it.\n\nIf you choose to delete it the application will create one for you.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logTask.Wait(TimeSpan.FromSeconds(2));
                return null;
            }
        }
    }
}

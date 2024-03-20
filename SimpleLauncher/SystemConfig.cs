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

        public static List<SystemConfig> LoadSystemConfigs(string xmlPath)
        {
            try
            {
                // Check for the existence of the system.xml file
                if (!File.Exists(xmlPath))
                {
                    // Search for backup files in the application directory
                    string directoryPath = Path.GetDirectoryName(xmlPath);
                    
                    try
                    {
                        var backupFiles = Directory.GetFiles(directoryPath!, "system_backup*.xml").ToList();
                        if (backupFiles.Count > 0)
                        {
                            // Sort the backup files by their creation time to find the most recent one
                            var mostRecentBackupFile = backupFiles.MaxBy(File.GetCreationTime);
                            MessageBoxResult restoreResult = MessageBox.Show("I could not find the file system.xml, which is required to start the application.\nBut I found a backup configuration file.\nWould you like to restore the last backup?", "Restore Backup?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (restoreResult == MessageBoxResult.Yes) 
                                try {
                                    // Rename the most recent backup file to system.xml
                                    File.Copy(mostRecentBackupFile, xmlPath!, false); // Does not Overwrite the file if it already exists
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
                                }
                        }
                        else
                        {
                            string systemModel = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.model");
                            // Prompt user to use the system_model.xml if no backup was found
                            MessageBoxResult restoreResult2 = MessageBox.Show(
                                "I could not find the file system.xml, which is required to start the application.\nI can create this file for you with pre configured values.\nCan I do that?",
                                "Create system.xml?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (restoreResult2 == MessageBoxResult.Yes)
                                try
                                {
                                    // Rename system.model to to system.xml
                                    File.Copy(systemModel, xmlPath!,
                                        false); // Does not Overwrite the file if it already exists
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    throw;
                                }
                        }
                        // else
                        // {
                        //     var defaultConfig = new SystemConfig();
                        //     defaultConfig.SetDefaultsAndSave(xmlPath);
                        //     return [defaultConfig];
                        // }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }

                }

                var doc = XDocument.Load(xmlPath!);
                var systemConfigs = new List<SystemConfig>();

                foreach (var sysConfigElement in doc.Root!.Elements("SystemConfig"))
                {
                    if (sysConfigElement.Element("SystemName") == null || string.IsNullOrEmpty(sysConfigElement.Element("SystemName")!.Value))
                        throw new InvalidOperationException("Missing or empty SystemName in XML.");

                    if (sysConfigElement.Element("SystemFolder") == null || string.IsNullOrEmpty(sysConfigElement.Element("SystemFolder")!.Value))
                        throw new InvalidOperationException("Missing or empty SystemFolder in XML.");
                    
                    if (!bool.TryParse(sysConfigElement.Element("SystemIsMAME")?.Value, out bool systemIsMame))
                        throw new InvalidOperationException("Invalid or missing value for SystemIsMAME.");

                    var formatsToSearch = sysConfigElement.Element("FileFormatsToSearch")?.Elements("FormatToSearch").Select(e => e.Value).ToList();
                    if (formatsToSearch == null || formatsToSearch.Count == 0)
                        throw new InvalidOperationException("FileFormatsToSearch should have at least one value.");

                    if (!bool.TryParse(sysConfigElement.Element("ExtractFileBeforeLaunch")?.Value, out bool extractFileBeforeLaunch))
                        throw new InvalidOperationException("Invalid or missing value for ExtractFileBeforeLaunch.");

                    var formatsToLaunch = sysConfigElement.Element("FileFormatsToLaunch")?.Elements("FormatToLaunch").Select(e => e.Value).ToList();
                    if (extractFileBeforeLaunch && (formatsToLaunch == null || formatsToLaunch.Count == 0))
                        throw new InvalidOperationException("FileFormatsToLaunch should have at least one value when ExtractFileBeforeLaunch is true.");

                    var emulators = sysConfigElement.Element("Emulators")?.Elements("Emulator").Select(emulatorElement =>
                    {
                        if (string.IsNullOrEmpty(emulatorElement.Element("EmulatorName")?.Value))
                            throw new InvalidOperationException("EmulatorName should not be empty or null.");

                        return new Emulator
                        {
                            EmulatorName = emulatorElement.Element("EmulatorName")!.Value,
                            EmulatorLocation = emulatorElement.Element("EmulatorLocation")!.Value,
                            EmulatorParameters = emulatorElement.Element("EmulatorParameters")?.Value // It's okay if this is null or empty
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

                return systemConfigs;
            }
            catch (Exception ex)
            {
                string contextMessage = $"Error loading system configurations from XML: {ex.Message}";
                // Log the error
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);

                MessageBox.Show($"The system.xml is broken: {ex.Message}\nPlease fix it manually or delete it.\nIf you choose to delete it the application will create one for you.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Wait the logger
                logTask.Wait(TimeSpan.FromSeconds(2));

                // Return null
                return null;
            }

        }
        
        // private void SetDefaultsAndSave(string xmlPath)
        // {
        //     SystemName = "Arcade";
        //     SystemFolder = @"c:\arcade";
        //     SystemImageFolder = @"c:\arcade\images";
        //     SystemIsMame = false;
        //     FileFormatsToSearch = ["zip"];
        //     ExtractFileBeforeLaunch = false;
        //     FileFormatsToLaunch = new List<string>(); // Empty list if no formats
        //     Emulators =
        //     [
        //         new Emulator
        //         {
        //             EmulatorName = "MAME",
        //             EmulatorLocation = @"c:\mame\mame.exe",
        //             EmulatorParameters = "-rompath \"c:\\mame\\roms\""
        //         }
        //     ];
        //
        //     Save(xmlPath);
        // }

        // private void Save(string xmlPath)
        // {
        //     var settings = new XElement("SystemConfigs",
        //         new XElement("SystemConfig",
        //             new XElement("SystemName", SystemName),
        //             new XElement("SystemFolder", SystemFolder),
        //             new XElement("SystemImageFolder", SystemImageFolder),
        //             new XElement("SystemIsMAME", SystemIsMame),
        //             new XElement("FileFormatsToSearch", FileFormatsToSearch.Select(f => new XElement("FormatToSearch", f))),
        //             new XElement("ExtractFileBeforeLaunch", ExtractFileBeforeLaunch),
        //             new XElement("FileFormatsToLaunch", FileFormatsToLaunch.Select(f => new XElement("FormatToLaunch", f))),
        //             new XElement("Emulators", Emulators.Select(e =>
        //                 new XElement("Emulator",
        //                     new XElement("EmulatorName", e.EmulatorName),
        //                     new XElement("EmulatorLocation", e.EmulatorLocation),
        //                     new XElement("EmulatorParameters", e.EmulatorParameters)
        //                 )
        //             ))
        //         )
        //     );
        //
        //     settings.Save(xmlPath);
        // }
    }
}
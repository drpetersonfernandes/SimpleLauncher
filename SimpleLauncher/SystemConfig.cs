using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace SimpleLauncher;

public class SystemConfig
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");

    public string SystemName { get; private init; }
    public string SystemFolder { get; private init; }
    public string SystemImageFolder { get; private init; }
    public bool SystemIsMame { get; private init; }
    public List<string> FileFormatsToSearch { get; private init; }
    public bool ExtractFileBeforeLaunch { get; set; }
    public List<string> FileFormatsToLaunch { get; private init; }
    public List<Emulator> Emulators { get; private init; }

    public class Emulator
    {
        public string EmulatorName { get; init; }
        public string EmulatorLocation { get; init; }
        public string EmulatorParameters { get; init; }
        public bool ReceiveANotificationOnEmulatorError { get; init; }
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
                var directoryPath = Path.GetDirectoryName(XmlPath);

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
                            var systemModel = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system_model.xml");

                            if (File.Exists(systemModel))
                            {
                                File.Copy(systemModel, XmlPath, false);
                            }
                            else
                            {
                                // Notify developer
                                const string contextMessage = "'system_model.xml' was not found in the application folder.";
                                var ex = new Exception(contextMessage);
                                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                                // Notify user
                                MessageBoxLibrary.SystemModelXmlIsMissingMessageBox();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "The file 'system.xml' is corrupted or could not be open.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.SystemXmlIsCorruptedMessageBox(LogPath);

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
                var contextMessage = $"The file 'system.xml' is badly corrupted at line {ex.LineNumber}, position {ex.LinePosition}.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.FiLeSystemXmlIsCorruptedMessageBox(LogPath);

                return null;
            }

            var systemConfigs = new List<SystemConfig>();
            var invalidConfigs = new Dictionary<XElement, string>();

            if (doc.Root == null) return systemConfigs;
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

                        if (!bool.TryParse(sysConfigElement.Element("SystemIsMAME")?.Value, out var systemIsMame))
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
                                out var extractFileBeforeLaunch))
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
                                
                                // Parse the ReceiveANotificationOnEmulatorError value with default = true
                                var receiveNotification = true; // Default to true
                                if (emulatorElement.Element("ReceiveANotificationOnEmulatorError") != null)
                                {
                                    // Only set to false if explicitly "false", otherwise keep default (true)
                                    if (!bool.TryParse(emulatorElement.Element("ReceiveANotificationOnEmulatorError")?.Value, out receiveNotification))
                                    {
                                        receiveNotification = true; // Reset to default if parsing fails
                                    }
                                }

                                return new Emulator
                                {
                                    EmulatorName = emulatorElement.Element("EmulatorName")?.Value,
                                    EmulatorLocation = emulatorElement.Element("EmulatorLocation")?.Value,
                                    EmulatorParameters = emulatorElement.Element("EmulatorParameters")?.Value, // It's okay if this is null or empty
                                    ReceiveANotificationOnEmulatorError = receiveNotification
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
                        var systemName = sysConfigElement.Element("SystemName")?.Value ?? "Unnamed System";
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
                if (invalidConfigs.Count != 0)
                {
                    doc.Save(XmlPath);
                }

                // Notify user about each invalid configuration in a single message per system
                foreach (var error in invalidConfigs.Values)
                {
                    // Notify user
                    MessageBoxLibrary.InvalidSystemConfigurationMessageBox(error);
                }
            }

            return systemConfigs;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error loading system configurations from 'system.xml'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.SystemXmlIsCorruptedMessageBox(LogPath);

            return null;
        }

        void FileSystemXmlNotFindMessageBox(string mostRecentBackupFile)
        {
            var restoreResult = MessageBoxLibrary.WouldYouLikeToRestoreTheLastBackupMessageBox();

            if (restoreResult != MessageBoxResult.Yes) return;

            // Rename the most recent backup file to system.xml
            try
            {
                File.Copy(mostRecentBackupFile, XmlPath, false);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "'Simple Launcher' was unable to restore the last backup.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SimpleLauncherWasUnableToRestoreBackupMessageBox();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

public partial class SystemManager
{
    private static readonly string LogPath = GetLogPath.Path();

    public string SystemName { get; private init; }
    public string SystemFolder { get; private init; }
    public string SystemImageFolder { get; private init; }
    public bool SystemIsMame { get; private init; }
    public List<string> FileFormatsToSearch { get; private init; }
    public bool ExtractFileBeforeLaunch { get; set; }
    public List<string> FileFormatsToLaunch { get; private init; }
    public List<Emulator> Emulators { get; private init; }

    private static readonly string XmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

    public static List<SystemManager> LoadSystemConfigs()
    {
        if (string.IsNullOrEmpty(XmlPath))
        {
            throw new ArgumentNullException(nameof(XmlPath), @"The path to the XML file cannot be null or empty.");
        }

        try
        {
            // Check if system.xml exists
            if (!File.Exists(XmlPath))
            {
                var directoryPath = Path.GetDirectoryName(XmlPath);
                var backupRestored = false;

                if (directoryPath != null)
                {
                    try
                    {
                        // Search for backup files in the application directory
                        var backupFiles = Directory.GetFiles(directoryPath, "system_backup*.xml").ToList();
                        if (backupFiles.Count > 0)
                        {
                            // Sort the backup files by their creation time to find the most recent one
                            var mostRecentBackupFile = backupFiles.MaxBy(File.GetCreationTime);

                            // Notify user and ask to restore
                            var restoreResult = MessageBoxLibrary.WouldYouLikeToRestoreTheLastBackupMessageBox();

                            if (restoreResult == MessageBoxResult.Yes)
                            {
                                try
                                {
                                    // Copy the most recent backup file to system.xml, overwriting if a dummy file exists
                                    File.Copy(mostRecentBackupFile, XmlPath, true);

                                    backupRestored = true;
                                }
                                catch (Exception ex)
                                {
                                    // Notify developer
                                    const string contextMessage = "'Simple Launcher' was unable to restore the last backup.";
                                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                                    // Notify user
                                    MessageBoxLibrary.SimpleLauncherWasUnableToRestoreBackupMessageBox();
                                    // backupRestored remains false, proceed to create empty file
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        // Error during backup search/restore attempt (e.g., directory access issues)
                        const string contextMessage = "Error during backup file handling.";
                        _ = LogErrors.LogErrorAsync(ex, contextMessage);
                        // Proceed to create empty file as backup handling failed
                    }
                }

                // If no backup was restored, create a new empty system.xml file
                if (!backupRestored)
                {
                    try
                    {
                        // Create a new XDocument with the root element
                        var emptyDoc = new XDocument(new XElement("SystemConfigs"));
                        emptyDoc.Save(XmlPath);
                        // No user notification needed for creating an expected empty file
                    }
                    catch (Exception createEx)
                    {
                        // Notify developer
                        const string contextMessage = "Error creating empty 'system.xml'.";
                        _ = LogErrors.LogErrorAsync(createEx, contextMessage);

                        // Notify user
                        MessageBoxLibrary.SystemXmlIsCorruptedMessageBox(LogPath);

                        return new List<SystemManager>(); // Return an empty list
                    }
                }
            }

            // At this point, system.xml exists (either original, restored backup, or newly created empty)
            XDocument doc;

            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };

                using var reader = XmlReader.Create(XmlPath, settings);
                doc = XDocument.Load(reader);
            }
            catch (XmlException ex)
            {
                // Notify developer
                var contextMessage = $"The file 'system.xml' is badly corrupted at line {ex.LineNumber}, position {ex.LinePosition}.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.FileSystemXmlIsCorruptedMessageBox(LogPath);

                return new List<SystemManager>(); // Return an empty list
            }

            var systemConfigs = new List<SystemManager>();
            var invalidConfigs = new Dictionary<XElement, string>();

            // If the root is null (e.g., empty file or invalid XML structure before root), return empty list
            if (doc.Root == null)
            {
                return systemConfigs;
            }

            // Iterate through SystemConfig elements. This loop will be skipped if the file is empty (<SystemConfigs/>).
            foreach (var sysConfigElement in doc.Root.Elements("SystemConfig"))
            {
                try
                {
                    // Attempt to parse each system configuration.
                    // These validations will only run if SystemConfig elements exist.
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
                        .Select(static e => e.Value.Trim())
                        .Where(static value =>
                            !string.IsNullOrWhiteSpace(value)) // Ensure no empty or whitespace-only entries
                        .ToList();
                    if (formatsToSearch == null || formatsToSearch.Count == 0)
                        throw new InvalidOperationException("'File Extension To Search' should have at least one value.");

                    // Validate ExtractFileBeforeLaunch
                    if (!bool.TryParse(sysConfigElement.Element("ExtractFileBeforeLaunch")?.Value,
                            out var extractFileBeforeLaunch))
                        throw new InvalidOperationException("Invalid or missing value for 'Extract File Before Launch'.");
                    if (extractFileBeforeLaunch && !formatsToSearch.All(static f => f is "zip" or "7z" or "rar"))
                        throw new InvalidOperationException("When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.");

                    // Validate FileFormatsToLaunch
                    var formatsToLaunch = sysConfigElement.Element("FileFormatsToLaunch")
                        ?.Elements("FormatToLaunch")
                        .Select(static e => e.Value.Trim())
                        .Where(static value =>
                            !string.IsNullOrWhiteSpace(value)) // Ensure no empty or whitespace-only entries
                        .ToList();
                    // This check was slightly modified: formatsToLaunch can be null/empty if ExtractFileBeforeLaunch is false.
                    // The original code had a check that failed if ExtractFileBeforeLaunch was true AND formatsToLaunch was null/empty.
                    // Let's keep the original logic's intent: if ExtractFileBeforeLaunch is true, FileFormatsToLaunch must have values.
                    if (extractFileBeforeLaunch && (formatsToLaunch == null || formatsToLaunch.Count == 0))
                        throw new InvalidOperationException("'File Extension To Launch' should have at least one value when 'Extract File Before Launch' is set to true.");


                    // Validate emulator configurations
                    var emulators = sysConfigElement.Element("Emulators")?.Elements("Emulator").Select(static emulatorElement =>
                    {
                        if (string.IsNullOrEmpty(emulatorElement.Element("EmulatorName")?.Value))
                            throw new InvalidOperationException("'Emulator Name' should not be empty or null.");

                        // Parse the ReceiveANotificationOnEmulatorError value with default = false
                        // If the element is missing or parsing fails, it defaults to false.
                        var receiveNotification = false; // Default value
                        if (emulatorElement.Element("ReceiveANotificationOnEmulatorError") == null)
                            return new Emulator
                            {
                                EmulatorName = emulatorElement.Element("EmulatorName")?.Value,
                                EmulatorLocation = emulatorElement.Element("EmulatorLocation")?.Value,
                                EmulatorParameters =
                                    emulatorElement.Element("EmulatorParameters")
                                        ?.Value, // It's okay if this is null or empty
                                ReceiveANotificationOnEmulatorError = receiveNotification
                            };

                        if (!bool.TryParse(emulatorElement.Element("ReceiveANotificationOnEmulatorError")?.Value, out receiveNotification))
                        {
                            receiveNotification = false; // Reset to default if parsing fails
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

                    systemConfigs.Add(new SystemManager
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

            // Return the list of valid system configurations (could be empty)
            return systemConfigs;
        }
        catch (Exception ex)
        {
            // Notify developer
            // Catch any other unexpected errors during the loading process
            const string contextMessage = "Error loading system configurations from 'system.xml'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.SystemXmlIsCorruptedMessageBox(LogPath);

            return new List<SystemManager>(); // Return an empty list
        }
    }
}

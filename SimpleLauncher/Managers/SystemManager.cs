using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using SimpleLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Managers;

public partial class SystemManager
{
    private static readonly string LogPath = GetLogPath.Path();

    public string SystemName { get; private init; }
    public List<string> SystemFolders { get; private init; }
    public string PrimarySystemFolder => SystemFolders?.FirstOrDefault();
    public string SystemImageFolder { get; private init; }
    public bool SystemIsMame { get; private init; }
    public List<string> FileFormatsToSearch { get; private init; }
    public bool ExtractFileBeforeLaunch { get; private set; }
    public List<string> FileFormatsToLaunch { get; private init; }
    public List<Emulator> Emulators { get; private init; }
    public bool GroupByFolder { get; private init; }

    private static readonly string XmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

    public static List<SystemManager> LoadSystemManagers()
    {
        if (string.IsNullOrEmpty(XmlPath))
        {
            throw new ArgumentNullException(nameof(XmlPath), @"The path to the XML file cannot be null or empty.");
        }

        // Notify user
        Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingSystemConfigurations") ?? "Loading system configurations...", Application.Current.MainWindow as MainWindow));

        try
        {
            if (!File.Exists(XmlPath))
            {
                var directoryPath = Path.GetDirectoryName(XmlPath);
                var backupRestored = false;

                if (directoryPath != null)
                {
                    backupRestored = RestoreBackupFile(directoryPath, backupRestored);
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
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(createEx, contextMessage);

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
                // Load the XML document
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };

                using var reader = XmlReader.Create(XmlPath, settings);
                doc = XDocument.Load(reader, LoadOptions.None);
            }
            catch (XmlException ex)
            {
                // Notify developer
                var contextMessage = $"The file 'system.xml' is badly corrupt at line {ex.LineNumber}, position {ex.LinePosition}.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.FileSystemXmlIsCorruptedMessageBox(LogPath);

                return new List<SystemManager>(); // Return an empty list
            }

            var systemManagers = new List<SystemManager>();
            var invalidManagers = new Dictionary<XElement, string>();

            // If the root is null (e.g., empty file or invalid XML structure before root), return empty list
            if (doc.Root == null)
            {
                return systemManagers;
            }

            // Iterate through SystemManager elements. This loop will be skipped if the file is empty (<SystemConfigs/>).
            foreach (var sysConfigElement in doc.Root.Elements("SystemConfig"))
            {
                try
                {
                    ValidateSystemConfiguration(sysConfigElement, systemManagers);
                }
                catch (Exception ex)
                {
                    var systemName = sysConfigElement.Element("SystemName")?.Value ?? "Unnamed System";
                    if (!invalidManagers.ContainsKey(sysConfigElement))
                    {
                        invalidManagers[sysConfigElement] = $"The system '{systemName}' was removed due to the following error(s):\n";
                    }

                    invalidManagers[sysConfigElement] += $"- {ex.Message}\n";
                }
            }

            // Rebuild the XML document from the valid, in-memory configurations
            var newRoot = new XElement("SystemConfigs");
            foreach (var config in systemManagers.OrderBy(static c => c.SystemName, StringComparer.OrdinalIgnoreCase))
            {
                newRoot.Add(new XElement("SystemConfig",
                    new XElement("SystemName", config.SystemName),
                    new XElement("SystemFolders", config.SystemFolders.Select(static f => new XElement("SystemFolder", f))),
                    new XElement("SystemImageFolder", config.SystemImageFolder),
                    new XElement("SystemIsMAME", config.SystemIsMame),
                    new XElement("FileFormatsToSearch", config.FileFormatsToSearch.Select(static f => new XElement("FormatToSearch", f))),
                    new XElement("GroupByFolder", config.GroupByFolder),
                    new XElement("ExtractFileBeforeLaunch", config.ExtractFileBeforeLaunch),
                    new XElement("FileFormatsToLaunch", config.FileFormatsToLaunch.Select(static f => new XElement("FormatToLaunch", f))),
                    new XElement("Emulators", config.Emulators.Select(static e =>
                        new XElement("Emulator",
                            new XElement("EmulatorName", e.EmulatorName),
                            new XElement("EmulatorLocation", e.EmulatorLocation),
                            new XElement("EmulatorParameters", e.EmulatorParameters),
                            new XElement("ReceiveANotificationOnEmulatorError", e.ReceiveANotificationOnEmulatorError)
                        )
                    ))
                ));
            }

            doc.Root.ReplaceNodes(newRoot.Nodes());

            // Notify user about each invalid configuration that was removed
            foreach (var error in invalidManagers.Values)
            {
                // Notify user
                MessageBoxLibrary.InvalidSystemConfigurationMessageBox(error);
            }

            // Save the cleaned, sorted, and reformatted document back to disk.
            try
            {
                // Save the XML document with indentation and new lines on attributes
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    NewLineOnAttributes = false
                };
                using var writer = XmlWriter.Create(XmlPath, settings);
                doc.Save(writer);
            }
            catch (Exception saveEx)
            {
                // Notify developer
                const string contextMessage = "Error saving 'system.xml' after loading, cleaning, and sorting.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(saveEx, contextMessage);
            }

            // Return the list of valid system configurations (could be empty)
            return systemManagers;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error loading system configurations from 'system.xml'.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.SystemXmlIsCorruptedMessageBox(LogPath);

            return new List<SystemManager>(); // Return an empty list
        }

        static void ValidateSystemConfiguration(XElement sysConfigElement, List<SystemManager> systemManagers)
        {
            // Attempt to parse each system configuration.
            // These validations will only run if SystemManager elements exist.
            var systemName = sysConfigElement.Element("SystemName")?.Value;
            if (string.IsNullOrEmpty(systemName))
                throw new InvalidOperationException("Missing or empty 'System Name' in XML.");

            List<string> systemFolders;
            var systemFoldersElement = sysConfigElement.Element("SystemFolders");
            if (systemFoldersElement != null)
            {
                systemFolders = systemFoldersElement.Elements("SystemFolder")
                    .Select(static f => f.Value)
                    .Where(static f => !string.IsNullOrWhiteSpace(f))
                    .ToList();
            }
            else
            {
                var singleFolder = sysConfigElement.Element("SystemFolder")?.Value;
                systemFolders = !string.IsNullOrWhiteSpace(singleFolder) ? new List<string> { singleFolder } : new List<string>();
            }

            if (systemFolders.Count == 0)
                throw new InvalidOperationException($"System '{systemName}': At least one 'System Folder' is required in XML.");

            var systemImageFolder = sysConfigElement.Element("SystemImageFolder")?.Value;
            if (string.IsNullOrEmpty(systemImageFolder))
                throw new InvalidOperationException($"System '{systemName}': Missing or empty 'System Image Folder' in XML.");

            if (!bool.TryParse(sysConfigElement.Element("SystemIsMAME")?.Value, out var systemIsMame))
                throw new InvalidOperationException($"System '{systemName}': Invalid or missing value for 'System Is MAME'.");

            // Validate FileFormatsToSearch
            var formatsToSearch = sysConfigElement.Element("FileFormatsToSearch")
                ?.Elements("FormatToSearch")
                .Select(static e => e.Value.Trim())
                .Where(static value =>
                    !string.IsNullOrWhiteSpace(value)) // Ensure no empty or whitespace-only entries
                .ToList();
            if (formatsToSearch == null || formatsToSearch.Count == 0)
                throw new InvalidOperationException($"System '{systemName}': 'File Extension To Search' should have at least one value.");

            // Validate ExtractFileBeforeLaunch
            if (!bool.TryParse(sysConfigElement.Element("ExtractFileBeforeLaunch")?.Value,
                    out var extractFileBeforeLaunch))
                throw new InvalidOperationException($"System '{systemName}': Invalid or missing value for 'Extract File Before Launch'.");

            if (extractFileBeforeLaunch && (formatsToSearch == null || !formatsToSearch.Any(static f => f.Equals("zip", StringComparison.OrdinalIgnoreCase) ||
                                                                                                        f.Equals("7z", StringComparison.OrdinalIgnoreCase) ||
                                                                                                        f.Equals("rar", StringComparison.OrdinalIgnoreCase))))
            {
                throw new InvalidOperationException($"System '{systemName}': When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.");
            }

            // Validate FileFormatsToLaunch
            var formatsToLaunch = sysConfigElement.Element("FileFormatsToLaunch")
                ?.Elements("FormatToLaunch")
                .Select(static e => e.Value.Trim())
                .Where(static value =>
                    !string.IsNullOrWhiteSpace(value)) // Ensure no empty or whitespace-only entries
                .ToList();
            // If ExtractFileBeforeLaunch is true, FileFormatsToLaunch must have values.
            if (extractFileBeforeLaunch && (formatsToLaunch == null || formatsToLaunch.Count == 0))
                throw new InvalidOperationException($"System '{systemName}': 'File Extension To Launch' should have at least one value when 'Extract File Before Launch' is set to true.");

            // Parse GroupByFolder
            if (!bool.TryParse(sysConfigElement.Element("GroupByFolder")?.Value, out var groupByFolder))
            {
                groupByFolder = false;
            }

            // Validate emulator configurations
            var emulators = new List<Emulator>();
            var emulatorElements = sysConfigElement.Element("Emulators")?.Elements("Emulator").ToList();

            if (emulatorElements == null || emulatorElements.Count == 0)
                throw new InvalidOperationException($"System '{systemName}': Emulators list should not be empty or null."); // Need at least one EmulatorName element

            foreach (var emulatorElement in emulatorElements)
            {
                var emulatorName = emulatorElement.Element("EmulatorName")?.Value;
                if (string.IsNullOrEmpty(emulatorName))
                    throw new InvalidOperationException($"System '{systemName}': An 'Emulator Name' should not be empty or null.");

                var emulatorLocation = emulatorElement.Element("EmulatorLocation")?.Value ?? string.Empty; // can be empty
                var emulatorParameters = emulatorElement.Element("EmulatorParameters")?.Value ?? string.Empty; // can be empty

                // Parse the ReceiveANotificationOnEmulatorError value with default = true
                // If the element is missing or parsing fails, it defaults to true.
                var receiveNotification = true; // Default value
                if (emulatorElement.Element("ReceiveANotificationOnEmulatorError") != null)
                {
                    if (!bool.TryParse(emulatorElement.Element("ReceiveANotificationOnEmulatorError")?.Value, out receiveNotification))
                    {
                        receiveNotification = true; // Reset to default if parsing fails
                    }
                }

                emulators.Add(new Emulator
                {
                    EmulatorName = emulatorName,
                    EmulatorLocation = emulatorLocation, // Store the raw string
                    EmulatorParameters = emulatorParameters, // Store the raw string
                    ReceiveANotificationOnEmulatorError = receiveNotification
                });
            }

            systemManagers.Add(new SystemManager
            {
                SystemName = systemName,
                SystemFolders = systemFolders, // Store the raw string
                SystemImageFolder = systemImageFolder, // Store the raw string
                SystemIsMame = systemIsMame,
                ExtractFileBeforeLaunch = extractFileBeforeLaunch,
                FileFormatsToSearch = formatsToSearch,
                FileFormatsToLaunch = formatsToLaunch,
                Emulators = emulators,
                GroupByFolder = groupByFolder
            });
        }

        bool RestoreBackupFile(string directoryPath, bool backupRestored)
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
                            if (mostRecentBackupFile != null) File.Copy(mostRecentBackupFile, XmlPath, true);

                            backupRestored = true;
                        }
                        catch (Exception ex)
                        {
                            // Notify developer
                            const string contextMessage = "'Simple Launcher' was unable to restore the last backup.";
                            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
                // Proceed to create empty file as backup handling failed
            }

            return backupRestored;
        }
    }
}
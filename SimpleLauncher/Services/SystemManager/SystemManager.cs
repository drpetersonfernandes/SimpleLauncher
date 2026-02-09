using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher.Services.SystemManager;

public partial class SystemManager
{
    private static readonly object XmlLock = new();
    public string SystemName { get; init; }
    public List<string> SystemFolders { get; init; }
    public string PrimarySystemFolder => SystemFolders?.FirstOrDefault();
    public string SystemImageFolder { get; init; }
    public bool SystemIsMame { get; init; }
    public List<string> FileFormatsToSearch { get; init; }
    public bool ExtractFileBeforeLaunch { get; init; }
    public List<string> FileFormatsToLaunch { get; init; }
    public List<Emulator> Emulators { get; init; }
    public bool GroupByFolder { get; init; }

    public static bool SystemExists(string systemName, IConfiguration configuration)
    {
        lock (XmlLock)
        {
            var systemXmlPath = PathHelper.ResolveRelativeToAppDirectory(configuration.GetValue<string>("SystemXmlPath") ?? "system.xml");
            if (!File.Exists(systemXmlPath))
            {
                return false;
            }

            try
            {
                // Use settings to prevent DTD processing for security
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };
                using var reader = XmlReader.Create(systemXmlPath, settings);
                var doc = XDocument.Load(reader, LoadOptions.None);

                return doc.Root?.Elements("SystemConfig")
                    .Any(el => el.Element("SystemName")?.Value.Equals(systemName, StringComparison.OrdinalIgnoreCase) ?? false) ?? false;
            }
            catch (Exception ex) // Catch XmlException, IOException, etc.
            {
                // If file is corrupt or locked, we can't check.
                DebugLogger.Log($"[SystemManager.SystemExists] Could not check system.xml: {ex.Message}");
                return false;
            }
        }
    }

    public static List<SystemManager> LoadSystemManagers(IConfiguration configuration)
    {
        lock (XmlLock)
        {
            var systemXmlPath = PathHelper.ResolveRelativeToAppDirectory(configuration.GetValue<string>("SystemXmlPath") ?? "system.xml");

            try
            {
                if (!File.Exists(systemXmlPath))
                {
                    var directoryPath = Path.GetDirectoryName(systemXmlPath);
                    var backupRestored = false;

                    if (directoryPath != null)
                    {
                        backupRestored = RestoreBackupFile(directoryPath, backupRestored, systemXmlPath);
                    }

                    // If no backup was restored, create a new empty system.xml file
                    if (!backupRestored)
                    {
                        try
                        {
                            // Create a new XDocument with the root element
                            var emptyDoc = new XDocument(new XElement("SystemConfigs"));
                            emptyDoc.Save(systemXmlPath);
                            // No user notification needed for creating an expected empty file
                        }
                        catch (Exception createEx)
                        {
                            // Notify developer
                            const string contextMessage = "Error creating empty 'system.xml'.";
                            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(createEx, contextMessage);

                            // Notify user
                            MessageBoxLibrary.SystemXmlIsCorruptedMessageBox(PathHelper.ResolveRelativeToAppDirectory(configuration.GetValue<string>("LogPath") ?? "error_user.log"));

                            return []; // Return an empty list
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

                    using var reader = XmlReader.Create(systemXmlPath, settings);
                    doc = XDocument.Load(reader, LoadOptions.None);
                }
                catch (XmlException ex)
                {
                    // Notify developer
                    var contextMessage = $"The file 'system.xml' is badly corrupt at line {ex.LineNumber}, position {ex.LinePosition}.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.FileSystemXmlIsCorruptedMessageBox(PathHelper.ResolveRelativeToAppDirectory(configuration.GetValue<string>("LogPath") ?? "error_user.log"));

                    return new List<SystemManager>(); // Return an empty list
                }
                catch (IOException ex)
                {
                    // Notify developer
                    const string contextMessage = "The file 'system.xml' is locked or inaccessible by another process.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                    // Notify user - File is locked, not corrupted
                    MessageBoxLibrary.FileSystemXmlIsLockedMessageBox();

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
                                new XElement("ReceiveANotificationOnEmulatorError", e.ReceiveANotificationOnEmulatorError),
                                string.IsNullOrEmpty(e.ImagePackDownloadLink) ? null : new XElement("ImagePackDownloadLink", e.ImagePackDownloadLink),
                                string.IsNullOrEmpty(e.ImagePackDownloadLink2) ? null : new XElement("ImagePackDownloadLink2", e.ImagePackDownloadLink2),
                                string.IsNullOrEmpty(e.ImagePackDownloadLink3) ? null : new XElement("ImagePackDownloadLink3", e.ImagePackDownloadLink3),
                                string.IsNullOrEmpty(e.ImagePackDownloadLink4) ? null : new XElement("ImagePackDownloadLink4", e.ImagePackDownloadLink4),
                                string.IsNullOrEmpty(e.ImagePackDownloadLink5) ? null : new XElement("ImagePackDownloadLink5", e.ImagePackDownloadLink5),
                                string.IsNullOrEmpty(e.ImagePackDownloadExtractPath) ? null : new XElement("ImagePackDownloadExtractPath", e.ImagePackDownloadExtractPath)
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
                    using var writer = XmlWriter.Create(systemXmlPath, settings);
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
                MessageBoxLibrary.SystemXmlIsCorruptedMessageBox(PathHelper.ResolveRelativeToAppDirectory(configuration.GetValue<string>("LogPath") ?? "error_user.log"));

                return new List<SystemManager>(); // Return an empty list
            }
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
                systemFolders = !string.IsNullOrWhiteSpace(singleFolder) ? [singleFolder] : new List<string>();
            }

            if (systemFolders.Count == 0)
                throw new InvalidOperationException($"System '{systemName}': At least one 'System Folder' is required in XML.");

            var systemImageFolder = sysConfigElement.Element("SystemImageFolder")?.Value;
            if (string.IsNullOrEmpty(systemImageFolder))
                throw new InvalidOperationException($"System '{systemName}': Missing or empty 'System Image Folder' in XML.");

            if (!bool.TryParse(sysConfigElement.Element("SystemIsMAME")?.Value, out var systemIsMame))
                throw new InvalidOperationException($"System '{systemName}': Invalid or missing value for 'Is the system MAME-based?'.");

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
                    ReceiveANotificationOnEmulatorError = receiveNotification,
                    ImagePackDownloadLink = emulatorElement.Element("ImagePackDownloadLink")?.Value ?? string.Empty,
                    ImagePackDownloadLink2 = emulatorElement.Element("ImagePackDownloadLink2")?.Value ?? string.Empty,
                    ImagePackDownloadLink3 = emulatorElement.Element("ImagePackDownloadLink3")?.Value ?? string.Empty,
                    ImagePackDownloadLink4 = emulatorElement.Element("ImagePackDownloadLink4")?.Value ?? string.Empty,
                    ImagePackDownloadLink5 = emulatorElement.Element("ImagePackDownloadLink5")?.Value ?? string.Empty,
                    ImagePackDownloadExtractPath = emulatorElement.Element("ImagePackDownloadExtractPath")?.Value ?? string.Empty
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

        bool RestoreBackupFile(string directoryPath, bool backupRestored, string systemXmlPath)
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
                            if (mostRecentBackupFile != null) File.Copy(mostRecentBackupFile, systemXmlPath, true);

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

    public static Task AddOrUpdateSystemFromEasyModeAsync(EasyModeSystemConfig selectedSystem, string systemFolder)
    {
        var systemToSave = new SystemManager
        {
            SystemName = selectedSystem.SystemName,
            SystemFolders = [systemFolder],
            SystemImageFolder = selectedSystem.SystemImageFolder,
            SystemIsMame = selectedSystem.SystemIsMame,
            FileFormatsToSearch = selectedSystem.FileFormatsToSearch,
            GroupByFolder = false, // Default to false for new EasyMode systems
            ExtractFileBeforeLaunch = selectedSystem.ExtractFileBeforeLaunch,
            FileFormatsToLaunch = selectedSystem.FileFormatsToLaunch,
            Emulators = [ConvertEasyModeEmulator(selectedSystem.Emulators.Emulator)]
        };

        // When called from Easy Mode, we always want to add or update, so the original name is the same as the new name.
        return SaveSystemConfigurationAsync(systemToSave, selectedSystem.SystemName);
    }

    private static Emulator ConvertEasyModeEmulator(EasyMode.Models.EmulatorConfig emulatorConfig)
    {
        return new Emulator
        {
            EmulatorName = emulatorConfig.EmulatorName,
            EmulatorLocation = emulatorConfig.EmulatorLocation,
            EmulatorParameters = emulatorConfig.EmulatorParameters,
            ReceiveANotificationOnEmulatorError = true, // Default to true for EasyMode systems
            ImagePackDownloadLink = emulatorConfig.ImagePackDownloadLink,
            ImagePackDownloadLink2 = emulatorConfig.ImagePackDownloadLink2,
            ImagePackDownloadLink3 = emulatorConfig.ImagePackDownloadLink3,
            ImagePackDownloadLink4 = emulatorConfig.ImagePackDownloadLink4,
            ImagePackDownloadLink5 = emulatorConfig.ImagePackDownloadLink5,
            ImagePackDownloadExtractPath = emulatorConfig.ImagePackDownloadExtractPath
        };
    }

    public static Task SaveSystemConfigurationAsync(SystemManager systemConfig, string originalSystemName = null)
    {
        Task.Run(() =>
        {
            lock (XmlLock)
            {
                var systemXmlPath = PathHelper.ResolveRelativeToAppDirectory(App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("SystemXmlPath") ?? "system.xml");
                XDocument xmlDoc;
                try
                {
                    if (File.Exists(systemXmlPath))
                    {
                        var xmlContent = File.ReadAllText(systemXmlPath);
                        xmlDoc = string.IsNullOrWhiteSpace(xmlContent) ? new XDocument(new XElement("SystemConfigs")) : XDocument.Parse(xmlContent);
                        if (xmlDoc.Root == null || xmlDoc.Root.Name != "SystemConfigs")
                        {
                            xmlDoc = new XDocument(new XElement("SystemConfigs"));
                        }
                    }
                    else
                    {
                        xmlDoc = new XDocument(new XElement("SystemConfigs"));
                    }
                }
                catch (Exception ex)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading/parsing system.xml for saving.");
                    throw new InvalidOperationException("Failed to load system configuration for saving.", ex);
                }

                var root = xmlDoc.Root;
                var systemIdentifier = originalSystemName ?? systemConfig.SystemName;
                if (root != null)
                {
                    var existingSystem = root.Elements("SystemConfig")
                        .FirstOrDefault(el => el.Element("SystemName")?.Value == systemIdentifier);

                    if (existingSystem != null)
                    {
                        UpdateSystemXElement(existingSystem, systemConfig);
                    }
                    else
                    {
                        root.Add(CreateSystemXElement(systemConfig));
                    }
                }

                if (root != null)
                {
                    var sortedSystems = root.Elements("SystemConfig")
                        .OrderBy(static system => system.Element("SystemName")?.Value, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    root.RemoveNodes();
                    root.Add(sortedSystems);
                }

                try
                {
                    var settings = new XmlWriterSettings { Indent = true, IndentChars = "  ", NewLineHandling = NewLineHandling.Replace, Encoding = System.Text.Encoding.UTF8 };
                    using var writer = XmlWriter.Create(systemXmlPath, settings);
                    xmlDoc.Declaration ??= new XDeclaration("1.0", "utf-8", null);
                    xmlDoc.Save(writer);
                }
                catch (Exception ex)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving system.xml.");
                    throw new InvalidOperationException("Failed to save system configuration.", ex);
                }
            }
        });
        return Task.CompletedTask;
    }

    public static Task DeleteSystemAsync(string systemNameToDelete)
    {
        return Task.Run(() =>
        {
            lock (XmlLock)
            {
                var systemXmlPath = PathHelper.ResolveRelativeToAppDirectory(App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("SystemXmlPath") ?? "system.xml");
                if (!File.Exists(systemXmlPath)) return;

                XDocument xmlDoc;
                try
                {
                    xmlDoc = XDocument.Load(systemXmlPath);
                }
                catch (Exception ex)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error loading system.xml for deleting system '{systemNameToDelete}'.");
                    throw new InvalidOperationException("Failed to load system configuration for deletion.", ex);
                }

                var systemNode = xmlDoc.Root?.Descendants("SystemConfig")
                    .FirstOrDefault(element => element.Element("SystemName")?.Value == systemNameToDelete);

                if (systemNode != null)
                {
                    systemNode.Remove();
                    xmlDoc.Save(systemXmlPath);
                }
            }
        });
    }

    private static XElement CreateSystemXElement(SystemManager config)
    {
        var element = new XElement("SystemConfig",
            new XElement("SystemName", config.SystemName),
            new XElement("SystemFolders", config.SystemFolders.Select(static folder => new XElement("SystemFolder", folder))),
            new XElement("SystemImageFolder", config.SystemImageFolder),
            new XElement("SystemIsMAME", config.SystemIsMame),
            new XElement("FileFormatsToSearch", config.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format))),
            new XElement("GroupByFolder", config.GroupByFolder),
            new XElement("ExtractFileBeforeLaunch", config.ExtractFileBeforeLaunch),
            new XElement("FileFormatsToLaunch", config.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format))),
            new XElement("Emulators", config.Emulators.Select(CreateEmulatorXElement))
        );
        return element;
    }

    private static void UpdateSystemXElement(XElement existingSystem, SystemManager config)
    {
        existingSystem.SetElementValue("SystemName", config.SystemName);

        var foldersElement = existingSystem.Element("SystemFolders");
        if (foldersElement == null)
        {
            foldersElement = new XElement("SystemFolders");
            existingSystem.Element("SystemName")?.AddAfterSelf(foldersElement);
        }

        foldersElement.ReplaceNodes(config.SystemFolders.Select(static folder => new XElement("SystemFolder", folder)));

        existingSystem.SetElementValue("SystemImageFolder", config.SystemImageFolder);
        existingSystem.SetElementValue("SystemIsMAME", config.SystemIsMame);
        existingSystem.Element("FileFormatsToSearch")?.ReplaceNodes(config.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format)));
        existingSystem.SetElementValue("GroupByFolder", config.GroupByFolder);
        existingSystem.SetElementValue("ExtractFileBeforeLaunch", config.ExtractFileBeforeLaunch);
        existingSystem.Element("FileFormatsToLaunch")?.ReplaceNodes(config.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format)));

        existingSystem.Element("Emulators")?.Remove();
        existingSystem.Add(new XElement("Emulators", config.Emulators.Select(CreateEmulatorXElement)));
    }

    private static XElement CreateEmulatorXElement(Emulator emulatorConfig)
    {
        var emulatorElement = new XElement("Emulator",
            new XElement("EmulatorName", emulatorConfig.EmulatorName),
            new XElement("EmulatorLocation", emulatorConfig.EmulatorLocation),
            new XElement("EmulatorParameters", emulatorConfig.EmulatorParameters),
            new XElement("ReceiveANotificationOnEmulatorError", emulatorConfig.ReceiveANotificationOnEmulatorError)
        );

        if (!string.IsNullOrEmpty(emulatorConfig.ImagePackDownloadLink))
            emulatorElement.Add(new XElement("ImagePackDownloadLink", emulatorConfig.ImagePackDownloadLink));
        if (!string.IsNullOrEmpty(emulatorConfig.ImagePackDownloadLink2))
            emulatorElement.Add(new XElement("ImagePackDownloadLink2", emulatorConfig.ImagePackDownloadLink2));
        if (!string.IsNullOrEmpty(emulatorConfig.ImagePackDownloadLink3))
            emulatorElement.Add(new XElement("ImagePackDownloadLink3", emulatorConfig.ImagePackDownloadLink3));
        if (!string.IsNullOrEmpty(emulatorConfig.ImagePackDownloadLink4))
            emulatorElement.Add(new XElement("ImagePackDownloadLink4", emulatorConfig.ImagePackDownloadLink4));
        if (!string.IsNullOrEmpty(emulatorConfig.ImagePackDownloadLink5))
            emulatorElement.Add(new XElement("ImagePackDownloadLink5", emulatorConfig.ImagePackDownloadLink5));
        if (!string.IsNullOrEmpty(emulatorConfig.ImagePackDownloadExtractPath))
            emulatorElement.Add(new XElement("ImagePackDownloadExtractPath", emulatorConfig.ImagePackDownloadExtractPath));

        return emulatorElement;
    }
}
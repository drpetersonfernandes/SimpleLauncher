using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.AppDataFile;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Core.Services.SystemConfiguration;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class SystemConfigurationService : ICoreSystemConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly DataFileLocation _fileLocation;
    private static readonly object XmlLock = new();

    public SystemConfigurationService(IConfiguration configuration, ILogErrors logErrors)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _fileLocation = new DataFileLocation(configuration, "SystemXmlPath", "system.xml");
    }

    public List<ISystemManager> LoadSystemManagers()
    {
        lock (XmlLock)
        {
            var systemXmlPath = _fileLocation.FilePath;

            try
            {
                if (!File.Exists(systemXmlPath))
                {
                    var directoryPath = Path.GetDirectoryName(systemXmlPath);
                    if (directoryPath != null)
                    {
                        RestoreBackupFile(directoryPath, systemXmlPath);
                    }

                    if (!File.Exists(systemXmlPath))
                    {
                        try
                        {
                            var emptyDoc = new XDocument(new XElement("SystemConfigs"));
                            emptyDoc.Save(systemXmlPath);
                        }
                        catch (Exception createEx)
                        {
                            _logErrors?.LogAndForget(createEx, "Error creating empty 'system.xml'.");
                            return [];
                        }
                    }
                }

                var systemManagers = new List<SystemManagerConfig>();
                XDocument doc;

                try
                {
                    var settings = new XmlReaderSettings
                    {
                        DtdProcessing = DtdProcessing.Prohibit,
                        XmlResolver = null
                    };

                    using var reader = XmlReader.Create(systemXmlPath, settings);
                    doc = XDocument.Load(reader, LoadOptions.None);

                    if (doc.Root != null)
                    {
                        foreach (var sysConfigElement in doc.Root.Elements("SystemConfig"))
                        {
                            try
                            {
                                ValidateSystemConfiguration(sysConfigElement, systemManagers);
                            }
                            catch (Exception ex)
                            {
                                var systemName = sysConfigElement.Element("SystemName")?.Value ?? "Unnamed System";
                                _logErrors?.LogAndForget(ex, $"System '{systemName}' was removed due to validation error.");
                            }
                        }
                    }
                }
                catch (XmlException ex)
                {
                    _logErrors?.LogAndForget(ex, "Structural corruption in 'system.xml'. Attempting partial recovery.");

                    doc = new XDocument(new XElement("SystemConfigs"));

                    try
                    {
                        var rawXml = File.ReadAllText(systemXmlPath);
                        var matches = SystemConfigRegex().Matches(rawXml);
                        foreach (Match match in matches)
                        {
                            try
                            {
                                var sysConfigElement = XElement.Parse(match.Value);
                                ValidateSystemConfiguration(sysConfigElement, systemManagers);
                            }
                            catch (Exception innerEx)
                            {
                                _logErrors?.LogAndForget(innerEx, "Failed to validate system configuration during recovery.");
                            }
                        }
                    }
                    catch (Exception fatalEx)
                    {
                        _logErrors?.LogAndForget(fatalEx, "Failed to perform regex recovery on system.xml.");
                    }
                }
                catch (IOException ex)
                {
                    _logErrors?.LogAndForget(ex, "The file 'system.xml' is locked.");
                    return [];
                }

                if (doc.Root == null)
                {
                    return systemManagers.Cast<ISystemManager>().ToList();
                }

                // Rebuild and save cleaned XML
                var newRoot = new XElement("SystemConfigs");
                foreach (var config in systemManagers.OrderBy(static c => c.SystemName, StringComparer.OrdinalIgnoreCase))
                {
                    newRoot.Add(new XElement("SystemConfig",
                        new XElement("SystemName", config.SystemName),
                        new XElement("SystemFolders", config.SystemFolders.Select(static f => new XElement("SystemFolder", f))),
                        new XElement("SystemImageFolder", config.SystemImageFolder),
                        new XElement("FileFormatsToSearch", config.FileFormatsToSearch.Select(static f => new XElement("FormatToSearch", f))),
                        new XElement("GroupByFolder", config.GroupByFolder),
                        new XElement("DisableRecursiveSearch", config.DisableRecursiveSearch),
                        config.ExtractFileBeforeLaunch ? new XElement("ExtractFileBeforeLaunch", true) : null,
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

                try
                {
                    var writerSettings = new XmlWriterSettings { Indent = true, NewLineOnAttributes = false };
                    using var writer = XmlWriter.Create(systemXmlPath, writerSettings);
                    doc.Save(writer);
                }
                catch (Exception saveEx)
                {
                    _logErrors?.LogAndForget(saveEx, "Error saving 'system.xml' after loading.");
                }

                return systemManagers.Cast<ISystemManager>().ToList();
            }
            catch (Exception ex)
            {
                _logErrors?.LogAndForget(ex, "Error loading system configurations from 'system.xml'.");
                return [];
            }
        }
    }

    private void RestoreBackupFile(string directoryPath, string systemXmlPath)
    {
        try
        {
            var backupFiles = Directory.GetFiles(directoryPath, "system_backup*.xml").ToList();
            if (backupFiles.Count > 0)
            {
                var mostRecentBackupFile = backupFiles.MaxBy(File.GetLastWriteTime);
                if (mostRecentBackupFile != null)
                {
                    File.Copy(mostRecentBackupFile, systemXmlPath, true);
                }
            }
        }
        catch (Exception ex)
        {
            _logErrors?.LogAndForget(ex, "Error during backup file handling.");
        }
    }

    private static void ValidateSystemConfiguration(XElement sysConfigElement, List<SystemManagerConfig> systemManagers)
    {
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
            throw new InvalidOperationException($"System '{systemName}': At least one 'System Folder' is required.");

        var systemImageFolder = sysConfigElement.Element("SystemImageFolder")?.Value;
        if (string.IsNullOrEmpty(systemImageFolder))
            throw new InvalidOperationException($"System '{systemName}': Missing or empty 'System Image Folder'.");

        var formatsToSearch = sysConfigElement.Element("FileFormatsToSearch")
            ?.Elements("FormatToSearch")
            .Select(static e => e.Value.Trim())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToList();
        if (formatsToSearch == null || formatsToSearch.Count == 0)
            throw new InvalidOperationException($"System '{systemName}': 'File Extension To Search' should have at least one value.");

        var extractFileBeforeLaunch = false;
        var extractElement = sysConfigElement.Element("ExtractFileBeforeLaunch");
        if (extractElement != null)
        {
            if (!bool.TryParse(extractElement.Value, out extractFileBeforeLaunch))
            {
                extractFileBeforeLaunch = false;
            }
        }

        if (extractFileBeforeLaunch && (formatsToSearch == null || !formatsToSearch.All(static f =>
                f.Equals("zip", StringComparison.OrdinalIgnoreCase) ||
                f.Equals("7z", StringComparison.OrdinalIgnoreCase) ||
                f.Equals("rar", StringComparison.OrdinalIgnoreCase))))
        {
            throw new InvalidOperationException($"System '{systemName}': When 'Extract File Before Launch' is true, extensions must ONLY be 'zip', '7z', or 'rar'.");
        }

        var formatsToLaunch = sysConfigElement.Element("FileFormatsToLaunch")
            ?.Elements("FormatToLaunch")
            .Select(static e => e.Value.Trim())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToList();
        if (extractFileBeforeLaunch && (formatsToLaunch == null || formatsToLaunch.Count == 0))
            throw new InvalidOperationException($"System '{systemName}': 'File Extension To Launch' should have at least one value when 'Extract File Before Launch' is true.");

        if (!bool.TryParse(sysConfigElement.Element("GroupByFolder")?.Value, out var groupByFolder))
        {
            groupByFolder = false;
        }

        if (!bool.TryParse(sysConfigElement.Element("DisableRecursiveSearch")?.Value, out var disableRecursiveSearch))
        {
            disableRecursiveSearch = false;
        }

        var emulators = new List<Emulator>();
        var emulatorElements = sysConfigElement.Element("Emulators")?.Elements("Emulator").ToList();
        if (emulatorElements == null || emulatorElements.Count == 0)
            throw new InvalidOperationException($"System '{systemName}': Emulators list should not be empty.");

        foreach (var emulatorElement in emulatorElements)
        {
            var emulatorName = emulatorElement.Element("EmulatorName")?.Value;
            if (string.IsNullOrEmpty(emulatorName))
                throw new InvalidOperationException($"System '{systemName}': An 'Emulator Name' should not be empty.");

            var receiveNotification = true;
            if (emulatorElement.Element("ReceiveANotificationOnEmulatorError") != null)
            {
                if (!bool.TryParse(emulatorElement.Element("ReceiveANotificationOnEmulatorError")?.Value, out receiveNotification))
                {
                    receiveNotification = true;
                }
            }

            emulators.Add(new Emulator
            {
                EmulatorName = emulatorName,
                EmulatorLocation = emulatorElement.Element("EmulatorLocation")?.Value ?? string.Empty,
                EmulatorParameters = emulatorElement.Element("EmulatorParameters")?.Value ?? string.Empty,
                ReceiveANotificationOnEmulatorError = receiveNotification,
                ImagePackDownloadLink = emulatorElement.Element("ImagePackDownloadLink")?.Value ?? string.Empty,
                ImagePackDownloadLink2 = emulatorElement.Element("ImagePackDownloadLink2")?.Value ?? string.Empty,
                ImagePackDownloadLink3 = emulatorElement.Element("ImagePackDownloadLink3")?.Value ?? string.Empty,
                ImagePackDownloadLink4 = emulatorElement.Element("ImagePackDownloadLink4")?.Value ?? string.Empty,
                ImagePackDownloadLink5 = emulatorElement.Element("ImagePackDownloadLink5")?.Value ?? string.Empty,
                ImagePackDownloadExtractPath = emulatorElement.Element("ImagePackDownloadExtractPath")?.Value ?? string.Empty
            });
        }

        systemManagers.Add(new SystemManagerConfig
        {
            SystemName = systemName,
            SystemFolders = systemFolders,
            SystemImageFolder = systemImageFolder,
            ExtractFileBeforeLaunch = extractFileBeforeLaunch,
            FileFormatsToSearch = formatsToSearch,
            FileFormatsToLaunch = formatsToLaunch ?? [],
            Emulators = emulators,
            GroupByFolder = groupByFolder,
            DisableRecursiveSearch = disableRecursiveSearch
        });
    }

    [GeneratedRegex(@"<SystemConfig[^>]*>[\s\S]*?<\/SystemConfig>")]
    private static partial Regex SystemConfigRegex();
}

using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services;

namespace SimpleLauncher.Avalonia.Services.SystemManager;

/// <summary>
/// Reads and writes system.xml, exposing system configurations.
/// Supports both legacy (nested child-element) and simplified (semicolon/comma-delimited) formats for reading.
/// Writes in the legacy format for compatibility with the original SimpleLauncher.
/// </summary>
public class SystemManagerService
{
    private static readonly Lock XmlLock = new();
    private readonly IConfiguration _configuration;
    private List<SystemManagerConfig>? _cachedSystems;

    public SystemManagerService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // ── Read ──────────────────────────────────────────────────────────

    /// <summary>
    /// Loads all system configurations from system.xml.
    /// </summary>
    public List<SystemManagerConfig> LoadSystems()
    {
        if (_cachedSystems is not null) return _cachedSystems;

        _cachedSystems = [];

        var path = GetSystemXmlPath();
        if (!File.Exists(path)) return _cachedSystems;

        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using var reader = XmlReader.Create(path, settings);
            var doc = XDocument.Load(reader, LoadOptions.None);

            foreach (var element in doc.Root?.Elements("SystemConfig") ?? [])
            {
                var config = new SystemManagerConfig
                {
                    SystemName = element.Element("SystemName")?.Value ?? "",
                    SystemFolders = ParseFoldersCompat(element),
                    SystemImageFolder = element.Element("SystemImageFolder")?.Value ?? "",
                    FileFormatsToSearch = ParseListCompat(element, "FileFormatsToSearch", "FormatToSearch"),
                    FileFormatsToLaunch = ParseListCompat(element, "FileFormatsToLaunch", "FormatToLaunch"),
                    ExtractFileBeforeLaunch = bool.TryParse(element.Element("ExtractFileBeforeLaunch")?.Value, out var b) && b,
                    GroupByFolder = bool.TryParse(element.Element("GroupByFolder")?.Value, out var g) && g,
                    DisableRecursiveSearch = bool.TryParse(element.Element("DisableRecursiveSearch")?.Value, out var d) && d,
                    Emulators = ParseEmulators(element.Element("Emulators"))
                };

                if (!string.IsNullOrWhiteSpace(config.SystemName))
                    _cachedSystems.Add(config);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to parse system.xml at {Path}", path);
        }

        return _cachedSystems;
    }

    /// <summary>
    /// Gets a single system by name.
    /// </summary>
    public SystemManagerConfig? GetSystem(string systemName)
    {
        return LoadSystems().FirstOrDefault(s =>
            string.Equals(s.SystemName, systemName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Invalidates the cache — call after system.xml changes.
    /// </summary>
    public void InvalidateCache()
    {
        _cachedSystems = null;
    }

    // ── Write (EasyMode) ──────────────────────────────────────────────

    /// <summary>
    /// Adds or updates a system configuration from an EasyMode preset.
    /// Mirrors the original SimpleLauncher's <c>AddOrUpdateSystemFromEasyModeAsync</c>.
    /// </summary>
    public static Task AddOrUpdateSystemFromEasyModeAsync(
        EasyModeSystemConfig selectedSystem,
        string systemFolder,
        IConfiguration configuration,
        ILogger? logErrors = null)
    {
        return SaveSystemConfigurationAsync(
            selectedSystem.SystemName,
            [systemFolder],
            selectedSystem.SystemImageFolder,
            selectedSystem.FileFormatsToSearch ?? [],
            selectedSystem.FileFormatsToLaunch ?? [],
            selectedSystem.ExtractFileBeforeLaunch,
            ConvertEasyModeEmulator(selectedSystem.Emulators?.Emulator),
            selectedSystem.SystemName,
            configuration,
            logErrors);
    }

    /// <summary>
    /// Persists a system configuration to system.xml (legacy format with nested child elements),
    /// creating or updating the entry with retry logic and atomic writes.
    /// </summary>
    internal static async Task SaveSystemConfigurationAsync(
        string systemName,
        IEnumerable<string> systemFolders,
        string systemImageFolder,
        IEnumerable<string> fileFormatsToSearch,
        IEnumerable<string> fileFormatsToLaunch,
        bool extractFileBeforeLaunch,
        Emulator? emulator,
        string? originalSystemName,
        IConfiguration configuration,
        ILogger? logErrors = null)
    {
        try
        {
            await Task.Run(() =>
            {
                lock (XmlLock)
                {
                    var systemXmlPath = GetSystemXmlPathStatic(configuration);
                    XDocument xmlDoc;

                    try
                    {
                        if (File.Exists(systemXmlPath))
                        {
                            var xmlContent = File.ReadAllText(systemXmlPath);
                            xmlDoc = string.IsNullOrWhiteSpace(xmlContent)
                                ? new XDocument(new XElement("SystemConfigs"))
                                : XDocument.Parse(xmlContent);
                            if (xmlDoc.Root?.Name != "SystemConfigs")
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
                        logErrors?.Error(ex, "Error loading/parsing system.xml for saving.");
                        throw new InvalidOperationException("Failed to load system configuration for saving.", ex);
                    }

                    var root = xmlDoc.Root!;
                    var identifier = originalSystemName ?? systemName;

                    var existingSystem = root.Elements("SystemConfig")
                        .FirstOrDefault(el => string.Equals(
                            el.Element("SystemName")?.Value, identifier, StringComparison.Ordinal));

                    if (existingSystem != null)
                    {
                        // Merge in-place: preserve any child elements we don't explicitly set
                        MergeSystemConfigElement(existingSystem,
                            systemName, systemFolders, systemImageFolder,
                            fileFormatsToSearch, fileFormatsToLaunch,
                            extractFileBeforeLaunch, emulator);
                    }
                    else
                    {
                        var newElement = BuildSystemConfigElement(
                            systemName, systemFolders, systemImageFolder,
                            fileFormatsToSearch, fileFormatsToLaunch,
                            extractFileBeforeLaunch, emulator);
                        root.Add(newElement);
                    }

                    // Sort alphabetically
                    var sorted = root.Elements("SystemConfig")
                        .OrderBy(static s => s.Element("SystemName")?.Value, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    root.RemoveNodes();
                    root.Add(sorted);

                    // Atomic write with retry
                    const int maxRetries = 3;
                    var retryDelayMs = 500;
                    Exception? lastException = null;

                    for (var attempt = 0; attempt < maxRetries; attempt++)
                    {
                        try
                        {
                            var tempPath = systemXmlPath + ".tmp";
                            var writerSettings = new XmlWriterSettings
                            {
                                Indent = true,
                                IndentChars = "  ",
                                NewLineHandling = NewLineHandling.Replace,
                                Encoding = System.Text.Encoding.UTF8
                            };

                            byte[] xmlBytes;
                            using (var ms = new MemoryStream())
                            {
                                using (var writer = XmlWriter.Create(ms, writerSettings))
                                {
                                    xmlDoc.Declaration ??= new XDeclaration("1.0", "utf-8", null);
                                    xmlDoc.Save(writer);
                                }

                                xmlBytes = ms.ToArray();
                            }

                            if (xmlBytes.Length == 0)
                                throw new InvalidOperationException("Generated system XML is empty.");

                            File.WriteAllBytes(tempPath, xmlBytes);
                            File.Move(tempPath, systemXmlPath, true);
                            return;
                        }
                        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
                        {
                            lastException = ex;
                            if (attempt < maxRetries - 1)
                            {
                                try
                                {
                                    File.Delete(systemXmlPath + ".tmp");
                                }
                                catch (Exception cleanupEx)
                                {
                                    Log.Debug(cleanupEx, "Failed to delete stale system.xml temp file {Path}", systemXmlPath + ".tmp");
                                }

                                Thread.Sleep(retryDelayMs);
                                retryDelayMs *= 2;
                            }
                        }
                        catch (Exception ex)
                        {
                            lastException = ex;
                            break;
                        }
                    }

                    logErrors?.Error(lastException, "Error saving system.xml.");
                    throw new InvalidOperationException("Failed to save system configuration.", lastException);
                }
            });
        }
        catch (Exception ex)
        {
            logErrors?.Error(ex, "Error saving system configuration.");
        }
    }

    // ── Path helpers ──────────────────────────────────────────────────

    private string GetSystemXmlPath()
    {
        return GetSystemXmlPathStatic(_configuration);
    }

    private static string GetSystemXmlPathStatic(IConfiguration configuration)
    {
        var fileName = configuration.GetValue<string>("SystemXmlPath") ?? "system.xml";
        var fileLocation = new DataFileLocation(configuration, "SystemXmlPath", fileName);
        return fileLocation.FilePath;
    }

    // ── Parsers (read: backward-compatible with legacy + simplified formats) ──

    /// <summary>
    /// Parses system folders from either:
    ///   (A) Legacy:   &lt;SystemFolders&gt;&lt;SystemFolder&gt;...&lt;/SystemFolder&gt;...&lt;/SystemFolders&gt;
    ///   (B) Simplified: &lt;SystemFolder&gt;path1;path2&lt;/SystemFolder&gt;  (direct child)
    /// </summary>
    private static List<string> ParseFoldersCompat(XElement systemConfigElement)
    {
        // Legacy format: <SystemFolders> containing <SystemFolder> children
        var foldersElement = systemConfigElement.Element("SystemFolders");
        if (foldersElement != null)
        {
            return
            [
                .. foldersElement.Elements("SystemFolder")
                    .Select(f => f.Value.Trim())
                    .Where(f => !string.IsNullOrEmpty(f))
            ];
        }

        // Simplified format: direct <SystemFolder> child, semicolon-separated
        var directValue = systemConfigElement.Element("SystemFolder")?.Value;
        return ParseSemicolonList(directValue);
    }

    /// <summary>
    /// Parses a list from either:
    ///   (A) Legacy:   &lt;container&gt;&lt;itemElement&gt;...&lt;/itemElement&gt;...&lt;/container&gt;
    ///   (B) Simplified: &lt;container&gt;item1,item2&lt;/container&gt;  (direct child, comma-separated)
    /// </summary>
    private static List<string> ParseListCompat(XElement systemConfigElement, string containerName, string itemElementName)
    {
        var container = systemConfigElement.Element(containerName);
        if (container == null) return [];

        // Legacy format: child elements
        var childItems = container.Elements(itemElementName).ToList();
        if (childItems.Count > 0)
        {
            return
            [
                .. childItems
                    .Select(e => e.Value.Trim())
                    .Where(v => !string.IsNullOrEmpty(v))
            ];
        }

        // Simplified format: comma-separated value
        return ParseCommaList(container.Value);
    }

    private static List<string> ParseSemicolonList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];

        return
        [
            .. value.Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
        ];
    }

    private static List<string> ParseCommaList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];

        return
        [
            .. value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
        ];
    }

    // ── Emulator parsing ──────────────────────────────────────────────

    private static List<Emulator> ParseEmulators(XElement? emulatorsElement)
    {
        var emulators = new List<Emulator>();
        if (emulatorsElement is null) return emulators;

        foreach (var el in emulatorsElement.Elements("Emulator"))
        {
            emulators.Add(new Emulator
            {
                EmulatorName = el.Element("EmulatorName")?.Value ?? "",
                EmulatorLocation = el.Element("EmulatorPath")?.Value
                                   ?? el.Element("EmulatorLocation")?.Value ?? "",
                EmulatorParameters = el.Element("EmulatorParameters")?.Value ?? "",
                ReceiveANotificationOnEmulatorError =
                    !bool.TryParse(el.Element("ReceiveANotificationOnEmulatorError")?.Value, out var notify) || notify,
                ImagePackDownloadLink = el.Element("ImagePackDownloadLink")?.Value ?? "",
                ImagePackDownloadLink2 = el.Element("ImagePackDownloadLink2")?.Value ?? "",
                ImagePackDownloadLink3 = el.Element("ImagePackDownloadLink3")?.Value ?? "",
                ImagePackDownloadLink4 = el.Element("ImagePackDownloadLink4")?.Value ?? "",
                ImagePackDownloadLink5 = el.Element("ImagePackDownloadLink5")?.Value ?? "",
                ImagePackDownloadExtractPath = el.Element("ImagePackDownloadExtractPath")?.Value ?? ""
            });
        }

        return emulators;
    }

    // ── XML builders (legacy format for backward compatibility) ───────

    private static XElement BuildSystemConfigElement(
        string systemName,
        IEnumerable<string> systemFolders,
        string systemImageFolder,
        IEnumerable<string> fileFormatsToSearch,
        IEnumerable<string> fileFormatsToLaunch,
        bool extractFileBeforeLaunch,
        Emulator? emulator)
    {
        var element = new XElement("SystemConfig",
            new XElement("SystemName", systemName),
            new XElement("SystemFolders",
                systemFolders.Select(f => new XElement("SystemFolder", f))),
            new XElement("SystemImageFolder", systemImageFolder),
            new XElement("FileFormatsToSearch",
                fileFormatsToSearch.Select(f => new XElement("FormatToSearch", f))),
            new XElement("GroupByFolder", false),
            new XElement("DisableRecursiveSearch", false),
            extractFileBeforeLaunch ? new XElement("ExtractFileBeforeLaunch", true) : null,
            new XElement("FileFormatsToLaunch",
                fileFormatsToLaunch.Select(f => new XElement("FormatToLaunch", f))));

        if (emulator != null)
        {
            element.Add(BuildEmulatorsElement(emulator));
        }

        return element;
    }

    private static XElement BuildEmulatorsElement(Emulator emu)
    {
        var emuEl = new XElement("Emulator",
            new XElement("EmulatorName", emu.EmulatorName),
            new XElement("EmulatorLocation", emu.EmulatorLocation ?? ""),
            new XElement("EmulatorParameters", emu.EmulatorParameters ?? ""),
            new XElement("ReceiveANotificationOnEmulatorError",
                emu.ReceiveANotificationOnEmulatorError));

        // Preserve image pack links when present (for round-trip fidelity with old SimpleLauncher)
        AppendIfNotEmpty(emuEl, "ImagePackDownloadLink", emu.ImagePackDownloadLink);
        AppendIfNotEmpty(emuEl, "ImagePackDownloadLink2", emu.ImagePackDownloadLink2);
        AppendIfNotEmpty(emuEl, "ImagePackDownloadLink3", emu.ImagePackDownloadLink3);
        AppendIfNotEmpty(emuEl, "ImagePackDownloadLink4", emu.ImagePackDownloadLink4);
        AppendIfNotEmpty(emuEl, "ImagePackDownloadLink5", emu.ImagePackDownloadLink5);
        AppendIfNotEmpty(emuEl, "ImagePackDownloadExtractPath", emu.ImagePackDownloadExtractPath);

        return new XElement("Emulators", emuEl);
    }

    private static void AppendIfNotEmpty(XElement parent, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            parent.Add(new XElement(name, value));
    }

    /// <summary>
    /// Merges EasyMode-supplied fields into an existing SystemConfig element in-place,
    /// preserving any child elements not explicitly set (e.g. custom user additions).
    /// </summary>
    private static void MergeSystemConfigElement(
        XElement existing,
        string systemName,
        IEnumerable<string> systemFolders,
        string systemImageFolder,
        IEnumerable<string> fileFormatsToSearch,
        IEnumerable<string> fileFormatsToLaunch,
        bool extractFileBeforeLaunch,
        Emulator? emulator)
    {
        existing.SetElementValue("SystemName", systemName);

        // Merge SystemFolders
        var foldersEl = existing.Element("SystemFolders");
        if (foldersEl == null)
        {
            foldersEl = new XElement("SystemFolders");
            existing.Element("SystemName")?.AddAfterSelf(foldersEl);
        }

        foldersEl.ReplaceNodes(systemFolders.Select(f => new XElement("SystemFolder", f)));

        existing.SetElementValue("SystemImageFolder", systemImageFolder);

        // Merge FileFormatsToSearch
        var searchEl = existing.Element("FileFormatsToSearch");
        if (searchEl != null)
            searchEl.ReplaceNodes(fileFormatsToSearch.Select(f => new XElement("FormatToSearch", f)));

        // Merge FileFormatsToLaunch
        var launchEl = existing.Element("FileFormatsToLaunch");
        if (launchEl != null)
            launchEl.ReplaceNodes(fileFormatsToLaunch.Select(f => new XElement("FormatToLaunch", f)));

        // Only set GroupByFolder if not already present (preserve user preference)
        if (existing.Element("GroupByFolder") == null)
            existing.Add(new XElement("GroupByFolder", false));

        // Only set DisableRecursiveSearch if not already present
        if (existing.Element("DisableRecursiveSearch") == null)
            existing.Add(new XElement("DisableRecursiveSearch", false));

        // ExtractFileBeforeLaunch
        existing.SetElementValue("ExtractFileBeforeLaunch", extractFileBeforeLaunch ? true : null);

        // Merge Emulators: replace if present, add if absent
        if (emulator != null)
        {
            var existingEmulators = existing.Element("Emulators");
            if (existingEmulators != null)
                existingEmulators.ReplaceWith(BuildEmulatorsElement(emulator));
            else
                existing.Add(BuildEmulatorsElement(emulator));
        }
    }

    // ── EasyMode conversion ───────────────────────────────────────────

    private static Emulator ConvertEasyModeEmulator(EmulatorConfig? emulatorConfig)
    {
        if (emulatorConfig == null)
            return new Emulator { EmulatorName = "", EmulatorLocation = "", EmulatorParameters = "" };

        return new Emulator
        {
            EmulatorName = emulatorConfig.EmulatorName,
            EmulatorLocation = emulatorConfig.EmulatorLocation,
            EmulatorParameters = emulatorConfig.EmulatorParameters,
            ReceiveANotificationOnEmulatorError = true,
            ImagePackDownloadLink = emulatorConfig.ImagePackDownloadLink ?? "",
            ImagePackDownloadLink2 = emulatorConfig.ImagePackDownloadLink2 ?? "",
            ImagePackDownloadLink3 = emulatorConfig.ImagePackDownloadLink3 ?? "",
            ImagePackDownloadLink4 = emulatorConfig.ImagePackDownloadLink4 ?? "",
            ImagePackDownloadLink5 = emulatorConfig.ImagePackDownloadLink5 ?? "",
            ImagePackDownloadExtractPath = emulatorConfig.ImagePackDownloadExtractPath ?? ""
        };
    }
}

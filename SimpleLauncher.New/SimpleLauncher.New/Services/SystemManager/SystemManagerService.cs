using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services;

namespace SimpleLauncher.New.Services.SystemManager;

/// <summary>
/// Reads system.xml and exposes system configurations.
/// Phase 6: Lightweight implementation using Core's SystemManagerConfig.
/// </summary>
public class SystemManagerService
{
    private readonly IConfiguration _configuration;
    private List<SystemManagerConfig>? _cachedSystems;

    public SystemManagerService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Loads all system configurations from system.xml.
    /// </summary>
    public List<SystemManagerConfig> LoadSystems()
    {
        if (_cachedSystems is not null) return _cachedSystems;

        _cachedSystems = new List<SystemManagerConfig>();

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
                    SystemFolders = ParseFolders(element.Element("SystemFolder")?.Value),
                    SystemImageFolder = element.Element("SystemImageFolder")?.Value ?? "",
                    FileFormatsToSearch = ParseList(element.Element("FileFormatsToSearch")?.Value),
                    FileFormatsToLaunch = ParseList(element.Element("FileFormatsToLaunch")?.Value),
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
            // Log parse errors; return empty list so the UI can fall back gracefully
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

    private string GetSystemXmlPath()
    {
        var fileName = _configuration.GetValue<string>("SystemXmlPath") ?? "system.xml";
        var fileLocation = new DataFileLocation(_configuration, "SystemXmlPath", fileName);
        return fileLocation.FilePath;
    }

    private static List<string> ParseFolders(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];

        return value.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .Where(f => !string.IsNullOrEmpty(f))
            .ToList();
    }

    private static List<string> ParseList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];

        return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .Where(f => !string.IsNullOrEmpty(f))
            .ToList();
    }

    private static List<Emulator> ParseEmulators(XElement? emulatorsElement)
    {
        var emulators = new List<Emulator>();
        if (emulatorsElement is null) return emulators;

        foreach (var el in emulatorsElement.Elements("Emulator"))
        {
            emulators.Add(new Emulator
            {
                EmulatorName = el.Element("EmulatorName")?.Value ?? "",
                EmulatorLocation = el.Element("EmulatorPath")?.Value ?? "",
                EmulatorParameters = el.Element("EmulatorParameters")?.Value ?? ""
            });
        }

        return emulators;
    }
}

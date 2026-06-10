using System.ComponentModel;
using System.Xml.Serialization;
using SimpleLauncher.Services.EasyMode.Models;

// Required for DefaultValueAttribute

namespace SimpleLauncher.Models;

/// <summary>
/// Configuration for a system in Easy Mode, including file formats and emulator settings.
/// </summary>
public class EasyModeSystemConfig
{
    /// <summary>
    /// Gets or sets the display name of the system.
    /// </summary>
    public string SystemName { get; set; }

    /// <summary>
    /// Gets or sets the folder path containing ROM files.
    /// </summary>
    public string SystemFolder { get; set; }

    /// <summary>
    /// Gets or sets the folder path containing system images.
    /// </summary>
    public string SystemImageFolder { get; set; }

    /// <summary>
    /// Gets or sets the file extensions to search for ROMs.
    /// </summary>
    [XmlArray("FileFormatsToSearch")]
    [XmlArrayItem("FormatToSearch")]
    public List<string> FileFormatsToSearch { get; set; }

    /// <summary>
    /// Gets or sets whether compressed files should be extracted before launching.
    /// </summary>
    [XmlElement("ExtractFileBeforeLaunch")]
    [DefaultValue(false)]
    public bool ExtractFileBeforeLaunch { get; set; }

    /// <summary>
    /// Determines whether <see cref="ExtractFileBeforeLaunch"/> should be serialized to XML.
    /// </summary>
    public bool ShouldSerializeExtractFileBeforeLaunch()
    {
        return ExtractFileBeforeLaunch;
    }

    /// <summary>
    /// Gets or sets the file extensions used to identify launchable files.
    /// </summary>
    [XmlArray("FileFormatsToLaunch")]
    [XmlArrayItem("FormatToLaunch")]
    public List<string> FileFormatsToLaunch { get; set; }

    /// <summary>
    /// Gets or sets the emulator configuration for this system.
    /// </summary>
    [XmlElement("Emulators")]
    public EmulatorsConfig Emulators { get; set; }

    /// <summary>
    /// Validates that the configuration has the minimum required fields.
    /// </summary>
    public bool IsValid()
    {
        // Validate only SystemName; Emulators and nested Emulator can be null.
        return !string.IsNullOrWhiteSpace(SystemName);
    }
}
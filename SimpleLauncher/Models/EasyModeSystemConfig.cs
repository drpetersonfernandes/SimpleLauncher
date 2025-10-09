using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel; // Required for DefaultValueAttribute

namespace SimpleLauncher.Models;

public class EasyModeSystemConfig
{
    public string SystemName { get; set; }
    public string SystemFolder { get; set; }
    public string SystemImageFolder { get; set; }

    [XmlElement("SystemIsMAME")]
    [DefaultValue(false)] // When SystemIsMAME is false, it won't be serialized to XML
    public bool SystemIsMame { get; set; } // Changed to non-nullable bool

    [XmlArray("FileFormatsToSearch")]
    [XmlArrayItem("FormatToSearch")]
    public List<string> FileFormatsToSearch { get; set; }

    [XmlElement("ExtractFileBeforeLaunch")]
    [DefaultValue(false)] // When ExtractFileBeforeLaunch is false, it won't be serialized to XML
    public bool ExtractFileBeforeLaunch { get; set; } // Changed to non-nullable bool

    [XmlArray("FileFormatsToLaunch")]
    [XmlArrayItem("FormatToLaunch")]
    public List<string> FileFormatsToLaunch { get; set; }

    [XmlElement("Emulators")]
    public EmulatorsConfig Emulators { get; set; }

    public bool IsValid()
    {
        // Validate only SystemName; Emulators and nested Emulator can be null.
        return !string.IsNullOrWhiteSpace(SystemName);
    }
}
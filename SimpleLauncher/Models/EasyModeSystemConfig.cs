using System.Collections.Generic;
using System.Xml.Serialization;

namespace SimpleLauncher.Models;

public class EasyModeSystemConfig
{
    public string SystemName { get; set; }
    public string SystemFolder { get; set; }
    public string SystemImageFolder { get; set; }

    [XmlElement("SystemIsMAME")]
    public bool? SystemIsMameNullable { get; set; }

    [XmlIgnore]
    public bool SystemIsMame => SystemIsMameNullable ?? false;

    [XmlArray("FileFormatsToSearch")]
    [XmlArrayItem("FormatToSearch")]
    public List<string> FileFormatsToSearch { get; set; }

    [XmlElement("ExtractFileBeforeLaunch")]
    public bool? ExtractFileBeforeLaunchNullable { get; set; }

    [XmlIgnore]
    public bool ExtractFileBeforeLaunch => ExtractFileBeforeLaunchNullable ?? false;

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
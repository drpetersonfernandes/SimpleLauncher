using System.Xml.Serialization;

namespace SimpleLauncher.Models;

public class EmulatorsConfig
{
    [XmlElement("Emulator")]
    public EmulatorConfig Emulator { get; set; }
}
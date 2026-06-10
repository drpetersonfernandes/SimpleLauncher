using System.Xml.Serialization;

namespace SimpleLauncher.Services.EasyMode.Models;

public class EmulatorsConfig
{
    [XmlElement("Emulator")]
    public EmulatorConfig Emulator { get; set; }
}
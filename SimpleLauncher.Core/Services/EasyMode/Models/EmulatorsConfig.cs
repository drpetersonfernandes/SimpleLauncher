using System.Xml.Serialization;

namespace SimpleLauncher.Core.Services.EasyMode.Models;

public class EmulatorsConfig
{
    [XmlElement("Emulator")]
    public EmulatorConfig Emulator { get; set; }
}
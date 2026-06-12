using System.Xml.Serialization;

namespace SimpleLauncher.Services.EasyMode.Models;

/// <summary>
/// XML wrapper that holds a single <see cref="EmulatorConfig"/> element for serialization.
/// </summary>
public class EmulatorsConfig
{
    /// <summary>
    /// Gets or sets the emulator configuration entry.
    /// </summary>
    [XmlElement("Emulator")]
    public EmulatorConfig Emulator { get; set; }
}

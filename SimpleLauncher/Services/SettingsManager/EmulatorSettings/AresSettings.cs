using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Represents the user-configurable settings for the Ares emulator, persisted to the system configuration under the "Ares" section.
/// </summary>
public class AresSettings : IEmulatorSettings
{
    private const string SectionName = "Ares";

    /// <summary>
    /// Gets or sets the video driver used by the emulator (e.g., "OpenGL 3.2").
    /// </summary>
    public string VideoDriver { get; set; } = "OpenGL 3.2";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator runs in exclusive fullscreen mode.
    /// </summary>
    public bool Exclusive { get; set; }

    /// <summary>
    /// Gets or sets the video shader preset applied during emulation.
    /// </summary>
    public string Shader { get; set; } = "None";

    /// <summary>
    /// Gets or sets the internal resolution multiplier applied by the emulator.
    /// </summary>
    public int Multiplier { get; set; } = 2;

    /// <summary>
    /// Gets or sets the aspect ratio correction mode used by the emulator.
    /// </summary>
    public string AspectCorrection { get; set; } = "Standard";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator audio is muted.
    /// </summary>
    public bool Mute { get; set; }

    /// <summary>
    /// Gets or sets the master audio volume, ranging from 0.0 to 1.0.
    /// </summary>
    public double Volume { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator skips the console boot screen and starts games directly.
    /// </summary>
    public bool FastBoot { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether rewind support is enabled.
    /// </summary>
    public bool Rewind { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether run-ahead (input latency reduction) is enabled.
    /// </summary>
    public bool RunAhead { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator automatically saves memory when a game closes.
    /// </summary>
    public bool AutoSaveMemory { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the Ares settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        VideoDriver = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(VideoDriver), "OpenGL 3.2");
        Exclusive = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Exclusive), false);
        Shader = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Shader), "None");
        Multiplier = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Multiplier), 2);
        AspectCorrection = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectCorrection), "Standard");
        Mute = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Mute), false);
        Volume = EmulatorXmlHelpers.ReadDouble(s, SectionName, settings, nameof(Volume), 1.0);
        FastBoot = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(FastBoot), false);
        Rewind = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Rewind), false);
        RunAhead = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(RunAhead), false);
        AutoSaveMemory = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AutoSaveMemory), true);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    /// Serializes the Ares settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Ares settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("VideoDriver", VideoDriver),
            new XElement("Exclusive", Exclusive),
            new XElement("Shader", Shader),
            new XElement("Multiplier", Multiplier),
            new XElement("AspectCorrection", AspectCorrection),
            new XElement("Mute", Mute),
            new XElement("Volume", Volume.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new XElement("FastBoot", FastBoot),
            new XElement("Rewind", Rewind),
            new XElement("RunAhead", RunAhead),
            new XElement("AutoSaveMemory", AutoSaveMemory),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is an Ares settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not AresSettings src) return;

        VideoDriver = src.VideoDriver;
        Exclusive = src.Exclusive;
        Shader = src.Shader;
        Multiplier = src.Multiplier;
        AspectCorrection = src.AspectCorrection;
        Mute = src.Mute;
        Volume = src.Volume;
        FastBoot = src.FastBoot;
        Rewind = src.Rewind;
        RunAhead = src.RunAhead;
        AutoSaveMemory = src.AutoSaveMemory;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    /// Resets all Ares settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new AresSettings());
    }
}

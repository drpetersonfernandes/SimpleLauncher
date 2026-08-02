using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Represents the user-configurable settings for the Mednafen emulator, persisted to the system configuration under the "Mednafen" section.
/// </summary>
public class MednafenSettings : IEmulatorSettings
{
    private const string SectionName = "Mednafen";

    /// <summary>
    /// Gets or sets the video driver used by Mednafen (e.g., "opengl").
    /// </summary>
    public string VideoDriver { get; set; } = "opengl";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; } = true;

    /// <summary>
    /// Gets or sets the video stretching mode (e.g., "aspect").
    /// </summary>
    public string Stretch { get; set; } = "aspect";

    /// <summary>
    /// Gets or sets a value indicating whether bilinear filtering is applied to the video output.
    /// </summary>
    public bool Bilinear { get; set; }

    /// <summary>
    /// Gets or sets the scanline intensity applied to the video output.
    /// </summary>
    public int Scanlines { get; set; }

    /// <summary>
    /// Gets or sets the video shader preset applied during emulation (e.g., "none").
    /// </summary>
    public string Shader { get; set; } = "none";

    /// <summary>
    /// Gets or sets the special video effect applied during emulation (e.g., "none").
    /// </summary>
    public string Special { get; set; } = "none";

    /// <summary>
    /// Gets or sets the audio volume percentage.
    /// </summary>
    public int Volume { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether cheat support is enabled.
    /// </summary>
    public bool Cheats { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether rewind support is enabled.
    /// </summary>
    public bool Rewind { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the Mednafen settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        VideoDriver = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(VideoDriver), "opengl");
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        Stretch = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Stretch), "aspect");
        Bilinear = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Bilinear), false);
        Scanlines = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Scanlines), 0);
        Shader = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Shader), "none");
        Special = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Special), "none");
        Volume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Volume), 100);
        Cheats = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Cheats), true);
        Rewind = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Rewind), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    /// Serializes the Mednafen settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Mednafen settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("VideoDriver", VideoDriver),
            new XElement("Fullscreen", Fullscreen),
            new XElement("Vsync", Vsync),
            new XElement("Stretch", Stretch),
            new XElement("Bilinear", Bilinear),
            new XElement("Scanlines", Scanlines),
            new XElement("Shader", Shader),
            new XElement("Special", Special),
            new XElement("Volume", Volume),
            new XElement("Cheats", Cheats),
            new XElement("Rewind", Rewind),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a Mednafen settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not MednafenSettings src) return;

        VideoDriver = src.VideoDriver;
        Fullscreen = src.Fullscreen;
        Vsync = src.Vsync;
        Stretch = src.Stretch;
        Bilinear = src.Bilinear;
        Scanlines = src.Scanlines;
        Shader = src.Shader;
        Special = src.Special;
        Volume = src.Volume;
        Cheats = src.Cheats;
        Rewind = src.Rewind;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    /// Resets all Mednafen settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new MednafenSettings());
    }
}

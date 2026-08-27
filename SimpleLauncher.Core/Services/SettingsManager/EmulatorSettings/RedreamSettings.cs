using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
/// Represents the user-configurable settings for the Redream emulator, persisted to the system configuration under the "Redream" section.
/// </summary>
public class RedreamSettings : IEmulatorSettings
{
    private const string SectionName = "Redream";

    /// <summary>
    /// Gets or sets the Dreamcast video cable type (e.g., "vga").
    /// </summary>
    public string Cable { get; set; } = "vga";

    /// <summary>
    /// Gets or sets the video broadcast standard (e.g., "ntsc").
    /// </summary>
    public string Broadcast { get; set; } = "ntsc";

    /// <summary>
    /// Gets or sets the emulator UI language (e.g., "english").
    /// </summary>
    public string Language { get; set; } = "english";

    /// <summary>
    /// Gets or sets the emulated console region (e.g., "usa").
    /// </summary>
    public string Region { get; set; } = "usa";

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether frame skipping is enabled.
    /// </summary>
    public bool Frameskip { get; set; } = true;

    /// <summary>
    /// Gets or sets the aspect ratio of the emulation window (e.g., "4:3").
    /// </summary>
    public string Aspect { get; set; } = "4:3";

    /// <summary>
    /// Gets or sets the internal resolution multiplier applied during emulation.
    /// </summary>
    public int Res { get; set; } = 2;

    /// <summary>
    /// Gets or sets the graphics renderer used by the emulator (e.g., "hle_perstrip").
    /// </summary>
    public string Renderer { get; set; } = "hle_perstrip";

    /// <summary>
    /// Gets or sets the fullscreen mode used by the emulator (e.g., "exclusive fullscreen").
    /// </summary>
    public string Fullmode { get; set; } = "exclusive fullscreen";

    /// <summary>
    /// Gets or sets the audio volume percentage.
    /// </summary>
    public int Volume { get; set; } = 100;

    /// <summary>
    /// Gets or sets the audio latency in milliseconds.
    /// </summary>
    public int Latency { get; set; } = 32;

    /// <summary>
    /// Gets or sets a value indicating whether the frames per second counter is displayed.
    /// </summary>
    public bool Framerate { get; set; }

    /// <summary>
    /// Gets or sets the width of the emulation window in windowed mode.
    /// </summary>
    public int Width { get; set; } = 1280;

    /// <summary>
    /// Gets or sets the height of the emulation window in windowed mode.
    /// </summary>
    public int Height { get; set; } = 720;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the Redream settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Cable = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Cable), "vga");
        Broadcast = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Broadcast), "ntsc");
        Language = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Language), "english");
        Region = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Region), "usa");
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        Frameskip = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Frameskip), true);
        Aspect = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Aspect), "4:3");
        Res = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Res), 2);
        Renderer = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Renderer), "hle_perstrip");
        Fullmode = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Fullmode), "exclusive fullscreen");
        Volume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Volume), 100);
        Latency = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Latency), 32);
        Framerate = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Framerate), false);
        Width = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Width), 1280);
        Height = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Height), 720);
        ShowSettingsBeforeLaunch =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    /// Serializes the Redream settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Redream settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Cable", Cable),
            new XElement("Broadcast", Broadcast),
            new XElement("Language", Language),
            new XElement("Region", Region),
            new XElement("Vsync", Vsync),
            new XElement("Frameskip", Frameskip),
            new XElement("Aspect", Aspect),
            new XElement("Res", Res),
            new XElement("Renderer", Renderer),
            new XElement("Fullmode", Fullmode),
            new XElement("Volume", Volume),
            new XElement("Latency", Latency),
            new XElement("Framerate", Framerate),
            new XElement("Width", Width),
            new XElement("Height", Height),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a Redream settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not RedreamSettings src) return;

        Cable = src.Cable;
        Broadcast = src.Broadcast;
        Language = src.Language;
        Region = src.Region;
        Vsync = src.Vsync;
        Frameskip = src.Frameskip;
        Aspect = src.Aspect;
        Res = src.Res;
        Renderer = src.Renderer;
        Fullmode = src.Fullmode;
        Volume = src.Volume;
        Latency = src.Latency;
        Framerate = src.Framerate;
        Width = src.Width;
        Height = src.Height;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    /// Resets all Redream settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new RedreamSettings());
    }
}
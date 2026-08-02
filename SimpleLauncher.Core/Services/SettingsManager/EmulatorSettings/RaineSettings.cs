using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
/// Represents the user-configurable settings for the Raine emulator, persisted to the system configuration under the "Raine" section.
/// </summary>
public class RaineSettings : IEmulatorSettings
{
    private const string SectionName = "Raine";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets the horizontal resolution of the emulation window.
    /// </summary>
    public int ResX { get; set; } = 640;

    /// <summary>
    /// Gets or sets the vertical resolution of the emulation window.
    /// </summary>
    public int ResY { get; set; } = 480;

    /// <summary>
    /// Gets or sets a value indicating whether the original game aspect ratio is preserved.
    /// </summary>
    public bool FixAspectRatio { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; } = true;

    /// <summary>
    /// Gets or sets the audio driver used by Raine (e.g., "directsound").
    /// </summary>
    public string SoundDriver { get; set; } = "directsound";

    /// <summary>
    /// Gets or sets the audio sample rate in Hz.
    /// </summary>
    public int SampleRate { get; set; } = 44100;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the frames per second counter is displayed.
    /// </summary>
    public bool ShowFps { get; set; }

    /// <summary>
    /// Gets or sets the number of frames skipped to maintain emulation speed.
    /// </summary>
    public int FrameSkip { get; set; }

    /// <summary>
    /// Gets or sets the BIOS file used for Neo Geo CD emulation.
    /// </summary>
    public string NeoCdBios { get; set; } = "";

    /// <summary>
    /// Gets or sets the music volume percentage for Neo Geo CD emulation.
    /// </summary>
    public int MusicVolume { get; set; } = 60;

    /// <summary>
    /// Gets or sets the sound effects volume percentage for Neo Geo CD emulation.
    /// </summary>
    public int SfxVolume { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether sound effects are muted for Neo Geo CD emulation.
    /// </summary>
    public bool MuteSfx { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether music is muted for Neo Geo CD emulation.
    /// </summary>
    public bool MuteMusic { get; set; }

    /// <summary>
    /// Gets or sets the ROM directory used by Raine.
    /// </summary>
    public string RomDirectory { get; set; } = "";

    /// <summary>
    /// Loads the Raine settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        ResX = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResX), 640);
        ResY = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResY), 480);
        FixAspectRatio = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(FixAspectRatio), true);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        SoundDriver = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(SoundDriver), "directsound");
        SampleRate = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(SampleRate), 44100);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
        ShowFps = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowFps), false);
        FrameSkip = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(FrameSkip), 0);
        NeoCdBios = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(NeoCdBios), "");
        MusicVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(MusicVolume), 60);
        SfxVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(SfxVolume), 60);
        MuteSfx = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(MuteSfx), false);
        MuteMusic = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(MuteMusic), false);
        RomDirectory = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(RomDirectory), "");
    }


    /// <summary>
    /// Serializes the Raine settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Raine settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("ResX", ResX),
            new XElement("ResY", ResY),
            new XElement("FixAspectRatio", FixAspectRatio),
            new XElement("Vsync", Vsync),
            new XElement("SoundDriver", SoundDriver),
            new XElement("SampleRate", SampleRate),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch),
            new XElement("ShowFps", ShowFps),
            new XElement("FrameSkip", FrameSkip),
            new XElement("NeoCdBios", NeoCdBios),
            new XElement("MusicVolume", MusicVolume),
            new XElement("SfxVolume", SfxVolume),
            new XElement("MuteSfx", MuteSfx),
            new XElement("MuteMusic", MuteMusic),
            new XElement("RomDirectory", RomDirectory));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a Raine settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not RaineSettings src) return;

        Fullscreen = src.Fullscreen;
        ResX = src.ResX;
        ResY = src.ResY;
        FixAspectRatio = src.FixAspectRatio;
        Vsync = src.Vsync;
        SoundDriver = src.SoundDriver;
        SampleRate = src.SampleRate;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
        ShowFps = src.ShowFps;
        FrameSkip = src.FrameSkip;
        NeoCdBios = src.NeoCdBios;
        MusicVolume = src.MusicVolume;
        SfxVolume = src.SfxVolume;
        MuteSfx = src.MuteSfx;
        MuteMusic = src.MuteMusic;
        RomDirectory = src.RomDirectory;
    }


    /// <summary>
    /// Resets all Raine settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new RaineSettings());
    }
}

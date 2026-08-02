using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
/// Represents the user-configurable settings for the Mesen emulator, persisted to the system configuration under the "Mesen" section.
/// </summary>
public class MesenSettings : IEmulatorSettings
{
    private const string SectionName = "Mesen";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; }

    /// <summary>
    /// Gets or sets the aspect ratio mode of the emulation window (e.g., "NoStretching").
    /// </summary>
    public string AspectRatio { get; set; } = "NoStretching";

    /// <summary>
    /// Gets or sets a value indicating whether bilinear interpolation is applied to the video output.
    /// </summary>
    public bool Bilinear { get; set; }

    /// <summary>
    /// Gets or sets the video filter applied during emulation (e.g., "None").
    /// </summary>
    public string VideoFilter { get; set; } = "None";

    /// <summary>
    /// Gets or sets a value indicating whether audio output is enabled.
    /// </summary>
    public bool EnableAudio { get; set; } = true;

    /// <summary>
    /// Gets or sets the master audio volume percentage.
    /// </summary>
    public int MasterVolume { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether rewind support is enabled.
    /// </summary>
    public bool Rewind { get; set; }

    /// <summary>
    /// Gets or sets the number of run-ahead frames used to reduce input latency.
    /// </summary>
    public int RunAhead { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator pauses when the window loses focus.
    /// </summary>
    public bool PauseInBackground { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the Mesen settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), false);
        AspectRatio = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatio), "NoStretching");
        Bilinear = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Bilinear), false);
        VideoFilter = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(VideoFilter), "None");
        EnableAudio = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(EnableAudio), true);
        MasterVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(MasterVolume), 100);
        Rewind = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Rewind), false);
        RunAhead = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(RunAhead), 0);
        PauseInBackground = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(PauseInBackground), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    /// Serializes the Mesen settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Mesen settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("Vsync", Vsync),
            new XElement("AspectRatio", AspectRatio),
            new XElement("Bilinear", Bilinear),
            new XElement("VideoFilter", VideoFilter),
            new XElement("EnableAudio", EnableAudio),
            new XElement("MasterVolume", MasterVolume),
            new XElement("Rewind", Rewind),
            new XElement("RunAhead", RunAhead),
            new XElement("PauseInBackground", PauseInBackground),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a Mesen settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not MesenSettings src) return;

        Fullscreen = src.Fullscreen;
        Vsync = src.Vsync;
        AspectRatio = src.AspectRatio;
        Bilinear = src.Bilinear;
        VideoFilter = src.VideoFilter;
        EnableAudio = src.EnableAudio;
        MasterVolume = src.MasterVolume;
        Rewind = src.Rewind;
        RunAhead = src.RunAhead;
        PauseInBackground = src.PauseInBackground;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    /// Resets all Mesen settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new MesenSettings());
    }
}

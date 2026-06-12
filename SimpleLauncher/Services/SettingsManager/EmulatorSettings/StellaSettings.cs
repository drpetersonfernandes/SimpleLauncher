using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Configuration settings for the Stella (Atari 2600) emulator, including video, audio, and time machine features.
/// </summary>
public class StellaSettings : IEmulatorSettings
{
    private const string SectionName = "Stella";

    /// <summary>Gets or sets whether fullscreen mode is enabled.</summary>
    public bool Fullscreen { get; set; }

    /// <summary>Gets or sets whether vertical sync is enabled.</summary>
    public bool Vsync { get; set; } = true;

    /// <summary>Gets or sets the video driver backend (e.g., "direct3d").</summary>
    public string VideoDriver { get; set; } = "direct3d";

    /// <summary>Gets or sets whether aspect ratio correction is enabled.</summary>
    public bool CorrectAspect { get; set; } = true;

    /// <summary>Gets or sets the TV filter intensity (0 = off).</summary>
    public int TvFilter { get; set; }

    /// <summary>Gets or sets the scanline intensity (0 = off).</summary>
    public int Scanlines { get; set; }

    /// <summary>Gets or sets whether audio output is enabled.</summary>
    public bool AudioEnabled { get; set; } = true;

    /// <summary>Gets or sets the audio volume level (0–100).</summary>
    public int AudioVolume { get; set; } = 80;

    /// <summary>Gets or sets whether the time machine rewind feature is enabled.</summary>
    public bool TimeMachine { get; set; } = true;

    /// <summary>Gets or sets whether a confirmation prompt is shown on exit.</summary>
    public bool ConfirmExit { get; set; }

    /// <summary>Gets or sets whether the emulator settings dialog is shown before launching a game.</summary>
    public bool ShowSettingsBeforeLaunch { get; set; }

    /// <summary>Loads emulator settings from the provided XML configuration element.</summary>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        VideoDriver = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(VideoDriver), "direct3d");
        CorrectAspect = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(CorrectAspect), true);
        TvFilter = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(TvFilter), 0);
        Scanlines = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Scanlines), 0);
        AudioEnabled = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AudioEnabled), true);
        AudioVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(AudioVolume), 80);
        TimeMachine = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(TimeMachine), true);
        ConfirmExit = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ConfirmExit), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    /// <summary>Serializes the current settings to an XML element.</summary>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("Vsync", Vsync),
            new XElement("VideoDriver", VideoDriver),
            new XElement("CorrectAspect", CorrectAspect),
            new XElement("TvFilter", TvFilter),
            new XElement("Scanlines", Scanlines),
            new XElement("AudioEnabled", AudioEnabled),
            new XElement("AudioVolume", AudioVolume),
            new XElement("TimeMachine", TimeMachine),
            new XElement("ConfirmExit", ConfirmExit),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    /// <summary>Copies all settings from another StellaSettings instance.</summary>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not StellaSettings src) return;

        Fullscreen = src.Fullscreen;
        Vsync = src.Vsync;
        VideoDriver = src.VideoDriver;
        CorrectAspect = src.CorrectAspect;
        TvFilter = src.TvFilter;
        Scanlines = src.Scanlines;
        AudioEnabled = src.AudioEnabled;
        AudioVolume = src.AudioVolume;
        TimeMachine = src.TimeMachine;
        ConfirmExit = src.ConfirmExit;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    /// <summary>Resets all settings to their default values.</summary>
    public void ResetDefaults()
    {
        CopyFrom(new StellaSettings());
    }
}

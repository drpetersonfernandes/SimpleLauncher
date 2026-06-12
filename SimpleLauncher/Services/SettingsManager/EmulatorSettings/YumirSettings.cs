using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Configuration settings for the Yumir emulator, including video, audio, and region detection options.
/// </summary>
public class YumirSettings : IEmulatorSettings
{
    private const string SectionName = "Yumir";

    /// <summary>Gets or sets whether fullscreen mode is enabled.</summary>
    public bool Fullscreen { get; set; }

    /// <summary>Gets or sets the audio volume level (0.0 to 1.0).</summary>
    public double Volume { get; set; } = 0.8;

    /// <summary>Gets or sets whether audio output is muted.</summary>
    public bool Mute { get; set; }

    /// <summary>Gets or sets the video standard (e.g., "PAL", "NTSC").</summary>
    public string VideoStandard { get; set; } = "PAL";

    /// <summary>Gets or sets whether automatic region detection is enabled.</summary>
    public bool AutoDetectRegion { get; set; } = true;

    /// <summary>Gets or sets whether emulation pauses when the window loses focus.</summary>
    public bool PauseWhenUnfocused { get; set; }

    /// <summary>Gets or sets the forced aspect ratio value.</summary>
    public double ForcedAspect { get; set; } = 1.7777777777777777;

    /// <summary>Gets or sets whether a custom aspect ratio is forced.</summary>
    public bool ForceAspectRatio { get; set; }

    /// <summary>Gets or sets whether latency reduction is enabled.</summary>
    public bool ReduceLatency { get; set; } = true;

    /// <summary>Gets or sets whether the emulator settings dialog is shown before launching a game.</summary>
    public bool ShowSettingsBeforeLaunch { get; set; }

    /// <summary>Loads emulator settings from the provided XML configuration element.</summary>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Volume = EmulatorXmlHelpers.ReadDouble(s, SectionName, settings, nameof(Volume), 0.8);
        Mute = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Mute), false);
        VideoStandard = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(VideoStandard), "PAL");
        AutoDetectRegion = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AutoDetectRegion), true);
        PauseWhenUnfocused = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(PauseWhenUnfocused), false);
        ForcedAspect = EmulatorXmlHelpers.ReadDouble(s, SectionName, settings, nameof(ForcedAspect), 1.7777777777777777);
        ForceAspectRatio = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ForceAspectRatio), false);
        ReduceLatency = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ReduceLatency), true);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    /// <summary>Serializes the current settings to an XML element.</summary>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("Volume", Volume.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new XElement("Mute", Mute),
            new XElement("VideoStandard", VideoStandard),
            new XElement("AutoDetectRegion", AutoDetectRegion),
            new XElement("PauseWhenUnfocused", PauseWhenUnfocused),
            new XElement("ForcedAspect", ForcedAspect.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new XElement("ForceAspectRatio", ForceAspectRatio),
            new XElement("ReduceLatency", ReduceLatency),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    /// <summary>Copies all settings from another YumirSettings instance.</summary>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not YumirSettings src) return;

        Fullscreen = src.Fullscreen;
        Volume = src.Volume;
        Mute = src.Mute;
        VideoStandard = src.VideoStandard;
        AutoDetectRegion = src.AutoDetectRegion;
        PauseWhenUnfocused = src.PauseWhenUnfocused;
        ForcedAspect = src.ForcedAspect;
        ForceAspectRatio = src.ForceAspectRatio;
        ReduceLatency = src.ReduceLatency;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    /// <summary>Resets all settings to their default values.</summary>
    public void ResetDefaults()
    {
        CopyFrom(new YumirSettings());
    }
}

using System.Xml.Linq;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

public class YumirSettings : IEmulatorSettings
{
    private const string SectionName = "Yumir";

    public bool Fullscreen { get; set; }
    public double Volume { get; set; } = 0.8;
    public bool Mute { get; set; }
    public string VideoStandard { get; set; } = "PAL";
    public bool AutoDetectRegion { get; set; } = true;
    public bool PauseWhenUnfocused { get; set; }
    public double ForcedAspect { get; set; } = 1.7777777777777777;
    public bool ForceAspectRatio { get; set; }
    public bool ReduceLatency { get; set; } = true;
    public bool ShowSettingsBeforeLaunch { get; set; }

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

    public void ResetDefaults()
    {
        CopyFrom(new YumirSettings());
    }
}

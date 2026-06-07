using System.Xml.Linq;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

public class MesenSettings : IEmulatorSettings
{
    private const string SectionName = "Mesen";

    public bool Fullscreen { get; set; }
    public bool Vsync { get; set; }
    public string AspectRatio { get; set; } = "NoStretching";
    public bool Bilinear { get; set; }
    public string VideoFilter { get; set; } = "None";
    public bool EnableAudio { get; set; } = true;
    public int MasterVolume { get; set; } = 100;
    public bool Rewind { get; set; }
    public int RunAhead { get; set; }
    public bool PauseInBackground { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }

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

    public void ResetDefaults()
    {
        CopyFrom(new MesenSettings());
    }
}

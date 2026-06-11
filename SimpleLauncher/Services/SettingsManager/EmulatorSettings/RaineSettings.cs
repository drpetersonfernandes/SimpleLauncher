using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

public class RaineSettings : IEmulatorSettings
{
    private const string SectionName = "Raine";

    public bool Fullscreen { get; set; }
    public int ResX { get; set; } = 640;
    public int ResY { get; set; } = 480;
    public bool FixAspectRatio { get; set; } = true;
    public bool Vsync { get; set; } = true;
    public string SoundDriver { get; set; } = "directsound";
    public int SampleRate { get; set; } = 44100;
    public bool ShowSettingsBeforeLaunch { get; set; }
    public bool ShowFps { get; set; }
    public int FrameSkip { get; set; }
    public string NeoCdBios { get; set; } = "";
    public int MusicVolume { get; set; } = 60;
    public int SfxVolume { get; set; } = 60;
    public bool MuteSfx { get; set; }
    public bool MuteMusic { get; set; }
    public string RomDirectory { get; set; } = "";

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

    public void ResetDefaults()
    {
        CopyFrom(new RaineSettings());
    }
}

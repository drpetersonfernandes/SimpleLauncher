using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

public class MednafenSettings : IEmulatorSettings
{
    private const string SectionName = "Mednafen";

    public string VideoDriver { get; set; } = "opengl";
    public bool Fullscreen { get; set; }
    public bool Vsync { get; set; } = true;
    public string Stretch { get; set; } = "aspect";
    public bool Bilinear { get; set; }
    public int Scanlines { get; set; }
    public string Shader { get; set; } = "none";
    public string Special { get; set; } = "none";
    public int Volume { get; set; } = 100;
    public bool Cheats { get; set; } = true;
    public bool Rewind { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }

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

    public void ResetDefaults()
    {
        CopyFrom(new MednafenSettings());
    }
}

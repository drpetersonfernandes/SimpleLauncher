using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

public class RedreamSettings : IEmulatorSettings
{
    private const string SectionName = "Redream";

    public string Cable { get; set; } = "vga";
    public string Broadcast { get; set; } = "ntsc";
    public string Language { get; set; } = "english";
    public string Region { get; set; } = "usa";
    public bool Vsync { get; set; } = true;
    public bool Frameskip { get; set; } = true;
    public string Aspect { get; set; } = "4:3";
    public int Res { get; set; } = 2;
    public string Renderer { get; set; } = "hle_perstrip";
    public string Fullmode { get; set; } = "exclusive fullscreen";
    public int Volume { get; set; } = 100;
    public int Latency { get; set; } = 32;
    public bool Framerate { get; set; }
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public bool ShowSettingsBeforeLaunch { get; set; }

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
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

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

    public void ResetDefaults()
    {
        CopyFrom(new RedreamSettings());
    }
}

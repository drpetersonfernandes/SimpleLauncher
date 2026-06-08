using System.Xml.Linq;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

public class XeniaSettings : IEmulatorSettings
{
    private const string SectionName = "Xenia";

    public string ReadbackResolve { get; set; } = "none";
    public bool GammaSrgb { get; set; }
    public bool Vibration { get; set; } = true;
    public bool MountCache { get; set; } = true;
    public string Gpu { get; set; } = "d3d12";
    public bool Vsync { get; set; } = true;
    public int ResScaleX { get; set; } = 1;
    public int ResScaleY { get; set; } = 1;
    public bool Fullscreen { get; set; }
    public string Apu { get; set; } = "xaudio2";
    public bool Mute { get; set; }
    public string Aa { get; set; } = "";
    public string Scaling { get; set; } = "fsr";
    public bool ApplyPatches { get; set; } = true;
    public bool DiscordPresence { get; set; } = true;
    public int UserLanguage { get; set; } = 1;
    public string Hid { get; set; } = "xinput";
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        ReadbackResolve = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(ReadbackResolve), "none");
        GammaSrgb = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(GammaSrgb), false);
        Vibration = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vibration), true);
        MountCache = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(MountCache), true);
        Gpu = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Gpu), "d3d12");
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        ResScaleX = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResScaleX), 1);
        ResScaleY = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResScaleY), 1);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Apu = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Apu), "xaudio2");
        Mute = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Mute), false);
        Aa = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Aa), "");
        Scaling = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Scaling), "fsr");
        ApplyPatches = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ApplyPatches), true);
        DiscordPresence = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DiscordPresence), true);
        UserLanguage = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(UserLanguage), 1);
        Hid = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Hid), "xinput");
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("ReadbackResolve", ReadbackResolve),
            new XElement("GammaSrgb", GammaSrgb),
            new XElement("Vibration", Vibration),
            new XElement("MountCache", MountCache),
            new XElement("Gpu", Gpu),
            new XElement("Vsync", Vsync),
            new XElement("ResScaleX", ResScaleX),
            new XElement("ResScaleY", ResScaleY),
            new XElement("Fullscreen", Fullscreen),
            new XElement("Apu", Apu),
            new XElement("Mute", Mute),
            new XElement("Aa", Aa),
            new XElement("Scaling", Scaling),
            new XElement("ApplyPatches", ApplyPatches),
            new XElement("DiscordPresence", DiscordPresence),
            new XElement("UserLanguage", UserLanguage),
            new XElement("Hid", Hid),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not XeniaSettings src) return;

        ReadbackResolve = src.ReadbackResolve;
        GammaSrgb = src.GammaSrgb;
        Vibration = src.Vibration;
        MountCache = src.MountCache;
        Gpu = src.Gpu;
        Vsync = src.Vsync;
        ResScaleX = src.ResScaleX;
        ResScaleY = src.ResScaleY;
        Fullscreen = src.Fullscreen;
        Apu = src.Apu;
        Mute = src.Mute;
        Aa = src.Aa;
        Scaling = src.Scaling;
        ApplyPatches = src.ApplyPatches;
        DiscordPresence = src.DiscordPresence;
        UserLanguage = src.UserLanguage;
        Hid = src.Hid;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new XeniaSettings());
    }
}

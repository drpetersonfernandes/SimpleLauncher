using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

public class Pcsx2Settings : IEmulatorSettings
{
    private const string SectionName = "Pcsx2";

    public bool StartFullscreen { get; set; } = true;
    public string AspectRatio { get; set; } = "16:9";
    public int Renderer { get; set; } = 14;
    public int UpscaleMultiplier { get; set; } = 2;
    public bool Vsync { get; set; }
    public bool EnableCheats { get; set; }
    public bool EnableWidescreenPatches { get; set; }
    public int Volume { get; set; } = 100;
    public bool AchievementsEnabled { get; set; }
    public bool AchievementsHardcore { get; set; } = true;
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        StartFullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(StartFullscreen), true);
        AspectRatio = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatio), "16:9");
        Renderer = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Renderer), 14);
        UpscaleMultiplier = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(UpscaleMultiplier), 2);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), false);
        EnableCheats = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(EnableCheats), false);
        EnableWidescreenPatches = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(EnableWidescreenPatches), false);
        Volume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Volume), 100);
        AchievementsEnabled = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AchievementsEnabled), false);
        AchievementsHardcore = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AchievementsHardcore), true);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("StartFullscreen", StartFullscreen),
            new XElement("AspectRatio", AspectRatio),
            new XElement("Renderer", Renderer),
            new XElement("UpscaleMultiplier", UpscaleMultiplier),
            new XElement("Vsync", Vsync),
            new XElement("EnableCheats", EnableCheats),
            new XElement("EnableWidescreenPatches", EnableWidescreenPatches),
            new XElement("Volume", Volume),
            new XElement("AchievementsEnabled", AchievementsEnabled),
            new XElement("AchievementsHardcore", AchievementsHardcore),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not Pcsx2Settings src) return;

        StartFullscreen = src.StartFullscreen;
        AspectRatio = src.AspectRatio;
        Renderer = src.Renderer;
        UpscaleMultiplier = src.UpscaleMultiplier;
        Vsync = src.Vsync;
        EnableCheats = src.EnableCheats;
        EnableWidescreenPatches = src.EnableWidescreenPatches;
        Volume = src.Volume;
        AchievementsEnabled = src.AchievementsEnabled;
        AchievementsHardcore = src.AchievementsHardcore;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new Pcsx2Settings());
    }
}

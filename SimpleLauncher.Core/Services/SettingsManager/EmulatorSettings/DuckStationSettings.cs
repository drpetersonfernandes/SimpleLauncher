using System.Xml.Linq;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

public class DuckStationSettings : IEmulatorSettings
{
    private const string SectionName = "DuckStation";

    public bool StartFullscreen { get; set; }
    public bool PauseOnFocusLoss { get; set; } = true;
    public bool SaveStateOnExit { get; set; } = true;
    public bool RewindEnable { get; set; }
    public int RunaheadFrameCount { get; set; }
    public string Renderer { get; set; } = "Automatic";
    public int ResolutionScale { get; set; } = 2;
    public string TextureFilter { get; set; } = "Nearest";
    public bool WidescreenHack { get; set; }
    public bool PgxpEnable { get; set; }
    public string AspectRatio { get; set; } = "16:9";
    public bool Vsync { get; set; }
    public int OutputVolume { get; set; } = 100;
    public bool OutputMuted { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        StartFullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(StartFullscreen), false);
        PauseOnFocusLoss = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(PauseOnFocusLoss), true);
        SaveStateOnExit = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(SaveStateOnExit), true);
        RewindEnable = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(RewindEnable), false);
        RunaheadFrameCount = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(RunaheadFrameCount), 0);
        Renderer = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Renderer), "Automatic");
        ResolutionScale = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResolutionScale), 2);
        TextureFilter = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(TextureFilter), "Nearest");
        WidescreenHack = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(WidescreenHack), false);
        PgxpEnable = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(PgxpEnable), false);
        AspectRatio = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatio), "16:9");
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), false);
        OutputVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(OutputVolume), 100);
        OutputMuted = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OutputMuted), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("StartFullscreen", StartFullscreen),
            new XElement("PauseOnFocusLoss", PauseOnFocusLoss),
            new XElement("SaveStateOnExit", SaveStateOnExit),
            new XElement("RewindEnable", RewindEnable),
            new XElement("RunaheadFrameCount", RunaheadFrameCount),
            new XElement("Renderer", Renderer),
            new XElement("ResolutionScale", ResolutionScale),
            new XElement("TextureFilter", TextureFilter),
            new XElement("WidescreenHack", WidescreenHack),
            new XElement("PgxpEnable", PgxpEnable),
            new XElement("AspectRatio", AspectRatio),
            new XElement("Vsync", Vsync),
            new XElement("OutputVolume", OutputVolume),
            new XElement("OutputMuted", OutputMuted),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not DuckStationSettings src) return;

        StartFullscreen = src.StartFullscreen;
        PauseOnFocusLoss = src.PauseOnFocusLoss;
        SaveStateOnExit = src.SaveStateOnExit;
        RewindEnable = src.RewindEnable;
        RunaheadFrameCount = src.RunaheadFrameCount;
        Renderer = src.Renderer;
        ResolutionScale = src.ResolutionScale;
        TextureFilter = src.TextureFilter;
        WidescreenHack = src.WidescreenHack;
        PgxpEnable = src.PgxpEnable;
        AspectRatio = src.AspectRatio;
        Vsync = src.Vsync;
        OutputVolume = src.OutputVolume;
        OutputMuted = src.OutputMuted;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new DuckStationSettings());
    }
}

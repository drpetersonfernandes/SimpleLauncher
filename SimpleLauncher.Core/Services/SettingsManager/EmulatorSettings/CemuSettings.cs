using System.Xml.Linq;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

public class CemuSettings : IEmulatorSettings
{
    private const string SectionName = "Cemu";

    public bool Fullscreen { get; set; }
    public int GraphicApi { get; set; } = 1;
    public int Vsync { get; set; } = 1;
    public bool AsyncCompile { get; set; } = true;
    public int TvVolume { get; set; } = 50;
    public int ConsoleLanguage { get; set; } = 1;
    public bool DiscordPresence { get; set; } = true;
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        GraphicApi = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(GraphicApi), 1);
        Vsync = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Vsync), 1);
        AsyncCompile = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AsyncCompile), true);
        TvVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(TvVolume), 50);
        ConsoleLanguage = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ConsoleLanguage), 1);
        DiscordPresence = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DiscordPresence), true);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("GraphicApi", GraphicApi),
            new XElement("Vsync", Vsync),
            new XElement("AsyncCompile", AsyncCompile),
            new XElement("TvVolume", TvVolume),
            new XElement("ConsoleLanguage", ConsoleLanguage),
            new XElement("DiscordPresence", DiscordPresence),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not CemuSettings src) return;

        Fullscreen = src.Fullscreen;
        GraphicApi = src.GraphicApi;
        Vsync = src.Vsync;
        AsyncCompile = src.AsyncCompile;
        TvVolume = src.TvVolume;
        ConsoleLanguage = src.ConsoleLanguage;
        DiscordPresence = src.DiscordPresence;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new CemuSettings());
    }
}

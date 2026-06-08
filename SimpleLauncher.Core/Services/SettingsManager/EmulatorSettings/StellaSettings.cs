using System.Xml.Linq;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

public class StellaSettings : IEmulatorSettings
{
    private const string SectionName = "Stella";

    public bool Fullscreen { get; set; }
    public bool Vsync { get; set; } = true;
    public string VideoDriver { get; set; } = "direct3d";
    public bool CorrectAspect { get; set; } = true;
    public int TvFilter { get; set; }
    public int Scanlines { get; set; }
    public bool AudioEnabled { get; set; } = true;
    public int AudioVolume { get; set; } = 80;
    public bool TimeMachine { get; set; } = true;
    public bool ConfirmExit { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        VideoDriver = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(VideoDriver), "direct3d");
        CorrectAspect = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(CorrectAspect), true);
        TvFilter = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(TvFilter), 0);
        Scanlines = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Scanlines), 0);
        AudioEnabled = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AudioEnabled), true);
        AudioVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(AudioVolume), 80);
        TimeMachine = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(TimeMachine), true);
        ConfirmExit = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ConfirmExit), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("Vsync", Vsync),
            new XElement("VideoDriver", VideoDriver),
            new XElement("CorrectAspect", CorrectAspect),
            new XElement("TvFilter", TvFilter),
            new XElement("Scanlines", Scanlines),
            new XElement("AudioEnabled", AudioEnabled),
            new XElement("AudioVolume", AudioVolume),
            new XElement("TimeMachine", TimeMachine),
            new XElement("ConfirmExit", ConfirmExit),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not StellaSettings src) return;

        Fullscreen = src.Fullscreen;
        Vsync = src.Vsync;
        VideoDriver = src.VideoDriver;
        CorrectAspect = src.CorrectAspect;
        TvFilter = src.TvFilter;
        Scanlines = src.Scanlines;
        AudioEnabled = src.AudioEnabled;
        AudioVolume = src.AudioVolume;
        TimeMachine = src.TimeMachine;
        ConfirmExit = src.ConfirmExit;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new StellaSettings());
    }
}

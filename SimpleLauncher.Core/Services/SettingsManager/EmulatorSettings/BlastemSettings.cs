using System.Xml.Linq;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

public class BlastemSettings : IEmulatorSettings
{
    private const string SectionName = "Blastem";

    public bool Fullscreen { get; set; }
    public bool Vsync { get; set; }
    public string Aspect { get; set; } = "4:3";
    public string Scaling { get; set; } = "linear";
    public bool Scanlines { get; set; }
    public int AudioRate { get; set; } = 48000;
    public string SyncSource { get; set; } = "audio";
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), false);
        Aspect = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Aspect), "4:3");
        Scaling = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Scaling), "linear");
        Scanlines = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Scanlines), false);
        AudioRate = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(AudioRate), 48000);
        SyncSource = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(SyncSource), "audio");
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("Vsync", Vsync),
            new XElement("Aspect", Aspect),
            new XElement("Scaling", Scaling),
            new XElement("Scanlines", Scanlines),
            new XElement("AudioRate", AudioRate),
            new XElement("SyncSource", SyncSource),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not BlastemSettings src) return;

        Fullscreen = src.Fullscreen;
        Vsync = src.Vsync;
        Aspect = src.Aspect;
        Scaling = src.Scaling;
        Scanlines = src.Scanlines;
        AudioRate = src.AudioRate;
        SyncSource = src.SyncSource;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new BlastemSettings());
    }
}

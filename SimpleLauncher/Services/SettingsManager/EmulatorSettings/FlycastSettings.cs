using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

public class FlycastSettings : IEmulatorSettings
{
    private const string SectionName = "Flycast";

    public bool Fullscreen { get; set; }
    public int Width { get; set; } = 640;
    public int Height { get; set; } = 480;
    public bool Maximized { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Width = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Width), 640);
        Height = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Height), 480);
        Maximized = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Maximized), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("Width", Width),
            new XElement("Height", Height),
            new XElement("Maximized", Maximized),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not FlycastSettings src) return;

        Fullscreen = src.Fullscreen;
        Width = src.Width;
        Height = src.Height;
        Maximized = src.Maximized;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new FlycastSettings());
    }
}

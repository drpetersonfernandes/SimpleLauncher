using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

public class DaphneSettings : IEmulatorSettings
{
    private const string SectionName = "Daphne";

    public bool Fullscreen { get; set; }
    public int ResX { get; set; } = 640;
    public int ResY { get; set; } = 480;
    public bool DisableCrosshairs { get; set; }
    public bool Bilinear { get; set; } = true;
    public bool EnableSound { get; set; } = true;
    public bool UseOverlays { get; set; } = true;
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        ResX = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResX), 640);
        ResY = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResY), 480);
        DisableCrosshairs = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DisableCrosshairs), false);
        Bilinear = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Bilinear), true);
        EnableSound = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(EnableSound), true);
        UseOverlays = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(UseOverlays), true);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("ResX", ResX),
            new XElement("ResY", ResY),
            new XElement("DisableCrosshairs", DisableCrosshairs),
            new XElement("Bilinear", Bilinear),
            new XElement("EnableSound", EnableSound),
            new XElement("UseOverlays", UseOverlays),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not DaphneSettings src) return;

        Fullscreen = src.Fullscreen;
        ResX = src.ResX;
        ResY = src.ResY;
        DisableCrosshairs = src.DisableCrosshairs;
        Bilinear = src.Bilinear;
        EnableSound = src.EnableSound;
        UseOverlays = src.UseOverlays;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new DaphneSettings());
    }
}

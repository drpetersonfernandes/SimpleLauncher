using System.Xml.Linq;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

public class SupermodelSettings : IEmulatorSettings
{
    private const string SectionName = "Supermodel";

    public bool New3DEngine { get; set; } = true;
    public bool QuadRendering { get; set; }
    public bool Fullscreen { get; set; } = true;
    public int ResX { get; set; } = 1920;
    public int ResY { get; set; } = 1080;
    public bool WideScreen { get; set; } = true;
    public bool Stretch { get; set; }
    public bool Vsync { get; set; } = true;
    public bool Throttle { get; set; } = true;
    public int MusicVolume { get; set; } = 100;
    public int SoundVolume { get; set; } = 100;
    public string InputSystem { get; set; } = "xinput";
    public bool MultiThreaded { get; set; } = true;
    public int PowerPcFrequency { get; set; } = 50;
    public bool ShowSettingsBeforeLaunch { get; set; }

    private static string ValidateInputSystem(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "xinput";

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "xinput" or "dinput" or "rawinput" ? normalized : "xinput";
    }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        New3DEngine = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(New3DEngine), true);
        QuadRendering = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(QuadRendering), false);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), true);
        ResX = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResX), 1920);
        ResY = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResY), 1080);
        WideScreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(WideScreen), true);
        Stretch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Stretch), false);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        Throttle = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Throttle), true);
        MusicVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(MusicVolume), 100);
        SoundVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(SoundVolume), 100);
        InputSystem = ValidateInputSystem(EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(InputSystem), "xinput"));
        MultiThreaded = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(MultiThreaded), true);
        PowerPcFrequency = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(PowerPcFrequency), 50);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("New3DEngine", New3DEngine),
            new XElement("QuadRendering", QuadRendering),
            new XElement("Fullscreen", Fullscreen),
            new XElement("ResX", ResX),
            new XElement("ResY", ResY),
            new XElement("WideScreen", WideScreen),
            new XElement("Stretch", Stretch),
            new XElement("Vsync", Vsync),
            new XElement("Throttle", Throttle),
            new XElement("MusicVolume", MusicVolume),
            new XElement("SoundVolume", SoundVolume),
            new XElement("InputSystem", InputSystem),
            new XElement("MultiThreaded", MultiThreaded),
            new XElement("PowerPcFrequency", PowerPcFrequency),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not SupermodelSettings src) return;

        New3DEngine = src.New3DEngine;
        QuadRendering = src.QuadRendering;
        Fullscreen = src.Fullscreen;
        ResX = src.ResX;
        ResY = src.ResY;
        WideScreen = src.WideScreen;
        Stretch = src.Stretch;
        Vsync = src.Vsync;
        Throttle = src.Throttle;
        MusicVolume = src.MusicVolume;
        SoundVolume = src.SoundVolume;
        InputSystem = src.InputSystem;
        MultiThreaded = src.MultiThreaded;
        PowerPcFrequency = src.PowerPcFrequency;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new SupermodelSettings());
    }
}

using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

public class AresSettings : IEmulatorSettings
{
    private const string SectionName = "Ares";

    public string VideoDriver { get; set; } = "OpenGL 3.2";
    public bool Exclusive { get; set; }
    public string Shader { get; set; } = "None";
    public int Multiplier { get; set; } = 2;
    public string AspectCorrection { get; set; } = "Standard";
    public bool Mute { get; set; }
    public double Volume { get; set; } = 1.0;
    public bool FastBoot { get; set; }
    public bool Rewind { get; set; }
    public bool RunAhead { get; set; }
    public bool AutoSaveMemory { get; set; } = true;
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        VideoDriver = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(VideoDriver), "OpenGL 3.2");
        Exclusive = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Exclusive), false);
        Shader = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Shader), "None");
        Multiplier = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Multiplier), 2);
        AspectCorrection = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectCorrection), "Standard");
        Mute = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Mute), false);
        Volume = EmulatorXmlHelpers.ReadDouble(s, SectionName, settings, nameof(Volume), 1.0);
        FastBoot = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(FastBoot), false);
        Rewind = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Rewind), false);
        RunAhead = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(RunAhead), false);
        AutoSaveMemory = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AutoSaveMemory), true);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("VideoDriver", VideoDriver),
            new XElement("Exclusive", Exclusive),
            new XElement("Shader", Shader),
            new XElement("Multiplier", Multiplier),
            new XElement("AspectCorrection", AspectCorrection),
            new XElement("Mute", Mute),
            new XElement("Volume", Volume.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            new XElement("FastBoot", FastBoot),
            new XElement("Rewind", Rewind),
            new XElement("RunAhead", RunAhead),
            new XElement("AutoSaveMemory", AutoSaveMemory),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not AresSettings src) return;

        VideoDriver = src.VideoDriver;
        Exclusive = src.Exclusive;
        Shader = src.Shader;
        Multiplier = src.Multiplier;
        AspectCorrection = src.AspectCorrection;
        Mute = src.Mute;
        Volume = src.Volume;
        FastBoot = src.FastBoot;
        Rewind = src.Rewind;
        RunAhead = src.RunAhead;
        AutoSaveMemory = src.AutoSaveMemory;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new AresSettings());
    }
}

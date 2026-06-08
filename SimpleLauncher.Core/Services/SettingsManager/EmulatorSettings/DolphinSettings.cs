using System.Xml.Linq;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

public class DolphinSettings : IEmulatorSettings
{
    private const string SectionName = "Dolphin";

    public string GfxBackend { get; set; } = "Vulkan";
    public bool DspThread { get; set; } = true;
    public bool WiimoteContinuousScanning { get; set; } = true;
    public bool WiimoteEnableSpeaker { get; set; } = true;
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        GfxBackend = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(GfxBackend), "Vulkan");
        DspThread = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DspThread), true);
        WiimoteContinuousScanning = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(WiimoteContinuousScanning), true);
        WiimoteEnableSpeaker = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(WiimoteEnableSpeaker), true);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("GfxBackend", GfxBackend),
            new XElement("DspThread", DspThread),
            new XElement("WiimoteContinuousScanning", WiimoteContinuousScanning),
            new XElement("WiimoteEnableSpeaker", WiimoteEnableSpeaker),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not DolphinSettings src) return;

        GfxBackend = src.GfxBackend;
        DspThread = src.DspThread;
        WiimoteContinuousScanning = src.WiimoteContinuousScanning;
        WiimoteEnableSpeaker = src.WiimoteEnableSpeaker;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new DolphinSettings());
    }
}

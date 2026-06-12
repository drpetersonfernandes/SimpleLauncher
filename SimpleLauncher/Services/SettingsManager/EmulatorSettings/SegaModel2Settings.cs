using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

public class SegaModel2Settings : IEmulatorSettings
{
    private const string SectionName = "SegaModel2";

    public int ResX { get; set; } = 640;
    public int ResY { get; set; } = 480;
    public int WideScreen { get; set; }
    public bool Bilinear { get; set; } = true;
    public bool Trilinear { get; set; }
    public bool FilterTilemaps { get; set; }
    public bool DrawCross { get; set; } = true;
    public int Fsaa { get; set; }
    public bool XInput { get; set; }
    public bool EnableFf { get; set; }
    public bool HoldGears { get; set; }
    public bool UseRawInput { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        ResX = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResX), 640);
        ResY = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResY), 480);
        WideScreen = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(WideScreen), 0);
        Bilinear = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Bilinear), true);
        Trilinear = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Trilinear), false);
        FilterTilemaps = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(FilterTilemaps), false);
        DrawCross = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DrawCross), true);
        Fsaa = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Fsaa), 0);
        XInput = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(XInput), false);
        EnableFf = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(EnableFf), false);
        HoldGears = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(HoldGears), false);
        UseRawInput = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(UseRawInput), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("ResX", ResX),
            new XElement("ResY", ResY),
            new XElement("WideScreen", WideScreen),
            new XElement("Bilinear", Bilinear),
            new XElement("Trilinear", Trilinear),
            new XElement("FilterTilemaps", FilterTilemaps),
            new XElement("DrawCross", DrawCross),
            new XElement("Fsaa", Fsaa),
            new XElement("XInput", XInput),
            new XElement("EnableFf", EnableFf),
            new XElement("HoldGears", HoldGears),
            new XElement("UseRawInput", UseRawInput),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not SegaModel2Settings src) return;

        ResX = src.ResX;
        ResY = src.ResY;
        WideScreen = src.WideScreen;
        Bilinear = src.Bilinear;
        Trilinear = src.Trilinear;
        FilterTilemaps = src.FilterTilemaps;
        DrawCross = src.DrawCross;
        Fsaa = src.Fsaa;
        XInput = src.XInput;
        EnableFf = src.EnableFf;
        HoldGears = src.HoldGears;
        UseRawInput = src.UseRawInput;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new SegaModel2Settings());
    }
}

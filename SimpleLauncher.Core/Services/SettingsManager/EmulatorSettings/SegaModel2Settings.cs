using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
/// Holds emulator configuration settings for Sega Model 2.
/// </summary>
public class SegaModel2Settings : IEmulatorSettings
{
    private const string SectionName = "SegaModel2";

    /// <summary>Horizontal resolution for the emulator display.</summary>
    public int ResX { get; set; } = 640;

    /// <summary>Vertical resolution for the emulator display.</summary>
    public int ResY { get; set; } = 480;

    /// <summary>Widescreen mode setting (0 = off, 1 = on).</summary>
    public int WideScreen { get; set; }

    /// <summary>Whether bilinear texture filtering is enabled.</summary>
    public bool Bilinear { get; set; } = true;

    /// <summary>Whether trilinear texture filtering is enabled.</summary>
    public bool Trilinear { get; set; }

    /// <summary>Whether tilemap filtering is enabled.</summary>
    public bool FilterTilemaps { get; set; }

    /// <summary>Whether to draw a crosshair overlay.</summary>
    public bool DrawCross { get; set; } = true;

    /// <summary>Full-screen anti-aliasing level.</summary>
    public int Fsaa { get; set; }

    /// <summary>Whether XInput controller support is enabled.</summary>
    public bool XInput { get; set; }

    /// <summary>Whether force feedback is enabled.</summary>
    public bool EnableFf { get; set; }

    /// <summary>Whether to hold gears in racing games.</summary>
    public bool HoldGears { get; set; }

    /// <summary>Whether raw input is used for controls.</summary>
    public bool UseRawInput { get; set; }

    /// <summary>Whether to show the emulator settings dialog before launching a game.</summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>Loads settings from the specified XML element.</summary>
    /// <param name="settings">The XML element containing the settings data.</param>
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
        ShowSettingsBeforeLaunch =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>Serializes the current settings to an XML element.</summary>
    /// <returns>An <see cref="XElement"/> containing the settings data.</returns>
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


    /// <summary>Copies settings from another <see cref="IEmulatorSettings"/> instance of the same type.</summary>
    /// <param name="other">The source settings to copy from.</param>
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


    /// <summary>Resets all settings to their default values.</summary>
    public void ResetDefaults()
    {
        CopyFrom(new SegaModel2Settings());
    }
}
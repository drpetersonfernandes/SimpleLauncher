using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Represents the user-configurable settings for the Daphne emulator, persisted to the system configuration under the "Daphne" section.
/// </summary>
public class DaphneSettings : IEmulatorSettings
{
    private const string SectionName = "Daphne";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets the horizontal resolution of the emulation window.
    /// </summary>
    public int ResX { get; set; } = 640;

    /// <summary>
    /// Gets or sets the vertical resolution of the emulation window.
    /// </summary>
    public int ResY { get; set; } = 480;

    /// <summary>
    /// Gets or sets a value indicating whether the light gun crosshairs are disabled.
    /// </summary>
    public bool DisableCrosshairs { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether bilinear filtering is applied to the video output.
    /// </summary>
    public bool Bilinear { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether sound is enabled.
    /// </summary>
    public bool EnableSound { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether laser disc overlays are displayed.
    /// </summary>
    public bool UseOverlays { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the Daphne settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
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


    /// <summary>
    /// Serializes the Daphne settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Daphne settings.</returns>
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


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a Daphne settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
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


    /// <summary>
    /// Resets all Daphne settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new DaphneSettings());
    }
}

using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Represents the user-configurable settings for the Flycast emulator, persisted to the system configuration under the "Flycast" section.
/// </summary>
public class FlycastSettings : IEmulatorSettings
{
    private const string SectionName = "Flycast";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets the width of the emulation window.
    /// </summary>
    public int Width { get; set; } = 640;

    /// <summary>
    /// Gets or sets the height of the emulation window.
    /// </summary>
    public int Height { get; set; } = 480;

    /// <summary>
    /// Gets or sets a value indicating whether the emulation window starts maximized.
    /// </summary>
    public bool Maximized { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the Flycast settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Width = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Width), 640);
        Height = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Height), 480);
        Maximized = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Maximized), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    /// Serializes the Flycast settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Flycast settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("Width", Width),
            new XElement("Height", Height),
            new XElement("Maximized", Maximized),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a Flycast settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not FlycastSettings src) return;

        Fullscreen = src.Fullscreen;
        Width = src.Width;
        Height = src.Height;
        Maximized = src.Maximized;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    /// Resets all Flycast settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new FlycastSettings());
    }
}

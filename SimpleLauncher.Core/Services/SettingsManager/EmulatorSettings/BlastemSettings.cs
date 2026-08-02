using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
/// Represents the user-configurable settings for the Blastem emulator, persisted to the system configuration under the "Blastem" section.
/// </summary>
public class BlastemSettings : IEmulatorSettings
{
    private const string SectionName = "Blastem";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; }

    /// <summary>
    /// Gets or sets the aspect ratio of the emulation window (e.g., "4:3").
    /// </summary>
    public string Aspect { get; set; } = "4:3";

    /// <summary>
    /// Gets or sets the video scaling method used by the emulator (e.g., "linear").
    /// </summary>
    public string Scaling { get; set; } = "linear";

    /// <summary>
    /// Gets or sets a value indicating whether scanline effects are applied.
    /// </summary>
    public bool Scanlines { get; set; }

    /// <summary>
    /// Gets or sets the audio sample rate in Hz.
    /// </summary>
    public int AudioRate { get; set; } = 48000;

    /// <summary>
    /// Gets or sets the synchronization source used by the emulator (e.g., "audio").
    /// </summary>
    public string SyncSource { get; set; } = "audio";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the Blastem settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
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


    /// <summary>
    /// Serializes the Blastem settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Blastem settings.</returns>
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


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a Blastem settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
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


    /// <summary>
    /// Resets all Blastem settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new BlastemSettings());
    }
}

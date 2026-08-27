using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
/// Represents the user-configurable settings for the Dolphin emulator, persisted to the system configuration under the "Dolphin" section.
/// </summary>
public class DolphinSettings : IEmulatorSettings
{
    private const string SectionName = "Dolphin";

    /// <summary>
    /// Gets or sets the graphics backend used by the emulator (e.g., "Vulkan").
    /// </summary>
    public string GfxBackend { get; set; } = "Vulkan";

    /// <summary>
    /// Gets or sets a value indicating whether audio is processed on a dedicated DSP thread.
    /// </summary>
    public bool DspThread { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether Wiimotes are continuously scanned for connections.
    /// </summary>
    public bool WiimoteContinuousScanning { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the Wiimote speaker is enabled.
    /// </summary>
    public bool WiimoteEnableSpeaker { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the Dolphin settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        GfxBackend = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(GfxBackend), "Vulkan");
        DspThread = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DspThread), true);
        WiimoteContinuousScanning =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(WiimoteContinuousScanning), true);
        WiimoteEnableSpeaker =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(WiimoteEnableSpeaker), true);
        ShowSettingsBeforeLaunch =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    /// Serializes the Dolphin settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Dolphin settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("GfxBackend", GfxBackend),
            new XElement("DspThread", DspThread),
            new XElement("WiimoteContinuousScanning", WiimoteContinuousScanning),
            new XElement("WiimoteEnableSpeaker", WiimoteEnableSpeaker),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a Dolphin settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not DolphinSettings src) return;

        GfxBackend = src.GfxBackend;
        DspThread = src.DspThread;
        WiimoteContinuousScanning = src.WiimoteContinuousScanning;
        WiimoteEnableSpeaker = src.WiimoteEnableSpeaker;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    /// Resets all Dolphin settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new DolphinSettings());
    }
}
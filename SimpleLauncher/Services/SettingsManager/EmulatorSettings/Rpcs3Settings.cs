using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Represents the user-configurable settings for the RPCS3 emulator, persisted to the system configuration under the "Rpcs3" section.
/// </summary>
public class Rpcs3Settings : IEmulatorSettings
{
    private const string SectionName = "Rpcs3";

    /// <summary>
    /// Gets or sets the graphics renderer used by RPCS3 (e.g., "Vulkan").
    /// </summary>
    public string Renderer { get; set; } = "Vulkan";
    /// <summary>
    /// Gets or sets the emulated screen resolution (e.g., "1280x720").
    /// </summary>
    public string Resolution { get; set; } = "1280x720";
    /// <summary>
    /// Gets or sets the aspect ratio of the emulation window (e.g., "16:9").
    /// </summary>
    public string AspectRatio { get; set; } = "16:9";
    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; }
    /// <summary>
    /// Gets or sets the internal resolution scale percentage.
    /// </summary>
    public int ResolutionScale { get; set; } = 100;
    /// <summary>
    /// Gets or sets the anisotropic filter override level.
    /// </summary>
    public int AnisotropicFilter { get; set; }
    /// <summary>
    /// Gets or sets the PPU decoder used by RPCS3 (e.g., "Recompiler (LLVM)").
    /// </summary>
    public string PpuDecoder { get; set; } = "Recompiler (LLVM)";
    /// <summary>
    /// Gets or sets the SPU decoder used by RPCS3 (e.g., "Recompiler (LLVM)").
    /// </summary>
    public string SpuDecoder { get; set; } = "Recompiler (LLVM)";
    /// <summary>
    /// Gets or sets the audio renderer used by RPCS3 (e.g., "Cubeb").
    /// </summary>
    public string AudioRenderer { get; set; } = "Cubeb";
    /// <summary>
    /// Gets or sets a value indicating whether audio buffering is enabled.
    /// </summary>
    public bool AudioBuffering { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether games start in fullscreen mode.
    /// </summary>
    public bool StartFullscreen { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }

    /// <summary>
    /// Loads the RPCS3 settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Renderer = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Renderer), "Vulkan");
        Resolution = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Resolution), "1280x720");
        AspectRatio = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatio), "16:9");
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), false);
        ResolutionScale = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResolutionScale), 100);
        AnisotropicFilter = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(AnisotropicFilter), 0);
        PpuDecoder = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(PpuDecoder), "Recompiler (LLVM)");
        SpuDecoder = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(SpuDecoder), "Recompiler (LLVM)");
        AudioRenderer = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AudioRenderer), "Cubeb");
        AudioBuffering = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AudioBuffering), true);
        StartFullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(StartFullscreen), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    /// <summary>
    /// Serializes the RPCS3 settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the RPCS3 settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Renderer", Renderer),
            new XElement("Resolution", Resolution),
            new XElement("AspectRatio", AspectRatio),
            new XElement("Vsync", Vsync),
            new XElement("ResolutionScale", ResolutionScale),
            new XElement("AnisotropicFilter", AnisotropicFilter),
            new XElement("PpuDecoder", PpuDecoder),
            new XElement("SpuDecoder", SpuDecoder),
            new XElement("AudioRenderer", AudioRenderer),
            new XElement("AudioBuffering", AudioBuffering),
            new XElement("StartFullscreen", StartFullscreen),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    /// <summary>
    /// Copies the values from another emulator settings instance if it is an RPCS3 settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not Rpcs3Settings src) return;

        Renderer = src.Renderer;
        Resolution = src.Resolution;
        AspectRatio = src.AspectRatio;
        Vsync = src.Vsync;
        ResolutionScale = src.ResolutionScale;
        AnisotropicFilter = src.AnisotropicFilter;
        PpuDecoder = src.PpuDecoder;
        SpuDecoder = src.SpuDecoder;
        AudioRenderer = src.AudioRenderer;
        AudioBuffering = src.AudioBuffering;
        StartFullscreen = src.StartFullscreen;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    /// <summary>
    /// Resets all RPCS3 settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new Rpcs3Settings());
    }
}

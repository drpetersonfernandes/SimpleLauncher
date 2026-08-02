using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Represents the user-configurable settings for the PCSX2 emulator, persisted to the system configuration under the "Pcsx2" section.
/// </summary>
public class Pcsx2Settings : IEmulatorSettings
{
    private const string SectionName = "Pcsx2";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool StartFullscreen { get; set; } = true;

    /// <summary>
    /// Gets or sets the aspect ratio of the emulation window (e.g., "16:9").
    /// </summary>
    public string AspectRatio { get; set; } = "16:9";

    /// <summary>
    /// Gets or sets the graphics renderer used by the emulator (e.g., 14 for Vulkan).
    /// </summary>
    public int Renderer { get; set; } = 14;

    /// <summary>
    /// Gets or sets the internal resolution upscaling multiplier applied during emulation.
    /// </summary>
    public int UpscaleMultiplier { get; set; } = 2;

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether cheat support is enabled.
    /// </summary>
    public bool EnableCheats { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether widescreen patches are enabled.
    /// </summary>
    public bool EnableWidescreenPatches { get; set; }

    /// <summary>
    /// Gets or sets the audio volume percentage.
    /// </summary>
    public int Volume { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether RetroAchievements integration is enabled.
    /// </summary>
    public bool AchievementsEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether RetroAchievements hardcore mode is enabled.
    /// </summary>
    public bool AchievementsHardcore { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the PCSX2 settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        StartFullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(StartFullscreen), true);
        AspectRatio = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatio), "16:9");
        Renderer = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Renderer), 14);
        UpscaleMultiplier = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(UpscaleMultiplier), 2);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), false);
        EnableCheats = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(EnableCheats), false);
        EnableWidescreenPatches = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(EnableWidescreenPatches), false);
        Volume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Volume), 100);
        AchievementsEnabled = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AchievementsEnabled), false);
        AchievementsHardcore = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AchievementsHardcore), true);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    /// Serializes the PCSX2 settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the PCSX2 settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("StartFullscreen", StartFullscreen),
            new XElement("AspectRatio", AspectRatio),
            new XElement("Renderer", Renderer),
            new XElement("UpscaleMultiplier", UpscaleMultiplier),
            new XElement("Vsync", Vsync),
            new XElement("EnableCheats", EnableCheats),
            new XElement("EnableWidescreenPatches", EnableWidescreenPatches),
            new XElement("Volume", Volume),
            new XElement("AchievementsEnabled", AchievementsEnabled),
            new XElement("AchievementsHardcore", AchievementsHardcore),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a PCSX2 settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not Pcsx2Settings src) return;

        StartFullscreen = src.StartFullscreen;
        AspectRatio = src.AspectRatio;
        Renderer = src.Renderer;
        UpscaleMultiplier = src.UpscaleMultiplier;
        Vsync = src.Vsync;
        EnableCheats = src.EnableCheats;
        EnableWidescreenPatches = src.EnableWidescreenPatches;
        Volume = src.Volume;
        AchievementsEnabled = src.AchievementsEnabled;
        AchievementsHardcore = src.AchievementsHardcore;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    /// Resets all PCSX2 settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new Pcsx2Settings());
    }
}

using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
/// Represents the user-configurable settings for the RetroArch emulator, persisted to the system configuration under the "RetroArch" section.
/// </summary>
public class RetroArchSettings : IEmulatorSettings
{
    private const string SectionName = "RetroArch";

    /// <summary>
    /// Gets or sets a value indicating whether RetroAchievements support is enabled.
    /// </summary>
    public bool CheevosEnable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether RetroAchievements hardcore mode is enabled.
    /// </summary>
    public bool CheevosHardcore { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; } = true;

    /// <summary>
    /// Gets or sets the video driver used by RetroArch (e.g., "gl").
    /// </summary>
    public string VideoDriver { get; set; } = "gl";

    /// <summary>
    /// Gets or sets a value indicating whether audio output is enabled.
    /// </summary>
    public bool AudioEnable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether audio output is muted.
    /// </summary>
    public bool AudioMute { get; set; }

    /// <summary>
    /// Gets or sets the menu driver used by RetroArch (e.g., "ozone").
    /// </summary>
    public string MenuDriver { get; set; } = "ozone";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator pauses when the window loses focus.
    /// </summary>
    public bool PauseNonActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the configuration is saved on exit.
    /// </summary>
    public bool SaveOnExit { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether save states are automatically saved when a game closes.
    /// </summary>
    public bool AutoSaveState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether save states are automatically loaded when a game starts.
    /// </summary>
    public bool AutoLoadState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether rewind support is enabled.
    /// </summary>
    public bool Rewind { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether video runs on a separate thread.
    /// </summary>
    public bool ThreadedVideo { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether bilinear filtering is applied to the video output.
    /// </summary>
    public bool Bilinear { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }

    /// <summary>
    /// Gets or sets the aspect ratio index used by the emulator (e.g., "22" for 4:3).
    /// </summary>
    public string AspectRatioIndex { get; set; } = "22";

    /// <summary>
    /// Gets or sets a value indicating whether integer scaling is enabled.
    /// </summary>
    public bool ScaleInteger { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether video shaders are enabled.
    /// </summary>
    public bool ShaderEnable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether hard GPU synchronization is enabled.
    /// </summary>
    public bool HardSync { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether run-ahead (input latency reduction) is enabled.
    /// </summary>
    public bool RunAhead { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether advanced settings are shown in the RetroArch menu.
    /// </summary>
    public bool ShowAdvancedSettings { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether Discord rich presence is enabled.
    /// </summary>
    public bool DiscordAllow { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the system directory is overridden.
    /// </summary>
    public bool OverrideSystemDir { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the save directory is overridden.
    /// </summary>
    public bool OverrideSaveDir { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the state directory is overridden.
    /// </summary>
    public bool OverrideStateDir { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the screenshot directory is overridden.
    /// </summary>
    public bool OverrideScreenshotDir { get; set; }


    /// <summary>
    /// Loads the RetroArch settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        CheevosEnable = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(CheevosEnable), false);
        CheevosHardcore = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(CheevosHardcore), false);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        VideoDriver = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(VideoDriver), "gl");
        AudioEnable = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AudioEnable), true);
        AudioMute = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AudioMute), false);
        MenuDriver = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(MenuDriver), "ozone");
        PauseNonActive = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(PauseNonActive), true);
        SaveOnExit = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(SaveOnExit), true);
        AutoSaveState = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AutoSaveState), false);
        AutoLoadState = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AutoLoadState), false);
        Rewind = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Rewind), false);
        ThreadedVideo = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ThreadedVideo), false);
        Bilinear = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Bilinear), false);
        ShowSettingsBeforeLaunch =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
        AspectRatioIndex = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatioIndex), "22");
        ScaleInteger = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ScaleInteger), false);
        ShaderEnable = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShaderEnable), true);
        HardSync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(HardSync), false);
        RunAhead = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(RunAhead), false);
        ShowAdvancedSettings =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowAdvancedSettings), true);
        DiscordAllow = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DiscordAllow), false);
        OverrideSystemDir = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OverrideSystemDir), false);
        OverrideSaveDir = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OverrideSaveDir), false);
        OverrideStateDir = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OverrideStateDir), false);
        OverrideScreenshotDir =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OverrideScreenshotDir), false);
    }


    /// <summary>
    /// Serializes the RetroArch settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the RetroArch settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("CheevosEnable", CheevosEnable),
            new XElement("CheevosHardcore", CheevosHardcore),
            new XElement("Fullscreen", Fullscreen),
            new XElement("Vsync", Vsync),
            new XElement("VideoDriver", VideoDriver),
            new XElement("AudioEnable", AudioEnable),
            new XElement("AudioMute", AudioMute),
            new XElement("MenuDriver", MenuDriver),
            new XElement("PauseNonActive", PauseNonActive),
            new XElement("SaveOnExit", SaveOnExit),
            new XElement("AutoSaveState", AutoSaveState),
            new XElement("AutoLoadState", AutoLoadState),
            new XElement("Rewind", Rewind),
            new XElement("ThreadedVideo", ThreadedVideo),
            new XElement("Bilinear", Bilinear),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch),
            new XElement("AspectRatioIndex", AspectRatioIndex),
            new XElement("ScaleInteger", ScaleInteger),
            new XElement("ShaderEnable", ShaderEnable),
            new XElement("HardSync", HardSync),
            new XElement("RunAhead", RunAhead),
            new XElement("ShowAdvancedSettings", ShowAdvancedSettings),
            new XElement("DiscordAllow", DiscordAllow),
            new XElement("OverrideSystemDir", OverrideSystemDir),
            new XElement("OverrideSaveDir", OverrideSaveDir),
            new XElement("OverrideStateDir", OverrideStateDir),
            new XElement("OverrideScreenshotDir", OverrideScreenshotDir));
    }

    /// <summary>
    /// Copies the values from another emulator settings instance if it is a RetroArch settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not RetroArchSettings src) return;

        CheevosEnable = src.CheevosEnable;
        CheevosHardcore = src.CheevosHardcore;
        Fullscreen = src.Fullscreen;
        Vsync = src.Vsync;
        VideoDriver = src.VideoDriver;
        AudioEnable = src.AudioEnable;
        AudioMute = src.AudioMute;
        MenuDriver = src.MenuDriver;
        PauseNonActive = src.PauseNonActive;
        SaveOnExit = src.SaveOnExit;
        AutoSaveState = src.AutoSaveState;
        AutoLoadState = src.AutoLoadState;
        Rewind = src.Rewind;
        ThreadedVideo = src.ThreadedVideo;
        Bilinear = src.Bilinear;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
        AspectRatioIndex = src.AspectRatioIndex;
        ScaleInteger = src.ScaleInteger;
        ShaderEnable = src.ShaderEnable;
        HardSync = src.HardSync;
        RunAhead = src.RunAhead;
        ShowAdvancedSettings = src.ShowAdvancedSettings;
        DiscordAllow = src.DiscordAllow;
        OverrideSystemDir = src.OverrideSystemDir;
        OverrideSaveDir = src.OverrideSaveDir;
        OverrideStateDir = src.OverrideStateDir;
        OverrideScreenshotDir = src.OverrideScreenshotDir;
    }


    /// <summary>
    /// Resets all RetroArch settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new RetroArchSettings());
    }
}
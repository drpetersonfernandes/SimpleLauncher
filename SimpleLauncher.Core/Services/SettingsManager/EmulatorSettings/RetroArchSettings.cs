using System.Xml.Linq;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

public class RetroArchSettings : IEmulatorSettings
{
    private const string SectionName = "RetroArch";

    public bool CheevosEnable { get; set; }
    public bool CheevosHardcore { get; set; }
    public bool Fullscreen { get; set; }
    public bool Vsync { get; set; } = true;
    public string VideoDriver { get; set; } = "gl";
    public bool AudioEnable { get; set; } = true;
    public bool AudioMute { get; set; }
    public string MenuDriver { get; set; } = "ozone";
    public bool PauseNonActive { get; set; } = true;
    public bool SaveOnExit { get; set; } = true;
    public bool AutoSaveState { get; set; }
    public bool AutoLoadState { get; set; }
    public bool Rewind { get; set; }
    public bool ThreadedVideo { get; set; }
    public bool Bilinear { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }
    public string AspectRatioIndex { get; set; } = "22";
    public bool ScaleInteger { get; set; }
    public bool ShaderEnable { get; set; } = true;
    public bool HardSync { get; set; }
    public bool RunAhead { get; set; }
    public bool ShowAdvancedSettings { get; set; } = true;
    public bool DiscordAllow { get; set; }
    public bool OverrideSystemDir { get; set; }
    public bool OverrideSaveDir { get; set; }
    public bool OverrideStateDir { get; set; }
    public bool OverrideScreenshotDir { get; set; }

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
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
        AspectRatioIndex = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatioIndex), "22");
        ScaleInteger = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ScaleInteger), false);
        ShaderEnable = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShaderEnable), true);
        HardSync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(HardSync), false);
        RunAhead = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(RunAhead), false);
        ShowAdvancedSettings = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowAdvancedSettings), true);
        DiscordAllow = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DiscordAllow), false);
        OverrideSystemDir = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OverrideSystemDir), false);
        OverrideSaveDir = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OverrideSaveDir), false);
        OverrideStateDir = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OverrideStateDir), false);
        OverrideScreenshotDir = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OverrideScreenshotDir), false);
    }

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

    public void ResetDefaults()
    {
        CopyFrom(new RetroArchSettings());
    }
}

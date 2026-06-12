using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Configuration settings for the Xenia (Xbox 360) emulator, including GPU, audio, scaling, and input options.
/// </summary>
public class XeniaSettings : IEmulatorSettings
{
    private const string SectionName = "Xenia";

    /// <summary>Gets or sets the readback resolve mode (e.g., "none").</summary>
    public string ReadbackResolve { get; set; } = "none";

    /// <summary>Gets or sets whether sRGB gamma correction is enabled.</summary>
    public bool GammaSrgb { get; set; }

    /// <summary>Gets or sets whether controller vibration is enabled.</summary>
    public bool Vibration { get; set; } = true;

    /// <summary>Gets or sets whether cache mounting is enabled.</summary>
    public bool MountCache { get; set; } = true;

    /// <summary>Gets or sets the GPU backend (e.g., "d3d12").</summary>
    public string Gpu { get; set; } = "d3d12";

    /// <summary>Gets or sets whether vertical sync is enabled.</summary>
    public bool Vsync { get; set; } = true;

    /// <summary>Gets or sets the horizontal resolution scale factor.</summary>
    public int ResScaleX { get; set; } = 1;

    /// <summary>Gets or sets the vertical resolution scale factor.</summary>
    public int ResScaleY { get; set; } = 1;

    /// <summary>Gets or sets whether fullscreen mode is enabled.</summary>
    public bool Fullscreen { get; set; }

    /// <summary>Gets or sets the audio processing unit backend (e.g., "xaudio2").</summary>
    public string Apu { get; set; } = "xaudio2";

    /// <summary>Gets or sets whether audio output is muted.</summary>
    public bool Mute { get; set; }

    /// <summary>Gets or sets the anti-aliasing mode.</summary>
    public string Aa { get; set; } = "";

    /// <summary>Gets or sets the resolution scaling method (e.g., "fsr").</summary>
    public string Scaling { get; set; } = "fsr";

    /// <summary>Gets or sets whether game patches are applied.</summary>
    public bool ApplyPatches { get; set; } = true;

    /// <summary>Gets or sets whether Discord Rich Presence integration is enabled.</summary>
    public bool DiscordPresence { get; set; } = true;

    /// <summary>Gets or sets the user language index.</summary>
    public int UserLanguage { get; set; } = 1;

    /// <summary>Gets or sets the HID (human interface device) input backend (e.g., "xinput").</summary>
    public string Hid { get; set; } = "xinput";

    /// <summary>Gets or sets whether the emulator settings dialog is shown before launching a game.</summary>
    public bool ShowSettingsBeforeLaunch { get; set; }

    /// <summary>Loads emulator settings from the provided XML configuration element.</summary>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        ReadbackResolve = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(ReadbackResolve), "none");
        GammaSrgb = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(GammaSrgb), false);
        Vibration = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vibration), true);
        MountCache = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(MountCache), true);
        Gpu = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Gpu), "d3d12");
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), true);
        ResScaleX = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResScaleX), 1);
        ResScaleY = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResScaleY), 1);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        Apu = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Apu), "xaudio2");
        Mute = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Mute), false);
        Aa = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Aa), "");
        Scaling = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Scaling), "fsr");
        ApplyPatches = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ApplyPatches), true);
        DiscordPresence = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DiscordPresence), true);
        UserLanguage = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(UserLanguage), 1);
        Hid = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Hid), "xinput");
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    /// <summary>Serializes the current settings to an XML element.</summary>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("ReadbackResolve", ReadbackResolve),
            new XElement("GammaSrgb", GammaSrgb),
            new XElement("Vibration", Vibration),
            new XElement("MountCache", MountCache),
            new XElement("Gpu", Gpu),
            new XElement("Vsync", Vsync),
            new XElement("ResScaleX", ResScaleX),
            new XElement("ResScaleY", ResScaleY),
            new XElement("Fullscreen", Fullscreen),
            new XElement("Apu", Apu),
            new XElement("Mute", Mute),
            new XElement("Aa", Aa),
            new XElement("Scaling", Scaling),
            new XElement("ApplyPatches", ApplyPatches),
            new XElement("DiscordPresence", DiscordPresence),
            new XElement("UserLanguage", UserLanguage),
            new XElement("Hid", Hid),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    /// <summary>Copies all settings from another XeniaSettings instance.</summary>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not XeniaSettings src) return;

        ReadbackResolve = src.ReadbackResolve;
        GammaSrgb = src.GammaSrgb;
        Vibration = src.Vibration;
        MountCache = src.MountCache;
        Gpu = src.Gpu;
        Vsync = src.Vsync;
        ResScaleX = src.ResScaleX;
        ResScaleY = src.ResScaleY;
        Fullscreen = src.Fullscreen;
        Apu = src.Apu;
        Mute = src.Mute;
        Aa = src.Aa;
        Scaling = src.Scaling;
        ApplyPatches = src.ApplyPatches;
        DiscordPresence = src.DiscordPresence;
        UserLanguage = src.UserLanguage;
        Hid = src.Hid;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    /// <summary>Resets all settings to their default values.</summary>
    public void ResetDefaults()
    {
        CopyFrom(new XeniaSettings());
    }
}

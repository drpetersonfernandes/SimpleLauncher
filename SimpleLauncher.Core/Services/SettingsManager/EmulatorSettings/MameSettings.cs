using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
///     Represents the user-configurable settings for the MAME emulator, persisted to the system configuration under the
///     "Mame" section.
/// </summary>
public class MameSettings : IEmulatorSettings
{
    private const string SectionName = "Mame";

    /// <summary>
    ///     Gets or sets the video output mode used by MAME (e.g., "auto").
    /// </summary>
    public string Video { get; set; } = "auto";

    /// <summary>
    ///     Gets or sets a value indicating whether MAME runs in a window instead of fullscreen.
    /// </summary>
    public bool Window { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the emulation window starts maximized.
    /// </summary>
    public bool Maximize { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the original game aspect ratio is preserved.
    /// </summary>
    public bool KeepAspect { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the game information screen is skipped on startup.
    /// </summary>
    public bool SkipGameInfo { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether NVRAM is automatically saved when exiting.
    /// </summary>
    public bool Autosave { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether MAME asks for confirmation before quitting.
    /// </summary>
    public bool ConfirmQuit { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether joystick input is enabled.
    /// </summary>
    public bool Joystick { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether automatic frame skipping is enabled to maintain speed.
    /// </summary>
    public bool Autoframeskip { get; set; }

    /// <summary>
    ///     Gets or sets the BGFX video backend used by MAME (e.g., "auto").
    /// </summary>
    public string BgfxBackend { get; set; } = "auto";

    /// <summary>
    ///     Gets or sets the BGFX screen chain effect applied to the video output.
    /// </summary>
    public string BgfxScreenChains { get; set; } = "default";

    /// <summary>
    ///     Gets or sets a value indicating whether texture filtering is enabled.
    /// </summary>
    public bool Filter { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether cheat support is enabled.
    /// </summary>
    public bool Cheat { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether rewind support is enabled.
    /// </summary>
    public bool Rewind { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether NVRAM data is saved on exit.
    /// </summary>
    public bool NvramSave { get; set; } = true;

    /// <summary>
    ///     Loads the MAME settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Video = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Video), "auto");
        Window = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Window), false);
        Maximize = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Maximize), true);
        KeepAspect = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(KeepAspect), true);
        SkipGameInfo = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(SkipGameInfo), true);
        Autosave = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Autosave), false);
        ConfirmQuit = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ConfirmQuit), false);
        Joystick = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Joystick), true);
        ShowSettingsBeforeLaunch =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
        Autoframeskip = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Autoframeskip), false);
        BgfxBackend = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(BgfxBackend), "auto");
        BgfxScreenChains = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(BgfxScreenChains), "default");
        Filter = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Filter), true);
        Cheat = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Cheat), false);
        Rewind = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Rewind), false);
        NvramSave = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(NvramSave), true);
    }


    /// <summary>
    ///     Serializes the MAME settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the MAME settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Video", Video),
            new XElement("Window", Window),
            new XElement("Maximize", Maximize),
            new XElement("KeepAspect", KeepAspect),
            new XElement("SkipGameInfo", SkipGameInfo),
            new XElement("Autosave", Autosave),
            new XElement("ConfirmQuit", ConfirmQuit),
            new XElement("Joystick", Joystick),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch),
            new XElement("Autoframeskip", Autoframeskip),
            new XElement("BgfxBackend", BgfxBackend),
            new XElement("BgfxScreenChains", BgfxScreenChains),
            new XElement("Filter", Filter),
            new XElement("Cheat", Cheat),
            new XElement("Rewind", Rewind),
            new XElement("NvramSave", NvramSave));
    }


    /// <summary>
    ///     Copies the values from another emulator settings instance if it is a MAME settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not MameSettings src) return;

        Video = src.Video;
        Window = src.Window;
        Maximize = src.Maximize;
        KeepAspect = src.KeepAspect;
        SkipGameInfo = src.SkipGameInfo;
        Autosave = src.Autosave;
        ConfirmQuit = src.ConfirmQuit;
        Joystick = src.Joystick;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
        Autoframeskip = src.Autoframeskip;
        BgfxBackend = src.BgfxBackend;
        BgfxScreenChains = src.BgfxScreenChains;
        Filter = src.Filter;
        Cheat = src.Cheat;
        Rewind = src.Rewind;
        NvramSave = src.NvramSave;
    }


    /// <summary>
    ///     Resets all MAME settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new MameSettings());
    }
}
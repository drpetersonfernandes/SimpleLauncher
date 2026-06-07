using System.Xml.Linq;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

public class MameSettings : IEmulatorSettings
{
    private const string SectionName = "Mame";

    public string Video { get; set; } = "auto";
    public bool Window { get; set; }
    public bool Maximize { get; set; } = true;
    public bool KeepAspect { get; set; } = true;
    public bool SkipGameInfo { get; set; } = true;
    public bool Autosave { get; set; }
    public bool ConfirmQuit { get; set; }
    public bool Joystick { get; set; } = true;
    public bool ShowSettingsBeforeLaunch { get; set; }
    public bool Autoframeskip { get; set; }
    public string BgfxBackend { get; set; } = "auto";
    public string BgfxScreenChains { get; set; } = "default";
    public bool Filter { get; set; } = true;
    public bool Cheat { get; set; }
    public bool Rewind { get; set; }
    public bool NvramSave { get; set; } = true;

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
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
        Autoframeskip = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Autoframeskip), false);
        BgfxBackend = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(BgfxBackend), "auto");
        BgfxScreenChains = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(BgfxScreenChains), "default");
        Filter = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Filter), true);
        Cheat = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Cheat), false);
        Rewind = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Rewind), false);
        NvramSave = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(NvramSave), true);
    }

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

    public void ResetDefaults()
    {
        CopyFrom(new MameSettings());
    }
}

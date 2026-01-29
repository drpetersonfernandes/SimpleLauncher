using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Managers;

[MessagePackObject(AllowPrivate = true)]
public class SettingsManager
{
    [IgnoreMember] private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultSettingsFilePath);
    [IgnoreMember] private readonly string _xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OldSettingsFilePath);

    [IgnoreMember] private readonly object _saveLock = new();

    [IgnoreMember] private readonly HashSet<int> _validThumbnailSizes = [50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800];
    [IgnoreMember] private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500, 1000, 10000, 1000000];

    [IgnoreMember] private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
    [IgnoreMember] private readonly HashSet<string> _validViewModes = ["GridView", "ListView"];

    [IgnoreMember] private readonly HashSet<string> _validButtonAspectRatio = ["Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2"];

    [Key(0)] public int ThumbnailSize { get; set; }
    [Key(1)] public int GamesPerPage { get; set; }
    [Key(2)] public string ShowGames { get; set; }
    [Key(3)] public string ViewMode { get; set; }
    [Key(4)] public bool EnableGamePadNavigation { get; set; }
    [Key(5)] public string VideoUrl { get; set; }
    [Key(6)] public string InfoUrl { get; set; }
    [Key(7)] public string BaseTheme { get; set; }
    [Key(8)] public string AccentColor { get; set; }
    [Key(9)] public string Language { get; set; }
    [Key(10)] public float DeadZoneX { get; set; }
    [Key(11)] public float DeadZoneY { get; set; }
    [Key(12)] public string ButtonAspectRatio { get; set; }
    [Key(13)] public bool EnableFuzzyMatching { get; set; }
    [Key(14)] public double FuzzyMatchingThreshold { get; set; }
    [IgnoreMember] public const float DefaultDeadZoneX = 0.05f;
    [IgnoreMember] public const float DefaultDeadZoneY = 0.02f;
    [Key(15)] public bool EnableNotificationSound { get; set; }
    [Key(16)] public string CustomNotificationSoundFile { get; set; }
    [Key(17)] public string RaUsername { get; set; }
    [Key(18)] public string RaApiKey { get; set; }
    [Key(19)] public string RaPassword { get; set; }
    [Key(30)] public string RaToken { get; set; }
    [Key(20)] public bool OverlayRetroAchievementButton { get; set; }
    [Key(21)] public bool OverlayOpenVideoButton { get; set; }
    [Key(22)] public bool OverlayOpenInfoButton { get; set; }
    [Key(23)] public bool AdditionalSystemFoldersExpanded { get; set; }
    [Key(24)] public bool Emulator1Expanded { get; set; }
    [Key(25)] public bool Emulator2Expanded { get; set; }
    [Key(26)] public bool Emulator3Expanded { get; set; }
    [Key(27)] public bool Emulator4Expanded { get; set; }
    [Key(28)] public bool Emulator5Expanded { get; set; }
    [Key(29)] public List<SystemPlayTime> SystemPlayTimes { get; set; } = [];

    // --- Xenia Global Configuration ---
    [Key(31)] public string XeniaGpu { get; set; } = "d3d12"; // d3d12, vulkan, null
    [Key(32)] public bool XeniaVsync { get; set; } = true;
    [Key(33)] public int XeniaResScaleX { get; set; } = 1;
    [Key(34)] public int XeniaResScaleY { get; set; } = 1;
    [Key(35)] public bool XeniaFullscreen { get; set; }
    [Key(36)] public string XeniaApu { get; set; } = "xaudio2"; // xaudio2, sdl, nop, any
    [Key(37)] public bool XeniaMute { get; set; }
    [Key(38)] public string XeniaAa { get; set; } = ""; // "", fxaa, fxaa_extreme
    [Key(39)] public string XeniaScaling { get; set; } = "fsr"; // fsr, cas, bilinear
    [Key(40)] public bool XeniaApplyPatches { get; set; } = true;
    [Key(41)] public bool XeniaDiscordPresence { get; set; } = true;
    [Key(42)] public int XeniaUserLanguage { get; set; } = 1; // 1=English
    [Key(43)] public string XeniaHid { get; set; } = "xinput"; // xinput, sdl, winkey, any
    [Key(44)] public bool XeniaShowSettingsBeforeLaunch { get; set; } = true;

    // --- Xenia Advanced Configuration ---
    [Key(84)] public string XeniaReadbackResolve { get; set; } = "none"; // none, fast, full
    [Key(85)] public bool XeniaGammaSrgb { get; set; }
    [Key(86)] public bool XeniaVibration { get; set; } = true;
    [Key(87)] public bool XeniaMountCache { get; set; } = true;

    // --- MAME Global Configuration ---
    [Key(45)] public string MameVideo { get; set; } = "auto"; // auto, d3d, opengl, bgfx
    [Key(46)] public bool MameWindow { get; set; }
    [Key(47)] public bool MameMaximize { get; set; } = true;
    [Key(48)] public bool MameKeepAspect { get; set; } = true;
    [Key(49)] public bool MameSkipGameInfo { get; set; } = true;
    [Key(50)] public bool MameAutosave { get; set; }
    [Key(51)] public bool MameConfirmQuit { get; set; }
    [Key(52)] public bool MameJoystick { get; set; } = true;
    [Key(53)] public bool MameShowSettingsBeforeLaunch { get; set; } = true;
    [Key(70)] public bool MameAutoframeskip { get; set; }
    [Key(71)] public string MameBgfxBackend { get; set; } = "auto";
    [Key(72)] public string MameBgfxScreenChains { get; set; } = "default";
    [Key(73)] public bool MameFilter { get; set; } = true;
    [Key(74)] public bool MameCheat { get; set; }
    [Key(75)] public bool MameRewind { get; set; }
    [Key(76)] public bool MameNvramSave { get; set; } = true;

    // --- RetroArch Global Configuration ---
    [Key(54)] public bool RetroArchCheevosEnable { get; set; }
    [Key(55)] public bool RetroArchCheevosHardcore { get; set; }
    [Key(56)] public bool RetroArchFullscreen { get; set; }
    [Key(57)] public bool RetroArchVsync { get; set; } = true;
    [Key(58)] public string RetroArchVideoDriver { get; set; } = "gl";
    [Key(59)] public bool RetroArchAudioEnable { get; set; } = true;
    [Key(60)] public bool RetroArchAudioMute { get; set; }
    [Key(61)] public string RetroArchMenuDriver { get; set; } = "ozone";
    [Key(62)] public bool RetroArchPauseNonActive { get; set; } = true;
    [Key(63)] public bool RetroArchSaveOnExit { get; set; } = true;
    [Key(64)] public bool RetroArchAutoSaveState { get; set; }
    [Key(65)] public bool RetroArchAutoLoadState { get; set; }
    [Key(66)] public bool RetroArchRewind { get; set; }
    [Key(67)] public bool RetroArchThreadedVideo { get; set; }
    [Key(68)] public bool RetroArchBilinear { get; set; }
    [Key(69)] public bool RetroArchShowSettingsBeforeLaunch { get; set; } = true;
    [Key(77)] public string RetroArchAspectRatioIndex { get; set; } = "22"; // 22 = Core Provided
    [Key(78)] public bool RetroArchScaleInteger { get; set; }
    [Key(79)] public bool RetroArchShaderEnable { get; set; } = true;
    [Key(80)] public bool RetroArchHardSync { get; set; }
    [Key(81)] public bool RetroArchRunAhead { get; set; }
    [Key(82)] public bool RetroArchShowAdvancedSettings { get; set; } = true;
    [Key(83)] public bool RetroArchDiscordAllow { get; set; }

    [IgnoreMember] private const string DefaultSettingsFilePath = "settings.dat";
    [IgnoreMember] private const string OldSettingsFilePath = "settings.xml";
    [IgnoreMember] private const string DefaultNotificationSoundFileName = "click.mp3";

    public void Load()
    {
        // 1. Try loading from MessagePack (.dat)
        if (File.Exists(_filePath))
        {
            try
            {
                var bytes = File.ReadAllBytes(_filePath);
                var loaded = MessagePackSerializer.Deserialize<SettingsManager>(bytes);
                CopyFrom(loaded);
                return;
            }
            catch (Exception ex)
            {
                if (App.ServiceProvider != null)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading settings.dat. Attempting fallback.");
                }
            }
        }

        // 2. Fallback: Try migrating from old XML (.xml)
        if (File.Exists(_xmlFilePath))
        {
            if (MigrateFromXml())
            {
                return;
            }
        }

        // 3. If nothing exists or migration failed, set defaults
        SetDefaultsAndSave();
    }

    private void CopyFrom(SettingsManager other)
    {
        ThumbnailSize = other.ThumbnailSize;
        GamesPerPage = other.GamesPerPage;
        ShowGames = other.ShowGames;
        ViewMode = other.ViewMode;
        EnableGamePadNavigation = other.EnableGamePadNavigation;
        VideoUrl = other.VideoUrl;
        InfoUrl = other.InfoUrl;
        BaseTheme = other.BaseTheme;
        AccentColor = other.AccentColor;
        Language = other.Language;
        DeadZoneX = other.DeadZoneX;
        DeadZoneY = other.DeadZoneY;
        ButtonAspectRatio = other.ButtonAspectRatio;
        EnableFuzzyMatching = other.EnableFuzzyMatching;
        FuzzyMatchingThreshold = other.FuzzyMatchingThreshold;
        EnableNotificationSound = other.EnableNotificationSound;
        CustomNotificationSoundFile = other.CustomNotificationSoundFile;
        RaUsername = other.RaUsername;
        RaApiKey = other.RaApiKey;
        RaPassword = other.RaPassword;
        RaToken = other.RaToken;
        OverlayRetroAchievementButton = other.OverlayRetroAchievementButton;
        OverlayOpenVideoButton = other.OverlayOpenVideoButton;
        OverlayOpenInfoButton = other.OverlayOpenInfoButton;
        AdditionalSystemFoldersExpanded = other.AdditionalSystemFoldersExpanded;
        Emulator1Expanded = other.Emulator1Expanded;
        Emulator2Expanded = other.Emulator2Expanded;
        Emulator3Expanded = other.Emulator3Expanded;
        Emulator4Expanded = other.Emulator4Expanded;
        Emulator5Expanded = other.Emulator5Expanded;
        SystemPlayTimes = other.SystemPlayTimes ?? [];

        // Xenia
        XeniaGpu = other.XeniaGpu;
        XeniaVsync = other.XeniaVsync;
        XeniaResScaleX = other.XeniaResScaleX;
        XeniaResScaleY = other.XeniaResScaleY;
        XeniaFullscreen = other.XeniaFullscreen;
        XeniaApu = other.XeniaApu;
        XeniaMute = other.XeniaMute;
        XeniaAa = other.XeniaAa;
        XeniaScaling = other.XeniaScaling;
        XeniaApplyPatches = other.XeniaApplyPatches;
        XeniaDiscordPresence = other.XeniaDiscordPresence;
        XeniaUserLanguage = other.XeniaUserLanguage;
        XeniaHid = other.XeniaHid;
        XeniaShowSettingsBeforeLaunch = other.XeniaShowSettingsBeforeLaunch;
        XeniaReadbackResolve = other.XeniaReadbackResolve;
        XeniaGammaSrgb = other.XeniaGammaSrgb;
        XeniaVibration = other.XeniaVibration;
        XeniaMountCache = other.XeniaMountCache;

        // MAME
        MameVideo = other.MameVideo;
        MameWindow = other.MameWindow;
        MameMaximize = other.MameMaximize;
        MameKeepAspect = other.MameKeepAspect;
        MameSkipGameInfo = other.MameSkipGameInfo;
        MameAutosave = other.MameAutosave;
        MameConfirmQuit = other.MameConfirmQuit;
        MameJoystick = other.MameJoystick;
        MameShowSettingsBeforeLaunch = other.MameShowSettingsBeforeLaunch;
        MameAutoframeskip = other.MameAutoframeskip;
        MameBgfxBackend = other.MameBgfxBackend;
        MameBgfxScreenChains = other.MameBgfxScreenChains;
        MameFilter = other.MameFilter;
        MameCheat = other.MameCheat;
        MameRewind = other.MameRewind;
        MameNvramSave = other.MameNvramSave;

        // RetroArch
        RetroArchCheevosEnable = other.RetroArchCheevosEnable;
        RetroArchCheevosHardcore = other.RetroArchCheevosHardcore;
        RetroArchFullscreen = other.RetroArchFullscreen;
        RetroArchVsync = other.RetroArchVsync;
        RetroArchVideoDriver = other.RetroArchVideoDriver;
        RetroArchAudioEnable = other.RetroArchAudioEnable;
        RetroArchAudioMute = other.RetroArchAudioMute;
        RetroArchMenuDriver = other.RetroArchMenuDriver;
        RetroArchPauseNonActive = other.RetroArchPauseNonActive;
        RetroArchSaveOnExit = other.RetroArchSaveOnExit;
        RetroArchAutoSaveState = other.RetroArchAutoSaveState;
        RetroArchAutoLoadState = other.RetroArchAutoLoadState;
        RetroArchRewind = other.RetroArchRewind;
        RetroArchThreadedVideo = other.RetroArchThreadedVideo;
        RetroArchBilinear = other.RetroArchBilinear;
        RetroArchShowSettingsBeforeLaunch = other.RetroArchShowSettingsBeforeLaunch;
        RetroArchAspectRatioIndex = other.RetroArchAspectRatioIndex;
        RetroArchScaleInteger = other.RetroArchScaleInteger;
        RetroArchShaderEnable = other.RetroArchShaderEnable;
        RetroArchHardSync = other.RetroArchHardSync;
        RetroArchRunAhead = other.RetroArchRunAhead;
        RetroArchShowAdvancedSettings = other.RetroArchShowAdvancedSettings;
        RetroArchDiscordAllow = other.RetroArchDiscordAllow;
    }

    private bool MigrateFromXml()
    {
        try
        {
            DebugLogger.Log("Migrating settings from settings.xml to settings.dat...");
            XElement settings;
            var xmlSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };

            using (var reader = XmlReader.Create(_xmlFilePath, xmlSettings))
            {
                settings = XElement.Load(reader);
            }

            ThumbnailSize = ValidateThumbnailSize(settings.Element("ThumbnailSize")?.Value);
            GamesPerPage = ValidateGamesPerPage(settings.Element("GamesPerPage")?.Value);
            ShowGames = ValidateShowGames(settings.Element("ShowGames")?.Value);
            ViewMode = ValidateViewMode(settings.Element("ViewMode")?.Value);
            EnableGamePadNavigation = !bool.TryParse(settings.Element("EnableGamePadNavigation")?.Value, out var gp) || gp;
            VideoUrl = settings.Element("VideoUrl")?.Value ?? App.Configuration["Urls:YouTubeSearch"] ?? "https://www.youtube.com/results?search_query=";
            InfoUrl = settings.Element("InfoUrl")?.Value ?? App.Configuration["Urls:IgdbSearch"] ?? "https://www.igdb.com/search?q=";
            BaseTheme = settings.Element("BaseTheme")?.Value ?? "Light";
            AccentColor = settings.Element("AccentColor")?.Value ?? "Blue";
            Language = settings.Element("Language")?.Value ?? "en";
            ButtonAspectRatio = ValidateButtonAspectRatio(settings.Element("ButtonAspectRatio")?.Value);
            RaUsername = settings.Element("RA_Username")?.Value ?? string.Empty;
            RaApiKey = settings.Element("RA_ApiKey")?.Value ?? string.Empty;
            RaPassword = settings.Element("RA_Password")?.Value ?? string.Empty;
            RaToken = settings.Element("RA_Token")?.Value ?? string.Empty; // Migrate token if it existed in XML (unlikely but safe)
            DeadZoneX = float.TryParse(settings.Element("DeadZoneX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzx) ? dzx : DefaultDeadZoneX;
            DeadZoneY = float.TryParse(settings.Element("DeadZoneY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzy) ? dzy : DefaultDeadZoneY;
            EnableFuzzyMatching = !bool.TryParse(settings.Element("EnableFuzzyMatching")?.Value, out var fm) || fm;
            FuzzyMatchingThreshold = double.TryParse(settings.Element("FuzzyMatchingThreshold")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fmt) ? fmt : 0.80;
            EnableNotificationSound = !bool.TryParse(settings.Element("EnableNotificationSound")?.Value, out var ens) || ens;
            CustomNotificationSoundFile = settings.Element("CustomNotificationSoundFile")?.Value ?? DefaultNotificationSoundFileName;
            OverlayRetroAchievementButton = bool.TryParse(settings.Element("OverlayRetroAchievementButton")?.Value, out var ora) && ora;
            OverlayOpenVideoButton = !bool.TryParse(settings.Element("OverlayOpenVideoButton")?.Value, out var ovb) || ovb;
            OverlayOpenInfoButton = bool.TryParse(settings.Element("OverlayOpenInfoButton")?.Value, out var oib) && oib;

            // Xenia
            XeniaGpu = settings.Element("XeniaGpu")?.Value ?? "d3d12";
            XeniaVsync = !bool.TryParse(settings.Element("XeniaVsync")?.Value, out var xv) || xv;
            XeniaResScaleX = int.TryParse(settings.Element("XeniaResScaleX")?.Value, out var xrsx) ? xrsx : 1;
            XeniaResScaleY = int.TryParse(settings.Element("XeniaResScaleY")?.Value, out var xrsy) ? xrsy : 1;
            XeniaFullscreen = bool.TryParse(settings.Element("XeniaFullscreen")?.Value, out var xf) && xf;
            XeniaApu = settings.Element("XeniaApu")?.Value ?? "xaudio2";
            XeniaMute = bool.TryParse(settings.Element("XeniaMute")?.Value, out var xm) && xm;
            XeniaAa = settings.Element("XeniaAa")?.Value ?? "";
            XeniaScaling = settings.Element("XeniaScaling")?.Value ?? "fsr";
            XeniaApplyPatches = !bool.TryParse(settings.Element("XeniaApplyPatches")?.Value, out var xap) || xap;
            XeniaDiscordPresence = !bool.TryParse(settings.Element("XeniaDiscordPresence")?.Value, out var xdp) || xdp;
            XeniaUserLanguage = int.TryParse(settings.Element("XeniaUserLanguage")?.Value, out var xul) ? xul : 1;
            XeniaHid = settings.Element("XeniaHid")?.Value ?? "xinput";
            XeniaShowSettingsBeforeLaunch = !bool.TryParse(settings.Element("XeniaShowSettingsBeforeLaunch")?.Value, out var xss) || xss;
            XeniaReadbackResolve = "none";
            XeniaGammaSrgb = false;
            XeniaVibration = true;
            XeniaMountCache = true;

            // MAME
            MameVideo = settings.Element("MameVideo")?.Value ?? "auto";
            MameWindow = bool.TryParse(settings.Element("MameWindow")?.Value, out var mw) && mw;
            MameMaximize = !bool.TryParse(settings.Element("MameMaximize")?.Value, out var mm) || mm;
            MameKeepAspect = !bool.TryParse(settings.Element("MameKeepAspect")?.Value, out var mka) || mka;
            MameSkipGameInfo = !bool.TryParse(settings.Element("MameSkipGameInfo")?.Value, out var msgi) || msgi;
            MameAutosave = bool.TryParse(settings.Element("MameAutosave")?.Value, out var mas) && mas;
            MameConfirmQuit = bool.TryParse(settings.Element("MameConfirmQuit")?.Value, out var mcq) && mcq;
            MameJoystick = !bool.TryParse(settings.Element("MameJoystick")?.Value, out var mj) || mj;
            MameShowSettingsBeforeLaunch = !bool.TryParse(settings.Element("MameShowSettingsBeforeLaunch")?.Value, out var mss) || mss;
            MameAutoframeskip = bool.TryParse(settings.Element("MameAutoframeskip")?.Value, out var mafs) && mafs;
            MameBgfxBackend = settings.Element("MameBgfxBackend")?.Value ?? "auto";
            MameBgfxScreenChains = settings.Element("MameBgfxScreenChains")?.Value ?? "default";
            MameFilter = !bool.TryParse(settings.Element("MameFilter")?.Value, out var mf) || mf;
            MameCheat = bool.TryParse(settings.Element("MameCheat")?.Value, out var mc) && mc;
            MameRewind = bool.TryParse(settings.Element("MameRewind")?.Value, out var mr) && mr;
            MameNvramSave = !bool.TryParse(settings.Element("MameNvramSave")?.Value, out var mns) || mns;

            // RetroArch
            RetroArchCheevosEnable = bool.TryParse(settings.Element("RetroArchCheevosEnable")?.Value, out var race) && race;
            RetroArchCheevosHardcore = bool.TryParse(settings.Element("RetroArchCheevosHardcore")?.Value, out var rach) && rach;
            RetroArchFullscreen = bool.TryParse(settings.Element("RetroArchFullscreen")?.Value, out var raf) && raf;
            RetroArchVsync = !bool.TryParse(settings.Element("RetroArchVsync")?.Value, out var rav) || rav;
            RetroArchVideoDriver = settings.Element("RetroArchVideoDriver")?.Value ?? "gl";
            RetroArchAudioEnable = !bool.TryParse(settings.Element("RetroArchAudioEnable")?.Value, out var raae) || raae;
            RetroArchAudioMute = bool.TryParse(settings.Element("RetroArchAudioMute")?.Value, out var raam) && raam;
            RetroArchMenuDriver = settings.Element("RetroArchMenuDriver")?.Value ?? "ozone";
            RetroArchPauseNonActive = !bool.TryParse(settings.Element("RetroArchPauseNonActive")?.Value, out var rapna) || rapna;
            RetroArchSaveOnExit = !bool.TryParse(settings.Element("RetroArchSaveOnExit")?.Value, out var rasoe) || rasoe;
            RetroArchAutoSaveState = bool.TryParse(settings.Element("RetroArchAutoSaveState")?.Value, out var raass) && raass;
            RetroArchAutoLoadState = bool.TryParse(settings.Element("RetroArchAutoLoadState")?.Value, out var raals) && raals;
            RetroArchRewind = bool.TryParse(settings.Element("RetroArchRewind")?.Value, out var rar) && rar;
            RetroArchThreadedVideo = bool.TryParse(settings.Element("RetroArchThreadedVideo")?.Value, out var ratv) && ratv;
            RetroArchBilinear = bool.TryParse(settings.Element("RetroArchBilinear")?.Value, out var rab) && rab;
            RetroArchShowSettingsBeforeLaunch = !bool.TryParse(settings.Element("RetroArchShowSettingsBeforeLaunch")?.Value, out var rass) || rass;
            RetroArchAspectRatioIndex = settings.Element("RetroArchAspectRatioIndex")?.Value ?? "22";
            RetroArchScaleInteger = bool.TryParse(settings.Element("RetroArchScaleInteger")?.Value, out var rasi) && rasi;
            RetroArchShaderEnable = !bool.TryParse(settings.Element("RetroArchShaderEnable")?.Value, out var rase) || rase;
            RetroArchHardSync = bool.TryParse(settings.Element("RetroArchHardSync")?.Value, out var rahs) && rahs;
            RetroArchRunAhead = bool.TryParse(settings.Element("RetroArchRunAhead")?.Value, out var rara) && rara;
            RetroArchShowAdvancedSettings = !bool.TryParse(settings.Element("RetroArchShowAdvancedSettings")?.Value, out var rasas) || rasas;
            RetroArchDiscordAllow = bool.TryParse(settings.Element("RetroArchDiscordAllow")?.Value, out var rada) && rada;

            var playTimes = settings.Element("SystemPlayTimes");
            if (playTimes != null)
            {
                foreach (var pt in playTimes.Elements("SystemPlayTime"))
                {
                    SystemPlayTimes.Add(new SystemPlayTime
                    {
                        SystemName = pt.Element("SystemName")?.Value ?? "",
                        PlayTime = pt.Element("PlayTime")?.Value ?? "00:00:00"
                    });
                }
            }

            Save(); // Save to .dat

            // Delete old file
            File.Delete(_xmlFilePath);
            DebugLogger.Log("Migration successful. settings.xml deleted.");
            return true;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to migrate settings from XML.");
            return false;
        }
    }

    public void Save()
    {
        lock (_saveLock)
        {
            try
            {
                var tempPath = _filePath + ".tmp";
                var bytes = MessagePackSerializer.Serialize(this);
                File.WriteAllBytes(tempPath, bytes);
                File.Move(tempPath, _filePath, true);
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving settings.dat");
            }
        }
    }

    private int ValidateThumbnailSize(string value)
    {
        return int.TryParse(value, out var p) && _validThumbnailSizes.Contains(p) ? p : 250;
    }

    private int ValidateGamesPerPage(string value)
    {
        return int.TryParse(value, out var p) && _validGamesPerPage.Contains(p) ? p : 200;
    }

    private string ValidateShowGames(string value)
    {
        return _validShowGames.Contains(value) ? value : "ShowAll";
    }

    private string ValidateViewMode(string value)
    {
        return _validViewModes.Contains(value) ? value : "GridView";
    }

    private string ValidateButtonAspectRatio(string value)
    {
        return _validButtonAspectRatio.Contains(value) ? value : "Square";
    }

    private void SetDefaultsAndSave()
    {
        ThumbnailSize = 250;
        GamesPerPage = 200;
        ShowGames = "ShowAll";
        ViewMode = "GridView";
        EnableGamePadNavigation = false;
        VideoUrl = App.Configuration["Urls:YouTubeSearch"] ?? "https://www.youtube.com/results?search_query=";
        InfoUrl = App.Configuration["Urls:IgdbSearch"] ?? "https://www.igdb.com/search?q=";
        BaseTheme = "Light";
        AccentColor = "Blue";
        Language = "en";
        DeadZoneX = DefaultDeadZoneX;
        DeadZoneY = DefaultDeadZoneY;
        ButtonAspectRatio = "Square";
        EnableFuzzyMatching = true;
        FuzzyMatchingThreshold = 0.80;
        EnableNotificationSound = true;
        CustomNotificationSoundFile = DefaultNotificationSoundFileName;
        OverlayRetroAchievementButton = false;
        OverlayOpenVideoButton = true;
        OverlayOpenInfoButton = false;
        AdditionalSystemFoldersExpanded = true;
        Emulator1Expanded = true;
        Emulator2Expanded = true;
        Emulator3Expanded = true;
        Emulator4Expanded = true;
        Emulator5Expanded = true;
        SystemPlayTimes = [];

        // Xenia Defaults
        XeniaGpu = "d3d12";
        XeniaVsync = true;
        XeniaResScaleX = 1;
        XeniaResScaleY = 1;
        XeniaFullscreen = false;
        XeniaApu = "xaudio2";
        XeniaMute = false;
        XeniaAa = "";
        XeniaScaling = "fsr";
        XeniaApplyPatches = true;
        XeniaDiscordPresence = true;
        XeniaUserLanguage = 1;
        XeniaHid = "xinput";
        XeniaShowSettingsBeforeLaunch = true;
        XeniaReadbackResolve = "none";
        XeniaGammaSrgb = false;
        XeniaVibration = true;
        XeniaMountCache = true;

        // MAME Defaults
        MameVideo = "auto";
        MameWindow = false;
        MameMaximize = true;
        MameKeepAspect = true;
        MameSkipGameInfo = true;
        MameAutosave = false;
        MameConfirmQuit = false;
        MameJoystick = true;
        MameShowSettingsBeforeLaunch = true;
        MameAutoframeskip = false;
        MameBgfxBackend = "auto";
        MameBgfxScreenChains = "default";
        MameFilter = true;
        MameCheat = false;
        MameRewind = false;
        MameNvramSave = true;

        // RetroArch Defaults
        RetroArchCheevosEnable = false;
        RetroArchCheevosHardcore = false;
        RetroArchFullscreen = false;
        RetroArchVsync = true;
        RetroArchVideoDriver = "gl";
        RetroArchAudioEnable = true;
        RetroArchAudioMute = false;
        RetroArchMenuDriver = "ozone";
        RetroArchPauseNonActive = true;
        RetroArchSaveOnExit = true;
        RetroArchAutoSaveState = false;
        RetroArchAutoLoadState = false;
        RetroArchRewind = false;
        RetroArchThreadedVideo = false;
        RetroArchBilinear = false;
        RetroArchShowSettingsBeforeLaunch = true;
        RetroArchAspectRatioIndex = "22";
        RetroArchScaleInteger = false;
        RetroArchShaderEnable = true;
        RetroArchHardSync = false;
        RetroArchRunAhead = false;
        RetroArchShowAdvancedSettings = true;
        RetroArchDiscordAllow = false;

        Save();
    }

    public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
    {
        if (string.IsNullOrWhiteSpace(systemName) || playTime == TimeSpan.Zero) return;

        lock (_saveLock)
        {
            var item = SystemPlayTimes.FirstOrDefault(s => s.SystemName == systemName);
            if (item == null)
            {
                item = new SystemPlayTime { SystemName = systemName, PlayTime = "00:00:00" };
                SystemPlayTimes.Add(item);
            }

            if (TimeSpan.TryParse(item.PlayTime, CultureInfo.InvariantCulture, out var existing))
            {
                item.PlayTime = (existing + playTime).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher.Services.SettingsManager;

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

    // --- Supermodel Global Configuration ---
    [Key(100)] public bool SupermodelNew3DEngine { get; set; } = true;
    [Key(101)] public bool SupermodelQuadRendering { get; set; }
    [Key(102)] public bool SupermodelFullscreen { get; set; } = true;
    [Key(103)] public int SupermodelResX { get; set; } = 1920;
    [Key(104)] public int SupermodelResY { get; set; } = 1080;
    [Key(105)] public bool SupermodelWideScreen { get; set; } = true;
    [Key(106)] public bool SupermodelStretch { get; set; }
    [Key(107)] public bool SupermodelVsync { get; set; } = true;
    [Key(108)] public bool SupermodelThrottle { get; set; } = true;
    [Key(109)] public int SupermodelMusicVolume { get; set; } = 100;
    [Key(110)] public int SupermodelSoundVolume { get; set; } = 100;
    [Key(111)] public string SupermodelInputSystem { get; set; } = "xinput";
    [Key(112)] public bool SupermodelMultiThreaded { get; set; } = true;
    [Key(113)] public int SupermodelPowerPcFrequency { get; set; } = 50;
    [Key(114)] public bool SupermodelShowSettingsBeforeLaunch { get; set; } = true;

    // --- Mednafen Global Configuration ---
    [Key(120)] public string MednafenVideoDriver { get; set; } = "opengl";
    [Key(121)] public bool MednafenFullscreen { get; set; }
    [Key(122)] public bool MednafenVsync { get; set; } = true;
    [Key(123)] public string MednafenStretch { get; set; } = "aspect";
    [Key(124)] public bool MednafenBilinear { get; set; }
    [Key(125)] public int MednafenScanlines { get; set; }
    [Key(126)] public string MednafenShader { get; set; } = "none";
    [Key(127)] public int MednafenVolume { get; set; } = 100;
    [Key(128)] public bool MednafenCheats { get; set; } = true;
    [Key(129)] public bool MednafenRewind { get; set; }
    [Key(130)] public bool MednafenShowSettingsBeforeLaunch { get; set; } = true;

    // --- SEGA Model 2 Global Configuration ---
    [Key(140)] public int SegaModel2ResX { get; set; } = 640;
    [Key(141)] public int SegaModel2ResY { get; set; } = 480;
    [Key(142)] public int SegaModel2WideScreen { get; set; } // 0=4:3, 1=16:9, 2=16:10
    [Key(143)] public bool SegaModel2Bilinear { get; set; } = true;
    [Key(144)] public bool SegaModel2Trilinear { get; set; }
    [Key(145)] public bool SegaModel2FilterTilemaps { get; set; }
    [Key(146)] public bool SegaModel2DrawCross { get; set; } = true;
    [Key(147)] public int SegaModel2Fsaa { get; set; }
    [Key(148)] public bool SegaModel2XInput { get; set; }
    [Key(149)] public bool SegaModel2EnableFf { get; set; }
    [Key(150)] public bool SegaModel2HoldGears { get; set; }
    [Key(151)] public bool SegaModel2UseRawInput { get; set; }
    [Key(152)] public bool SegaModel2ShowSettingsBeforeLaunch { get; set; } = true;

    // --- Ares Global Configuration ---
    [Key(160)] public string AresVideoDriver { get; set; } = "OpenGL 3.2";
    [Key(161)] public bool AresExclusive { get; set; } // Fullscreen
    [Key(162)] public string AresShader { get; set; } = "None";
    [Key(163)] public int AresMultiplier { get; set; } = 2;
    [Key(164)] public string AresAspectCorrection { get; set; } = "Standard";
    [Key(165)] public bool AresMute { get; set; }
    [Key(166)] public double AresVolume { get; set; } = 1.0;
    [Key(167)] public bool AresFastBoot { get; set; }
    [Key(168)] public bool AresRewind { get; set; }
    [Key(169)] public bool AresRunAhead { get; set; }
    [Key(170)] public bool AresAutoSaveMemory { get; set; } = true;
    [Key(171)] public bool AresShowSettingsBeforeLaunch { get; set; } = true;

    // --- Daphne Global Configuration ---
    [Key(180)] public bool DaphneFullscreen { get; set; }
    [Key(181)] public int DaphneResX { get; set; } = 640;
    [Key(182)] public int DaphneResY { get; set; } = 480;
    [Key(183)] public bool DaphneDisableCrosshairs { get; set; }
    [Key(184)] public bool DaphneBilinear { get; set; } = true;
    [Key(185)] public bool DaphneEnableSound { get; set; } = true;
    [Key(186)] public bool DaphneUseOverlays { get; set; } = true;
    [Key(187)] public bool DaphneShowSettingsBeforeLaunch { get; set; } = true;

    // --- Blastem Global Configuration ---
    [Key(200)] public bool BlastemFullscreen { get; set; }
    [Key(201)] public bool BlastemVsync { get; set; }
    [Key(202)] public string BlastemAspect { get; set; } = "4:3";
    [Key(203)] public string BlastemScaling { get; set; } = "linear";
    [Key(204)] public bool BlastemScanlines { get; set; }
    [Key(205)] public int BlastemAudioRate { get; set; } = 48000;
    [Key(206)] public string BlastemSyncSource { get; set; } = "audio";
    [Key(207)] public bool BlastemShowSettingsBeforeLaunch { get; set; } = true;

    // --- Mesen Global Configuration ---
    [Key(220)] public bool MesenFullscreen { get; set; }
    [Key(221)] public bool MesenVsync { get; set; }
    [Key(222)] public string MesenAspectRatio { get; set; } = "NoStretching";
    [Key(223)] public bool MesenBilinear { get; set; }
    [Key(224)] public string MesenVideoFilter { get; set; } = "None";
    [Key(225)] public bool MesenEnableAudio { get; set; } = true;
    [Key(226)] public int MesenMasterVolume { get; set; } = 100;
    [Key(227)] public bool MesenRewind { get; set; }
    [Key(228)] public int MesenRunAhead { get; set; }
    [Key(229)] public bool MesenPauseInBackground { get; set; }
    [Key(230)] public bool MesenShowSettingsBeforeLaunch { get; set; } = true;

    // --- DuckStation Global Configuration ---
    [Key(240)] public bool DuckStationStartFullscreen { get; set; }
    [Key(241)] public bool DuckStationPauseOnFocusLoss { get; set; } = true;
    [Key(242)] public bool DuckStationSaveStateOnExit { get; set; } = true;
    [Key(243)] public bool DuckStationRewindEnable { get; set; }
    [Key(244)] public int DuckStationRunaheadFrameCount { get; set; }
    [Key(245)] public string DuckStationRenderer { get; set; } = "Automatic";
    [Key(246)] public int DuckStationResolutionScale { get; set; } = 2;
    [Key(247)] public string DuckStationTextureFilter { get; set; } = "Nearest";
    [Key(248)] public bool DuckStationWidescreenHack { get; set; }
    [Key(249)] public bool DuckStationPgxpEnable { get; set; }
    [Key(250)] public string DuckStationAspectRatio { get; set; } = "16:9";
    [Key(251)] public bool DuckStationVsync { get; set; }
    [Key(252)] public int DuckStationOutputVolume { get; set; } = 100;
    [Key(253)] public bool DuckStationOutputMuted { get; set; }
    [Key(254)] public bool DuckStationShowSettingsBeforeLaunch { get; set; } = true;

    // --- RPCS3 Global Configuration ---
    [Key(260)] public string Rpcs3Renderer { get; set; } = "Vulkan";
    [Key(261)] public string Rpcs3Resolution { get; set; } = "1280x720";
    [Key(262)] public string Rpcs3AspectRatio { get; set; } = "16:9";
    [Key(263)] public bool Rpcs3Vsync { get; set; }
    [Key(264)] public int Rpcs3ResolutionScale { get; set; } = 100;
    [Key(265)] public int Rpcs3AnisotropicFilter { get; set; }
    [Key(266)] public string Rpcs3PpuDecoder { get; set; } = "Recompiler (LLVM)";
    [Key(267)] public string Rpcs3SpuDecoder { get; set; } = "Recompiler (LLVM)";
    [Key(268)] public string Rpcs3AudioRenderer { get; set; } = "Cubeb";
    [Key(269)] public bool Rpcs3AudioBuffering { get; set; } = true;
    [Key(270)] public bool Rpcs3StartFullscreen { get; set; }
    [Key(271)] public bool Rpcs3ShowSettingsBeforeLaunch { get; set; } = true;

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

        // Supermodel
        SupermodelNew3DEngine = other.SupermodelNew3DEngine;
        SupermodelQuadRendering = other.SupermodelQuadRendering;
        SupermodelFullscreen = other.SupermodelFullscreen;
        SupermodelResX = other.SupermodelResX;
        SupermodelResY = other.SupermodelResY;
        SupermodelWideScreen = other.SupermodelWideScreen;
        SupermodelStretch = other.SupermodelStretch;
        SupermodelVsync = other.SupermodelVsync;
        SupermodelThrottle = other.SupermodelThrottle;
        SupermodelMusicVolume = other.SupermodelMusicVolume;
        SupermodelSoundVolume = other.SupermodelSoundVolume;
        SupermodelInputSystem = other.SupermodelInputSystem;
        SupermodelMultiThreaded = other.SupermodelMultiThreaded;
        SupermodelPowerPcFrequency = other.SupermodelPowerPcFrequency;
        SupermodelShowSettingsBeforeLaunch = other.SupermodelShowSettingsBeforeLaunch;

        // Mednafen
        MednafenVideoDriver = other.MednafenVideoDriver;
        MednafenFullscreen = other.MednafenFullscreen;
        MednafenVsync = other.MednafenVsync;
        MednafenStretch = other.MednafenStretch;
        MednafenBilinear = other.MednafenBilinear;
        MednafenScanlines = other.MednafenScanlines;
        MednafenShader = other.MednafenShader;
        MednafenVolume = other.MednafenVolume;
        MednafenCheats = other.MednafenCheats;
        MednafenRewind = other.MednafenRewind;
        MednafenShowSettingsBeforeLaunch = other.MednafenShowSettingsBeforeLaunch;

        // Sega Model 2
        SegaModel2ResX = other.SegaModel2ResX;
        SegaModel2ResY = other.SegaModel2ResY;
        SegaModel2WideScreen = other.SegaModel2WideScreen;
        SegaModel2Bilinear = other.SegaModel2Bilinear;
        SegaModel2Trilinear = other.SegaModel2Trilinear;
        SegaModel2FilterTilemaps = other.SegaModel2FilterTilemaps;
        SegaModel2DrawCross = other.SegaModel2DrawCross;
        SegaModel2Fsaa = other.SegaModel2Fsaa;
        SegaModel2XInput = other.SegaModel2XInput;
        SegaModel2EnableFf = other.SegaModel2EnableFf;
        SegaModel2HoldGears = other.SegaModel2HoldGears;
        SegaModel2UseRawInput = other.SegaModel2UseRawInput;
        SegaModel2ShowSettingsBeforeLaunch = other.SegaModel2ShowSettingsBeforeLaunch;

        // Ares
        AresVideoDriver = other.AresVideoDriver;
        AresExclusive = other.AresExclusive;
        AresShader = other.AresShader;
        AresMultiplier = other.AresMultiplier;
        AresAspectCorrection = other.AresAspectCorrection;
        AresMute = other.AresMute;
        AresVolume = other.AresVolume;
        AresFastBoot = other.AresFastBoot;
        AresRewind = other.AresRewind;
        AresRunAhead = other.AresRunAhead;
        AresAutoSaveMemory = other.AresAutoSaveMemory;
        AresShowSettingsBeforeLaunch = other.AresShowSettingsBeforeLaunch;

        // Daphne
        DaphneFullscreen = other.DaphneFullscreen;
        DaphneResX = other.DaphneResX;
        DaphneResY = other.DaphneResY;
        DaphneDisableCrosshairs = other.DaphneDisableCrosshairs;
        DaphneBilinear = other.DaphneBilinear;
        DaphneEnableSound = other.DaphneEnableSound;
        DaphneUseOverlays = other.DaphneUseOverlays;
        DaphneShowSettingsBeforeLaunch = other.DaphneShowSettingsBeforeLaunch;

        // Blastem
        BlastemFullscreen = other.BlastemFullscreen;
        BlastemVsync = other.BlastemVsync;
        BlastemAspect = other.BlastemAspect;
        BlastemScaling = other.BlastemScaling;
        BlastemScanlines = other.BlastemScanlines;
        BlastemAudioRate = other.BlastemAudioRate;
        BlastemSyncSource = other.BlastemSyncSource;
        BlastemShowSettingsBeforeLaunch = other.BlastemShowSettingsBeforeLaunch;

        // Mesen
        MesenFullscreen = other.MesenFullscreen;
        MesenVsync = other.MesenVsync;
        MesenAspectRatio = other.MesenAspectRatio;
        MesenBilinear = other.MesenBilinear;
        MesenVideoFilter = other.MesenVideoFilter;
        MesenEnableAudio = other.MesenEnableAudio;
        MesenMasterVolume = other.MesenMasterVolume;
        MesenRewind = other.MesenRewind;
        MesenRunAhead = other.MesenRunAhead;
        MesenPauseInBackground = other.MesenPauseInBackground;
        MesenShowSettingsBeforeLaunch = other.MesenShowSettingsBeforeLaunch;

        // DuckStation
        DuckStationStartFullscreen = other.DuckStationStartFullscreen;
        DuckStationPauseOnFocusLoss = other.DuckStationPauseOnFocusLoss;
        DuckStationSaveStateOnExit = other.DuckStationSaveStateOnExit;
        DuckStationRewindEnable = other.DuckStationRewindEnable;
        DuckStationRunaheadFrameCount = other.DuckStationRunaheadFrameCount;
        DuckStationRenderer = other.DuckStationRenderer;
        DuckStationResolutionScale = other.DuckStationResolutionScale;
        DuckStationTextureFilter = other.DuckStationTextureFilter;
        DuckStationWidescreenHack = other.DuckStationWidescreenHack;
        DuckStationPgxpEnable = other.DuckStationPgxpEnable;
        DuckStationAspectRatio = other.DuckStationAspectRatio;
        DuckStationVsync = other.DuckStationVsync;
        DuckStationOutputVolume = other.DuckStationOutputVolume;
        DuckStationOutputMuted = other.DuckStationOutputMuted;
        DuckStationShowSettingsBeforeLaunch = other.DuckStationShowSettingsBeforeLaunch;

        // RPCS3
        Rpcs3Renderer = other.Rpcs3Renderer;
        Rpcs3Resolution = other.Rpcs3Resolution;
        Rpcs3AspectRatio = other.Rpcs3AspectRatio;
        Rpcs3Vsync = other.Rpcs3Vsync;
        Rpcs3ResolutionScale = other.Rpcs3ResolutionScale;
        Rpcs3AnisotropicFilter = other.Rpcs3AnisotropicFilter;
        Rpcs3PpuDecoder = other.Rpcs3PpuDecoder;
        Rpcs3SpuDecoder = other.Rpcs3SpuDecoder;
        Rpcs3AudioRenderer = other.Rpcs3AudioRenderer;
        Rpcs3AudioBuffering = other.Rpcs3AudioBuffering;
        Rpcs3StartFullscreen = other.Rpcs3StartFullscreen;
        Rpcs3ShowSettingsBeforeLaunch = other.Rpcs3ShowSettingsBeforeLaunch;
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

            // Supermodel (set defaults during migration)
            SupermodelNew3DEngine = true;
            SupermodelQuadRendering = false;
            SupermodelFullscreen = true;
            SupermodelResX = 1920;
            SupermodelResY = 1080;
            SupermodelWideScreen = true;
            SupermodelStretch = false;
            SupermodelVsync = true;
            SupermodelThrottle = true;
            SupermodelMusicVolume = 100;
            SupermodelSoundVolume = 100;
            SupermodelInputSystem = "xinput";
            SupermodelMultiThreaded = true;
            SupermodelPowerPcFrequency = 50;
            SupermodelShowSettingsBeforeLaunch = true;

            // Mednafen (set defaults during migration)
            MednafenVideoDriver = "opengl";
            MednafenFullscreen = false;
            MednafenVsync = true;
            MednafenStretch = "aspect";
            MednafenBilinear = false;
            MednafenScanlines = 0;
            MednafenShader = "none";
            MednafenVolume = 100;
            MednafenCheats = true;
            MednafenRewind = false;
            MednafenShowSettingsBeforeLaunch = true;

            // Sega Model 2 (set defaults during migration)
            SegaModel2ResX = 640;
            SegaModel2ResY = 480;
            SegaModel2WideScreen = 0;
            SegaModel2Bilinear = true;
            SegaModel2Trilinear = false;
            SegaModel2FilterTilemaps = false;
            SegaModel2DrawCross = true;
            SegaModel2Fsaa = 0;
            SegaModel2XInput = false;
            SegaModel2EnableFf = false;
            SegaModel2HoldGears = false;
            SegaModel2UseRawInput = false;
            SegaModel2ShowSettingsBeforeLaunch = true;

            // Ares Defaults
            AresVideoDriver = "OpenGL 3.2";
            AresExclusive = false;
            AresShader = "None";
            AresMultiplier = 2;
            AresAspectCorrection = "Standard";
            AresMute = false;
            AresVolume = 1.0;
            AresFastBoot = false;
            AresRewind = false;
            AresRunAhead = false;
            AresAutoSaveMemory = true;
            AresShowSettingsBeforeLaunch = true;

            // Daphne Defaults
            DaphneFullscreen = false;
            DaphneResX = 640;
            DaphneResY = 480;
            DaphneDisableCrosshairs = false;
            DaphneBilinear = true;
            DaphneEnableSound = true;
            DaphneUseOverlays = true;
            DaphneShowSettingsBeforeLaunch = true;

            // Blastem Defaults
            BlastemFullscreen = false;
            BlastemVsync = false;
            BlastemAspect = "4:3";
            BlastemScaling = "linear";
            BlastemScanlines = false;
            BlastemAudioRate = 48000;
            BlastemSyncSource = "audio";
            BlastemShowSettingsBeforeLaunch = true;

            // Mesen Defaults
            MesenFullscreen = false;
            MesenVsync = false;
            MesenAspectRatio = "NoStretching";
            MesenBilinear = false;
            MesenVideoFilter = "None";
            MesenEnableAudio = true;
            MesenMasterVolume = 100;
            MesenRewind = false;
            MesenRunAhead = 0;
            MesenPauseInBackground = false;
            MesenShowSettingsBeforeLaunch = true;

            // DuckStation Defaults
            DuckStationStartFullscreen = false;
            DuckStationPauseOnFocusLoss = true;
            DuckStationSaveStateOnExit = true;
            DuckStationRewindEnable = false;
            DuckStationRunaheadFrameCount = 0;
            DuckStationRenderer = "Automatic";
            DuckStationResolutionScale = 2;
            DuckStationTextureFilter = "Nearest";
            DuckStationWidescreenHack = false;
            DuckStationPgxpEnable = false;
            DuckStationAspectRatio = "16:9";
            DuckStationVsync = false;
            DuckStationOutputVolume = 100;
            DuckStationOutputMuted = false;
            DuckStationShowSettingsBeforeLaunch = true;

            // RPCS3 Defaults
            Rpcs3Renderer = "Vulkan";
            Rpcs3Resolution = "1280x720";
            Rpcs3AspectRatio = "16:9";
            Rpcs3Vsync = false;
            Rpcs3ResolutionScale = 100;
            Rpcs3AnisotropicFilter = 0;
            Rpcs3PpuDecoder = "Recompiler (LLVM)";
            Rpcs3SpuDecoder = "Recompiler (LLVM)";
            Rpcs3AudioRenderer = "Cubeb";
            Rpcs3AudioBuffering = true;
            Rpcs3StartFullscreen = false;
            Rpcs3ShowSettingsBeforeLaunch = true;

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

        // Supermodel Defaults
        SupermodelNew3DEngine = true;
        SupermodelQuadRendering = false;
        SupermodelFullscreen = true;
        SupermodelResX = 1920;
        SupermodelResY = 1080;
        SupermodelWideScreen = true;
        SupermodelStretch = false;
        SupermodelVsync = true;
        SupermodelThrottle = true;
        SupermodelMusicVolume = 100;
        SupermodelSoundVolume = 100;
        SupermodelInputSystem = "xinput";
        SupermodelMultiThreaded = true;
        SupermodelPowerPcFrequency = 50;
        SupermodelShowSettingsBeforeLaunch = true;

        // Mednafen Defaults
        MednafenVideoDriver = "opengl";
        MednafenFullscreen = false;
        MednafenVsync = true;
        MednafenStretch = "aspect";
        MednafenBilinear = false;
        MednafenScanlines = 0;
        MednafenShader = "none";
        MednafenVolume = 100;
        MednafenCheats = true;
        MednafenRewind = false;
        MednafenShowSettingsBeforeLaunch = true;

        // Sega Model 2 Defaults
        SegaModel2ResX = 640;
        SegaModel2ResY = 480;
        SegaModel2WideScreen = 0;
        SegaModel2Bilinear = true;
        SegaModel2Trilinear = false;
        SegaModel2FilterTilemaps = false;
        SegaModel2DrawCross = true;
        SegaModel2Fsaa = 0;
        SegaModel2XInput = false;
        SegaModel2EnableFf = false;
        SegaModel2HoldGears = false;
        SegaModel2UseRawInput = false;
        SegaModel2ShowSettingsBeforeLaunch = true;

        // Ares Defaults
        AresVideoDriver = "OpenGL 3.2";
        AresExclusive = false;
        AresShader = "None";
        AresMultiplier = 2;
        AresAspectCorrection = "Standard";
        AresMute = false;
        AresVolume = 1.0;
        AresFastBoot = false;
        AresRewind = false;
        AresRunAhead = false;
        AresAutoSaveMemory = true;
        AresShowSettingsBeforeLaunch = true;

        // Daphne Defaults
        DaphneFullscreen = false;
        DaphneResX = 640;
        DaphneResY = 480;
        DaphneDisableCrosshairs = false;
        DaphneBilinear = true;
        DaphneEnableSound = true;
        DaphneUseOverlays = true;
        DaphneShowSettingsBeforeLaunch = true;

        // Blastem Defaults
        BlastemFullscreen = false;
        BlastemVsync = false;
        BlastemAspect = "4:3";
        BlastemScaling = "linear";
        BlastemScanlines = false;
        BlastemAudioRate = 48000;
        BlastemSyncSource = "audio";
        BlastemShowSettingsBeforeLaunch = true;

        // Mesen Defaults
        MesenFullscreen = false;
        MesenVsync = false;
        MesenAspectRatio = "NoStretching";
        MesenBilinear = false;
        MesenVideoFilter = "None";
        MesenEnableAudio = true;
        MesenMasterVolume = 100;
        MesenRewind = false;
        MesenRunAhead = 0;
        MesenPauseInBackground = false;
        MesenShowSettingsBeforeLaunch = true;

        // DuckStation Defaults
        DuckStationStartFullscreen = false;
        DuckStationPauseOnFocusLoss = true;
        DuckStationSaveStateOnExit = true;
        DuckStationRewindEnable = false;
        DuckStationRunaheadFrameCount = 0;
        DuckStationRenderer = "Automatic";
        DuckStationResolutionScale = 2;
        DuckStationTextureFilter = "Nearest";
        DuckStationWidescreenHack = false;
        DuckStationPgxpEnable = false;
        DuckStationAspectRatio = "16:9";
        DuckStationVsync = false;
        DuckStationOutputVolume = 100;
        DuckStationOutputMuted = false;
        DuckStationShowSettingsBeforeLaunch = true;

        // RPCS3 Defaults
        Rpcs3Renderer = "Vulkan";
        Rpcs3Resolution = "1280x720";
        Rpcs3AspectRatio = "16:9";
        Rpcs3Vsync = false;
        Rpcs3ResolutionScale = 100;
        Rpcs3AnisotropicFilter = 0;
        Rpcs3PpuDecoder = "Recompiler (LLVM)";
        Rpcs3SpuDecoder = "Recompiler (LLVM)";
        Rpcs3AudioRenderer = "Cubeb";
        Rpcs3AudioBuffering = true;
        Rpcs3StartFullscreen = false;
        Rpcs3ShowSettingsBeforeLaunch = true;

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
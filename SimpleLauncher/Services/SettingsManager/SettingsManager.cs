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

    // Application Settings
    public int ThumbnailSize { get; set; }
    public int GamesPerPage { get; set; }
    public string ShowGames { get; set; }
    public string ViewMode { get; set; }
    public bool EnableGamePadNavigation { get; set; }
    public string VideoUrl { get; set; }
    public string InfoUrl { get; set; }
    public string BaseTheme { get; set; }
    public string AccentColor { get; set; }
    public string Language { get; set; }
    public float DeadZoneX { get; set; }
    public float DeadZoneY { get; set; }
    public string ButtonAspectRatio { get; set; }
    public bool EnableFuzzyMatching { get; set; }
    public double FuzzyMatchingThreshold { get; set; }
    [IgnoreMember] public const float DefaultDeadZoneX = 0.05f;
    [IgnoreMember] public const float DefaultDeadZoneY = 0.02f;
    public bool EnableNotificationSound { get; set; }
    public string CustomNotificationSoundFile { get; set; }
    public string RaUsername { get; set; }
    public string RaApiKey { get; set; }
    public string RaPassword { get; set; }
    public string RaToken { get; set; }
    public bool OverlayRetroAchievementButton { get; set; }
    public bool OverlayOpenVideoButton { get; set; }
    public bool OverlayOpenInfoButton { get; set; }
    public bool AdditionalSystemFoldersExpanded { get; set; }
    public bool Emulator1Expanded { get; set; }
    public bool Emulator2Expanded { get; set; }
    public bool Emulator3Expanded { get; set; }
    public bool Emulator4Expanded { get; set; }
    public bool Emulator5Expanded { get; set; }
    public List<SystemPlayTime> SystemPlayTimes { get; set; } = [];

    // Ares
    public string AresVideoDriver { get; set; } = "OpenGL 3.2";
    public bool AresExclusive { get; set; } // Fullscreen
    public string AresShader { get; set; } = "None";
    public int AresMultiplier { get; set; } = 2;
    public string AresAspectCorrection { get; set; } = "Standard";
    public bool AresMute { get; set; }
    public double AresVolume { get; set; } = 1.0;
    public bool AresFastBoot { get; set; }
    public bool AresRewind { get; set; }
    public bool AresRunAhead { get; set; }
    public bool AresAutoSaveMemory { get; set; } = true;
    public bool AresShowSettingsBeforeLaunch { get; set; } = true;

    // Azahar
    public int AzaharGraphicsApi { get; set; } = 1; // 0=OpenGL, 1=Vulkan
    public int AzaharResolutionFactor { get; set; } = 1; // 0=Auto, 1=1x, 2=2x...
    public bool AzaharUseVsync { get; set; } = true;
    public bool AzaharAsyncShaderCompilation { get; set; } = true;
    public bool AzaharFullscreen { get; set; } = true;
    public int AzaharVolume { get; set; } = 100;
    public bool AzaharIsNew3ds { get; set; } = true;
    public int AzaharLayoutOption { get; set; } // 0=Default, 1=Single, 2=Large...
    public bool AzaharShowSettingsBeforeLaunch { get; set; } = true;

    // Blastem
    public bool BlastemFullscreen { get; set; }
    public bool BlastemVsync { get; set; }
    public string BlastemAspect { get; set; } = "4:3";
    public string BlastemScaling { get; set; } = "linear";
    public bool BlastemScanlines { get; set; }
    public int BlastemAudioRate { get; set; } = 48000;
    public string BlastemSyncSource { get; set; } = "audio";
    public bool BlastemShowSettingsBeforeLaunch { get; set; } = true;

    // Cemu
    public bool CemuFullscreen { get; set; }
    public int CemuGraphicApi { get; set; } = 1; // 0=OpenGL, 1=Vulkan
    public int CemuVsync { get; set; } = 1; // 0=Off, 1=On
    public bool CemuAsyncCompile { get; set; } = true;
    public int CemuTvVolume { get; set; } = 50;
    public int CemuConsoleLanguage { get; set; } = 1; // 1=English
    public bool CemuDiscordPresence { get; set; } = true;
    public bool CemuShowSettingsBeforeLaunch { get; set; } = true;

    // Daphne
    public bool DaphneFullscreen { get; set; }
    public int DaphneResX { get; set; } = 640;
    public int DaphneResY { get; set; } = 480;
    public bool DaphneDisableCrosshairs { get; set; }
    public bool DaphneBilinear { get; set; } = true;
    public bool DaphneEnableSound { get; set; } = true;
    public bool DaphneUseOverlays { get; set; } = true;
    public bool DaphneShowSettingsBeforeLaunch { get; set; } = true;

    // Dolphin
    public string DolphinGfxBackend { get; set; } = "Vulkan";
    public bool DolphinDspThread { get; set; } = true;
    public bool DolphinWiimoteContinuousScanning { get; set; } = true;
    public bool DolphinWiimoteEnableSpeaker { get; set; } = true;
    public bool DolphinShowSettingsBeforeLaunch { get; set; } = true;

    // DuckStation
    public bool DuckStationStartFullscreen { get; set; }
    public bool DuckStationPauseOnFocusLoss { get; set; } = true;
    public bool DuckStationSaveStateOnExit { get; set; } = true;
    public bool DuckStationRewindEnable { get; set; }
    public int DuckStationRunaheadFrameCount { get; set; }
    public string DuckStationRenderer { get; set; } = "Automatic";
    public int DuckStationResolutionScale { get; set; } = 2;
    public string DuckStationTextureFilter { get; set; } = "Nearest";
    public bool DuckStationWidescreenHack { get; set; }
    public bool DuckStationPgxpEnable { get; set; }
    public string DuckStationAspectRatio { get; set; } = "16:9";
    public bool DuckStationVsync { get; set; }
    public int DuckStationOutputVolume { get; set; } = 100;
    public bool DuckStationOutputMuted { get; set; }
    public bool DuckStationShowSettingsBeforeLaunch { get; set; } = true;

    // Flycast
    public bool FlycastFullscreen { get; set; }
    public int FlycastWidth { get; set; } = 640;
    public int FlycastHeight { get; set; } = 480;
    public bool FlycastMaximized { get; set; }
    public bool FlycastShowSettingsBeforeLaunch { get; set; } = true;

    // MAME
    public string MameVideo { get; set; } = "auto"; // auto, d3d, opengl, bgfx
    public bool MameWindow { get; set; }
    public bool MameMaximize { get; set; } = true;
    public bool MameKeepAspect { get; set; } = true;
    public bool MameSkipGameInfo { get; set; } = true;
    public bool MameAutosave { get; set; }
    public bool MameConfirmQuit { get; set; }
    public bool MameJoystick { get; set; } = true;
    public bool MameShowSettingsBeforeLaunch { get; set; } = true;
    public bool MameAutoframeskip { get; set; }
    public string MameBgfxBackend { get; set; } = "auto";
    public string MameBgfxScreenChains { get; set; } = "default";
    public bool MameFilter { get; set; } = true;
    public bool MameCheat { get; set; }
    public bool MameRewind { get; set; }
    public bool MameNvramSave { get; set; } = true;

    // Mednafen
    public string MednafenVideoDriver { get; set; } = "opengl";
    public bool MednafenFullscreen { get; set; }
    public bool MednafenVsync { get; set; } = true;
    public string MednafenStretch { get; set; } = "aspect";
    public bool MednafenBilinear { get; set; }
    public int MednafenScanlines { get; set; }
    public string MednafenShader { get; set; } = "none";
    public int MednafenVolume { get; set; } = 100;
    public bool MednafenCheats { get; set; } = true;
    public bool MednafenRewind { get; set; }
    public bool MednafenShowSettingsBeforeLaunch { get; set; } = true;

    // Mesen
    public bool MesenFullscreen { get; set; }
    public bool MesenVsync { get; set; }
    public string MesenAspectRatio { get; set; } = "NoStretching";
    public bool MesenBilinear { get; set; }
    public string MesenVideoFilter { get; set; } = "None";
    public bool MesenEnableAudio { get; set; } = true;
    public int MesenMasterVolume { get; set; } = 100;
    public bool MesenRewind { get; set; }
    public int MesenRunAhead { get; set; }
    public bool MesenPauseInBackground { get; set; }
    public bool MesenShowSettingsBeforeLaunch { get; set; } = true;

    // PCSX2
    public bool Pcsx2StartFullscreen { get; set; } = true;
    public string Pcsx2AspectRatio { get; set; } = "16:9"; // 4:3, 16:9, Stretch
    public int Pcsx2Renderer { get; set; } = 14; // 14=Vulkan, 13=D3D12, 12=D3D11, 15=OpenGL, 11=Software
    public int Pcsx2UpscaleMultiplier { get; set; } = 2; // 1 (Native) to 8
    public int Pcsx2Vsync { get; set; } // 0=Off, 1=On
    public bool Pcsx2EnableCheats { get; set; }
    public bool Pcsx2EnableWidescreenPatches { get; set; }
    public int Pcsx2Volume { get; set; } = 100;
    public bool Pcsx2AchievementsEnabled { get; set; }
    public bool Pcsx2AchievementsHardcore { get; set; } = true;
    public bool Pcsx2ShowSettingsBeforeLaunch { get; set; } = true;

    // RetroArch
    public bool RetroArchCheevosEnable { get; set; }
    public bool RetroArchCheevosHardcore { get; set; }
    public bool RetroArchFullscreen { get; set; }
    public bool RetroArchVsync { get; set; } = true;
    public string RetroArchVideoDriver { get; set; } = "gl";
    public bool RetroArchAudioEnable { get; set; } = true;
    public bool RetroArchAudioMute { get; set; }
    public string RetroArchMenuDriver { get; set; } = "ozone";
    public bool RetroArchPauseNonActive { get; set; } = true;
    public bool RetroArchSaveOnExit { get; set; } = true;
    public bool RetroArchAutoSaveState { get; set; }
    public bool RetroArchAutoLoadState { get; set; }
    public bool RetroArchRewind { get; set; }
    public bool RetroArchThreadedVideo { get; set; }
    public bool RetroArchBilinear { get; set; }
    public bool RetroArchShowSettingsBeforeLaunch { get; set; } = true;
    public string RetroArchAspectRatioIndex { get; set; } = "22"; // 22 = Core Provided
    public bool RetroArchScaleInteger { get; set; }
    public bool RetroArchShaderEnable { get; set; } = true;
    public bool RetroArchHardSync { get; set; }
    public bool RetroArchRunAhead { get; set; }
    public bool RetroArchShowAdvancedSettings { get; set; } = true;
    public bool RetroArchDiscordAllow { get; set; }

    // RPCS3
    public string Rpcs3Renderer { get; set; } = "Vulkan";
    public string Rpcs3Resolution { get; set; } = "1280x720";
    public string Rpcs3AspectRatio { get; set; } = "16:9";
    public bool Rpcs3Vsync { get; set; }
    public int Rpcs3ResolutionScale { get; set; } = 100;
    public int Rpcs3AnisotropicFilter { get; set; }
    public string Rpcs3PpuDecoder { get; set; } = "Recompiler (LLVM)";
    public string Rpcs3SpuDecoder { get; set; } = "Recompiler (LLVM)";
    public string Rpcs3AudioRenderer { get; set; } = "Cubeb";
    public bool Rpcs3AudioBuffering { get; set; } = true;
    public bool Rpcs3StartFullscreen { get; set; }
    public bool Rpcs3ShowSettingsBeforeLaunch { get; set; } = true;

    // SEGA Model 2
    public int SegaModel2ResX { get; set; } = 640;
    public int SegaModel2ResY { get; set; } = 480;
    public int SegaModel2WideScreen { get; set; } // 0=4:3, 1=16:9, 2=16:10
    public bool SegaModel2Bilinear { get; set; } = true;
    public bool SegaModel2Trilinear { get; set; }
    public bool SegaModel2FilterTilemaps { get; set; }
    public bool SegaModel2DrawCross { get; set; } = true;
    public int SegaModel2Fsaa { get; set; }
    public bool SegaModel2XInput { get; set; }
    public bool SegaModel2EnableFf { get; set; }
    public bool SegaModel2HoldGears { get; set; }
    public bool SegaModel2UseRawInput { get; set; }
    public bool SegaModel2ShowSettingsBeforeLaunch { get; set; } = true;

    // Stella
    public bool StellaFullscreen { get; set; }
    public bool StellaVsync { get; set; } = true;
    public string StellaVideoDriver { get; set; } = "direct3d";
    public bool StellaCorrectAspect { get; set; } = true;
    public int StellaTvFilter { get; set; }
    public int StellaScanlines { get; set; }
    public bool StellaAudioEnabled { get; set; } = true;
    public int StellaAudioVolume { get; set; } = 80;
    public bool StellaTimeMachine { get; set; } = true;
    public bool StellaConfirmExit { get; set; }
    public bool StellaShowSettingsBeforeLaunch { get; set; } = true;

    // Supermodel
    public bool SupermodelNew3DEngine { get; set; } = true;
    public bool SupermodelQuadRendering { get; set; }
    public bool SupermodelFullscreen { get; set; } = true;
    public int SupermodelResX { get; set; } = 1920;
    public int SupermodelResY { get; set; } = 1080;
    public bool SupermodelWideScreen { get; set; } = true;
    public bool SupermodelStretch { get; set; }
    public bool SupermodelVsync { get; set; } = true;
    public bool SupermodelThrottle { get; set; } = true;
    public int SupermodelMusicVolume { get; set; } = 100;
    public int SupermodelSoundVolume { get; set; } = 100;
    public string SupermodelInputSystem { get; set; } = "xinput";
    public bool SupermodelMultiThreaded { get; set; } = true;
    public int SupermodelPowerPcFrequency { get; set; } = 50;
    public bool SupermodelShowSettingsBeforeLaunch { get; set; } = true;

    // Xenia
    public string XeniaReadbackResolve { get; set; } = "none"; // none, fast, full
    public bool XeniaGammaSrgb { get; set; }
    public bool XeniaVibration { get; set; } = true;
    public bool XeniaMountCache { get; set; } = true;
    public string XeniaGpu { get; set; } = "d3d12"; // d3d12, vulkan, null
    public bool XeniaVsync { get; set; } = true;
    public int XeniaResScaleX { get; set; } = 1;
    public int XeniaResScaleY { get; set; } = 1;
    public bool XeniaFullscreen { get; set; }
    public string XeniaApu { get; set; } = "xaudio2"; // xaudio2, sdl, nop, any
    public bool XeniaMute { get; set; }
    public string XeniaAa { get; set; } = ""; // "", fxaa, fxaa_extreme
    public string XeniaScaling { get; set; } = "fsr"; // fsr, cas, bilinear
    public bool XeniaApplyPatches { get; set; } = true;
    public bool XeniaDiscordPresence { get; set; } = true;
    public int XeniaUserLanguage { get; set; } = 1; // 1=English
    public string XeniaHid { get; set; } = "xinput"; // xinput, sdl, winkey, any
    public bool XeniaShowSettingsBeforeLaunch { get; set; } = true;

    // Yumir
    public bool YumirFullscreen { get; set; }
    public double YumirVolume { get; set; } = 0.8;
    public bool YumirMute { get; set; }
    public string YumirVideoStandard { get; set; } = "PAL"; // PAL, NTSC
    public bool YumirAutoDetectRegion { get; set; } = true;
    public bool YumirPauseWhenUnfocused { get; set; }
    public double YumirForcedAspect { get; set; } = 1.7777777777777777;
    public bool YumirForceAspectRatio { get; set; }
    public bool YumirReduceLatency { get; set; } = true;
    public bool YumirShowSettingsBeforeLaunch { get; set; } = true;

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
        // Application Settings
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

        // Azahar
        AzaharGraphicsApi = other.AzaharGraphicsApi;
        AzaharResolutionFactor = other.AzaharResolutionFactor;
        AzaharUseVsync = other.AzaharUseVsync;
        AzaharAsyncShaderCompilation = other.AzaharAsyncShaderCompilation;
        AzaharFullscreen = other.AzaharFullscreen;
        AzaharVolume = other.AzaharVolume;
        AzaharIsNew3ds = other.AzaharIsNew3ds;
        AzaharLayoutOption = other.AzaharLayoutOption;
        AzaharShowSettingsBeforeLaunch = other.AzaharShowSettingsBeforeLaunch;

        // Blastem
        BlastemFullscreen = other.BlastemFullscreen;
        BlastemVsync = other.BlastemVsync;
        BlastemAspect = other.BlastemAspect;
        BlastemScaling = other.BlastemScaling;
        BlastemScanlines = other.BlastemScanlines;
        BlastemAudioRate = other.BlastemAudioRate;
        BlastemSyncSource = other.BlastemSyncSource;
        BlastemShowSettingsBeforeLaunch = other.BlastemShowSettingsBeforeLaunch;

        // Cemu
        CemuFullscreen = other.CemuFullscreen;
        CemuGraphicApi = other.CemuGraphicApi;
        CemuVsync = other.CemuVsync;
        CemuAsyncCompile = other.CemuAsyncCompile;
        CemuTvVolume = other.CemuTvVolume;
        CemuConsoleLanguage = other.CemuConsoleLanguage;
        CemuDiscordPresence = other.CemuDiscordPresence;
        CemuShowSettingsBeforeLaunch = other.CemuShowSettingsBeforeLaunch;

        // Daphne
        DaphneFullscreen = other.DaphneFullscreen;
        DaphneResX = other.DaphneResX;
        DaphneResY = other.DaphneResY;
        DaphneDisableCrosshairs = other.DaphneDisableCrosshairs;
        DaphneBilinear = other.DaphneBilinear;
        DaphneEnableSound = other.DaphneEnableSound;
        DaphneUseOverlays = other.DaphneUseOverlays;
        DaphneShowSettingsBeforeLaunch = other.DaphneShowSettingsBeforeLaunch;

        // Dolphin
        DolphinGfxBackend = other.DolphinGfxBackend;
        DolphinDspThread = other.DolphinDspThread;
        DolphinWiimoteContinuousScanning = other.DolphinWiimoteContinuousScanning;
        DolphinWiimoteEnableSpeaker = other.DolphinWiimoteEnableSpeaker;
        DolphinShowSettingsBeforeLaunch = other.DolphinShowSettingsBeforeLaunch;

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

        // Flycast
        FlycastFullscreen = other.FlycastFullscreen;
        FlycastWidth = other.FlycastWidth;
        FlycastHeight = other.FlycastHeight;
        FlycastMaximized = other.FlycastMaximized;
        FlycastShowSettingsBeforeLaunch = other.FlycastShowSettingsBeforeLaunch;

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

        // PCSX2
        Pcsx2StartFullscreen = other.Pcsx2StartFullscreen;
        Pcsx2AspectRatio = other.Pcsx2AspectRatio;
        Pcsx2Renderer = other.Pcsx2Renderer;
        Pcsx2UpscaleMultiplier = other.Pcsx2UpscaleMultiplier;
        Pcsx2Vsync = other.Pcsx2Vsync;
        Pcsx2EnableCheats = other.Pcsx2EnableCheats;
        Pcsx2EnableWidescreenPatches = other.Pcsx2EnableWidescreenPatches;
        Pcsx2Volume = other.Pcsx2Volume;
        Pcsx2AchievementsEnabled = other.Pcsx2AchievementsEnabled;
        Pcsx2AchievementsHardcore = other.Pcsx2AchievementsHardcore;
        Pcsx2ShowSettingsBeforeLaunch = other.Pcsx2ShowSettingsBeforeLaunch;

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

        // Stella
        StellaFullscreen = other.StellaFullscreen;
        StellaVsync = other.StellaVsync;
        StellaVideoDriver = other.StellaVideoDriver;
        StellaCorrectAspect = other.StellaCorrectAspect;
        StellaTvFilter = other.StellaTvFilter;
        StellaScanlines = other.StellaScanlines;
        StellaAudioEnabled = other.StellaAudioEnabled;
        StellaAudioVolume = other.StellaAudioVolume;
        StellaTimeMachine = other.StellaTimeMachine;
        StellaConfirmExit = other.StellaConfirmExit;
        StellaShowSettingsBeforeLaunch = other.StellaShowSettingsBeforeLaunch;

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

        // Yumir
        YumirFullscreen = other.YumirFullscreen;
        YumirVolume = other.YumirVolume;
        YumirMute = other.YumirMute;
        YumirVideoStandard = other.YumirVideoStandard;
        YumirAutoDetectRegion = other.YumirAutoDetectRegion;
        YumirPauseWhenUnfocused = other.YumirPauseWhenUnfocused;
        YumirForcedAspect = other.YumirForcedAspect;
        YumirForceAspectRatio = other.YumirForceAspectRatio;
        YumirReduceLatency = other.YumirReduceLatency;
        YumirShowSettingsBeforeLaunch = other.YumirShowSettingsBeforeLaunch;
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

            // Application Settings
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

            // Ares
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

            // Azahar
            AzaharGraphicsApi = 1;
            AzaharResolutionFactor = 1;
            AzaharUseVsync = true;
            AzaharAsyncShaderCompilation = true;
            AzaharFullscreen = true;
            AzaharVolume = 100;
            AzaharIsNew3ds = true;
            AzaharLayoutOption = 0;
            AzaharShowSettingsBeforeLaunch = true;

            // Blastem
            BlastemFullscreen = false;
            BlastemVsync = false;
            BlastemAspect = "4:3";
            BlastemScaling = "linear";
            BlastemScanlines = false;
            BlastemAudioRate = 48000;
            BlastemSyncSource = "audio";
            BlastemShowSettingsBeforeLaunch = true;

            // Cemu
            CemuFullscreen = false;
            CemuGraphicApi = 1;
            CemuVsync = 1;
            CemuAsyncCompile = true;
            CemuTvVolume = 50;
            CemuConsoleLanguage = 1;
            CemuDiscordPresence = true;
            CemuShowSettingsBeforeLaunch = true;

            // Daphne
            DaphneFullscreen = false;
            DaphneResX = 640;
            DaphneResY = 480;
            DaphneDisableCrosshairs = false;
            DaphneBilinear = true;
            DaphneEnableSound = true;
            DaphneUseOverlays = true;
            DaphneShowSettingsBeforeLaunch = true;

            // Dolphin
            DolphinGfxBackend = "Vulkan";
            DolphinDspThread = true;
            DolphinWiimoteContinuousScanning = true;
            DolphinWiimoteEnableSpeaker = true;
            DolphinShowSettingsBeforeLaunch = true;

            // DuckStation
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

            // Flycast
            FlycastFullscreen = false;
            FlycastWidth = 640;
            FlycastHeight = 480;
            FlycastMaximized = false;
            FlycastShowSettingsBeforeLaunch = true;

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

            // Mednafen
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

            // Mesen
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

            // PCSX2
            Pcsx2StartFullscreen = true;
            Pcsx2AspectRatio = "16:9";
            Pcsx2Renderer = 14;
            Pcsx2UpscaleMultiplier = 2;
            Pcsx2Vsync = 0;
            Pcsx2EnableCheats = false;
            Pcsx2EnableWidescreenPatches = false;
            Pcsx2Volume = 100;
            Pcsx2AchievementsEnabled = false;
            Pcsx2AchievementsHardcore = true;
            Pcsx2ShowSettingsBeforeLaunch = true;

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

            // RPCS3
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

            // Sega Model 2
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

            // Stella
            StellaFullscreen = false;
            StellaVsync = true;
            StellaVideoDriver = "direct3d";
            StellaCorrectAspect = true;
            StellaTvFilter = 0;
            StellaScanlines = 0;
            StellaAudioEnabled = true;
            StellaAudioVolume = 80;
            StellaTimeMachine = true;
            StellaConfirmExit = false;
            StellaShowSettingsBeforeLaunch = true;

            // Supermodel
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

            // Yumir
            YumirFullscreen = false;
            YumirVolume = 0.8;
            YumirMute = false;
            YumirVideoStandard = "PAL";
            YumirAutoDetectRegion = true;
            YumirPauseWhenUnfocused = false;
            YumirForcedAspect = 1.7777777777777777;
            YumirForceAspectRatio = false;
            YumirReduceLatency = true;
            YumirShowSettingsBeforeLaunch = true;

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
        // Application Settings
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

        // Ares
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

        // Azahar
        AzaharGraphicsApi = 1;
        AzaharResolutionFactor = 1;
        AzaharUseVsync = true;
        AzaharAsyncShaderCompilation = true;
        AzaharFullscreen = true;
        AzaharVolume = 100;
        AzaharIsNew3ds = true;
        AzaharLayoutOption = 0;
        AzaharShowSettingsBeforeLaunch = true;

        // Blastem
        BlastemFullscreen = false;
        BlastemVsync = false;
        BlastemAspect = "4:3";
        BlastemScaling = "linear";
        BlastemScanlines = false;
        BlastemAudioRate = 48000;
        BlastemSyncSource = "audio";
        BlastemShowSettingsBeforeLaunch = true;

        // Cemu
        CemuFullscreen = false;
        CemuGraphicApi = 1;
        CemuVsync = 1;
        CemuAsyncCompile = true;
        CemuTvVolume = 50;
        CemuConsoleLanguage = 1;
        CemuDiscordPresence = true;
        CemuShowSettingsBeforeLaunch = true;

        // Daphne
        DaphneFullscreen = false;
        DaphneResX = 640;
        DaphneResY = 480;
        DaphneDisableCrosshairs = false;
        DaphneBilinear = true;
        DaphneEnableSound = true;
        DaphneUseOverlays = true;
        DaphneShowSettingsBeforeLaunch = true;

        // Dolphin
        DolphinGfxBackend = "Vulkan";
        DolphinDspThread = true;
        DolphinWiimoteContinuousScanning = true;
        DolphinWiimoteEnableSpeaker = true;
        DolphinShowSettingsBeforeLaunch = true;

        // DuckStation
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

        // Flycast
        FlycastFullscreen = false;
        FlycastWidth = 640;
        FlycastHeight = 480;
        FlycastMaximized = false;
        FlycastShowSettingsBeforeLaunch = true;

        // MAME
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

        // Mednafen
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

        // Mesen
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

        // PCSX2
        Pcsx2StartFullscreen = true;
        Pcsx2AspectRatio = "16:9";
        Pcsx2Renderer = 14;
        Pcsx2UpscaleMultiplier = 2;
        Pcsx2Vsync = 0;
        Pcsx2EnableCheats = false;
        Pcsx2EnableWidescreenPatches = false;
        Pcsx2Volume = 100;
        Pcsx2AchievementsEnabled = false;
        Pcsx2AchievementsHardcore = true;
        Pcsx2ShowSettingsBeforeLaunch = true;

        // RPCS3
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

        // Sega Model 2
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

        // Stella
        StellaFullscreen = false;
        StellaVsync = true;
        StellaVideoDriver = "direct3d";
        StellaCorrectAspect = true;
        StellaTvFilter = 0;
        StellaScanlines = 0;
        StellaAudioEnabled = true;
        StellaAudioVolume = 80;
        StellaTimeMachine = true;
        StellaConfirmExit = false;
        StellaShowSettingsBeforeLaunch = true;

        // Supermodel
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

        // RetroArch
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

        // Xenia
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

        // Yumir
        YumirFullscreen = false;
        YumirVolume = 0.8;
        YumirMute = false;
        YumirVideoStandard = "PAL";
        YumirAutoDetectRegion = true;
        YumirPauseWhenUnfocused = false;
        YumirForcedAspect = 1.7777777777777777;
        YumirForceAspectRatio = false;
        YumirReduceLatency = true;
        YumirShowSettingsBeforeLaunch = true;

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
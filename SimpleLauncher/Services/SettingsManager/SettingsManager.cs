using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher.Services.SettingsManager;

public class SettingsManager
{
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultSettingsFilePath);
    private readonly object _saveLock = new();
    private readonly HashSet<int> _validThumbnailSizes = [50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800];
    private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500, 1000, 10000, 1000000];
    private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
    private readonly HashSet<string> _validViewModes = ["GridView", "ListView"];
    private readonly HashSet<string> _validButtonAspectRatio = ["Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2"];

    // Application Settings
    public int ThumbnailSize { get; set; } = 250;
    public int GamesPerPage { get; set; } = 200;
    public string ShowGames { get; set; } = "ShowAll";
    public string ViewMode { get; set; } = "GridView";
    public bool EnableGamePadNavigation { get; set; }
    public string VideoUrl { get; set; } = App.Configuration?["Urls:YouTubeSearch"] ?? "https://www.youtube.com/results?search_query=";
    public string InfoUrl { get; set; } = App.Configuration?["Urls:IgdbSearch"] ?? "https://www.igdb.com/search?q=";
    public string BaseTheme { get; set; } = "Light";
    public string AccentColor { get; set; } = "Blue";
    public string Language { get; set; } = "en";
    public float DeadZoneX { get; set; } = DefaultDeadZoneX;
    public float DeadZoneY { get; set; } = DefaultDeadZoneY;
    public string ButtonAspectRatio { get; set; } = "Square";
    public bool EnableFuzzyMatching { get; set; } = true;
    public double FuzzyMatchingThreshold { get; set; } = 0.80;
    public const float DefaultDeadZoneX = 0.05f;
    public const float DefaultDeadZoneY = 0.02f;
    public bool EnableNotificationSound { get; set; } = true;
    public string CustomNotificationSoundFile { get; set; } = DefaultNotificationSoundFileName;
    public string RaUsername { get; set; } = string.Empty;
    public string RaApiKey { get; set; } = string.Empty;
    public string RaPassword { get; set; } = string.Empty;
    public string RaToken { get; set; } = string.Empty;
    public bool OverlayRetroAchievementButton { get; set; }
    public bool OverlayOpenVideoButton { get; set; } = true;
    public bool OverlayOpenInfoButton { get; set; }
    public bool AdditionalSystemFoldersExpanded { get; set; } = true;
    public bool Emulator1Expanded { get; set; } = true;
    public bool Emulator2Expanded { get; set; } = true;
    public bool Emulator3Expanded { get; set; } = true;
    public bool Emulator4Expanded { get; set; } = true;
    public bool Emulator5Expanded { get; set; } = true;
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
    public int AzaharGraphicsApi { get; set; } = 1; // 1=Vulkan
    public int AzaharResolutionFactor { get; set; } = 1; // 0=Auto, 1=1x, 2=2x...
    public bool AzaharUseVsync { get; set; } = true;
    public bool AzaharAsyncShaderCompilation { get; set; } = true;
    public bool AzaharFullscreen { get; set; } = true;
    public int AzaharVolume { get; set; } = 100;
    public bool AzaharIsNew3ds { get; set; } = true;
    public int AzaharLayoutOption { get; set; } // 0=Default, 1=Single, 2=Large...
    public bool AzaharShowSettingsBeforeLaunch { get; set; } = true;
    public bool AzaharEnableAudioStretching { get; set; } = true;

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
    public bool Pcsx2Vsync { get; set; } // false=Off, true=On
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
    public bool RetroArchOverrideSystemDir { get; set; }
    public bool RetroArchOverrideSaveDir { get; set; }
    public bool RetroArchOverrideStateDir { get; set; }
    public bool RetroArchOverrideScreenshotDir { get; set; }

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

    private const string DefaultSettingsFilePath = "settings.xml";
    private const string DefaultNotificationSoundFileName = "click.mp3";

    public void Load()
    {
        lock (_saveLock)
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var settings = XElement.Load(_filePath);
                    LoadFromXml(settings);
                    return;
                }
                catch (Exception ex)
                {
                    App.ServiceProvider?.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading settings.xml.");
                }
            }

            SetDefaultsAndSave();
        }
    }

    private void CopyFrom(SettingsManager other)
    {
        ArgumentNullException.ThrowIfNull(other);

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
        AzaharEnableAudioStretching = other.AzaharEnableAudioStretching;

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
        RetroArchOverrideSystemDir = other.RetroArchOverrideSystemDir;
        RetroArchOverrideSaveDir = other.RetroArchOverrideSaveDir;
        RetroArchOverrideStateDir = other.RetroArchOverrideStateDir;
        RetroArchOverrideScreenshotDir = other.RetroArchOverrideScreenshotDir;

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

    private void LoadFromXml(XElement settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        // Application Settings
        var app = settings.Element("Application");
        if (app != null)
        {
            ThumbnailSize = ValidateThumbnailSize(app.Element("ThumbnailSize")?.Value);
            GamesPerPage = ValidateGamesPerPage(app.Element("GamesPerPage")?.Value);
            ShowGames = ValidateShowGames(app.Element("ShowGames")?.Value);
            ViewMode = ValidateViewMode(app.Element("ViewMode")?.Value);
            if (bool.TryParse(app.Element("EnableGamePadNavigation")?.Value, out var gp))
            {
                EnableGamePadNavigation = gp;
            }

            VideoUrl = app.Element("VideoUrl")?.Value ?? VideoUrl;
            InfoUrl = app.Element("InfoUrl")?.Value ?? InfoUrl;
            BaseTheme = app.Element("BaseTheme")?.Value ?? BaseTheme;
            AccentColor = app.Element("AccentColor")?.Value ?? AccentColor;
            Language = app.Element("Language")?.Value ?? Language;
            ButtonAspectRatio = ValidateButtonAspectRatio(app.Element("ButtonAspectRatio")?.Value);
            RaUsername = app.Element("RaUsername")?.Value ?? RaUsername;
            RaApiKey = app.Element("RaApiKey")?.Value ?? RaApiKey;
            RaPassword = app.Element("RaPassword")?.Value ?? RaPassword;
            RaToken = app.Element("RaToken")?.Value ?? RaToken;
            if (float.TryParse(app.Element("DeadZoneX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzx))
            {
                DeadZoneX = dzx;
            }

            if (float.TryParse(app.Element("DeadZoneY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzy))
            {
                DeadZoneY = dzy;
            }

            if (bool.TryParse(app.Element("EnableFuzzyMatching")?.Value, out var fm))
            {
                EnableFuzzyMatching = fm;
            }

            if (double.TryParse(app.Element("FuzzyMatchingThreshold")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fmt))
            {
                FuzzyMatchingThreshold = fmt;
            }

            if (bool.TryParse(app.Element("EnableNotificationSound")?.Value, out var ens))
            {
                EnableNotificationSound = ens;
            }

            CustomNotificationSoundFile = app.Element("CustomNotificationSoundFile")?.Value ?? CustomNotificationSoundFile;
            if (bool.TryParse(app.Element("OverlayRetroAchievementButton")?.Value, out var ora))
            {
                OverlayRetroAchievementButton = ora;
            }

            if (bool.TryParse(app.Element("OverlayOpenVideoButton")?.Value, out var ovb))
            {
                OverlayOpenVideoButton = ovb;
            }

            if (bool.TryParse(app.Element("OverlayOpenInfoButton")?.Value, out var oib))
            {
                OverlayOpenInfoButton = oib;
            }

            if (bool.TryParse(app.Element("AdditionalSystemFoldersExpanded")?.Value, out var asfe))
            {
                AdditionalSystemFoldersExpanded = asfe;
            }

            if (bool.TryParse(app.Element("Emulator1Expanded")?.Value, out var e1E))
            {
                Emulator1Expanded = e1E;
            }

            if (bool.TryParse(app.Element("Emulator2Expanded")?.Value, out var e2E))
            {
                Emulator2Expanded = e2E;
            }

            if (bool.TryParse(app.Element("Emulator3Expanded")?.Value, out var e3E))
            {
                Emulator3Expanded = e3E;
            }

            if (bool.TryParse(app.Element("Emulator4Expanded")?.Value, out var e4E))
            {
                Emulator4Expanded = e4E;
            }

            if (bool.TryParse(app.Element("Emulator5Expanded")?.Value, out var e5E))
            {
                Emulator5Expanded = e5E;
            }
        }

        // Ares
        var ares = settings.Element("Ares");
        if (ares != null)
        {
            AresVideoDriver = ares.Element("VideoDriver")?.Value ?? AresVideoDriver;
            if (bool.TryParse(ares.Element("Exclusive")?.Value, out var ae))
            {
                AresExclusive = ae;
            }

            AresShader = ares.Element("Shader")?.Value ?? AresShader;
            if (int.TryParse(ares.Element("Multiplier")?.Value, out var am))
            {
                AresMultiplier = am;
            }

            AresAspectCorrection = ares.Element("AspectCorrection")?.Value ?? AresAspectCorrection;
            if (bool.TryParse(ares.Element("Mute")?.Value, out var amu))
            {
                AresMute = amu;
            }

            if (double.TryParse(ares.Element("Volume")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var av))
            {
                AresVolume = av;
            }

            if (bool.TryParse(ares.Element("FastBoot")?.Value, out var afb))
            {
                AresFastBoot = afb;
            }

            if (bool.TryParse(ares.Element("Rewind")?.Value, out var ar))
            {
                AresRewind = ar;
            }

            if (bool.TryParse(ares.Element("RunAhead")?.Value, out var ara))
            {
                AresRunAhead = ara;
            }

            if (bool.TryParse(ares.Element("AutoSaveMemory")?.Value, out var asm))
            {
                AresAutoSaveMemory = asm;
            }

            if (bool.TryParse(ares.Element("ShowSettingsBeforeLaunch")?.Value, out var assbl))
            {
                AresShowSettingsBeforeLaunch = assbl;
            }
        }

        // Azahar
        var azahar = settings.Element("Azahar");
        if (azahar != null)
        {
            if (int.TryParse(azahar.Element("GraphicsApi")?.Value, out var aga))
            {
                AzaharGraphicsApi = aga;
            }

            if (int.TryParse(azahar.Element("ResolutionFactor")?.Value, out var arf))
            {
                AzaharResolutionFactor = arf;
            }

            if (bool.TryParse(azahar.Element("UseVsync")?.Value, out var auv))
            {
                AzaharUseVsync = auv;
            }

            if (bool.TryParse(azahar.Element("AsyncShaderCompilation")?.Value, out var aasc))
            {
                AzaharAsyncShaderCompilation = aasc;
            }

            if (bool.TryParse(azahar.Element("Fullscreen")?.Value, out var af))
            {
                AzaharFullscreen = af;
            }

            if (int.TryParse(azahar.Element("Volume")?.Value, out var avol))
            {
                AzaharVolume = avol;
            }

            if (bool.TryParse(azahar.Element("IsNew3ds")?.Value, out var ain))
            {
                AzaharIsNew3ds = ain;
            }

            if (int.TryParse(azahar.Element("LayoutOption")?.Value, out var alo))
            {
                AzaharLayoutOption = alo;
            }

            if (bool.TryParse(azahar.Element("ShowSettingsBeforeLaunch")?.Value, out var asbl))
            {
                AzaharShowSettingsBeforeLaunch = asbl;
            }

            if (bool.TryParse(azahar.Element("EnableAudioStretching")?.Value, out var aeas))
            {
                AzaharEnableAudioStretching = aeas;
            }
        }

        // Blastem
        var blastem = settings.Element("Blastem");
        if (blastem != null)
        {
            if (bool.TryParse(blastem.Element("Fullscreen")?.Value, out var bf))
            {
                BlastemFullscreen = bf;
            }

            if (bool.TryParse(blastem.Element("Vsync")?.Value, out var bv))
            {
                BlastemVsync = bv;
            }

            BlastemAspect = blastem.Element("Aspect")?.Value ?? BlastemAspect;
            BlastemScaling = blastem.Element("Scaling")?.Value ?? BlastemScaling;
            if (bool.TryParse(blastem.Element("Scanlines")?.Value, out var bs))
            {
                BlastemScanlines = bs;
            }

            if (int.TryParse(blastem.Element("AudioRate")?.Value, out var bar))
            {
                BlastemAudioRate = bar;
            }

            BlastemSyncSource = blastem.Element("SyncSource")?.Value ?? BlastemSyncSource;
            if (bool.TryParse(blastem.Element("ShowSettingsBeforeLaunch")?.Value, out var bssbl))
            {
                BlastemShowSettingsBeforeLaunch = bssbl;
            }
        }

        // Cemu
        var cemu = settings.Element("Cemu");
        if (cemu != null)
        {
            if (bool.TryParse(cemu.Element("Fullscreen")?.Value, out var cf))
            {
                CemuFullscreen = cf;
            }

            if (int.TryParse(cemu.Element("GraphicApi")?.Value, out var cga))
            {
                CemuGraphicApi = cga;
            }

            if (int.TryParse(cemu.Element("Vsync")?.Value, out var cv))
            {
                CemuVsync = cv;
            }

            if (bool.TryParse(cemu.Element("AsyncCompile")?.Value, out var cac))
            {
                CemuAsyncCompile = cac;
            }

            if (int.TryParse(cemu.Element("TvVolume")?.Value, out var ctv))
            {
                CemuTvVolume = ctv;
            }

            if (int.TryParse(cemu.Element("ConsoleLanguage")?.Value, out var ccl))
            {
                CemuConsoleLanguage = ccl;
            }

            if (bool.TryParse(cemu.Element("DiscordPresence")?.Value, out var cdp))
            {
                CemuDiscordPresence = cdp;
            }

            if (bool.TryParse(cemu.Element("ShowSettingsBeforeLaunch")?.Value, out var cssbl))
            {
                CemuShowSettingsBeforeLaunch = cssbl;
            }
        }

        // Daphne
        var daphne = settings.Element("Daphne");
        if (daphne != null)
        {
            if (bool.TryParse(daphne.Element("Fullscreen")?.Value, out var df))
            {
                DaphneFullscreen = df;
            }

            if (int.TryParse(daphne.Element("ResX")?.Value, out var drx))
            {
                DaphneResX = drx;
            }

            if (int.TryParse(daphne.Element("ResY")?.Value, out var dry))
            {
                DaphneResY = dry;
            }

            if (bool.TryParse(daphne.Element("DisableCrosshairs")?.Value, out var ddc))
            {
                DaphneDisableCrosshairs = ddc;
            }

            if (bool.TryParse(daphne.Element("Bilinear")?.Value, out var db))
            {
                DaphneBilinear = db;
            }

            if (bool.TryParse(daphne.Element("EnableSound")?.Value, out var des))
            {
                DaphneEnableSound = des;
            }

            if (bool.TryParse(daphne.Element("UseOverlays")?.Value, out var duo))
            {
                DaphneUseOverlays = duo;
            }

            if (bool.TryParse(daphne.Element("ShowSettingsBeforeLaunch")?.Value, out var dssbl))
            {
                DaphneShowSettingsBeforeLaunch = dssbl;
            }
        }

        // Dolphin
        var dolphin = settings.Element("Dolphin");
        if (dolphin != null)
        {
            DolphinGfxBackend = dolphin.Element("GfxBackend")?.Value ?? DolphinGfxBackend;
            if (bool.TryParse(dolphin.Element("DspThread")?.Value, out var ddt))
            {
                DolphinDspThread = ddt;
            }

            if (bool.TryParse(dolphin.Element("WiimoteContinuousScanning")?.Value, out var dwcs))
            {
                DolphinWiimoteContinuousScanning = dwcs;
            }

            if (bool.TryParse(dolphin.Element("WiimoteEnableSpeaker")?.Value, out var dwes))
            {
                DolphinWiimoteEnableSpeaker = dwes;
            }

            if (bool.TryParse(dolphin.Element("ShowSettingsBeforeLaunch")?.Value, out var dssbl2))
            {
                DolphinShowSettingsBeforeLaunch = dssbl2;
            }
        }

        // DuckStation
        var duckstation = settings.Element("DuckStation");
        if (duckstation != null)
        {
            if (bool.TryParse(duckstation.Element("StartFullscreen")?.Value, out var dssf))
            {
                DuckStationStartFullscreen = dssf;
            }

            if (bool.TryParse(duckstation.Element("PauseOnFocusLoss")?.Value, out var dpofl))
            {
                DuckStationPauseOnFocusLoss = dpofl;
            }

            if (bool.TryParse(duckstation.Element("SaveStateOnExit")?.Value, out var dssoe))
            {
                DuckStationSaveStateOnExit = dssoe;
            }

            if (bool.TryParse(duckstation.Element("RewindEnable")?.Value, out var dre))
            {
                DuckStationRewindEnable = dre;
            }

            if (int.TryParse(duckstation.Element("RunaheadFrameCount")?.Value, out var drfc))
            {
                DuckStationRunaheadFrameCount = drfc;
            }

            DuckStationRenderer = duckstation.Element("Renderer")?.Value ?? DuckStationRenderer;
            if (int.TryParse(duckstation.Element("ResolutionScale")?.Value, out var drs))
            {
                DuckStationResolutionScale = drs;
            }

            DuckStationTextureFilter = duckstation.Element("TextureFilter")?.Value ?? DuckStationTextureFilter;
            if (bool.TryParse(duckstation.Element("WidescreenHack")?.Value, out var dwh))
            {
                DuckStationWidescreenHack = dwh;
            }

            if (bool.TryParse(duckstation.Element("PgxpEnable")?.Value, out var dpe))
            {
                DuckStationPgxpEnable = dpe;
            }

            DuckStationAspectRatio = duckstation.Element("AspectRatio")?.Value ?? DuckStationAspectRatio;
            if (bool.TryParse(duckstation.Element("Vsync")?.Value, out var dsv))
            {
                DuckStationVsync = dsv;
            }

            if (int.TryParse(duckstation.Element("OutputVolume")?.Value, out var dov))
            {
                DuckStationOutputVolume = dov;
            }

            if (bool.TryParse(duckstation.Element("OutputMuted")?.Value, out var dom))
            {
                DuckStationOutputMuted = dom;
            }

            if (bool.TryParse(duckstation.Element("ShowSettingsBeforeLaunch")?.Value, out var dssbl3))
            {
                DuckStationShowSettingsBeforeLaunch = dssbl3;
            }
        }

        // Flycast
        var flycast = settings.Element("Flycast");
        if (flycast != null)
        {
            if (bool.TryParse(flycast.Element("Fullscreen")?.Value, out var ff))
            {
                FlycastFullscreen = ff;
            }

            if (int.TryParse(flycast.Element("Width")?.Value, out var fw))
            {
                FlycastWidth = fw;
            }

            if (int.TryParse(flycast.Element("Height")?.Value, out var fh))
            {
                FlycastHeight = fh;
            }

            if (bool.TryParse(flycast.Element("Maximized")?.Value, out var flycastMaximized))
            {
                FlycastMaximized = flycastMaximized;
            }

            if (bool.TryParse(flycast.Element("ShowSettingsBeforeLaunch")?.Value, out var fssbl))
            {
                FlycastShowSettingsBeforeLaunch = fssbl;
            }
        }

        // MAME
        var mame = settings.Element("Mame");
        if (mame != null)
        {
            MameVideo = mame.Element("Video")?.Value ?? MameVideo;
            if (bool.TryParse(mame.Element("Window")?.Value, out var mw))
            {
                MameWindow = mw;
            }

            if (bool.TryParse(mame.Element("Maximize")?.Value, out var mm))
            {
                MameMaximize = mm;
            }

            if (bool.TryParse(mame.Element("KeepAspect")?.Value, out var mka))
            {
                MameKeepAspect = mka;
            }

            if (bool.TryParse(mame.Element("SkipGameInfo")?.Value, out var msgi))
            {
                MameSkipGameInfo = msgi;
            }

            if (bool.TryParse(mame.Element("Autosave")?.Value, out var mas))
            {
                MameAutosave = mas;
            }

            if (bool.TryParse(mame.Element("ConfirmQuit")?.Value, out var mcq))
            {
                MameConfirmQuit = mcq;
            }

            if (bool.TryParse(mame.Element("Joystick")?.Value, out var mj))
            {
                MameJoystick = mj;
            }

            if (bool.TryParse(mame.Element("ShowSettingsBeforeLaunch")?.Value, out var mssbl))
            {
                MameShowSettingsBeforeLaunch = mssbl;
            }

            if (bool.TryParse(mame.Element("Autoframeskip")?.Value, out var maf))
            {
                MameAutoframeskip = maf;
            }

            MameBgfxBackend = mame.Element("BgfxBackend")?.Value ?? MameBgfxBackend;
            MameBgfxScreenChains = mame.Element("BgfxScreenChains")?.Value ?? MameBgfxScreenChains;
            if (bool.TryParse(mame.Element("Filter")?.Value, out var mf))
            {
                MameFilter = mf;
            }

            if (bool.TryParse(mame.Element("Cheat")?.Value, out var mc))
            {
                MameCheat = mc;
            }

            if (bool.TryParse(mame.Element("Rewind")?.Value, out var mr))
            {
                MameRewind = mr;
            }

            if (bool.TryParse(mame.Element("NvramSave")?.Value, out var mns))
            {
                MameNvramSave = mns;
            }
        }

        // Mednafen
        var mednafen = settings.Element("Mednafen");
        if (mednafen != null)
        {
            MednafenVideoDriver = mednafen.Element("VideoDriver")?.Value ?? MednafenVideoDriver;
            if (bool.TryParse(mednafen.Element("Fullscreen")?.Value, out var mef))
            {
                MednafenFullscreen = mef;
            }

            if (bool.TryParse(mednafen.Element("Vsync")?.Value, out var mev))
            {
                MednafenVsync = mev;
            }

            MednafenStretch = mednafen.Element("Stretch")?.Value ?? MednafenStretch;
            if (bool.TryParse(mednafen.Element("Bilinear")?.Value, out var meb))
            {
                MednafenBilinear = meb;
            }

            if (int.TryParse(mednafen.Element("Scanlines")?.Value, out var mes))
            {
                MednafenScanlines = mes;
            }

            MednafenShader = mednafen.Element("Shader")?.Value ?? MednafenShader;
            if (int.TryParse(mednafen.Element("Volume")?.Value, out var mevo))
            {
                MednafenVolume = mevo;
            }

            if (bool.TryParse(mednafen.Element("Cheats")?.Value, out var mec))
            {
                MednafenCheats = mec;
            }

            if (bool.TryParse(mednafen.Element("Rewind")?.Value, out var mer))
            {
                MednafenRewind = mer;
            }

            if (bool.TryParse(mednafen.Element("ShowSettingsBeforeLaunch")?.Value, out var messbl))
            {
                MednafenShowSettingsBeforeLaunch = messbl;
            }
        }

        // Mesen
        var mesen = settings.Element("Mesen");
        if (mesen != null)
        {
            if (bool.TryParse(mesen.Element("Fullscreen")?.Value, out var msnf))
            {
                MesenFullscreen = msnf;
            }

            if (bool.TryParse(mesen.Element("Vsync")?.Value, out var msnv))
            {
                MesenVsync = msnv;
            }

            MesenAspectRatio = mesen.Element("AspectRatio")?.Value ?? MesenAspectRatio;
            if (bool.TryParse(mesen.Element("Bilinear")?.Value, out var msnb))
            {
                MesenBilinear = msnb;
            }

            MesenVideoFilter = mesen.Element("VideoFilter")?.Value ?? MesenVideoFilter;
            if (bool.TryParse(mesen.Element("EnableAudio")?.Value, out var msnbea))
            {
                MesenEnableAudio = msnbea;
            }

            if (int.TryParse(mesen.Element("MasterVolume")?.Value, out var msnmv))
            {
                MesenMasterVolume = msnmv;
            }

            if (bool.TryParse(mesen.Element("Rewind")?.Value, out var msnr))
            {
                MesenRewind = msnr;
            }

            if (int.TryParse(mesen.Element("RunAhead")?.Value, out var msnra))
            {
                MesenRunAhead = msnra;
            }

            if (bool.TryParse(mesen.Element("PauseInBackground")?.Value, out var msnpib))
            {
                MesenPauseInBackground = msnpib;
            }

            if (bool.TryParse(mesen.Element("ShowSettingsBeforeLaunch")?.Value, out var msnssbl))
            {
                MesenShowSettingsBeforeLaunch = msnssbl;
            }
        }

        // PCSX2
        var pcsx2 = settings.Element("Pcsx2");
        if (pcsx2 != null)
        {
            if (bool.TryParse(pcsx2.Element("StartFullscreen")?.Value, out var psf))
            {
                Pcsx2StartFullscreen = psf;
            }

            Pcsx2AspectRatio = pcsx2.Element("AspectRatio")?.Value ?? Pcsx2AspectRatio;
            if (int.TryParse(pcsx2.Element("Renderer")?.Value, out var pr))
            {
                Pcsx2Renderer = pr;
            }

            if (int.TryParse(pcsx2.Element("UpscaleMultiplier")?.Value, out var pum))
            {
                Pcsx2UpscaleMultiplier = pum;
            }

            if (bool.TryParse(pcsx2.Element("Vsync")?.Value, out var pv))
            {
                Pcsx2Vsync = pv;
            }

            if (bool.TryParse(pcsx2.Element("EnableCheats")?.Value, out var pec))
            {
                Pcsx2EnableCheats = pec;
            }

            if (bool.TryParse(pcsx2.Element("EnableWidescreenPatches")?.Value, out var pewp))
            {
                Pcsx2EnableWidescreenPatches = pewp;
            }

            if (int.TryParse(pcsx2.Element("Volume")?.Value, out var pvol))
            {
                Pcsx2Volume = pvol;
            }

            if (bool.TryParse(pcsx2.Element("AchievementsEnabled")?.Value, out var pae))
            {
                Pcsx2AchievementsEnabled = pae;
            }

            if (bool.TryParse(pcsx2.Element("AchievementsHardcore")?.Value, out var pah))
            {
                Pcsx2AchievementsHardcore = pah;
            }

            if (bool.TryParse(pcsx2.Element("ShowSettingsBeforeLaunch")?.Value, out var pssbl))
            {
                Pcsx2ShowSettingsBeforeLaunch = pssbl;
            }
        }

        // RetroArch
        var retroarch = settings.Element("RetroArch");
        if (retroarch != null)
        {
            if (bool.TryParse(retroarch.Element("CheevosEnable")?.Value, out var race))
            {
                RetroArchCheevosEnable = race;
            }

            if (bool.TryParse(retroarch.Element("CheevosHardcore")?.Value, out var rach))
            {
                RetroArchCheevosHardcore = rach;
            }

            if (bool.TryParse(retroarch.Element("Fullscreen")?.Value, out var raf))
            {
                RetroArchFullscreen = raf;
            }

            if (bool.TryParse(retroarch.Element("Vsync")?.Value, out var rav))
            {
                RetroArchVsync = rav;
            }

            RetroArchVideoDriver = retroarch.Element("VideoDriver")?.Value ?? RetroArchVideoDriver;
            if (bool.TryParse(retroarch.Element("AudioEnable")?.Value, out var raae))
            {
                RetroArchAudioEnable = raae;
            }

            if (bool.TryParse(retroarch.Element("AudioMute")?.Value, out var raam))
            {
                RetroArchAudioMute = raam;
            }

            RetroArchMenuDriver = retroarch.Element("MenuDriver")?.Value ?? RetroArchMenuDriver;
            if (bool.TryParse(retroarch.Element("PauseNonActive")?.Value, out var rapna))
            {
                RetroArchPauseNonActive = rapna;
            }

            if (bool.TryParse(retroarch.Element("SaveOnExit")?.Value, out var rasoe))
            {
                RetroArchSaveOnExit = rasoe;
            }

            if (bool.TryParse(retroarch.Element("AutoSaveState")?.Value, out var raass))
            {
                RetroArchAutoSaveState = raass;
            }

            if (bool.TryParse(retroarch.Element("AutoLoadState")?.Value, out var raals))
            {
                RetroArchAutoLoadState = raals;
            }

            if (bool.TryParse(retroarch.Element("Rewind")?.Value, out var rar))
            {
                RetroArchRewind = rar;
            }

            if (bool.TryParse(retroarch.Element("ThreadedVideo")?.Value, out var ratv))
            {
                RetroArchThreadedVideo = ratv;
            }

            if (bool.TryParse(retroarch.Element("Bilinear")?.Value, out var rab))
            {
                RetroArchBilinear = rab;
            }

            if (bool.TryParse(retroarch.Element("ShowSettingsBeforeLaunch")?.Value, out var rassbl))
            {
                RetroArchShowSettingsBeforeLaunch = rassbl;
            }

            RetroArchAspectRatioIndex = retroarch.Element("AspectRatioIndex")?.Value ?? RetroArchAspectRatioIndex;
            if (bool.TryParse(retroarch.Element("ScaleInteger")?.Value, out var rasi))
            {
                RetroArchScaleInteger = rasi;
            }

            if (bool.TryParse(retroarch.Element("ShaderEnable")?.Value, out var rase))
            {
                RetroArchShaderEnable = rase;
            }

            if (bool.TryParse(retroarch.Element("HardSync")?.Value, out var rahs))
            {
                RetroArchHardSync = rahs;
            }

            if (bool.TryParse(retroarch.Element("RunAhead")?.Value, out var rara))
            {
                RetroArchRunAhead = rara;
            }

            if (bool.TryParse(retroarch.Element("ShowAdvancedSettings")?.Value, out var rasas))
            {
                RetroArchShowAdvancedSettings = rasas;
            }

            if (bool.TryParse(retroarch.Element("DiscordAllow")?.Value, out var rada))
            {
                RetroArchDiscordAllow = rada;
            }

            if (bool.TryParse(retroarch.Element("OverrideSystemDir")?.Value, out var raosd))
            {
                RetroArchOverrideSystemDir = raosd;
            }

            if (bool.TryParse(retroarch.Element("OverrideSaveDir")?.Value, out var raovsd))
            {
                RetroArchOverrideSaveDir = raovsd;
            }

            if (bool.TryParse(retroarch.Element("OverrideStateDir")?.Value, out var raovstd))
            {
                RetroArchOverrideStateDir = raovstd;
            }

            if (bool.TryParse(retroarch.Element("OverrideScreenshotDir")?.Value, out var raovscd))
            {
                RetroArchOverrideScreenshotDir = raovscd;
            }
        }

        // RPCS3
        var rpcs3 = settings.Element("Rpcs3");
        if (rpcs3 != null)
        {
            Rpcs3Renderer = rpcs3.Element("Renderer")?.Value ?? Rpcs3Renderer;
            Rpcs3Resolution = rpcs3.Element("Resolution")?.Value ?? Rpcs3Resolution;
            Rpcs3AspectRatio = rpcs3.Element("AspectRatio")?.Value ?? Rpcs3AspectRatio;
            if (bool.TryParse(rpcs3.Element("Vsync")?.Value, out var rv))
            {
                Rpcs3Vsync = rv;
            }

            if (int.TryParse(rpcs3.Element("ResolutionScale")?.Value, out var rrs))
            {
                Rpcs3ResolutionScale = rrs;
            }

            if (int.TryParse(rpcs3.Element("AnisotropicFilter")?.Value, out var raf2))
            {
                Rpcs3AnisotropicFilter = raf2;
            }

            Rpcs3PpuDecoder = rpcs3.Element("PpuDecoder")?.Value ?? Rpcs3PpuDecoder;
            Rpcs3SpuDecoder = rpcs3.Element("SpuDecoder")?.Value ?? Rpcs3SpuDecoder;
            Rpcs3AudioRenderer = rpcs3.Element("AudioRenderer")?.Value ?? Rpcs3AudioRenderer;
            if (bool.TryParse(rpcs3.Element("AudioBuffering")?.Value, out var rabuf))
            {
                Rpcs3AudioBuffering = rabuf;
            }

            if (bool.TryParse(rpcs3.Element("StartFullscreen")?.Value, out var rsf))
            {
                Rpcs3StartFullscreen = rsf;
            }

            if (bool.TryParse(rpcs3.Element("ShowSettingsBeforeLaunch")?.Value, out var rssbl))
            {
                Rpcs3ShowSettingsBeforeLaunch = rssbl;
            }
        }

        // Sega Model 2
        var sm2 = settings.Element("SegaModel2");
        if (sm2 != null)
        {
            if (int.TryParse(sm2.Element("ResX")?.Value, out var sm2Rx))
            {
                SegaModel2ResX = sm2Rx;
            }

            if (int.TryParse(sm2.Element("ResY")?.Value, out var sm2Ry))
            {
                SegaModel2ResY = sm2Ry;
            }

            if (int.TryParse(sm2.Element("WideScreen")?.Value, out var sm2Ws))
            {
                SegaModel2WideScreen = sm2Ws;
            }

            if (bool.TryParse(sm2.Element("Bilinear")?.Value, out var sm2B))
            {
                SegaModel2Bilinear = sm2B;
            }

            if (bool.TryParse(sm2.Element("Trilinear")?.Value, out var sm2T))
            {
                SegaModel2Trilinear = sm2T;
            }

            if (bool.TryParse(sm2.Element("FilterTilemaps")?.Value, out var sm2Ft))
            {
                SegaModel2FilterTilemaps = sm2Ft;
            }

            if (bool.TryParse(sm2.Element("DrawCross")?.Value, out var sm2dc))
            {
                SegaModel2DrawCross = sm2dc;
            }

            if (int.TryParse(sm2.Element("Fsaa")?.Value, out var sm2Fsaa))
            {
                SegaModel2Fsaa = sm2Fsaa;
            }

            if (bool.TryParse(sm2.Element("XInput")?.Value, out var sm2Xi))
            {
                SegaModel2XInput = sm2Xi;
            }

            if (bool.TryParse(sm2.Element("EnableFf")?.Value, out var sm2Eff))
            {
                SegaModel2EnableFf = sm2Eff;
            }

            if (bool.TryParse(sm2.Element("HoldGears")?.Value, out var sm2Hg))
            {
                SegaModel2HoldGears = sm2Hg;
            }

            if (bool.TryParse(sm2.Element("UseRawInput")?.Value, out var sm2Uri))
            {
                SegaModel2UseRawInput = sm2Uri;
            }

            if (bool.TryParse(sm2.Element("ShowSettingsBeforeLaunch")?.Value, out var sm2Ssbl))
            {
                SegaModel2ShowSettingsBeforeLaunch = sm2Ssbl;
            }
        }

        // Stella
        var stella = settings.Element("Stella");
        if (stella != null)
        {
            if (bool.TryParse(stella.Element("Fullscreen")?.Value, out var stf))
            {
                StellaFullscreen = stf;
            }

            if (bool.TryParse(stella.Element("Vsync")?.Value, out var stv))
            {
                StellaVsync = stv;
            }

            StellaVideoDriver = stella.Element("VideoDriver")?.Value ?? StellaVideoDriver;
            if (bool.TryParse(stella.Element("CorrectAspect")?.Value, out var stca))
            {
                StellaCorrectAspect = stca;
            }

            if (int.TryParse(stella.Element("TvFilter")?.Value, out var sttf))
            {
                StellaTvFilter = sttf;
            }

            if (int.TryParse(stella.Element("Scanlines")?.Value, out var sts))
            {
                StellaScanlines = sts;
            }

            if (bool.TryParse(stella.Element("AudioEnabled")?.Value, out var stae))
            {
                StellaAudioEnabled = stae;
            }

            if (int.TryParse(stella.Element("AudioVolume")?.Value, out var stav))
            {
                StellaAudioVolume = stav;
            }

            if (bool.TryParse(stella.Element("TimeMachine")?.Value, out var stm))
            {
                StellaTimeMachine = stm;
            }

            if (bool.TryParse(stella.Element("ConfirmExit")?.Value, out var stce))
            {
                StellaConfirmExit = stce;
            }

            if (bool.TryParse(stella.Element("ShowSettingsBeforeLaunch")?.Value, out var stssbl))
            {
                StellaShowSettingsBeforeLaunch = stssbl;
            }
        }

        // Supermodel
        var supermodel = settings.Element("Supermodel");
        if (supermodel != null)
        {
            if (bool.TryParse(supermodel.Element("New3DEngine")?.Value, out var smn3E))
            {
                SupermodelNew3DEngine = smn3E;
            }

            if (bool.TryParse(supermodel.Element("QuadRendering")?.Value, out var smqr))
            {
                SupermodelQuadRendering = smqr;
            }

            if (bool.TryParse(supermodel.Element("Fullscreen")?.Value, out var smfs))
            {
                SupermodelFullscreen = smfs;
            }

            if (int.TryParse(supermodel.Element("ResX")?.Value, out var smrx))
            {
                SupermodelResX = smrx;
            }

            if (int.TryParse(supermodel.Element("ResY")?.Value, out var smry))
            {
                SupermodelResY = smry;
            }

            if (bool.TryParse(supermodel.Element("WideScreen")?.Value, out var smws))
            {
                SupermodelWideScreen = smws;
            }

            if (bool.TryParse(supermodel.Element("Stretch")?.Value, out var smst))
            {
                SupermodelStretch = smst;
            }

            if (bool.TryParse(supermodel.Element("Vsync")?.Value, out var smvs))
            {
                SupermodelVsync = smvs;
            }

            if (bool.TryParse(supermodel.Element("Throttle")?.Value, out var smth))
            {
                SupermodelThrottle = smth;
            }

            if (int.TryParse(supermodel.Element("MusicVolume")?.Value, out var smmv))
            {
                SupermodelMusicVolume = smmv;
            }

            if (int.TryParse(supermodel.Element("SoundVolume")?.Value, out var ssv))
            {
                SupermodelSoundVolume = ssv;
            }

            SupermodelInputSystem = ValidateSupermodelInputSystem(supermodel.Element("InputSystem")?.Value);
            if (bool.TryParse(supermodel.Element("MultiThreaded")?.Value, out var smmt))
            {
                SupermodelMultiThreaded = smmt;
            }

            if (int.TryParse(supermodel.Element("PowerPcFrequency")?.Value, out var smppf))
            {
                SupermodelPowerPcFrequency = smppf;
            }

            if (bool.TryParse(supermodel.Element("ShowSettingsBeforeLaunch")?.Value, out var smssbl))
            {
                SupermodelShowSettingsBeforeLaunch = smssbl;
            }
        }

        // Xenia
        var xenia = settings.Element("Xenia");
        if (xenia != null)
        {
            XeniaReadbackResolve = xenia.Element("ReadbackResolve")?.Value ?? XeniaReadbackResolve;
            if (bool.TryParse(xenia.Element("GammaSrgb")?.Value, out var xgs))
            {
                XeniaGammaSrgb = xgs;
            }

            if (bool.TryParse(xenia.Element("Vibration")?.Value, out var xvib))
            {
                XeniaVibration = xvib;
            }

            if (bool.TryParse(xenia.Element("MountCache")?.Value, out var xmc))
            {
                XeniaMountCache = xmc;
            }

            XeniaGpu = xenia.Element("Gpu")?.Value ?? XeniaGpu;
            if (bool.TryParse(xenia.Element("Vsync")?.Value, out var xvs))
            {
                XeniaVsync = xvs;
            }

            if (int.TryParse(xenia.Element("ResScaleX")?.Value, out var xrsx))
            {
                XeniaResScaleX = xrsx;
            }

            if (int.TryParse(xenia.Element("ResScaleY")?.Value, out var xrsy))
            {
                XeniaResScaleY = xrsy;
            }

            if (bool.TryParse(xenia.Element("Fullscreen")?.Value, out var xfs))
            {
                XeniaFullscreen = xfs;
            }

            XeniaApu = xenia.Element("Apu")?.Value ?? XeniaApu;
            if (bool.TryParse(xenia.Element("Mute")?.Value, out var xmu))
            {
                XeniaMute = xmu;
            }

            XeniaAa = xenia.Element("Aa")?.Value ?? XeniaAa;
            XeniaScaling = xenia.Element("Scaling")?.Value ?? XeniaScaling;
            if (bool.TryParse(xenia.Element("ApplyPatches")?.Value, out var xap))
            {
                XeniaApplyPatches = xap;
            }

            if (bool.TryParse(xenia.Element("DiscordPresence")?.Value, out var xdp))
            {
                XeniaDiscordPresence = xdp;
            }

            if (int.TryParse(xenia.Element("UserLanguage")?.Value, out var xul))
            {
                XeniaUserLanguage = xul;
            }

            XeniaHid = xenia.Element("Hid")?.Value ?? XeniaHid;
            if (bool.TryParse(xenia.Element("ShowSettingsBeforeLaunch")?.Value, out var xssbl))
            {
                XeniaShowSettingsBeforeLaunch = xssbl;
            }
        }

        // Yumir
        var yumir = settings.Element("Yumir");
        if (yumir != null)
        {
            if (bool.TryParse(yumir.Element("Fullscreen")?.Value, out var yf))
            {
                YumirFullscreen = yf;
            }

            if (double.TryParse(yumir.Element("Volume")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var yv))
            {
                YumirVolume = yv;
            }

            if (bool.TryParse(yumir.Element("Mute")?.Value, out var ym))
            {
                YumirMute = ym;
            }

            YumirVideoStandard = yumir.Element("VideoStandard")?.Value ?? YumirVideoStandard;
            if (bool.TryParse(yumir.Element("AutoDetectRegion")?.Value, out var yadr))
            {
                YumirAutoDetectRegion = yadr;
            }

            if (bool.TryParse(yumir.Element("PauseWhenUnfocused")?.Value, out var ypwu))
            {
                YumirPauseWhenUnfocused = ypwu;
            }

            if (double.TryParse(yumir.Element("ForcedAspect")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var yfa))
            {
                YumirForcedAspect = yfa;
            }

            if (bool.TryParse(yumir.Element("ForceAspectRatio")?.Value, out var yfar))
            {
                YumirForceAspectRatio = yfar;
            }

            if (bool.TryParse(yumir.Element("ReduceLatency")?.Value, out var yrl))
            {
                YumirReduceLatency = yrl;
            }

            if (bool.TryParse(yumir.Element("ShowSettingsBeforeLaunch")?.Value, out var yssbl))
            {
                YumirShowSettingsBeforeLaunch = yssbl;
            }
        }

        // SystemPlayTimes
        var playTimes = settings.Element("SystemPlayTimes");
        if (playTimes != null)
        {
            SystemPlayTimes.Clear();
            foreach (var pt in playTimes.Elements("SystemPlayTime"))
            {
                SystemPlayTimes.Add(new SystemPlayTime
                {
                    SystemName = pt.Element("SystemName")?.Value ?? "",
                    PlayTime = pt.Element("PlayTime")?.Value ?? "00:00:00"
                });
            }
        }
    }

    public void Save()
    {
        lock (_saveLock)
        {
            try
            {
                var root = new XElement("Settings",
                    // Application Settings
                    new XElement("Application",
                        new XElement("ThumbnailSize", ThumbnailSize),
                        new XElement("GamesPerPage", GamesPerPage),
                        new XElement("ShowGames", ShowGames),
                        new XElement("ViewMode", ViewMode),
                        new XElement("EnableGamePadNavigation", EnableGamePadNavigation),
                        new XElement("VideoUrl", VideoUrl),
                        new XElement("InfoUrl", InfoUrl),
                        new XElement("BaseTheme", BaseTheme),
                        new XElement("AccentColor", AccentColor),
                        new XElement("Language", Language),
                        new XElement("DeadZoneX", DeadZoneX.ToString(CultureInfo.InvariantCulture)),
                        new XElement("DeadZoneY", DeadZoneY.ToString(CultureInfo.InvariantCulture)),
                        new XElement("ButtonAspectRatio", ButtonAspectRatio),
                        new XElement("EnableFuzzyMatching", EnableFuzzyMatching),
                        new XElement("FuzzyMatchingThreshold", FuzzyMatchingThreshold.ToString(CultureInfo.InvariantCulture)),
                        new XElement("EnableNotificationSound", EnableNotificationSound),
                        new XElement("CustomNotificationSoundFile", CustomNotificationSoundFile),
                        new XElement("RaUsername", RaUsername),
                        new XElement("RaApiKey", RaApiKey),
                        new XElement("RaPassword", RaPassword),
                        new XElement("RaToken", RaToken),
                        new XElement("OverlayRetroAchievementButton", OverlayRetroAchievementButton),
                        new XElement("OverlayOpenVideoButton", OverlayOpenVideoButton),
                        new XElement("OverlayOpenInfoButton", OverlayOpenInfoButton),
                        new XElement("AdditionalSystemFoldersExpanded", AdditionalSystemFoldersExpanded),
                        new XElement("Emulator1Expanded", Emulator1Expanded),
                        new XElement("Emulator2Expanded", Emulator2Expanded),
                        new XElement("Emulator3Expanded", Emulator3Expanded),
                        new XElement("Emulator4Expanded", Emulator4Expanded),
                        new XElement("Emulator5Expanded", Emulator5Expanded)
                    ),

                    // Ares
                    new XElement("Ares",
                        new XElement("VideoDriver", AresVideoDriver),
                        new XElement("Exclusive", AresExclusive),
                        new XElement("Shader", AresShader),
                        new XElement("Multiplier", AresMultiplier),
                        new XElement("AspectCorrection", AresAspectCorrection),
                        new XElement("Mute", AresMute),
                        new XElement("Volume", AresVolume.ToString(CultureInfo.InvariantCulture)),
                        new XElement("FastBoot", AresFastBoot),
                        new XElement("Rewind", AresRewind),
                        new XElement("RunAhead", AresRunAhead),
                        new XElement("AutoSaveMemory", AresAutoSaveMemory),
                        new XElement("ShowSettingsBeforeLaunch", AresShowSettingsBeforeLaunch)
                    ),

                    // Azahar
                    new XElement("Azahar",
                        new XElement("GraphicsApi", AzaharGraphicsApi),
                        new XElement("ResolutionFactor", AzaharResolutionFactor),
                        new XElement("UseVsync", AzaharUseVsync),
                        new XElement("AsyncShaderCompilation", AzaharAsyncShaderCompilation),
                        new XElement("Fullscreen", AzaharFullscreen),
                        new XElement("Volume", AzaharVolume),
                        new XElement("IsNew3ds", AzaharIsNew3ds),
                        new XElement("LayoutOption", AzaharLayoutOption),
                        new XElement("ShowSettingsBeforeLaunch", AzaharShowSettingsBeforeLaunch),
                        new XElement("EnableAudioStretching", AzaharEnableAudioStretching)
                    ),

                    // Blastem
                    new XElement("Blastem",
                        new XElement("Fullscreen", BlastemFullscreen),
                        new XElement("Vsync", BlastemVsync),
                        new XElement("Aspect", BlastemAspect),
                        new XElement("Scaling", BlastemScaling),
                        new XElement("Scanlines", BlastemScanlines),
                        new XElement("AudioRate", BlastemAudioRate),
                        new XElement("SyncSource", BlastemSyncSource),
                        new XElement("ShowSettingsBeforeLaunch", BlastemShowSettingsBeforeLaunch)
                    ),

                    // Cemu
                    new XElement("Cemu",
                        new XElement("Fullscreen", CemuFullscreen),
                        new XElement("GraphicApi", CemuGraphicApi),
                        new XElement("Vsync", CemuVsync),
                        new XElement("AsyncCompile", CemuAsyncCompile),
                        new XElement("TvVolume", CemuTvVolume),
                        new XElement("ConsoleLanguage", CemuConsoleLanguage),
                        new XElement("DiscordPresence", CemuDiscordPresence),
                        new XElement("ShowSettingsBeforeLaunch", CemuShowSettingsBeforeLaunch)
                    ),

                    // Daphne
                    new XElement("Daphne",
                        new XElement("Fullscreen", DaphneFullscreen),
                        new XElement("ResX", DaphneResX),
                        new XElement("ResY", DaphneResY),
                        new XElement("DisableCrosshairs", DaphneDisableCrosshairs),
                        new XElement("Bilinear", DaphneBilinear),
                        new XElement("EnableSound", DaphneEnableSound),
                        new XElement("UseOverlays", DaphneUseOverlays),
                        new XElement("ShowSettingsBeforeLaunch", DaphneShowSettingsBeforeLaunch)
                    ),

                    // Dolphin
                    new XElement("Dolphin",
                        new XElement("GfxBackend", DolphinGfxBackend),
                        new XElement("DspThread", DolphinDspThread),
                        new XElement("WiimoteContinuousScanning", DolphinWiimoteContinuousScanning),
                        new XElement("WiimoteEnableSpeaker", DolphinWiimoteEnableSpeaker),
                        new XElement("ShowSettingsBeforeLaunch", DolphinShowSettingsBeforeLaunch)
                    ),

                    // DuckStation
                    new XElement("DuckStation",
                        new XElement("StartFullscreen", DuckStationStartFullscreen),
                        new XElement("PauseOnFocusLoss", DuckStationPauseOnFocusLoss),
                        new XElement("SaveStateOnExit", DuckStationSaveStateOnExit),
                        new XElement("RewindEnable", DuckStationRewindEnable),
                        new XElement("RunaheadFrameCount", DuckStationRunaheadFrameCount),
                        new XElement("Renderer", DuckStationRenderer),
                        new XElement("ResolutionScale", DuckStationResolutionScale),
                        new XElement("TextureFilter", DuckStationTextureFilter),
                        new XElement("WidescreenHack", DuckStationWidescreenHack),
                        new XElement("PgxpEnable", DuckStationPgxpEnable),
                        new XElement("AspectRatio", DuckStationAspectRatio),
                        new XElement("Vsync", DuckStationVsync),
                        new XElement("OutputVolume", DuckStationOutputVolume),
                        new XElement("OutputMuted", DuckStationOutputMuted),
                        new XElement("ShowSettingsBeforeLaunch", DuckStationShowSettingsBeforeLaunch)
                    ),

                    // Flycast
                    new XElement("Flycast",
                        new XElement("Fullscreen", FlycastFullscreen),
                        new XElement("Width", FlycastWidth),
                        new XElement("Height", FlycastHeight),
                        new XElement("Maximized", FlycastMaximized),
                        new XElement("ShowSettingsBeforeLaunch", FlycastShowSettingsBeforeLaunch)
                    ),

                    // MAME
                    new XElement("Mame",
                        new XElement("Video", MameVideo),
                        new XElement("Window", MameWindow),
                        new XElement("Maximize", MameMaximize),
                        new XElement("KeepAspect", MameKeepAspect),
                        new XElement("SkipGameInfo", MameSkipGameInfo),
                        new XElement("Autosave", MameAutosave),
                        new XElement("ConfirmQuit", MameConfirmQuit),
                        new XElement("Joystick", MameJoystick),
                        new XElement("ShowSettingsBeforeLaunch", MameShowSettingsBeforeLaunch),
                        new XElement("Autoframeskip", MameAutoframeskip),
                        new XElement("BgfxBackend", MameBgfxBackend),
                        new XElement("BgfxScreenChains", MameBgfxScreenChains),
                        new XElement("Filter", MameFilter),
                        new XElement("Cheat", MameCheat),
                        new XElement("Rewind", MameRewind),
                        new XElement("NvramSave", MameNvramSave)
                    ),

                    // Mednafen
                    new XElement("Mednafen",
                        new XElement("VideoDriver", MednafenVideoDriver),
                        new XElement("Fullscreen", MednafenFullscreen),
                        new XElement("Vsync", MednafenVsync),
                        new XElement("Stretch", MednafenStretch),
                        new XElement("Bilinear", MednafenBilinear),
                        new XElement("Scanlines", MednafenScanlines),
                        new XElement("Shader", MednafenShader),
                        new XElement("Volume", MednafenVolume),
                        new XElement("Cheats", MednafenCheats),
                        new XElement("Rewind", MednafenRewind),
                        new XElement("ShowSettingsBeforeLaunch", MednafenShowSettingsBeforeLaunch)
                    ),

                    // Mesen
                    new XElement("Mesen",
                        new XElement("Fullscreen", MesenFullscreen),
                        new XElement("Vsync", MesenVsync),
                        new XElement("AspectRatio", MesenAspectRatio),
                        new XElement("Bilinear", MesenBilinear),
                        new XElement("VideoFilter", MesenVideoFilter),
                        new XElement("EnableAudio", MesenEnableAudio),
                        new XElement("MasterVolume", MesenMasterVolume),
                        new XElement("Rewind", MesenRewind),
                        new XElement("RunAhead", MesenRunAhead),
                        new XElement("PauseInBackground", MesenPauseInBackground),
                        new XElement("ShowSettingsBeforeLaunch", MesenShowSettingsBeforeLaunch)
                    ),

                    // PCSX2
                    new XElement("Pcsx2",
                        new XElement("StartFullscreen", Pcsx2StartFullscreen),
                        new XElement("AspectRatio", Pcsx2AspectRatio),
                        new XElement("Renderer", Pcsx2Renderer),
                        new XElement("UpscaleMultiplier", Pcsx2UpscaleMultiplier),
                        new XElement("Vsync", Pcsx2Vsync),
                        new XElement("EnableCheats", Pcsx2EnableCheats),
                        new XElement("EnableWidescreenPatches", Pcsx2EnableWidescreenPatches),
                        new XElement("Volume", Pcsx2Volume),
                        new XElement("AchievementsEnabled", Pcsx2AchievementsEnabled),
                        new XElement("AchievementsHardcore", Pcsx2AchievementsHardcore),
                        new XElement("ShowSettingsBeforeLaunch", Pcsx2ShowSettingsBeforeLaunch)
                    ),

                    // RetroArch
                    new XElement("RetroArch",
                        new XElement("CheevosEnable", RetroArchCheevosEnable),
                        new XElement("CheevosHardcore", RetroArchCheevosHardcore),
                        new XElement("Fullscreen", RetroArchFullscreen),
                        new XElement("Vsync", RetroArchVsync),
                        new XElement("VideoDriver", RetroArchVideoDriver),
                        new XElement("AudioEnable", RetroArchAudioEnable),
                        new XElement("AudioMute", RetroArchAudioMute),
                        new XElement("MenuDriver", RetroArchMenuDriver),
                        new XElement("PauseNonActive", RetroArchPauseNonActive),
                        new XElement("SaveOnExit", RetroArchSaveOnExit),
                        new XElement("AutoSaveState", RetroArchAutoSaveState),
                        new XElement("AutoLoadState", RetroArchAutoLoadState),
                        new XElement("Rewind", RetroArchRewind),
                        new XElement("ThreadedVideo", RetroArchThreadedVideo),
                        new XElement("Bilinear", RetroArchBilinear),
                        new XElement("ShowSettingsBeforeLaunch", RetroArchShowSettingsBeforeLaunch),
                        new XElement("AspectRatioIndex", RetroArchAspectRatioIndex),
                        new XElement("ScaleInteger", RetroArchScaleInteger),
                        new XElement("ShaderEnable", RetroArchShaderEnable),
                        new XElement("HardSync", RetroArchHardSync),
                        new XElement("RunAhead", RetroArchRunAhead),
                        new XElement("ShowAdvancedSettings", RetroArchShowAdvancedSettings),
                        new XElement("DiscordAllow", RetroArchDiscordAllow),
                        new XElement("OverrideSystemDir", RetroArchOverrideSystemDir),
                        new XElement("OverrideSaveDir", RetroArchOverrideSaveDir),
                        new XElement("OverrideStateDir", RetroArchOverrideStateDir),
                        new XElement("OverrideScreenshotDir", RetroArchOverrideScreenshotDir)
                    ),

                    // RPCS3
                    new XElement("Rpcs3",
                        new XElement("Renderer", Rpcs3Renderer),
                        new XElement("Resolution", Rpcs3Resolution),
                        new XElement("AspectRatio", Rpcs3AspectRatio),
                        new XElement("Vsync", Rpcs3Vsync),
                        new XElement("ResolutionScale", Rpcs3ResolutionScale),
                        new XElement("AnisotropicFilter", Rpcs3AnisotropicFilter),
                        new XElement("PpuDecoder", Rpcs3PpuDecoder),
                        new XElement("SpuDecoder", Rpcs3SpuDecoder),
                        new XElement("AudioRenderer", Rpcs3AudioRenderer),
                        new XElement("AudioBuffering", Rpcs3AudioBuffering),
                        new XElement("StartFullscreen", Rpcs3StartFullscreen),
                        new XElement("ShowSettingsBeforeLaunch", Rpcs3ShowSettingsBeforeLaunch)
                    ),

                    // SEGA Model 2
                    new XElement("SegaModel2",
                        new XElement("ResX", SegaModel2ResX),
                        new XElement("ResY", SegaModel2ResY),
                        new XElement("WideScreen", SegaModel2WideScreen),
                        new XElement("Bilinear", SegaModel2Bilinear),
                        new XElement("Trilinear", SegaModel2Trilinear),
                        new XElement("FilterTilemaps", SegaModel2FilterTilemaps),
                        new XElement("DrawCross", SegaModel2DrawCross),
                        new XElement("Fsaa", SegaModel2Fsaa),
                        new XElement("XInput", SegaModel2XInput),
                        new XElement("EnableFf", SegaModel2EnableFf),
                        new XElement("HoldGears", SegaModel2HoldGears),
                        new XElement("UseRawInput", SegaModel2UseRawInput),
                        new XElement("ShowSettingsBeforeLaunch", SegaModel2ShowSettingsBeforeLaunch)
                    ),

                    // Stella
                    new XElement("Stella",
                        new XElement("Fullscreen", StellaFullscreen),
                        new XElement("Vsync", StellaVsync),
                        new XElement("VideoDriver", StellaVideoDriver),
                        new XElement("CorrectAspect", StellaCorrectAspect),
                        new XElement("TvFilter", StellaTvFilter),
                        new XElement("Scanlines", StellaScanlines),
                        new XElement("AudioEnabled", StellaAudioEnabled),
                        new XElement("AudioVolume", StellaAudioVolume),
                        new XElement("TimeMachine", StellaTimeMachine),
                        new XElement("ConfirmExit", StellaConfirmExit),
                        new XElement("ShowSettingsBeforeLaunch", StellaShowSettingsBeforeLaunch)
                    ),

                    // Supermodel
                    new XElement("Supermodel",
                        new XElement("New3DEngine", SupermodelNew3DEngine),
                        new XElement("QuadRendering", SupermodelQuadRendering),
                        new XElement("Fullscreen", SupermodelFullscreen),
                        new XElement("ResX", SupermodelResX),
                        new XElement("ResY", SupermodelResY),
                        new XElement("WideScreen", SupermodelWideScreen),
                        new XElement("Stretch", SupermodelStretch),
                        new XElement("Vsync", SupermodelVsync),
                        new XElement("Throttle", SupermodelThrottle),
                        new XElement("MusicVolume", SupermodelMusicVolume),
                        new XElement("SoundVolume", SupermodelSoundVolume),
                        new XElement("InputSystem", SupermodelInputSystem),
                        new XElement("MultiThreaded", SupermodelMultiThreaded),
                        new XElement("PowerPcFrequency", SupermodelPowerPcFrequency),
                        new XElement("ShowSettingsBeforeLaunch", SupermodelShowSettingsBeforeLaunch)
                    ),

                    // Xenia
                    new XElement("Xenia",
                        new XElement("ReadbackResolve", XeniaReadbackResolve),
                        new XElement("GammaSrgb", XeniaGammaSrgb),
                        new XElement("Vibration", XeniaVibration),
                        new XElement("MountCache", XeniaMountCache),
                        new XElement("Gpu", XeniaGpu),
                        new XElement("Vsync", XeniaVsync),
                        new XElement("ResScaleX", XeniaResScaleX),
                        new XElement("ResScaleY", XeniaResScaleY),
                        new XElement("Fullscreen", XeniaFullscreen),
                        new XElement("Apu", XeniaApu),
                        new XElement("Mute", XeniaMute),
                        new XElement("Aa", XeniaAa),
                        new XElement("Scaling", XeniaScaling),
                        new XElement("ApplyPatches", XeniaApplyPatches),
                        new XElement("DiscordPresence", XeniaDiscordPresence),
                        new XElement("UserLanguage", XeniaUserLanguage),
                        new XElement("Hid", XeniaHid),
                        new XElement("ShowSettingsBeforeLaunch", XeniaShowSettingsBeforeLaunch)
                    ),

                    // Yumir
                    new XElement("Yumir",
                        new XElement("Fullscreen", YumirFullscreen),
                        new XElement("Volume", YumirVolume.ToString(CultureInfo.InvariantCulture)),
                        new XElement("Mute", YumirMute),
                        new XElement("VideoStandard", YumirVideoStandard),
                        new XElement("AutoDetectRegion", YumirAutoDetectRegion),
                        new XElement("PauseWhenUnfocused", YumirPauseWhenUnfocused),
                        new XElement("ForcedAspect", YumirForcedAspect.ToString(CultureInfo.InvariantCulture)),
                        new XElement("ForceAspectRatio", YumirForceAspectRatio),
                        new XElement("ReduceLatency", YumirReduceLatency),
                        new XElement("ShowSettingsBeforeLaunch", YumirShowSettingsBeforeLaunch)
                    ),

                    // SystemPlayTimes
                    new XElement("SystemPlayTimes",
                        SystemPlayTimes.Select(static pt =>
                            new XElement("SystemPlayTime",
                                new XElement("SystemName", pt.SystemName),
                                new XElement("PlayTime", pt.PlayTime)
                            )
                        )
                    )
                );

                root.Save(_filePath);
            }
            catch (Exception ex)
            {
                App.ServiceProvider?.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving settings.xml");
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

    private static string ValidateSupermodelInputSystem(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "xinput";

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "xinput" or "dinput" or "rawinput" ? normalized : "xinput";
    }

    public void ResetToDefaults()
    {
        CopyFrom(new SettingsManager());
    }

    private void SetDefaultsAndSave()
    {
        ResetToDefaults();
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

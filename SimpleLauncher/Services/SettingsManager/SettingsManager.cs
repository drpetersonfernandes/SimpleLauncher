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
        ThumbnailSize = ValidateThumbnailSize(settings.Element("ThumbnailSize")?.Value);
        GamesPerPage = ValidateGamesPerPage(settings.Element("GamesPerPage")?.Value);
        ShowGames = ValidateShowGames(settings.Element("ShowGames")?.Value);
        ViewMode = ValidateViewMode(settings.Element("ViewMode")?.Value);
        if (bool.TryParse(settings.Element("EnableGamePadNavigation")?.Value, out var gp))
        {
            EnableGamePadNavigation = gp;
        }

        VideoUrl = settings.Element("VideoUrl")?.Value ?? VideoUrl;
        InfoUrl = settings.Element("InfoUrl")?.Value ?? InfoUrl;
        BaseTheme = settings.Element("BaseTheme")?.Value ?? BaseTheme;
        AccentColor = settings.Element("AccentColor")?.Value ?? AccentColor;
        Language = settings.Element("Language")?.Value ?? Language;
        ButtonAspectRatio = ValidateButtonAspectRatio(settings.Element("ButtonAspectRatio")?.Value);
        RaUsername = settings.Element("RaUsername")?.Value ?? RaUsername;
        RaApiKey = settings.Element("RaApiKey")?.Value ?? RaApiKey;
        RaPassword = settings.Element("RaPassword")?.Value ?? RaPassword;
        RaToken = settings.Element("RaToken")?.Value ?? RaToken;
        if (float.TryParse(settings.Element("DeadZoneX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzx))
        {
            DeadZoneX = dzx;
        }

        if (float.TryParse(settings.Element("DeadZoneY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzy))
        {
            DeadZoneY = dzy;
        }

        if (bool.TryParse(settings.Element("EnableFuzzyMatching")?.Value, out var fm))
        {
            EnableFuzzyMatching = fm;
        }

        if (double.TryParse(settings.Element("FuzzyMatchingThreshold")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fmt))
        {
            FuzzyMatchingThreshold = fmt;
        }

        if (bool.TryParse(settings.Element("EnableNotificationSound")?.Value, out var ens))
        {
            EnableNotificationSound = ens;
        }

        CustomNotificationSoundFile = settings.Element("CustomNotificationSoundFile")?.Value ?? CustomNotificationSoundFile;
        if (bool.TryParse(settings.Element("OverlayRetroAchievementButton")?.Value, out var ora))
        {
            OverlayRetroAchievementButton = ora;
        }

        if (bool.TryParse(settings.Element("OverlayOpenVideoButton")?.Value, out var ovb))
        {
            OverlayOpenVideoButton = ovb;
        }

        if (bool.TryParse(settings.Element("OverlayOpenInfoButton")?.Value, out var oib))
        {
            OverlayOpenInfoButton = oib;
        }

        if (bool.TryParse(settings.Element("AdditionalSystemFoldersExpanded")?.Value, out var asfe))
        {
            AdditionalSystemFoldersExpanded = asfe;
        }

        if (bool.TryParse(settings.Element("Emulator1Expanded")?.Value, out var e1E))
        {
            Emulator1Expanded = e1E;
        }

        if (bool.TryParse(settings.Element("Emulator2Expanded")?.Value, out var e2E))
        {
            Emulator2Expanded = e2E;
        }

        if (bool.TryParse(settings.Element("Emulator3Expanded")?.Value, out var e3E))
        {
            Emulator3Expanded = e3E;
        }

        if (bool.TryParse(settings.Element("Emulator4Expanded")?.Value, out var e4E))
        {
            Emulator4Expanded = e4E;
        }

        if (bool.TryParse(settings.Element("Emulator5Expanded")?.Value, out var e5E))
        {
            Emulator5Expanded = e5E;
        }

        // Ares
        AresVideoDriver = settings.Element("AresVideoDriver")?.Value ?? AresVideoDriver;
        if (bool.TryParse(settings.Element("AresExclusive")?.Value, out var ae))
        {
            AresExclusive = ae;
        }

        AresShader = settings.Element("AresShader")?.Value ?? AresShader;
        if (int.TryParse(settings.Element("AresMultiplier")?.Value, out var am))
        {
            AresMultiplier = am;
        }

        AresAspectCorrection = settings.Element("AresAspectCorrection")?.Value ?? AresAspectCorrection;
        if (bool.TryParse(settings.Element("AresMute")?.Value, out var amu))
        {
            AresMute = amu;
        }

        if (double.TryParse(settings.Element("AresVolume")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var av))
        {
            AresVolume = av;
        }

        if (bool.TryParse(settings.Element("AresFastBoot")?.Value, out var afb))
        {
            AresFastBoot = afb;
        }

        if (bool.TryParse(settings.Element("AresRewind")?.Value, out var ar))
        {
            AresRewind = ar;
        }

        if (bool.TryParse(settings.Element("AresRunAhead")?.Value, out var ara))
        {
            AresRunAhead = ara;
        }

        if (bool.TryParse(settings.Element("AresAutoSaveMemory")?.Value, out var asm))
        {
            AresAutoSaveMemory = asm;
        }

        if (bool.TryParse(settings.Element("AresShowSettingsBeforeLaunch")?.Value, out var assbl))
        {
            AresShowSettingsBeforeLaunch = assbl;
        }

        // Azahar
        if (int.TryParse(settings.Element("AzaharGraphicsApi")?.Value, out var aga))
        {
            AzaharGraphicsApi = aga;
        }

        if (int.TryParse(settings.Element("AzaharResolutionFactor")?.Value, out var arf))
        {
            AzaharResolutionFactor = arf;
        }

        if (bool.TryParse(settings.Element("AzaharUseVsync")?.Value, out var auv))
        {
            AzaharUseVsync = auv;
        }

        if (bool.TryParse(settings.Element("AzaharAsyncShaderCompilation")?.Value, out var aasc))
        {
            AzaharAsyncShaderCompilation = aasc;
        }

        if (bool.TryParse(settings.Element("AzaharFullscreen")?.Value, out var af))
        {
            AzaharFullscreen = af;
        }

        if (int.TryParse(settings.Element("AzaharVolume")?.Value, out var avol))
        {
            AzaharVolume = avol;
        }

        if (bool.TryParse(settings.Element("AzaharIsNew3ds")?.Value, out var ain))
        {
            AzaharIsNew3ds = ain;
        }

        if (int.TryParse(settings.Element("AzaharLayoutOption")?.Value, out var alo))
        {
            AzaharLayoutOption = alo;
        }

        if (bool.TryParse(settings.Element("AzaharShowSettingsBeforeLaunch")?.Value, out var asbl))
        {
            AzaharShowSettingsBeforeLaunch = asbl;
        }

        if (bool.TryParse(settings.Element("AzaharEnableAudioStretching")?.Value, out var aeas))
        {
            AzaharEnableAudioStretching = aeas;
        }

        // Blastem
        if (bool.TryParse(settings.Element("BlastemFullscreen")?.Value, out var bf))
        {
            BlastemFullscreen = bf;
        }

        if (bool.TryParse(settings.Element("BlastemVsync")?.Value, out var bv))
        {
            BlastemVsync = bv;
        }

        BlastemAspect = settings.Element("BlastemAspect")?.Value ?? BlastemAspect;
        BlastemScaling = settings.Element("BlastemScaling")?.Value ?? BlastemScaling;
        if (bool.TryParse(settings.Element("BlastemScanlines")?.Value, out var bs))
        {
            BlastemScanlines = bs;
        }

        if (int.TryParse(settings.Element("BlastemAudioRate")?.Value, out var bar))
        {
            BlastemAudioRate = bar;
        }

        BlastemSyncSource = settings.Element("BlastemSyncSource")?.Value ?? BlastemSyncSource;
        if (bool.TryParse(settings.Element("BlastemShowSettingsBeforeLaunch")?.Value, out var bssbl))
        {
            BlastemShowSettingsBeforeLaunch = bssbl;
        }

        // Cemu
        if (bool.TryParse(settings.Element("CemuFullscreen")?.Value, out var cf))
        {
            CemuFullscreen = cf;
        }

        if (int.TryParse(settings.Element("CemuGraphicApi")?.Value, out var cga))
        {
            CemuGraphicApi = cga;
        }

        if (int.TryParse(settings.Element("CemuVsync")?.Value, out var cv))
        {
            CemuVsync = cv;
        }

        if (bool.TryParse(settings.Element("CemuAsyncCompile")?.Value, out var cac))
        {
            CemuAsyncCompile = cac;
        }

        if (int.TryParse(settings.Element("CemuTvVolume")?.Value, out var ctv))
        {
            CemuTvVolume = ctv;
        }

        if (int.TryParse(settings.Element("CemuConsoleLanguage")?.Value, out var ccl))
        {
            CemuConsoleLanguage = ccl;
        }

        if (bool.TryParse(settings.Element("CemuDiscordPresence")?.Value, out var cdp))
        {
            CemuDiscordPresence = cdp;
        }

        if (bool.TryParse(settings.Element("CemuShowSettingsBeforeLaunch")?.Value, out var cssbl))
        {
            CemuShowSettingsBeforeLaunch = cssbl;
        }

        // Daphne
        if (bool.TryParse(settings.Element("DaphneFullscreen")?.Value, out var df))
        {
            DaphneFullscreen = df;
        }

        if (int.TryParse(settings.Element("DaphneResX")?.Value, out var drx))
        {
            DaphneResX = drx;
        }

        if (int.TryParse(settings.Element("DaphneResY")?.Value, out var dry))
        {
            DaphneResY = dry;
        }

        if (bool.TryParse(settings.Element("DaphneDisableCrosshairs")?.Value, out var ddc))
        {
            DaphneDisableCrosshairs = ddc;
        }

        if (bool.TryParse(settings.Element("DaphneBilinear")?.Value, out var db))
        {
            DaphneBilinear = db;
        }

        if (bool.TryParse(settings.Element("DaphneEnableSound")?.Value, out var des))
        {
            DaphneEnableSound = des;
        }

        if (bool.TryParse(settings.Element("DaphneUseOverlays")?.Value, out var duo))
        {
            DaphneUseOverlays = duo;
        }

        if (bool.TryParse(settings.Element("DaphneShowSettingsBeforeLaunch")?.Value, out var dssbl))
        {
            DaphneShowSettingsBeforeLaunch = dssbl;
        }

        // Dolphin
        DolphinGfxBackend = settings.Element("DolphinGfxBackend")?.Value ?? DolphinGfxBackend;
        if (bool.TryParse(settings.Element("DolphinDspThread")?.Value, out var ddt))
        {
            DolphinDspThread = ddt;
        }

        if (bool.TryParse(settings.Element("DolphinWiimoteContinuousScanning")?.Value, out var dwcs))
        {
            DolphinWiimoteContinuousScanning = dwcs;
        }

        if (bool.TryParse(settings.Element("DolphinWiimoteEnableSpeaker")?.Value, out var dwes))
        {
            DolphinWiimoteEnableSpeaker = dwes;
        }

        if (bool.TryParse(settings.Element("DolphinShowSettingsBeforeLaunch")?.Value, out var dssbl2))
        {
            DolphinShowSettingsBeforeLaunch = dssbl2;
        }

        // DuckStation
        if (bool.TryParse(settings.Element("DuckStationStartFullscreen")?.Value, out var dssf))
        {
            DuckStationStartFullscreen = dssf;
        }

        if (bool.TryParse(settings.Element("DuckStationPauseOnFocusLoss")?.Value, out var dpofl))
        {
            DuckStationPauseOnFocusLoss = dpofl;
        }

        if (bool.TryParse(settings.Element("DuckStationSaveStateOnExit")?.Value, out var dssoe))
        {
            DuckStationSaveStateOnExit = dssoe;
        }

        if (bool.TryParse(settings.Element("DuckStationRewindEnable")?.Value, out var dre))
        {
            DuckStationRewindEnable = dre;
        }

        if (int.TryParse(settings.Element("DuckStationRunaheadFrameCount")?.Value, out var drfc))
        {
            DuckStationRunaheadFrameCount = drfc;
        }

        DuckStationRenderer = settings.Element("DuckStationRenderer")?.Value ?? DuckStationRenderer;
        if (int.TryParse(settings.Element("DuckStationResolutionScale")?.Value, out var drs))
        {
            DuckStationResolutionScale = drs;
        }

        DuckStationTextureFilter = settings.Element("DuckStationTextureFilter")?.Value ?? DuckStationTextureFilter;
        if (bool.TryParse(settings.Element("DuckStationWidescreenHack")?.Value, out var dwh))
        {
            DuckStationWidescreenHack = dwh;
        }

        if (bool.TryParse(settings.Element("DuckStationPgxpEnable")?.Value, out var dpe))
        {
            DuckStationPgxpEnable = dpe;
        }

        DuckStationAspectRatio = settings.Element("DuckStationAspectRatio")?.Value ?? DuckStationAspectRatio;
        if (bool.TryParse(settings.Element("DuckStationVsync")?.Value, out var dsv))
        {
            DuckStationVsync = dsv;
        }

        if (int.TryParse(settings.Element("DuckStationOutputVolume")?.Value, out var dov))
        {
            DuckStationOutputVolume = dov;
        }

        if (bool.TryParse(settings.Element("DuckStationOutputMuted")?.Value, out var dom))
        {
            DuckStationOutputMuted = dom;
        }

        if (bool.TryParse(settings.Element("DuckStationShowSettingsBeforeLaunch")?.Value, out var dssbl3))
        {
            DuckStationShowSettingsBeforeLaunch = dssbl3;
        }

        // Flycast
        if (bool.TryParse(settings.Element("FlycastFullscreen")?.Value, out var ff))
        {
            FlycastFullscreen = ff;
        }

        if (int.TryParse(settings.Element("FlycastWidth")?.Value, out var fw))
        {
            FlycastWidth = fw;
        }

        if (int.TryParse(settings.Element("FlycastHeight")?.Value, out var fh))
        {
            FlycastHeight = fh;
        }

        if (bool.TryParse(settings.Element("FlycastMaximized")?.Value, out var flycastMaximized))
        {
            FlycastMaximized = flycastMaximized;
        }

        if (bool.TryParse(settings.Element("FlycastShowSettingsBeforeLaunch")?.Value, out var fssbl))
        {
            FlycastShowSettingsBeforeLaunch = fssbl;
        }

        // MAME
        MameVideo = settings.Element("MameVideo")?.Value ?? MameVideo;
        if (bool.TryParse(settings.Element("MameWindow")?.Value, out var mw))
        {
            MameWindow = mw;
        }

        if (bool.TryParse(settings.Element("MameMaximize")?.Value, out var mm))
        {
            MameMaximize = mm;
        }

        if (bool.TryParse(settings.Element("MameKeepAspect")?.Value, out var mka))
        {
            MameKeepAspect = mka;
        }

        if (bool.TryParse(settings.Element("MameSkipGameInfo")?.Value, out var msgi))
        {
            MameSkipGameInfo = msgi;
        }

        if (bool.TryParse(settings.Element("MameAutosave")?.Value, out var mas))
        {
            MameAutosave = mas;
        }

        if (bool.TryParse(settings.Element("MameConfirmQuit")?.Value, out var mcq))
        {
            MameConfirmQuit = mcq;
        }

        if (bool.TryParse(settings.Element("MameJoystick")?.Value, out var mj))
        {
            MameJoystick = mj;
        }

        if (bool.TryParse(settings.Element("MameShowSettingsBeforeLaunch")?.Value, out var mssbl))
        {
            MameShowSettingsBeforeLaunch = mssbl;
        }

        if (bool.TryParse(settings.Element("MameAutoframeskip")?.Value, out var maf))
        {
            MameAutoframeskip = maf;
        }

        MameBgfxBackend = settings.Element("MameBgfxBackend")?.Value ?? MameBgfxBackend;
        MameBgfxScreenChains = settings.Element("MameBgfxScreenChains")?.Value ?? MameBgfxScreenChains;
        if (bool.TryParse(settings.Element("MameFilter")?.Value, out var mf))
        {
            MameFilter = mf;
        }

        if (bool.TryParse(settings.Element("MameCheat")?.Value, out var mc))
        {
            MameCheat = mc;
        }

        if (bool.TryParse(settings.Element("MameRewind")?.Value, out var mr))
        {
            MameRewind = mr;
        }

        if (bool.TryParse(settings.Element("MameNvramSave")?.Value, out var mns))
        {
            MameNvramSave = mns;
        }

        // Mednafen
        MednafenVideoDriver = settings.Element("MednafenVideoDriver")?.Value ?? MednafenVideoDriver;
        if (bool.TryParse(settings.Element("MednafenFullscreen")?.Value, out var mef))
        {
            MednafenFullscreen = mef;
        }

        if (bool.TryParse(settings.Element("MednafenVsync")?.Value, out var mev))
        {
            MednafenVsync = mev;
        }

        MednafenStretch = settings.Element("MednafenStretch")?.Value ?? MednafenStretch;
        if (bool.TryParse(settings.Element("MednafenBilinear")?.Value, out var meb))
        {
            MednafenBilinear = meb;
        }

        if (int.TryParse(settings.Element("MednafenScanlines")?.Value, out var mes))
        {
            MednafenScanlines = mes;
        }

        MednafenShader = settings.Element("MednafenShader")?.Value ?? MednafenShader;
        if (int.TryParse(settings.Element("MednafenVolume")?.Value, out var mevo))
        {
            MednafenVolume = mevo;
        }

        if (bool.TryParse(settings.Element("MednafenCheats")?.Value, out var mec))
        {
            MednafenCheats = mec;
        }

        if (bool.TryParse(settings.Element("MednafenRewind")?.Value, out var mer))
        {
            MednafenRewind = mer;
        }

        if (bool.TryParse(settings.Element("MednafenShowSettingsBeforeLaunch")?.Value, out var messbl))
        {
            MednafenShowSettingsBeforeLaunch = messbl;
        }

        // Mesen
        if (bool.TryParse(settings.Element("MesenFullscreen")?.Value, out var msnf))
        {
            MesenFullscreen = msnf;
        }

        if (bool.TryParse(settings.Element("MesenVsync")?.Value, out var msnv))
        {
            MesenVsync = msnv;
        }

        MesenAspectRatio = settings.Element("MesenAspectRatio")?.Value ?? MesenAspectRatio;
        if (bool.TryParse(settings.Element("MesenBilinear")?.Value, out var msnb))
        {
            MesenBilinear = msnb;
        }

        MesenVideoFilter = settings.Element("MesenVideoFilter")?.Value ?? MesenVideoFilter;
        if (bool.TryParse(settings.Element("MesenEnableAudio")?.Value, out var msnbea))
        {
            MesenEnableAudio = msnbea;
        }

        if (int.TryParse(settings.Element("MesenMasterVolume")?.Value, out var msnmv))
        {
            MesenMasterVolume = msnmv;
        }

        if (bool.TryParse(settings.Element("MesenRewind")?.Value, out var msnr))
        {
            MesenRewind = msnr;
        }

        if (int.TryParse(settings.Element("MesenRunAhead")?.Value, out var msnra))
        {
            MesenRunAhead = msnra;
        }

        if (bool.TryParse(settings.Element("MesenPauseInBackground")?.Value, out var msnpib))
        {
            MesenPauseInBackground = msnpib;
        }

        if (bool.TryParse(settings.Element("MesenShowSettingsBeforeLaunch")?.Value, out var msnssbl))
        {
            MesenShowSettingsBeforeLaunch = msnssbl;
        }

        // PCSX2
        if (bool.TryParse(settings.Element("Pcsx2StartFullscreen")?.Value, out var psf))
        {
            Pcsx2StartFullscreen = psf;
        }

        Pcsx2AspectRatio = settings.Element("Pcsx2AspectRatio")?.Value ?? Pcsx2AspectRatio;
        if (int.TryParse(settings.Element("Pcsx2Renderer")?.Value, out var pr))
        {
            Pcsx2Renderer = pr;
        }

        if (int.TryParse(settings.Element("Pcsx2UpscaleMultiplier")?.Value, out var pum))
        {
            Pcsx2UpscaleMultiplier = pum;
        }

        if (bool.TryParse(settings.Element("Pcsx2Vsync")?.Value, out var pv))
        {
            Pcsx2Vsync = pv;
        }

        if (bool.TryParse(settings.Element("Pcsx2EnableCheats")?.Value, out var pec))
        {
            Pcsx2EnableCheats = pec;
        }

        if (bool.TryParse(settings.Element("Pcsx2EnableWidescreenPatches")?.Value, out var pewp))
        {
            Pcsx2EnableWidescreenPatches = pewp;
        }

        if (int.TryParse(settings.Element("Pcsx2Volume")?.Value, out var pvol))
        {
            Pcsx2Volume = pvol;
        }

        if (bool.TryParse(settings.Element("Pcsx2AchievementsEnabled")?.Value, out var pae))
        {
            Pcsx2AchievementsEnabled = pae;
        }

        if (bool.TryParse(settings.Element("Pcsx2AchievementsHardcore")?.Value, out var pah))
        {
            Pcsx2AchievementsHardcore = pah;
        }

        if (bool.TryParse(settings.Element("Pcsx2ShowSettingsBeforeLaunch")?.Value, out var pssbl))
        {
            Pcsx2ShowSettingsBeforeLaunch = pssbl;
        }

        // RetroArch
        if (bool.TryParse(settings.Element("RetroArchCheevosEnable")?.Value, out var race))
        {
            RetroArchCheevosEnable = race;
        }

        if (bool.TryParse(settings.Element("RetroArchCheevosHardcore")?.Value, out var rach))
        {
            RetroArchCheevosHardcore = rach;
        }

        if (bool.TryParse(settings.Element("RetroArchFullscreen")?.Value, out var raf))
        {
            RetroArchFullscreen = raf;
        }

        if (bool.TryParse(settings.Element("RetroArchVsync")?.Value, out var rav))
        {
            RetroArchVsync = rav;
        }

        RetroArchVideoDriver = settings.Element("RetroArchVideoDriver")?.Value ?? RetroArchVideoDriver;
        if (bool.TryParse(settings.Element("RetroArchAudioEnable")?.Value, out var raae))
        {
            RetroArchAudioEnable = raae;
        }

        if (bool.TryParse(settings.Element("RetroArchAudioMute")?.Value, out var raam))
        {
            RetroArchAudioMute = raam;
        }

        RetroArchMenuDriver = settings.Element("RetroArchMenuDriver")?.Value ?? RetroArchMenuDriver;
        if (bool.TryParse(settings.Element("RetroArchPauseNonActive")?.Value, out var rapna))
        {
            RetroArchPauseNonActive = rapna;
        }

        if (bool.TryParse(settings.Element("RetroArchSaveOnExit")?.Value, out var rasoe))
        {
            RetroArchSaveOnExit = rasoe;
        }

        if (bool.TryParse(settings.Element("RetroArchAutoSaveState")?.Value, out var raass))
        {
            RetroArchAutoSaveState = raass;
        }

        if (bool.TryParse(settings.Element("RetroArchAutoLoadState")?.Value, out var raals))
        {
            RetroArchAutoLoadState = raals;
        }

        if (bool.TryParse(settings.Element("RetroArchRewind")?.Value, out var rar))
        {
            RetroArchRewind = rar;
        }

        if (bool.TryParse(settings.Element("RetroArchThreadedVideo")?.Value, out var ratv))
        {
            RetroArchThreadedVideo = ratv;
        }

        if (bool.TryParse(settings.Element("RetroArchBilinear")?.Value, out var rab))
        {
            RetroArchBilinear = rab;
        }

        if (bool.TryParse(settings.Element("RetroArchShowSettingsBeforeLaunch")?.Value, out var rassbl))
        {
            RetroArchShowSettingsBeforeLaunch = rassbl;
        }

        RetroArchAspectRatioIndex = settings.Element("RetroArchAspectRatioIndex")?.Value ?? RetroArchAspectRatioIndex;
        if (bool.TryParse(settings.Element("RetroArchScaleInteger")?.Value, out var rasi))
        {
            RetroArchScaleInteger = rasi;
        }

        if (bool.TryParse(settings.Element("RetroArchShaderEnable")?.Value, out var rase))
        {
            RetroArchShaderEnable = rase;
        }

        if (bool.TryParse(settings.Element("RetroArchHardSync")?.Value, out var rahs))
        {
            RetroArchHardSync = rahs;
        }

        if (bool.TryParse(settings.Element("RetroArchRunAhead")?.Value, out var rara))
        {
            RetroArchRunAhead = rara;
        }

        if (bool.TryParse(settings.Element("RetroArchShowAdvancedSettings")?.Value, out var rasas))
        {
            RetroArchShowAdvancedSettings = rasas;
        }

        if (bool.TryParse(settings.Element("RetroArchDiscordAllow")?.Value, out var rada))
        {
            RetroArchDiscordAllow = rada;
        }

        if (bool.TryParse(settings.Element("RetroArchOverrideSystemDir")?.Value, out var raosd))
        {
            RetroArchOverrideSystemDir = raosd;
        }

        if (bool.TryParse(settings.Element("RetroArchOverrideSaveDir")?.Value, out var raovsd))
        {
            RetroArchOverrideSaveDir = raovsd;
        }

        if (bool.TryParse(settings.Element("RetroArchOverrideStateDir")?.Value, out var raovstd))
        {
            RetroArchOverrideStateDir = raovstd;
        }

        if (bool.TryParse(settings.Element("RetroArchOverrideScreenshotDir")?.Value, out var raovscd))
        {
            RetroArchOverrideScreenshotDir = raovscd;
        }

        // RPCS3
        Rpcs3Renderer = settings.Element("Rpcs3Renderer")?.Value ?? Rpcs3Renderer;
        Rpcs3Resolution = settings.Element("Rpcs3Resolution")?.Value ?? Rpcs3Resolution;
        Rpcs3AspectRatio = settings.Element("Rpcs3AspectRatio")?.Value ?? Rpcs3AspectRatio;
        if (bool.TryParse(settings.Element("Rpcs3Vsync")?.Value, out var rv))
        {
            Rpcs3Vsync = rv;
        }

        if (int.TryParse(settings.Element("Rpcs3ResolutionScale")?.Value, out var rrs))
        {
            Rpcs3ResolutionScale = rrs;
        }

        if (int.TryParse(settings.Element("Rpcs3AnisotropicFilter")?.Value, out var raf2))
        {
            Rpcs3AnisotropicFilter = raf2;
        }

        Rpcs3PpuDecoder = settings.Element("Rpcs3PpuDecoder")?.Value ?? Rpcs3PpuDecoder;
        Rpcs3SpuDecoder = settings.Element("Rpcs3SpuDecoder")?.Value ?? Rpcs3SpuDecoder;
        Rpcs3AudioRenderer = settings.Element("Rpcs3AudioRenderer")?.Value ?? Rpcs3AudioRenderer;
        if (bool.TryParse(settings.Element("Rpcs3AudioBuffering")?.Value, out var rabuf))
        {
            Rpcs3AudioBuffering = rabuf;
        }

        if (bool.TryParse(settings.Element("Rpcs3StartFullscreen")?.Value, out var rsf))
        {
            Rpcs3StartFullscreen = rsf;
        }

        if (bool.TryParse(settings.Element("Rpcs3ShowSettingsBeforeLaunch")?.Value, out var rssbl))
        {
            Rpcs3ShowSettingsBeforeLaunch = rssbl;
        }

        // Sega Model 2
        if (int.TryParse(settings.Element("SegaModel2ResX")?.Value, out var sm2Rx))
        {
            SegaModel2ResX = sm2Rx;
        }

        if (int.TryParse(settings.Element("SegaModel2ResY")?.Value, out var sm2Ry))
        {
            SegaModel2ResY = sm2Ry;
        }

        if (int.TryParse(settings.Element("SegaModel2WideScreen")?.Value, out var sm2Ws))
        {
            SegaModel2WideScreen = sm2Ws;
        }

        if (bool.TryParse(settings.Element("SegaModel2Bilinear")?.Value, out var sm2B))
        {
            SegaModel2Bilinear = sm2B;
        }

        if (bool.TryParse(settings.Element("SegaModel2Trilinear")?.Value, out var sm2T))
        {
            SegaModel2Trilinear = sm2T;
        }

        if (bool.TryParse(settings.Element("SegaModel2FilterTilemaps")?.Value, out var sm2Ft))
        {
            SegaModel2FilterTilemaps = sm2Ft;
        }

        if (bool.TryParse(settings.Element("SegaModel2DrawCross")?.Value, out var sm2dc))
        {
            SegaModel2DrawCross = sm2dc;
        }

        if (int.TryParse(settings.Element("SegaModel2Fsaa")?.Value, out var sm2Fsaa))
        {
            SegaModel2Fsaa = sm2Fsaa;
        }

        if (bool.TryParse(settings.Element("SegaModel2XInput")?.Value, out var sm2Xi))
        {
            SegaModel2XInput = sm2Xi;
        }

        if (bool.TryParse(settings.Element("SegaModel2EnableFf")?.Value, out var sm2Eff))
        {
            SegaModel2EnableFf = sm2Eff;
        }

        if (bool.TryParse(settings.Element("SegaModel2HoldGears")?.Value, out var sm2Hg))
        {
            SegaModel2HoldGears = sm2Hg;
        }

        if (bool.TryParse(settings.Element("SegaModel2UseRawInput")?.Value, out var sm2Uri))
        {
            SegaModel2UseRawInput = sm2Uri;
        }

        if (bool.TryParse(settings.Element("SegaModel2ShowSettingsBeforeLaunch")?.Value, out var sm2Ssbl))
        {
            SegaModel2ShowSettingsBeforeLaunch = sm2Ssbl;
        }

        // Stella
        if (bool.TryParse(settings.Element("StellaFullscreen")?.Value, out var stf))
        {
            StellaFullscreen = stf;
        }

        if (bool.TryParse(settings.Element("StellaVsync")?.Value, out var stv))
        {
            StellaVsync = stv;
        }

        StellaVideoDriver = settings.Element("StellaVideoDriver")?.Value ?? StellaVideoDriver;
        if (bool.TryParse(settings.Element("StellaCorrectAspect")?.Value, out var stca))
        {
            StellaCorrectAspect = stca;
        }

        if (int.TryParse(settings.Element("StellaTvFilter")?.Value, out var sttf))
        {
            StellaTvFilter = sttf;
        }

        if (int.TryParse(settings.Element("StellaScanlines")?.Value, out var sts))
        {
            StellaScanlines = sts;
        }

        if (bool.TryParse(settings.Element("StellaAudioEnabled")?.Value, out var stae))
        {
            StellaAudioEnabled = stae;
        }

        if (int.TryParse(settings.Element("StellaAudioVolume")?.Value, out var stav))
        {
            StellaAudioVolume = stav;
        }

        if (bool.TryParse(settings.Element("StellaTimeMachine")?.Value, out var stm))
        {
            StellaTimeMachine = stm;
        }

        if (bool.TryParse(settings.Element("StellaConfirmExit")?.Value, out var stce))
        {
            StellaConfirmExit = stce;
        }

        if (bool.TryParse(settings.Element("StellaShowSettingsBeforeLaunch")?.Value, out var stssbl))
        {
            StellaShowSettingsBeforeLaunch = stssbl;
        }

        // Supermodel
        if (bool.TryParse(settings.Element("SupermodelNew3DEngine")?.Value, out var smn3E))
        {
            SupermodelNew3DEngine = smn3E;
        }

        if (bool.TryParse(settings.Element("SupermodelQuadRendering")?.Value, out var smqr))
        {
            SupermodelQuadRendering = smqr;
        }

        if (bool.TryParse(settings.Element("SupermodelFullscreen")?.Value, out var smfs))
        {
            SupermodelFullscreen = smfs;
        }

        if (int.TryParse(settings.Element("SupermodelResX")?.Value, out var smrx))
        {
            SupermodelResX = smrx;
        }

        if (int.TryParse(settings.Element("SupermodelResY")?.Value, out var smry))
        {
            SupermodelResY = smry;
        }

        if (bool.TryParse(settings.Element("SupermodelWideScreen")?.Value, out var smws))
        {
            SupermodelWideScreen = smws;
        }

        if (bool.TryParse(settings.Element("SupermodelStretch")?.Value, out var smst))
        {
            SupermodelStretch = smst;
        }

        if (bool.TryParse(settings.Element("SupermodelVsync")?.Value, out var smvs))
        {
            SupermodelVsync = smvs;
        }

        if (bool.TryParse(settings.Element("SupermodelThrottle")?.Value, out var smth))
        {
            SupermodelThrottle = smth;
        }

        if (int.TryParse(settings.Element("SupermodelMusicVolume")?.Value, out var smmv))
        {
            SupermodelMusicVolume = smmv;
        }

        if (int.TryParse(settings.Element("SupermodelSoundVolume")?.Value, out var ssv))
        {
            SupermodelSoundVolume = ssv;
        }

        SupermodelInputSystem = ValidateSupermodelInputSystem(settings.Element("SupermodelInputSystem")?.Value);
        if (bool.TryParse(settings.Element("SupermodelMultiThreaded")?.Value, out var smmt))
        {
            SupermodelMultiThreaded = smmt;
        }

        if (int.TryParse(settings.Element("SupermodelPowerPcFrequency")?.Value, out var smppf))
        {
            SupermodelPowerPcFrequency = smppf;
        }

        if (bool.TryParse(settings.Element("SupermodelShowSettingsBeforeLaunch")?.Value, out var smssbl))
        {
            SupermodelShowSettingsBeforeLaunch = smssbl;
        }

        // Xenia
        XeniaReadbackResolve = settings.Element("XeniaReadbackResolve")?.Value ?? XeniaReadbackResolve;
        if (bool.TryParse(settings.Element("XeniaGammaSrgb")?.Value, out var xgs))
        {
            XeniaGammaSrgb = xgs;
        }

        if (bool.TryParse(settings.Element("XeniaVibration")?.Value, out var xvib))
        {
            XeniaVibration = xvib;
        }

        if (bool.TryParse(settings.Element("XeniaMountCache")?.Value, out var xmc))
        {
            XeniaMountCache = xmc;
        }

        XeniaGpu = settings.Element("XeniaGpu")?.Value ?? XeniaGpu;
        if (bool.TryParse(settings.Element("XeniaVsync")?.Value, out var xvs))
        {
            XeniaVsync = xvs;
        }

        if (int.TryParse(settings.Element("XeniaResScaleX")?.Value, out var xrsx))
        {
            XeniaResScaleX = xrsx;
        }

        if (int.TryParse(settings.Element("XeniaResScaleY")?.Value, out var xrsy))
        {
            XeniaResScaleY = xrsy;
        }

        if (bool.TryParse(settings.Element("XeniaFullscreen")?.Value, out var xfs))
        {
            XeniaFullscreen = xfs;
        }

        XeniaApu = settings.Element("XeniaApu")?.Value ?? XeniaApu;
        if (bool.TryParse(settings.Element("XeniaMute")?.Value, out var xmu))
        {
            XeniaMute = xmu;
        }

        XeniaAa = settings.Element("XeniaAa")?.Value ?? XeniaAa;
        XeniaScaling = settings.Element("XeniaScaling")?.Value ?? XeniaScaling;
        if (bool.TryParse(settings.Element("XeniaApplyPatches")?.Value, out var xap))
        {
            XeniaApplyPatches = xap;
        }

        if (bool.TryParse(settings.Element("XeniaDiscordPresence")?.Value, out var xdp))
        {
            XeniaDiscordPresence = xdp;
        }

        if (int.TryParse(settings.Element("XeniaUserLanguage")?.Value, out var xul))
        {
            XeniaUserLanguage = xul;
        }

        XeniaHid = settings.Element("XeniaHid")?.Value ?? XeniaHid;
        if (bool.TryParse(settings.Element("XeniaShowSettingsBeforeLaunch")?.Value, out var xssbl))
        {
            XeniaShowSettingsBeforeLaunch = xssbl;
        }

        // Yumir
        if (bool.TryParse(settings.Element("YumirFullscreen")?.Value, out var yf))
        {
            YumirFullscreen = yf;
        }

        if (double.TryParse(settings.Element("YumirVolume")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var yv))
        {
            YumirVolume = yv;
        }

        if (bool.TryParse(settings.Element("YumirMute")?.Value, out var ym))
        {
            YumirMute = ym;
        }

        YumirVideoStandard = settings.Element("YumirVideoStandard")?.Value ?? YumirVideoStandard;
        if (bool.TryParse(settings.Element("YumirAutoDetectRegion")?.Value, out var yadr))
        {
            YumirAutoDetectRegion = yadr;
        }

        if (bool.TryParse(settings.Element("YumirPauseWhenUnfocused")?.Value, out var ypwu))
        {
            YumirPauseWhenUnfocused = ypwu;
        }

        if (double.TryParse(settings.Element("YumirForcedAspect")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var yfa))
        {
            YumirForcedAspect = yfa;
        }

        if (bool.TryParse(settings.Element("YumirForceAspectRatio")?.Value, out var yfar))
        {
            YumirForceAspectRatio = yfar;
        }

        if (bool.TryParse(settings.Element("YumirReduceLatency")?.Value, out var yrl))
        {
            YumirReduceLatency = yrl;
        }

        if (bool.TryParse(settings.Element("YumirShowSettingsBeforeLaunch")?.Value, out var yssbl))
        {
            YumirShowSettingsBeforeLaunch = yssbl;
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
                    new XElement("Emulator5Expanded", Emulator5Expanded),

                    // Ares
                    new XElement("AresVideoDriver", AresVideoDriver),
                    new XElement("AresExclusive", AresExclusive),
                    new XElement("AresShader", AresShader),
                    new XElement("AresMultiplier", AresMultiplier),
                    new XElement("AresAspectCorrection", AresAspectCorrection),
                    new XElement("AresMute", AresMute),
                    new XElement("AresVolume", AresVolume.ToString(CultureInfo.InvariantCulture)),
                    new XElement("AresFastBoot", AresFastBoot),
                    new XElement("AresRewind", AresRewind),
                    new XElement("AresRunAhead", AresRunAhead),
                    new XElement("AresAutoSaveMemory", AresAutoSaveMemory),
                    new XElement("AresShowSettingsBeforeLaunch", AresShowSettingsBeforeLaunch),

                    // Azahar
                    new XElement("AzaharGraphicsApi", AzaharGraphicsApi),
                    new XElement("AzaharResolutionFactor", AzaharResolutionFactor),
                    new XElement("AzaharUseVsync", AzaharUseVsync),
                    new XElement("AzaharAsyncShaderCompilation", AzaharAsyncShaderCompilation),
                    new XElement("AzaharFullscreen", AzaharFullscreen),
                    new XElement("AzaharVolume", AzaharVolume),
                    new XElement("AzaharIsNew3ds", AzaharIsNew3ds),
                    new XElement("AzaharLayoutOption", AzaharLayoutOption),
                    new XElement("AzaharShowSettingsBeforeLaunch", AzaharShowSettingsBeforeLaunch),
                    new XElement("AzaharEnableAudioStretching", AzaharEnableAudioStretching),

                    // Blastem
                    new XElement("BlastemFullscreen", BlastemFullscreen),
                    new XElement("BlastemVsync", BlastemVsync),
                    new XElement("BlastemAspect", BlastemAspect),
                    new XElement("BlastemScaling", BlastemScaling),
                    new XElement("BlastemScanlines", BlastemScanlines),
                    new XElement("BlastemAudioRate", BlastemAudioRate),
                    new XElement("BlastemSyncSource", BlastemSyncSource),
                    new XElement("BlastemShowSettingsBeforeLaunch", BlastemShowSettingsBeforeLaunch),

                    // Cemu
                    new XElement("CemuFullscreen", CemuFullscreen),
                    new XElement("CemuGraphicApi", CemuGraphicApi),
                    new XElement("CemuVsync", CemuVsync),
                    new XElement("CemuAsyncCompile", CemuAsyncCompile),
                    new XElement("CemuTvVolume", CemuTvVolume),
                    new XElement("CemuConsoleLanguage", CemuConsoleLanguage),
                    new XElement("CemuDiscordPresence", CemuDiscordPresence),
                    new XElement("CemuShowSettingsBeforeLaunch", CemuShowSettingsBeforeLaunch),

                    // Daphne
                    new XElement("DaphneFullscreen", DaphneFullscreen),
                    new XElement("DaphneResX", DaphneResX),
                    new XElement("DaphneResY", DaphneResY),
                    new XElement("DaphneDisableCrosshairs", DaphneDisableCrosshairs),
                    new XElement("DaphneBilinear", DaphneBilinear),
                    new XElement("DaphneEnableSound", DaphneEnableSound),
                    new XElement("DaphneUseOverlays", DaphneUseOverlays),
                    new XElement("DaphneShowSettingsBeforeLaunch", DaphneShowSettingsBeforeLaunch),

                    // Dolphin
                    new XElement("DolphinGfxBackend", DolphinGfxBackend),
                    new XElement("DolphinDspThread", DolphinDspThread),
                    new XElement("DolphinWiimoteContinuousScanning", DolphinWiimoteContinuousScanning),
                    new XElement("DolphinWiimoteEnableSpeaker", DolphinWiimoteEnableSpeaker),
                    new XElement("DolphinShowSettingsBeforeLaunch", DolphinShowSettingsBeforeLaunch),

                    // DuckStation
                    new XElement("DuckStationStartFullscreen", DuckStationStartFullscreen),
                    new XElement("DuckStationPauseOnFocusLoss", DuckStationPauseOnFocusLoss),
                    new XElement("DuckStationSaveStateOnExit", DuckStationSaveStateOnExit),
                    new XElement("DuckStationRewindEnable", DuckStationRewindEnable),
                    new XElement("DuckStationRunaheadFrameCount", DuckStationRunaheadFrameCount),
                    new XElement("DuckStationRenderer", DuckStationRenderer),
                    new XElement("DuckStationResolutionScale", DuckStationResolutionScale),
                    new XElement("DuckStationTextureFilter", DuckStationTextureFilter),
                    new XElement("DuckStationWidescreenHack", DuckStationWidescreenHack),
                    new XElement("DuckStationPgxpEnable", DuckStationPgxpEnable),
                    new XElement("DuckStationAspectRatio", DuckStationAspectRatio),
                    new XElement("DuckStationVsync", DuckStationVsync),
                    new XElement("DuckStationOutputVolume", DuckStationOutputVolume),
                    new XElement("DuckStationOutputMuted", DuckStationOutputMuted),
                    new XElement("DuckStationShowSettingsBeforeLaunch", DuckStationShowSettingsBeforeLaunch),

                    // Flycast
                    new XElement("FlycastFullscreen", FlycastFullscreen),
                    new XElement("FlycastWidth", FlycastWidth),
                    new XElement("FlycastHeight", FlycastHeight),
                    new XElement("FlycastMaximized", FlycastMaximized),
                    new XElement("FlycastShowSettingsBeforeLaunch", FlycastShowSettingsBeforeLaunch),

                    // MAME
                    new XElement("MameVideo", MameVideo),
                    new XElement("MameWindow", MameWindow),
                    new XElement("MameMaximize", MameMaximize),
                    new XElement("MameKeepAspect", MameKeepAspect),
                    new XElement("MameSkipGameInfo", MameSkipGameInfo),
                    new XElement("MameAutosave", MameAutosave),
                    new XElement("MameConfirmQuit", MameConfirmQuit),
                    new XElement("MameJoystick", MameJoystick),
                    new XElement("MameShowSettingsBeforeLaunch", MameShowSettingsBeforeLaunch),
                    new XElement("MameAutoframeskip", MameAutoframeskip),
                    new XElement("MameBgfxBackend", MameBgfxBackend),
                    new XElement("MameBgfxScreenChains", MameBgfxScreenChains),
                    new XElement("MameFilter", MameFilter),
                    new XElement("MameCheat", MameCheat),
                    new XElement("MameRewind", MameRewind),
                    new XElement("MameNvramSave", MameNvramSave),

                    // Mednafen
                    new XElement("MednafenVideoDriver", MednafenVideoDriver),
                    new XElement("MednafenFullscreen", MednafenFullscreen),
                    new XElement("MednafenVsync", MednafenVsync),
                    new XElement("MednafenStretch", MednafenStretch),
                    new XElement("MednafenBilinear", MednafenBilinear),
                    new XElement("MednafenScanlines", MednafenScanlines),
                    new XElement("MednafenShader", MednafenShader),
                    new XElement("MednafenVolume", MednafenVolume),
                    new XElement("MednafenCheats", MednafenCheats),
                    new XElement("MednafenRewind", MednafenRewind),
                    new XElement("MednafenShowSettingsBeforeLaunch", MednafenShowSettingsBeforeLaunch),

                    // Mesen
                    new XElement("MesenFullscreen", MesenFullscreen),
                    new XElement("MesenVsync", MesenVsync),
                    new XElement("MesenAspectRatio", MesenAspectRatio),
                    new XElement("MesenBilinear", MesenBilinear),
                    new XElement("MesenVideoFilter", MesenVideoFilter),
                    new XElement("MesenEnableAudio", MesenEnableAudio),
                    new XElement("MesenMasterVolume", MesenMasterVolume),
                    new XElement("MesenRewind", MesenRewind),
                    new XElement("MesenRunAhead", MesenRunAhead),
                    new XElement("MesenPauseInBackground", MesenPauseInBackground),
                    new XElement("MesenShowSettingsBeforeLaunch", MesenShowSettingsBeforeLaunch),

                    // PCSX2
                    new XElement("Pcsx2StartFullscreen", Pcsx2StartFullscreen),
                    new XElement("Pcsx2AspectRatio", Pcsx2AspectRatio),
                    new XElement("Pcsx2Renderer", Pcsx2Renderer),
                    new XElement("Pcsx2UpscaleMultiplier", Pcsx2UpscaleMultiplier),
                    new XElement("Pcsx2Vsync", Pcsx2Vsync),
                    new XElement("Pcsx2EnableCheats", Pcsx2EnableCheats),
                    new XElement("Pcsx2EnableWidescreenPatches", Pcsx2EnableWidescreenPatches),
                    new XElement("Pcsx2Volume", Pcsx2Volume),
                    new XElement("Pcsx2AchievementsEnabled", Pcsx2AchievementsEnabled),
                    new XElement("Pcsx2AchievementsHardcore", Pcsx2AchievementsHardcore),
                    new XElement("Pcsx2ShowSettingsBeforeLaunch", Pcsx2ShowSettingsBeforeLaunch),

                    // RetroArch
                    new XElement("RetroArchCheevosEnable", RetroArchCheevosEnable),
                    new XElement("RetroArchCheevosHardcore", RetroArchCheevosHardcore),
                    new XElement("RetroArchFullscreen", RetroArchFullscreen),
                    new XElement("RetroArchVsync", RetroArchVsync),
                    new XElement("RetroArchVideoDriver", RetroArchVideoDriver),
                    new XElement("RetroArchAudioEnable", RetroArchAudioEnable),
                    new XElement("RetroArchAudioMute", RetroArchAudioMute),
                    new XElement("RetroArchMenuDriver", RetroArchMenuDriver),
                    new XElement("RetroArchPauseNonActive", RetroArchPauseNonActive),
                    new XElement("RetroArchSaveOnExit", RetroArchSaveOnExit),
                    new XElement("RetroArchAutoSaveState", RetroArchAutoSaveState),
                    new XElement("RetroArchAutoLoadState", RetroArchAutoLoadState),
                    new XElement("RetroArchRewind", RetroArchRewind),
                    new XElement("RetroArchThreadedVideo", RetroArchThreadedVideo),
                    new XElement("RetroArchBilinear", RetroArchBilinear),
                    new XElement("RetroArchShowSettingsBeforeLaunch", RetroArchShowSettingsBeforeLaunch),
                    new XElement("RetroArchAspectRatioIndex", RetroArchAspectRatioIndex),
                    new XElement("RetroArchScaleInteger", RetroArchScaleInteger),
                    new XElement("RetroArchShaderEnable", RetroArchShaderEnable),
                    new XElement("RetroArchHardSync", RetroArchHardSync),
                    new XElement("RetroArchRunAhead", RetroArchRunAhead),
                    new XElement("RetroArchShowAdvancedSettings", RetroArchShowAdvancedSettings),
                    new XElement("RetroArchDiscordAllow", RetroArchDiscordAllow),
                    new XElement("RetroArchOverrideSystemDir", RetroArchOverrideSystemDir),
                    new XElement("RetroArchOverrideSaveDir", RetroArchOverrideSaveDir),
                    new XElement("RetroArchOverrideStateDir", RetroArchOverrideStateDir),
                    new XElement("RetroArchOverrideScreenshotDir", RetroArchOverrideScreenshotDir),

                    // RPCS3
                    new XElement("Rpcs3Renderer", Rpcs3Renderer),
                    new XElement("Rpcs3Resolution", Rpcs3Resolution),
                    new XElement("Rpcs3AspectRatio", Rpcs3AspectRatio),
                    new XElement("Rpcs3Vsync", Rpcs3Vsync),
                    new XElement("Rpcs3ResolutionScale", Rpcs3ResolutionScale),
                    new XElement("Rpcs3AnisotropicFilter", Rpcs3AnisotropicFilter),
                    new XElement("Rpcs3PpuDecoder", Rpcs3PpuDecoder),
                    new XElement("Rpcs3SpuDecoder", Rpcs3SpuDecoder),
                    new XElement("Rpcs3AudioRenderer", Rpcs3AudioRenderer),
                    new XElement("Rpcs3AudioBuffering", Rpcs3AudioBuffering),
                    new XElement("Rpcs3StartFullscreen", Rpcs3StartFullscreen),
                    new XElement("Rpcs3ShowSettingsBeforeLaunch", Rpcs3ShowSettingsBeforeLaunch),

                    // SEGA Model 2
                    new XElement("SegaModel2ResX", SegaModel2ResX),
                    new XElement("SegaModel2ResY", SegaModel2ResY),
                    new XElement("SegaModel2WideScreen", SegaModel2WideScreen),
                    new XElement("SegaModel2Bilinear", SegaModel2Bilinear),
                    new XElement("SegaModel2Trilinear", SegaModel2Trilinear),
                    new XElement("SegaModel2FilterTilemaps", SegaModel2FilterTilemaps),
                    new XElement("SegaModel2DrawCross", SegaModel2DrawCross),
                    new XElement("SegaModel2Fsaa", SegaModel2Fsaa),
                    new XElement("SegaModel2XInput", SegaModel2XInput),
                    new XElement("SegaModel2EnableFf", SegaModel2EnableFf),
                    new XElement("SegaModel2HoldGears", SegaModel2HoldGears),
                    new XElement("SegaModel2UseRawInput", SegaModel2UseRawInput),
                    new XElement("SegaModel2ShowSettingsBeforeLaunch", SegaModel2ShowSettingsBeforeLaunch),

                    // Stella
                    new XElement("StellaFullscreen", StellaFullscreen),
                    new XElement("StellaVsync", StellaVsync),
                    new XElement("StellaVideoDriver", StellaVideoDriver),
                    new XElement("StellaCorrectAspect", StellaCorrectAspect),
                    new XElement("StellaTvFilter", StellaTvFilter),
                    new XElement("StellaScanlines", StellaScanlines),
                    new XElement("StellaAudioEnabled", StellaAudioEnabled),
                    new XElement("StellaAudioVolume", StellaAudioVolume),
                    new XElement("StellaTimeMachine", StellaTimeMachine),
                    new XElement("StellaConfirmExit", StellaConfirmExit),
                    new XElement("StellaShowSettingsBeforeLaunch", StellaShowSettingsBeforeLaunch),

                    // Supermodel
                    new XElement("SupermodelNew3DEngine", SupermodelNew3DEngine),
                    new XElement("SupermodelQuadRendering", SupermodelQuadRendering),
                    new XElement("SupermodelFullscreen", SupermodelFullscreen),
                    new XElement("SupermodelResX", SupermodelResX),
                    new XElement("SupermodelResY", SupermodelResY),
                    new XElement("SupermodelWideScreen", SupermodelWideScreen),
                    new XElement("SupermodelStretch", SupermodelStretch),
                    new XElement("SupermodelVsync", SupermodelVsync),
                    new XElement("SupermodelThrottle", SupermodelThrottle),
                    new XElement("SupermodelMusicVolume", SupermodelMusicVolume),
                    new XElement("SupermodelSoundVolume", SupermodelSoundVolume),
                    new XElement("SupermodelInputSystem", SupermodelInputSystem),
                    new XElement("SupermodelMultiThreaded", SupermodelMultiThreaded),
                    new XElement("SupermodelPowerPcFrequency", SupermodelPowerPcFrequency),
                    new XElement("SupermodelShowSettingsBeforeLaunch", SupermodelShowSettingsBeforeLaunch),

                    // Xenia
                    new XElement("XeniaReadbackResolve", XeniaReadbackResolve),
                    new XElement("XeniaGammaSrgb", XeniaGammaSrgb),
                    new XElement("XeniaVibration", XeniaVibration),
                    new XElement("XeniaMountCache", XeniaMountCache),
                    new XElement("XeniaGpu", XeniaGpu),
                    new XElement("XeniaVsync", XeniaVsync),
                    new XElement("XeniaResScaleX", XeniaResScaleX),
                    new XElement("XeniaResScaleY", XeniaResScaleY),
                    new XElement("XeniaFullscreen", XeniaFullscreen),
                    new XElement("XeniaApu", XeniaApu),
                    new XElement("XeniaMute", XeniaMute),
                    new XElement("XeniaAa", XeniaAa),
                    new XElement("XeniaScaling", XeniaScaling),
                    new XElement("XeniaApplyPatches", XeniaApplyPatches),
                    new XElement("XeniaDiscordPresence", XeniaDiscordPresence),
                    new XElement("XeniaUserLanguage", XeniaUserLanguage),
                    new XElement("XeniaHid", XeniaHid),
                    new XElement("XeniaShowSettingsBeforeLaunch", XeniaShowSettingsBeforeLaunch),

                    // Yumir
                    new XElement("YumirFullscreen", YumirFullscreen),
                    new XElement("YumirVolume", YumirVolume.ToString(CultureInfo.InvariantCulture)),
                    new XElement("YumirMute", YumirMute),
                    new XElement("YumirVideoStandard", YumirVideoStandard),
                    new XElement("YumirAutoDetectRegion", YumirAutoDetectRegion),
                    new XElement("YumirPauseWhenUnfocused", YumirPauseWhenUnfocused),
                    new XElement("YumirForcedAspect", YumirForcedAspect.ToString(CultureInfo.InvariantCulture)),
                    new XElement("YumirForceAspectRatio", YumirForceAspectRatio),
                    new XElement("YumirReduceLatency", YumirReduceLatency),
                    new XElement("YumirShowSettingsBeforeLaunch", YumirShowSettingsBeforeLaunch),

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
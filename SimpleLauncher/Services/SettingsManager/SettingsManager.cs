using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.SharedModels;
using Microsoft.Extensions.Configuration;

namespace SimpleLauncher.Services.SettingsManager;

public class SettingsManager : IDisposable
{
    private readonly IConfiguration _configuration;

    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultSettingsFilePath);
    private readonly ReaderWriterLockSlim _settingsLock = new(LockRecursionPolicy.SupportsRecursion);

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
    public string VideoUrl { get; set; }
    public string InfoUrl { get; set; }
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
    public string MednafenSpecial { get; set; } = "none";
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

    // Raine
    public bool RaineFullscreen { get; set; }
    public int RaineResX { get; set; } = 640;
    public int RaineResY { get; set; } = 480;
    public bool RaineFixAspectRatio { get; set; } = true;
    public bool RaineVsync { get; set; } = true;
    public string RaineSoundDriver { get; set; } = "directsound";
    public int RaineSampleRate { get; set; } = 44100;
    public bool RaineShowSettingsBeforeLaunch { get; set; } = true;
    public bool RaineShowFps { get; set; }
    public int RaineFrameSkip { get; set; }
    public string RaineNeoCdBios { get; set; } = string.Empty;
    public int RaineMusicVolume { get; set; } = 60;
    public int RaineSfxVolume { get; set; } = 60;
    public bool RaineMuteSfx { get; set; }
    public bool RaineMuteMusic { get; set; }
    public string RaineRomDirectory { get; set; } = string.Empty;

    // Redream
    public string RedreamCable { get; set; } = "vga";
    public string RedreamBroadcast { get; set; } = "ntsc";
    public string RedreamLanguage { get; set; } = "english";
    public string RedreamRegion { get; set; } = "usa";
    public bool RedreamVsync { get; set; } = true;
    public bool RedreamFrameskip { get; set; } = true; // 1 = auto, 0 = off
    public string RedreamAspect { get; set; } = "4:3";
    public int RedreamRes { get; set; } = 2;
    public string RedreamRenderer { get; set; } = "hle_perstrip";
    public string RedreamFullmode { get; set; } = "exclusive fullscreen";
    public int RedreamVolume { get; set; } = 100;
    public int RedreamLatency { get; set; } = 32;
    public bool RedreamFramerate { get; set; }
    public int RedreamWidth { get; set; } = 1280;
    public int RedreamHeight { get; set; } = 720;
    public bool RedreamShowSettingsBeforeLaunch { get; set; } = true;

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

    public SettingsManager(IConfiguration configuration)
    {
        _configuration = configuration;

        // Initialize properties that depend on configuration
        VideoUrl = configuration.GetValue<string>("Urls:YouTubeSearch") ?? "https://www.youtube.com/results?search_query=";
        InfoUrl = configuration.GetValue<string>("Urls:IgdbSearch") ?? "https://www.igdb.com/search?q=";
    }

    public void Load()
    {
        _settingsLock.EnterWriteLock();
        try
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
        finally
        {
            _settingsLock.ExitWriteLock();
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

        // Deep copy of SystemPlayTimes to ensure snapshot isolation
        SystemPlayTimes = other.SystemPlayTimes?
            .Select(static pt => new SystemPlayTime { SystemName = pt.SystemName, PlayTime = pt.PlayTime })
            .ToList() ?? [];

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
        MednafenSpecial = other.MednafenSpecial;
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

        // Raine
        RaineFullscreen = other.RaineFullscreen;
        RaineResX = other.RaineResX;
        RaineResY = other.RaineResY;
        RaineFixAspectRatio = other.RaineFixAspectRatio;
        RaineVsync = other.RaineVsync;
        RaineSoundDriver = other.RaineSoundDriver;
        RaineSampleRate = other.RaineSampleRate;
        RaineShowSettingsBeforeLaunch = other.RaineShowSettingsBeforeLaunch;
        RaineShowFps = other.RaineShowFps;
        RaineFrameSkip = other.RaineFrameSkip;
        RaineNeoCdBios = other.RaineNeoCdBios;
        RaineMusicVolume = other.RaineMusicVolume;
        RaineSfxVolume = other.RaineSfxVolume;
        RaineMuteSfx = other.RaineMuteSfx;
        RaineMuteMusic = other.RaineMuteMusic;
        RaineRomDirectory = other.RaineRomDirectory;

        // Redream
        RedreamCable = other.RedreamCable;
        RedreamBroadcast = other.RedreamBroadcast;
        RedreamLanguage = other.RedreamLanguage;
        RedreamRegion = other.RedreamRegion;
        RedreamVsync = other.RedreamVsync;
        RedreamFrameskip = other.RedreamFrameskip;
        RedreamAspect = other.RedreamAspect;
        RedreamRes = other.RedreamRes;
        RedreamRenderer = other.RedreamRenderer;
        RedreamFullmode = other.RedreamFullmode;
        RedreamVolume = other.RedreamVolume;
        RedreamLatency = other.RedreamLatency;
        RedreamFramerate = other.RedreamFramerate;
        RedreamWidth = other.RedreamWidth;
        RedreamHeight = other.RedreamHeight;
        RedreamShowSettingsBeforeLaunch = other.RedreamShowSettingsBeforeLaunch;

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

        // Application Settings Fallback Logic
        var app = settings.Element("Application");
        ThumbnailSize = ValidateThumbnailSize(app?.Element("ThumbnailSize")?.Value ?? settings.Element("ThumbnailSize")?.Value);
        GamesPerPage = ValidateGamesPerPage(app?.Element("GamesPerPage")?.Value ?? settings.Element("GamesPerPage")?.Value);
        ShowGames = ValidateShowGames(app?.Element("ShowGames")?.Value ?? settings.Element("ShowGames")?.Value);
        ViewMode = ValidateViewMode(app?.Element("ViewMode")?.Value ?? settings.Element("ViewMode")?.Value);

        if (bool.TryParse(app?.Element("EnableGamePadNavigation")?.Value ?? settings.Element("EnableGamePadNavigation")?.Value, out var gp))
        {
            EnableGamePadNavigation = gp;
        }

        VideoUrl = app?.Element("VideoUrl")?.Value ?? settings.Element("VideoUrl")?.Value ?? VideoUrl;
        InfoUrl = app?.Element("InfoUrl")?.Value ?? settings.Element("InfoUrl")?.Value ?? InfoUrl;
        BaseTheme = app?.Element("BaseTheme")?.Value ?? settings.Element("BaseTheme")?.Value ?? BaseTheme;
        AccentColor = app?.Element("AccentColor")?.Value ?? settings.Element("AccentColor")?.Value ?? AccentColor;
        Language = app?.Element("Language")?.Value ?? settings.Element("Language")?.Value ?? Language;
        ButtonAspectRatio = ValidateButtonAspectRatio(app?.Element("ButtonAspectRatio")?.Value ?? settings.Element("ButtonAspectRatio")?.Value);

        // RetroAchievements mapping (Old used RA_Username, New uses RaUsername)
        RaUsername = app?.Element("RaUsername")?.Value ?? settings.Element("RaUsername")?.Value ?? settings.Element("RA_Username")?.Value ?? RaUsername;
        RaApiKey = app?.Element("RaApiKey")?.Value ?? settings.Element("RaApiKey")?.Value ?? settings.Element("RA_ApiKey")?.Value ?? RaApiKey;
        RaPassword = app?.Element("RaPassword")?.Value ?? settings.Element("RaPassword")?.Value ?? RaPassword;
        RaToken = app?.Element("RaToken")?.Value ?? settings.Element("RaToken")?.Value ?? RaToken;

        if (float.TryParse(app?.Element("DeadZoneX")?.Value ?? settings.Element("DeadZoneX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzx))
        {
            DeadZoneX = dzx;
        }

        if (float.TryParse(app?.Element("DeadZoneY")?.Value ?? settings.Element("DeadZoneY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzy))
        {
            DeadZoneY = dzy;
        }

        if (bool.TryParse(app?.Element("EnableFuzzyMatching")?.Value ?? settings.Element("EnableFuzzyMatching")?.Value, out var fm))
        {
            EnableFuzzyMatching = fm;
        }

        if (double.TryParse(app?.Element("FuzzyMatchingThreshold")?.Value ?? settings.Element("FuzzyMatchingThreshold")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fmt))
        {
            FuzzyMatchingThreshold = fmt;
        }

        if (bool.TryParse(app?.Element("EnableNotificationSound")?.Value ?? settings.Element("EnableNotificationSound")?.Value, out var ens))
        {
            EnableNotificationSound = ens;
        }

        CustomNotificationSoundFile = app?.Element("CustomNotificationSoundFile")?.Value ?? settings.Element("CustomNotificationSoundFile")?.Value ?? CustomNotificationSoundFile;
        if (bool.TryParse(app?.Element("OverlayRetroAchievementButton")?.Value ?? settings.Element("OverlayRetroAchievementButton")?.Value, out var ora))
        {
            OverlayRetroAchievementButton = ora;
        }

        if (bool.TryParse(app?.Element("OverlayOpenVideoButton")?.Value ?? settings.Element("OverlayOpenVideoButton")?.Value, out var ovb))
        {
            OverlayOpenVideoButton = ovb;
        }

        if (bool.TryParse(app?.Element("OverlayOpenInfoButton")?.Value ?? settings.Element("OverlayOpenInfoButton")?.Value, out var oib))
        {
            OverlayOpenInfoButton = oib;
        }

        if (bool.TryParse(app?.Element("AdditionalSystemFoldersExpanded")?.Value ?? settings.Element("AdditionalSystemFoldersExpanded")?.Value, out var asfe))
        {
            AdditionalSystemFoldersExpanded = asfe;
        }

        if (bool.TryParse(app?.Element("Emulator1Expanded")?.Value ?? settings.Element("Emulator1Expanded")?.Value, out var e1E))
        {
            Emulator1Expanded = e1E;
        }

        if (bool.TryParse(app?.Element("Emulator2Expanded")?.Value ?? settings.Element("Emulator2Expanded")?.Value, out var e2E))
        {
            Emulator2Expanded = e2E;
        }

        if (bool.TryParse(app?.Element("Emulator3Expanded")?.Value ?? settings.Element("Emulator3Expanded")?.Value, out var e3E))
        {
            Emulator3Expanded = e3E;
        }

        if (bool.TryParse(app?.Element("Emulator4Expanded")?.Value ?? settings.Element("Emulator4Expanded")?.Value, out var e4E))
        {
            Emulator4Expanded = e4E;
        }

        if (bool.TryParse(app?.Element("Emulator5Expanded")?.Value ?? settings.Element("Emulator5Expanded")?.Value, out var e5E))
        {
            Emulator5Expanded = e5E;
        }

        // Ares Fallback
        var ares = settings.Element("Ares");
        AresVideoDriver = ares?.Element("VideoDriver")?.Value ?? settings.Element("AresVideoDriver")?.Value ?? AresVideoDriver;
        if (bool.TryParse(ares?.Element("Exclusive")?.Value ?? settings.Element("AresExclusive")?.Value, out var ae))
        {
            AresExclusive = ae;
        }

        AresShader = ares?.Element("Shader")?.Value ?? settings.Element("AresShader")?.Value ?? AresShader;
        if (int.TryParse(ares?.Element("Multiplier")?.Value ?? settings.Element("AresMultiplier")?.Value, out var am))
        {
            AresMultiplier = am;
        }

        AresAspectCorrection = ares?.Element("AspectCorrection")?.Value ?? settings.Element("AresAspectCorrection")?.Value ?? AresAspectCorrection;
        if (bool.TryParse(ares?.Element("Mute")?.Value ?? settings.Element("AresMute")?.Value, out var amu))
        {
            AresMute = amu;
        }

        if (double.TryParse(ares?.Element("Volume")?.Value ?? settings.Element("AresVolume")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var av))
        {
            AresVolume = av;
        }

        if (bool.TryParse(ares?.Element("FastBoot")?.Value ?? settings.Element("AresFastBoot")?.Value, out var afb))
        {
            AresFastBoot = afb;
        }

        if (bool.TryParse(ares?.Element("Rewind")?.Value ?? settings.Element("AresRewind")?.Value, out var ar))
        {
            AresRewind = ar;
        }

        if (bool.TryParse(ares?.Element("RunAhead")?.Value ?? settings.Element("AresRunAhead")?.Value, out var ara))
        {
            AresRunAhead = ara;
        }

        if (bool.TryParse(ares?.Element("AutoSaveMemory")?.Value ?? settings.Element("AresAutoSaveMemory")?.Value, out var asm))
        {
            AresAutoSaveMemory = asm;
        }

        if (bool.TryParse(ares?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("AresShowSettingsBeforeLaunch")?.Value, out var assbl))
        {
            AresShowSettingsBeforeLaunch = assbl;
        }

        // Azahar Fallback
        var azahar = settings.Element("Azahar");
        if (int.TryParse(azahar?.Element("GraphicsApi")?.Value ?? settings.Element("AzaharGraphicsApi")?.Value, out var aga))
        {
            AzaharGraphicsApi = aga;
        }

        if (int.TryParse(azahar?.Element("ResolutionFactor")?.Value ?? settings.Element("AzaharResolutionFactor")?.Value, out var arf))
        {
            AzaharResolutionFactor = arf;
        }

        if (bool.TryParse(azahar?.Element("UseVsync")?.Value ?? settings.Element("AzaharUseVsync")?.Value, out var auv))
        {
            AzaharUseVsync = auv;
        }

        if (bool.TryParse(azahar?.Element("AsyncShaderCompilation")?.Value ?? settings.Element("AzaharAsyncShaderCompilation")?.Value, out var aasc))
        {
            AzaharAsyncShaderCompilation = aasc;
        }

        if (bool.TryParse(azahar?.Element("Fullscreen")?.Value ?? settings.Element("AzaharFullscreen")?.Value, out var af))
        {
            AzaharFullscreen = af;
        }

        if (int.TryParse(azahar?.Element("Volume")?.Value ?? settings.Element("AzaharVolume")?.Value, out var avol))
        {
            AzaharVolume = avol;
        }

        if (bool.TryParse(azahar?.Element("IsNew3ds")?.Value ?? settings.Element("AzaharIsNew3ds")?.Value, out var ain))
        {
            AzaharIsNew3ds = ain;
        }

        if (int.TryParse(azahar?.Element("LayoutOption")?.Value ?? settings.Element("AzaharLayoutOption")?.Value, out var alo))
        {
            AzaharLayoutOption = alo;
        }

        if (bool.TryParse(azahar?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("AzaharShowSettingsBeforeLaunch")?.Value, out var asbl))
        {
            AzaharShowSettingsBeforeLaunch = asbl;
        }

        if (bool.TryParse(azahar?.Element("EnableAudioStretching")?.Value ?? settings.Element("AzaharEnableAudioStretching")?.Value, out var aeas))
        {
            AzaharEnableAudioStretching = aeas;
        }

        // Blastem Fallback
        var blastem = settings.Element("Blastem");
        if (bool.TryParse(blastem?.Element("Fullscreen")?.Value ?? settings.Element("BlastemFullscreen")?.Value, out var bf))
        {
            BlastemFullscreen = bf;
        }

        if (bool.TryParse(blastem?.Element("Vsync")?.Value ?? settings.Element("BlastemVsync")?.Value, out var bv))
        {
            BlastemVsync = bv;
        }

        BlastemAspect = blastem?.Element("Aspect")?.Value ?? settings.Element("BlastemAspect")?.Value ?? BlastemAspect;
        BlastemScaling = blastem?.Element("Scaling")?.Value ?? settings.Element("BlastemScaling")?.Value ?? BlastemScaling;
        if (bool.TryParse(blastem?.Element("Scanlines")?.Value ?? settings.Element("BlastemScanlines")?.Value, out var bs))
        {
            BlastemScanlines = bs;
        }

        if (int.TryParse(blastem?.Element("AudioRate")?.Value ?? settings.Element("BlastemAudioRate")?.Value, out var bar))
        {
            BlastemAudioRate = bar;
        }

        BlastemSyncSource = blastem?.Element("SyncSource")?.Value ?? settings.Element("BlastemSyncSource")?.Value ?? BlastemSyncSource;
        if (bool.TryParse(blastem?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("BlastemShowSettingsBeforeLaunch")?.Value, out var bssbl))
        {
            BlastemShowSettingsBeforeLaunch = bssbl;
        }

        // Cemu Fallback
        var cemu = settings.Element("Cemu");
        if (bool.TryParse(cemu?.Element("Fullscreen")?.Value ?? settings.Element("CemuFullscreen")?.Value, out var cf))
        {
            CemuFullscreen = cf;
        }

        if (int.TryParse(cemu?.Element("GraphicApi")?.Value ?? settings.Element("CemuGraphicApi")?.Value, out var cga))
        {
            CemuGraphicApi = cga;
        }

        if (int.TryParse(cemu?.Element("Vsync")?.Value ?? settings.Element("CemuVsync")?.Value, out var cv))
        {
            CemuVsync = cv;
        }

        if (bool.TryParse(cemu?.Element("AsyncCompile")?.Value ?? settings.Element("CemuAsyncCompile")?.Value, out var cac))
        {
            CemuAsyncCompile = cac;
        }

        if (int.TryParse(cemu?.Element("TvVolume")?.Value ?? settings.Element("CemuTvVolume")?.Value, out var ctv))
        {
            CemuTvVolume = ctv;
        }

        if (int.TryParse(cemu?.Element("ConsoleLanguage")?.Value ?? settings.Element("CemuConsoleLanguage")?.Value, out var ccl))
        {
            CemuConsoleLanguage = ccl;
        }

        if (bool.TryParse(cemu?.Element("DiscordPresence")?.Value ?? settings.Element("CemuDiscordPresence")?.Value, out var cdp))
        {
            CemuDiscordPresence = cdp;
        }

        if (bool.TryParse(cemu?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("CemuShowSettingsBeforeLaunch")?.Value, out var cssbl))
        {
            CemuShowSettingsBeforeLaunch = cssbl;
        }

        // Daphne Fallback
        var daphne = settings.Element("Daphne");
        if (bool.TryParse(daphne?.Element("Fullscreen")?.Value ?? settings.Element("DaphneFullscreen")?.Value, out var df))
        {
            DaphneFullscreen = df;
        }

        if (int.TryParse(daphne?.Element("ResX")?.Value ?? settings.Element("DaphneResX")?.Value, out var drx))
        {
            DaphneResX = drx;
        }

        if (int.TryParse(daphne?.Element("ResY")?.Value ?? settings.Element("DaphneResY")?.Value, out var dry))
        {
            DaphneResY = dry;
        }

        if (bool.TryParse(daphne?.Element("DisableCrosshairs")?.Value ?? settings.Element("DaphneDisableCrosshairs")?.Value, out var ddc))
        {
            DaphneDisableCrosshairs = ddc;
        }

        if (bool.TryParse(daphne?.Element("Bilinear")?.Value ?? settings.Element("DaphneBilinear")?.Value, out var db))
        {
            DaphneBilinear = db;
        }

        if (bool.TryParse(daphne?.Element("EnableSound")?.Value ?? settings.Element("DaphneEnableSound")?.Value, out var des))
        {
            DaphneEnableSound = des;
        }

        if (bool.TryParse(daphne?.Element("UseOverlays")?.Value ?? settings.Element("DaphneUseOverlays")?.Value, out var duo))
        {
            DaphneUseOverlays = duo;
        }

        if (bool.TryParse(daphne?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("DaphneShowSettingsBeforeLaunch")?.Value, out var dssbl))
        {
            DaphneShowSettingsBeforeLaunch = dssbl;
        }

        // Dolphin Fallback
        var dolphin = settings.Element("Dolphin");
        DolphinGfxBackend = dolphin?.Element("GfxBackend")?.Value ?? settings.Element("DolphinGfxBackend")?.Value ?? DolphinGfxBackend;
        if (bool.TryParse(dolphin?.Element("DspThread")?.Value ?? settings.Element("DolphinDspThread")?.Value, out var ddt))
        {
            DolphinDspThread = ddt;
        }

        if (bool.TryParse(dolphin?.Element("WiimoteContinuousScanning")?.Value ?? settings.Element("DolphinWiimoteContinuousScanning")?.Value, out var dwcs))
        {
            DolphinWiimoteContinuousScanning = dwcs;
        }

        if (bool.TryParse(dolphin?.Element("WiimoteEnableSpeaker")?.Value ?? settings.Element("DolphinWiimoteEnableSpeaker")?.Value, out var dwes))
        {
            DolphinWiimoteEnableSpeaker = dwes;
        }

        if (bool.TryParse(dolphin?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("DolphinShowSettingsBeforeLaunch")?.Value, out var dssbl2))
        {
            DolphinShowSettingsBeforeLaunch = dssbl2;
        }

        // DuckStation Fallback
        var duckstation = settings.Element("DuckStation");
        if (bool.TryParse(duckstation?.Element("StartFullscreen")?.Value ?? settings.Element("DuckStationStartFullscreen")?.Value, out var dssf))
        {
            DuckStationStartFullscreen = dssf;
        }

        if (bool.TryParse(duckstation?.Element("PauseOnFocusLoss")?.Value ?? settings.Element("DuckStationPauseOnFocusLoss")?.Value, out var dpofl))
        {
            DuckStationPauseOnFocusLoss = dpofl;
        }

        if (bool.TryParse(duckstation?.Element("SaveStateOnExit")?.Value ?? settings.Element("DuckStationSaveStateOnExit")?.Value, out var dssoe))
        {
            DuckStationSaveStateOnExit = dssoe;
        }

        if (bool.TryParse(duckstation?.Element("RewindEnable")?.Value ?? settings.Element("DuckStationRewindEnable")?.Value, out var dre))
        {
            DuckStationRewindEnable = dre;
        }

        if (int.TryParse(duckstation?.Element("RunaheadFrameCount")?.Value ?? settings.Element("DuckStationRunaheadFrameCount")?.Value, out var drfc))
        {
            DuckStationRunaheadFrameCount = drfc;
        }

        DuckStationRenderer = duckstation?.Element("Renderer")?.Value ?? settings.Element("DuckStationRenderer")?.Value ?? DuckStationRenderer;
        if (int.TryParse(duckstation?.Element("ResolutionScale")?.Value ?? settings.Element("DuckStationResolutionScale")?.Value, out var drs))
        {
            DuckStationResolutionScale = drs;
        }

        DuckStationTextureFilter = duckstation?.Element("TextureFilter")?.Value ?? settings.Element("DuckStationTextureFilter")?.Value ?? DuckStationTextureFilter;
        if (bool.TryParse(duckstation?.Element("WidescreenHack")?.Value ?? settings.Element("DuckStationWidescreenHack")?.Value, out var dwh))
        {
            DuckStationWidescreenHack = dwh;
        }

        if (bool.TryParse(duckstation?.Element("PgxpEnable")?.Value ?? settings.Element("DuckStationPgxpEnable")?.Value, out var dpe))
        {
            DuckStationPgxpEnable = dpe;
        }

        DuckStationAspectRatio = duckstation?.Element("AspectRatio")?.Value ?? settings.Element("DuckStationAspectRatio")?.Value ?? DuckStationAspectRatio;
        if (bool.TryParse(duckstation?.Element("Vsync")?.Value ?? settings.Element("DuckStationVsync")?.Value, out var dsv))
        {
            DuckStationVsync = dsv;
        }

        if (int.TryParse(duckstation?.Element("OutputVolume")?.Value ?? settings.Element("DuckStationOutputVolume")?.Value, out var dov))
        {
            DuckStationOutputVolume = dov;
        }

        if (bool.TryParse(duckstation?.Element("OutputMuted")?.Value ?? settings.Element("DuckStationOutputMuted")?.Value, out var dom))
        {
            DuckStationOutputMuted = dom;
        }

        if (bool.TryParse(duckstation?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("DuckStationShowSettingsBeforeLaunch")?.Value, out var dssbl3))
        {
            DuckStationShowSettingsBeforeLaunch = dssbl3;
        }

        // Flycast Fallback
        var flycast = settings.Element("Flycast");
        if (bool.TryParse(flycast?.Element("Fullscreen")?.Value ?? settings.Element("FlycastFullscreen")?.Value, out var ff))
        {
            FlycastFullscreen = ff;
        }

        if (int.TryParse(flycast?.Element("Width")?.Value ?? settings.Element("FlycastWidth")?.Value, out var fw))
        {
            FlycastWidth = fw;
        }

        if (int.TryParse(flycast?.Element("Height")?.Value ?? settings.Element("FlycastHeight")?.Value, out var fh))
        {
            FlycastHeight = fh;
        }

        if (bool.TryParse(flycast?.Element("Maximized")?.Value ?? settings.Element("FlycastMaximized")?.Value, out var flycastMaximized))
        {
            FlycastMaximized = flycastMaximized;
        }

        if (bool.TryParse(flycast?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("FlycastShowSettingsBeforeLaunch")?.Value, out var fssbl))
        {
            FlycastShowSettingsBeforeLaunch = fssbl;
        }

        // MAME Fallback
        var mame = settings.Element("Mame");
        MameVideo = mame?.Element("Video")?.Value ?? settings.Element("MameVideo")?.Value ?? MameVideo;
        if (bool.TryParse(mame?.Element("Window")?.Value ?? settings.Element("MameWindow")?.Value, out var mw))
        {
            MameWindow = mw;
        }

        if (bool.TryParse(mame?.Element("Maximize")?.Value ?? settings.Element("MameMaximize")?.Value, out var mm))
        {
            MameMaximize = mm;
        }

        if (bool.TryParse(mame?.Element("KeepAspect")?.Value ?? settings.Element("MameKeepAspect")?.Value, out var mka))
        {
            MameKeepAspect = mka;
        }

        if (bool.TryParse(mame?.Element("SkipGameInfo")?.Value ?? settings.Element("MameSkipGameInfo")?.Value, out var msgi))
        {
            MameSkipGameInfo = msgi;
        }

        if (bool.TryParse(mame?.Element("Autosave")?.Value ?? settings.Element("MameAutosave")?.Value, out var mas))
        {
            MameAutosave = mas;
        }

        if (bool.TryParse(mame?.Element("ConfirmQuit")?.Value ?? settings.Element("MameConfirmQuit")?.Value, out var mcq))
        {
            MameConfirmQuit = mcq;
        }

        if (bool.TryParse(mame?.Element("Joystick")?.Value ?? settings.Element("MameJoystick")?.Value, out var mj))
        {
            MameJoystick = mj;
        }

        if (bool.TryParse(mame?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("MameShowSettingsBeforeLaunch")?.Value, out var mssbl))
        {
            MameShowSettingsBeforeLaunch = mssbl;
        }

        if (bool.TryParse(mame?.Element("Autoframeskip")?.Value ?? settings.Element("MameAutoframeskip")?.Value, out var maf))
        {
            MameAutoframeskip = maf;
        }

        MameBgfxBackend = mame?.Element("BgfxBackend")?.Value ?? settings.Element("MameBgfxBackend")?.Value ?? MameBgfxBackend;
        MameBgfxScreenChains = mame?.Element("BgfxScreenChains")?.Value ?? settings.Element("MameBgfxScreenChains")?.Value ?? MameBgfxScreenChains;
        if (bool.TryParse(mame?.Element("Filter")?.Value ?? settings.Element("MameFilter")?.Value, out var mf))
        {
            MameFilter = mf;
        }

        if (bool.TryParse(mame?.Element("Cheat")?.Value ?? settings.Element("MameCheat")?.Value, out var mc))
        {
            MameCheat = mc;
        }

        if (bool.TryParse(mame?.Element("Rewind")?.Value ?? settings.Element("MameRewind")?.Value, out var mr))
        {
            MameRewind = mr;
        }

        if (bool.TryParse(mame?.Element("NvramSave")?.Value ?? settings.Element("MameNvramSave")?.Value, out var mns))
        {
            MameNvramSave = mns;
        }

        // Mednafen Fallback
        var mednafen = settings.Element("Mednafen");
        MednafenVideoDriver = mednafen?.Element("VideoDriver")?.Value ?? settings.Element("MednafenVideoDriver")?.Value ?? MednafenVideoDriver;
        if (bool.TryParse(mednafen?.Element("Fullscreen")?.Value ?? settings.Element("MednafenFullscreen")?.Value, out var mef))
        {
            MednafenFullscreen = mef;
        }

        if (bool.TryParse(mednafen?.Element("Vsync")?.Value ?? settings.Element("MednafenVsync")?.Value, out var mev))
        {
            MednafenVsync = mev;
        }

        MednafenStretch = mednafen?.Element("Stretch")?.Value ?? settings.Element("MednafenStretch")?.Value ?? MednafenStretch;
        if (bool.TryParse(mednafen?.Element("Bilinear")?.Value ?? settings.Element("MednafenBilinear")?.Value, out var meb))
        {
            MednafenBilinear = meb;
        }

        if (int.TryParse(mednafen?.Element("Scanlines")?.Value ?? settings.Element("MednafenScanlines")?.Value, out var mes))
        {
            MednafenScanlines = mes;
        }

        MednafenShader = mednafen?.Element("Shader")?.Value ?? settings.Element("MednafenShader")?.Value ?? MednafenShader;
        MednafenSpecial = mednafen?.Element("Special")?.Value ?? settings.Element("MednafenSpecial")?.Value ?? MednafenSpecial;
        if (int.TryParse(mednafen?.Element("Volume")?.Value ?? settings.Element("MednafenVolume")?.Value, out var mevo))
        {
            MednafenVolume = mevo;
        }

        if (bool.TryParse(mednafen?.Element("Cheats")?.Value ?? settings.Element("MednafenCheats")?.Value, out var mec))
        {
            MednafenCheats = mec;
        }

        if (bool.TryParse(mednafen?.Element("Rewind")?.Value ?? settings.Element("MednafenRewind")?.Value, out var mer))
        {
            MednafenRewind = mer;
        }

        if (bool.TryParse(mednafen?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("MednafenShowSettingsBeforeLaunch")?.Value, out var messbl))
        {
            MednafenShowSettingsBeforeLaunch = messbl;
        }

        // Mesen Fallback
        var mesen = settings.Element("Mesen");
        if (bool.TryParse(mesen?.Element("Fullscreen")?.Value ?? settings.Element("MesenFullscreen")?.Value, out var msnf))
        {
            MesenFullscreen = msnf;
        }

        if (bool.TryParse(mesen?.Element("Vsync")?.Value ?? settings.Element("MesenVsync")?.Value, out var msnv))
        {
            MesenVsync = msnv;
        }

        MesenAspectRatio = mesen?.Element("AspectRatio")?.Value ?? settings.Element("MesenAspectRatio")?.Value ?? MesenAspectRatio;
        if (bool.TryParse(mesen?.Element("Bilinear")?.Value ?? settings.Element("MesenBilinear")?.Value, out var msnb))
        {
            MesenBilinear = msnb;
        }

        MesenVideoFilter = mesen?.Element("VideoFilter")?.Value ?? settings.Element("MesenVideoFilter")?.Value ?? MesenVideoFilter;
        if (bool.TryParse(mesen?.Element("EnableAudio")?.Value ?? settings.Element("MesenEnableAudio")?.Value, out var msnbea))
        {
            MesenEnableAudio = msnbea;
        }

        if (int.TryParse(mesen?.Element("MasterVolume")?.Value ?? settings.Element("MesenMasterVolume")?.Value, out var msnmv))
        {
            MesenMasterVolume = msnmv;
        }

        if (bool.TryParse(mesen?.Element("Rewind")?.Value ?? settings.Element("MesenRewind")?.Value, out var msnr))
        {
            MesenRewind = msnr;
        }

        if (int.TryParse(mesen?.Element("RunAhead")?.Value ?? settings.Element("MesenRunAhead")?.Value, out var msnra))
        {
            MesenRunAhead = msnra;
        }

        if (bool.TryParse(mesen?.Element("PauseInBackground")?.Value ?? settings.Element("MesenPauseInBackground")?.Value, out var msnpib))
        {
            MesenPauseInBackground = msnpib;
        }

        if (bool.TryParse(mesen?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("MesenShowSettingsBeforeLaunch")?.Value, out var msnssbl))
        {
            MesenShowSettingsBeforeLaunch = msnssbl;
        }

        // PCSX2 Fallback
        var pcsx2 = settings.Element("Pcsx2");
        if (bool.TryParse(pcsx2?.Element("StartFullscreen")?.Value ?? settings.Element("Pcsx2StartFullscreen")?.Value, out var psf))
        {
            Pcsx2StartFullscreen = psf;
        }

        Pcsx2AspectRatio = pcsx2?.Element("AspectRatio")?.Value ?? settings.Element("Pcsx2AspectRatio")?.Value ?? Pcsx2AspectRatio;
        if (int.TryParse(pcsx2?.Element("Renderer")?.Value ?? settings.Element("Pcsx2Renderer")?.Value, out var pr))
        {
            Pcsx2Renderer = pr;
        }

        if (int.TryParse(pcsx2?.Element("UpscaleMultiplier")?.Value ?? settings.Element("Pcsx2UpscaleMultiplier")?.Value, out var pum))
        {
            Pcsx2UpscaleMultiplier = pum;
        }

        if (bool.TryParse(pcsx2?.Element("Vsync")?.Value ?? settings.Element("Pcsx2Vsync")?.Value, out var pv))
        {
            Pcsx2Vsync = pv;
        }

        if (bool.TryParse(pcsx2?.Element("EnableCheats")?.Value ?? settings.Element("Pcsx2EnableCheats")?.Value, out var pec))
        {
            Pcsx2EnableCheats = pec;
        }

        if (bool.TryParse(pcsx2?.Element("EnableWidescreenPatches")?.Value ?? settings.Element("Pcsx2EnableWidescreenPatches")?.Value, out var pewp))
        {
            Pcsx2EnableWidescreenPatches = pewp;
        }

        if (int.TryParse(pcsx2?.Element("Volume")?.Value ?? settings.Element("Pcsx2Volume")?.Value, out var pvol))
        {
            Pcsx2Volume = pvol;
        }

        if (bool.TryParse(pcsx2?.Element("AchievementsEnabled")?.Value ?? settings.Element("Pcsx2AchievementsEnabled")?.Value, out var pae))
        {
            Pcsx2AchievementsEnabled = pae;
        }

        if (bool.TryParse(pcsx2?.Element("AchievementsHardcore")?.Value ?? settings.Element("Pcsx2AchievementsHardcore")?.Value, out var pah))
        {
            Pcsx2AchievementsHardcore = pah;
        }

        if (bool.TryParse(pcsx2?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("Pcsx2ShowSettingsBeforeLaunch")?.Value, out var pssbl))
        {
            Pcsx2ShowSettingsBeforeLaunch = pssbl;
        }

        // Raine
        var raine = settings.Element("Raine");
        if (bool.TryParse(raine?.Element("Fullscreen")?.Value ?? settings.Element("RaineFullscreen")?.Value, out var rf))
        {
            RaineFullscreen = rf;
        }

        if (int.TryParse(raine?.Element("ResX")?.Value ?? settings.Element("RaineResX")?.Value, out var rrx))
        {
            RaineResX = rrx;
        }

        if (int.TryParse(raine?.Element("ResY")?.Value ?? settings.Element("RaineResY")?.Value, out var rry))
        {
            RaineResY = rry;
        }

        if (bool.TryParse(raine?.Element("FixAspectRatio")?.Value ?? settings.Element("RaineFixAspectRatio")?.Value, out var rfar))
        {
            RaineFixAspectRatio = rfar;
        }

        if (bool.TryParse(raine?.Element("Vsync")?.Value ?? settings.Element("RaineVsync")?.Value, out var raineVsync))
        {
            RaineVsync = raineVsync;
        }

        RaineSoundDriver = raine?.Element("SoundDriver")?.Value ?? settings.Element("RaineSoundDriver")?.Value ?? "directsound";
        if (int.TryParse(raine?.Element("SampleRate")?.Value ?? settings.Element("RaineSampleRate")?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rsr))
        {
            RaineSampleRate = rsr;
        }

        if (bool.TryParse(raine?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("RaineShowSettingsBeforeLaunch")?.Value, out var raineShowSettingsBeforeLaunch))
        {
            RaineShowSettingsBeforeLaunch = raineShowSettingsBeforeLaunch;
        }

        if (bool.TryParse(raine?.Element("ShowFps")?.Value ?? settings.Element("RaineShowFps")?.Value, out var rfp))
        {
            RaineShowFps = rfp;
        }

        if (int.TryParse(raine?.Element("FrameSkip")?.Value ?? settings.Element("RaineFrameSkip")?.Value, out var rfs))
        {
            RaineFrameSkip = rfs;
        }

        RaineNeoCdBios = raine?.Element("NeoCdBios")?.Value ?? settings.Element("RaineNeoCdBios")?.Value ?? string.Empty;
        if (int.TryParse(raine?.Element("MusicVolume")?.Value ?? settings.Element("RaineMusicVolume")?.Value, out var rmu))
        {
            RaineMusicVolume = rmu;
        }

        if (int.TryParse(raine?.Element("SfxVolume")?.Value ?? settings.Element("RaineSfxVolume")?.Value, out var rsfx))
        {
            RaineSfxVolume = rsfx;
        }

        if (bool.TryParse(raine?.Element("MuteSfx")?.Value ?? settings.Element("RaineMuteSfx")?.Value, out var rms))
        {
            RaineMuteSfx = rms;
        }

        if (bool.TryParse(raine?.Element("MuteMusic")?.Value ?? settings.Element("RaineMuteMusic")?.Value, out var rmm))
        {
            RaineMuteMusic = rmm;
        }

        RaineRomDirectory = raine?.Element("RomDirectory")?.Value ?? settings.Element("RaineRomDirectory")?.Value ?? RaineRomDirectory;

        // Redream Fallback
        var redream = settings.Element("Redream");
        RedreamCable = redream?.Element("Cable")?.Value ?? settings.Element("RedreamCable")?.Value ?? RedreamCable;
        RedreamBroadcast = redream?.Element("Broadcast")?.Value ?? settings.Element("RedreamBroadcast")?.Value ?? RedreamBroadcast;
        RedreamLanguage = redream?.Element("Language")?.Value ?? settings.Element("RedreamLanguage")?.Value ?? RedreamLanguage;
        RedreamRegion = redream?.Element("Region")?.Value ?? settings.Element("RedreamRegion")?.Value ?? RedreamRegion;
        if (bool.TryParse(redream?.Element("Vsync")?.Value ?? settings.Element("RedreamVsync")?.Value, out var rvs))
        {
            RedreamVsync = rvs;
        }

        if (bool.TryParse(redream?.Element("Frameskip")?.Value ?? settings.Element("RedreamFrameskip")?.Value, out var redreamFrameskip))
        {
            RedreamFrameskip = redreamFrameskip;
        }

        RedreamAspect = redream?.Element("Aspect")?.Value ?? settings.Element("RedreamAspect")?.Value ?? RedreamAspect;
        if (int.TryParse(redream?.Element("Res")?.Value ?? settings.Element("RedreamRes")?.Value, out var rr))
        {
            RedreamRes = rr;
        }

        RedreamRenderer = redream?.Element("Renderer")?.Value ?? settings.Element("RedreamRenderer")?.Value ?? RedreamRenderer;
        RedreamFullmode = redream?.Element("Fullmode")?.Value ?? settings.Element("RedreamFullmode")?.Value ?? RedreamFullmode;
        if (int.TryParse(redream?.Element("Volume")?.Value ?? settings.Element("RedreamVolume")?.Value, out var rVol))
        {
            RedreamVolume = rVol;
        }

        if (int.TryParse(redream?.Element("Latency")?.Value ?? settings.Element("RedreamLatency")?.Value, out var rLat))
        {
            RedreamLatency = rLat;
        }

        if (bool.TryParse(redream?.Element("Framerate")?.Value ?? settings.Element("RedreamFramerate")?.Value, out var rFr))
        {
            RedreamFramerate = rFr;
        }

        if (int.TryParse(redream?.Element("Width")?.Value ?? settings.Element("RedreamWidth")?.Value, out var rW))
        {
            RedreamWidth = rW;
        }

        if (int.TryParse(redream?.Element("Height")?.Value ?? settings.Element("RedreamHeight")?.Value, out var rH))
        {
            RedreamHeight = rH;
        }

        if (bool.TryParse(redream?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("RedreamShowSettingsBeforeLaunch")?.Value, out var redreamShowSettingsBeforeLaunch))
        {
            RedreamShowSettingsBeforeLaunch = redreamShowSettingsBeforeLaunch;
        }

        // RetroArch Fallback
        var retroarch = settings.Element("RetroArch");
        if (bool.TryParse(retroarch?.Element("CheevosEnable")?.Value ?? settings.Element("RetroArchCheevosEnable")?.Value, out var race))
        {
            RetroArchCheevosEnable = race;
        }

        if (bool.TryParse(retroarch?.Element("CheevosHardcore")?.Value ?? settings.Element("RetroArchCheevosHardcore")?.Value, out var rach))
        {
            RetroArchCheevosHardcore = rach;
        }

        if (bool.TryParse(retroarch?.Element("Fullscreen")?.Value ?? settings.Element("RetroArchFullscreen")?.Value, out var raf))
        {
            RetroArchFullscreen = raf;
        }

        if (bool.TryParse(retroarch?.Element("Vsync")?.Value ?? settings.Element("RetroArchVsync")?.Value, out var rav))
        {
            RetroArchVsync = rav;
        }

        RetroArchVideoDriver = retroarch?.Element("VideoDriver")?.Value ?? settings.Element("RetroArchVideoDriver")?.Value ?? RetroArchVideoDriver;
        if (bool.TryParse(retroarch?.Element("AudioEnable")?.Value ?? settings.Element("RetroArchAudioEnable")?.Value, out var raae))
        {
            RetroArchAudioEnable = raae;
        }

        if (bool.TryParse(retroarch?.Element("AudioMute")?.Value ?? settings.Element("RetroArchAudioMute")?.Value, out var raam))
        {
            RetroArchAudioMute = raam;
        }

        RetroArchMenuDriver = retroarch?.Element("MenuDriver")?.Value ?? settings.Element("RetroArchMenuDriver")?.Value ?? RetroArchMenuDriver;
        if (bool.TryParse(retroarch?.Element("PauseNonActive")?.Value ?? settings.Element("RetroArchPauseNonActive")?.Value, out var rapna))
        {
            RetroArchPauseNonActive = rapna;
        }

        if (bool.TryParse(retroarch?.Element("SaveOnExit")?.Value ?? settings.Element("RetroArchSaveOnExit")?.Value, out var rasoe))
        {
            RetroArchSaveOnExit = rasoe;
        }

        if (bool.TryParse(retroarch?.Element("AutoSaveState")?.Value ?? settings.Element("RetroArchAutoSaveState")?.Value, out var raass))
        {
            RetroArchAutoSaveState = raass;
        }

        if (bool.TryParse(retroarch?.Element("AutoLoadState")?.Value ?? settings.Element("RetroArchAutoLoadState")?.Value, out var raals))
        {
            RetroArchAutoLoadState = raals;
        }

        if (bool.TryParse(retroarch?.Element("Rewind")?.Value ?? settings.Element("RetroArchRewind")?.Value, out var rar))
        {
            RetroArchRewind = rar;
        }

        if (bool.TryParse(retroarch?.Element("ThreadedVideo")?.Value ?? settings.Element("RetroArchThreadedVideo")?.Value, out var ratv))
        {
            RetroArchThreadedVideo = ratv;
        }

        if (bool.TryParse(retroarch?.Element("Bilinear")?.Value ?? settings.Element("RetroArchBilinear")?.Value, out var rab))
        {
            RetroArchBilinear = rab;
        }

        if (bool.TryParse(retroarch?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("RetroArchShowSettingsBeforeLaunch")?.Value, out var rassbl))
        {
            RetroArchShowSettingsBeforeLaunch = rassbl;
        }

        RetroArchAspectRatioIndex = retroarch?.Element("AspectRatioIndex")?.Value ?? settings.Element("RetroArchAspectRatioIndex")?.Value ?? RetroArchAspectRatioIndex;
        if (bool.TryParse(retroarch?.Element("ScaleInteger")?.Value ?? settings.Element("RetroArchScaleInteger")?.Value, out var rasi))
        {
            RetroArchScaleInteger = rasi;
        }

        if (bool.TryParse(retroarch?.Element("ShaderEnable")?.Value ?? settings.Element("RetroArchShaderEnable")?.Value, out var rase))
        {
            RetroArchShaderEnable = rase;
        }

        if (bool.TryParse(retroarch?.Element("HardSync")?.Value ?? settings.Element("RetroArchHardSync")?.Value, out var rahs))
        {
            RetroArchHardSync = rahs;
        }

        if (bool.TryParse(retroarch?.Element("RunAhead")?.Value ?? settings.Element("RetroArchRunAhead")?.Value, out var rara))
        {
            RetroArchRunAhead = rara;
        }

        if (bool.TryParse(retroarch?.Element("ShowAdvancedSettings")?.Value ?? settings.Element("RetroArchShowAdvancedSettings")?.Value, out var rasas))
        {
            RetroArchShowAdvancedSettings = rasas;
        }

        if (bool.TryParse(retroarch?.Element("DiscordAllow")?.Value ?? settings.Element("RetroArchDiscordAllow")?.Value, out var rada))
        {
            RetroArchDiscordAllow = rada;
        }

        if (bool.TryParse(retroarch?.Element("OverrideSystemDir")?.Value ?? settings.Element("RetroArchOverrideSystemDir")?.Value, out var raosd))
        {
            RetroArchOverrideSystemDir = raosd;
        }

        if (bool.TryParse(retroarch?.Element("OverrideSaveDir")?.Value ?? settings.Element("RetroArchOverrideSaveDir")?.Value, out var raovsd))
        {
            RetroArchOverrideSaveDir = raovsd;
        }

        if (bool.TryParse(retroarch?.Element("OverrideStateDir")?.Value ?? settings.Element("RetroArchOverrideStateDir")?.Value, out var raovstd))
        {
            RetroArchOverrideStateDir = raovstd;
        }

        if (bool.TryParse(retroarch?.Element("OverrideScreenshotDir")?.Value ?? settings.Element("RetroArchOverrideScreenshotDir")?.Value, out var raovscd))
        {
            RetroArchOverrideScreenshotDir = raovscd;
        }

        // RPCS3 Fallback
        var rpcs3 = settings.Element("Rpcs3");
        Rpcs3Renderer = rpcs3?.Element("Renderer")?.Value ?? settings.Element("Rpcs3Renderer")?.Value ?? Rpcs3Renderer;
        Rpcs3Resolution = rpcs3?.Element("Resolution")?.Value ?? settings.Element("Rpcs3Resolution")?.Value ?? Rpcs3Resolution;
        Rpcs3AspectRatio = rpcs3?.Element("AspectRatio")?.Value ?? settings.Element("Rpcs3AspectRatio")?.Value ?? Rpcs3AspectRatio;
        if (bool.TryParse(rpcs3?.Element("Vsync")?.Value ?? settings.Element("Rpcs3Vsync")?.Value, out var rv))
        {
            Rpcs3Vsync = rv;
        }

        if (int.TryParse(rpcs3?.Element("ResolutionScale")?.Value ?? settings.Element("Rpcs3ResolutionScale")?.Value, out var rrs))
        {
            Rpcs3ResolutionScale = rrs;
        }

        if (int.TryParse(rpcs3?.Element("AnisotropicFilter")?.Value ?? settings.Element("Rpcs3AnisotropicFilter")?.Value, out var raf2))
        {
            Rpcs3AnisotropicFilter = raf2;
        }

        Rpcs3PpuDecoder = rpcs3?.Element("PpuDecoder")?.Value ?? settings.Element("Rpcs3PpuDecoder")?.Value ?? Rpcs3PpuDecoder;
        Rpcs3SpuDecoder = rpcs3?.Element("SpuDecoder")?.Value ?? settings.Element("Rpcs3SpuDecoder")?.Value ?? Rpcs3SpuDecoder;
        Rpcs3AudioRenderer = rpcs3?.Element("AudioRenderer")?.Value ?? settings.Element("Rpcs3AudioRenderer")?.Value ?? Rpcs3AudioRenderer;
        if (bool.TryParse(rpcs3?.Element("AudioBuffering")?.Value ?? settings.Element("Rpcs3AudioBuffering")?.Value, out var rabuf))
        {
            Rpcs3AudioBuffering = rabuf;
        }

        if (bool.TryParse(rpcs3?.Element("StartFullscreen")?.Value ?? settings.Element("Rpcs3StartFullscreen")?.Value, out var rsf))
        {
            Rpcs3StartFullscreen = rsf;
        }

        if (bool.TryParse(rpcs3?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("Rpcs3ShowSettingsBeforeLaunch")?.Value, out var rssbl))
        {
            Rpcs3ShowSettingsBeforeLaunch = rssbl;
        }

        // Sega Model 2 Fallback
        var sm2 = settings.Element("SegaModel2");
        if (int.TryParse(sm2?.Element("ResX")?.Value ?? settings.Element("SegaModel2ResX")?.Value, out var sm2Rx))
        {
            SegaModel2ResX = sm2Rx;
        }

        if (int.TryParse(sm2?.Element("ResY")?.Value ?? settings.Element("SegaModel2ResY")?.Value, out var sm2Ry))
        {
            SegaModel2ResY = sm2Ry;
        }

        if (int.TryParse(sm2?.Element("WideScreen")?.Value ?? settings.Element("SegaModel2WideScreen")?.Value, out var sm2Ws))
        {
            SegaModel2WideScreen = sm2Ws;
        }

        if (bool.TryParse(sm2?.Element("Bilinear")?.Value ?? settings.Element("SegaModel2Bilinear")?.Value, out var sm2B))
        {
            SegaModel2Bilinear = sm2B;
        }

        if (bool.TryParse(sm2?.Element("Trilinear")?.Value ?? settings.Element("SegaModel2Trilinear")?.Value, out var sm2T))
        {
            SegaModel2Trilinear = sm2T;
        }

        if (bool.TryParse(sm2?.Element("FilterTilemaps")?.Value ?? settings.Element("SegaModel2FilterTilemaps")?.Value, out var sm2Ft))
        {
            SegaModel2FilterTilemaps = sm2Ft;
        }

        if (bool.TryParse(sm2?.Element("DrawCross")?.Value ?? settings.Element("SegaModel2DrawCross")?.Value, out var sm2dc))
        {
            SegaModel2DrawCross = sm2dc;
        }

        if (int.TryParse(sm2?.Element("Fsaa")?.Value ?? settings.Element("SegaModel2Fsaa")?.Value, out var sm2Fsaa))
        {
            SegaModel2Fsaa = sm2Fsaa;
        }

        if (bool.TryParse(sm2?.Element("XInput")?.Value ?? settings.Element("SegaModel2XInput")?.Value, out var sm2Xi))
        {
            SegaModel2XInput = sm2Xi;
        }

        if (bool.TryParse(sm2?.Element("EnableFf")?.Value ?? settings.Element("SegaModel2EnableFf")?.Value, out var sm2Eff))
        {
            SegaModel2EnableFf = sm2Eff;
        }

        if (bool.TryParse(sm2?.Element("HoldGears")?.Value ?? settings.Element("SegaModel2HoldGears")?.Value, out var sm2Hg))
        {
            SegaModel2HoldGears = sm2Hg;
        }

        if (bool.TryParse(sm2?.Element("UseRawInput")?.Value ?? settings.Element("SegaModel2UseRawInput")?.Value, out var sm2Uri))
        {
            SegaModel2UseRawInput = sm2Uri;
        }

        if (bool.TryParse(sm2?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("SegaModel2ShowSettingsBeforeLaunch")?.Value, out var sm2Ssbl))
        {
            SegaModel2ShowSettingsBeforeLaunch = sm2Ssbl;
        }

        // Stella Fallback
        var stella = settings.Element("Stella");
        if (bool.TryParse(stella?.Element("Fullscreen")?.Value ?? settings.Element("StellaFullscreen")?.Value, out var stf))
        {
            StellaFullscreen = stf;
        }

        if (bool.TryParse(stella?.Element("Vsync")?.Value ?? settings.Element("StellaVsync")?.Value, out var stv))
        {
            StellaVsync = stv;
        }

        StellaVideoDriver = stella?.Element("VideoDriver")?.Value ?? settings.Element("StellaVideoDriver")?.Value ?? StellaVideoDriver;
        if (bool.TryParse(stella?.Element("CorrectAspect")?.Value ?? settings.Element("StellaCorrectAspect")?.Value, out var stca))
        {
            StellaCorrectAspect = stca;
        }

        if (int.TryParse(stella?.Element("TvFilter")?.Value ?? settings.Element("StellaTvFilter")?.Value, out var sttf))
        {
            StellaTvFilter = sttf;
        }

        if (int.TryParse(stella?.Element("Scanlines")?.Value ?? settings.Element("StellaScanlines")?.Value, out var sts))
        {
            StellaScanlines = sts;
        }

        if (bool.TryParse(stella?.Element("AudioEnabled")?.Value ?? settings.Element("StellaAudioEnabled")?.Value, out var stae))
        {
            StellaAudioEnabled = stae;
        }

        if (int.TryParse(stella?.Element("AudioVolume")?.Value ?? settings.Element("StellaAudioVolume")?.Value, out var stav))
        {
            StellaAudioVolume = stav;
        }

        if (bool.TryParse(stella?.Element("TimeMachine")?.Value ?? settings.Element("StellaTimeMachine")?.Value, out var stm))
        {
            StellaTimeMachine = stm;
        }

        if (bool.TryParse(stella?.Element("ConfirmExit")?.Value ?? settings.Element("StellaConfirmExit")?.Value, out var stce))
        {
            StellaConfirmExit = stce;
        }

        if (bool.TryParse(stella?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("StellaShowSettingsBeforeLaunch")?.Value, out var stssbl))
        {
            StellaShowSettingsBeforeLaunch = stssbl;
        }

        // Supermodel Fallback
        var supermodel = settings.Element("Supermodel");
        if (bool.TryParse(supermodel?.Element("New3DEngine")?.Value ?? settings.Element("SupermodelNew3DEngine")?.Value, out var smn3E))
        {
            SupermodelNew3DEngine = smn3E;
        }

        if (bool.TryParse(supermodel?.Element("QuadRendering")?.Value ?? settings.Element("SupermodelQuadRendering")?.Value, out var smqr))
        {
            SupermodelQuadRendering = smqr;
        }

        if (bool.TryParse(supermodel?.Element("Fullscreen")?.Value ?? settings.Element("SupermodelFullscreen")?.Value, out var smfs))
        {
            SupermodelFullscreen = smfs;
        }

        if (int.TryParse(supermodel?.Element("ResX")?.Value ?? settings.Element("SupermodelResX")?.Value, out var smrx))
        {
            SupermodelResX = smrx;
        }

        if (int.TryParse(supermodel?.Element("ResY")?.Value ?? settings.Element("SupermodelResY")?.Value, out var smry))
        {
            SupermodelResY = smry;
        }

        if (bool.TryParse(supermodel?.Element("WideScreen")?.Value ?? settings.Element("SupermodelWideScreen")?.Value, out var smws))
        {
            SupermodelWideScreen = smws;
        }

        if (bool.TryParse(supermodel?.Element("Stretch")?.Value ?? settings.Element("SupermodelStretch")?.Value, out var smst))
        {
            SupermodelStretch = smst;
        }

        if (bool.TryParse(supermodel?.Element("Vsync")?.Value ?? settings.Element("SupermodelVsync")?.Value, out var smvs))
        {
            SupermodelVsync = smvs;
        }

        if (bool.TryParse(supermodel?.Element("Throttle")?.Value ?? settings.Element("SupermodelThrottle")?.Value, out var smth))
        {
            SupermodelThrottle = smth;
        }

        if (int.TryParse(supermodel?.Element("MusicVolume")?.Value ?? settings.Element("SupermodelMusicVolume")?.Value, out var smmv))
        {
            SupermodelMusicVolume = smmv;
        }

        if (int.TryParse(supermodel?.Element("SoundVolume")?.Value ?? settings.Element("SupermodelSoundVolume")?.Value, out var ssv))
        {
            SupermodelSoundVolume = ssv;
        }

        SupermodelInputSystem = ValidateSupermodelInputSystem(supermodel?.Element("InputSystem")?.Value ?? settings.Element("SupermodelInputSystem")?.Value);
        if (bool.TryParse(supermodel?.Element("MultiThreaded")?.Value ?? settings.Element("SupermodelMultiThreaded")?.Value, out var smmt))
        {
            SupermodelMultiThreaded = smmt;
        }

        if (int.TryParse(supermodel?.Element("PowerPcFrequency")?.Value ?? settings.Element("SupermodelPowerPcFrequency")?.Value, out var smppf))
        {
            SupermodelPowerPcFrequency = smppf;
        }

        if (bool.TryParse(supermodel?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("SupermodelShowSettingsBeforeLaunch")?.Value, out var smssbl))
        {
            SupermodelShowSettingsBeforeLaunch = smssbl;
        }

        // Xenia Fallback
        var xenia = settings.Element("Xenia");
        XeniaReadbackResolve = xenia?.Element("ReadbackResolve")?.Value ?? settings.Element("XeniaReadbackResolve")?.Value ?? XeniaReadbackResolve;
        if (bool.TryParse(xenia?.Element("GammaSrgb")?.Value ?? settings.Element("XeniaGammaSrgb")?.Value, out var xgs))
        {
            XeniaGammaSrgb = xgs;
        }

        if (bool.TryParse(xenia?.Element("Vibration")?.Value ?? settings.Element("XeniaVibration")?.Value, out var xvib))
        {
            XeniaVibration = xvib;
        }

        if (bool.TryParse(xenia?.Element("MountCache")?.Value ?? settings.Element("XeniaMountCache")?.Value, out var xmc))
        {
            XeniaMountCache = xmc;
        }

        XeniaGpu = xenia?.Element("Gpu")?.Value ?? settings.Element("XeniaGpu")?.Value ?? XeniaGpu;
        if (bool.TryParse(xenia?.Element("Vsync")?.Value ?? settings.Element("XeniaVsync")?.Value, out var xvs))
        {
            XeniaVsync = xvs;
        }

        if (int.TryParse(xenia?.Element("ResScaleX")?.Value ?? settings.Element("XeniaResScaleX")?.Value, out var xrsx))
        {
            XeniaResScaleX = xrsx;
        }

        if (int.TryParse(xenia?.Element("ResScaleY")?.Value ?? settings.Element("XeniaResScaleY")?.Value, out var xrsy))
        {
            XeniaResScaleY = xrsy;
        }

        if (bool.TryParse(xenia?.Element("Fullscreen")?.Value ?? settings.Element("XeniaFullscreen")?.Value, out var xfs))
        {
            XeniaFullscreen = xfs;
        }

        XeniaApu = xenia?.Element("Apu")?.Value ?? settings.Element("XeniaApu")?.Value ?? XeniaApu;
        if (bool.TryParse(xenia?.Element("Mute")?.Value ?? settings.Element("XeniaMute")?.Value, out var xmu))
        {
            XeniaMute = xmu;
        }

        XeniaAa = xenia?.Element("Aa")?.Value ?? settings.Element("XeniaAa")?.Value ?? XeniaAa;
        XeniaScaling = xenia?.Element("Scaling")?.Value ?? settings.Element("XeniaScaling")?.Value ?? XeniaScaling;
        if (bool.TryParse(xenia?.Element("ApplyPatches")?.Value ?? settings.Element("XeniaApplyPatches")?.Value, out var xap))
        {
            XeniaApplyPatches = xap;
        }

        if (bool.TryParse(xenia?.Element("DiscordPresence")?.Value ?? settings.Element("XeniaDiscordPresence")?.Value, out var xdp))
        {
            XeniaDiscordPresence = xdp;
        }

        if (int.TryParse(xenia?.Element("UserLanguage")?.Value ?? settings.Element("XeniaUserLanguage")?.Value, out var xul))
        {
            XeniaUserLanguage = xul;
        }

        XeniaHid = xenia?.Element("Hid")?.Value ?? settings.Element("XeniaHid")?.Value ?? XeniaHid;
        if (bool.TryParse(xenia?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("XeniaShowSettingsBeforeLaunch")?.Value, out var xssbl))
        {
            XeniaShowSettingsBeforeLaunch = xssbl;
        }

        // Yumir Fallback
        var yumir = settings.Element("Yumir");
        if (bool.TryParse(yumir?.Element("Fullscreen")?.Value ?? settings.Element("YumirFullscreen")?.Value, out var yf))
        {
            YumirFullscreen = yf;
        }

        if (double.TryParse(yumir?.Element("Volume")?.Value ?? settings.Element("YumirVolume")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var yv))
        {
            YumirVolume = yv;
        }

        if (bool.TryParse(yumir?.Element("Mute")?.Value ?? settings.Element("YumirMute")?.Value, out var ym))
        {
            YumirMute = ym;
        }

        YumirVideoStandard = yumir?.Element("VideoStandard")?.Value ?? settings.Element("YumirVideoStandard")?.Value ?? YumirVideoStandard;
        if (bool.TryParse(yumir?.Element("AutoDetectRegion")?.Value ?? settings.Element("YumirAutoDetectRegion")?.Value, out var yadr))
        {
            YumirAutoDetectRegion = yadr;
        }

        if (bool.TryParse(yumir?.Element("PauseWhenUnfocused")?.Value ?? settings.Element("YumirPauseWhenUnfocused")?.Value, out var ypwu))
        {
            YumirPauseWhenUnfocused = ypwu;
        }

        if (double.TryParse(yumir?.Element("ForcedAspect")?.Value ?? settings.Element("YumirForcedAspect")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var yfa))
        {
            YumirForcedAspect = yfa;
        }

        if (bool.TryParse(yumir?.Element("ForceAspectRatio")?.Value ?? settings.Element("YumirForceAspectRatio")?.Value, out var yfar))
        {
            YumirForceAspectRatio = yfar;
        }

        if (bool.TryParse(yumir?.Element("ReduceLatency")?.Value ?? settings.Element("YumirReduceLatency")?.Value, out var yrl))
        {
            YumirReduceLatency = yrl;
        }

        if (bool.TryParse(yumir?.Element("ShowSettingsBeforeLaunch")?.Value ?? settings.Element("YumirShowSettingsBeforeLaunch")?.Value, out var yssbl))
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
        // 1. Capture consistent snapshot under read lock
        SettingsManager snapshot;
        _settingsLock.EnterReadLock();
        try
        {
            snapshot = new SettingsManager(_configuration);
            snapshot.CopyFrom(this);
        }
        finally
        {
            _settingsLock.ExitReadLock();
        }

        // 2. Perform serialization and I/O outside the lock
        try
        {
            var root = BuildXElement(snapshot);
            root.Save(_filePath);
        }
        catch (Exception ex)
        {
            App.ServiceProvider?.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving settings.xml");
        }
    }

    private static XElement BuildXElement(SettingsManager s)
    {
        return new XElement("Settings",
            // Application Settings
            new XElement("Application",
                new XElement("ThumbnailSize", s.ThumbnailSize),
                new XElement("GamesPerPage", s.GamesPerPage),
                new XElement("ShowGames", s.ShowGames),
                new XElement("ViewMode", s.ViewMode),
                new XElement("EnableGamePadNavigation", s.EnableGamePadNavigation),
                new XElement("VideoUrl", s.VideoUrl),
                new XElement("InfoUrl", s.InfoUrl),
                new XElement("BaseTheme", s.BaseTheme),
                new XElement("AccentColor", s.AccentColor),
                new XElement("Language", s.Language),
                new XElement("DeadZoneX", s.DeadZoneX.ToString(CultureInfo.InvariantCulture)),
                new XElement("DeadZoneY", s.DeadZoneY.ToString(CultureInfo.InvariantCulture)),
                new XElement("ButtonAspectRatio", s.ButtonAspectRatio),
                new XElement("EnableFuzzyMatching", s.EnableFuzzyMatching),
                new XElement("FuzzyMatchingThreshold", s.FuzzyMatchingThreshold.ToString(CultureInfo.InvariantCulture)),
                new XElement("EnableNotificationSound", s.EnableNotificationSound),
                new XElement("CustomNotificationSoundFile", s.CustomNotificationSoundFile),
                new XElement("RaUsername", s.RaUsername),
                new XElement("RaApiKey", s.RaApiKey),
                new XElement("RaPassword", s.RaPassword),
                new XElement("RaToken", s.RaToken),
                new XElement("OverlayRetroAchievementButton", s.OverlayRetroAchievementButton),
                new XElement("OverlayOpenVideoButton", s.OverlayOpenVideoButton),
                new XElement("OverlayOpenInfoButton", s.OverlayOpenInfoButton),
                new XElement("AdditionalSystemFoldersExpanded", s.AdditionalSystemFoldersExpanded),
                new XElement("Emulator1Expanded", s.Emulator1Expanded),
                new XElement("Emulator2Expanded", s.Emulator2Expanded),
                new XElement("Emulator3Expanded", s.Emulator3Expanded),
                new XElement("Emulator4Expanded", s.Emulator4Expanded),
                new XElement("Emulator5Expanded", s.Emulator5Expanded)
            ),

            // Ares
            new XElement("Ares",
                new XElement("VideoDriver", s.AresVideoDriver),
                new XElement("Exclusive", s.AresExclusive),
                new XElement("Shader", s.AresShader),
                new XElement("Multiplier", s.AresMultiplier),
                new XElement("AspectCorrection", s.AresAspectCorrection),
                new XElement("Mute", s.AresMute),
                new XElement("Volume", s.AresVolume.ToString(CultureInfo.InvariantCulture)),
                new XElement("FastBoot", s.AresFastBoot),
                new XElement("Rewind", s.AresRewind),
                new XElement("RunAhead", s.AresRunAhead),
                new XElement("AutoSaveMemory", s.AresAutoSaveMemory),
                new XElement("ShowSettingsBeforeLaunch", s.AresShowSettingsBeforeLaunch)
            ),

            // Azahar
            new XElement("Azahar",
                new XElement("GraphicsApi", s.AzaharGraphicsApi),
                new XElement("ResolutionFactor", s.AzaharResolutionFactor),
                new XElement("UseVsync", s.AzaharUseVsync),
                new XElement("AsyncShaderCompilation", s.AzaharAsyncShaderCompilation),
                new XElement("Fullscreen", s.AzaharFullscreen),
                new XElement("Volume", s.AzaharVolume),
                new XElement("IsNew3ds", s.AzaharIsNew3ds),
                new XElement("LayoutOption", s.AzaharLayoutOption),
                new XElement("ShowSettingsBeforeLaunch", s.AzaharShowSettingsBeforeLaunch),
                new XElement("EnableAudioStretching", s.AzaharEnableAudioStretching)
            ),

            // Blastem
            new XElement("Blastem",
                new XElement("Fullscreen", s.BlastemFullscreen),
                new XElement("Vsync", s.BlastemVsync),
                new XElement("Aspect", s.BlastemAspect),
                new XElement("Scaling", s.BlastemScaling),
                new XElement("Scanlines", s.BlastemScanlines),
                new XElement("AudioRate", s.BlastemAudioRate),
                new XElement("SyncSource", s.BlastemSyncSource),
                new XElement("ShowSettingsBeforeLaunch", s.BlastemShowSettingsBeforeLaunch)
            ),

            // Cemu
            new XElement("Cemu",
                new XElement("Fullscreen", s.CemuFullscreen),
                new XElement("GraphicApi", s.CemuGraphicApi),
                new XElement("Vsync", s.CemuVsync),
                new XElement("AsyncCompile", s.CemuAsyncCompile),
                new XElement("TvVolume", s.CemuTvVolume),
                new XElement("ConsoleLanguage", s.CemuConsoleLanguage),
                new XElement("DiscordPresence", s.CemuDiscordPresence),
                new XElement("ShowSettingsBeforeLaunch", s.CemuShowSettingsBeforeLaunch)
            ),

            // Daphne
            new XElement("Daphne",
                new XElement("Fullscreen", s.DaphneFullscreen),
                new XElement("ResX", s.DaphneResX),
                new XElement("ResY", s.DaphneResY),
                new XElement("DisableCrosshairs", s.DaphneDisableCrosshairs),
                new XElement("Bilinear", s.DaphneBilinear),
                new XElement("EnableSound", s.DaphneEnableSound),
                new XElement("UseOverlays", s.DaphneUseOverlays),
                new XElement("ShowSettingsBeforeLaunch", s.DaphneShowSettingsBeforeLaunch)
            ),

            // Dolphin
            new XElement("Dolphin",
                new XElement("GfxBackend", s.DolphinGfxBackend),
                new XElement("DspThread", s.DolphinDspThread),
                new XElement("WiimoteContinuousScanning", s.DolphinWiimoteContinuousScanning),
                new XElement("WiimoteEnableSpeaker", s.DolphinWiimoteEnableSpeaker),
                new XElement("ShowSettingsBeforeLaunch", s.DolphinShowSettingsBeforeLaunch)
            ),

            // DuckStation
            new XElement("DuckStation",
                new XElement("StartFullscreen", s.DuckStationStartFullscreen),
                new XElement("PauseOnFocusLoss", s.DuckStationPauseOnFocusLoss),
                new XElement("SaveStateOnExit", s.DuckStationSaveStateOnExit),
                new XElement("RewindEnable", s.DuckStationRewindEnable),
                new XElement("RunaheadFrameCount", s.DuckStationRunaheadFrameCount),
                new XElement("Renderer", s.DuckStationRenderer),
                new XElement("ResolutionScale", s.DuckStationResolutionScale),
                new XElement("TextureFilter", s.DuckStationTextureFilter),
                new XElement("WidescreenHack", s.DuckStationWidescreenHack),
                new XElement("PgxpEnable", s.DuckStationPgxpEnable),
                new XElement("AspectRatio", s.DuckStationAspectRatio),
                new XElement("Vsync", s.DuckStationVsync),
                new XElement("OutputVolume", s.DuckStationOutputVolume),
                new XElement("OutputMuted", s.DuckStationOutputMuted),
                new XElement("ShowSettingsBeforeLaunch", s.DuckStationShowSettingsBeforeLaunch)
            ),

            // Flycast
            new XElement("Flycast",
                new XElement("Fullscreen", s.FlycastFullscreen),
                new XElement("Width", s.FlycastWidth),
                new XElement("Height", s.FlycastHeight),
                new XElement("Maximized", s.FlycastMaximized),
                new XElement("ShowSettingsBeforeLaunch", s.FlycastShowSettingsBeforeLaunch)
            ),

            // MAME
            new XElement("Mame",
                new XElement("Video", s.MameVideo),
                new XElement("Window", s.MameWindow),
                new XElement("Maximize", s.MameMaximize),
                new XElement("KeepAspect", s.MameKeepAspect),
                new XElement("SkipGameInfo", s.MameSkipGameInfo),
                new XElement("Autosave", s.MameAutosave),
                new XElement("ConfirmQuit", s.MameConfirmQuit),
                new XElement("Joystick", s.MameJoystick),
                new XElement("ShowSettingsBeforeLaunch", s.MameShowSettingsBeforeLaunch),
                new XElement("Autoframeskip", s.MameAutoframeskip),
                new XElement("BgfxBackend", s.MameBgfxBackend),
                new XElement("BgfxScreenChains", s.MameBgfxScreenChains),
                new XElement("Filter", s.MameFilter),
                new XElement("Cheat", s.MameCheat),
                new XElement("Rewind", s.MameRewind),
                new XElement("NvramSave", s.MameNvramSave)
            ),

            // Mednafen
            new XElement("Mednafen",
                new XElement("VideoDriver", s.MednafenVideoDriver),
                new XElement("Fullscreen", s.MednafenFullscreen),
                new XElement("Vsync", s.MednafenVsync),
                new XElement("Stretch", s.MednafenStretch),
                new XElement("Bilinear", s.MednafenBilinear),
                new XElement("Scanlines", s.MednafenScanlines),
                new XElement("Shader", s.MednafenShader),
                new XElement("Special", s.MednafenSpecial),
                new XElement("Volume", s.MednafenVolume),
                new XElement("Cheats", s.MednafenCheats),
                new XElement("Rewind", s.MednafenRewind),
                new XElement("ShowSettingsBeforeLaunch", s.MednafenShowSettingsBeforeLaunch)
            ),

            // Mesen
            new XElement("Mesen",
                new XElement("Fullscreen", s.MesenFullscreen),
                new XElement("Vsync", s.MesenVsync),
                new XElement("AspectRatio", s.MesenAspectRatio),
                new XElement("Bilinear", s.MesenBilinear),
                new XElement("VideoFilter", s.MesenVideoFilter),
                new XElement("EnableAudio", s.MesenEnableAudio),
                new XElement("MasterVolume", s.MesenMasterVolume),
                new XElement("Rewind", s.MesenRewind),
                new XElement("RunAhead", s.MesenRunAhead),
                new XElement("PauseInBackground", s.MesenPauseInBackground),
                new XElement("ShowSettingsBeforeLaunch", s.MesenShowSettingsBeforeLaunch)
            ),

            // PCSX2
            new XElement("Pcsx2",
                new XElement("StartFullscreen", s.Pcsx2StartFullscreen),
                new XElement("AspectRatio", s.Pcsx2AspectRatio),
                new XElement("Renderer", s.Pcsx2Renderer),
                new XElement("UpscaleMultiplier", s.Pcsx2UpscaleMultiplier),
                new XElement("Vsync", s.Pcsx2Vsync),
                new XElement("EnableCheats", s.Pcsx2EnableCheats),
                new XElement("EnableWidescreenPatches", s.Pcsx2EnableWidescreenPatches),
                new XElement("Volume", s.Pcsx2Volume),
                new XElement("AchievementsEnabled", s.Pcsx2AchievementsEnabled),
                new XElement("AchievementsHardcore", s.Pcsx2AchievementsHardcore),
                new XElement("ShowSettingsBeforeLaunch", s.Pcsx2ShowSettingsBeforeLaunch)
            ),

            // Raine
            new XElement("Raine",
                new XElement("Fullscreen", s.RaineFullscreen),
                new XElement("ResX", s.RaineResX),
                new XElement("ResY", s.RaineResY),
                new XElement("FixAspectRatio", s.RaineFixAspectRatio),
                new XElement("Vsync", s.RaineVsync),
                new XElement("SoundDriver", s.RaineSoundDriver),
                new XElement("SampleRate", s.RaineSampleRate),
                new XElement("ShowSettingsBeforeLaunch", s.RaineShowSettingsBeforeLaunch),
                new XElement("ShowFps", s.RaineShowFps),
                new XElement("FrameSkip", s.RaineFrameSkip),
                new XElement("NeoCdBios", s.RaineNeoCdBios),
                new XElement("MusicVolume", s.RaineMusicVolume),
                new XElement("SfxVolume", s.RaineSfxVolume),
                new XElement("MuteSfx", s.RaineMuteSfx),
                new XElement("MuteMusic", s.RaineMuteMusic),
                new XElement("RomDirectory", s.RaineRomDirectory)
            ),

            // Redream
            new XElement("Redream",
                new XElement("Cable", s.RedreamCable),
                new XElement("Broadcast", s.RedreamBroadcast),
                new XElement("Language", s.RedreamLanguage),
                new XElement("Region", s.RedreamRegion),
                new XElement("Vsync", s.RedreamVsync),
                new XElement("Frameskip", s.RedreamFrameskip),
                new XElement("Aspect", s.RedreamAspect),
                new XElement("Res", s.RedreamRes),
                new XElement("Renderer", s.RedreamRenderer),
                new XElement("Fullmode", s.RedreamFullmode),
                new XElement("Volume", s.RedreamVolume),
                new XElement("Latency", s.RedreamLatency),
                new XElement("Framerate", s.RedreamFramerate),
                new XElement("Width", s.RedreamWidth),
                new XElement("Height", s.RedreamHeight),
                new XElement("ShowSettingsBeforeLaunch", s.RedreamShowSettingsBeforeLaunch)
            ),

            // RetroArch
            new XElement("RetroArch",
                new XElement("CheevosEnable", s.RetroArchCheevosEnable),
                new XElement("CheevosHardcore", s.RetroArchCheevosHardcore),
                new XElement("Fullscreen", s.RetroArchFullscreen),
                new XElement("Vsync", s.RetroArchVsync),
                new XElement("VideoDriver", s.RetroArchVideoDriver),
                new XElement("AudioEnable", s.RetroArchAudioEnable),
                new XElement("AudioMute", s.RetroArchAudioMute),
                new XElement("MenuDriver", s.RetroArchMenuDriver),
                new XElement("PauseNonActive", s.RetroArchPauseNonActive),
                new XElement("SaveOnExit", s.RetroArchSaveOnExit),
                new XElement("AutoSaveState", s.RetroArchAutoSaveState),
                new XElement("AutoLoadState", s.RetroArchAutoLoadState),
                new XElement("Rewind", s.RetroArchRewind),
                new XElement("ThreadedVideo", s.RetroArchThreadedVideo),
                new XElement("Bilinear", s.RetroArchBilinear),
                new XElement("ShowSettingsBeforeLaunch", s.RetroArchShowSettingsBeforeLaunch),
                new XElement("AspectRatioIndex", s.RetroArchAspectRatioIndex),
                new XElement("ScaleInteger", s.RetroArchScaleInteger),
                new XElement("ShaderEnable", s.RetroArchShaderEnable),
                new XElement("HardSync", s.RetroArchHardSync),
                new XElement("RunAhead", s.RetroArchRunAhead),
                new XElement("ShowAdvancedSettings", s.RetroArchShowAdvancedSettings),
                new XElement("DiscordAllow", s.RetroArchDiscordAllow),
                new XElement("OverrideSystemDir", s.RetroArchOverrideSystemDir),
                new XElement("OverrideSaveDir", s.RetroArchOverrideSaveDir),
                new XElement("OverrideStateDir", s.RetroArchOverrideStateDir),
                new XElement("OverrideScreenshotDir", s.RetroArchOverrideScreenshotDir)
            ),

            // RPCS3
            new XElement("Rpcs3",
                new XElement("Renderer", s.Rpcs3Renderer),
                new XElement("Resolution", s.Rpcs3Resolution),
                new XElement("AspectRatio", s.Rpcs3AspectRatio),
                new XElement("Vsync", s.Rpcs3Vsync),
                new XElement("ResolutionScale", s.Rpcs3ResolutionScale),
                new XElement("AnisotropicFilter", s.Rpcs3AnisotropicFilter),
                new XElement("PpuDecoder", s.Rpcs3PpuDecoder),
                new XElement("SpuDecoder", s.Rpcs3SpuDecoder),
                new XElement("AudioRenderer", s.Rpcs3AudioRenderer),
                new XElement("AudioBuffering", s.Rpcs3AudioBuffering),
                new XElement("StartFullscreen", s.Rpcs3StartFullscreen),
                new XElement("ShowSettingsBeforeLaunch", s.Rpcs3ShowSettingsBeforeLaunch)
            ),

            // SEGA Model 2
            new XElement("SegaModel2",
                new XElement("ResX", s.SegaModel2ResX),
                new XElement("ResY", s.SegaModel2ResY),
                new XElement("WideScreen", s.SegaModel2WideScreen),
                new XElement("Bilinear", s.SegaModel2Bilinear),
                new XElement("Trilinear", s.SegaModel2Trilinear),
                new XElement("FilterTilemaps", s.SegaModel2FilterTilemaps),
                new XElement("DrawCross", s.SegaModel2DrawCross),
                new XElement("Fsaa", s.SegaModel2Fsaa),
                new XElement("XInput", s.SegaModel2XInput),
                new XElement("EnableFf", s.SegaModel2EnableFf),
                new XElement("HoldGears", s.SegaModel2HoldGears),
                new XElement("UseRawInput", s.SegaModel2UseRawInput),
                new XElement("ShowSettingsBeforeLaunch", s.SegaModel2ShowSettingsBeforeLaunch)
            ),

            // Stella
            new XElement("Stella",
                new XElement("Fullscreen", s.StellaFullscreen),
                new XElement("Vsync", s.StellaVsync),
                new XElement("VideoDriver", s.StellaVideoDriver),
                new XElement("CorrectAspect", s.StellaCorrectAspect),
                new XElement("TvFilter", s.StellaTvFilter),
                new XElement("Scanlines", s.StellaScanlines),
                new XElement("AudioEnabled", s.StellaAudioEnabled),
                new XElement("AudioVolume", s.StellaAudioVolume),
                new XElement("TimeMachine", s.StellaTimeMachine),
                new XElement("ConfirmExit", s.StellaConfirmExit),
                new XElement("ShowSettingsBeforeLaunch", s.StellaShowSettingsBeforeLaunch)
            ),

            // Supermodel
            new XElement("Supermodel",
                new XElement("New3DEngine", s.SupermodelNew3DEngine),
                new XElement("QuadRendering", s.SupermodelQuadRendering),
                new XElement("Fullscreen", s.SupermodelFullscreen),
                new XElement("ResX", s.SupermodelResX),
                new XElement("ResY", s.SupermodelResY),
                new XElement("WideScreen", s.SupermodelWideScreen),
                new XElement("Stretch", s.SupermodelStretch),
                new XElement("Vsync", s.SupermodelVsync),
                new XElement("Throttle", s.SupermodelThrottle),
                new XElement("MusicVolume", s.SupermodelMusicVolume),
                new XElement("SoundVolume", s.SupermodelSoundVolume),
                new XElement("InputSystem", s.SupermodelInputSystem),
                new XElement("MultiThreaded", s.SupermodelMultiThreaded),
                new XElement("PowerPcFrequency", s.SupermodelPowerPcFrequency),
                new XElement("ShowSettingsBeforeLaunch", s.SupermodelShowSettingsBeforeLaunch)
            ),

            // Xenia
            new XElement("Xenia",
                new XElement("ReadbackResolve", s.XeniaReadbackResolve),
                new XElement("GammaSrgb", s.XeniaGammaSrgb),
                new XElement("Vibration", s.XeniaVibration),
                new XElement("MountCache", s.XeniaMountCache),
                new XElement("Gpu", s.XeniaGpu),
                new XElement("Vsync", s.XeniaVsync),
                new XElement("ResScaleX", s.XeniaResScaleX),
                new XElement("ResScaleY", s.XeniaResScaleY),
                new XElement("Fullscreen", s.XeniaFullscreen),
                new XElement("Apu", s.XeniaApu),
                new XElement("Mute", s.XeniaMute),
                new XElement("Aa", s.XeniaAa),
                new XElement("Scaling", s.XeniaScaling),
                new XElement("ApplyPatches", s.XeniaApplyPatches),
                new XElement("DiscordPresence", s.XeniaDiscordPresence),
                new XElement("UserLanguage", s.XeniaUserLanguage),
                new XElement("Hid", s.XeniaHid),
                new XElement("ShowSettingsBeforeLaunch", s.XeniaShowSettingsBeforeLaunch)
            ),

            // Yumir
            new XElement("Yumir",
                new XElement("Fullscreen", s.YumirFullscreen),
                new XElement("Volume", s.YumirVolume.ToString(CultureInfo.InvariantCulture)),
                new XElement("Mute", s.YumirMute),
                new XElement("VideoStandard", s.YumirVideoStandard),
                new XElement("AutoDetectRegion", s.YumirAutoDetectRegion),
                new XElement("PauseWhenUnfocused", s.YumirPauseWhenUnfocused),
                new XElement("ForcedAspect", s.YumirForcedAspect.ToString(CultureInfo.InvariantCulture)),
                new XElement("ForceAspectRatio", s.YumirForceAspectRatio),
                new XElement("ReduceLatency", s.YumirReduceLatency),
                new XElement("ShowSettingsBeforeLaunch", s.YumirShowSettingsBeforeLaunch)
            ),

            // SystemPlayTimes
            new XElement("SystemPlayTimes",
                s.SystemPlayTimes.Select(static pt =>
                    new XElement("SystemPlayTime",
                        new XElement("SystemName", pt.SystemName),
                        new XElement("PlayTime", pt.PlayTime)
                    )
                )
            )
        );
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
        CopyFrom(new SettingsManager(_configuration));
    }

    private void SetDefaultsAndSave()
    {
        ResetToDefaults();
        Save();
    }

    public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
    {
        if (string.IsNullOrWhiteSpace(systemName) || playTime == TimeSpan.Zero) return;

        _settingsLock.EnterWriteLock();
        try
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
        finally
        {
            _settingsLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        _settingsLock?.Dispose();

        GC.SuppressFinalize(this);
    }
}
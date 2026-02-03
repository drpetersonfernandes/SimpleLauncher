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
    [Key(0)] public int ThumbnailSize { get; set; } = 250;
    [Key(1)] public int GamesPerPage { get; set; } = 200;
    [Key(2)] public string ShowGames { get; set; } = "ShowAll";
    [Key(3)] public string ViewMode { get; set; } = "GridView";
    [Key(4)] public bool EnableGamePadNavigation { get; set; }
    [Key(5)] public string VideoUrl { get; set; } = App.Configuration?["Urls:YouTubeSearch"] ?? "https://www.youtube.com/results?search_query=";
    [Key(6)] public string InfoUrl { get; set; } = App.Configuration?["Urls:IgdbSearch"] ?? "https://www.igdb.com/search?q=";
    [Key(7)] public string BaseTheme { get; set; } = "Light";
    [Key(8)] public string AccentColor { get; set; } = "Blue";
    [Key(9)] public string Language { get; set; } = "en";
    [Key(10)] public float DeadZoneX { get; set; } = DefaultDeadZoneX;
    [Key(11)] public float DeadZoneY { get; set; } = DefaultDeadZoneY;
    [Key(12)] public string ButtonAspectRatio { get; set; } = "Square";
    [Key(13)] public bool EnableFuzzyMatching { get; set; } = true;
    [Key(14)] public double FuzzyMatchingThreshold { get; set; } = 0.80;
    [IgnoreMember] public const float DefaultDeadZoneX = 0.05f;
    [IgnoreMember] public const float DefaultDeadZoneY = 0.02f;
    [Key(15)] public bool EnableNotificationSound { get; set; } = true;
    [Key(16)] public string CustomNotificationSoundFile { get; set; } = DefaultNotificationSoundFileName;
    [Key(17)] public string RaUsername { get; set; } = string.Empty;
    [Key(18)] public string RaApiKey { get; set; } = string.Empty;
    [Key(19)] public string RaPassword { get; set; } = string.Empty;
    [Key(30)] public string RaToken { get; set; } = string.Empty;
    [Key(20)] public bool OverlayRetroAchievementButton { get; set; }
    [Key(21)] public bool OverlayOpenVideoButton { get; set; } = true;
    [Key(22)] public bool OverlayOpenInfoButton { get; set; }
    [Key(23)] public bool AdditionalSystemFoldersExpanded { get; set; } = true;
    [Key(24)] public bool Emulator1Expanded { get; set; } = true;
    [Key(25)] public bool Emulator2Expanded { get; set; } = true;
    [Key(26)] public bool Emulator3Expanded { get; set; } = true;
    [Key(27)] public bool Emulator4Expanded { get; set; } = true;
    [Key(28)] public bool Emulator5Expanded { get; set; } = true;
    [Key(29)] public List<SystemPlayTime> SystemPlayTimes { get; set; } = [];

    // Ares
    [Key(100)] public string AresVideoDriver { get; set; } = "OpenGL 3.2";
    [Key(101)] public bool AresExclusive { get; set; } // Fullscreen
    [Key(102)] public string AresShader { get; set; } = "None";
    [Key(103)] public int AresMultiplier { get; set; } = 2;
    [Key(104)] public string AresAspectCorrection { get; set; } = "Standard";
    [Key(105)] public bool AresMute { get; set; }
    [Key(106)] public double AresVolume { get; set; } = 1.0;
    [Key(107)] public bool AresFastBoot { get; set; }
    [Key(108)] public bool AresRewind { get; set; }
    [Key(109)] public bool AresRunAhead { get; set; }
    [Key(110)] public bool AresAutoSaveMemory { get; set; } = true;
    [Key(111)] public bool AresShowSettingsBeforeLaunch { get; set; } = true;

    // Azahar
    [Key(200)] public int AzaharGraphicsApi { get; set; } = 1; // 1=Vulkan
    [Key(201)] public int AzaharResolutionFactor { get; set; } = 1; // 0=Auto, 1=1x, 2=2x...
    [Key(202)] public bool AzaharUseVsync { get; set; } = true;
    [Key(203)] public bool AzaharAsyncShaderCompilation { get; set; } = true;
    [Key(204)] public bool AzaharFullscreen { get; set; } = true;
    [Key(205)] public int AzaharVolume { get; set; } = 100;
    [Key(206)] public bool AzaharIsNew3ds { get; set; } = true;
    [Key(207)] public int AzaharLayoutOption { get; set; } // 0=Default, 1=Single, 2=Large...
    [Key(208)] public bool AzaharShowSettingsBeforeLaunch { get; set; } = true;
    [Key(209)] public bool AzaharEnableAudioStretching { get; set; } = true;

    // Blastem
    [Key(300)] public bool BlastemFullscreen { get; set; }
    [Key(301)] public bool BlastemVsync { get; set; }
    [Key(302)] public string BlastemAspect { get; set; } = "4:3";
    [Key(303)] public string BlastemScaling { get; set; } = "linear";
    [Key(304)] public bool BlastemScanlines { get; set; }
    [Key(305)] public int BlastemAudioRate { get; set; } = 48000;
    [Key(306)] public string BlastemSyncSource { get; set; } = "audio";
    [Key(307)] public bool BlastemShowSettingsBeforeLaunch { get; set; } = true;

    // Cemu
    [Key(400)] public bool CemuFullscreen { get; set; }
    [Key(401)] public int CemuGraphicApi { get; set; } = 1; // 0=OpenGL, 1=Vulkan
    [Key(402)] public int CemuVsync { get; set; } = 1; // 0=Off, 1=On
    [Key(403)] public bool CemuAsyncCompile { get; set; } = true;
    [Key(404)] public int CemuTvVolume { get; set; } = 50;
    [Key(405)] public int CemuConsoleLanguage { get; set; } = 1; // 1=English
    [Key(406)] public bool CemuDiscordPresence { get; set; } = true;
    [Key(407)] public bool CemuShowSettingsBeforeLaunch { get; set; } = true;

    // Daphne
    [Key(500)] public bool DaphneFullscreen { get; set; }
    [Key(501)] public int DaphneResX { get; set; } = 640;
    [Key(502)] public int DaphneResY { get; set; } = 480;
    [Key(503)] public bool DaphneDisableCrosshairs { get; set; }
    [Key(504)] public bool DaphneBilinear { get; set; } = true;
    [Key(505)] public bool DaphneEnableSound { get; set; } = true;
    [Key(506)] public bool DaphneUseOverlays { get; set; } = true;
    [Key(507)] public bool DaphneShowSettingsBeforeLaunch { get; set; } = true;

    // Dolphin
    [Key(600)] public string DolphinGfxBackend { get; set; } = "Vulkan";
    [Key(601)] public bool DolphinDspThread { get; set; } = true;
    [Key(602)] public bool DolphinWiimoteContinuousScanning { get; set; } = true;
    [Key(603)] public bool DolphinWiimoteEnableSpeaker { get; set; } = true;
    [Key(604)] public bool DolphinShowSettingsBeforeLaunch { get; set; } = true;

    // DuckStation
    [Key(700)] public bool DuckStationStartFullscreen { get; set; }
    [Key(701)] public bool DuckStationPauseOnFocusLoss { get; set; } = true;
    [Key(702)] public bool DuckStationSaveStateOnExit { get; set; } = true;
    [Key(703)] public bool DuckStationRewindEnable { get; set; }
    [Key(704)] public int DuckStationRunaheadFrameCount { get; set; }
    [Key(705)] public string DuckStationRenderer { get; set; } = "Automatic";
    [Key(706)] public int DuckStationResolutionScale { get; set; } = 2;
    [Key(707)] public string DuckStationTextureFilter { get; set; } = "Nearest";
    [Key(708)] public bool DuckStationWidescreenHack { get; set; }
    [Key(709)] public bool DuckStationPgxpEnable { get; set; }
    [Key(710)] public string DuckStationAspectRatio { get; set; } = "16:9";
    [Key(711)] public bool DuckStationVsync { get; set; }
    [Key(712)] public int DuckStationOutputVolume { get; set; } = 100;
    [Key(713)] public bool DuckStationOutputMuted { get; set; }
    [Key(714)] public bool DuckStationShowSettingsBeforeLaunch { get; set; } = true;

    // Flycast
    [Key(800)] public bool FlycastFullscreen { get; set; }
    [Key(801)] public int FlycastWidth { get; set; } = 640;
    [Key(802)] public int FlycastHeight { get; set; } = 480;
    [Key(803)] public bool FlycastMaximized { get; set; }
    [Key(804)] public bool FlycastShowSettingsBeforeLaunch { get; set; } = true;

    // MAME
    [Key(900)] public string MameVideo { get; set; } = "auto"; // auto, d3d, opengl, bgfx
    [Key(901)] public bool MameWindow { get; set; }
    [Key(902)] public bool MameMaximize { get; set; } = true;
    [Key(903)] public bool MameKeepAspect { get; set; } = true;
    [Key(904)] public bool MameSkipGameInfo { get; set; } = true;
    [Key(905)] public bool MameAutosave { get; set; }
    [Key(906)] public bool MameConfirmQuit { get; set; }
    [Key(907)] public bool MameJoystick { get; set; } = true;
    [Key(908)] public bool MameShowSettingsBeforeLaunch { get; set; } = true;
    [Key(909)] public bool MameAutoframeskip { get; set; }
    [Key(910)] public string MameBgfxBackend { get; set; } = "auto";
    [Key(911)] public string MameBgfxScreenChains { get; set; } = "default";
    [Key(912)] public bool MameFilter { get; set; } = true;
    [Key(913)] public bool MameCheat { get; set; }
    [Key(914)] public bool MameRewind { get; set; }
    [Key(915)] public bool MameNvramSave { get; set; } = true;

    // Mednafen
    [Key(1000)] public string MednafenVideoDriver { get; set; } = "opengl";
    [Key(1001)] public bool MednafenFullscreen { get; set; }
    [Key(1002)] public bool MednafenVsync { get; set; } = true;
    [Key(1003)] public string MednafenStretch { get; set; } = "aspect";
    [Key(1004)] public bool MednafenBilinear { get; set; }
    [Key(1005)] public int MednafenScanlines { get; set; }
    [Key(1006)] public string MednafenShader { get; set; } = "none";
    [Key(1007)] public int MednafenVolume { get; set; } = 100;
    [Key(1008)] public bool MednafenCheats { get; set; } = true;
    [Key(1009)] public bool MednafenRewind { get; set; }
    [Key(1010)] public bool MednafenShowSettingsBeforeLaunch { get; set; } = true;

    // Mesen
    [Key(1100)] public bool MesenFullscreen { get; set; }
    [Key(1101)] public bool MesenVsync { get; set; }
    [Key(1102)] public string MesenAspectRatio { get; set; } = "NoStretching";
    [Key(1103)] public bool MesenBilinear { get; set; }
    [Key(1104)] public string MesenVideoFilter { get; set; } = "None";
    [Key(1105)] public bool MesenEnableAudio { get; set; } = true;
    [Key(1106)] public int MesenMasterVolume { get; set; } = 100;
    [Key(1107)] public bool MesenRewind { get; set; }
    [Key(1108)] public int MesenRunAhead { get; set; }
    [Key(1109)] public bool MesenPauseInBackground { get; set; }
    [Key(1110)] public bool MesenShowSettingsBeforeLaunch { get; set; } = true;

    // PCSX2
    [Key(1200)] public bool Pcsx2StartFullscreen { get; set; } = true;
    [Key(1201)] public string Pcsx2AspectRatio { get; set; } = "16:9"; // 4:3, 16:9, Stretch
    [Key(1202)] public int Pcsx2Renderer { get; set; } = 14; // 14=Vulkan, 13=D3D12, 12=D3D11, 15=OpenGL, 11=Software
    [Key(1203)] public int Pcsx2UpscaleMultiplier { get; set; } = 2; // 1 (Native) to 8
    [Key(1204)] public bool Pcsx2Vsync { get; set; } // false=Off, true=On
    [Key(1205)] public bool Pcsx2EnableCheats { get; set; }
    [Key(1206)] public bool Pcsx2EnableWidescreenPatches { get; set; }
    [Key(1207)] public int Pcsx2Volume { get; set; } = 100;
    [Key(1208)] public bool Pcsx2AchievementsEnabled { get; set; }
    [Key(1209)] public bool Pcsx2AchievementsHardcore { get; set; } = true;
    [Key(1210)] public bool Pcsx2ShowSettingsBeforeLaunch { get; set; } = true;

    // RetroArch
    [Key(1300)] public bool RetroArchCheevosEnable { get; set; }
    [Key(1301)] public bool RetroArchCheevosHardcore { get; set; }
    [Key(1302)] public bool RetroArchFullscreen { get; set; }
    [Key(1303)] public bool RetroArchVsync { get; set; } = true;
    [Key(1304)] public string RetroArchVideoDriver { get; set; } = "gl";
    [Key(1305)] public bool RetroArchAudioEnable { get; set; } = true;
    [Key(1306)] public bool RetroArchAudioMute { get; set; }
    [Key(1307)] public string RetroArchMenuDriver { get; set; } = "ozone";
    [Key(1308)] public bool RetroArchPauseNonActive { get; set; } = true;
    [Key(1309)] public bool RetroArchSaveOnExit { get; set; } = true;
    [Key(1310)] public bool RetroArchAutoSaveState { get; set; }
    [Key(1311)] public bool RetroArchAutoLoadState { get; set; }
    [Key(1312)] public bool RetroArchRewind { get; set; }
    [Key(1313)] public bool RetroArchThreadedVideo { get; set; }
    [Key(1314)] public bool RetroArchBilinear { get; set; }
    [Key(1315)] public bool RetroArchShowSettingsBeforeLaunch { get; set; } = true;
    [Key(1316)] public string RetroArchAspectRatioIndex { get; set; } = "22"; // 22 = Core Provided
    [Key(1317)] public bool RetroArchScaleInteger { get; set; }
    [Key(1318)] public bool RetroArchShaderEnable { get; set; } = true;
    [Key(1319)] public bool RetroArchHardSync { get; set; }
    [Key(1320)] public bool RetroArchRunAhead { get; set; }
    [Key(1321)] public bool RetroArchShowAdvancedSettings { get; set; } = true;
    [Key(1322)] public bool RetroArchDiscordAllow { get; set; }
    [Key(1323)] public bool RetroArchOverrideSystemDir { get; set; }
    [Key(1324)] public bool RetroArchOverrideSaveDir { get; set; }
    [Key(1325)] public bool RetroArchOverrideStateDir { get; set; }
    [Key(1326)] public bool RetroArchOverrideScreenshotDir { get; set; }

    // RPCS3
    [Key(1400)] public string Rpcs3Renderer { get; set; } = "Vulkan";
    [Key(1401)] public string Rpcs3Resolution { get; set; } = "1280x720";
    [Key(1402)] public string Rpcs3AspectRatio { get; set; } = "16:9";
    [Key(1403)] public bool Rpcs3Vsync { get; set; }
    [Key(1404)] public int Rpcs3ResolutionScale { get; set; } = 100;
    [Key(1405)] public int Rpcs3AnisotropicFilter { get; set; }
    [Key(1406)] public string Rpcs3PpuDecoder { get; set; } = "Recompiler (LLVM)";
    [Key(1407)] public string Rpcs3SpuDecoder { get; set; } = "Recompiler (LLVM)";
    [Key(1408)] public string Rpcs3AudioRenderer { get; set; } = "Cubeb";
    [Key(1409)] public bool Rpcs3AudioBuffering { get; set; } = true;
    [Key(1410)] public bool Rpcs3StartFullscreen { get; set; }
    [Key(1411)] public bool Rpcs3ShowSettingsBeforeLaunch { get; set; } = true;

    // SEGA Model 2
    [Key(1500)] public int SegaModel2ResX { get; set; } = 640;
    [Key(1501)] public int SegaModel2ResY { get; set; } = 480;
    [Key(1502)] public int SegaModel2WideScreen { get; set; } // 0=4:3, 1=16:9, 2=16:10
    [Key(1503)] public bool SegaModel2Bilinear { get; set; } = true;
    [Key(1504)] public bool SegaModel2Trilinear { get; set; }
    [Key(1505)] public bool SegaModel2FilterTilemaps { get; set; }
    [Key(1506)] public bool SegaModel2DrawCross { get; set; } = true;
    [Key(1507)] public int SegaModel2Fsaa { get; set; }
    [Key(1508)] public bool SegaModel2XInput { get; set; }
    [Key(1509)] public bool SegaModel2EnableFf { get; set; }
    [Key(1510)] public bool SegaModel2HoldGears { get; set; }
    [Key(1511)] public bool SegaModel2UseRawInput { get; set; }
    [Key(1512)] public bool SegaModel2ShowSettingsBeforeLaunch { get; set; } = true;

    // Stella
    [Key(1600)] public bool StellaFullscreen { get; set; }
    [Key(1601)] public bool StellaVsync { get; set; } = true;
    [Key(1602)] public string StellaVideoDriver { get; set; } = "direct3d";
    [Key(1603)] public bool StellaCorrectAspect { get; set; } = true;
    [Key(1604)] public int StellaTvFilter { get; set; }
    [Key(1605)] public int StellaScanlines { get; set; }
    [Key(1606)] public bool StellaAudioEnabled { get; set; } = true;
    [Key(1607)] public int StellaAudioVolume { get; set; } = 80;
    [Key(1608)] public bool StellaTimeMachine { get; set; } = true;
    [Key(1609)] public bool StellaConfirmExit { get; set; }
    [Key(1610)] public bool StellaShowSettingsBeforeLaunch { get; set; } = true;

    // Supermodel
    [Key(1700)] public bool SupermodelNew3DEngine { get; set; } = true;
    [Key(1701)] public bool SupermodelQuadRendering { get; set; }
    [Key(1702)] public bool SupermodelFullscreen { get; set; } = true;
    [Key(1703)] public int SupermodelResX { get; set; } = 1920;
    [Key(1704)] public int SupermodelResY { get; set; } = 1080;
    [Key(1705)] public bool SupermodelWideScreen { get; set; } = true;
    [Key(1706)] public bool SupermodelStretch { get; set; }
    [Key(1707)] public bool SupermodelVsync { get; set; } = true;
    [Key(1708)] public bool SupermodelThrottle { get; set; } = true;
    [Key(1709)] public int SupermodelMusicVolume { get; set; } = 100;
    [Key(1710)] public int SupermodelSoundVolume { get; set; } = 100;

    [field: Key(1711)]
    public string SupermodelInputSystem
    {
        get;
        set
        {
            // Validate and normalize the input system value
            if (string.IsNullOrWhiteSpace(value))
            {
                field = "xinput";
                return;
            }

            var normalized = value.Trim().ToLowerInvariant();
            field = normalized is "xinput" or "dinput" or "rawinput" ? normalized : "xinput";
        }
    } = "xinput";

    [Key(1712)] public bool SupermodelMultiThreaded { get; set; } = true;
    [Key(1713)] public int SupermodelPowerPcFrequency { get; set; } = 50;
    [Key(1714)] public bool SupermodelShowSettingsBeforeLaunch { get; set; } = true;

    // Xenia
    [Key(1800)] public string XeniaReadbackResolve { get; set; } = "none"; // none, fast, full
    [Key(1801)] public bool XeniaGammaSrgb { get; set; }
    [Key(1802)] public bool XeniaVibration { get; set; } = true;
    [Key(1803)] public bool XeniaMountCache { get; set; } = true;
    [Key(1804)] public string XeniaGpu { get; set; } = "d3d12"; // d3d12, vulkan, null
    [Key(1805)] public bool XeniaVsync { get; set; } = true;
    [Key(1806)] public int XeniaResScaleX { get; set; } = 1;
    [Key(1807)] public int XeniaResScaleY { get; set; } = 1;
    [Key(1808)] public bool XeniaFullscreen { get; set; }
    [Key(1809)] public string XeniaApu { get; set; } = "xaudio2"; // xaudio2, sdl, nop, any
    [Key(1810)] public bool XeniaMute { get; set; }
    [Key(1811)] public string XeniaAa { get; set; } = ""; // "", fxaa, fxaa_extreme
    [Key(1812)] public string XeniaScaling { get; set; } = "fsr"; // fsr, cas, bilinear
    [Key(1813)] public bool XeniaApplyPatches { get; set; } = true;
    [Key(1814)] public bool XeniaDiscordPresence { get; set; } = true;
    [Key(1815)] public int XeniaUserLanguage { get; set; } = 1; // 1=English
    [Key(1816)] public string XeniaHid { get; set; } = "xinput"; // xinput, sdl, winkey, any
    [Key(1817)] public bool XeniaShowSettingsBeforeLaunch { get; set; } = true;

    // Yumir
    [Key(1900)] public bool YumirFullscreen { get; set; }
    [Key(1901)] public double YumirVolume { get; set; } = 0.8;
    [Key(1902)] public bool YumirMute { get; set; }
    [Key(1903)] public string YumirVideoStandard { get; set; } = "PAL"; // PAL, NTSC
    [Key(1904)] public bool YumirAutoDetectRegion { get; set; } = true;
    [Key(1905)] public bool YumirPauseWhenUnfocused { get; set; }
    [Key(1906)] public double YumirForcedAspect { get; set; } = 1.7777777777777777;
    [Key(1907)] public bool YumirForceAspectRatio { get; set; }
    [Key(1908)] public bool YumirReduceLatency { get; set; } = true;
    [Key(1909)] public bool YumirShowSettingsBeforeLaunch { get; set; } = true;

    [IgnoreMember] private const string DefaultSettingsFilePath = "settings.dat";
    [IgnoreMember] private const string OldSettingsFilePath = "settings.xml";
    [IgnoreMember] private const string DefaultNotificationSoundFileName = "click.mp3";

    public void Load()
    {
        lock (_saveLock)
        {
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
                    App.ServiceProvider?.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading settings.dat. Attempting fallback.");
                }
            }

            if (File.Exists(_xmlFilePath))
            {
                if (MigrateFromXml()) return;
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

            // Reset to defaults first so any missing XML values remain at default
            ResetToDefaults();

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
            RaUsername = settings.Element("RA_Username")?.Value ?? RaUsername;
            RaApiKey = settings.Element("RA_ApiKey")?.Value ?? RaApiKey;
            RaPassword = settings.Element("RA_Password")?.Value ?? RaPassword;
            RaToken = settings.Element("RA_Token")?.Value ?? RaToken;
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

            // MAME (Only migrate what was previously in XML)
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

            if (bool.TryParse(settings.Element("MameShowSettingsBeforeLaunch")?.Value, out var mss))
            {
                MameShowSettingsBeforeLaunch = mss;
            }

            if (bool.TryParse(settings.Element("MameAutoframeskip")?.Value, out var mafs))
            {
                MameAutoframeskip = mafs;
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

            if (bool.TryParse(settings.Element("RetroArchShowSettingsBeforeLaunch")?.Value, out var rass))
            {
                RetroArchShowSettingsBeforeLaunch = rass;
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

            // Xenia
            XeniaGpu = settings.Element("XeniaGpu")?.Value ?? XeniaGpu;
            if (bool.TryParse(settings.Element("XeniaVsync")?.Value, out var xv))
            {
                XeniaVsync = xv;
            }

            if (int.TryParse(settings.Element("XeniaResScaleX")?.Value, out var xrsx))
            {
                XeniaResScaleX = xrsx;
            }

            if (int.TryParse(settings.Element("XeniaResScaleY")?.Value, out var xrsy))
            {
                XeniaResScaleY = xrsy;
            }

            if (bool.TryParse(settings.Element("XeniaFullscreen")?.Value, out var xf))
            {
                XeniaFullscreen = xf;
            }

            XeniaApu = settings.Element("XeniaApu")?.Value ?? XeniaApu;
            if (bool.TryParse(settings.Element("XeniaMute")?.Value, out var xm))
            {
                XeniaMute = xm;
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
            if (bool.TryParse(settings.Element("XeniaShowSettingsBeforeLaunch")?.Value, out var xss))
            {
                XeniaShowSettingsBeforeLaunch = xss;
            }

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

            Save();
            File.Delete(_xmlFilePath);
            DebugLogger.Log("Migration successful. settings.xml deleted.");
            return true;
        }
        catch (Exception ex)
        {
            App.ServiceProvider?.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to migrate settings from XML.");
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
                App.ServiceProvider?.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving settings.dat");
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
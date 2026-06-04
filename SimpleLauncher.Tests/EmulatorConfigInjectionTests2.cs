using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Tomlyn;
using Tomlyn.Model;
using Xunit;

namespace SimpleLauncher.Tests;

public class EmulatorConfigInjectionTests2 : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();

    public EmulatorConfigInjectionTests2()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();

        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_EmuInjectionTest2_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Best-effort cleanup
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    private void CopySampleToEmuDir(string emulatorDirName, string sampleSubDir, string configFileName)
    {
        var emuDir = Path.Combine(_testDirectory, emulatorDirName);
        Directory.CreateDirectory(emuDir);

        var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", sampleSubDir, configFileName);
        var destPath = Path.Combine(emuDir, configFileName);
        File.Copy(samplePath, destPath);
    }

    private static string FakeEmulatorExePath(string emuDir)
    {
        return Path.Combine(emuDir, "emulator.exe");
    }

    private SettingsManager CreateSettingsManager()
    {
        return new SettingsManager(_configuration, _logErrors);
    }

    [Fact]
    public void AresInjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("Ares", "Ares", "settings.bml");

        var settings = CreateSettingsManager();
        settings.AresVideoDriver = "OpenGL 3.2";
        settings.AresExclusive = true;
        settings.AresShader = "CRT";
        settings.AresMultiplier = 4;
        settings.AresAspectCorrection = "Stretch";
        settings.AresMute = true;
        settings.AresVolume = 0.5;
        settings.AresFastBoot = true;
        settings.AresRewind = true;
        settings.AresRunAhead = false;
        settings.AresAutoSaveMemory = false;

        var emuDir = Path.Combine(_testDirectory, "Ares");
        AresConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "settings.bml");
        var content = File.ReadAllText(configPath);

        Assert.Contains("Driver: OpenGL 3.2", content);
        Assert.Contains("Exclusive: true", content);
        Assert.Contains("Shader: CRT", content);
        Assert.Contains("Multiplier: 4", content);
        Assert.Contains("Output: Stretch", content);
        Assert.Contains("Mute: true", content);
        Assert.Contains("Volume: 0.5", content);
        Assert.Contains("Fast: true", content);
        Assert.Contains("Rewind: true", content);
        Assert.Contains("RunAhead: false", content);
        Assert.Contains("AutoSaveMemory: false", content);
    }

    [Fact]
    public void AzaharInjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("Azahar", "Azahar", "qt-config.ini");

        var settings = CreateSettingsManager();
        settings.AzaharGraphicsApi = 1;
        settings.AzaharResolutionFactor = 2;
        settings.AzaharUseVsync = true;
        settings.AzaharAsyncShaderCompilation = false;
        settings.AzaharFullscreen = true;
        settings.AzaharVolume = 75;
        settings.AzaharEnableAudioStretching = true;
        settings.AzaharIsNew3Ds = false;
        settings.AzaharLayoutOption = 1;

        var emuDir = Path.Combine(_testDirectory, "Azahar");
        AzaharConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "qt-config.ini");
        var lines = File.ReadAllLines(configPath).ToList();
        var sectionValues = ParseIniSections(lines);

        Assert.Equal("1", sectionValues[("Renderer", "graphics_api")]);
        Assert.Equal("2", sectionValues[("Renderer", "resolution_factor")]);
        Assert.Equal("true", sectionValues[("Renderer", "use_vsync_new")]);
        Assert.Equal("false", sectionValues[("Renderer", "async_shader_compilation")]);
        Assert.Equal("true", sectionValues[("UI", "fullscreen")]);
        Assert.Equal("0.75", sectionValues[("Audio", "volume")]);
        Assert.Equal("true", sectionValues[("Audio", "enable_audio_stretching")]);
        Assert.Equal("false", sectionValues[("System", "is_new_3ds")]);
        Assert.Equal("1", sectionValues[("Layout", "layout_option")]);
    }

    [Fact]
    public void AzaharInjectsDefaultKeysAsFalse()
    {
        CopySampleToEmuDir("Azahar", "Azahar", "qt-config.ini");

        var settings = CreateSettingsManager();
        settings.AzaharFullscreen = true;

        var emuDir = Path.Combine(_testDirectory, "Azahar");
        AzaharConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "qt-config.ini");
        var lines = File.ReadAllLines(configPath).ToList();
        var sectionValues = ParseIniSections(lines);

        // The \default key should be set to "false" for injected settings
        Assert.Equal("false", sectionValues[("UI", "fullscreen\\default")]);
    }

    [Fact]
    public void CemuInjectsXmlSettingsCorrectly()
    {
        CopySampleToEmuDir("Cemu", "Cemu", "settings.xml");

        var settings = CreateSettingsManager();
        settings.CemuFullscreen = true;
        settings.CemuDiscordPresence = false;
        settings.CemuConsoleLanguage = 2;
        settings.CemuGraphicApi = 1;
        settings.CemuVsync = 1;
        settings.CemuAsyncCompile = true;
        settings.CemuTvVolume = 80;

        var emuDir = Path.Combine(_testDirectory, "Cemu");
        CemuConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "settings.xml");
        var doc = System.Xml.Linq.XDocument.Load(configPath);
        var content = doc.Element("content");

        Assert.NotNull(content);
        Assert.Equal("true", content.Element("fullscreen")?.Value);
        Assert.Equal("false", content.Element("use_discord_presence")?.Value);
        Assert.Equal("2", content.Element("console_language")?.Value);

        var graphic = content.Element("Graphic");
        Assert.NotNull(graphic);
        Assert.Equal("1", graphic.Element("api")?.Value);
        Assert.Equal("1", graphic.Element("VSync")?.Value);
        Assert.Equal("true", graphic.Element("AsyncCompile")?.Value);

        var audio = content.Element("Audio");
        Assert.NotNull(audio);
        Assert.Equal("80", audio.Element("TVVolume")?.Value);
    }

    [Fact]
    public void DolphinInjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("Dolphin", "Dolphin", "Dolphin.ini");

        var settings = CreateSettingsManager();
        settings.DolphinGfxBackend = "D3D12";
        settings.DolphinWiimoteContinuousScanning = false;
        settings.DolphinWiimoteEnableSpeaker = false;
        settings.DolphinDspThread = false;

        var emuDir = Path.Combine(_testDirectory, "Dolphin");

        // Create portable.txt so Dolphin uses the local config
        File.WriteAllText(Path.Combine(emuDir, "portable.txt"), "");

        DolphinConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "User", "Config", "Dolphin.ini");
        Assert.True(File.Exists(configPath), $"Config file should exist at {configPath}");

        var lines = File.ReadAllLines(configPath).ToList();
        var sectionValues = ParseIniSections(lines);

        Assert.Equal("D3D12", sectionValues[("Core", "GFXBackend")]);
        Assert.Equal("False", sectionValues[("Core", "WiimoteContinuousScanning")]);
        Assert.Equal("False", sectionValues[("Core", "WiimoteEnableSpeaker")]);
        Assert.Equal("False", sectionValues[("DSP", "DSPThread")]);
    }

    [Fact]
    public void FlycastInjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("Flycast", "Flycast", "emu.cfg");

        var settings = CreateSettingsManager();
        settings.FlycastFullscreen = true;
        settings.FlycastWidth = 1920;
        settings.FlycastHeight = 1080;
        settings.FlycastMaximized = true;

        var emuDir = Path.Combine(_testDirectory, "Flycast");
        FlycastConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "emu.cfg");
        var lines = File.ReadAllLines(configPath).ToList();
        var sectionValues = ParseIniSections(lines);

        Assert.Equal("yes", sectionValues[("window", "fullscreen")]);
        Assert.Equal("1920", sectionValues[("window", "width")]);
        Assert.Equal("1080", sectionValues[("window", "height")]);
        Assert.Equal("yes", sectionValues[("window", "maximized")]);
    }

    [Fact]
    public void FlycastDisabledOptions_UsesNoValues()
    {
        CopySampleToEmuDir("Flycast", "Flycast", "emu.cfg");

        var settings = CreateSettingsManager();
        settings.FlycastFullscreen = false;
        settings.FlycastMaximized = false;

        var emuDir = Path.Combine(_testDirectory, "Flycast");
        FlycastConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "emu.cfg");
        var lines = File.ReadAllLines(configPath).ToList();
        var sectionValues = ParseIniSections(lines);

        Assert.Equal("no", sectionValues[("window", "fullscreen")]);
        Assert.Equal("no", sectionValues[("window", "maximized")]);
    }

    [Fact]
    public void MameInjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("Mame", "MAME", "mame.ini");

        var settings = CreateSettingsManager();
        settings.MameVideo = "d3d";
        settings.MameWindow = false;
        settings.MameMaximize = true;
        settings.MameKeepAspect = true;
        settings.MameSkipGameInfo = true;
        settings.MameAutosave = true;
        settings.MameConfirmQuit = false;
        settings.MameJoystick = true;
        settings.MameAutoframeskip = false;
        settings.MameBgfxBackend = "vulkan";
        settings.MameBgfxScreenChains = "crt-geom";
        settings.MameFilter = true;
        settings.MameCheat = true;
        settings.MameRewind = true;
        settings.MameNvramSave = false;

        var emuDir = Path.Combine(_testDirectory, "Mame");
        MameConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "mame.ini");
        var content = File.ReadAllText(configPath);

        Assert.Contains("video", content);
        Assert.Contains("d3d", content);
    }

    [Fact]
    public void SupermodelInjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("Supermodel", "Supermodel", "Supermodel.ini");

        var settings = CreateSettingsManager();
        settings.SupermodelNew3DEngine = true;
        settings.SupermodelQuadRendering = true;
        settings.SupermodelFullscreen = true;
        settings.SupermodelResX = 1920;
        settings.SupermodelResY = 1080;
        settings.SupermodelWideScreen = true;
        settings.SupermodelStretch = false;
        settings.SupermodelVsync = true;
        settings.SupermodelThrottle = true;
        settings.SupermodelMusicVolume = 100;
        settings.SupermodelSoundVolume = 100;
        settings.SupermodelInputSystem = "dinput";
        settings.SupermodelMultiThreaded = true;
        settings.SupermodelPowerPcFrequency = 50;

        var emuDir = Path.Combine(_testDirectory, "Supermodel");
        SupermodelConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "Supermodel.ini");
        Assert.True(File.Exists(configPath), $"Config file should exist at {configPath}");

        var content = File.ReadAllText(configPath);

        Assert.Contains("New3DEngine = 1", content);
        Assert.Contains("QuadRendering = 1", content);
        Assert.Contains("FullScreen = 1", content);
        Assert.Contains("XResolution = 1920", content);
        Assert.Contains("YResolution = 1080", content);
        Assert.Contains("WideScreen = 1", content);
        Assert.Contains("Stretch = 0", content);
        Assert.Contains("VSync = 1", content);
        Assert.Contains("Throttle = 1", content);
        Assert.Contains("MusicVolume = 100", content);
        Assert.Contains("SoundVolume = 100", content);
        Assert.Contains("InputSystem = dinput", content);
        Assert.Contains("MultiThreaded = 1", content);
        Assert.Contains("PowerPCFrequency = 50", content);
    }

    [Fact]
    public void SupermodelInvalidInputSystem_DefaultsToXinput()
    {
        CopySampleToEmuDir("Supermodel", "Supermodel", "Supermodel.ini");

        var settings = CreateSettingsManager();
        settings.SupermodelInputSystem = "invalid_value";

        var emuDir = Path.Combine(_testDirectory, "Supermodel");
        SupermodelConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "Supermodel.ini");
        var content = File.ReadAllText(configPath);

        Assert.Contains("InputSystem = xinput", content);
    }

    [Fact]
    public void SegaModel2InjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("SegaModel2", "SEGA Model 2", "EMULATOR.INI");

        var settings = CreateSettingsManager();
        settings.SegaModel2ResX = 1920;
        settings.SegaModel2ResY = 1080;
        settings.SegaModel2WideScreen = 1;
        settings.SegaModel2Bilinear = true;
        settings.SegaModel2Trilinear = true;
        settings.SegaModel2FilterTilemaps = true;
        settings.SegaModel2DrawCross = false;
        settings.SegaModel2Fsaa = 4;
        settings.SegaModel2XInput = true;
        settings.SegaModel2EnableFf = true;
        settings.SegaModel2HoldGears = true;
        settings.SegaModel2UseRawInput = true;

        var emuDir = Path.Combine(_testDirectory, "SegaModel2");
        SegaModel2ConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "EMULATOR.INI");
        var lines = File.ReadAllLines(configPath).ToList();
        var sectionValues = ParseIniSections(lines);

        Assert.Equal("1920", sectionValues[("Renderer", "FullScreenWidth")]);
        Assert.Equal("1080", sectionValues[("Renderer", "FullScreenHeight")]);
        Assert.Equal("1", sectionValues[("Renderer", "WideScreenWindow")]);
        Assert.Equal("1", sectionValues[("Renderer", "Bilinear")]);
        Assert.Equal("1", sectionValues[("Renderer", "Trilinear")]);
        Assert.Equal("1", sectionValues[("Renderer", "FilterTilemaps")]);
        Assert.Equal("0", sectionValues[("Renderer", "DrawCross")]);
        Assert.Equal("4", sectionValues[("Renderer", "FSAA")]);
        Assert.Equal("1", sectionValues[("Input", "XInput")]);
        Assert.Equal("1", sectionValues[("Input", "EnableFF")]);
        Assert.Equal("1", sectionValues[("Input", "HoldGears")]);
        Assert.Equal("1", sectionValues[("Input", "UseRawInput")]);
    }

    [Fact]
    public void YumirInjectsTomlSettingsCorrectly()
    {
        CopySampleToEmuDir("Yumir", "Yumir", "Ymir.toml");

        var settings = CreateSettingsManager();
        settings.YumirFullscreen = true;
        settings.YumirForceAspectRatio = true;
        settings.YumirForcedAspect = 1.78;
        settings.YumirReduceLatency = true;
        settings.YumirVolume = 0.5;
        settings.YumirMute = true;
        settings.YumirVideoStandard = "NTSC";
        settings.YumirAutoDetectRegion = false;
        settings.YumirPauseWhenUnfocused = true;

        var emuDir = Path.Combine(_testDirectory, "Yumir");
        YumirConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "Ymir.toml");
        var tomlContent = File.ReadAllText(configPath);
        var model = TomlSerializer.Deserialize<TomlTable>(tomlContent) ?? new TomlTable();

        var video = (TomlTable)model["Video"];
        Assert.True((bool)video["FullScreen"]);
        Assert.True((bool)video["ForceAspectRatio"]);
        Assert.True((bool)video["ReduceLatency"]);

        var audio = (TomlTable)model["Audio"];
        Assert.True((bool)audio["Mute"]);

        var system = (TomlTable)model["System"];
        Assert.Equal("NTSC", (string)system["VideoStandard"]);
        Assert.False((bool)system["AutoDetectRegion"]);

        var general = (TomlTable)model["General"];
        Assert.True((bool)general["PauseWhenUnfocused"]);
    }

    [Fact]
    public void RaineInjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("Raine", "Raine", "raine32_sdl.cfg");

        var settings = CreateSettingsManager();
        settings.RaineFullscreen = true;
        settings.RaineResX = 1920;
        settings.RaineResY = 1080;
        settings.RaineFixAspectRatio = false;
        settings.RaineVsync = true;
        settings.RaineSoundDriver = "SDL";
        settings.RaineSampleRate = 48000;
        settings.RaineFrameSkip = 1;
        settings.RaineShowFps = true;

        var emuDir = Path.Combine(_testDirectory, "Raine");
        RaineConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors);

        var configPath = Path.Combine(emuDir, "config", "raine32_sdl.cfg");
        Assert.True(File.Exists(configPath), $"Config file should exist at {configPath}");

        var lines = File.ReadAllLines(configPath).ToList();
        var sectionValues = ParseIniSections(lines);

        Assert.Equal("1", sectionValues[("Display", "fullscreen")]);
        Assert.Equal("1920", sectionValues[("Display", "screen_x")]);
        Assert.Equal("1080", sectionValues[("Display", "screen_y")]);
        Assert.Equal("0", sectionValues[("Display", "fix_aspect_ratio")]);
        Assert.Equal("2", sectionValues[("Display", "ogl_dbuf")]);
        Assert.Equal("SDL", sectionValues[("Sound", "driver")]);
        Assert.Equal("48000", sectionValues[("Sound", "sample_rate")]);
        Assert.Equal("1", sectionValues[("General", "frame_skip")]);
        Assert.Equal("1", sectionValues[("General", "ShowFPS")]);
    }

    // --- Helpers ---

    private static Dictionary<(string Section, string Key), string> ParseIniSections(List<string> lines)
    {
        var result = new Dictionary<(string, string), string>();
        var currentSection = "";

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
            {
                currentSection = trimmed[1..^1];
                continue;
            }

            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith(';') || trimmed.StartsWith('#'))
                continue;

            var parts = trimmed.Split('=', 2);
            if (parts.Length == 2)
            {
                result[(currentSection, parts[0].Trim())] = parts[1].Trim();
            }
        }

        return result;
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}

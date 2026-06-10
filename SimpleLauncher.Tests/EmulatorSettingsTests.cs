using System.Xml.Linq;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.SettingsManager.EmulatorSettings;
using Xunit;

namespace SimpleLauncher.Tests;

public class EmulatorSettingsTests
{
    // AresSettings Tests
    [Fact]
    public void AresSettingsDefaults()
    {
        var s = new AresSettings();
        Assert.Equal("OpenGL 3.2", s.VideoDriver);
        Assert.False(s.Exclusive);
        Assert.Equal("None", s.Shader);
        Assert.Equal(2, s.Multiplier);
        Assert.Equal("Standard", s.AspectCorrection);
        Assert.False(s.Mute);
        Assert.Equal(1.0, s.Volume);
        Assert.False(s.FastBoot);
        Assert.False(s.Rewind);
        Assert.False(s.RunAhead);
        Assert.True(s.AutoSaveMemory);
        Assert.False(s.ShowSettingsBeforeLaunch);
    }

    [Fact]
    public void AresSettingsToXElementRoundTrip()
    {
        var original = new AresSettings { VideoDriver = "Vulkan", Multiplier = 4, Volume = 0.5 };
        var element = original.ToXElement();
        var loaded = new AresSettings();
        loaded.LoadFromXml(new XElement("Settings", element));

        Assert.Equal("Vulkan", loaded.VideoDriver);
        Assert.Equal(4, loaded.Multiplier);
        Assert.Equal(0.5, loaded.Volume);
    }

    [Fact]
    public void AresSettingsCopyFrom()
    {
        var source = new AresSettings { VideoDriver = "Vulkan", Mute = true };
        var target = new AresSettings();
        target.CopyFrom(source);

        Assert.Equal("Vulkan", target.VideoDriver);
        Assert.True(target.Mute);
    }

    [Fact]
    public void AresSettingsCopyFromWrongTypeDoesNothing()
    {
        var source = new DuckStationSettings();
        var target = new AresSettings { VideoDriver = "Original" };
        target.CopyFrom(source);
        Assert.Equal("Original", target.VideoDriver);
    }

    [Fact]
    public void AresSettingsResetDefaults()
    {
        var s = new AresSettings { VideoDriver = "Custom", Mute = true };
        s.ResetDefaults();
        Assert.Equal("OpenGL 3.2", s.VideoDriver);
        Assert.False(s.Mute);
    }

    // DuckStationSettings Tests
    [Fact]
    public void DuckStationSettingsDefaults()
    {
        var s = new DuckStationSettings();
        Assert.False(s.StartFullscreen);
        Assert.True(s.PauseOnFocusLoss);
        Assert.True(s.SaveStateOnExit);
        Assert.Equal("Automatic", s.Renderer);
        Assert.Equal(2, s.ResolutionScale);
        Assert.Equal("Nearest", s.TextureFilter);
        Assert.Equal("16:9", s.AspectRatio);
        Assert.Equal(100, s.OutputVolume);
    }

    [Fact]
    public void DuckStationSettingsToXElementRoundTrip()
    {
        var original = new DuckStationSettings { Renderer = "Vulkan", ResolutionScale = 4 };
        var element = original.ToXElement();
        var loaded = new DuckStationSettings();
        loaded.LoadFromXml(new XElement("Settings", element));

        Assert.Equal("Vulkan", loaded.Renderer);
        Assert.Equal(4, loaded.ResolutionScale);
    }

    [Fact]
    public void DuckStationSettingsResetDefaults()
    {
        var s = new DuckStationSettings { Renderer = "Custom", StartFullscreen = true };
        s.ResetDefaults();
        Assert.Equal("Automatic", s.Renderer);
        Assert.False(s.StartFullscreen);
    }

    // RetroArchSettings Tests
    [Fact]
    public void RetroArchSettingsDefaults()
    {
        var s = new RetroArchSettings();
        Assert.False(s.Fullscreen);
        Assert.Equal("gl", s.VideoDriver);
        Assert.True(s.Vsync);
        Assert.True(s.AudioEnable);
        Assert.Equal("ozone", s.MenuDriver);
        Assert.True(s.ShowAdvancedSettings);
    }

    [Fact]
    public void RetroArchSettingsToXElementRoundTrip()
    {
        var original = new RetroArchSettings { VideoDriver = "vulkan", MenuDriver = "xmb" };
        var element = original.ToXElement();
        var loaded = new RetroArchSettings();
        loaded.LoadFromXml(new XElement("Settings", element));

        Assert.Equal("vulkan", loaded.VideoDriver);
        Assert.Equal("xmb", loaded.MenuDriver);
    }

    // Pcsx2Settings Tests
    [Fact]
    public void Pcsx2SettingsDefaults()
    {
        var s = new Pcsx2Settings();
        Assert.True(s.StartFullscreen);
        Assert.Equal(14, s.Renderer);
        Assert.Equal(2, s.UpscaleMultiplier);
    }

    [Fact]
    public void Pcsx2SettingsResetDefaults()
    {
        var s = new Pcsx2Settings { StartFullscreen = false, Renderer = 0 };
        s.ResetDefaults();
        Assert.True(s.StartFullscreen);
        Assert.Equal(14, s.Renderer);
    }

    // DolphinSettings Tests
    [Fact]
    public void DolphinSettingsDefaults()
    {
        var s = new DolphinSettings();
        Assert.Equal("Vulkan", s.GfxBackend);
        Assert.True(s.DspThread);
    }

    [Fact]
    public void DolphinSettingsToXElementRoundTrip()
    {
        var original = new DolphinSettings { GfxBackend = "D3D12" };
        var element = original.ToXElement();
        var loaded = new DolphinSettings();
        loaded.LoadFromXml(new XElement("Settings", element));
        Assert.Equal("D3D12", loaded.GfxBackend);
    }

    // FlycastSettings Tests
    [Fact]
    public void FlycastSettingsDefaults()
    {
        var s = new FlycastSettings();
        Assert.Equal(640, s.Width);
        Assert.Equal(480, s.Height);
    }

    // MameSettings Tests
    [Fact]
    public void MameSettingsDefaults()
    {
        var s = new MameSettings();
        Assert.Equal("auto", s.Video);
        Assert.True(s.Maximize);
        Assert.True(s.Joystick);
    }

    // MednafenSettings Tests
    [Fact]
    public void MednafenSettingsDefaults()
    {
        var s = new MednafenSettings();
        Assert.Equal("opengl", s.VideoDriver);
        Assert.Equal(100, s.Volume);
    }

    // MesenSettings Tests
    [Fact]
    public void MesenSettingsDefaults()
    {
        var s = new MesenSettings();
        Assert.Equal("NoStretching", s.AspectRatio);
        Assert.Equal(100, s.MasterVolume);
    }

    // RaineSettings Tests
    [Fact]
    public void RaineSettingsDefaults()
    {
        var s = new RaineSettings();
        Assert.Equal(640, s.ResX);
        Assert.Equal(44100, s.SampleRate);
        Assert.Equal(60, s.MusicVolume);
    }

    // RedreamSettings Tests
    [Fact]
    public void RedreamSettingsDefaults()
    {
        var s = new RedreamSettings();
        Assert.Equal("vga", s.Cable);
        Assert.Equal("usa", s.Region);
        Assert.Equal(2, s.Res);
    }

    // Rpcs3Settings Tests
    [Fact]
    public void Rpcs3SettingsDefaults()
    {
        var s = new Rpcs3Settings();
        Assert.Equal("Vulkan", s.Renderer);
        Assert.Equal("Recompiler (LLVM)", s.PpuDecoder);
    }

    // BlastemSettings Tests
    [Fact]
    public void BlastemSettingsDefaults()
    {
        var s = new BlastemSettings();
        Assert.Equal("4:3", s.Aspect);
        Assert.Equal(48000, s.AudioRate);
    }

    // AzaharSettings Tests
    [Fact]
    public void AzaharSettingsDefaults()
    {
        var s = new AzaharSettings();
        Assert.Equal(1, s.GraphicsApi);
        Assert.Equal(100, s.Volume);
        Assert.True(s.IsNew3Ds);
    }

    // CemuSettings Tests
    [Fact]
    public void CemuSettingsDefaults()
    {
        var s = new CemuSettings();
        Assert.Equal(1, s.GraphicApi);
        Assert.Equal(50, s.TvVolume);
    }

    // DaphneSettings Tests
    [Fact]
    public void DaphneSettingsDefaults()
    {
        var s = new DaphneSettings();
        Assert.Equal(640, s.ResX);
        Assert.Equal(480, s.ResY);
        Assert.True(s.Bilinear);
    }

    // SegaModel2Settings Tests
    [Fact]
    public void SegaModel2SettingsDefaults()
    {
        var s = new SegaModel2Settings();
        Assert.Equal(640, s.ResX);
        Assert.True(s.Bilinear);
    }

    // StellaSettings Tests
    [Fact]
    public void StellaSettingsDefaults()
    {
        var s = new StellaSettings();
        Assert.Equal("direct3d", s.VideoDriver);
        Assert.Equal(80, s.AudioVolume);
    }

    // SupermodelSettings Tests
    [Fact]
    public void SupermodelSettingsDefaults()
    {
        var s = new SupermodelSettings();
        Assert.True(s.Fullscreen);
        Assert.Equal(1920, s.ResX);
        Assert.Equal("xinput", s.InputSystem);
    }

    // XeniaSettings Tests
    [Fact]
    public void XeniaSettingsDefaults()
    {
        var s = new XeniaSettings();
        Assert.Equal("d3d12", s.Gpu);
        Assert.Equal("xinput", s.Hid);
    }

    // YumirSettings Tests
    [Fact]
    public void YumirSettingsDefaults()
    {
        var s = new YumirSettings();
        Assert.Equal(0.8, s.Volume);
        Assert.Equal("PAL", s.VideoStandard);
    }

    // Cross-cutting tests for all emulator settings

    [Fact]
    public void AllEmulatorSettingsImplementIEmulatorSettings()
    {
        IEmulatorSettings[] allSettings =
        [
            new AresSettings(),
            new AzaharSettings(),
            new BlastemSettings(),
            new CemuSettings(),
            new DaphneSettings(),
            new DolphinSettings(),
            new DuckStationSettings(),
            new FlycastSettings(),
            new MameSettings(),
            new MednafenSettings(),
            new MesenSettings(),
            new Pcsx2Settings(),
            new RaineSettings(),
            new RedreamSettings(),
            new RetroArchSettings(),
            new Rpcs3Settings(),
            new SegaModel2Settings(),
            new StellaSettings(),
            new SupermodelSettings(),
            new XeniaSettings(),
            new YumirSettings()
        ];

        foreach (var settings in allSettings)
        {
            Assert.NotNull(settings);
        }
    }

    [Fact]
    public void AllEmulatorSettingsToXElementReturnsNonNull()
    {
        IEmulatorSettings[] allSettings =
        [
            new AresSettings(),
            new AzaharSettings(),
            new BlastemSettings(),
            new CemuSettings(),
            new DaphneSettings(),
            new DolphinSettings(),
            new DuckStationSettings(),
            new FlycastSettings(),
            new MameSettings(),
            new MednafenSettings(),
            new MesenSettings(),
            new Pcsx2Settings(),
            new RaineSettings(),
            new RedreamSettings(),
            new RetroArchSettings(),
            new Rpcs3Settings(),
            new SegaModel2Settings(),
            new StellaSettings(),
            new SupermodelSettings(),
            new XeniaSettings(),
            new YumirSettings()
        ];

        foreach (var settings in allSettings)
        {
            var element = settings.ToXElement();
            Assert.NotNull(element);
        }
    }

    [Fact]
    public void AllEmulatorSettingsResetDefaultsDoesNotThrow()
    {
        IEmulatorSettings[] allSettings =
        [
            new AresSettings(),
            new AzaharSettings(),
            new BlastemSettings(),
            new CemuSettings(),
            new DaphneSettings(),
            new DolphinSettings(),
            new DuckStationSettings(),
            new FlycastSettings(),
            new MameSettings(),
            new MednafenSettings(),
            new MesenSettings(),
            new Pcsx2Settings(),
            new RaineSettings(),
            new RedreamSettings(),
            new RetroArchSettings(),
            new Rpcs3Settings(),
            new SegaModel2Settings(),
            new StellaSettings(),
            new SupermodelSettings(),
            new XeniaSettings(),
            new YumirSettings()
        ];

        foreach (var settings in allSettings)
        {
            // Modify then reset
            settings.ToXElement();
            settings.ResetDefaults();
            var elementAfterReset = settings.ToXElement();
            Assert.NotNull(elementAfterReset);
        }
    }

    [Fact]
    public void AllEmulatorSettingsCopyFromSelfDoesNotThrow()
    {
        IEmulatorSettings[] allSettings =
        [
            new AresSettings(),
            new AzaharSettings(),
            new BlastemSettings(),
            new CemuSettings(),
            new DaphneSettings(),
            new DolphinSettings(),
            new DuckStationSettings(),
            new FlycastSettings(),
            new MameSettings(),
            new MednafenSettings(),
            new MesenSettings(),
            new Pcsx2Settings(),
            new RaineSettings(),
            new RedreamSettings(),
            new RetroArchSettings(),
            new Rpcs3Settings(),
            new SegaModel2Settings(),
            new StellaSettings(),
            new SupermodelSettings(),
            new XeniaSettings(),
            new YumirSettings()
        ];

        foreach (var settings in allSettings)
        {
            var exception = Record.Exception(() => settings.CopyFrom(settings));
            Assert.Null(exception);
        }
    }

    [Fact]
    public void AllEmulatorSettingsLoadFromXmlWithEmptyXmlDoesNotThrow()
    {
        var emptyXml = new XElement("Settings");

        IEmulatorSettings[] allSettings =
        [
            new AresSettings(),
            new AzaharSettings(),
            new BlastemSettings(),
            new CemuSettings(),
            new DaphneSettings(),
            new DolphinSettings(),
            new DuckStationSettings(),
            new FlycastSettings(),
            new MameSettings(),
            new MednafenSettings(),
            new MesenSettings(),
            new Pcsx2Settings(),
            new RaineSettings(),
            new RedreamSettings(),
            new RetroArchSettings(),
            new Rpcs3Settings(),
            new SegaModel2Settings(),
            new StellaSettings(),
            new SupermodelSettings(),
            new XeniaSettings(),
            new YumirSettings()
        ];

        foreach (var settings in allSettings)
        {
            var exception = Record.Exception(() => settings.LoadFromXml(emptyXml));
            Assert.Null(exception);
        }
    }
}

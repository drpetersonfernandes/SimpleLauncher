using System.Xml.Linq;
using SimpleLauncher.Services.SettingsManager.EmulatorSettings;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Tests for individual emulator settings classes covering defaults, XML round-trip, copy, and reset behavior.
/// </summary>
public class EmulatorSettingsTests
{
    /// <summary>
    /// Verifies that AresSettings properties have the expected default values.
    /// </summary>
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

    /// <summary>
    /// Verifies that AresSettings can be serialized to XElement and deserialized back correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that AresSettings.CopyFrom copies all properties from a source instance.
    /// </summary>
    [Fact]
    public void AresSettingsCopyFrom()
    {
        var source = new AresSettings { VideoDriver = "Vulkan", Mute = true };
        var target = new AresSettings();
        target.CopyFrom(source);

        Assert.Equal("Vulkan", target.VideoDriver);
        Assert.True(target.Mute);
    }

    /// <summary>
    /// Verifies that AresSettings.CopyFrom with a wrong type does not modify the target.
    /// </summary>
    [Fact]
    public void AresSettingsCopyFromWrongTypeDoesNothing()
    {
        var source = new DuckStationSettings();
        var target = new AresSettings { VideoDriver = "Original" };
        target.CopyFrom(source);
        Assert.Equal("Original", target.VideoDriver);
    }

    /// <summary>
    /// Verifies that AresSettings.ResetDefaults restores all properties to their default values.
    /// </summary>
    [Fact]
    public void AresSettingsResetDefaults()
    {
        var s = new AresSettings { VideoDriver = "Custom", Mute = true };
        s.ResetDefaults();
        Assert.Equal("OpenGL 3.2", s.VideoDriver);
        Assert.False(s.Mute);
    }

    /// <summary>
    /// Verifies that DuckStationSettings properties have the expected default values.
    /// </summary>
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

    /// <summary>
    /// Verifies that DuckStationSettings can be serialized to XElement and deserialized back correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that DuckStationSettings.ResetDefaults restores all properties to their default values.
    /// </summary>
    [Fact]
    public void DuckStationSettingsResetDefaults()
    {
        var s = new DuckStationSettings { Renderer = "Custom", StartFullscreen = true };
        s.ResetDefaults();
        Assert.Equal("Automatic", s.Renderer);
        Assert.False(s.StartFullscreen);
    }

    /// <summary>
    /// Verifies that RetroArchSettings properties have the expected default values.
    /// </summary>
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

    /// <summary>
    /// Verifies that RetroArchSettings can be serialized to XElement and deserialized back correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that Pcsx2Settings properties have the expected default values.
    /// </summary>
    [Fact]
    public void Pcsx2SettingsDefaults()
    {
        var s = new Pcsx2Settings();
        Assert.True(s.StartFullscreen);
        Assert.Equal(14, s.Renderer);
        Assert.Equal(2, s.UpscaleMultiplier);
    }

    /// <summary>
    /// Verifies that Pcsx2Settings.ResetDefaults restores all properties to their default values.
    /// </summary>
    [Fact]
    public void Pcsx2SettingsResetDefaults()
    {
        var s = new Pcsx2Settings { StartFullscreen = false, Renderer = 0 };
        s.ResetDefaults();
        Assert.True(s.StartFullscreen);
        Assert.Equal(14, s.Renderer);
    }

    /// <summary>
    /// Verifies that DolphinSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void DolphinSettingsDefaults()
    {
        var s = new DolphinSettings();
        Assert.Equal("Vulkan", s.GfxBackend);
        Assert.True(s.DspThread);
    }

    /// <summary>
    /// Verifies that DolphinSettings can be serialized to XElement and deserialized back correctly.
    /// </summary>
    [Fact]
    public void DolphinSettingsToXElementRoundTrip()
    {
        var original = new DolphinSettings { GfxBackend = "D3D12" };
        var element = original.ToXElement();
        var loaded = new DolphinSettings();
        loaded.LoadFromXml(new XElement("Settings", element));
        Assert.Equal("D3D12", loaded.GfxBackend);
    }

    /// <summary>
    /// Verifies that FlycastSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void FlycastSettingsDefaults()
    {
        var s = new FlycastSettings();
        Assert.Equal(640, s.Width);
        Assert.Equal(480, s.Height);
    }

    /// <summary>
    /// Verifies that MameSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void MameSettingsDefaults()
    {
        var s = new MameSettings();
        Assert.Equal("auto", s.Video);
        Assert.True(s.Maximize);
        Assert.True(s.Joystick);
    }

    /// <summary>
    /// Verifies that MednafenSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void MednafenSettingsDefaults()
    {
        var s = new MednafenSettings();
        Assert.Equal("opengl", s.VideoDriver);
        Assert.Equal(100, s.Volume);
    }

    /// <summary>
    /// Verifies that MesenSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void MesenSettingsDefaults()
    {
        var s = new MesenSettings();
        Assert.Equal("NoStretching", s.AspectRatio);
        Assert.Equal(100, s.MasterVolume);
    }

    /// <summary>
    /// Verifies that RaineSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void RaineSettingsDefaults()
    {
        var s = new RaineSettings();
        Assert.Equal(640, s.ResX);
        Assert.Equal(44100, s.SampleRate);
        Assert.Equal(60, s.MusicVolume);
    }

    /// <summary>
    /// Verifies that RedreamSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void RedreamSettingsDefaults()
    {
        var s = new RedreamSettings();
        Assert.Equal("vga", s.Cable);
        Assert.Equal("usa", s.Region);
        Assert.Equal(2, s.Res);
    }

    /// <summary>
    /// Verifies that Rpcs3Settings properties have the expected default values.
    /// </summary>
    [Fact]
    public void Rpcs3SettingsDefaults()
    {
        var s = new Rpcs3Settings();
        Assert.Equal("Vulkan", s.Renderer);
        Assert.Equal("Recompiler (LLVM)", s.PpuDecoder);
    }

    /// <summary>
    /// Verifies that BlastemSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void BlastemSettingsDefaults()
    {
        var s = new BlastemSettings();
        Assert.Equal("4:3", s.Aspect);
        Assert.Equal(48000, s.AudioRate);
    }

    /// <summary>
    /// Verifies that AzaharSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void AzaharSettingsDefaults()
    {
        var s = new AzaharSettings();
        Assert.Equal(1, s.GraphicsApi);
        Assert.Equal(100, s.Volume);
        Assert.True(s.IsNew3Ds);
    }

    /// <summary>
    /// Verifies that CemuSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void CemuSettingsDefaults()
    {
        var s = new CemuSettings();
        Assert.Equal(1, s.GraphicApi);
        Assert.Equal(50, s.TvVolume);
    }

    /// <summary>
    /// Verifies that DaphneSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void DaphneSettingsDefaults()
    {
        var s = new DaphneSettings();
        Assert.Equal(640, s.ResX);
        Assert.Equal(480, s.ResY);
        Assert.True(s.Bilinear);
    }

    /// <summary>
    /// Verifies that SegaModel2Settings properties have the expected default values.
    /// </summary>
    [Fact]
    public void SegaModel2SettingsDefaults()
    {
        var s = new SegaModel2Settings();
        Assert.Equal(640, s.ResX);
        Assert.True(s.Bilinear);
    }

    /// <summary>
    /// Verifies that StellaSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void StellaSettingsDefaults()
    {
        var s = new StellaSettings();
        Assert.Equal("direct3d", s.VideoDriver);
        Assert.Equal(80, s.AudioVolume);
    }

    /// <summary>
    /// Verifies that SupermodelSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void SupermodelSettingsDefaults()
    {
        var s = new SupermodelSettings();
        Assert.True(s.Fullscreen);
        Assert.Equal(1920, s.ResX);
        Assert.Equal("xinput", s.InputSystem);
    }

    /// <summary>
    /// Verifies that XeniaSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void XeniaSettingsDefaults()
    {
        var s = new XeniaSettings();
        Assert.Equal("d3d12", s.Gpu);
        Assert.Equal("xinput", s.Hid);
    }

    /// <summary>
    /// Verifies that YumirSettings properties have the expected default values.
    /// </summary>
    [Fact]
    public void YumirSettingsDefaults()
    {
        var s = new YumirSettings();
        Assert.Equal(0.8, s.Volume);
        Assert.Equal("PAL", s.VideoStandard);
    }

    // Cross-cutting tests for all emulator settings

    /// <summary>
    /// Verifies that all emulator settings classes implement the IEmulatorSettings interface.
    /// </summary>
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

    /// <summary>
    /// Verifies that ToXElement returns a non-null element for all emulator settings classes.
    /// </summary>
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

    /// <summary>
    /// Verifies that ResetDefaults does not throw for any emulator settings class.
    /// </summary>
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

    /// <summary>
    /// Verifies that CopyFrom with self does not throw for any emulator settings class.
    /// </summary>
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

    /// <summary>
    /// Verifies that LoadFromXml with an empty XML element does not throw for any emulator settings class.
    /// </summary>
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

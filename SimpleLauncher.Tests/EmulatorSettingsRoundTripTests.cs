using System.Xml.Linq;
using SimpleLauncher.Services.SettingsManager.EmulatorSettings;
using Xunit;

namespace SimpleLauncher.Tests;

public class EmulatorSettingsRoundTripTests
{
    [Fact]
    public void AresSettingsFullRoundTrip()
    {
        var original = new AresSettings
        {
            VideoDriver = "Vulkan",
            Exclusive = true,
            Shader = "CRT",
            Multiplier = 4,
            AspectCorrection = "Widescreen",
            Mute = true,
            Volume = 0.75,
            FastBoot = true,
            Rewind = true,
            RunAhead = true,
            AutoSaveMemory = false,
            ShowSettingsBeforeLaunch = true
        };

        var element = original.ToXElement();
        var loaded = new AresSettings();
        loaded.LoadFromXml(new XElement("Settings", element));

        Assert.Equal(original.VideoDriver, loaded.VideoDriver);
        Assert.Equal(original.Exclusive, loaded.Exclusive);
        Assert.Equal(original.Shader, loaded.Shader);
        Assert.Equal(original.Multiplier, loaded.Multiplier);
        Assert.Equal(original.AspectCorrection, loaded.AspectCorrection);
        Assert.Equal(original.Mute, loaded.Mute);
        Assert.Equal(original.Volume, loaded.Volume);
        Assert.Equal(original.FastBoot, loaded.FastBoot);
        Assert.Equal(original.Rewind, loaded.Rewind);
        Assert.Equal(original.RunAhead, loaded.RunAhead);
        Assert.Equal(original.AutoSaveMemory, loaded.AutoSaveMemory);
        Assert.Equal(original.ShowSettingsBeforeLaunch, loaded.ShowSettingsBeforeLaunch);
    }

    [Fact]
    public void DuckStationSettingsFullRoundTrip()
    {
        var original = new DuckStationSettings
        {
            StartFullscreen = true,
            PauseOnFocusLoss = false,
            SaveStateOnExit = false,
            RewindEnable = true,
            RunaheadFrameCount = 3,
            Renderer = "Vulkan",
            ResolutionScale = 4,
            TextureFilter = "Bilinear",
            WidescreenHack = true,
            PgxpEnable = true,
            AspectRatio = "4:3",
            Vsync = true,
            OutputVolume = 50,
            OutputMuted = true,
            ShowSettingsBeforeLaunch = true
        };

        var element = original.ToXElement();
        var loaded = new DuckStationSettings();
        loaded.LoadFromXml(new XElement("Settings", element));

        Assert.Equal(original.StartFullscreen, loaded.StartFullscreen);
        Assert.Equal(original.PauseOnFocusLoss, loaded.PauseOnFocusLoss);
        Assert.Equal(original.SaveStateOnExit, loaded.SaveStateOnExit);
        Assert.Equal(original.RewindEnable, loaded.RewindEnable);
        Assert.Equal(original.RunaheadFrameCount, loaded.RunaheadFrameCount);
        Assert.Equal(original.Renderer, loaded.Renderer);
        Assert.Equal(original.ResolutionScale, loaded.ResolutionScale);
        Assert.Equal(original.TextureFilter, loaded.TextureFilter);
        Assert.Equal(original.WidescreenHack, loaded.WidescreenHack);
        Assert.Equal(original.PgxpEnable, loaded.PgxpEnable);
        Assert.Equal(original.AspectRatio, loaded.AspectRatio);
        Assert.Equal(original.Vsync, loaded.Vsync);
        Assert.Equal(original.OutputVolume, loaded.OutputVolume);
        Assert.Equal(original.OutputMuted, loaded.OutputMuted);
        Assert.Equal(original.ShowSettingsBeforeLaunch, loaded.ShowSettingsBeforeLaunch);
    }

    [Fact]
    public void RetroArchSettingsFullRoundTrip()
    {
        var original = new RetroArchSettings
        {
            Fullscreen = true,
            VideoDriver = "vulkan",
            Vsync = false,
            AudioEnable = false,
            MenuDriver = "xmb",
            ShowAdvancedSettings = false
        };

        var element = original.ToXElement();
        var loaded = new RetroArchSettings();
        loaded.LoadFromXml(new XElement("Settings", element));

        Assert.Equal(original.Fullscreen, loaded.Fullscreen);
        Assert.Equal(original.VideoDriver, loaded.VideoDriver);
        Assert.Equal(original.Vsync, loaded.Vsync);
        Assert.Equal(original.AudioEnable, loaded.AudioEnable);
        Assert.Equal(original.MenuDriver, loaded.MenuDriver);
        Assert.Equal(original.ShowAdvancedSettings, loaded.ShowAdvancedSettings);
    }

    [Fact]
    public void Pcsx2SettingsFullRoundTrip()
    {
        var original = new Pcsx2Settings
        {
            StartFullscreen = false,
            Renderer = 12,
            UpscaleMultiplier = 4
        };

        var element = original.ToXElement();
        var loaded = new Pcsx2Settings();
        loaded.LoadFromXml(new XElement("Settings", element));

        Assert.Equal(original.StartFullscreen, loaded.StartFullscreen);
        Assert.Equal(original.Renderer, loaded.Renderer);
        Assert.Equal(original.UpscaleMultiplier, loaded.UpscaleMultiplier);
    }

    [Fact]
    public void DolphinSettingsFullRoundTrip()
    {
        var original = new DolphinSettings
        {
            GfxBackend = "D3D12",
            DspThread = false
        };

        var element = original.ToXElement();
        var loaded = new DolphinSettings();
        loaded.LoadFromXml(new XElement("Settings", element));

        Assert.Equal(original.GfxBackend, loaded.GfxBackend);
        Assert.Equal(original.DspThread, loaded.DspThread);
    }

    [Fact]
    public void AllSettingsResetDefaultsRestoresToConstructorDefaults()
    {
        // Test that ResetDefaults produces the same values as a new instance
        var ares = new AresSettings { VideoDriver = "Modified" };
        ares.ResetDefaults();
        var fresh = new AresSettings();
        Assert.Equal(fresh.VideoDriver, ares.VideoDriver);
        Assert.Equal(fresh.Volume, ares.Volume);

        var duck = new DuckStationSettings { Renderer = "Modified" };
        duck.ResetDefaults();
        var freshDuck = new DuckStationSettings();
        Assert.Equal(freshDuck.Renderer, duck.Renderer);
        Assert.Equal(freshDuck.ResolutionScale, duck.ResolutionScale);

        var ra = new RetroArchSettings { VideoDriver = "Modified" };
        ra.ResetDefaults();
        var freshRa = new RetroArchSettings();
        Assert.Equal(freshRa.VideoDriver, ra.VideoDriver);
    }

    [Fact]
    public void LoadFromXmlWithMissingElementsKeepsDefaults()
    {
        var emptyXml = new XElement("Settings");
        var ares = new AresSettings();
        var originalDriver = ares.VideoDriver;

        ares.LoadFromXml(emptyXml);
        Assert.Equal(originalDriver, ares.VideoDriver);
    }

    [Fact]
    public void CopyFromPreservesAllProperties()
    {
        var source = new DuckStationSettings
        {
            StartFullscreen = true,
            Renderer = "OpenGL",
            ResolutionScale = 8,
            OutputVolume = 25
        };

        var target = new DuckStationSettings();
        target.CopyFrom(source);

        Assert.Equal(source.StartFullscreen, target.StartFullscreen);
        Assert.Equal(source.Renderer, target.Renderer);
        Assert.Equal(source.ResolutionScale, target.ResolutionScale);
        Assert.Equal(source.OutputVolume, target.OutputVolume);
    }
}

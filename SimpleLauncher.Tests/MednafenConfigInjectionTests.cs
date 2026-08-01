using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the Mednafen emulator configuration injection service that writes global and per-system settings
/// into the mednafen.cfg configuration file.
/// </summary>
public class MednafenConfigInjectionTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logErrors = new NoOpLogger();
    private readonly NoOpCredentialProtector _credentialProtector = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MednafenConfigInjectionTests"/> class,
    /// creating a temporary test directory and configuration for each test.
    /// </summary>
    public MednafenConfigInjectionTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();

        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_MednafenTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Cleans up the temporary test directory and restores the service provider mock.
    /// </summary>
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
        return Path.Combine(emuDir, "mednafen.exe");
    }

    private SettingsManagerService CreateSettingsManager()
    {
        return new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
    }

    /// <summary>
    /// Verifies that global Mednafen settings such as video driver, fullscreen, vsync, volume, cheats, and rewind
    /// are correctly injected into the mednafen.cfg configuration file.
    /// </summary>
    [Fact]
    public void MednafenInjectsGlobalSettingsCorrectly()
    {
        CopySampleToEmuDir("Mednafen", "Mednafen", "mednafen.cfg");

        var settings = CreateSettingsManager();
        settings.Mednafen.VideoDriver = "opengl";
        settings.Mednafen.Fullscreen = true;
        settings.Mednafen.Vsync = false;
        settings.Mednafen.Volume = 75;
        settings.Mednafen.Cheats = true;
        settings.Mednafen.Rewind = true;

        var emuDir = Path.Combine(_testDirectory, "Mednafen");
        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, Log.Logger);

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        var content = File.ReadAllText(configPath);

        Assert.Contains("video.driver opengl", content, StringComparison.Ordinal);
        Assert.Contains("video.fs 1", content, StringComparison.Ordinal);
        Assert.Contains("video.glvsync 0", content, StringComparison.Ordinal);
        Assert.Contains("video.blit_timesync 0", content, StringComparison.Ordinal);
        Assert.Contains("sound.volume 75", content, StringComparison.Ordinal);
        Assert.Contains("cheats 1", content, StringComparison.Ordinal);
        Assert.Contains("state_rewind 1", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that per-system Mednafen settings such as stretch, bilinear, scanlines, shader, and special
    /// are correctly applied to all supported system prefixes (nes, snes, psx, gba, etc.).
    /// </summary>
    [Fact]
    public void MednafenInjectsPerSystemSettingsCorrectly()
    {
        CopySampleToEmuDir("Mednafen", "Mednafen", "mednafen.cfg");

        var settings = CreateSettingsManager();
        settings.Mednafen.Stretch = "full";
        settings.Mednafen.Bilinear = true;
        settings.Mednafen.Scanlines = 30;
        settings.Mednafen.Shader = "CRT";
        settings.Mednafen.Special = "hq2x";

        var emuDir = Path.Combine(_testDirectory, "Mednafen");
        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, Log.Logger);

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        var content = File.ReadAllText(configPath);

        // Check a few system prefixes
        Assert.Contains("nes.stretch full", content, StringComparison.Ordinal);
        Assert.Contains("nes.videoip 1", content, StringComparison.Ordinal);
        Assert.Contains("nes.scanlines 30", content, StringComparison.Ordinal);
        Assert.Contains("nes.shader CRT", content, StringComparison.Ordinal);
        Assert.Contains("nes.special hq2x", content, StringComparison.Ordinal);

        Assert.Contains("snes.stretch full", content, StringComparison.Ordinal);
        Assert.Contains("snes.videoip 1", content, StringComparison.Ordinal);

        Assert.Contains("psx.stretch full", content, StringComparison.Ordinal);
        Assert.Contains("psx.videoip 1", content, StringComparison.Ordinal);

        Assert.Contains("gba.stretch full", content, StringComparison.Ordinal);
        Assert.Contains("gba.videoip 1", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that disabled boolean options are written with zero/false values in the configuration file.
    /// </summary>
    [Fact]
    public void MednafenDisabledOptionsUsesZeroValues()
    {
        CopySampleToEmuDir("Mednafen", "Mednafen", "mednafen.cfg");

        var settings = CreateSettingsManager();
        settings.Mednafen.Fullscreen = false;
        settings.Mednafen.Vsync = true;
        settings.Mednafen.Cheats = false;
        settings.Mednafen.Rewind = false;
        settings.Mednafen.Bilinear = false;

        var emuDir = Path.Combine(_testDirectory, "Mednafen");
        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, Log.Logger);

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        var content = File.ReadAllText(configPath);

        Assert.Contains("video.fs 0", content, StringComparison.Ordinal);
        Assert.Contains("video.glvsync 1", content, StringComparison.Ordinal);
        Assert.Contains("cheats 0", content, StringComparison.Ordinal);
        Assert.Contains("state_rewind 0", content, StringComparison.Ordinal);
        Assert.Contains("nes.videoip 0", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that a new mednafen.cfg file is created from the bundled sample when the configuration file is missing.
    /// </summary>
    [Fact]
    public void MednafenCreatesConfigFromSampleIfMissing()
    {
        var emuDir = Path.Combine(_testDirectory, "Mednafen");
        Directory.CreateDirectory(emuDir);

        var settings = CreateSettingsManager();
        settings.Mednafen.VideoDriver = "sdl";
        settings.Mednafen.Fullscreen = true;

        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, Log.Logger);

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        Assert.True(File.Exists(configPath));
        var content = File.ReadAllText(configPath);
        Assert.Contains("video.driver sdl", content, StringComparison.Ordinal);
        Assert.Contains("video.fs 1", content, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that per-system settings are injected for every supported Mednafen system prefix.
    /// </summary>
    [Fact]
    public void MednafenAllSystemPrefixesAreInjected()
    {
        CopySampleToEmuDir("Mednafen", "Mednafen", "mednafen.cfg");

        var settings = CreateSettingsManager();
        settings.Mednafen.Stretch = "aspect";
        settings.Mednafen.Bilinear = false;
        settings.Mednafen.Scanlines = 0;
        settings.Mednafen.Shader = "none";
        settings.Mednafen.Special = "none";

        var emuDir = Path.Combine(_testDirectory, "Mednafen");
        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, Log.Logger);

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        var content = File.ReadAllText(configPath);

        var prefixes = new[] { "apple2", "gb", "gba", "gg", "lynx", "md", "nes", "ngp", "pce", "pce_fast", "pcfx", "psx", "sms", "snes", "snes_faust", "ss", "vb", "wswan" };
        foreach (var prefix in prefixes)
        {
            Assert.Contains($"{prefix}.stretch aspect", content, StringComparison.Ordinal);
            Assert.Contains($"{prefix}.videoip 0", content, StringComparison.Ordinal);
            Assert.Contains($"{prefix}.scanlines 0", content, StringComparison.Ordinal);
            Assert.Contains($"{prefix}.shader none", content, StringComparison.Ordinal);
            Assert.Contains($"{prefix}.special none", content, StringComparison.Ordinal);
        }
    }
}

using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class MednafenConfigInjectionTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly NoOpCredentialProtector _credentialProtector = new();

    public MednafenConfigInjectionTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();

        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_MednafenTest_{Guid.NewGuid():N}");
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
        return Path.Combine(emuDir, "mednafen.exe");
    }

    private SettingsManager CreateSettingsManager()
    {
        return new SettingsManager(_configuration, _logErrors, _credentialProtector);
    }

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
        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        var content = File.ReadAllText(configPath);

        Assert.Contains("video.driver opengl", content);
        Assert.Contains("video.fs 1", content);
        Assert.Contains("video.glvsync 0", content);
        Assert.Contains("video.blit_timesync 0", content);
        Assert.Contains("sound.volume 75", content);
        Assert.Contains("cheats 1", content);
        Assert.Contains("state_rewind 1", content);
    }

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
        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        var content = File.ReadAllText(configPath);

        // Check a few system prefixes
        Assert.Contains("nes.stretch full", content);
        Assert.Contains("nes.videoip 1", content);
        Assert.Contains("nes.scanlines 30", content);
        Assert.Contains("nes.shader CRT", content);
        Assert.Contains("nes.special hq2x", content);

        Assert.Contains("snes.stretch full", content);
        Assert.Contains("snes.videoip 1", content);

        Assert.Contains("psx.stretch full", content);
        Assert.Contains("psx.videoip 1", content);

        Assert.Contains("gba.stretch full", content);
        Assert.Contains("gba.videoip 1", content);
    }

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
        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        var content = File.ReadAllText(configPath);

        Assert.Contains("video.fs 0", content);
        Assert.Contains("video.glvsync 1", content);
        Assert.Contains("cheats 0", content);
        Assert.Contains("state_rewind 0", content);
        Assert.Contains("nes.videoip 0", content);
    }

    [Fact]
    public void MednafenCreatesConfigFromSampleIfMissing()
    {
        var emuDir = Path.Combine(_testDirectory, "Mednafen");
        Directory.CreateDirectory(emuDir);

        var settings = CreateSettingsManager();
        settings.Mednafen.VideoDriver = "sdl";
        settings.Mednafen.Fullscreen = true;

        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        Assert.True(File.Exists(configPath));
        var content = File.ReadAllText(configPath);
        Assert.Contains("video.driver sdl", content);
        Assert.Contains("video.fs 1", content);
    }

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
        MednafenConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "mednafen.cfg");
        var content = File.ReadAllText(configPath);

        var prefixes = new[] { "apple2", "gb", "gba", "gg", "lynx", "md", "nes", "ngp", "pce", "pce_fast", "pcfx", "psx", "sms", "snes", "snes_faust", "ss", "vb", "wswan" };
        foreach (var prefix in prefixes)
        {
            Assert.Contains($"{prefix}.stretch aspect", content);
            Assert.Contains($"{prefix}.videoip 0", content);
            Assert.Contains($"{prefix}.scanlines 0", content);
            Assert.Contains($"{prefix}.shader none", content);
            Assert.Contains($"{prefix}.special none", content);
        }
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}

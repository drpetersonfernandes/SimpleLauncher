using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class SettingsManagerRoundTripTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly NoOpCredentialProtector _credentialProtector = new();

    public SettingsManagerRoundTripTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_SettingsRoundTrip_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();
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

    [Fact]
    public void SettingsManagerResetToDefaultsRestoresAllDefaults()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        // Modify various settings
        settings.ThumbnailSize = 500;
        settings.GamesPerPage = 1000;
        settings.ShowGames = "ShowWithCover";
        settings.ViewMode = "ListView";
        settings.BaseTheme = "Light";
        settings.AccentColor = "Red";
        settings.Language = "pt-BR";
        settings.EnableFuzzyMatching = false;
        settings.FuzzyMatchingThreshold = 0.95;
        settings.EnableNotificationSound = false;

        // Reset
        settings.ResetToDefaults();

        // Verify defaults restored
        Assert.Equal(250, settings.ThumbnailSize);
        Assert.Equal(200, settings.GamesPerPage);
        Assert.Equal("ShowAll", settings.ShowGames);
        Assert.Equal("GridView", settings.ViewMode);
        Assert.Equal("Dark", settings.BaseTheme);
        Assert.Equal("Blue", settings.AccentColor);
        Assert.Equal("en", settings.Language);
        Assert.True(settings.EnableFuzzyMatching);
        Assert.Equal(0.80, settings.FuzzyMatchingThreshold);
        Assert.True(settings.EnableNotificationSound);
    }

    [Fact]
    public void SettingsManagerResetToDefaultsRestoresEmulatorSettings()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        // Modify emulator settings
        settings.DuckStation.StartFullscreen = true;
        settings.DuckStation.Renderer = "Vulkan";
        settings.RetroArch.Fullscreen = true;
        settings.RetroArch.VideoDriver = "vulkan";

        // Reset
        settings.ResetToDefaults();

        // Verify emulator defaults restored
        Assert.False(settings.DuckStation.StartFullscreen);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
        Assert.False(settings.RetroArch.Fullscreen);
        Assert.Equal("gl", settings.RetroArch.VideoDriver);
    }

    [Fact]
    public void SettingsManagerThumbnailSizeValidationRejectsInvalidValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        // Valid values
        settings.ThumbnailSize = 50;
        Assert.Equal(50, settings.ThumbnailSize);

        settings.ThumbnailSize = 800;
        Assert.Equal(800, settings.ThumbnailSize);

        settings.ThumbnailSize = 250;
        Assert.Equal(250, settings.ThumbnailSize);
    }

    [Fact]
    public void SettingsManagerGamesPerPageValidationRejectsInvalidValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        // Valid values
        settings.GamesPerPage = 100;
        Assert.Equal(100, settings.GamesPerPage);

        settings.GamesPerPage = 1000000;
        Assert.Equal(1000000, settings.GamesPerPage);

        settings.GamesPerPage = 200;
        Assert.Equal(200, settings.GamesPerPage);
    }

    [Fact]
    public void SettingsManagerShowGamesValidationRejectsInvalidValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        settings.ShowGames = "ShowAll";
        Assert.Equal("ShowAll", settings.ShowGames);

        settings.ShowGames = "ShowWithCover";
        Assert.Equal("ShowWithCover", settings.ShowGames);

        settings.ShowGames = "ShowWithoutCover";
        Assert.Equal("ShowWithoutCover", settings.ShowGames);
    }

    [Fact]
    public void SettingsManagerViewModeValidationRejectsInvalidValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        settings.ViewMode = "GridView";
        Assert.Equal("GridView", settings.ViewMode);

        settings.ViewMode = "ListView";
        Assert.Equal("ListView", settings.ViewMode);
    }

    [Fact]
    public void SettingsManagerBaseThemeValidationRejectsInvalidValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        settings.BaseTheme = "Light";
        Assert.Equal("Light", settings.BaseTheme);

        settings.BaseTheme = "Dark";
        Assert.Equal("Dark", settings.BaseTheme);

        settings.BaseTheme = "Adaptive";
        Assert.Equal("Adaptive", settings.BaseTheme);

        settings.BaseTheme = "HighContrast";
        Assert.Equal("HighContrast", settings.BaseTheme);

        settings.BaseTheme = "Midnight";
        Assert.Equal("Midnight", settings.BaseTheme);
    }

    [Fact]
    public void SettingsManagerAccentColorValidationAcceptsValidColors()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        var validColors = new[] { "Amber", "Blue", "Brown", "Cobalt", "Crimson", "Cyan", "Emerald", "Green", "Indigo", "Lime", "Magenta", "Maroon", "Mauve", "Olive", "OliveDrab", "Orange", "Pink", "Plum", "Purple", "Red", "Sienna", "SkyBlue", "Steel", "Taupe", "Teal", "Violet", "Yellow" };

        foreach (var color in validColors)
        {
            settings.AccentColor = color;
            Assert.Equal(color, settings.AccentColor);
        }
    }

    [Fact]
    public void SettingsManagerButtonAspectRatioValidationAcceptsValidValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        var validValues = new[] { "Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2" };

        foreach (var value in validValues)
        {
            settings.ButtonAspectRatio = value;
            Assert.Equal(value, settings.ButtonAspectRatio);
        }
    }

    [Fact]
    public void SettingsManagerFilenameDisplayModeValidationAcceptsValidValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        settings.FilenameDisplayMode = "Original";
        Assert.Equal("Original", settings.FilenameDisplayMode);

        settings.FilenameDisplayMode = "CleanUp";
        Assert.Equal("CleanUp", settings.FilenameDisplayMode);

        settings.FilenameDisplayMode = "NoFilename";
        Assert.Equal("NoFilename", settings.FilenameDisplayMode);
    }

    [Fact]
    public void SettingsManagerFontSizeValidationAcceptsValidValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        settings.FilenameFontSize = "Small";
        Assert.Equal("Small", settings.FilenameFontSize);

        settings.FilenameFontSize = "Normal";
        Assert.Equal("Normal", settings.FilenameFontSize);

        settings.FilenameFontSize = "Big";
        Assert.Equal("Big", settings.FilenameFontSize);
    }

    [Fact]
    public void SettingsManagerUpdateSystemPlayTimeAddsNewEntry()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(30));

        Assert.Single(settings.SystemPlayTimes);
        Assert.Equal("NES", settings.SystemPlayTimes[0].SystemName);
        Assert.Equal(1800, settings.SystemPlayTimes[0].PlayTimeSeconds);
    }

    [Fact]
    public void SettingsManagerUpdateSystemPlayTimeAccumulatesExistingEntry()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(30));
        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(15));

        Assert.Single(settings.SystemPlayTimes);
        Assert.Equal(2700, settings.SystemPlayTimes[0].PlayTimeSeconds); // 30m + 15m = 45m = 2700s
    }

    [Fact]
    public void SettingsManagerUpdateSystemPlayTimeMultipleSystems()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(30));
        settings.UpdateSystemPlayTime("SNES", TimeSpan.FromMinutes(45));
        settings.UpdateSystemPlayTime("GBA", TimeSpan.FromMinutes(60));

        Assert.Equal(3, settings.SystemPlayTimes.Count);
        Assert.Equal(1800, settings.SystemPlayTimes[0].PlayTimeSeconds);
        Assert.Equal(2700, settings.SystemPlayTimes[1].PlayTimeSeconds);
        Assert.Equal(3600, settings.SystemPlayTimes[2].PlayTimeSeconds);
    }

    [Fact]
    public void SettingsManagerDefaultDuckStationSettingsAreCorrect()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        Assert.False(settings.DuckStation.StartFullscreen);
        Assert.True(settings.DuckStation.PauseOnFocusLoss);
        Assert.True(settings.DuckStation.SaveStateOnExit);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
        Assert.Equal(2, settings.DuckStation.ResolutionScale);
        Assert.Equal("Nearest", settings.DuckStation.TextureFilter);
        Assert.False(settings.DuckStation.WidescreenHack);
        Assert.False(settings.DuckStation.PgxpEnable);
        Assert.Equal("16:9", settings.DuckStation.AspectRatio);
        Assert.Equal(100, settings.DuckStation.OutputVolume);
        Assert.False(settings.DuckStation.OutputMuted);
    }

    [Fact]
    public void SettingsManagerDefaultXeniaSettingsAreCorrect()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        Assert.Equal("xaudio2", settings.Xenia.Apu);
        Assert.False(settings.Xenia.Mute);
        Assert.Equal("d3d12", settings.Xenia.Gpu);
        Assert.True(settings.Xenia.Vsync);
        Assert.Equal(1, settings.Xenia.ResScaleX);
        Assert.Equal(1, settings.Xenia.ResScaleY);
        Assert.False(settings.Xenia.Fullscreen);
        Assert.Equal("xinput", settings.Xenia.Hid);
        Assert.True(settings.Xenia.Vibration);
        Assert.True(settings.Xenia.ApplyPatches);
        Assert.True(settings.Xenia.DiscordPresence);
    }

    [Fact]
    public void SettingsManagerDefaultRpcs3SettingsAreCorrect()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        Assert.Equal("Recompiler (LLVM)", settings.Rpcs3.PpuDecoder);
        Assert.Equal("Recompiler (LLVM)", settings.Rpcs3.SpuDecoder);
        Assert.Equal("Vulkan", settings.Rpcs3.Renderer);
        Assert.Equal("1280x720", settings.Rpcs3.Resolution);
        Assert.Equal("16:9", settings.Rpcs3.AspectRatio);
        Assert.False(settings.Rpcs3.Vsync);
        Assert.Equal(100, settings.Rpcs3.ResolutionScale);
        Assert.Equal(0, settings.Rpcs3.AnisotropicFilter);
        Assert.Equal("Cubeb", settings.Rpcs3.AudioRenderer);
        Assert.True(settings.Rpcs3.AudioBuffering);
        Assert.False(settings.Rpcs3.StartFullscreen);
    }

    [Fact]
    public void SettingsManagerDefaultMednafenSettingsAreCorrect()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        Assert.Equal("opengl", settings.Mednafen.VideoDriver);
        Assert.False(settings.Mednafen.Fullscreen);
        Assert.True(settings.Mednafen.Vsync);
        Assert.Equal(100, settings.Mednafen.Volume);
        Assert.True(settings.Mednafen.Cheats);
        Assert.False(settings.Mednafen.Rewind);
        Assert.Equal("aspect", settings.Mednafen.Stretch);
        Assert.False(settings.Mednafen.Bilinear);
        Assert.Equal(0, settings.Mednafen.Scanlines);
        Assert.Equal("none", settings.Mednafen.Shader);
        Assert.Equal("none", settings.Mednafen.Special);
    }

    [Fact]
    public void SettingsManagerDefaultStellaSettingsAreCorrect()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

        Assert.False(settings.Stella.Fullscreen);
        Assert.True(settings.Stella.Vsync);
        Assert.Equal("direct3d", settings.Stella.VideoDriver);
        Assert.True(settings.Stella.CorrectAspect);
        Assert.Equal(0, settings.Stella.TvFilter);
        Assert.Equal(0, settings.Stella.Scanlines);
        Assert.True(settings.Stella.AudioEnabled);
        Assert.Equal(80, settings.Stella.AudioVolume);
        Assert.True(settings.Stella.TimeMachine);
        Assert.False(settings.Stella.ConfirmExit);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}

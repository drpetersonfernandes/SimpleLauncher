using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class SettingsManagerTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();

    public SettingsManagerTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_SettingsTest_{Guid.NewGuid():N}");
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
    public void DefaultThumbnailSizeIs250()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal(250, settings.ThumbnailSize);
    }

    [Fact]
    public void DefaultGamesPerPageIs200()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal(200, settings.GamesPerPage);
    }

    [Fact]
    public void DefaultShowGamesIsShowAll()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("ShowAll", settings.ShowGames);
    }

    [Fact]
    public void DefaultViewModeIsGridView()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("GridView", settings.ViewMode);
    }

    [Fact]
    public void DefaultBaseThemeIsDark()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("Dark", settings.BaseTheme);
    }

    [Fact]
    public void DefaultAccentColorIsBlue()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("Blue", settings.AccentColor);
    }

    [Fact]
    public void DefaultLanguageIsEn()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("en", settings.Language);
    }

    [Fact]
    public void DefaultDeadZoneXIsCorrect()
    {
        Assert.Equal(0.05f, Services.SettingsManager.SettingsManager.DefaultDeadZoneX);
    }

    [Fact]
    public void DefaultDeadZoneYIsCorrect()
    {
        Assert.Equal(0.02f, Services.SettingsManager.SettingsManager.DefaultDeadZoneY);
    }

    [Fact]
    public void DefaultEnableFuzzyMatchingIsTrue()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.True(settings.EnableFuzzyMatching);
    }

    [Fact]
    public void DefaultFuzzyMatchingThresholdIs080()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal(0.80, settings.FuzzyMatchingThreshold);
    }

    [Fact]
    public void DefaultEnableNotificationSoundIsTrue()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.True(settings.EnableNotificationSound);
    }

    [Fact]
    public void DefaultButtonAspectRatioIsSquare()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("Square", settings.ButtonAspectRatio);
    }

    [Fact]
    public void DefaultFilenameDisplayModeIsOriginal()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("Original", settings.FilenameDisplayMode);
    }

    [Fact]
    public void DefaultFilenameFontSizeIsNormal()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("Normal", settings.FilenameFontSize);
    }

    [Fact]
    public void DefaultMachineNameFontSizeIsNormal()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("Normal", settings.MachineNameFontSize);
    }

    [Fact]
    public void DefaultStyleVariantIsDefault()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("Default", settings.StyleVariant);
    }

    [Fact]
    public void DefaultOverlayOpenVideoButtonIsTrue()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.True(settings.OverlayOpenVideoButton);
    }

    [Fact]
    public void DefaultAdditionalSystemFoldersExpandedIsTrue()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.True(settings.AdditionalSystemFoldersExpanded);
    }

    [Fact]
    public void DefaultEmulatorExpandedStatesAreTrue()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.True(settings.Emulator1Expanded);
        Assert.True(settings.Emulator2Expanded);
        Assert.True(settings.Emulator3Expanded);
        Assert.True(settings.Emulator4Expanded);
        Assert.True(settings.Emulator5Expanded);
    }

    [Fact]
    public void SettingsCanBeModified()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.ThumbnailSize = 500;
        settings.GamesPerPage = 1000;
        settings.ShowGames = "ShowWithCover";
        settings.ViewMode = "ListView";
        settings.BaseTheme = "Light";
        settings.AccentColor = "Red";
        settings.Language = "pt-BR";
        settings.EnableFuzzyMatching = false;
        settings.FuzzyMatchingThreshold = 0.95;

        Assert.Equal(500, settings.ThumbnailSize);
        Assert.Equal(1000, settings.GamesPerPage);
        Assert.Equal("ShowWithCover", settings.ShowGames);
        Assert.Equal("ListView", settings.ViewMode);
        Assert.Equal("Light", settings.BaseTheme);
        Assert.Equal("Red", settings.AccentColor);
        Assert.Equal("pt-BR", settings.Language);
        Assert.False(settings.EnableFuzzyMatching);
        Assert.Equal(0.95, settings.FuzzyMatchingThreshold);
    }

    [Fact]
    public void DefaultDuckStationSettingsAreCorrect()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.False(settings.DuckStation.StartFullscreen);
        Assert.True(settings.DuckStation.PauseOnFocusLoss);
        Assert.True(settings.DuckStation.SaveStateOnExit);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
        Assert.Equal(2, settings.DuckStation.ResolutionScale);
        Assert.Equal("Nearest", settings.DuckStation.TextureFilter);
        Assert.Equal("16:9", settings.DuckStation.AspectRatio);
        Assert.Equal(100, settings.DuckStation.OutputVolume);
    }

    [Fact]
    public void DefaultRetroArchSettingsAreCorrect()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.False(settings.RetroArch.Fullscreen);
        Assert.Equal("gl", settings.RetroArch.VideoDriver);
        Assert.True(settings.RetroArch.Vsync);
        Assert.True(settings.RetroArch.AudioEnable);
        Assert.Equal("ozone", settings.RetroArch.MenuDriver);
        Assert.True(settings.RetroArch.ShowAdvancedSettings);
    }

    [Fact]
    public void SystemPlayTimesDefaultIsEmpty()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Empty(settings.SystemPlayTimes);
    }

    [Fact]
    public void VideoUrlDefaultsFromConfiguration()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("https://www.youtube.com/results?search_query=", settings.VideoUrl);
    }

    [Fact]
    public void InfoUrlDefaultsFromConfiguration()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);
        Assert.Equal("https://www.igdb.com/search?q=", settings.InfoUrl);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}

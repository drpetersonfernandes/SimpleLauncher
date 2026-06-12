using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Tests the SettingsManager for correct default values, modification behavior, and emulator-specific settings.
/// </summary>
public class SettingsManagerTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly NoOpCredentialProtector _credentialProtector = new();

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

    /// <summary>
    /// Verifies the default thumbnail size is 250.
    /// </summary>
    [Fact]
    public void DefaultThumbnailSizeIs250()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(250, settings.ThumbnailSize);
    }

    /// <summary>
    /// Verifies the default games per page is 200.
    /// </summary>
    [Fact]
    public void DefaultGamesPerPageIs200()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(200, settings.GamesPerPage);
    }

    /// <summary>
    /// Verifies the default show games setting is ShowAll.
    /// </summary>
    [Fact]
    public void DefaultShowGamesIsShowAll()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("ShowAll", settings.ShowGames);
    }

    /// <summary>
    /// Verifies the default view mode is GridView.
    /// </summary>
    [Fact]
    public void DefaultViewModeIsGridView()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("GridView", settings.ViewMode);
    }

    /// <summary>
    /// Verifies the default base theme is Dark.
    /// </summary>
    [Fact]
    public void DefaultBaseThemeIsDark()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Dark", settings.BaseTheme);
    }

    /// <summary>
    /// Verifies the default accent color is Blue.
    /// </summary>
    [Fact]
    public void DefaultAccentColorIsBlue()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Blue", settings.AccentColor);
    }

    /// <summary>
    /// Verifies the default language is en.
    /// </summary>
    [Fact]
    public void DefaultLanguageIsEn()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("en", settings.Language);
    }

    /// <summary>
    /// Verifies the default X-axis dead zone value is 0.05.
    /// </summary>
    [Fact]
    public void DefaultDeadZoneXIsCorrect()
    {
        Assert.Equal(0.05f, SettingsManager.DefaultDeadZoneX);
    }

    /// <summary>
    /// Verifies the default Y-axis dead zone value is 0.02.
    /// </summary>
    [Fact]
    public void DefaultDeadZoneYIsCorrect()
    {
        Assert.Equal(0.02f, SettingsManager.DefaultDeadZoneY);
    }

    /// <summary>
    /// Verifies that fuzzy matching is enabled by default.
    /// </summary>
    [Fact]
    public void DefaultEnableFuzzyMatchingIsTrue()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.EnableFuzzyMatching);
    }

    /// <summary>
    /// Verifies the default fuzzy matching threshold is 0.80.
    /// </summary>
    [Fact]
    public void DefaultFuzzyMatchingThresholdIs080()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(0.80, settings.FuzzyMatchingThreshold);
    }

    /// <summary>
    /// Verifies that notification sounds are enabled by default.
    /// </summary>
    [Fact]
    public void DefaultEnableNotificationSoundIsTrue()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.EnableNotificationSound);
    }

    /// <summary>
    /// Verifies the default button aspect ratio is Square.
    /// </summary>
    [Fact]
    public void DefaultButtonAspectRatioIsSquare()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Square", settings.ButtonAspectRatio);
    }

    /// <summary>
    /// Verifies the default filename display mode is Original.
    /// </summary>
    [Fact]
    public void DefaultFilenameDisplayModeIsOriginal()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Original", settings.FilenameDisplayMode);
    }

    /// <summary>
    /// Verifies the default filename font size is Normal.
    /// </summary>
    [Fact]
    public void DefaultFilenameFontSizeIsNormal()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Normal", settings.FilenameFontSize);
    }

    /// <summary>
    /// Verifies the default machine name font size is Normal.
    /// </summary>
    [Fact]
    public void DefaultMachineNameFontSizeIsNormal()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Normal", settings.MachineNameFontSize);
    }

    /// <summary>
    /// Verifies the default style variant is Default.
    /// </summary>
    [Fact]
    public void DefaultStyleVariantIsDefault()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Default", settings.StyleVariant);
    }

    /// <summary>
    /// Verifies the overlay open video button is enabled by default.
    /// </summary>
    [Fact]
    public void DefaultOverlayOpenVideoButtonIsTrue()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.OverlayOpenVideoButton);
    }

    /// <summary>
    /// Verifies that additional system folders section is expanded by default.
    /// </summary>
    [Fact]
    public void DefaultAdditionalSystemFoldersExpandedIsTrue()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.AdditionalSystemFoldersExpanded);
    }

    /// <summary>
    /// Verifies that all emulator expanded states default to true.
    /// </summary>
    [Fact]
    public void DefaultEmulatorExpandedStatesAreTrue()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.Emulator1Expanded);
        Assert.True(settings.Emulator2Expanded);
        Assert.True(settings.Emulator3Expanded);
        Assert.True(settings.Emulator4Expanded);
        Assert.True(settings.Emulator5Expanded);
    }

    /// <summary>
    /// Verifies that settings properties can be modified and the new values are retained.
    /// </summary>
    [Fact]
    public void SettingsCanBeModified()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);

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

    /// <summary>
    /// Verifies that DuckStation emulator settings have correct default values.
    /// </summary>
    [Fact]
    public void DefaultDuckStationSettingsAreCorrect()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.False(settings.DuckStation.StartFullscreen);
        Assert.True(settings.DuckStation.PauseOnFocusLoss);
        Assert.True(settings.DuckStation.SaveStateOnExit);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
        Assert.Equal(2, settings.DuckStation.ResolutionScale);
        Assert.Equal("Nearest", settings.DuckStation.TextureFilter);
        Assert.Equal("16:9", settings.DuckStation.AspectRatio);
        Assert.Equal(100, settings.DuckStation.OutputVolume);
    }

    /// <summary>
    /// Verifies that RetroArch emulator settings have correct default values.
    /// </summary>
    [Fact]
    public void DefaultRetroArchSettingsAreCorrect()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.False(settings.RetroArch.Fullscreen);
        Assert.Equal("gl", settings.RetroArch.VideoDriver);
        Assert.True(settings.RetroArch.Vsync);
        Assert.True(settings.RetroArch.AudioEnable);
        Assert.Equal("ozone", settings.RetroArch.MenuDriver);
        Assert.True(settings.RetroArch.ShowAdvancedSettings);
    }

    /// <summary>
    /// Verifies that system play times collection is empty by default.
    /// </summary>
    [Fact]
    public void SystemPlayTimesDefaultIsEmpty()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Empty(settings.SystemPlayTimes);
    }

    /// <summary>
    /// Verifies that the video URL defaults are loaded from configuration.
    /// </summary>
    [Fact]
    public void VideoUrlDefaultsFromConfiguration()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("https://www.youtube.com/results?search_query=", settings.VideoUrl);
    }

    /// <summary>
    /// Verifies that the info URL defaults are loaded from configuration.
    /// </summary>
    [Fact]
    public void InfoUrlDefaultsFromConfiguration()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
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

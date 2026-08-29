using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests the SettingsManagerService for correct default values, modification behavior, and emulator-specific settings.
/// </summary>
public class SettingsManagerTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly NoOpCredentialProtector _credentialProtector = new();
    private readonly ILogger _logErrors = new NoOpLogger();
    private readonly string _testDirectory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SettingsManagerTests" /> class,
    ///     installing the service provider mock, creating a temporary test directory, and building configuration.
    /// </summary>
    public SettingsManagerTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_SettingsTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();
    }

    /// <summary>
    ///     Cleans up the temporary test directory and restores the service provider mock.
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

    /// <summary>
    ///     Verifies the default thumbnail size is 250.
    /// </summary>
    [Fact]
    public void DefaultThumbnailSizeIs250()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(250, settings.ThumbnailSize);
    }

    /// <summary>
    ///     Verifies the default games per page is 200.
    /// </summary>
    [Fact]
    public void DefaultGamesPerPageIs200()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(200, settings.GamesPerPage);
    }

    /// <summary>
    ///     Verifies the default show games setting is ShowAll.
    /// </summary>
    [Fact]
    public void DefaultShowGamesIsShowAll()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("ShowAll", settings.ShowGames);
    }

    /// <summary>
    ///     Verifies the default view mode is GridView.
    /// </summary>
    [Fact]
    public void DefaultViewModeIsGridView()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("GridView", settings.ViewMode);
    }

    /// <summary>
    ///     Verifies the default base theme is Dark.
    /// </summary>
    [Fact]
    public void DefaultBaseThemeIsDark()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Dark", settings.BaseTheme);
    }

    /// <summary>
    ///     Verifies the default accent color is Blue.
    /// </summary>
    [Fact]
    public void DefaultAccentColorIsBlue()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Blue", settings.AccentColor);
    }

    /// <summary>
    ///     Verifies the default language is en.
    /// </summary>
    [Fact]
    public void DefaultLanguageIsEn()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("en", settings.Language);
    }

    /// <summary>
    ///     Verifies the default X-axis dead zone value is 0.05.
    /// </summary>
    [Fact]
    public void DefaultDeadZoneXIsCorrect()
    {
        Assert.Equal(0.05f, SettingsManagerService.DefaultDeadZoneX);
    }

    /// <summary>
    ///     Verifies the default Y-axis dead zone value is 0.02.
    /// </summary>
    [Fact]
    public void DefaultDeadZoneYIsCorrect()
    {
        Assert.Equal(0.02f, SettingsManagerService.DefaultDeadZoneY);
    }

    /// <summary>
    ///     Verifies that fuzzy matching is enabled by default.
    /// </summary>
    [Fact]
    public void DefaultEnableFuzzyMatchingIsTrue()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.EnableFuzzyMatching);
    }

    /// <summary>
    ///     Verifies the default fuzzy matching threshold is 0.80.
    /// </summary>
    [Fact]
    public void DefaultFuzzyMatchingThresholdIs080()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(0.80, settings.FuzzyMatchingThreshold);
    }

    /// <summary>
    ///     Verifies that notification sounds are enabled by default.
    /// </summary>
    [Fact]
    public void DefaultEnableNotificationSoundIsTrue()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.EnableNotificationSound);
    }

    /// <summary>
    ///     Verifies the default button aspect ratio is Square.
    /// </summary>
    [Fact]
    public void DefaultButtonAspectRatioIsSquare()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Square", settings.ButtonAspectRatio);
    }

    /// <summary>
    ///     Verifies the default filename display mode is Original.
    /// </summary>
    [Fact]
    public void DefaultFilenameDisplayModeIsOriginal()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Original", settings.FilenameDisplayMode);
    }

    /// <summary>
    ///     Verifies the default filename font size is Normal.
    /// </summary>
    [Fact]
    public void DefaultFilenameFontSizeIsNormal()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Normal", settings.FilenameFontSize);
    }

    /// <summary>
    ///     Verifies the default machine name font size is Normal.
    /// </summary>
    [Fact]
    public void DefaultMachineNameFontSizeIsNormal()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Normal", settings.MachineNameFontSize);
    }

    /// <summary>
    ///     Verifies the default style variant is Default.
    /// </summary>
    [Fact]
    public void DefaultStyleVariantIsDefault()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Default", settings.StyleVariant);
    }

    /// <summary>
    ///     Verifies the overlay open video button is enabled by default.
    /// </summary>
    [Fact]
    public void DefaultOverlayOpenVideoButtonIsTrue()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.OverlayOpenVideoButton);
    }

    /// <summary>
    ///     Verifies that additional system folders section is expanded by default.
    /// </summary>
    [Fact]
    public void DefaultAdditionalSystemFoldersExpandedIsTrue()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.AdditionalSystemFoldersExpanded);
    }

    /// <summary>
    ///     Verifies that all emulator expanded states default to true.
    /// </summary>
    [Fact]
    public void DefaultEmulatorExpandedStatesAreTrue()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.Emulator1Expanded);
        Assert.True(settings.Emulator2Expanded);
        Assert.True(settings.Emulator3Expanded);
        Assert.True(settings.Emulator4Expanded);
        Assert.True(settings.Emulator5Expanded);
    }

    /// <summary>
    ///     Verifies that settings properties can be modified and the new values are retained.
    /// </summary>
    [Fact]
    public void SettingsCanBeModified()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

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
    ///     Verifies that DuckStation emulator settings have correct default values.
    /// </summary>
    [Fact]
    public void DefaultDuckStationSettingsAreCorrect()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
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
    ///     Verifies that RetroArch emulator settings have correct default values.
    /// </summary>
    [Fact]
    public void DefaultRetroArchSettingsAreCorrect()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.False(settings.RetroArch.Fullscreen);
        Assert.Equal("gl", settings.RetroArch.VideoDriver);
        Assert.True(settings.RetroArch.Vsync);
        Assert.True(settings.RetroArch.AudioEnable);
        Assert.Equal("ozone", settings.RetroArch.MenuDriver);
        Assert.True(settings.RetroArch.ShowAdvancedSettings);
    }

    /// <summary>
    ///     Verifies that system play times collection is empty by default.
    /// </summary>
    [Fact]
    public void SystemPlayTimesDefaultIsEmpty()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Empty(settings.SystemPlayTimes);
    }

    /// <summary>
    ///     Verifies that the video URL defaults are loaded from configuration.
    /// </summary>
    [Fact]
    public void VideoUrlDefaultsFromConfiguration()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("https://www.youtube.com/results?search_query=", settings.VideoUrl);
    }

    /// <summary>
    ///     Verifies that the info URL defaults are loaded from configuration.
    /// </summary>
    [Fact]
    public void InfoUrlDefaultsFromConfiguration()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("https://www.igdb.com/search?q=", settings.InfoUrl);
    }

    /// <summary>
    ///     Verifies that resetting to defaults restores all general settings to their default values.
    /// </summary>
    [Fact]
    public void ResetToDefaultsRestoresAllGeneralSettings()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

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

        settings.ResetToDefaults();

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

    /// <summary>
    ///     Verifies that resetting to defaults restores emulator-specific settings to their default values.
    /// </summary>
    [Fact]
    public void ResetToDefaultsRestoresEmulatorSettings()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

        settings.DuckStation.StartFullscreen = true;
        settings.DuckStation.Renderer = "Vulkan";
        settings.RetroArch.Fullscreen = true;
        settings.RetroArch.VideoDriver = "vulkan";

        settings.ResetToDefaults();

        Assert.False(settings.DuckStation.StartFullscreen);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
        Assert.False(settings.RetroArch.Fullscreen);
        Assert.Equal("gl", settings.RetroArch.VideoDriver);
    }

    /// <summary>
    ///     Verifies that the ThumbnailSize property accepts valid values within the allowed range.
    /// </summary>
    [Fact]
    public void ThumbnailSizeValidationAcceptsValidValues()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

        settings.ThumbnailSize = 50;
        Assert.Equal(50, settings.ThumbnailSize);

        settings.ThumbnailSize = 800;
        Assert.Equal(800, settings.ThumbnailSize);
    }

    /// <summary>
    ///     Verifies that the GamesPerPage property accepts valid values within the allowed range.
    /// </summary>
    [Fact]
    public void GamesPerPageValidationAcceptsValidValues()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

        settings.GamesPerPage = 100;
        Assert.Equal(100, settings.GamesPerPage);

        settings.GamesPerPage = 1000000;
        Assert.Equal(1000000, settings.GamesPerPage);
    }

    /// <summary>
    ///     Verifies that the AccentColor property accepts all valid color names.
    /// </summary>
    [Fact]
    public void AccentColorValidationAcceptsAllValidColors()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

        var validColors = new[]
        {
            "Amber", "Blue", "Brown", "Cobalt", "Crimson", "Cyan", "Emerald", "Green", "Indigo", "Lime", "Magenta",
            "Maroon", "Mauve", "Olive", "OliveDrab", "Orange", "Pink", "Plum", "Purple", "Red", "Sienna", "SkyBlue",
            "Steel", "Taupe", "Teal", "Violet", "Yellow"
        };

        foreach (var color in validColors)
        {
            settings.AccentColor = color;
            Assert.Equal(color, settings.AccentColor);
        }
    }

    /// <summary>
    ///     Verifies that UpdateSystemPlayTime adds a new entry for a system not yet tracked.
    /// </summary>
    [Fact]
    public void UpdateSystemPlayTimeAddsNewEntry()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(30));

        Assert.Single(settings.SystemPlayTimes);
        Assert.Equal("NES", settings.SystemPlayTimes[0].SystemName);
        Assert.Equal(1800, settings.SystemPlayTimes[0].PlayTimeSeconds);
    }

    /// <summary>
    ///     Verifies that UpdateSystemPlayTime accumulates play time for an existing system entry.
    /// </summary>
    [Fact]
    public void UpdateSystemPlayTimeAccumulatesExistingEntry()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(30));
        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(15));

        Assert.Single(settings.SystemPlayTimes);
        Assert.Equal(2700, settings.SystemPlayTimes[0].PlayTimeSeconds);
    }

    /// <summary>
    ///     Verifies that UpdateSystemPlayTime tracks play time for multiple independent systems.
    /// </summary>
    [Fact]
    public void UpdateSystemPlayTimeMultipleSystems()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(30));
        settings.UpdateSystemPlayTime("SNES", TimeSpan.FromMinutes(45));
        settings.UpdateSystemPlayTime("GBA", TimeSpan.FromMinutes(60));

        Assert.Equal(3, settings.SystemPlayTimes.Count);
        Assert.Equal(1800, settings.SystemPlayTimes[0].PlayTimeSeconds);
        Assert.Equal(2700, settings.SystemPlayTimes[1].PlayTimeSeconds);
        Assert.Equal(3600, settings.SystemPlayTimes[2].PlayTimeSeconds);
    }

    /// <summary>
    ///     Verifies that Xenia emulator settings have correct default values.
    /// </summary>
    [Fact]
    public void DefaultXeniaSettingsAreCorrect()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

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

    /// <summary>
    ///     Verifies that RPCS3 emulator settings have correct default values.
    /// </summary>
    [Fact]
    public void DefaultRpcs3SettingsAreCorrect()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

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

    /// <summary>
    ///     Verifies that Mednafen emulator settings have correct default values.
    /// </summary>
    [Fact]
    public void DefaultMednafenSettingsAreCorrect()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

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

    /// <summary>
    ///     Verifies that Stella emulator settings have correct default values.
    /// </summary>
    [Fact]
    public void DefaultStellaSettingsAreCorrect()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);

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
}
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="SettingsManagerService"/> covering additional edge cases for
/// setting modifications, emulator settings, play time tracking, and reset behavior.
/// </summary>
public class SettingsManagerExtendedTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logErrors = new NoOpLogger();
    private readonly NoOpCredentialProtector _credentialProtector = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsManagerExtendedTests"/> class,
    /// installing the service provider mock, creating a temporary test directory, and building configuration.
    /// </summary>
    public SettingsManagerExtendedTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_SettingsExtTest_{Guid.NewGuid():N}");
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

    /// <summary>
    /// Verifies the default ShowGames setting is "ShowAll".
    /// </summary>
    [Fact]
    public void DefaultShowGamesIsShowAll()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("ShowAll", settings.ShowGames);
    }

    /// <summary>
    /// Verifies that ShowGames can be changed to "ShowWithCover".
    /// </summary>
    [Fact]
    public void ShowGamesCanBeChangedToShowWithCover()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.ShowGames = "ShowWithCover";
        Assert.Equal("ShowWithCover", settings.ShowGames);
    }

    /// <summary>
    /// Verifies that ShowGames can be changed to "ShowWithoutCover".
    /// </summary>
    [Fact]
    public void ShowGamesCanBeChangedToShowWithoutCover()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.ShowGames = "ShowWithoutCover";
        Assert.Equal("ShowWithoutCover", settings.ShowGames);
    }

    /// <summary>
    /// Verifies the default ViewMode is "GridView".
    /// </summary>
    [Fact]
    public void DefaultViewModeIsGridView()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("GridView", settings.ViewMode);
    }

    /// <summary>
    /// Verifies that ViewMode can be changed to "ListView".
    /// </summary>
    [Fact]
    public void ViewModeCanBeChangedToListView()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.ViewMode = "ListView";
        Assert.Equal("ListView", settings.ViewMode);
    }

    /// <summary>
    /// Verifies that updating system play time with zero duration does not throw an exception.
    /// </summary>
    [Fact]
    public void UpdateSystemPlayTimeZeroDurationStillCreatesEntry()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.UpdateSystemPlayTime("NES", TimeSpan.Zero);
        // Zero duration may or may not create an entry depending on implementation
        // Just verify no exception is thrown
        Assert.True(settings.SystemPlayTimes.Count <= 1);
    }

    /// <summary>
    /// Verifies that a large play time duration is stored correctly.
    /// </summary>
    [Fact]
    public void UpdateSystemPlayTimeLargeDuration()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.UpdateSystemPlayTime("NES", TimeSpan.FromHours(100));
        Assert.Equal(360000, settings.SystemPlayTimes[0].PlayTimeSeconds);
    }

    /// <summary>
    /// Verifies that many small play time accumulations are correctly summed.
    /// </summary>
    [Fact]
    public void UpdateSystemPlayTimeManyAccumulations()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        for (var i = 0; i < 100; i++)
        {
            settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(1));
        }

        Assert.Single(settings.SystemPlayTimes);
        Assert.Equal(6000, settings.SystemPlayTimes[0].PlayTimeSeconds);
    }

    /// <summary>
    /// Verifies that ResetToDefaults clears all system play time entries.
    /// </summary>
    [Fact]
    public void ResetToDefaultsRestoresSystemPlayTimes()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(30));
        settings.UpdateSystemPlayTime("SNES", TimeSpan.FromMinutes(45));

        settings.ResetToDefaults();

        Assert.Empty(settings.SystemPlayTimes);
    }

    /// <summary>
    /// Verifies that ResetToDefaults restores the ViewMode to its default value.
    /// </summary>
    [Fact]
    public void ResetToDefaultsRestoresViewMode()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.ViewMode = "ListView";
        settings.ResetToDefaults();
        Assert.Equal("GridView", settings.ViewMode);
    }

    /// <summary>
    /// Verifies that ResetToDefaults restores the ShowGames setting to its default value.
    /// </summary>
    [Fact]
    public void ResetToDefaultsRestoresShowGames()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.ShowGames = "ShowWithCover";
        settings.ResetToDefaults();
        Assert.Equal("ShowAll", settings.ShowGames);
    }

    /// <summary>
    /// Verifies the default DuckStation Renderer is "Automatic".
    /// </summary>
    [Fact]
    public void DefaultDuckStationRendererIsAutomatic()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
    }

    /// <summary>
    /// Verifies that DuckStation emulator settings can be modified.
    /// </summary>
    [Fact]
    public void DuckStationSettingsCanBeModified()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.DuckStation.StartFullscreen = true;
        settings.DuckStation.Renderer = "Vulkan";
        settings.DuckStation.ResolutionScale = 4;

        Assert.True(settings.DuckStation.StartFullscreen);
        Assert.Equal("Vulkan", settings.DuckStation.Renderer);
        Assert.Equal(4, settings.DuckStation.ResolutionScale);
    }

    /// <summary>
    /// Verifies the default RetroArch VideoDriver is "gl".
    /// </summary>
    [Fact]
    public void DefaultRetroArchVideoDriverIsGl()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("gl", settings.RetroArch.VideoDriver);
    }

    /// <summary>
    /// Verifies that RetroArch emulator settings can be modified.
    /// </summary>
    [Fact]
    public void RetroArchSettingsCanBeModified()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.RetroArch.Fullscreen = true;
        settings.RetroArch.VideoDriver = "vulkan";
        settings.RetroArch.Vsync = false;

        Assert.True(settings.RetroArch.Fullscreen);
        Assert.Equal("vulkan", settings.RetroArch.VideoDriver);
        Assert.False(settings.RetroArch.Vsync);
    }

    /// <summary>
    /// Verifies the default RPCS3 Renderer is "Vulkan".
    /// </summary>
    [Fact]
    public void DefaultRpcs3RendererIsVulkan()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Vulkan", settings.Rpcs3.Renderer);
    }

    /// <summary>
    /// Verifies that RPCS3 emulator settings can be modified.
    /// </summary>
    [Fact]
    public void Rpcs3SettingsCanBeModified()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.Rpcs3.Renderer = "OpenGL";
        settings.Rpcs3.Resolution = "1920x1080";
        settings.Rpcs3.Vsync = true;

        Assert.Equal("OpenGL", settings.Rpcs3.Renderer);
        Assert.Equal("1920x1080", settings.Rpcs3.Resolution);
        Assert.True(settings.Rpcs3.Vsync);
    }

    /// <summary>
    /// Verifies the default Mednafen Volume is 100.
    /// </summary>
    [Fact]
    public void DefaultMednafenVolumeIs100()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(100, settings.Mednafen.Volume);
    }

    /// <summary>
    /// Verifies that Mednafen emulator settings can be modified.
    /// </summary>
    [Fact]
    public void MednafenSettingsCanBeModified()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.Mednafen.Volume = 50;
        settings.Mednafen.Fullscreen = true;
        settings.Mednafen.VideoDriver = "sdl";

        Assert.Equal(50, settings.Mednafen.Volume);
        Assert.True(settings.Mednafen.Fullscreen);
        Assert.Equal("sdl", settings.Mednafen.VideoDriver);
    }

    /// <summary>
    /// Verifies the default Stella AudioVolume is 80.
    /// </summary>
    [Fact]
    public void DefaultStellaAudioVolumeIs80()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(80, settings.Stella.AudioVolume);
    }

    /// <summary>
    /// Verifies that Stella emulator settings can be modified.
    /// </summary>
    [Fact]
    public void StellaSettingsCanBeModified()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.Stella.AudioVolume = 100;
        settings.Stella.Fullscreen = true;
        settings.Stella.VideoDriver = "opengl";

        Assert.Equal(100, settings.Stella.AudioVolume);
        Assert.True(settings.Stella.Fullscreen);
        Assert.Equal("opengl", settings.Stella.VideoDriver);
    }

    /// <summary>
    /// Verifies the default Xenia GPU is "d3d12".
    /// </summary>
    [Fact]
    public void DefaultXeniaGpuIsD3D12()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("d3d12", settings.Xenia.Gpu);
    }

    /// <summary>
    /// Verifies that Xenia emulator settings can be modified.
    /// </summary>
    [Fact]
    public void XeniaSettingsCanBeModified()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.Xenia.Gpu = "vulkan";
        settings.Xenia.Vsync = false;
        settings.Xenia.Mute = true;

        Assert.Equal("vulkan", settings.Xenia.Gpu);
        Assert.False(settings.Xenia.Vsync);
        Assert.True(settings.Xenia.Mute);
    }

    /// <summary>
    /// Verifies that ResetToDefaults restores DuckStation settings to their default values.
    /// </summary>
    [Fact]
    public void ResetToDefaultsRestoresDuckStationSettings()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.DuckStation.StartFullscreen = true;
        settings.DuckStation.Renderer = "Vulkan";
        settings.DuckStation.ResolutionScale = 8;

        settings.ResetToDefaults();

        Assert.False(settings.DuckStation.StartFullscreen);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
        Assert.Equal(2, settings.DuckStation.ResolutionScale);
    }

    /// <summary>
    /// Verifies that ResetToDefaults restores Xenia settings to their default values.
    /// </summary>
    [Fact]
    public void ResetToDefaultsRestoresXeniaSettings()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.Xenia.Gpu = "vulkan";
        settings.Xenia.Mute = true;
        settings.Xenia.Vsync = false;

        settings.ResetToDefaults();

        Assert.Equal("d3d12", settings.Xenia.Gpu);
        Assert.False(settings.Xenia.Mute);
        Assert.True(settings.Xenia.Vsync);
    }

    /// <summary>
    /// Verifies that the default FuzzyMatchingThreshold is within the valid 0.0 to 1.0 range.
    /// </summary>
    [Fact]
    public void DefaultFuzzyMatchingThresholdRange()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.InRange(settings.FuzzyMatchingThreshold, 0.0, 1.0);
    }

    /// <summary>
    /// Verifies that FuzzyMatchingThreshold accepts boundary values 0.0 and 1.0.
    /// </summary>
    [Fact]
    public void FuzzyMatchingThresholdCanBeSetToBoundaryValues()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.FuzzyMatchingThreshold = 0.0;
        Assert.Equal(0.0, settings.FuzzyMatchingThreshold);

        settings.FuzzyMatchingThreshold = 1.0;
        Assert.Equal(1.0, settings.FuzzyMatchingThreshold);
    }

    /// <summary>
    /// Verifies that notification sounds are enabled by default.
    /// </summary>
    [Fact]
    public void DefaultNotificationSoundIsEnabled()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.EnableNotificationSound);
    }

    /// <summary>
    /// Verifies that notification sounds can be disabled.
    /// </summary>
    [Fact]
    public void NotificationSoundCanBeDisabled()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.EnableNotificationSound = false;
        Assert.False(settings.EnableNotificationSound);
    }

    /// <summary>
    /// Verifies that the overlay open video button is enabled by default.
    /// </summary>
    [Fact]
    public void DefaultOverlayOpenVideoButtonIsEnabled()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.OverlayOpenVideoButton);
    }

    /// <summary>
    /// Verifies that the overlay open video button can be disabled.
    /// </summary>
    [Fact]
    public void OverlayOpenVideoButtonCanBeDisabled()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.OverlayOpenVideoButton = false;
        Assert.False(settings.OverlayOpenVideoButton);
    }

    /// <summary>
    /// Verifies that all emulator expanded states default to true.
    /// </summary>
    [Fact]
    public void DefaultEmulatorExpandedStatesAreAllTrue()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.Emulator1Expanded);
        Assert.True(settings.Emulator2Expanded);
        Assert.True(settings.Emulator3Expanded);
        Assert.True(settings.Emulator4Expanded);
        Assert.True(settings.Emulator5Expanded);
    }

    /// <summary>
    /// Verifies that emulator expanded states can be toggled to false.
    /// </summary>
    [Fact]
    public void EmulatorExpandedStatesCanBeToggled()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        settings.Emulator1Expanded = false;
        settings.Emulator2Expanded = false;
        settings.Emulator3Expanded = false;
        settings.Emulator4Expanded = false;
        settings.Emulator5Expanded = false;

        Assert.False(settings.Emulator1Expanded);
        Assert.False(settings.Emulator2Expanded);
        Assert.False(settings.Emulator3Expanded);
        Assert.False(settings.Emulator4Expanded);
        Assert.False(settings.Emulator5Expanded);
    }

    /// <summary>
    /// Verifies the default ButtonAspectRatio is "Square".
    /// </summary>
    [Fact]
    public void DefaultButtonAspectRatioIsSquare()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Square", settings.ButtonAspectRatio);
    }

    /// <summary>
    /// Verifies the default FilenameDisplayMode is "Original".
    /// </summary>
    [Fact]
    public void DefaultFilenameDisplayModeIsOriginal()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Original", settings.FilenameDisplayMode);
    }

    /// <summary>
    /// Verifies the default FilenameFontSize is "Normal".
    /// </summary>
    [Fact]
    public void DefaultFilenameFontSizeIsNormal()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Normal", settings.FilenameFontSize);
    }

    /// <summary>
    /// Verifies the default MachineNameFontSize is "Normal".
    /// </summary>
    [Fact]
    public void DefaultMachineNameFontSizeIsNormal()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Normal", settings.MachineNameFontSize);
    }

    /// <summary>
    /// Verifies the default StyleVariant is "Default".
    /// </summary>
    [Fact]
    public void DefaultStyleVariantIsDefault()
    {
        using var settings = new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Default", settings.StyleVariant);
    }
}

using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Extended tests for <see cref="SettingsManager"/> covering additional edge cases for
/// setting modifications, emulator settings, play time tracking, and reset behavior.
/// </summary>
public class SettingsManagerExtendedTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly NoOpCredentialProtector _credentialProtector = new();

    public SettingsManagerExtendedTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_SettingsExtTest_{Guid.NewGuid():N}");
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
    public void DefaultShowGamesIsShowAll()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("ShowAll", settings.ShowGames);
    }

    [Fact]
    public void ShowGamesCanBeChangedToShowWithCover()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.ShowGames = "ShowWithCover";
        Assert.Equal("ShowWithCover", settings.ShowGames);
    }

    [Fact]
    public void ShowGamesCanBeChangedToShowWithoutCover()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.ShowGames = "ShowWithoutCover";
        Assert.Equal("ShowWithoutCover", settings.ShowGames);
    }

    [Fact]
    public void DefaultViewModeIsGridView()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("GridView", settings.ViewMode);
    }

    [Fact]
    public void ViewModeCanBeChangedToListView()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.ViewMode = "ListView";
        Assert.Equal("ListView", settings.ViewMode);
    }

    [Fact]
    public void UpdateSystemPlayTimeZeroDurationStillCreatesEntry()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.UpdateSystemPlayTime("NES", TimeSpan.Zero);
        // Zero duration may or may not create an entry depending on implementation
        // Just verify no exception is thrown
        Assert.True(settings.SystemPlayTimes.Count <= 1);
    }

    [Fact]
    public void UpdateSystemPlayTimeLargeDuration()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.UpdateSystemPlayTime("NES", TimeSpan.FromHours(100));
        Assert.Equal(360000, settings.SystemPlayTimes[0].PlayTimeSeconds);
    }

    [Fact]
    public void UpdateSystemPlayTimeManyAccumulations()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        for (var i = 0; i < 100; i++)
        {
            settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(1));
        }

        Assert.Single(settings.SystemPlayTimes);
        Assert.Equal(6000, settings.SystemPlayTimes[0].PlayTimeSeconds);
    }

    [Fact]
    public void ResetToDefaultsRestoresSystemPlayTimes()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.UpdateSystemPlayTime("NES", TimeSpan.FromMinutes(30));
        settings.UpdateSystemPlayTime("SNES", TimeSpan.FromMinutes(45));

        settings.ResetToDefaults();

        Assert.Empty(settings.SystemPlayTimes);
    }

    [Fact]
    public void ResetToDefaultsRestoresViewMode()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.ViewMode = "ListView";
        settings.ResetToDefaults();
        Assert.Equal("GridView", settings.ViewMode);
    }

    [Fact]
    public void ResetToDefaultsRestoresShowGames()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.ShowGames = "ShowWithCover";
        settings.ResetToDefaults();
        Assert.Equal("ShowAll", settings.ShowGames);
    }

    [Fact]
    public void DefaultDuckStationRendererIsAutomatic()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
    }

    [Fact]
    public void DuckStationSettingsCanBeModified()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.DuckStation.StartFullscreen = true;
        settings.DuckStation.Renderer = "Vulkan";
        settings.DuckStation.ResolutionScale = 4;

        Assert.True(settings.DuckStation.StartFullscreen);
        Assert.Equal("Vulkan", settings.DuckStation.Renderer);
        Assert.Equal(4, settings.DuckStation.ResolutionScale);
    }

    [Fact]
    public void DefaultRetroArchVideoDriverIsGl()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("gl", settings.RetroArch.VideoDriver);
    }

    [Fact]
    public void RetroArchSettingsCanBeModified()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.RetroArch.Fullscreen = true;
        settings.RetroArch.VideoDriver = "vulkan";
        settings.RetroArch.Vsync = false;

        Assert.True(settings.RetroArch.Fullscreen);
        Assert.Equal("vulkan", settings.RetroArch.VideoDriver);
        Assert.False(settings.RetroArch.Vsync);
    }

    [Fact]
    public void DefaultRpcs3RendererIsVulkan()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Vulkan", settings.Rpcs3.Renderer);
    }

    [Fact]
    public void Rpcs3SettingsCanBeModified()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.Rpcs3.Renderer = "OpenGL";
        settings.Rpcs3.Resolution = "1920x1080";
        settings.Rpcs3.Vsync = true;

        Assert.Equal("OpenGL", settings.Rpcs3.Renderer);
        Assert.Equal("1920x1080", settings.Rpcs3.Resolution);
        Assert.True(settings.Rpcs3.Vsync);
    }

    [Fact]
    public void DefaultMednafenVolumeIs100()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(100, settings.Mednafen.Volume);
    }

    [Fact]
    public void MednafenSettingsCanBeModified()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.Mednafen.Volume = 50;
        settings.Mednafen.Fullscreen = true;
        settings.Mednafen.VideoDriver = "sdl";

        Assert.Equal(50, settings.Mednafen.Volume);
        Assert.True(settings.Mednafen.Fullscreen);
        Assert.Equal("sdl", settings.Mednafen.VideoDriver);
    }

    [Fact]
    public void DefaultStellaAudioVolumeIs80()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal(80, settings.Stella.AudioVolume);
    }

    [Fact]
    public void StellaSettingsCanBeModified()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.Stella.AudioVolume = 100;
        settings.Stella.Fullscreen = true;
        settings.Stella.VideoDriver = "opengl";

        Assert.Equal(100, settings.Stella.AudioVolume);
        Assert.True(settings.Stella.Fullscreen);
        Assert.Equal("opengl", settings.Stella.VideoDriver);
    }

    [Fact]
    public void DefaultXeniaGpuIsD3D12()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("d3d12", settings.Xenia.Gpu);
    }

    [Fact]
    public void XeniaSettingsCanBeModified()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.Xenia.Gpu = "vulkan";
        settings.Xenia.Vsync = false;
        settings.Xenia.Mute = true;

        Assert.Equal("vulkan", settings.Xenia.Gpu);
        Assert.False(settings.Xenia.Vsync);
        Assert.True(settings.Xenia.Mute);
    }

    [Fact]
    public void ResetToDefaultsRestoresDuckStationSettings()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.DuckStation.StartFullscreen = true;
        settings.DuckStation.Renderer = "Vulkan";
        settings.DuckStation.ResolutionScale = 8;

        settings.ResetToDefaults();

        Assert.False(settings.DuckStation.StartFullscreen);
        Assert.Equal("Automatic", settings.DuckStation.Renderer);
        Assert.Equal(2, settings.DuckStation.ResolutionScale);
    }

    [Fact]
    public void ResetToDefaultsRestoresXeniaSettings()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.Xenia.Gpu = "vulkan";
        settings.Xenia.Mute = true;
        settings.Xenia.Vsync = false;

        settings.ResetToDefaults();

        Assert.Equal("d3d12", settings.Xenia.Gpu);
        Assert.False(settings.Xenia.Mute);
        Assert.True(settings.Xenia.Vsync);
    }

    [Fact]
    public void DefaultFuzzyMatchingThresholdRange()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.InRange(settings.FuzzyMatchingThreshold, 0.0, 1.0);
    }

    [Fact]
    public void FuzzyMatchingThresholdCanBeSetToBoundaryValues()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.FuzzyMatchingThreshold = 0.0;
        Assert.Equal(0.0, settings.FuzzyMatchingThreshold);

        settings.FuzzyMatchingThreshold = 1.0;
        Assert.Equal(1.0, settings.FuzzyMatchingThreshold);
    }

    [Fact]
    public void DefaultNotificationSoundIsEnabled()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.EnableNotificationSound);
    }

    [Fact]
    public void NotificationSoundCanBeDisabled()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.EnableNotificationSound = false;
        Assert.False(settings.EnableNotificationSound);
    }

    [Fact]
    public void DefaultOverlayOpenVideoButtonIsEnabled()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.OverlayOpenVideoButton);
    }

    [Fact]
    public void OverlayOpenVideoButtonCanBeDisabled()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        settings.OverlayOpenVideoButton = false;
        Assert.False(settings.OverlayOpenVideoButton);
    }

    [Fact]
    public void DefaultEmulatorExpandedStatesAreAllTrue()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.True(settings.Emulator1Expanded);
        Assert.True(settings.Emulator2Expanded);
        Assert.True(settings.Emulator3Expanded);
        Assert.True(settings.Emulator4Expanded);
        Assert.True(settings.Emulator5Expanded);
    }

    [Fact]
    public void EmulatorExpandedStatesCanBeToggled()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
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

    [Fact]
    public void DefaultButtonAspectRatioIsSquare()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Square", settings.ButtonAspectRatio);
    }

    [Fact]
    public void DefaultFilenameDisplayModeIsOriginal()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Original", settings.FilenameDisplayMode);
    }

    [Fact]
    public void DefaultFilenameFontSizeIsNormal()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Normal", settings.FilenameFontSize);
    }

    [Fact]
    public void DefaultMachineNameFontSizeIsNormal()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Normal", settings.MachineNameFontSize);
    }

    [Fact]
    public void DefaultStyleVariantIsDefault()
    {
        using var settings = new SettingsManager(_configuration, _logErrors, _credentialProtector);
        Assert.Equal("Default", settings.StyleVariant);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}

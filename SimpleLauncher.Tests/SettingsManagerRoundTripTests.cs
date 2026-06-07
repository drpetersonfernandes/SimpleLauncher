using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class SettingsManagerRoundTripTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();

    public SettingsManagerRoundTripTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_SettingsRT_{Guid.NewGuid():N}");
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
            // ignored
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void SettingsManagerAllPropertiesCanBeModified()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.ThumbnailSize = 300;
        Assert.Equal(300, settings.ThumbnailSize);

        settings.GamesPerPage = 500;
        Assert.Equal(500, settings.GamesPerPage);

        settings.ShowGames = "ShowWithCover";
        Assert.Equal("ShowWithCover", settings.ShowGames);

        settings.ViewMode = "ListView";
        Assert.Equal("ListView", settings.ViewMode);

        settings.BaseTheme = "Light";
        Assert.Equal("Light", settings.BaseTheme);

        settings.AccentColor = "Red";
        Assert.Equal("Red", settings.AccentColor);

        settings.Language = "pt-BR";
        Assert.Equal("pt-BR", settings.Language);
    }

    [Fact]
    public void SettingsManagerBooleanProperties()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.EnableFuzzyMatching = false;
        Assert.False(settings.EnableFuzzyMatching);

        settings.EnableNotificationSound = false;
        Assert.False(settings.EnableNotificationSound);

        settings.OverlayOpenVideoButton = false;
        Assert.False(settings.OverlayOpenVideoButton);

        settings.AdditionalSystemFoldersExpanded = false;
        Assert.False(settings.AdditionalSystemFoldersExpanded);
    }

    [Fact]
    public void SettingsManagerEmulatorExpandedStates()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.Emulator1Expanded = false;
        Assert.False(settings.Emulator1Expanded);

        settings.Emulator2Expanded = false;
        Assert.False(settings.Emulator2Expanded);

        settings.Emulator3Expanded = false;
        Assert.False(settings.Emulator3Expanded);

        settings.Emulator4Expanded = false;
        Assert.False(settings.Emulator4Expanded);

        settings.Emulator5Expanded = false;
        Assert.False(settings.Emulator5Expanded);
    }

    [Fact]
    public void SettingsManagerDuckStationSettingsModifiable()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.DuckStation.StartFullscreen = true;
        settings.DuckStation.Renderer = "Vulkan";
        settings.DuckStation.ResolutionScale = 4;
        settings.DuckStation.OutputVolume = 50;

        Assert.True(settings.DuckStation.StartFullscreen);
        Assert.Equal("Vulkan", settings.DuckStation.Renderer);
        Assert.Equal(4, settings.DuckStation.ResolutionScale);
        Assert.Equal(50, settings.DuckStation.OutputVolume);
    }

    [Fact]
    public void SettingsManagerRetroArchSettingsModifiable()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.RetroArch.Fullscreen = true;
        settings.RetroArch.VideoDriver = "vulkan";
        settings.RetroArch.Vsync = false;
        settings.RetroArch.MenuDriver = "xmb";

        Assert.True(settings.RetroArch.Fullscreen);
        Assert.Equal("vulkan", settings.RetroArch.VideoDriver);
        Assert.False(settings.RetroArch.Vsync);
        Assert.Equal("xmb", settings.RetroArch.MenuDriver);
    }

    [Fact]
    public void SettingsManagerFuzzyMatchingThresholdRange()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.FuzzyMatchingThreshold = 0.0;
        Assert.Equal(0.0, settings.FuzzyMatchingThreshold);

        settings.FuzzyMatchingThreshold = 1.0;
        Assert.Equal(1.0, settings.FuzzyMatchingThreshold);

        settings.FuzzyMatchingThreshold = 0.5;
        Assert.Equal(0.5, settings.FuzzyMatchingThreshold);
    }

    [Fact]
    public void SettingsManagerThumbnailSizeRange()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.ThumbnailSize = 50;
        Assert.Equal(50, settings.ThumbnailSize);

        settings.ThumbnailSize = 500;
        Assert.Equal(500, settings.ThumbnailSize);

        settings.ThumbnailSize = 1000;
        Assert.Equal(1000, settings.ThumbnailSize);
    }

    [Fact]
    public void SettingsManagerStyleVariant()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.StyleVariant = "Compact";
        Assert.Equal("Compact", settings.StyleVariant);

        settings.StyleVariant = "Default";
        Assert.Equal("Default", settings.StyleVariant);
    }

    [Fact]
    public void SettingsManagerFilenameDisplayMode()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.FilenameDisplayMode = "CleanUp";
        Assert.Equal("CleanUp", settings.FilenameDisplayMode);

        settings.FilenameDisplayMode = "NoFilename";
        Assert.Equal("NoFilename", settings.FilenameDisplayMode);

        settings.FilenameDisplayMode = "Original";
        Assert.Equal("Original", settings.FilenameDisplayMode);
    }

    [Fact]
    public void SettingsManagerFilenameFontSize()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.FilenameFontSize = "Small";
        Assert.Equal("Small", settings.FilenameFontSize);

        settings.FilenameFontSize = "Big";
        Assert.Equal("Big", settings.FilenameFontSize);

        settings.FilenameFontSize = "Normal";
        Assert.Equal("Normal", settings.FilenameFontSize);
    }

    [Fact]
    public void SettingsManagerButtonAspectRatio()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.ButtonAspectRatio = "Wider";
        Assert.Equal("Wider", settings.ButtonAspectRatio);

        settings.ButtonAspectRatio = "Taller";
        Assert.Equal("Taller", settings.ButtonAspectRatio);

        settings.ButtonAspectRatio = "Square";
        Assert.Equal("Square", settings.ButtonAspectRatio);
    }

    [Fact]
    public void SettingsManagerSystemPlayTimesCanBeAdded()
    {
        using var settings = new Services.SettingsManager.SettingsManager(_configuration, _logErrors);

        settings.SystemPlayTimes.Add(new SystemPlayTime
        {
            SystemName = "NES",
            PlayTimeSeconds = 3600
        });

        Assert.Single(settings.SystemPlayTimes);
        Assert.Equal("NES", settings.SystemPlayTimes[0].SystemName);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}

using SimpleLauncher.Services.EasyMode.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="EmulatorConfig"/> property defaults, getters, and setters.
/// </summary>
public class EmulatorConfigTests
{
    /// <summary>
    /// Verifies that all properties on EmulatorConfig default to null.
    /// </summary>
    [Fact]
    public void AllPropertiesDefaultToNull()
    {
        var config = new EmulatorConfig();

        Assert.Null(config.EmulatorName);
        Assert.Null(config.EmulatorLocation);
        Assert.Null(config.EmulatorParameters);
        Assert.Null(config.EmulatorDownloadPage);
        Assert.Null(config.EmulatorLatestVersion);
        Assert.Null(config.EmulatorDownloadLink);
        Assert.Null(config.EmulatorDownloadExtractPath);
        Assert.Null(config.CoreLocation);
        Assert.Null(config.CoreLatestVersion);
        Assert.Null(config.CoreDownloadLink);
        Assert.Null(config.CoreDownloadExtractPath);
        Assert.Null(config.ImagePackLocation);
        Assert.Null(config.ImagePackLatestVersion);
        Assert.Null(config.ImagePackDownloadLink);
        Assert.Null(config.ImagePackDownloadLink2);
        Assert.Null(config.ImagePackDownloadLink3);
        Assert.Null(config.ImagePackDownloadLink4);
        Assert.Null(config.ImagePackDownloadLink5);
        Assert.Null(config.ImagePackDownloadExtractPath);
    }

    /// <summary>
    /// Verifies that all properties can be set via object initializer and retrieved correctly.
    /// </summary>
    [Fact]
    public void PropertiesCanBeSet()
    {
        var config = new EmulatorConfig
        {
            EmulatorName = "RetroArch",
            EmulatorLocation = @"C:\emulators\retroarch.exe",
            EmulatorParameters = "-L core.dll",
            EmulatorDownloadPage = "https://retroarch.com",
            EmulatorLatestVersion = "1.19.0",
            EmulatorDownloadLink = "https://example.com/retroarch.7z",
            EmulatorDownloadExtractPath = @"C:\emulators",
            CoreLocation = @"C:\emulators\cores\nes.dll",
            CoreLatestVersion = "1.0",
            CoreDownloadLink = "https://example.com/nes.7z",
            CoreDownloadExtractPath = @"C:\emulators\cores",
            ImagePackLocation = @"C:\images\nes",
            ImagePackLatestVersion = "1.0",
            ImagePackDownloadLink = "https://example.com/images.7z",
            ImagePackDownloadLink2 = "https://example.com/images2.7z",
            ImagePackDownloadLink3 = "https://example.com/images3.7z",
            ImagePackDownloadLink4 = "https://example.com/images4.7z",
            ImagePackDownloadLink5 = "https://example.com/images5.7z",
            ImagePackDownloadExtractPath = @"C:\images"
        };

        Assert.Equal("RetroArch", config.EmulatorName);
        Assert.Equal(@"C:\emulators\retroarch.exe", config.EmulatorLocation);
        Assert.Equal("-L core.dll", config.EmulatorParameters);
        Assert.Equal("https://retroarch.com", config.EmulatorDownloadPage);
        Assert.Equal("1.19.0", config.EmulatorLatestVersion);
        Assert.Equal("https://example.com/retroarch.7z", config.EmulatorDownloadLink);
        Assert.Equal(@"C:\emulators", config.EmulatorDownloadExtractPath);
        Assert.Equal(@"C:\emulators\cores\nes.dll", config.CoreLocation);
        Assert.Equal("1.0", config.CoreLatestVersion);
        Assert.Equal("https://example.com/nes.7z", config.CoreDownloadLink);
        Assert.Equal(@"C:\emulators\cores", config.CoreDownloadExtractPath);
        Assert.Equal(@"C:\images\nes", config.ImagePackLocation);
        Assert.Equal("1.0", config.ImagePackLatestVersion);
        Assert.Equal("https://example.com/images.7z", config.ImagePackDownloadLink);
        Assert.Equal("https://example.com/images2.7z", config.ImagePackDownloadLink2);
        Assert.Equal("https://example.com/images3.7z", config.ImagePackDownloadLink3);
        Assert.Equal("https://example.com/images4.7z", config.ImagePackDownloadLink4);
        Assert.Equal("https://example.com/images5.7z", config.ImagePackDownloadLink5);
        Assert.Equal(@"C:\images", config.ImagePackDownloadExtractPath);
    }

    /// <summary>
    /// Verifies that properties can be modified after the object is created.
    /// </summary>
    [Fact]
    public void PropertiesCanBeChangedAfterCreation()
    {
        var config = new EmulatorConfig
        {
            EmulatorName = "Old Name",
            EmulatorLatestVersion = "1.0"
        };

        config.EmulatorName = "New Name";
        config.EmulatorLatestVersion = "2.0";

        Assert.Equal("New Name", config.EmulatorName);
        Assert.Equal("2.0", config.EmulatorLatestVersion);
    }
}

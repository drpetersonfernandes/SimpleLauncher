using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.FindCoverImage;
using SimpleLauncher.Core.Services.SettingsManager;
using Serilog;

namespace SimpleLauncher.New.Tests;

/// <summary>
/// Verifies that cover art is found in the system's image folder — the lookup
/// the game grid relies on for card artwork. The Core service always returns a
/// path (falling back to default.png), so callers must check File.Exists.
/// </summary>
public class CoverImageTests
{
    private static FindCoverImageService CreateService()
    {
        Log.Logger ??= new LoggerConfiguration().CreateLogger();

        var configuration = new ConfigurationBuilder().Build();
        var settings = new SettingsManagerService(
            configuration,
            new Mock<ILogger>().Object,
            new Mock<ICredentialProtector>().Object);

        return new FindCoverImageService(configuration, new Mock<ILogger>().Object, settings);
    }

    [Fact]
    public void FindCoverImagePath_FindsExactMatchInSystemImageFolder()
    {
        var service = CreateService();
        var baseDir = Path.Combine(Path.GetTempPath(), $"sln_cover_{Guid.NewGuid():N}");
        var imageFolder = Path.Combine(baseDir, "images", "Test System");
        Directory.CreateDirectory(imageFolder);
        try
        {
            File.WriteAllText(Path.Combine(imageFolder, "Combat.png"), "fake");

            var result = service.FindCoverImagePath("Combat", "Test System", imageFolder);

            Assert.Equal(Path.Combine(imageFolder, "Combat.png"), result);
            Assert.True(File.Exists(result));
        }
        finally
        {
            try
            {
                Directory.Delete(baseDir, true);
            }
            catch
            {
                /* best effort */
            }
        }
    }

    [Fact]
    public void FindCoverImagePath_NoImage_FallsBackToDefaultPng()
    {
        var service = CreateService();
        var imageFolder = Path.Combine(Path.GetTempPath(), $"sln_cover_{Guid.NewGuid():N}");
        Directory.CreateDirectory(imageFolder);
        try
        {
            var result = service.FindCoverImagePath("MissingGame", "Test System", imageFolder);

            // The Core contract: never empty — always a default.png fallback path.
            // The grid then checks File.Exists to decide image vs placeholder.
            Assert.False(string.IsNullOrEmpty(result));
            Assert.Contains("default.png", result, StringComparison.OrdinalIgnoreCase);
            Assert.False(File.Exists(Path.Combine(imageFolder, "MissingGame.png")));
        }
        finally
        {
            try
            {
                Directory.Delete(imageFolder, true);
            }
            catch
            {
                /* best effort */
            }
        }
    }

    [Fact]
    public void FindCoverImagePath_NoImageFolder_StillReturnsDefaultPath()
    {
        var service = CreateService();
        var missingFolder = Path.Combine(Path.GetTempPath(), $"sln_cover_missing_{Guid.NewGuid():N}");

        var result = service.FindCoverImagePath("MissingGame", "Ghost System", missingFolder);

        Assert.False(string.IsNullOrEmpty(result));
        Assert.Contains("default.png", result, StringComparison.OrdinalIgnoreCase);
    }
}

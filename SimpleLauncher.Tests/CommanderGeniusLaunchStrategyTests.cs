using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.GameLauncher.Strategies;
using SimpleLauncher.Services.NotificationToast;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="CommanderGeniusLaunchStrategy"/> class.
/// </summary>
public class CommanderGeniusLaunchStrategyTests
{
    private static CommanderGeniusLaunchStrategy CreateStrategy()
    {
        var extractionServiceMock = new Mock<IExtractionService>();
        var configurationMock = new Mock<IConfiguration>();
        var updateStatusBarMock = new Mock<IUpdateStatusBar>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var debugLoggerMock = new Mock<ILogger>();
        var toastNotificationServiceMock = new Mock<IToastNotificationService>();

        return new CommanderGeniusLaunchStrategy(
            extractionServiceMock.Object,
            configurationMock.Object,
            updateStatusBarMock.Object,
            messageBoxMock.Object,
            debugLoggerMock.Object,
            toastNotificationServiceMock.Object);
    }

    /// <summary>
    /// Verifies that the strategy has a priority of 20.
    /// </summary>
    [Fact]
    public void PriorityIs20()
    {
        var strategy = CreateStrategy();
        Assert.Equal(20, strategy.Priority);
    }

    /// <summary>
    /// Verifies that IsMatch returns false when the emulator name is empty.
    /// </summary>
    [Fact]
    public void IsMatchEmptyEmulatorNameReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\games\keen.zip",
            EmulatorName = ""
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false when the file path is empty.
    /// </summary>
    [Fact]
    public void IsMatchEmptyFilePathReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = "",
            EmulatorName = "Commander Genius"
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false for a non-Commander Genius emulator.
    /// </summary>
    [Fact]
    public void IsMatchNonCommanderGeniusEmulatorReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\games\keen.zip",
            EmulatorName = "Mesen"
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true for archive files paired with the Commander Genius emulator.
    /// </summary>
    /// <param name="extension">The archive file extension to test.</param>
    [Theory]
    [InlineData(".zip")]
    [InlineData(".7z")]
    [InlineData(".rar")]
    public void IsMatchArchiveWithCommanderGeniusReturnsTrue(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\games\keen{extension}",
            EmulatorName = "Commander Genius"
        };

        Assert.True(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true for archive files with uppercase extensions.
    /// </summary>
    /// <param name="extension">The uppercase archive file extension to test.</param>
    [Theory]
    [InlineData(".ZIP")]
    [InlineData(".7Z")]
    [InlineData(".RAR")]
    public void IsMatchArchiveUpperCaseReturnsTrue(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\games\keen{extension}",
            EmulatorName = "Commander Genius"
        };

        Assert.True(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true for various Commander Genius name variants.
    /// </summary>
    /// <param name="emulatorName">The Commander Genius emulator name variant to test.</param>
    [Theory]
    [InlineData("Commander Genius")]
    [InlineData("commander genius")]
    [InlineData("COMMANDER GENIUS")]
    [InlineData("My Commander Genius Emulator")]
    public void IsMatchCommanderGeniusNameVariants(string emulatorName)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\games\keen.zip",
            EmulatorName = emulatorName
        };

        Assert.True(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false for non-archive file extensions.
    /// </summary>
    /// <param name="extension">The non-archive file extension to test.</param>
    [Theory]
    [InlineData(".exe")]
    [InlineData(".bat")]
    [InlineData(".iso")]
    [InlineData(".bin")]
    [InlineData(".nes")]
    [InlineData(".sfc")]
    public void IsMatchNonArchiveExtensionReturnsFalse(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\games\keen{extension}",
            EmulatorName = "Commander Genius"
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false for a directory path.
    /// </summary>
    [Fact]
    public void IsMatchDirectoryReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\games\keen",
            EmulatorName = "Commander Genius"
        };

        Assert.False(strategy.IsMatch(context));
    }
}

using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
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
        var logErrorsMock = new Mock<ILogErrors>();
        var updateStatusBarMock = new Mock<IUpdateStatusBar>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var debugLoggerMock = new Mock<IDebugLogger>();

        return new CommanderGeniusLaunchStrategy(
            extractionServiceMock.Object,
            configurationMock.Object,
            logErrorsMock.Object,
            updateStatusBarMock.Object,
            messageBoxMock.Object,
            debugLoggerMock.Object);
    }

    [Fact]
    public void PriorityIs20()
    {
        var strategy = CreateStrategy();
        Assert.Equal(20, strategy.Priority);
    }

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

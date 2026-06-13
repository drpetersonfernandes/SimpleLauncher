using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using SimpleLauncher.Services.SystemManager;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="PbpToCueStrategy"/> class.
/// </summary>
public class PbpToCueStrategyTests
{
    private static PbpToCueStrategy CreateStrategy()
    {
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var debugLoggerMock = new Mock<IDebugLogger>();
        var discConverterMock = new Mock<IDiscConverter>();

        return new PbpToCueStrategy(
            messageBoxMock.Object,
            debugLoggerMock.Object,
            discConverterMock.Object);
    }

    [Fact]
    public void PriorityIs15()
    {
        var strategy = CreateStrategy();
        Assert.Equal(15, strategy.Priority);
    }

    [Fact]
    public void IsMatchEmptyFilePathReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = "",
            EmulatorName = "Mednafen"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchEmptyEmulatorNameReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\psp\game.pbp",
            EmulatorName = ""
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchNonPbpFileReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\psp\game.iso",
            EmulatorName = "Mednafen"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchNonMednafenEmulatorReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\psp\game.pbp",
            EmulatorName = "PPSSPP"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Theory]
    [InlineData("Mednafen")]
    [InlineData("mednafen")]
    [InlineData("MEDNAFEN")]
    public void IsMatchMednafenWithPbpReturnsTrue(string emulatorName)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\psp\game.pbp",
            EmulatorName = emulatorName
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchMednafenByEmulatorLocationReturnsTrue()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\psp\game.pbp",
            EmulatorName = "SomeEmulator",
            EmulatorManager = new Emulator { EmulatorLocation = @"C:\emu\mednafen.exe" }
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchPbpExtensionCaseInsensitive()
    {
        var strategy = CreateStrategy();
        var context1 = new LaunchContext
        {
            ResolvedFilePath = @"C:\psp\game.pbp",
            EmulatorName = "Mednafen"
        };
        var context2 = new LaunchContext
        {
            ResolvedFilePath = @"C:\psp\game.PBP",
            EmulatorName = "Mednafen"
        };
        var context3 = new LaunchContext
        {
            ResolvedFilePath = @"C:\psp\game.Pbp",
            EmulatorName = "Mednafen"
        };

        Assert.True(strategy.IsMatch(context1));
        Assert.True(strategy.IsMatch(context2));
        Assert.True(strategy.IsMatch(context3));
    }

    [Theory]
    [InlineData(".iso")]
    [InlineData(".bin")]
    [InlineData(".cue")]
    [InlineData(".zip")]
    [InlineData(".7z")]
    public void IsMatchNonPbpExtensionReturnsFalse(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\psp\game{extension}",
            EmulatorName = "Mednafen"
        };

        Assert.False(strategy.IsMatch(context));
    }
}

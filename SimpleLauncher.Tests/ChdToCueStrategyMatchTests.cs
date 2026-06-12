using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using SimpleLauncher.Services.SystemManager;
using Xunit;

namespace SimpleLauncher.Tests;

public class ChdToCueStrategyMatchTests
{
    private static ChdToCueStrategy CreateStrategy()
    {
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var debugLoggerMock = new Mock<IDebugLogger>();
        var discConverterMock = new Mock<IDiscConverter>();

        return new ChdToCueStrategy(
            messageBoxMock.Object,
            debugLoggerMock.Object,
            discConverterMock.Object);
    }

    [Fact]
    public void IsMatchEmptyEmulatorNameReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
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
            EmulatorName = "4DO"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchNonChdFileReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.iso",
            EmulatorName = "4DO"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchUnsupportedEmulatorReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "Mesen"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatch4DoReturnsTrue()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "4DO"
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchRaineReturnsTrue()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "Raine"
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatch4DoByEmulatorLocationReturnsTrue()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "SomeLauncher",
            EmulatorManager = new Emulator { EmulatorLocation = @"C:\emu\4do.exe" }
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchRaineByEmulatorLocationReturnsTrue()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "CustomFrontend",
            EmulatorManager = new Emulator { EmulatorLocation = @"C:\emu\raine.exe" }
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void PriorityIs25()
    {
        var strategy = CreateStrategy();
        Assert.Equal(25, strategy.Priority);
    }
}

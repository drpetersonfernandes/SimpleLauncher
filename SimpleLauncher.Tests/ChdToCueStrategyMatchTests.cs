using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="ChdToCueStrategy.IsMatch"/> method for CHD-to-CUE conversion detection.
/// </summary>
public class ChdToCueStrategyMatchTests
{
    private static ChdToCueStrategy CreateStrategy()
    {
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var debugLoggerMock = new Mock<ILogger>();
        var discConverterMock = new Mock<IDiscConverter>();

        return new ChdToCueStrategy(
            messageBoxMock.Object,
            debugLoggerMock.Object,
            discConverterMock.Object);
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
            ResolvedFilePath = @"C:\roms\game.chd",
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
            EmulatorName = "4DO"
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false for non-CHD file extensions.
    /// </summary>
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

    /// <summary>
    /// Verifies that IsMatch returns false for an unsupported emulator name.
    /// </summary>
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

    /// <summary>
    /// Verifies that IsMatch returns true for CHD files with the 4DO emulator.
    /// </summary>
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

    /// <summary>
    /// Verifies that IsMatch returns true for CHD files with the Raine emulator.
    /// </summary>
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

    /// <summary>
    /// Verifies that IsMatch returns true when the emulator location points to a 4DO executable.
    /// </summary>
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

    /// <summary>
    /// Verifies that IsMatch returns true when the emulator location points to a Raine executable.
    /// </summary>
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

    /// <summary>
    /// Verifies that the strategy has a priority of 25.
    /// </summary>
    [Fact]
    public void PriorityIs25()
    {
        var strategy = CreateStrategy();
        Assert.Equal(25, strategy.Priority);
    }
}

using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
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
        var debugLoggerMock = new Mock<ILogger>();
        var discConverterMock = new Mock<IDiscConverter>();

        return new PbpToCueStrategy(
            messageBoxMock.Object,
            debugLoggerMock.Object,
            discConverterMock.Object);
    }

    /// <summary>
    /// Verifies that the strategy has a priority of 15.
    /// </summary>
    [Fact]
    public void PriorityIs15()
    {
        var strategy = CreateStrategy();
        Assert.Equal(15, strategy.Priority);
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
            EmulatorName = "Mednafen"
        };

        Assert.False(strategy.IsMatch(context));
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
            ResolvedFilePath = @"C:\psp\game.pbp",
            EmulatorName = ""
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false for non-PBP file extensions.
    /// </summary>
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

    /// <summary>
    /// Verifies that IsMatch returns false for a non-Mednafen emulator.
    /// </summary>
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

    /// <summary>
    /// Verifies that IsMatch returns true for PBP files paired with Mednafen emulator variants.
    /// </summary>
    /// <param name="emulatorName">The Mednafen emulator name variant to test.</param>
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

    /// <summary>
    /// Verifies that IsMatch returns true when the emulator location points to a Mednafen executable.
    /// </summary>
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

    /// <summary>
    /// Verifies that PBP file extension matching is case-insensitive.
    /// </summary>
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

    /// <summary>
    /// Verifies that IsMatch returns false for non-PBP file extensions.
    /// </summary>
    /// <param name="extension">The non-PBP file extension to test.</param>
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

using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="ChdMountStrategy.IsMatch"/> method for CHD file detection with various emulators.
/// </summary>
public class ChdMountStrategyMatchTests
{
    private static ChdMountStrategy CreateStrategy()
    {
        var configurationMock = new Mock<IConfiguration>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var mountChdMock = new Mock<IMountChdFiles>();
        var debugLoggerMock = new Mock<ILogger>();

        return new ChdMountStrategy(
            configurationMock.Object,
            messageBoxMock.Object,
            mountChdMock.Object,
            debugLoggerMock.Object);
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
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "4DO"
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false when the emulator is RetroArch.
    /// </summary>
    [Fact]
    public void IsMatchRetroArchReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "RetroArch"
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false when the emulator location points to a RetroArch executable.
    /// </summary>
    [Fact]
    public void IsMatchRetroArchByEmulatorLocationReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "CustomEmu",
            EmulatorManager = new Emulator { EmulatorLocation = @"C:\emu\retroarch.exe" }
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns false when the emulator is a DOSBox variant.
    /// </summary>
    [Fact]
    public void IsMatchDosBoxReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "DOSBox-X",
            EmulatorManager = new Emulator { EmulatorLocation = @"C:\emu\dosbox-x.exe" }
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true for a CHD file combined with any of the supported emulators.
    /// </summary>
    /// <param name="emulatorName">The supported emulator name to test.</param>
    [Theory]
    [InlineData("4DO")]
    [InlineData("BlastEm")]
    [InlineData("CDiEmu")]
    [InlineData("Cxbx-Reloaded")]
    [InlineData("FB Alpha")]
    [InlineData("FBNeo")]
    [InlineData("Gens")]
    [InlineData("Mednafen")]
    [InlineData("Mesen")]
    [InlineData("Nebula")]
    [InlineData("PCSX-Redux")]
    [InlineData("PicoDrive")]
    [InlineData("Raine")]
    [InlineData("RPCS3")]
    [InlineData("Tsugaru")]
    [InlineData("Xemu")]
    [InlineData("Xenia")]
    [InlineData("Yabause")]
    [InlineData("Genesis Plus GX")]
    public void IsMatchChdWithSupportedEmulatorReturnsTrue(string emulatorName)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = emulatorName
        };

        Assert.True(strategy.IsMatch(context));
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
            EmulatorName = "UnknownEmulator"
        };

        Assert.False(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true for every recognized CDiEmu emulator name variant.
    /// </summary>
    /// <param name="emulatorName">The CDiEmu name variant to test.</param>
    [Theory]
    [InlineData("CDiEmu")]
    [InlineData("CDi Emu")]
    [InlineData("CDi-Emu")]
    [InlineData("CDiEmulator")]
    [InlineData("CDi Emulator")]
    [InlineData("CDi-Emulator")]
    public void IsMatchCdiEmuNameVariants(string emulatorName)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = emulatorName
        };

        Assert.True(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true when the emulator location points to a CDiEmu executable.
    /// </summary>
    [Fact]
    public void IsMatchCdiEmuByLocation()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "SomeEmu",
            EmulatorManager = new Emulator { EmulatorLocation = @"C:\emu\wcdiemu-v053b9.exe" }
        };

        Assert.True(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true for every recognized FB Alpha emulator name variant.
    /// </summary>
    /// <param name="emulatorName">The FB Alpha name variant to test.</param>
    [Theory]
    [InlineData("FBAlpha")]
    [InlineData("FB Alpha")]
    [InlineData("FinalBurnAlpha")]
    [InlineData("Final Burn Alpha")]
    [InlineData("FinalBurn Alpha")]
    public void IsMatchFbAlphaNameVariants(string emulatorName)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = emulatorName
        };

        Assert.True(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true for every recognized FBNeo emulator name variant.
    /// </summary>
    /// <param name="emulatorName">The FBNeo name variant to test.</param>
    [Theory]
    [InlineData("FBNeo")]
    [InlineData("FB Neo")]
    [InlineData("FinalBurnNeo")]
    [InlineData("Final Burn Neo")]
    [InlineData("FinalBurn Neo")]
    public void IsMatchFbNeoNameVariants(string emulatorName)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = emulatorName
        };

        Assert.True(strategy.IsMatch(context));
    }

    /// <summary>
    /// Verifies that IsMatch returns true for PCSX-Redux and PCSX Redux name variants.
    /// </summary>
    [Fact]
    public void IsMatchPcsxReduxNameVariants()
    {
        var strategy = CreateStrategy();
        var context1 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "PCSX-Redux"
        };
        var context2 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.chd",
            EmulatorName = "PCSX Redux"
        };

        Assert.True(strategy.IsMatch(context1));
        Assert.True(strategy.IsMatch(context2));
    }

    /// <summary>
    /// Verifies that the strategy has a priority of 10.
    /// </summary>
    [Fact]
    public void PriorityIs10()
    {
        var strategy = CreateStrategy();
        Assert.Equal(10, strategy.Priority);
    }
}

using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using SimpleLauncher.Services.SystemManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class ChdMountStrategyMatchTests
{
    private static ChdMountStrategy CreateStrategy()
    {
        var configurationMock = new Mock<IConfiguration>();
        var logErrorsMock = new Mock<ILogErrors>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var mountChdMock = new Mock<IMountChdFiles>();
        var debugLoggerMock = new Mock<NoOpDebugLogger>();

        return new ChdMountStrategy(
            configurationMock.Object,
            logErrorsMock.Object,
            messageBoxMock.Object,
            mountChdMock.Object,
            debugLoggerMock.Object);
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
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "4DO"
        };

        Assert.False(strategy.IsMatch(context));
    }

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

    [Fact]
    public void PriorityIs10()
    {
        var strategy = CreateStrategy();
        Assert.Equal(10, strategy.Priority);
    }
}

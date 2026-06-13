using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using SimpleLauncher.Services.SystemManager;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="DosBoxLaunchStrategy"/> class.
/// </summary>
public class DosBoxLaunchStrategyTests
{
    private static DosBoxLaunchStrategy CreateStrategy()
    {
        var extractionServiceMock = new Mock<IExtractionService>();
        var configurationMock = new Mock<IConfiguration>();
        var logErrorsMock = new Mock<ILogErrors>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var mountChdFilesMock = new Mock<IMountChdFiles>();
        var mountIsoFilesMock = new Mock<IMountIsoFiles>();
        var debugLoggerMock = new Mock<IDebugLogger>();

        return new DosBoxLaunchStrategy(
            extractionServiceMock.Object,
            configurationMock.Object,
            logErrorsMock.Object,
            messageBoxMock.Object,
            mountChdFilesMock.Object,
            mountIsoFilesMock.Object,
            debugLoggerMock.Object);
    }

    [Fact]
    public void PriorityIs25()
    {
        var strategy = CreateStrategy();
        Assert.Equal(25, strategy.Priority);
    }

    [Fact]
    public void IsMatchEmptyEmulatorNameReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\dos\game.zip",
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
            EmulatorName = "DOSBox-X"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchNonDosBoxEmulatorReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\dos\game.zip",
            EmulatorName = "Mesen"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Theory]
    [InlineData("DOSBox-X")]
    [InlineData("DOSBox")]
    [InlineData("dosbox")]
    [InlineData("DOSBox Staging")]
    [InlineData("dosbox_pure")]
    public void IsMatchDosBoxEmulatorWithArchiveReturnsTrue(string emulatorName)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\dos\game.zip",
            EmulatorName = emulatorName
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Theory]
    [InlineData(".zip")]
    [InlineData(".7z")]
    [InlineData(".rar")]
    [InlineData(".iso")]
    [InlineData(".chd")]
    public void IsMatchDosBoxWithSupportedFormatsReturnsTrue(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\dos\game{extension}",
            EmulatorName = "DOSBox-X"
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Theory]
    [InlineData(".exe")]
    [InlineData(".bat")]
    [InlineData(".nes")]
    [InlineData(".sfc")]
    public void IsMatchDosBoxWithUnsupportedFormatsReturnsFalse(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\dos\game{extension}",
            EmulatorName = "DOSBox-X"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchDosBoxByEmulatorLocationReturnsTrue()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\dos\game.zip",
            EmulatorName = "SomeEmulator",
            EmulatorManager = new Emulator { EmulatorLocation = @"C:\emu\dosbox-x.exe" }
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsDosBoxEmulatorWithDosBoxNameReturnsTrue()
    {
        var context = new LaunchContext { EmulatorName = "DOSBox-X" };
        Assert.True(DosBoxLaunchStrategy.IsDosBoxEmulator(context));
    }

    [Fact]
    public void IsDosBoxEmulatorWithDosBoxPathReturnsTrue()
    {
        var context = new LaunchContext
        {
            EmulatorName = "CustomEmu",
            EmulatorManager = new Emulator { EmulatorLocation = @"C:\emu\dosbox.exe" }
        };
        Assert.True(DosBoxLaunchStrategy.IsDosBoxEmulator(context));
    }

    [Fact]
    public void IsDosBoxEmulatorWithNonDosBoxReturnsFalse()
    {
        var context = new LaunchContext { EmulatorName = "Mesen" };
        Assert.False(DosBoxLaunchStrategy.IsDosBoxEmulator(context));
    }

    [Fact]
    public void IsDosBoxEmulatorCaseInsensitive()
    {
        var context1 = new LaunchContext { EmulatorName = "dosbox" };
        var context2 = new LaunchContext { EmulatorName = "DOSBOX" };
        var context3 = new LaunchContext { EmulatorName = "DosBox" };

        Assert.True(DosBoxLaunchStrategy.IsDosBoxEmulator(context1));
        Assert.True(DosBoxLaunchStrategy.IsDosBoxEmulator(context2));
        Assert.True(DosBoxLaunchStrategy.IsDosBoxEmulator(context3));
    }

    [Fact]
    public void IsDosBoxEmulatorWithDosBoxStagingReturnsTrue()
    {
        var context = new LaunchContext { EmulatorName = "DOSBox Staging" };
        Assert.True(DosBoxLaunchStrategy.IsDosBoxEmulator(context));
    }

    [Fact]
    public void IsDosBoxEmulatorWithDosBoxPureReturnsTrue()
    {
        var context = new LaunchContext { EmulatorName = "dosbox_pure" };
        Assert.True(DosBoxLaunchStrategy.IsDosBoxEmulator(context));
    }
}

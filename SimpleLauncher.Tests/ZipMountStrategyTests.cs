using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="ZipMountStrategy"/> class.
/// </summary>
public class ZipMountStrategyTests
{
    private static ZipMountStrategy CreateStrategy()
    {
        var configurationMock = new Mock<IConfiguration>();
        var logErrorsMock = new Mock<ILogErrors>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var mountZipFilesMock = new Mock<IMountZipFiles>();

        return new ZipMountStrategy(
            configurationMock.Object,
            logErrorsMock.Object,
            messageBoxMock.Object,
            mountZipFilesMock.Object);
    }

    [Fact]
    public void PriorityIs30()
    {
        var strategy = CreateStrategy();
        Assert.Equal(30, strategy.Priority);
    }

    [Fact]
    public void IsMatchEmptyFilePathReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = "",
            EmulatorName = "RPCS3",
            SystemName = "PS3"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchEmptyEmulatorNameReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "",
            SystemName = "PS3"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchEmptySystemNameReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "RPCS3",
            SystemName = ""
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchNonArchiveFileReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.iso",
            EmulatorName = "RPCS3",
            SystemName = "PS3"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Theory]
    [InlineData(".zip")]
    [InlineData(".7z")]
    [InlineData(".rar")]
    public void IsMatchArchiveWithRpcs3ReturnsTrue(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\roms\game{extension}",
            EmulatorName = "RPCS3",
            SystemName = "PS3"
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
            ResolvedFilePath = $@"C:\roms\game{extension}",
            EmulatorName = "RPCS3",
            SystemName = "PS3"
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Theory]
    [InlineData(".zip")]
    [InlineData(".7z")]
    [InlineData(".rar")]
    public void IsMatchArchiveWithScummSystemReturnsTrue(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\roms\game{extension}",
            EmulatorName = "ScummVM",
            SystemName = "ScummVM"
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Theory]
    [InlineData(".zip")]
    [InlineData(".7z")]
    [InlineData(".rar")]
    public void IsMatchArchiveWithXblaSystemReturnsTrue(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\roms\game{extension}",
            EmulatorName = "Xenia",
            SystemName = "XBLA"
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchArchiveWithUnsupportedEmulatorReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "Mesen",
            SystemName = "NES"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchRpcs3NameVariants()
    {
        var strategy = CreateStrategy();
        var context1 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "RPCS3",
            SystemName = "PS3"
        };
        var context2 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "rpcs3",
            SystemName = "PS3"
        };
        var context3 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "Rpcs3",
            SystemName = "PS3"
        };

        Assert.True(strategy.IsMatch(context1));
        Assert.True(strategy.IsMatch(context2));
        Assert.True(strategy.IsMatch(context3));
    }

    [Fact]
    public void IsMatchScummSystemNameVariants()
    {
        var strategy = CreateStrategy();
        var context1 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "ScummVM",
            SystemName = "ScummVM"
        };
        var context2 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "ScummVM",
            SystemName = "Scumm"
        };
        var context3 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "ScummVM",
            SystemName = "scumm"
        };

        Assert.True(strategy.IsMatch(context1));
        Assert.True(strategy.IsMatch(context2));
        Assert.True(strategy.IsMatch(context3));
    }

    [Fact]
    public void IsMatchXblaSystemNameVariants()
    {
        var strategy = CreateStrategy();
        var context1 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "Xenia",
            SystemName = "XBLA"
        };
        var context2 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "Xenia",
            SystemName = "xbla"
        };
        var context3 = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "Xenia",
            SystemName = "Xbla"
        };

        Assert.True(strategy.IsMatch(context1));
        Assert.True(strategy.IsMatch(context2));
        Assert.True(strategy.IsMatch(context3));
    }
}

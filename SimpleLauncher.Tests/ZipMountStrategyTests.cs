using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.GameLauncher.Strategies;
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
        var logErrorsMock = new Mock<ILogger>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var mountZipFilesMock = new Mock<IMountZipFiles>();

        return new ZipMountStrategy(
            configurationMock.Object,
            logErrorsMock.Object,
            messageBoxMock.Object,
            mountZipFilesMock.Object);
    }

    /// <summary>
    /// Verifies that the strategy has a priority of 30.
    /// </summary>
    [Fact]
    public void PriorityIs30()
    {
        var strategy = CreateStrategy();
        Assert.Equal(30, strategy.Priority);
    }

    /// <summary>
    /// Verifies that <see cref="ZipMountStrategy.IsMatch"/> returns false when the file path is empty.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="ZipMountStrategy.IsMatch"/> returns false when the emulator name is empty.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="ZipMountStrategy.IsMatch"/> returns false when the system name is empty.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="ZipMountStrategy.IsMatch"/> returns false for non-archive files like ISO.
    /// </summary>
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

    /// <summary>
    /// Verifies that archive files with RPCS3 emulator are matched correctly.
    /// </summary>
    /// <param name="extension">The file extension to test.</param>
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

    /// <summary>
    /// Verifies that archive extension matching is case-insensitive.
    /// </summary>
    /// <param name="extension">The uppercase file extension to test.</param>
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

    /// <summary>
    /// Verifies that archive files with ScummVM system name are matched correctly.
    /// </summary>
    /// <param name="extension">The file extension to test.</param>
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

    /// <summary>
    /// Verifies that archive files with XBLA system name are matched correctly.
    /// </summary>
    /// <param name="extension">The file extension to test.</param>
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

    /// <summary>
    /// Verifies that archive files are not matched when the emulator is unsupported.
    /// </summary>
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

    /// <summary>
    /// Verifies that RPCS3 name casing variants are all recognized.
    /// </summary>
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

    /// <summary>
    /// Verifies that ScummVM system name variants are all recognized.
    /// </summary>
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

    /// <summary>
    /// Verifies that XBLA system name variants are all recognized.
    /// </summary>
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

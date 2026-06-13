using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="XisoMountStrategy"/> class.
/// </summary>
public class XisoMountStrategyTests
{
    private static XisoMountStrategy CreateStrategy()
    {
        var configurationMock = new Mock<IConfiguration>();
        var logErrorsMock = new Mock<ILogErrors>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var mountXisoFilesMock = new Mock<IMountXisoFiles>();

        return new XisoMountStrategy(
            configurationMock.Object,
            logErrorsMock.Object,
            messageBoxMock.Object,
            mountXisoFilesMock.Object);
    }

    [Fact]
    public void PriorityIs20()
    {
        var strategy = CreateStrategy();
        Assert.Equal(20, strategy.Priority);
    }

    [Fact]
    public void IsMatchEmptyFilePathReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = "",
            EmulatorName = "Cxbx-Reloaded"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchEmptyEmulatorNameReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\xbox\game.iso",
            EmulatorName = ""
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchNonIsoFileReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\xbox\game.zip",
            EmulatorName = "Cxbx-Reloaded"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchNonCxbxEmulatorReturnsFalse()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\xbox\game.iso",
            EmulatorName = "Mesen"
        };

        Assert.False(strategy.IsMatch(context));
    }

    [Theory]
    [InlineData("Cxbx-Reloaded")]
    [InlineData("Cxbx")]
    [InlineData("cxbx-reloaded")]
    [InlineData("CXBX-RELOADED")]
    public void IsMatchCxbxEmulatorWithIsoReturnsTrue(string emulatorName)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\xbox\game.iso",
            EmulatorName = emulatorName
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchIsoExtensionCaseInsensitive()
    {
        var strategy = CreateStrategy();
        var context1 = new LaunchContext
        {
            ResolvedFilePath = @"C:\xbox\game.iso",
            EmulatorName = "Cxbx-Reloaded"
        };
        var context2 = new LaunchContext
        {
            ResolvedFilePath = @"C:\xbox\game.ISO",
            EmulatorName = "Cxbx-Reloaded"
        };
        var context3 = new LaunchContext
        {
            ResolvedFilePath = @"C:\xbox\game.Iso",
            EmulatorName = "Cxbx-Reloaded"
        };

        Assert.True(strategy.IsMatch(context1));
        Assert.True(strategy.IsMatch(context2));
        Assert.True(strategy.IsMatch(context3));
    }

    [Theory]
    [InlineData(".zip")]
    [InlineData(".7z")]
    [InlineData(".rar")]
    [InlineData(".bin")]
    [InlineData(".img")]
    [InlineData(".chd")]
    public void IsMatchNonIsoExtensionReturnsFalse(string extension)
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\xbox\game{extension}",
            EmulatorName = "Cxbx-Reloaded"
        };

        Assert.False(strategy.IsMatch(context));
    }
}

using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.GameLauncher.Strategies;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for the <see cref="XisoMountStrategy" /> class.
/// </summary>
public class XisoMountStrategyTests
{
    private static XisoMountStrategy CreateStrategy()
    {
        var configurationMock = new Mock<IConfiguration>();
        var logErrorsMock = new Mock<ILogger>();
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        var mountXisoFilesMock = new Mock<IMountXisoFiles>();

        return new XisoMountStrategy(
            configurationMock.Object,
            logErrorsMock.Object,
            messageBoxMock.Object,
            mountXisoFilesMock.Object);
    }

    /// <summary>
    ///     Verifies that the strategy has a priority of 20.
    /// </summary>
    [Fact]
    public void PriorityIs20()
    {
        var strategy = CreateStrategy();
        Assert.Equal(20, strategy.Priority);
    }

    /// <summary>
    ///     Verifies that <see cref="XisoMountStrategy.IsMatch" /> returns false when the file path is empty.
    /// </summary>
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

    /// <summary>
    ///     Verifies that <see cref="XisoMountStrategy.IsMatch" /> returns false when the emulator name is empty.
    /// </summary>
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

    /// <summary>
    ///     Verifies that <see cref="XisoMountStrategy.IsMatch" /> returns false when the file is not an ISO.
    /// </summary>
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

    /// <summary>
    ///     Verifies that <see cref="XisoMountStrategy.IsMatch" /> returns false for non-Cxbx emulators.
    /// </summary>
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

    /// <summary>
    ///     Verifies that <see cref="XisoMountStrategy.IsMatch" /> returns true when the emulator name is a Cxbx variant and
    ///     the file is an ISO.
    /// </summary>
    /// <param name="emulatorName">The emulator name variant to test.</param>
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

    /// <summary>
    ///     Verifies that ISO file extension matching is case-insensitive.
    /// </summary>
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

    /// <summary>
    ///     Verifies that <see cref="XisoMountStrategy.IsMatch" /> returns false for non-ISO file extensions.
    /// </summary>
    /// <param name="extension">The file extension to test.</param>
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
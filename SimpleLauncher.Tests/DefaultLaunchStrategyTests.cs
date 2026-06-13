using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameLauncher.Strategies;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="DefaultLaunchStrategy"/> class.
/// </summary>
public class DefaultLaunchStrategyTests
{
    private static DefaultLaunchStrategy CreateStrategy()
    {
        return new DefaultLaunchStrategy();
    }

    [Fact]
    public void PriorityIs999()
    {
        var strategy = CreateStrategy();
        Assert.Equal(999, strategy.Priority);
    }

    [Fact]
    public void IsMatchAlwaysReturnsTrue()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.zip",
            EmulatorName = "Mesen"
        };

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public void IsMatchWithEmptyContextReturnsTrue()
    {
        var strategy = CreateStrategy();
        var context = new LaunchContext();

        Assert.True(strategy.IsMatch(context));
    }

    [Fact]
    public async Task ExecuteAsyncBatFileCallsRunBatchFileAsync()
    {
        var strategy = CreateStrategy();
        var launcherMock = new Mock<ILauncherService>();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.bat",
            EmulatorName = "DOSBox-X"
        };

        await strategy.ExecuteAsync(context, launcherMock.Object);

        launcherMock.Verify(l => l.RunBatchFileAsync(
            context.ResolvedFilePath,
            context.EmulatorManager,
            context.WindowContext), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncLnkFileCallsLaunchShortcutFileAsync()
    {
        var strategy = CreateStrategy();
        var launcherMock = new Mock<ILauncherService>();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.lnk",
            EmulatorName = "Mesen"
        };

        await strategy.ExecuteAsync(context, launcherMock.Object);

        launcherMock.Verify(l => l.LaunchShortcutFileAsync(
            context.ResolvedFilePath,
            context.EmulatorManager,
            context.WindowContext), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncUrlFileCallsLaunchShortcutFileAsync()
    {
        var strategy = CreateStrategy();
        var launcherMock = new Mock<ILauncherService>();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.url",
            EmulatorName = "Mesen"
        };

        await strategy.ExecuteAsync(context, launcherMock.Object);

        launcherMock.Verify(l => l.LaunchShortcutFileAsync(
            context.ResolvedFilePath,
            context.EmulatorManager,
            context.WindowContext), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncExeFileCallsLaunchExecutableAsync()
    {
        var strategy = CreateStrategy();
        var launcherMock = new Mock<ILauncherService>();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\game.exe",
            EmulatorName = "Mesen"
        };

        await strategy.ExecuteAsync(context, launcherMock.Object);

        launcherMock.Verify(l => l.LaunchExecutableAsync(
            context.ResolvedFilePath,
            context.EmulatorManager,
            context.WindowContext), Times.Once);
    }

    [Theory]
    [InlineData(".nes")]
    [InlineData(".sfc")]
    [InlineData(".smc")]
    [InlineData(".gba")]
    [InlineData(".gb")]
    [InlineData(".gbc")]
    [InlineData(".n64")]
    [InlineData(".z64")]
    [InlineData(".v64")]
    [InlineData(".md")]
    [InlineData(".gen")]
    [InlineData(".smd")]
    [InlineData(".psx")]
    [InlineData(".iso")]
    [InlineData(".bin")]
    [InlineData(".img")]
    public async Task ExecuteAsyncRomFileCallsLaunchRegularEmulatorAsync(string extension)
    {
        var strategy = CreateStrategy();
        var launcherMock = new Mock<ILauncherService>();
        var context = new LaunchContext
        {
            ResolvedFilePath = $@"C:\roms\game{extension}",
            EmulatorName = "Mesen",
            SystemName = "NES",
            Parameters = "--fullscreen"
        };

        await strategy.ExecuteAsync(context, launcherMock.Object);

        launcherMock.Verify(l => l.LaunchRegularEmulatorAsync(
            context.ResolvedFilePath,
            context.EmulatorName,
            context.SystemManager,
            context.EmulatorManager,
            context.Parameters,
            context.WindowContext,
            context.LoadingState,
            null), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncUpperCaseExtensionCallsCorrectMethod()
    {
        var strategy = CreateStrategy();
        var launcherMock = new Mock<ILauncherService>();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\GAME.BAT",
            EmulatorName = "DOSBox-X"
        };

        await strategy.ExecuteAsync(context, launcherMock.Object);

        launcherMock.Verify(l => l.RunBatchFileAsync(
            context.ResolvedFilePath,
            context.EmulatorManager,
            context.WindowContext), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsyncMixedCaseExtensionCallsCorrectMethod()
    {
        var strategy = CreateStrategy();
        var launcherMock = new Mock<ILauncherService>();
        var context = new LaunchContext
        {
            ResolvedFilePath = @"C:\roms\Game.Exe",
            EmulatorName = "Mesen"
        };

        await strategy.ExecuteAsync(context, launcherMock.Object);

        launcherMock.Verify(l => l.LaunchExecutableAsync(
            context.ResolvedFilePath,
            context.EmulatorManager,
            context.WindowContext), Times.Once);
    }
}

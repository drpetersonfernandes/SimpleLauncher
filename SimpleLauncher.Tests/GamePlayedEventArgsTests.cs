using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="GamePlayedEventArgs"/>.
/// </summary>
public class GamePlayedEventArgsTests
{
    [Fact]
    public void Constructor_StoresFileNameAndSystemName()
    {
        var args = new GamePlayedEventArgs("game.zip", "NES");

        Assert.Equal("game.zip", args.FileName);
        Assert.Equal("NES", args.SystemName);
    }

    [Fact]
    public void Constructor_ExtendsEventArgs()
    {
        var args = new GamePlayedEventArgs("game.zip", "NES");

        Assert.IsAssignableFrom<EventArgs>(args);
    }
}

using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameButtonTagTests
{
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        var tag = new GameButtonTag();

        Assert.False(tag.IsDefaultImage);
        Assert.Null(tag.Key);
    }

    [Fact]
    public void PropertiesCanBeSet()
    {
        var tag = new GameButtonTag
        {
            IsDefaultImage = true,
            Key = "game_key_123"
        };

        Assert.True(tag.IsDefaultImage);
        Assert.Equal("game_key_123", tag.Key);
    }

    [Fact]
    public void PropertiesCanBeChangedAfterCreation()
    {
        var tag = new GameButtonTag
        {
            IsDefaultImage = false,
            Key = "initial"
        };

        tag.IsDefaultImage = true;
        tag.Key = "updated";

        Assert.True(tag.IsDefaultImage);
        Assert.Equal("updated", tag.Key);
    }

    [Fact]
    public void KeyCanBeNull()
    {
        var tag = new GameButtonTag
        {
            Key = "something"
        };

        tag.Key = null;

        Assert.Null(tag.Key);
    }

    [Fact]
    public void KeyCanContainSpecialCharacters()
    {
        var tag = new GameButtonTag
        {
            Key = @"C:\roms\game (USA) [!].zip"
        };

        Assert.Equal(@"C:\roms\game (USA) [!].zip", tag.Key);
    }
}

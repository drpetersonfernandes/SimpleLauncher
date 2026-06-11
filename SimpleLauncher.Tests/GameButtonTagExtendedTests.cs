using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameButtonTagExtendedTests
{
    [Fact]
    public void GameButtonTagDefaultIsDefaultImageIsFalse()
    {
        var tag = new GameButtonTag();
        Assert.False(tag.IsDefaultImage);
    }

    [Fact]
    public void GameButtonTagDefaultKeyIsEmpty()
    {
        var tag = new GameButtonTag();
        Assert.Equal("", tag.Key);
    }

    [Fact]
    public void GameButtonTagPropertiesCanBeSet()
    {
        var tag = new GameButtonTag
        {
            IsDefaultImage = true,
            Key = "NES|game.zip"
        };

        Assert.True(tag.IsDefaultImage);
        Assert.Equal("NES|game.zip", tag.Key);
    }

    [Fact]
    public void GameButtonTagKeyWithSpecialCharacters()
    {
        var tag = new GameButtonTag
        {
            Key = "Arcade|game (v1.0) [!].zip"
        };

        Assert.Contains("(", tag.Key);
        Assert.Contains("[", tag.Key);
    }

    [Fact]
    public void GameButtonTagKeyWithPipe()
    {
        var tag = new GameButtonTag
        {
            Key = "SystemName|FileName"
        };

        Assert.Contains("|", tag.Key);
    }

    [Fact]
    public void GameButtonTagKeyWithUnicode()
    {
        var tag = new GameButtonTag
        {
            Key = "GBA|ポケモン.zip"
        };

        Assert.Contains("ポケモン", tag.Key);
    }

    [Fact]
    public void GameButtonTagIsDefaultImageCanBeToggled()
    {
        var tag = new GameButtonTag { IsDefaultImage = true };
        Assert.True(tag.IsDefaultImage);

        tag.IsDefaultImage = false;
        Assert.False(tag.IsDefaultImage);
    }

    [Fact]
    public void GameButtonTagKeyWithEmptySystem()
    {
        var tag = new GameButtonTag
        {
            Key = "|game.zip"
        };

        Assert.StartsWith("|", tag.Key);
    }

    [Fact]
    public void GameButtonTagKeyWithLongFileName()
    {
        var longName = new string('a', 500) + ".zip";
        var tag = new GameButtonTag
        {
            Key = $"NES|{longName}"
        };

        Assert.Contains(longName, tag.Key);
    }
}

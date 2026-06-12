using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for the <see cref="GameButtonTag"/> model class covering additional edge cases.
/// </summary>
public class GameButtonTagExtendedTests
{
    /// <summary>
    /// Verifies that the default value of IsDefaultImage is false.
    /// </summary>
    [Fact]
    public void GameButtonTagDefaultIsDefaultImageIsFalse()
    {
        var tag = new GameButtonTag();
        Assert.False(tag.IsDefaultImage);
    }

    /// <summary>
    /// Verifies that the default value of Key is an empty string.
    /// </summary>
    [Fact]
    public void GameButtonTagDefaultKeyIsEmpty()
    {
        var tag = new GameButtonTag();
        Assert.Equal("", tag.Key);
    }

    /// <summary>
    /// Verifies that GameButtonTag properties can be set.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Key property supports special characters like parentheses and brackets.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Key property supports the pipe separator character.
    /// </summary>
    [Fact]
    public void GameButtonTagKeyWithPipe()
    {
        var tag = new GameButtonTag
        {
            Key = "SystemName|FileName"
        };

        Assert.Contains("|", tag.Key);
    }

    /// <summary>
    /// Verifies that the Key property supports Unicode characters.
    /// </summary>
    [Fact]
    public void GameButtonTagKeyWithUnicode()
    {
        var tag = new GameButtonTag
        {
            Key = "GBA|ポケモン.zip"
        };

        Assert.Contains("ポケモン", tag.Key);
    }

    /// <summary>
    /// Verifies that IsDefaultImage can be toggled between true and false.
    /// </summary>
    [Fact]
    public void GameButtonTagIsDefaultImageCanBeToggled()
    {
        var tag = new GameButtonTag { IsDefaultImage = true };
        Assert.True(tag.IsDefaultImage);

        tag.IsDefaultImage = false;
        Assert.False(tag.IsDefaultImage);
    }

    /// <summary>
    /// Verifies that the Key property supports an empty system prefix.
    /// </summary>
    [Fact]
    public void GameButtonTagKeyWithEmptySystem()
    {
        var tag = new GameButtonTag
        {
            Key = "|game.zip"
        };

        Assert.StartsWith("|", tag.Key);
    }

    /// <summary>
    /// Verifies that the Key property supports long file names.
    /// </summary>
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

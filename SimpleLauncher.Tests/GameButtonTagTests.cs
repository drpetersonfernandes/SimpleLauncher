using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="GameButtonTag"/> model class.
/// </summary>
public class GameButtonTagTests
{
    /// <summary>
    /// Verifies that a new GameButtonTag has correct default values.
    /// </summary>
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        var tag = new GameButtonTag();

        Assert.False(tag.IsDefaultImage);
        Assert.Equal("", tag.Key);
    }

    /// <summary>
    /// Verifies that GameButtonTag properties can be set during initialization.
    /// </summary>
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

    /// <summary>
    /// Verifies that GameButtonTag properties can be modified after creation.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Key property can be set to null.
    /// </summary>
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

    /// <summary>
    /// Verifies that the Key property can contain special characters and file paths.
    /// </summary>
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

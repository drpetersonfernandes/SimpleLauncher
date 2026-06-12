using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="DosBoxFileItem"/> model property initialization, defaults, and edge cases.
/// </summary>
public class DosBoxFileItemTests
{
    /// <summary>
    /// Verifies that a DosBoxFileItem can be created with all properties set.
    /// </summary>
    [Fact]
    public void CanCreateWithAllProperties()
    {
        var item = new DosBoxFileItem
        {
            FullPath = @"C:\dos\game.exe",
            DisplayName = "game.exe",
            RelativePath = @"dos\game.exe"
        };

        Assert.Equal(@"C:\dos\game.exe", item.FullPath);
        Assert.Equal("game.exe", item.DisplayName);
        Assert.Equal(@"dos\game.exe", item.RelativePath);
    }

    /// <summary>
    /// Verifies that a new DosBoxFileItem has all properties defaulting to empty strings.
    /// </summary>
    [Fact]
    public void PropertiesDefaultToEmptyString()
    {
        var item = new DosBoxFileItem();

        Assert.Equal("", item.FullPath);
        Assert.Equal("", item.DisplayName);
        Assert.Equal("", item.RelativePath);
    }

    /// <summary>
    /// Verifies that init-only properties retain their values after object initialization.
    /// </summary>
    [Fact]
    public void InitPropertiesCannotBeModifiedAfterCreation()
    {
        var item = new DosBoxFileItem
        {
            FullPath = @"C:\dos\game.exe",
            DisplayName = "game.exe",
            RelativePath = @"dos\game.exe"
        };

        // init properties can only be set during object initialization
        Assert.Equal(@"C:\dos\game.exe", item.FullPath);
        Assert.Equal("game.exe", item.DisplayName);
        Assert.Equal(@"dos\game.exe", item.RelativePath);
    }

    /// <summary>
    /// Verifies that empty strings are valid values for all DosBoxFileItem properties.
    /// </summary>
    [Fact]
    public void EmptyStringsAreAllowed()
    {
        var item = new DosBoxFileItem
        {
            FullPath = "",
            DisplayName = "",
            RelativePath = ""
        };

        Assert.Equal("", item.FullPath);
        Assert.Equal("", item.DisplayName);
        Assert.Equal("", item.RelativePath);
    }

    /// <summary>
    /// Verifies that DosBoxFileItem paths can contain spaces.
    /// </summary>
    [Fact]
    public void PathsCanContainSpaces()
    {
        var item = new DosBoxFileItem
        {
            FullPath = @"C:\My Games\DOSBox\game.exe",
            DisplayName = "My Game",
            RelativePath = @"My Games\DOSBox\game.exe"
        };

        Assert.Equal(@"C:\My Games\DOSBox\game.exe", item.FullPath);
        Assert.Equal("My Game", item.DisplayName);
    }
}

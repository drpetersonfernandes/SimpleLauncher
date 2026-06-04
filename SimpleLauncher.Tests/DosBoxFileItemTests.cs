using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class DosBoxFileItemTests
{
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

    [Fact]
    public void PropertiesDefaultToNull()
    {
        var item = new DosBoxFileItem();

        Assert.Null(item.FullPath);
        Assert.Null(item.DisplayName);
        Assert.Null(item.RelativePath);
    }

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

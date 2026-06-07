using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class DosBoxFileItemExtendedTests
{
    [Fact]
    public void DosBoxFileItemDefaultFullPathIsNull()
    {
        var item = new DosBoxFileItem();
        Assert.Null(item.FullPath);
    }

    [Fact]
    public void DosBoxFileItemDefaultDisplayNameIsNull()
    {
        var item = new DosBoxFileItem();
        Assert.Null(item.DisplayName);
    }

    [Fact]
    public void DosBoxFileItemDefaultRelativePathIsNull()
    {
        var item = new DosBoxFileItem();
        Assert.Null(item.RelativePath);
    }

    [Fact]
    public void DosBoxFileItemPropertiesCanBeSet()
    {
        var item = new DosBoxFileItem
        {
            FullPath = @"C:\dos\game\GAME.EXE",
            DisplayName = "GAME.EXE",
            RelativePath = "GAME.EXE"
        };

        Assert.Equal(@"C:\dos\game\GAME.EXE", item.FullPath);
        Assert.Equal("GAME.EXE", item.DisplayName);
        Assert.Equal("GAME.EXE", item.RelativePath);
    }

    [Fact]
    public void DosBoxFileItemWithSpacesInPath()
    {
        var item = new DosBoxFileItem
        {
            FullPath = @"C:\DOS Games\My Game\GAME.EXE",
            DisplayName = "My Game",
            RelativePath = "My Game\\GAME.EXE"
        };

        Assert.Contains(" ", item.FullPath);
        Assert.Contains(" ", item.DisplayName);
    }

    [Fact]
    public void DosBoxFileItemWithLongPath()
    {
        var longPath = @"C:\" + new string('a', 200) + @"\GAME.EXE";
        var item = new DosBoxFileItem
        {
            FullPath = longPath
        };

        Assert.Equal(longPath, item.FullPath);
    }

    [Fact]
    public void DosBoxFileItemWithSpecialCharacters()
    {
        var item = new DosBoxFileItem
        {
            FullPath = @"C:\dos\game (v1.0)\GAME.EXE",
            DisplayName = "game (v1.0)"
        };

        Assert.Contains("(", item.FullPath);
        Assert.Contains("(", item.DisplayName);
    }

    [Fact]
    public void DosBoxFileItemWithUnicode()
    {
        var item = new DosBoxFileItem
        {
            FullPath = @"C:\dos\ゲーム\GAME.EXE",
            DisplayName = "ゲーム"
        };

        Assert.Contains("ゲーム", item.FullPath);
        Assert.Contains("ゲーム", item.DisplayName);
    }

    [Fact]
    public void DosBoxFileItemRelativePathWithoutBackslash()
    {
        var item = new DosBoxFileItem
        {
            RelativePath = "GAME.EXE"
        };

        Assert.Equal("GAME.EXE", item.RelativePath);
    }

    [Fact]
    public void DosBoxFileItemRelativePathWithBackslash()
    {
        var item = new DosBoxFileItem
        {
            RelativePath = "SUBDIR\\GAME.EXE"
        };

        Assert.Contains("\\", item.RelativePath);
    }
}

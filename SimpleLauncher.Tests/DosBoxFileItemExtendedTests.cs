using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="DosBoxFileItem"/> covering special characters, Unicode, long paths, and path formatting.
/// </summary>
public class DosBoxFileItemExtendedTests
{
    /// <summary>
    /// Verifies that the default FullPath property is an empty string.
    /// </summary>
    [Fact]
    public void DosBoxFileItemDefaultFullPathIsEmpty()
    {
        var item = new DosBoxFileItem();
        Assert.Equal("", item.FullPath);
    }

    /// <summary>
    /// Verifies that the default DisplayName property is an empty string.
    /// </summary>
    [Fact]
    public void DosBoxFileItemDefaultDisplayNameIsEmpty()
    {
        var item = new DosBoxFileItem();
        Assert.Equal("", item.DisplayName);
    }

    /// <summary>
    /// Verifies that the default RelativePath property is an empty string.
    /// </summary>
    [Fact]
    public void DosBoxFileItemDefaultRelativePathIsEmpty()
    {
        var item = new DosBoxFileItem();
        Assert.Equal("", item.RelativePath);
    }

    /// <summary>
    /// Verifies that all DosBoxFileItem properties can be set and retrieved correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that DosBoxFileItem handles paths containing spaces.
    /// </summary>
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

    /// <summary>
    /// Verifies that DosBoxFileItem handles long file system paths.
    /// </summary>
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

    /// <summary>
    /// Verifies that DosBoxFileItem handles paths with special characters like parentheses.
    /// </summary>
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

    /// <summary>
    /// Verifies that DosBoxFileItem handles Unicode characters in paths and display names.
    /// </summary>
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

    /// <summary>
    /// Verifies that a RelativePath without a backslash separator is stored correctly.
    /// </summary>
    [Fact]
    public void DosBoxFileItemRelativePathWithoutBackslash()
    {
        var item = new DosBoxFileItem
        {
            RelativePath = "GAME.EXE"
        };

        Assert.Equal("GAME.EXE", item.RelativePath);
    }

    /// <summary>
    /// Verifies that a RelativePath with a backslash separator is stored correctly.
    /// </summary>
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

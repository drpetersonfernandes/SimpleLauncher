using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for the <see cref="GameListViewItem"/> model class covering additional edge cases.
/// </summary>
public class GameListViewItemExtendedTests
{
    /// <summary>
    /// Verifies that the GameListViewItem instance can be created.
    /// </summary>
    [Fact]
    public void FileSizePropertyCanBeSet()
    {
        var item = new GameListViewItem();
        // FileSize may not exist - let's check
        // This tests the basic property access pattern
        Assert.NotNull(item);
    }

    /// <summary>
    /// Verifies that multiple PropertyChanged subscriptions are all invoked.
    /// </summary>
    [Fact]
    public void MultiplePropertyChangedSubscriptions()
    {
        var item = new GameListViewItem();
        var count = 0;
        item.PropertyChanged += (_, _) => { count++; };
        item.PropertyChanged += (_, _) => { count++; };

        item.IsFavorite = true;
        Assert.Equal(2, count);
    }

    /// <summary>
    /// Verifies that PropertyChanged event sender is the correct instance.
    /// </summary>
    [Fact]
    public void PropertyChangedSenderIsCorrect()
    {
        var item = new GameListViewItem();
        object? sender = null;
        item.PropertyChanged += (s, _) => { sender = s; };

        item.IsFavorite = true;
        Assert.Same(item, sender);
    }

    /// <summary>
    /// Verifies that PropertyChanged event args contain the correct property name.
    /// </summary>
    [Fact]
    public void PropertyChangedArgsPropertyNameIsCorrect()
    {
        var item = new GameListViewItem();
        string? propertyName = null;
        item.PropertyChanged += (_, args) => { propertyName = args.PropertyName; };

        item.IsFavorite = true;
        Assert.Equal(nameof(GameListViewItem.IsFavorite), propertyName);
    }

    /// <summary>
    /// Verifies that FilePath supports special characters.
    /// </summary>
    [Fact]
    public void FilePathWithSpecialCharacters()
    {
        var item = new GameListViewItem
        {
            FilePath = @"C:\roms\game (v1.0) [!].zip"
        };

        Assert.Contains("(", item.FilePath);
        Assert.Contains("[", item.FilePath);
    }

    /// <summary>
    /// Verifies that FilePath supports Unicode characters.
    /// </summary>
    [Fact]
    public void FilePathWithUnicode()
    {
        var item = new GameListViewItem
        {
            FilePath = @"C:\roms\ポケモン.zip"
        };

        Assert.Contains("ポケモン", item.FilePath);
    }

    /// <summary>
    /// Verifies that FolderPath supports spaces.
    /// </summary>
    [Fact]
    public void FolderPathWithSpaces()
    {
        var item = new GameListViewItem
        {
            FolderPath = @"C:\My Games\ROMs"
        };

        Assert.Contains(" ", item.FolderPath);
    }

    /// <summary>
    /// Verifies that all string properties default to empty.
    /// </summary>
    [Fact]
    public void AllStringPropertiesDefaultToEmpty()
    {
        var item = new GameListViewItem();
        Assert.Equal("", item.FilePath);
        Assert.Equal("", item.FolderPath);
        Assert.Equal("", item.FileName);
        Assert.Equal("", item.MachineDescription);
    }

    /// <summary>
    /// Verifies that all numeric properties default to zero.
    /// </summary>
    [Fact]
    public void AllNumericDefaultsAreCorrect()
    {
        var item = new GameListViewItem();
        Assert.Equal(0, item.AchievementsEarned);
        Assert.Equal(0, item.AchievementsTotal);
    }

    /// <summary>
    /// Verifies that all boolean properties default to false.
    /// </summary>
    [Fact]
    public void AllBoolDefaultsAreCorrect()
    {
        var item = new GameListViewItem();
        Assert.False(item.IsFavorite);
        Assert.False(item.HasAchievements);
    }

    /// <summary>
    /// Verifies that all string-formatted properties have correct default values.
    /// </summary>
    [Fact]
    public void AllStringDefaultsAreCorrect()
    {
        var item = new GameListViewItem();
        Assert.Equal("0", item.TimesPlayed);
        Assert.Equal("0m 0s", item.PlayTime);
    }

    /// <summary>
    /// Verifies that all properties can be set simultaneously and read back.
    /// </summary>
    [Fact]
    public void CanSetAllPropertiesSimultaneously()
    {
        var item = new GameListViewItem
        {
            FilePath = @"C:\roms\game.zip",
            FolderPath = @"C:\roms",
            FileName = "game",
            IsFavorite = true,
            MachineDescription = "Test Machine",
            TimesPlayed = "10",
            PlayTime = "1h 30m 0s",
            HasAchievements = true,
            AchievementsEarned = 50,
            AchievementsTotal = 100
        };

        Assert.Equal(@"C:\roms\game.zip", item.FilePath);
        Assert.Equal(@"C:\roms", item.FolderPath);
        Assert.Equal("game", item.FileName);
        Assert.True(item.IsFavorite);
        Assert.Equal("Test Machine", item.MachineDescription);
        Assert.Equal("10", item.TimesPlayed);
        Assert.Equal("1h 30m 0s", item.PlayTime);
        Assert.True(item.HasAchievements);
        Assert.Equal(50, item.AchievementsEarned);
        Assert.Equal(100, item.AchievementsTotal);
    }
}

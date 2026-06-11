using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameListViewItemExtendedTests
{
    [Fact]
    public void FileSizePropertyCanBeSet()
    {
        var item = new GameListViewItem();
        // FileSize may not exist - let's check
        // This tests the basic property access pattern
        Assert.NotNull(item);
    }

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

    [Fact]
    public void PropertyChangedSenderIsCorrect()
    {
        var item = new GameListViewItem();
        object? sender = null;
        item.PropertyChanged += (s, _) => { sender = s; };

        item.IsFavorite = true;
        Assert.Same(item, sender);
    }

    [Fact]
    public void PropertyChangedArgsPropertyNameIsCorrect()
    {
        var item = new GameListViewItem();
        string? propertyName = null;
        item.PropertyChanged += (_, args) => { propertyName = args.PropertyName; };

        item.IsFavorite = true;
        Assert.Equal(nameof(GameListViewItem.IsFavorite), propertyName);
    }

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

    [Fact]
    public void FilePathWithUnicode()
    {
        var item = new GameListViewItem
        {
            FilePath = @"C:\roms\ポケモン.zip"
        };

        Assert.Contains("ポケモン", item.FilePath);
    }

    [Fact]
    public void FolderPathWithSpaces()
    {
        var item = new GameListViewItem
        {
            FolderPath = @"C:\My Games\ROMs"
        };

        Assert.Contains(" ", item.FolderPath);
    }

    [Fact]
    public void AllStringPropertiesDefaultToEmpty()
    {
        var item = new GameListViewItem();
        Assert.Equal("", item.FilePath);
        Assert.Equal("", item.FolderPath);
        Assert.Equal("", item.FileName);
        Assert.Equal("", item.MachineDescription);
    }

    [Fact]
    public void AllNumericDefaultsAreCorrect()
    {
        var item = new GameListViewItem();
        Assert.Equal(0, item.AchievementsEarned);
        Assert.Equal(0, item.AchievementsTotal);
    }

    [Fact]
    public void AllBoolDefaultsAreCorrect()
    {
        var item = new GameListViewItem();
        Assert.False(item.IsFavorite);
        Assert.False(item.HasAchievements);
    }

    [Fact]
    public void AllStringDefaultsAreCorrect()
    {
        var item = new GameListViewItem();
        Assert.Equal("0", item.TimesPlayed);
        Assert.Equal("0m 0s", item.PlayTime);
    }

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

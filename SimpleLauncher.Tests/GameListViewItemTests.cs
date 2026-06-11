using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameListViewItemTests
{
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        var item = new GameListViewItem();

        Assert.Equal("", item.FilePath);
        Assert.Equal("", item.FolderPath);
        Assert.Equal("", item.FileName);
        Assert.False(item.IsFavorite);
        Assert.Equal("", item.MachineDescription);
        Assert.Equal("0", item.TimesPlayed);
        Assert.Equal("0m 0s", item.PlayTime);
        Assert.False(item.HasAchievements);
        Assert.Equal(0, item.AchievementsEarned);
        Assert.Equal(0, item.AchievementsTotal);
    }

    [Fact]
    public void InitPropertiesCanBeSetDuringCreation()
    {
        var item = new GameListViewItem
        {
            FilePath = @"C:\roms\game.zip",
            FolderPath = @"C:\roms",
            FileName = "game.zip"
        };

        Assert.Equal(@"C:\roms\game.zip", item.FilePath);
        Assert.Equal(@"C:\roms", item.FolderPath);
        Assert.Equal("game.zip", item.FileName);
    }

    [Fact]
    public void IsFavoriteRaisesPropertyChanged()
    {
        var item = new GameListViewItem();
        var eventRaised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameListViewItem.IsFavorite))
            {
                eventRaised = true;
            }
        };

        item.IsFavorite = true;

        Assert.True(eventRaised);
        Assert.True(item.IsFavorite);
    }

    [Fact]
    public void IsFavoriteSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { IsFavorite = true };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.IsFavorite = true;

        Assert.False(eventRaised);
    }

    [Fact]
    public void MachineDescriptionRaisesPropertyChanged()
    {
        var item = new GameListViewItem();
        var eventRaised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameListViewItem.MachineDescription))
            {
                eventRaised = true;
            }
        };

        item.MachineDescription = "Neo Geo";

        Assert.True(eventRaised);
        Assert.Equal("Neo Geo", item.MachineDescription);
    }

    [Fact]
    public void MachineDescriptionSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { MachineDescription = "Neo Geo" };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.MachineDescription = "Neo Geo";

        Assert.False(eventRaised);
    }

    [Fact]
    public void TimesPlayedRaisesPropertyChanged()
    {
        var item = new GameListViewItem();
        var eventRaised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameListViewItem.TimesPlayed))
            {
                eventRaised = true;
            }
        };

        item.TimesPlayed = "5";

        Assert.True(eventRaised);
        Assert.Equal("5", item.TimesPlayed);
    }

    [Fact]
    public void TimesPlayedSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { TimesPlayed = "3" };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.TimesPlayed = "3";

        Assert.False(eventRaised);
    }

    [Fact]
    public void PlayTimeRaisesPropertyChanged()
    {
        var item = new GameListViewItem();
        var eventRaised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameListViewItem.PlayTime))
            {
                eventRaised = true;
            }
        };

        item.PlayTime = "1h 30m 0s";

        Assert.True(eventRaised);
        Assert.Equal("1h 30m 0s", item.PlayTime);
    }

    [Fact]
    public void PlayTimeSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { PlayTime = "10m 0s" };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.PlayTime = "10m 0s";

        Assert.False(eventRaised);
    }

    [Fact]
    public void HasAchievementsRaisesPropertyChanged()
    {
        var item = new GameListViewItem();
        var eventRaised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameListViewItem.HasAchievements))
            {
                eventRaised = true;
            }
        };

        item.HasAchievements = true;

        Assert.True(eventRaised);
        Assert.True(item.HasAchievements);
    }

    [Fact]
    public void AchievementsEarnedRaisesPropertyChanged()
    {
        var item = new GameListViewItem();
        var eventRaised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameListViewItem.AchievementsEarned))
            {
                eventRaised = true;
            }
        };

        item.AchievementsEarned = 10;

        Assert.True(eventRaised);
        Assert.Equal(10, item.AchievementsEarned);
    }

    [Fact]
    public void AchievementsTotalRaisesPropertyChanged()
    {
        var item = new GameListViewItem();
        var eventRaised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameListViewItem.AchievementsTotal))
            {
                eventRaised = true;
            }
        };

        item.AchievementsTotal = 50;

        Assert.True(eventRaised);
        Assert.Equal(50, item.AchievementsTotal);
    }

    [Fact]
    public void AchievementsEarnedSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { AchievementsEarned = 5 };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.AchievementsEarned = 5;

        Assert.False(eventRaised);
    }

    [Fact]
    public void AchievementsTotalSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { AchievementsTotal = 20 };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.AchievementsTotal = 20;

        Assert.False(eventRaised);
    }
}

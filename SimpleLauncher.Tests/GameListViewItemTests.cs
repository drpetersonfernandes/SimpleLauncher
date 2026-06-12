using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="GameListViewItem"/> model class.
/// </summary>
public class GameListViewItemTests
{
    /// <summary>
    /// Verifies that a new GameListViewItem has correct default values.
    /// </summary>
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

    /// <summary>
    /// Verifies that init properties can be set during object creation.
    /// </summary>
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

    /// <summary>
    /// Verifies that setting IsFavorite raises PropertyChanged.
    /// </summary>
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

    /// <summary>
    /// Verifies that setting IsFavorite to the same value does not raise PropertyChanged.
    /// </summary>
    [Fact]
    public void IsFavoriteSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { IsFavorite = true };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.IsFavorite = true;

        Assert.False(eventRaised);
    }

    /// <summary>
    /// Verifies that setting MachineDescription raises PropertyChanged.
    /// </summary>
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

    /// <summary>
    /// Verifies that setting MachineDescription to the same value does not raise PropertyChanged.
    /// </summary>
    [Fact]
    public void MachineDescriptionSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { MachineDescription = "Neo Geo" };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.MachineDescription = "Neo Geo";

        Assert.False(eventRaised);
    }

    /// <summary>
    /// Verifies that setting TimesPlayed raises PropertyChanged.
    /// </summary>
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

    /// <summary>
    /// Verifies that setting TimesPlayed to the same value does not raise PropertyChanged.
    /// </summary>
    [Fact]
    public void TimesPlayedSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { TimesPlayed = "3" };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.TimesPlayed = "3";

        Assert.False(eventRaised);
    }

    /// <summary>
    /// Verifies that setting PlayTime raises PropertyChanged.
    /// </summary>
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

    /// <summary>
    /// Verifies that setting PlayTime to the same value does not raise PropertyChanged.
    /// </summary>
    [Fact]
    public void PlayTimeSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { PlayTime = "10m 0s" };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.PlayTime = "10m 0s";

        Assert.False(eventRaised);
    }

    /// <summary>
    /// Verifies that setting HasAchievements raises PropertyChanged.
    /// </summary>
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

    /// <summary>
    /// Verifies that setting AchievementsEarned raises PropertyChanged.
    /// </summary>
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

    /// <summary>
    /// Verifies that setting AchievementsTotal raises PropertyChanged.
    /// </summary>
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

    /// <summary>
    /// Verifies that setting AchievementsEarned to the same value does not raise PropertyChanged.
    /// </summary>
    [Fact]
    public void AchievementsEarnedSameValueDoesNotRaisePropertyChanged()
    {
        var item = new GameListViewItem { AchievementsEarned = 5 };
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        item.AchievementsEarned = 5;

        Assert.False(eventRaised);
    }

    /// <summary>
    /// Verifies that setting AchievementsTotal to the same value does not raise PropertyChanged.
    /// </summary>
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

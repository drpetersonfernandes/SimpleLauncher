using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class PlayHistoryItemExtendedTests
{
    [Fact]
    public void PlayHistoryItemDefaultFileNameIsNull()
    {
        var item = new PlayHistoryItem();
        Assert.Null(item.FileName);
    }

    [Fact]
    public void PlayHistoryItemDefaultSystemNameIsNull()
    {
        var item = new PlayHistoryItem();
        Assert.Null(item.SystemName);
    }

    [Fact]
    public void PlayHistoryItemDefaultTotalPlayTimeIsZero()
    {
        var item = new PlayHistoryItem();
        Assert.Equal(0, item.TotalPlayTime);
    }

    [Fact]
    public void PlayHistoryItemDefaultTimesPlayedIsZero()
    {
        var item = new PlayHistoryItem();
        Assert.Equal(0, item.TimesPlayed);
    }

    [Fact]
    public void PlayHistoryItemDefaultLastPlayDateIsNull()
    {
        var item = new PlayHistoryItem();
        Assert.Null(item.LastPlayDate);
    }

    [Fact]
    public void PlayHistoryItemDefaultLastPlayTimeIsNull()
    {
        var item = new PlayHistoryItem();
        Assert.Null(item.LastPlayTime);
    }

    [Fact]
    public void PlayHistoryItemDisplayNameReturnsFileName()
    {
        var item = new PlayHistoryItem { FileName = "Super Mario World" };
        Assert.Equal("Super Mario World", item.DisplayName);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeZero()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 0 };
        Assert.Equal("0m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeOneMinute()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 60 };
        Assert.Equal("1m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeOneHour()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 3600 };
        Assert.Equal("1h 0m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeComplex()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 3723 }; // 1h 2m 3s
        Assert.Equal("1h 2m 3s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeOnlySeconds()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 45 };
        Assert.Equal("0m 45s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemAllPropertiesCanBeSet()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 3600,
            TimesPlayed = 10,
            LastPlayDate = "2024-01-15",
            LastPlayTime = "14:30:00"
        };

        Assert.Equal("game.zip", item.FileName);
        Assert.Equal("NES", item.SystemName);
        Assert.Equal(3600, item.TotalPlayTime);
        Assert.Equal(10, item.TimesPlayed);
        Assert.Equal("2024-01-15", item.LastPlayDate);
        Assert.Equal("14:30:00", item.LastPlayTime);
    }

    [Fact]
    public void PlayHistoryItemPropertyChangedTotalPlayTime()
    {
        var item = new PlayHistoryItem();
        var raised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(PlayHistoryItem.TotalPlayTime))
            {
                raised = true;
            }
        };

        item.TotalPlayTime = 100;
        Assert.False(raised);
    }

    [Fact]
    public void PlayHistoryItemPropertyChangedFormattedPlayTime()
    {
        var item = new PlayHistoryItem();
        var raised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(PlayHistoryItem.FormattedPlayTime))
            {
                raised = true;
            }
        };

        item.TotalPlayTime = 100;
        Assert.False(raised);
    }

    [Fact]
    public void PlayHistoryItemSameValueDoesNotRaisePropertyChanged()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 100 };
        var raised = false;
        item.PropertyChanged += (_, _) => { raised = true; };

        item.TotalPlayTime = 100;
        Assert.False(raised);
    }

    [Fact]
    public void PlayHistoryItemWithUnicodeFileName()
    {
        var item = new PlayHistoryItem
        {
            FileName = "ポケモン.zip",
            SystemName = "GBA"
        };

        Assert.Equal("ポケモン.zip", item.DisplayName);
    }

    [Fact]
    public void PlayHistoryItemWithSpecialCharactersInFileName()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game (v1.0) [!].zip"
        };

        Assert.Equal("game (v1.0) [!].zip", item.DisplayName);
    }

    [Fact]
    public void PlayHistoryItemLargePlayTimeValues()
    {
        var item = new PlayHistoryItem
        {
            TotalPlayTime = 999999, // ~11.5 days
            TimesPlayed = 10000
        };

        Assert.Equal(999999, item.TotalPlayTime);
        Assert.Equal(10000, item.TimesPlayed);
        Assert.Contains("h", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemIsoDateFormatVariations()
    {
        var item = new PlayHistoryItem
        {
            LastPlayDate = "2024-12-25",
            LastPlayTime = "23:59:59"
        };

        Assert.Equal("2024-12-25", item.LastPlayDate);
        Assert.Equal("23:59:59", item.LastPlayTime);
    }

    [Fact]
    public void PlayHistoryItemUsDateFormat()
    {
        var item = new PlayHistoryItem
        {
            LastPlayDate = "12/25/2024"
        };

        Assert.Equal("12/25/2024", item.LastPlayDate);
    }
}

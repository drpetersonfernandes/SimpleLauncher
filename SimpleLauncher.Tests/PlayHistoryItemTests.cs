using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class PlayHistoryItemTests
{
    [Fact]
    public void DisplayNameWithFullPathReturnsFileName()
    {
        var item = new PlayHistoryItem
        {
            FileName = "C:\\roms\\Arcade\\game.zip",
            SystemName = "Arcade"
        };

        Assert.Equal("game.zip", item.DisplayName);
    }

    [Fact]
    public void DisplayNameWithFileNameOnlyReturnsFileName()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "Arcade"
        };

        Assert.Equal("game.zip", item.DisplayName);
    }

    [Fact]
    public void DisplayNameWithNullFileNameReturnsEmpty()
    {
        var item = new PlayHistoryItem
        {
            FileName = null,
            SystemName = "Arcade"
        };

        Assert.Equal("", item.DisplayName);
    }

    [Fact]
    public void DisplayNameWithEmptyFileNameReturnsEmpty()
    {
        var item = new PlayHistoryItem
        {
            FileName = "",
            SystemName = "Arcade"
        };

        Assert.Equal("", item.DisplayName);
    }

    [Fact]
    public void FormattedPlayTimeZeroSecondsReturnsZeroMinutesZeroSeconds()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 0 };
        Assert.Equal("0m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeUnderOneHourReturnsMinutesAndSeconds()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 90 }; // 1m 30s
        Assert.Equal("1m 30s", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeExactlyOneHourReturnsHoursMinutesSeconds()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 3600 }; // 1h 0m 0s
        Assert.Equal("1h 0m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeOverOneHourReturnsHoursMinutesSeconds()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 3661 }; // 1h 1m 1s
        Assert.Equal("1h 1m 1s", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeLargeValueReturnsCorrectFormat()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 7384 }; // 2h 3m 4s
        Assert.Equal("2h 3m 4s", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTime59MinutesReturnsMinutesOnly()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 3540 }; // 59m 0s
        Assert.Equal("59m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void PropertyChangedCanBeSubscribed()
    {
        var item = new PlayHistoryItem();
        var eventRaised = false;
        item.PropertyChanged += (_, _) => { eventRaised = true; };

        // Trigger via reflection since OnPropertyChanged is protected
        var method = typeof(PlayHistoryItem).GetMethod("OnPropertyChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(item, ["TestProperty"]);

        Assert.True(eventRaised);
    }

    [Fact]
    public void DefaultValuesAreCorrect()
    {
        var item = new PlayHistoryItem();

        Assert.Null(item.FileName);
        Assert.Null(item.SystemName);
        Assert.Equal(0, item.TotalPlayTime);
        Assert.Equal(0, item.TimesPlayed);
        Assert.Null(item.LastPlayDate);
        Assert.Null(item.LastPlayTime);
    }
}

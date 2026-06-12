using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="PlayHistoryItem"/> covering default property values,
/// unicode and special character handling, property change notifications, and date formats.
/// </summary>
public class PlayHistoryItemExtendedTests
{
    /// <summary>
    /// Verifies that the default FileName is null.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDefaultFileNameIsNull()
    {
        var item = new PlayHistoryItem();
        Assert.Null(item.FileName);
    }

    /// <summary>
    /// Verifies that the default SystemName is null.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDefaultSystemNameIsNull()
    {
        var item = new PlayHistoryItem();
        Assert.Null(item.SystemName);
    }

    /// <summary>
    /// Verifies that the default TotalPlayTime is zero.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDefaultTotalPlayTimeIsZero()
    {
        var item = new PlayHistoryItem();
        Assert.Equal(0, item.TotalPlayTime);
    }

    /// <summary>
    /// Verifies that the default TimesPlayed is zero.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDefaultTimesPlayedIsZero()
    {
        var item = new PlayHistoryItem();
        Assert.Equal(0, item.TimesPlayed);
    }

    /// <summary>
    /// Verifies that the default LastPlayDate is null.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDefaultLastPlayDateIsNull()
    {
        var item = new PlayHistoryItem();
        Assert.Null(item.LastPlayDate);
    }

    /// <summary>
    /// Verifies that the default LastPlayTime is null.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDefaultLastPlayTimeIsNull()
    {
        var item = new PlayHistoryItem();
        Assert.Null(item.LastPlayTime);
    }

    /// <summary>
    /// Verifies that DisplayName returns the FileName when it contains no path separators.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDisplayNameReturnsFileName()
    {
        var item = new PlayHistoryItem { FileName = "Super Mario World" };
        Assert.Equal("Super Mario World", item.DisplayName);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays "0m 0s" for zero total play time.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeZero()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 0 };
        Assert.Equal("0m 0s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays "1m 0s" for exactly 60 seconds.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeOneMinute()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 60 };
        Assert.Equal("1m 0s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays "1h 0m 0s" for exactly 3600 seconds.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeOneHour()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 3600 };
        Assert.Equal("1h 0m 0s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays hours, minutes, and seconds for a complex duration.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeComplex()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 3723 }; // 1h 2m 3s
        Assert.Equal("1h 2m 3s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays "0m 45s" for only seconds with no full minutes.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeOnlySeconds()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 45 };
        Assert.Equal("0m 45s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that all properties of PlayHistoryItem can be set and retrieved.
    /// </summary>
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

    /// <summary>
    /// Verifies that PropertyChanged fires for TotalPlayTime when its value changes.
    /// </summary>
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
        Assert.True(raised);
    }

    /// <summary>
    /// Verifies that PropertyChanged fires for FormattedPlayTime when TotalPlayTime changes.
    /// </summary>
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
        Assert.True(raised);
    }

    /// <summary>
    /// Verifies that PropertyChanged does not fire when the same value is assigned to TotalPlayTime.
    /// </summary>
    [Fact]
    public void PlayHistoryItemSameValueDoesNotRaisePropertyChanged()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 100 };
        var raised = false;
        item.PropertyChanged += (_, _) => { raised = true; };

        item.TotalPlayTime = 100;
        Assert.False(raised);
    }

    /// <summary>
    /// Verifies that DisplayName handles Unicode file names correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that DisplayName handles file names with special characters like parentheses and brackets.
    /// </summary>
    [Fact]
    public void PlayHistoryItemWithSpecialCharactersInFileName()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game (v1.0) [!].zip"
        };

        Assert.Equal("game (v1.0) [!].zip", item.DisplayName);
    }

    /// <summary>
    /// Verifies that PlayHistoryItem correctly stores and formats very large play time values.
    /// </summary>
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

    /// <summary>
    /// Verifies that ISO date format strings are preserved as-is in LastPlayDate and LastPlayTime.
    /// </summary>
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

    /// <summary>
    /// Verifies that US-style date format strings are preserved as-is in LastPlayDate.
    /// </summary>
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

using MessagePack;
using SimpleLauncher.Models;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="Services.PlayHistory.PlayHistoryManager"/> covering
/// large lists, special characters, unicode, version preservation, and property change notifications.
/// </summary>
public class PlayHistoryManagerExtendedTests : IDisposable
{
    private readonly string _testDirectory;

    public PlayHistoryManagerExtendedTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_HistoryExtTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Best-effort cleanup
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that DisplayName extracts the file name without path from a full path.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDisplayNameReturnsFileNameWithoutPath()
    {
        var item = new PlayHistoryItem
        {
            FileName = "C:\\roms\\game.zip",
            SystemName = "NES"
        };

        Assert.Equal("game.zip", item.DisplayName);
    }

    /// <summary>
    /// Verifies that DisplayName returns the file name when it contains no directory path.
    /// </summary>
    [Fact]
    public void PlayHistoryItemDisplayNameHandlesJustFileName()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES"
        };

        Assert.Equal("game.zip", item.DisplayName);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays "0m 0s" for zero total play time.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeZeroSeconds()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 0
        };

        Assert.Equal("0m 0s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays seconds only when under one minute.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeSecondsOnly()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 45
        };

        Assert.Equal("0m 45s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays minutes and seconds for durations under one hour.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeMinutesAndSeconds()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 150 // 2m 30s
        };

        Assert.Equal("2m 30s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime displays hours, minutes, and seconds for durations over one hour.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeHoursMinutesSeconds()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 3661 // 1h 1m 1s
        };

        Assert.Equal("1h 1m 1s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that FormattedPlayTime correctly formats a large value of 24 hours.
    /// </summary>
    [Fact]
    public void PlayHistoryItemFormattedPlayTimeLargeValue()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 86400 // 24h
        };

        Assert.Equal("24h 0m 0s", item.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that a PlayHistoryManager with a single item serializes and deserializes correctly.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerWithSingleItemSerializesCorrectly()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList =
            [
                new PlayHistoryItem
                {
                    FileName = "game.zip",
                    SystemName = "Arcade",
                    TotalPlayTime = 600,
                    TimesPlayed = 3,
                    LastPlayDate = "2024-06-15",
                    LastPlayTime = "20:30:00"
                }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Single(deserialized.PlayHistoryList);
        Assert.Equal(600, deserialized.PlayHistoryList[0].TotalPlayTime);
        Assert.Equal(3, deserialized.PlayHistoryList[0].TimesPlayed);
        Assert.Equal("2024-06-15", deserialized.PlayHistoryList[0].LastPlayDate);
        Assert.Equal("20:30:00", deserialized.PlayHistoryList[0].LastPlayTime);
    }

    /// <summary>
    /// Verifies that file names with special characters survive MessagePack round-trip.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerWithSpecialCharactersInFileName()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList =
            [
                new PlayHistoryItem { FileName = "C:\\roms\\[BIOS] Test.zip", SystemName = "PS1", TotalPlayTime = 100, TimesPlayed = 1 },
                new PlayHistoryItem { FileName = "C:\\roms\\game (v2.0).zip", SystemName = "NES", TotalPlayTime = 200, TimesPlayed = 2 }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal(2, deserialized.PlayHistoryList.Count);
        Assert.Equal("C:\\roms\\[BIOS] Test.zip", deserialized.PlayHistoryList[0].FileName);
        Assert.Equal("C:\\roms\\game (v2.0).zip", deserialized.PlayHistoryList[1].FileName);
    }

    /// <summary>
    /// Verifies that Unicode characters in file names survive MessagePack round-trip.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerWithUnicodeCharacters()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList =
            [
                new PlayHistoryItem { FileName = "C:\\roms\\ポケモン.zip", SystemName = "GBA", TotalPlayTime = 300, TimesPlayed = 5 }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal("C:\\roms\\ポケモン.zip", deserialized.PlayHistoryList[0].FileName);
    }

    /// <summary>
    /// Verifies that a non-default Version value is preserved after serialization.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerVersionPreservedAfterSerialization()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList = [],
            Version = 99
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal(99, deserialized.Version);
    }

    /// <summary>
    /// Verifies that a large list of 1000 items serializes and deserializes correctly.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerLargeListSerializesCorrectly()
    {
        var items = Enumerable.Range(1, 1000)
            .Select(static i => new PlayHistoryItem
            {
                FileName = $"game{i}.zip",
                SystemName = "NES",
                TotalPlayTime = i * 10,
                TimesPlayed = i,
                LastPlayDate = "2024-01-01",
                LastPlayTime = "12:00:00"
            })
            .ToList();

        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList = new System.Collections.ObjectModel.ObservableCollection<PlayHistoryItem>(items),
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal(1000, deserialized.PlayHistoryList.Count);
        Assert.Equal(10, deserialized.PlayHistoryList[0].TotalPlayTime);
        Assert.Equal(10000, deserialized.PlayHistoryList[999].TotalPlayTime);
    }

    /// <summary>
    /// Verifies that zero TimesPlayed and TotalPlayTime values are handled correctly.
    /// </summary>
    [Fact]
    public void PlayHistoryItemZeroTimesPlayed()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 0,
            TimesPlayed = 0
        };

        Assert.Equal(0, item.TimesPlayed);
        Assert.Equal(0, item.TotalPlayTime);
    }

    /// <summary>
    /// Verifies that empty string values for LastPlayDate and LastPlayTime are preserved.
    /// </summary>
    [Fact]
    public void PlayHistoryItemEmptyDateAndTime()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            LastPlayDate = "",
            LastPlayTime = ""
        };

        Assert.Equal("", item.LastPlayDate);
        Assert.Equal("", item.LastPlayTime);
    }

    /// <summary>
    /// Verifies that the order of items in the list is preserved after serialization round-trip.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerPreservesOrder()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList =
            [
                new PlayHistoryItem { FileName = "first.zip", SystemName = "NES", TotalPlayTime = 100, TimesPlayed = 1 },
                new PlayHistoryItem { FileName = "second.zip", SystemName = "SNES", TotalPlayTime = 200, TimesPlayed = 2 },
                new PlayHistoryItem { FileName = "third.zip", SystemName = "GBA", TotalPlayTime = 300, TimesPlayed = 3 }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal("first.zip", deserialized.PlayHistoryList[0].FileName);
        Assert.Equal("second.zip", deserialized.PlayHistoryList[1].FileName);
        Assert.Equal("third.zip", deserialized.PlayHistoryList[2].FileName);
    }

    /// <summary>
    /// Verifies that PropertyChanged fires when TotalPlayTime is modified after initial assignment.
    /// </summary>
    [Fact]
    public void PlayHistoryItemPropertyChangedOnTotalPlayTime()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 100
        };

        var propertyChanged = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(PlayHistoryItem.TotalPlayTime))
            {
                propertyChanged = true;
            }
        };

        item.TotalPlayTime = 200;
        Assert.True(propertyChanged);
    }
}

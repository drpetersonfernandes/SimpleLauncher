using MessagePack;
using SimpleLauncher.Models;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="Services.PlayHistory.PlayHistoryManager"/> covering MessagePack
/// serialization/deserialization, default values, corruption handling, and item ordering.
/// </summary>
public class PlayHistoryManagerTests : IDisposable
{
    private readonly string _testDirectory;

    public PlayHistoryManagerTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_HistoryTest_{Guid.NewGuid():N}");
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
    /// Verifies that PlayHistoryManager can be serialized and deserialized via MessagePack
    /// while preserving all item properties.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerCanBeSerializedAndDeserialized()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList =
            [
                new PlayHistoryItem
                {
                    FileName = "C:\\roms\\game.zip",
                    SystemName = "Arcade",
                    TotalPlayTime = 3600,
                    TimesPlayed = 5,
                    LastPlayDate = "2024-01-15",
                    LastPlayTime = "14:30:00"
                }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.NotNull(deserialized);
        Assert.Single(deserialized.PlayHistoryList);
        Assert.Equal("C:\\roms\\game.zip", deserialized.PlayHistoryList[0].FileName);
        Assert.Equal("Arcade", deserialized.PlayHistoryList[0].SystemName);
        Assert.Equal(3600, deserialized.PlayHistoryList[0].TotalPlayTime);
        Assert.Equal(5, deserialized.PlayHistoryList[0].TimesPlayed);
        Assert.Equal("2024-01-15", deserialized.PlayHistoryList[0].LastPlayDate);
        Assert.Equal("14:30:00", deserialized.PlayHistoryList[0].LastPlayTime);
    }

    /// <summary>
    /// Verifies that an empty PlayHistoryManager serializes and deserializes correctly.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerEmptyListSerializesCorrectly()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList = [],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.NotNull(deserialized);
        Assert.Empty(deserialized.PlayHistoryList);
    }

    /// <summary>
    /// Verifies that the default Version of a new PlayHistoryManager is 1.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerDefaultVersionIsOne()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager();
        Assert.Equal(1, manager.Version);
    }

    /// <summary>
    /// Verifies that the default PlayHistoryList of a new PlayHistoryManager is empty.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerDefaultListIsEmpty()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager();
        Assert.Empty(manager.PlayHistoryList);
    }

    /// <summary>
    /// Verifies that deserializing corrupted MessagePack bytes throws a MessagePackSerializationException.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerCorruptedBytesThrowsException()
    {
        var corruptedBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        Assert.Throws<MessagePackSerializationException>(() =>
            MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(corruptedBytes));
    }

    /// <summary>
    /// Verifies that multiple play history items preserve their order after serialization and deserialization.
    /// </summary>
    [Fact]
    public void PlayHistoryManagerMultipleItemsPreservesOrder()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList =
            [
                new PlayHistoryItem { FileName = "game1.zip", SystemName = "Arcade", TotalPlayTime = 100, TimesPlayed = 1 },
                new PlayHistoryItem { FileName = "game2.nes", SystemName = "NES", TotalPlayTime = 200, TimesPlayed = 2 },
                new PlayHistoryItem { FileName = "game3.smc", SystemName = "SNES", TotalPlayTime = 300, TimesPlayed = 3 }
            ]
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal(3, deserialized.PlayHistoryList.Count);
        Assert.Equal("game1.zip", deserialized.PlayHistoryList[0].FileName);
        Assert.Equal("game2.nes", deserialized.PlayHistoryList[1].FileName);
        Assert.Equal("game3.smc", deserialized.PlayHistoryList[2].FileName);
    }

    /// <summary>
    /// Verifies that ISO date and time format strings are preserved after serialization.
    /// </summary>
    [Fact]
    public void PlayHistoryItemIsoDateFormatIsPreserved()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "Arcade",
            LastPlayDate = "2024-12-25",
            LastPlayTime = "23:59:59"
        };

        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList = [item]
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal("2024-12-25", deserialized.PlayHistoryList[0].LastPlayDate);
        Assert.Equal("23:59:59", deserialized.PlayHistoryList[0].LastPlayTime);
    }

    /// <summary>
    /// Verifies that large play time values serialize and deserialize correctly.
    /// </summary>
    [Fact]
    public void PlayHistoryItemLargePlayTimeSerializesCorrectly()
    {
        var item = new PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "Arcade",
            TotalPlayTime = 86400, // 24 hours
            TimesPlayed = 100
        };

        var manager = new Services.PlayHistory.PlayHistoryManager
        {
            PlayHistoryList = [item]
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal(86400, deserialized.PlayHistoryList[0].TotalPlayTime);
        Assert.Equal(100, deserialized.PlayHistoryList[0].TimesPlayed);
    }

    [Fact]
    public void PlayHistoryItemDisplayNameHandlesJustFileName()
    {
        var item = new PlayHistoryItem { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("game.zip", item.DisplayName);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeSecondsOnly()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 45 };
        Assert.Equal("0m 45s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeMinutesAndSeconds()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 150 }; // 2m 30s
        Assert.Equal("2m 30s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeLargeValue()
    {
        var item = new PlayHistoryItem { TotalPlayTime = 86400 }; // 24h
        Assert.Equal("24h 0m 0s", item.FormattedPlayTime);
    }

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

    [Fact]
    public void PlayHistoryItemZeroTimesPlayed()
    {
        var item = new PlayHistoryItem { FileName = "game.zip", SystemName = "NES", TotalPlayTime = 0, TimesPlayed = 0 };
        Assert.Equal(0, item.TimesPlayed);
        Assert.Equal(0, item.TotalPlayTime);
    }

    [Fact]
    public void PlayHistoryItemEmptyDateAndTime()
    {
        var item = new PlayHistoryItem { FileName = "game.zip", SystemName = "NES", LastPlayDate = "", LastPlayTime = "" };
        Assert.Equal("", item.LastPlayDate);
        Assert.Equal("", item.LastPlayTime);
    }

    [Fact]
    public void PlayHistoryItemPropertyChangedOnTotalPlayTime()
    {
        var item = new PlayHistoryItem { FileName = "game.zip", SystemName = "NES", TotalPlayTime = 100 };
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

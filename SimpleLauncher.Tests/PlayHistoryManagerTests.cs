using MessagePack;
using SimpleLauncher.SharedModels;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

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

    [Fact]
    public void PlayHistoryManagerDefaultVersionIsOne()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager();
        Assert.Equal(1, manager.Version);
    }

    [Fact]
    public void PlayHistoryManagerDefaultListIsEmpty()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager();
        Assert.Empty(manager.PlayHistoryList);
    }

    [Fact]
    public void PlayHistoryManagerCorruptedBytesThrowsException()
    {
        var corruptedBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        Assert.Throws<MessagePackSerializationException>(() =>
            MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(corruptedBytes));
    }

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
}

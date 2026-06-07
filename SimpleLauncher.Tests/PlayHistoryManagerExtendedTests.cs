using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class PlayHistoryManagerExtendedTests : IDisposable
{
    private readonly string _testDirectory;

    public PlayHistoryManagerExtendedTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_PHExtTest_{Guid.NewGuid():N}");
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
            // ignored
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void PlayHistoryItemDisplayNameReturnsFileName()
    {
        var item = new Models.PlayHistoryItem
        {
            FileName = "Super Mario World",
            SystemName = "SNES"
        };

        Assert.Equal("Super Mario World", item.DisplayName);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeZeroSeconds()
    {
        var item = new Models.PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 0
        };

        Assert.Equal("0m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeMinutesAndSeconds()
    {
        var item = new Models.PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 125 // 2m 5s
        };

        Assert.Equal("2m 5s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeHours()
    {
        var item = new Models.PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 3661 // 1h 1m 1s
        };

        Assert.Equal("1h 1m 1s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemFormattedPlayTimeLargeValue()
    {
        var item = new Models.PlayHistoryItem
        {
            FileName = "game.zip",
            SystemName = "NES",
            TotalPlayTime = 86400 // 24h
        };

        Assert.Equal("24h 0m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryItemPropertyChangedFileNameNotRaisedForSimpleSetter()
    {
        var item = new Models.PlayHistoryItem();
        var raised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Models.PlayHistoryItem.FileName))
            {
                raised = true;
            }
        };

        item.FileName = "new.zip";
        Assert.False(raised);
    }

    [Fact]
    public void PlayHistoryItemPropertyChangedSystemNameNotRaisedForSimpleSetter()
    {
        var item = new Models.PlayHistoryItem();
        var raised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Models.PlayHistoryItem.SystemName))
            {
                raised = true;
            }
        };

        item.SystemName = "SNES";
        Assert.False(raised);
    }

    [Fact]
    public void PlayHistoryItemPropertyChangedTimesPlayedNotRaisedForSimpleSetter()
    {
        var item = new Models.PlayHistoryItem();
        var raised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Models.PlayHistoryItem.TimesPlayed))
            {
                raised = true;
            }
        };

        item.TimesPlayed = 5;
        Assert.False(raised);
    }

    [Fact]
    public void PlayHistoryItemPropertyChangedLastPlayDateNotRaisedForSimpleSetter()
    {
        var item = new Models.PlayHistoryItem();
        var raised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Models.PlayHistoryItem.LastPlayDate))
            {
                raised = true;
            }
        };

        item.LastPlayDate = "2024-01-01";
        Assert.False(raised);
    }

    [Fact]
    public void PlayHistoryItemPropertyChangedLastPlayTimeNotRaisedForSimpleSetter()
    {
        var item = new Models.PlayHistoryItem();
        var raised = false;
        item.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Models.PlayHistoryItem.LastPlayTime))
            {
                raised = true;
            }
        };

        item.LastPlayTime = "12:00:00";
        Assert.False(raised);
    }

    [Fact]
    public void PlayHistoryItemDefaultValues()
    {
        var item = new Models.PlayHistoryItem();
        Assert.Equal(0, item.TotalPlayTime);
        Assert.Equal(0, item.TimesPlayed);
        Assert.Equal("0m 0s", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayHistoryManagerSerializesLargeList()
    {
        var manager = new Services.PlayHistory.PlayHistoryManager();
        for (var i = 0; i < 100; i++)
        {
            manager.PlayHistoryList.Add(new Models.PlayHistoryItem
            {
                FileName = $"game{i}.zip",
                SystemName = "NES",
                TotalPlayTime = i * 60,
                TimesPlayed = i
            });
        }

        var bytes = MessagePack.MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePack.MessagePackSerializer.Deserialize<Services.PlayHistory.PlayHistoryManager>(bytes);

        Assert.Equal(100, deserialized.PlayHistoryList.Count);
        Assert.Equal("game50.zip", deserialized.PlayHistoryList[50].FileName);
    }
}

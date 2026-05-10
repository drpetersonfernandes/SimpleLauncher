using MessagePack;
using SimpleLauncher.SharedModels;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class FavoritesManagerTests : IDisposable
{
    private readonly string _testDirectory;

    public FavoritesManagerTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_FavTest_{Guid.NewGuid():N}");
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
    public void FavoritesManagerCanBeSerializedAndDeserialized()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = "C:\\roms\\game1.zip", SystemName = "Arcade" },
                new Favorite { FileName = "C:\\roms\\game2.nes", SystemName = "NES" }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.FavoriteList.Count);
        Assert.Equal("C:\\roms\\game1.zip", deserialized.FavoriteList[0].FileName);
        Assert.Equal("Arcade", deserialized.FavoriteList[0].SystemName);
        Assert.Equal("C:\\roms\\game2.nes", deserialized.FavoriteList[1].FileName);
        Assert.Equal("NES", deserialized.FavoriteList[1].SystemName);
    }

    [Fact]
    public void FavoritesManagerEmptyListSerializesCorrectly()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList = [],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.NotNull(deserialized);
        Assert.Empty(deserialized.FavoriteList);
    }

    [Fact]
    public void FavoritesManagerDefaultVersionIsOne()
    {
        var manager = new Services.Favorites.FavoritesManager();
        Assert.Equal(1, manager.Version);
    }

    [Fact]
    public void FavoritesManagerDefaultListIsEmpty()
    {
        var manager = new Services.Favorites.FavoritesManager();
        Assert.Empty(manager.FavoriteList);
    }

    [Fact]
    public void FavoritesManagerCorruptedBytesThrowsException()
    {
        var corruptedBytes = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        Assert.Throws<MessagePackSerializationException>(() =>
            MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(corruptedBytes));
    }

    [Fact]
    public void FavoriteWithOptionalPropertiesSerializesCorrectly()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite
                {
                    FileName = "game.zip",
                    SystemName = "Arcade",
                    MachineDescription = "Pac-Man",
                    CoverImage = "cover.png"
                }
            ]
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        // Note: MachineDescription and CoverImage are [IgnoreMember] so won't survive serialization
        Assert.Equal("game.zip", deserialized.FavoriteList[0].FileName);
        Assert.Equal("Arcade", deserialized.FavoriteList[0].SystemName);
    }
}

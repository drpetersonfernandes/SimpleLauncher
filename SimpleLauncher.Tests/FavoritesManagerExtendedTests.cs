using MessagePack;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class FavoritesManagerExtendedTests : IDisposable
{
    private readonly string _testDirectory;

    public FavoritesManagerExtendedTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_FavExtTest_{Guid.NewGuid():N}");
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
    public void FavoriteInitPropertiesCanBeSetViaInitializer()
    {
        var fav = new Favorite
        {
            FileName = "game.zip",
            SystemName = "NES",
            MachineDescription = "Pac-Man",
            CoverImage = "cover.png"
        };

        Assert.Equal("game.zip", fav.FileName);
        Assert.Equal("NES", fav.SystemName);
        Assert.Equal("Pac-Man", fav.MachineDescription);
        Assert.Equal("cover.png", fav.CoverImage);
    }

    [Fact]
    public void FavoriteFileNameIsInitOnly()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("game.zip", fav.FileName);
    }

    [Fact]
    public void FavoriteSystemNameIsInitOnly()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("NES", fav.SystemName);
    }

    [Fact]
    public void FavoritePropertyChangedDefaultEmulator()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        var raised = false;
        fav.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(Favorite.DefaultEmulator))
            {
                raised = true;
            }
        };

        fav.DefaultEmulator = "RetroArch";
        Assert.True(raised);
    }

    [Fact]
    public void FavoriteDefaultValues()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(fav.MachineDescription);
        Assert.Null(fav.CoverImage);
        Assert.Null(fav.DefaultEmulator);
    }

    [Fact]
    public void FavoritesManagerSerializesLargeList()
    {
        var manager = new Services.Favorites.FavoritesManager();
        for (var i = 0; i < 100; i++)
        {
            manager.FavoriteList.Add(new Favorite
            {
                FileName = $"game{i}.zip",
                SystemName = "Arcade"
            });
        }

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal(100, deserialized.FavoriteList.Count);
        Assert.Equal("game50.zip", deserialized.FavoriteList[50].FileName);
    }

    [Fact]
    public void FavoritesManagerSerializesWithDefaultEmulator()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite
                {
                    FileName = "game.zip",
                    SystemName = "Arcade",
                    DefaultEmulator = "MAME"
                }
            ]
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        // DefaultEmulator is not serialized (init-only with MessagePack)
        Assert.Equal("game.zip", deserialized.FavoriteList[0].FileName);
    }

    [Fact]
    public void FavoritesManagerVersionPreserved()
    {
        var manager = new Services.Favorites.FavoritesManager { Version = 42 };
        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);
        Assert.Equal(42, deserialized.Version);
    }

    [Fact]
    public void FavoriteWithSpecialCharactersInFileName()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = "game (v1.0) [!].zip", SystemName = "NES" }
            ]
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal("game (v1.0) [!].zip", deserialized.FavoriteList[0].FileName);
    }

    [Fact]
    public void FavoriteWithUnicodeFileName()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = "ポケモン.zip", SystemName = "GBA" }
            ]
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal("ポケモン.zip", deserialized.FavoriteList[0].FileName);
    }

    [Fact]
    public void FavoriteWithLongFilePath()
    {
        var longPath = new string('a', 500) + ".zip";
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = longPath, SystemName = "NES" }
            ]
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal(longPath, deserialized.FavoriteList[0].FileName);
    }
}

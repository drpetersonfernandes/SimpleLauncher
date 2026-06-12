using MessagePack;
using SimpleLauncher.Models;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="SimpleLauncher.Services.Favorites.FavoritesManager"/> covering edge cases, Unicode, and large datasets.
/// </summary>
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
            // Best-effort cleanup
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that a FavoritesManager with a single item serializes and deserializes correctly.
    /// </summary>
    [Fact]
    public void FavoritesManagerWithSingleItemSerializesCorrectly()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = "C:\\roms\\game.zip", SystemName = "Arcade" }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Single(deserialized.FavoriteList);
        Assert.Equal("C:\\roms\\game.zip", deserialized.FavoriteList[0].FileName);
        Assert.Equal("Arcade", deserialized.FavoriteList[0].SystemName);
    }

    /// <summary>
    /// Verifies that duplicate favorite entries are preserved during serialization.
    /// </summary>
    [Fact]
    public void FavoritesManagerWithDuplicateEntriesSerializesCorrectly()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = "game.zip", SystemName = "Arcade" },
                new Favorite { FileName = "game.zip", SystemName = "Arcade" }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal(2, deserialized.FavoriteList.Count);
    }

    /// <summary>
    /// Verifies that special characters in file names survive serialization round-trip.
    /// </summary>
    [Fact]
    public void FavoritesManagerWithSpecialCharactersInFileName()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = "C:\\roms\\[BIOS] PlayStation.zip", SystemName = "PS1" },
                new Favorite { FileName = "C:\\roms\\game (v1.0).zip", SystemName = "NES" },
                new Favorite { FileName = "C:\\roms\\game & friends.zip", SystemName = "SNES" }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal(3, deserialized.FavoriteList.Count);
        Assert.Equal("C:\\roms\\[BIOS] PlayStation.zip", deserialized.FavoriteList[0].FileName);
        Assert.Equal("C:\\roms\\game (v1.0).zip", deserialized.FavoriteList[1].FileName);
        Assert.Equal("C:\\roms\\game & friends.zip", deserialized.FavoriteList[2].FileName);
    }

    /// <summary>
    /// Verifies that Unicode characters in file names survive serialization round-trip.
    /// </summary>
    [Fact]
    public void FavoritesManagerWithUnicodeCharacters()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = "C:\\roms\\ポケモン.zip", SystemName = "GBA" },
                new Favorite { FileName = "C:\\roms\\Jürgen's Game.zip", SystemName = "NES" }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal(2, deserialized.FavoriteList.Count);
        Assert.Equal("C:\\roms\\ポケモン.zip", deserialized.FavoriteList[0].FileName);
        Assert.Equal("C:\\roms\\Jürgen's Game.zip", deserialized.FavoriteList[1].FileName);
    }

    /// <summary>
    /// Verifies that the Favorite model's CoverImage defaults to null.
    /// </summary>
    [Fact]
    public void FavoriteModelDefaultCoverImageIsNull()
    {
        var favorite = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(favorite.CoverImage);
    }

    /// <summary>
    /// Verifies that the Favorite model's MachineDescription defaults to null.
    /// </summary>
    [Fact]
    public void FavoriteModelDefaultMachineDescriptionIsNull()
    {
        var favorite = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(favorite.MachineDescription);
    }

    /// <summary>
    /// Verifies that the Favorite model's DefaultEmulator defaults to null.
    /// </summary>
    [Fact]
    public void FavoriteModelDefaultDefaultEmulatorIsNull()
    {
        var favorite = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(favorite.DefaultEmulator);
    }

    /// <summary>
    /// Verifies that CoverImage can be set and retrieved on the Favorite model.
    /// </summary>
    [Fact]
    public void FavoriteModelCoverImageCanBeSet()
    {
        var favorite = new Favorite
        {
            FileName = "game.zip",
            SystemName = "NES",
            CoverImage = "cover.png"
        };
        Assert.Equal("cover.png", favorite.CoverImage);
    }

    /// <summary>
    /// Verifies that DefaultEmulator can be set and retrieved on the Favorite model.
    /// </summary>
    [Fact]
    public void FavoriteModelDefaultEmulatorCanBeSet()
    {
        var favorite = new Favorite
        {
            FileName = "game.zip",
            SystemName = "NES",
            DefaultEmulator = "RetroArch"
        };
        Assert.Equal("RetroArch", favorite.DefaultEmulator);
    }

    /// <summary>
    /// Verifies that the Version property is preserved after serialization round-trip.
    /// </summary>
    [Fact]
    public void FavoritesManagerVersionPreservedAfterSerialization()
    {
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList = [],
            Version = 42
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal(42, deserialized.Version);
    }

    /// <summary>
    /// Verifies that a large list of 1000 favorites serializes and deserializes correctly.
    /// </summary>
    [Fact]
    public void FavoritesManagerLargeListSerializesCorrectly()
    {
        var favorites = Enumerable.Range(1, 1000)
            .Select(static i => new Favorite { FileName = $"game{i}.zip", SystemName = "NES" })
            .ToList();

        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList = new System.Collections.ObjectModel.ObservableCollection<Favorite>(favorites),
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal(1000, deserialized.FavoriteList.Count);
        Assert.Equal("game1.zip", deserialized.FavoriteList[0].FileName);
        Assert.Equal("game1000.zip", deserialized.FavoriteList[999].FileName);
    }

    /// <summary>
    /// Verifies that long file paths survive serialization round-trip.
    /// </summary>
    [Fact]
    public void FavoritesManagerLongPathsSerializesCorrectly()
    {
        var longPath = "C:\\" + string.Join("\\", Enumerable.Repeat("verylongfoldername", 10)) + "\\game.zip";
        var manager = new Services.Favorites.FavoritesManager
        {
            FavoriteList =
            [
                new Favorite { FileName = longPath, SystemName = "NES" }
            ],
            Version = 1
        };

        var bytes = MessagePackSerializer.Serialize(manager);
        var deserialized = MessagePackSerializer.Deserialize<Services.Favorites.FavoritesManager>(bytes);

        Assert.Equal(longPath, deserialized.FavoriteList[0].FileName);
    }
}

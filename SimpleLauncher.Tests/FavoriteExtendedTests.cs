using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class FavoriteExtendedTests
{
    [Fact]
    public void FavoriteFileNameIsRequiredInit()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("game.zip", fav.FileName);
    }

    [Fact]
    public void FavoriteSystemNameIsRequiredInit()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "SNES" };
        Assert.Equal("SNES", fav.SystemName);
    }

    [Fact]
    public void FavoriteMachineDescriptionDefaultIsNull()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(fav.MachineDescription);
    }

    [Fact]
    public void FavoriteCoverImageDefaultIsNull()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(fav.CoverImage);
    }

    [Fact]
    public void FavoriteDefaultEmulatorDefaultIsNull()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(fav.DefaultEmulator);
    }

    [Fact]
    public void FavoriteAllPropertiesCanBeSet()
    {
        var fav = new Favorite
        {
            FileName = "game.zip",
            SystemName = "Arcade",
            MachineDescription = "Pac-Man",
            CoverImage = "pacman.png",
            DefaultEmulator = "MAME"
        };

        Assert.Equal("game.zip", fav.FileName);
        Assert.Equal("Arcade", fav.SystemName);
        Assert.Equal("Pac-Man", fav.MachineDescription);
        Assert.Equal("pacman.png", fav.CoverImage);
        Assert.Equal("MAME", fav.DefaultEmulator);
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
    public void FavoriteDefaultEmulatorPropertyChanged()
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
    public void FavoriteWithUnicodeCharacters()
    {
        var fav = new Favorite
        {
            FileName = "ポケモン.zip",
            SystemName = "GBA",
            MachineDescription = "ポケットモンスター"
        };

        Assert.Contains("ポケモン", fav.FileName);
        Assert.Contains("ポケットモンスター", fav.MachineDescription);
    }

    [Fact]
    public void FavoriteWithLongFileName()
    {
        var longName = new string('a', 500) + ".zip";
        var fav = new Favorite { FileName = longName, SystemName = "NES" };
        Assert.Equal(longName, fav.FileName);
    }

    [Fact]
    public void FavoriteWithSpecialCharactersInFileName()
    {
        var fav = new Favorite
        {
            FileName = "game (v1.0) [!] {proto}.zip",
            SystemName = "NES"
        };

        Assert.Contains("(", fav.FileName);
        Assert.Contains("[", fav.FileName);
        Assert.Contains("{", fav.FileName);
    }
}

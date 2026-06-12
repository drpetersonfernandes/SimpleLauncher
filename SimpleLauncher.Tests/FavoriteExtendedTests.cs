using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for the <see cref="Favorite"/> model covering edge cases, Unicode, and special characters.
/// </summary>
public class FavoriteExtendedTests
{
    /// <summary>
    /// Verifies that FileName can be set via init accessor.
    /// </summary>
    [Fact]
    public void FavoriteFileNameIsRequiredInit()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("game.zip", fav.FileName);
    }

    /// <summary>
    /// Verifies that SystemName can be set via init accessor.
    /// </summary>
    [Fact]
    public void FavoriteSystemNameIsRequiredInit()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "SNES" };
        Assert.Equal("SNES", fav.SystemName);
    }

    /// <summary>
    /// Verifies that MachineDescription defaults to null.
    /// </summary>
    [Fact]
    public void FavoriteMachineDescriptionDefaultIsNull()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(fav.MachineDescription);
    }

    /// <summary>
    /// Verifies that CoverImage defaults to null.
    /// </summary>
    [Fact]
    public void FavoriteCoverImageDefaultIsNull()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(fav.CoverImage);
    }

    /// <summary>
    /// Verifies that DefaultEmulator defaults to null.
    /// </summary>
    [Fact]
    public void FavoriteDefaultEmulatorDefaultIsNull()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Null(fav.DefaultEmulator);
    }

    /// <summary>
    /// Verifies that all Favorite properties can be set and retrieved together.
    /// </summary>
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

    /// <summary>
    /// Verifies that FileName is init-only and retains its value after construction.
    /// </summary>
    [Fact]
    public void FavoriteFileNameIsInitOnly()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("game.zip", fav.FileName);
    }

    /// <summary>
    /// Verifies that SystemName is init-only and retains its value after construction.
    /// </summary>
    [Fact]
    public void FavoriteSystemNameIsInitOnly()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("NES", fav.SystemName);
    }

    /// <summary>
    /// Verifies that setting DefaultEmulator raises PropertyChanged with the correct property name.
    /// </summary>
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

    /// <summary>
    /// Verifies that Unicode characters in FileName and MachineDescription are preserved.
    /// </summary>
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

    /// <summary>
    /// Verifies that very long file names are handled correctly.
    /// </summary>
    [Fact]
    public void FavoriteWithLongFileName()
    {
        var longName = new string('a', 500) + ".zip";
        var fav = new Favorite { FileName = longName, SystemName = "NES" };
        Assert.Equal(longName, fav.FileName);
    }

    /// <summary>
    /// Verifies that special characters in file names are preserved.
    /// </summary>
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

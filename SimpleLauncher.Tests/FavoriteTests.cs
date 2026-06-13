using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="Favorite"/> model covering required properties, optional defaults, and property change notifications.
/// </summary>
public class FavoriteTests
{
    /// <summary>
    /// Verifies that a Favorite can be created with required FileName and SystemName properties.
    /// </summary>
    [Fact]
    public void FavoriteCanBeCreatedWithRequiredProperties()
    {
        var favorite = new Favorite
        {
            FileName = "C:\\roms\\game.zip",
            SystemName = "Arcade"
        };

        Assert.Equal("C:\\roms\\game.zip", favorite.FileName);
        Assert.Equal("Arcade", favorite.SystemName);
    }

    /// <summary>
    /// Verifies that optional properties default to null when only required properties are set.
    /// </summary>
    [Fact]
    public void FavoriteOptionalPropertiesDefaultToNull()
    {
        var favorite = new Favorite
        {
            FileName = "game.zip",
            SystemName = "NES"
        };

        Assert.Null(favorite.MachineDescription);
        Assert.Null(favorite.CoverImage);
        Assert.Null(favorite.DefaultEmulator);
    }

    /// <summary>
    /// Verifies that optional properties can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void FavoriteOptionalPropertiesCanBeSet()
    {
        var favorite = new Favorite
        {
            FileName = "game.zip",
            SystemName = "NES",
            MachineDescription = "Test Machine",
            CoverImage = "cover.png"
        };

        Assert.Equal("Test Machine", favorite.MachineDescription);
        Assert.Equal("cover.png", favorite.CoverImage);
    }

    /// <summary>
    /// Verifies that setting DefaultEmulator raises a PropertyChanged event.
    /// </summary>
    [Fact]
    public void DefaultEmulatorPropertyRaisesPropertyChanged()
    {
        var favorite = new Favorite
        {
            FileName = "game.zip",
            SystemName = "NES"
        };

        var eventRaised = false;
        favorite.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == "DefaultEmulator")
            {
                eventRaised = true;
            }
        };

        favorite.DefaultEmulator = "RetroArch";
        Assert.True(eventRaised);
        Assert.Equal("RetroArch", favorite.DefaultEmulator);
    }

    /// <summary>
    /// Verifies that setting DefaultEmulator to the same value does not raise PropertyChanged.
    /// </summary>
    [Fact]
    public void DefaultEmulatorSameValueDoesNotRaisePropertyChanged()
    {
        var favorite = new Favorite
        {
            FileName = "game.zip",
            SystemName = "NES",
            DefaultEmulator = "RetroArch"
        };

        var eventRaised = false;
        favorite.PropertyChanged += (_, _) => { eventRaised = true; };

        favorite.DefaultEmulator = "RetroArch";
        Assert.False(eventRaised);
    }

    [Fact]
    public void AllPropertiesCanBeSet()
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
    public void FileNameIsInitOnly()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("game.zip", fav.FileName);
    }

    [Fact]
    public void SystemNameIsInitOnly()
    {
        var fav = new Favorite { FileName = "game.zip", SystemName = "NES" };
        Assert.Equal("NES", fav.SystemName);
    }

    [Fact]
    public void UnicodeCharactersArePreserved()
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
    public void LongFileNameIsPreserved()
    {
        var longName = new string('a', 500) + ".zip";
        var fav = new Favorite { FileName = longName, SystemName = "NES" };
        Assert.Equal(longName, fav.FileName);
    }

    [Fact]
    public void SpecialCharactersInFileNameArePreserved()
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

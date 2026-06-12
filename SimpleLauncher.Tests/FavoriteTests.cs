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
}

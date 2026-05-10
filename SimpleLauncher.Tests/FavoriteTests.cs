using SimpleLauncher.SharedModels;
using Xunit;

namespace SimpleLauncher.Tests;

public class FavoriteTests
{
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

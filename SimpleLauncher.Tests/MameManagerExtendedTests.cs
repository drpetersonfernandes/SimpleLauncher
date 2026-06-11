using Xunit;

namespace SimpleLauncher.Tests;

public class MameManagerExtendedTests
{
    [Fact]
    public void MameManagerDefaultMachineNameIsNull()
    {
        var manager = new Services.MameManager.MameManager();
        Assert.Equal("", manager.MachineName);
    }

    [Fact]
    public void MameManagerDefaultDescriptionIsNull()
    {
        var manager = new Services.MameManager.MameManager();
        Assert.Equal("", manager.Description);
    }

    [Fact]
    public void MameManagerPropertiesCanBeSet()
    {
        var manager = new Services.MameManager.MameManager
        {
            MachineName = "pacman",
            Description = "Pac-Man (Midway)"
        };

        Assert.Equal("pacman", manager.MachineName);
        Assert.Equal("Pac-Man (Midway)", manager.Description);
    }

    [Fact]
    public void MameManagerWithSpecialCharacters()
    {
        var manager = new Services.MameManager.MameManager
        {
            MachineName = "sf2ce",
            Description = "Street Fighter II': Champion Edition (World 920313)"
        };

        Assert.Contains("'", manager.Description);
        Assert.Contains("(", manager.Description);
    }

    [Fact]
    public void MameManagerWithUnicodeDescription()
    {
        var manager = new Services.MameManager.MameManager
        {
            MachineName = "game",
            Description = "ゲーム"
        };

        Assert.Equal("ゲーム", manager.Description);
    }

    [Fact]
    public void MameManagerWithEmptyStrings()
    {
        var manager = new Services.MameManager.MameManager
        {
            MachineName = "",
            Description = ""
        };

        Assert.Equal("", manager.MachineName);
        Assert.Equal("", manager.Description);
    }

    [Fact]
    public void MameManagerWithLongDescription()
    {
        var longDesc = new string('A', 500);
        var manager = new Services.MameManager.MameManager
        {
            Description = longDesc
        };

        Assert.Equal(longDesc, manager.Description);
    }
}

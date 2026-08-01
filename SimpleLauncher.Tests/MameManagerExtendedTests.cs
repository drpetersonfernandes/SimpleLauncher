using Xunit;

namespace SimpleLauncher.Tests;

public class MameManagerExtendedTests
{
    /// <summary>
    /// Verifies that the default MachineName is an empty string.
    /// </summary>
    [Fact]
    public void MameManagerDefaultMachineNameIsNull()
    {
        var manager = new Services.MameManager.MameManagerService();
        Assert.Equal("", manager.MachineName);
    }

    /// <summary>
    /// Verifies that the default Description is an empty string.
    /// </summary>
    [Fact]
    public void MameManagerDefaultDescriptionIsNull()
    {
        var manager = new Services.MameManager.MameManagerService();
        Assert.Equal("", manager.Description);
    }

    /// <summary>
    /// Verifies that MameManagerService properties can be set.
    /// </summary>
    [Fact]
    public void MameManagerPropertiesCanBeSet()
    {
        var manager = new Services.MameManager.MameManagerService
        {
            MachineName = "pacman",
            Description = "Pac-Man (Midway)"
        };

        Assert.Equal("pacman", manager.MachineName);
        Assert.Equal("Pac-Man (Midway)", manager.Description);
    }

    /// <summary>
    /// Verifies that MameManagerService properties support special characters.
    /// </summary>
    [Fact]
    public void MameManagerWithSpecialCharacters()
    {
        var manager = new Services.MameManager.MameManagerService
        {
            MachineName = "sf2ce",
            Description = "Street Fighter II': Champion Edition (World 920313)"
        };

        Assert.Contains("'", manager.Description, StringComparison.Ordinal);
        Assert.Contains("(", manager.Description, StringComparison.Ordinal);
    }

    [Fact]
    public void MameManagerWithUnicodeDescription()
    {
        var manager = new Services.MameManager.MameManagerService
        {
            MachineName = "game",
            Description = "ゲーム"
        };

        Assert.Equal("ゲーム", manager.Description);
    }

    [Fact]
    public void MameManagerWithEmptyStrings()
    {
        var manager = new Services.MameManager.MameManagerService
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
        var manager = new Services.MameManager.MameManagerService
        {
            Description = longDesc
        };

        Assert.Equal(longDesc, manager.Description);
    }
}

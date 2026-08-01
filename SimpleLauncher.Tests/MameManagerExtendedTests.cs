using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for the <see cref="MameMachineData"/> model covering default values, property assignment, and edge cases.
/// </summary>
public class MameManagerExtendedTests
{
    /// <summary>
    /// Verifies that the default MachineName is an empty string.
    /// </summary>
    [Fact]
    public void MameManagerDefaultMachineNameIsNull()
    {
        var manager = new MameMachineData();
        Assert.Equal("", manager.MachineName);
    }

    /// <summary>
    /// Verifies that the default Description is an empty string.
    /// </summary>
    [Fact]
    public void MameManagerDefaultDescriptionIsNull()
    {
        var manager = new MameMachineData();
        Assert.Equal("", manager.Description);
    }

    /// <summary>
    /// Verifies that MameMachineData properties can be set.
    /// </summary>
    [Fact]
    public void MameManagerPropertiesCanBeSet()
    {
        var manager = new MameMachineData
        {
            MachineName = "pacman",
            Description = "Pac-Man (Midway)"
        };

        Assert.Equal("pacman", manager.MachineName);
        Assert.Equal("Pac-Man (Midway)", manager.Description);
    }

    /// <summary>
    /// Verifies that MameMachineData properties support special characters.
    /// </summary>
    [Fact]
    public void MameManagerWithSpecialCharacters()
    {
        var manager = new MameMachineData
        {
            MachineName = "sf2ce",
            Description = "Street Fighter II': Champion Edition (World 920313)"
        };

        Assert.Contains("'", manager.Description, StringComparison.Ordinal);
        Assert.Contains("(", manager.Description, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that MameMachineData supports Unicode characters in the Description property.
    /// </summary>
    [Fact]
    public void MameManagerWithUnicodeDescription()
    {
        var manager = new MameMachineData
        {
            MachineName = "game",
            Description = "ゲーム"
        };

        Assert.Equal("ゲーム", manager.Description);
    }

    /// <summary>
    /// Verifies that MameMachineData handles empty string values correctly.
    /// </summary>
    [Fact]
    public void MameManagerWithEmptyStrings()
    {
        var manager = new MameMachineData
        {
            MachineName = "",
            Description = ""
        };

        Assert.Equal("", manager.MachineName);
        Assert.Equal("", manager.Description);
    }

    /// <summary>
    /// Verifies that MameMachineData handles long description strings correctly.
    /// </summary>
    [Fact]
    public void MameManagerWithLongDescription()
    {
        var longDesc = new string('A', 500);
        var manager = new MameMachineData
        {
            Description = longDesc
        };

        Assert.Equal(longDesc, manager.Description);
    }
}

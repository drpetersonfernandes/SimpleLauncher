namespace SimpleLauncher.Models;

/// <summary>
/// Defines a Rockstar Games launcher game entry with its title, name, and executable.
/// </summary>
public class RockstarGameDef
{
    /// <summary>
    /// The unique title identifier for the Rockstar game.
    /// </summary>
    public string TitleId { get; set; } = null!;

    /// <summary>
    /// The display name of the Rockstar game.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The executable file name used to launch the Rockstar game.
    /// </summary>
    public string Exe { get; set; } = null!;
}

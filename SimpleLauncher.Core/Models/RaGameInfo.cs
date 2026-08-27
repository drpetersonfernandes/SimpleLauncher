using MessagePack;

namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents basic RetroAchievements game metadata including console info, achievement count, and ROM hashes.
/// </summary>
[MessagePackObject]
public record RaGameInfo
{
    /// <summary>
    /// Gets or sets the identifier of the game.
    /// </summary>
    [Key(0)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the title of the game.
    /// </summary>
    [Key(1)]
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the console identifier of the game.
    /// </summary>
    [Key(2)]
    public int ConsoleId { get; set; }

    /// <summary>
    /// Gets or sets the name of the console the game runs on.
    /// </summary>
    [Key(3)]
    public string ConsoleName { get; set; } = "";

    /// <summary>
    /// Gets or sets the icon image path of the game.
    /// </summary>
    [Key(4)]
    public string ImageIcon { get; set; } = "";

    /// <summary>
    /// Gets or sets the total number of achievements for the game.
    /// </summary>
    [Key(5)]
    public int NumAchievements { get; set; }

    /// <summary>
    /// Gets or sets the total points available for the game.
    /// </summary>
    [Key(6)]
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets the date the game data was last modified.
    /// </summary>
    [Key(7)]
    public string DateModified { get; set; } = "";

    /// <summary>
    /// Gets or sets the list of ROM hash values associated with the game.
    /// </summary>
    [Key(8)]
    public IList<string> Hashes { get; set; } = [];
}
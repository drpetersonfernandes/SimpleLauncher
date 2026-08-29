using System.Text.Json.Serialization;
using MessagePack;

namespace RetroAchievements.DataFetcher.Models;

/// <summary>
///     Represents a game entry from the RetroAchievements API.
/// </summary>
[MessagePackObject]
public class GameInfo
{
    /// <summary>
    ///     Gets or sets the unique identifier for the game.
    /// </summary>
    [Key(0)]
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the title of the game.
    /// </summary>
    [Key(1)]
    [JsonPropertyName("Title")]
    public required string Title { get; set; }

    /// <summary>
    ///     Gets or sets the console identifier.
    /// </summary>
    [Key(2)]
    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    /// <summary>
    ///     Gets or sets the console name.
    /// </summary>
    [Key(3)]
    [JsonPropertyName("ConsoleName")]
    public required string ConsoleName { get; set; }

    /// <summary>
    ///     Gets or sets the URL of the game's icon image.
    /// </summary>
    [Key(4)]
    [JsonPropertyName("ImageIcon")]
    public required string ImageIcon { get; set; }

    /// <summary>
    ///     Gets or sets the number of achievements for this game.
    /// </summary>
    [Key(5)]
    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; set; }

    /// <summary>
    ///     Gets or sets the total points available for this game.
    /// </summary>
    [Key(6)]
    [JsonPropertyName("Points")]
    public int Points { get; set; }

    /// <summary>
    ///     Gets or sets the date the game was last modified.
    /// </summary>
    [Key(7)]
    [JsonPropertyName("DateModified")]
    public required string DateModified { get; set; }

    /// <summary>
    ///     Gets or sets the list of ROM hashes for this game.
    /// </summary>
    [Key(8)]
    [JsonPropertyName("Hashes")]
    public IList<string> Hashes { get; set; } = [];
}
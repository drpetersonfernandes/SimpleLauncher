using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a user's rank and score for a specific game on RetroAchievements.
/// </summary>
public record RaUserGameRank
{
    /// <summary>
    /// The username of the player.
    /// </summary>
    [JsonPropertyName("User")]
    public string User { get; init; } = "";

    /// <summary>
    /// The unique identifier (ULID) of the user.
    /// </summary>
    [JsonPropertyName("ULID")]
    public string Ulid { get; init; } = "";

    /// <summary>
    /// The user's rank for this game, or null if unranked.
    /// </summary>
    [JsonPropertyName("UserRank")]
    public int? UserRank { get; init; }

    /// <summary>
    /// The total score the user has earned for this game.
    /// </summary>
    [JsonPropertyName("TotalScore")]
    public int TotalScore { get; init; }

    /// <summary>
    /// The date of the user's last award for this game.
    /// </summary>
    [JsonPropertyName("LastAward")]
    public string LastAward { get; init; } = "";
}

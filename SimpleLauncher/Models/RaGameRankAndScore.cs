using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a user's rank and score entry in a game leaderboard on RetroAchievements.
/// </summary>
public record RaGameRankAndScore
{
    /// <summary>
    /// Gets the username of the player.
    /// </summary>
    [JsonPropertyName("User")]
    public string User { get; init; } = "";

    /// <summary>
    /// Gets the ULID identifier of the player.
    /// </summary>
    [JsonPropertyName("ULID")]
    public string Ulid { get; init; } = "";

    /// <summary>
    /// Gets the number of achievements earned by the player.
    /// </summary>
    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; init; }

    /// <summary>
    /// Gets the total score of the player.
    /// </summary>
    [JsonPropertyName("TotalScore")]
    public int TotalScore { get; init; }

    /// <summary>
    /// Gets the total score (alias for TotalScore).
    /// </summary>
    [JsonIgnore]
    public int Score => TotalScore;

    /// <summary>
    /// Gets the date of the player's last award.
    /// </summary>
    [JsonPropertyName("LastAward")]
    public string LastAward { get; init; } = "";

    /// <summary>
    /// Gets the total true points of the player.
    /// </summary>
    [JsonPropertyName("TotalTruePoints")]
    public int? TotalTruePoints { get; init; }

    /// <summary>
    /// Gets the ratio of true points to total score as a percentage.
    /// </summary>
    [JsonIgnore]
    public double TrueRatio => TotalTruePoints.HasValue && TotalScore > 0
        ? (double)TotalTruePoints.Value / TotalScore * 100
        : 0;

    /// <summary>
    /// Gets or sets the user's rank position in the leaderboard.
    /// </summary>
    [JsonIgnore]
    public int Rank { get; set; }
}

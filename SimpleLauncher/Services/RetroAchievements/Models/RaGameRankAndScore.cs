using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a user's rank and score entry in a game leaderboard on RetroAchievements.
/// </summary>
public record RaGameRankAndScore
{
    [JsonPropertyName("User")]
    public string User { get; init; } = "";

    [JsonPropertyName("ULID")]
    public string Ulid { get; init; } = "";

    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; init; }

    [JsonPropertyName("TotalScore")]
    public int TotalScore { get; init; }

    /// <summary>
    /// Gets the total score (alias for TotalScore).
    /// </summary>
    [JsonIgnore]
    public int Score => TotalScore;

    [JsonPropertyName("LastAward")]
    public string LastAward { get; init; } = "";

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

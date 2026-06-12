using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a user's rank and score entry in a game leaderboard on RetroAchievements.
/// </summary>
public record RaGameRankAndScore
{
    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("ULID")]
    public string Ulid { get; set; } = "";

    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; set; }

    [JsonPropertyName("TotalScore")]
    public int TotalScore { get; set; }

    /// <summary>
    /// Gets the total score (alias for TotalScore).
    /// </summary>
    [JsonIgnore]
    public int Score => TotalScore;

    [JsonPropertyName("LastAward")]
    public string LastAward { get; set; } = "";

    [JsonPropertyName("TotalTruePoints")]
    public int? TotalTruePoints { get; set; }

    /// <summary>
    /// Gets the ratio of true points to total score as a percentage.
    /// </summary>
    [JsonIgnore]
    public int TrueRatio => TotalTruePoints.HasValue && TotalScore > 0
        ? (int)((double)TotalTruePoints.Value / TotalScore * 100)
        : 0;

    /// <summary>
    /// Gets or sets the user's rank position in the leaderboard.
    /// </summary>
    [JsonIgnore]
    public int Rank { get; set; }
}

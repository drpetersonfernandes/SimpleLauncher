using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a user's rank and score for a specific game on RetroAchievements.
/// </summary>
public record RaUserGameRank
{
    [JsonPropertyName("User")]
    public string User { get; init; } = "";

    [JsonPropertyName("ULID")]
    public string Ulid { get; init; } = "";

    [JsonPropertyName("UserRank")]
    public int? UserRank { get; init; }

    [JsonPropertyName("TotalScore")]
    public int TotalScore { get; init; }

    [JsonPropertyName("LastAward")]
    public string LastAward { get; init; } = "";
}

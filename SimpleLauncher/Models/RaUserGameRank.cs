using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a specific user's rank and score for a single game.
/// Corresponds to the response from API_GetUserGameRankAndScore.php.
/// </summary>
public class RaUserGameRank
{
    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("ULID")]
    public string Ulid { get; set; } = "";

    [JsonPropertyName("UserRank")]
    public int? UserRank { get; set; } // Nullable as the user might not be ranked

    [JsonPropertyName("TotalScore")]
    public int TotalScore { get; set; }

    [JsonPropertyName("LastAward")]
    public string LastAward { get; set; } = "";
}
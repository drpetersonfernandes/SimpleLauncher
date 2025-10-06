using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents the top entry for a specific leaderboard.
/// </summary>
public class RaGameLeaderboardTopEntry
{
    [JsonPropertyName("User")]
    public string User { get; set; } = string.Empty;

    [JsonPropertyName("ULID")]
    public string Ulid { get; set; } = string.Empty;

    [JsonPropertyName("Score")]
    public string Score { get; set; } = string.Empty;

    [JsonPropertyName("FormattedScore")]
    public string FormattedScore { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single game leaderboard.
/// </summary>
public class RaGameLeaderboard
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("RankAsc")]
    public bool RankAsc { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("Format")]
    public string Format { get; set; } = string.Empty;

    [JsonPropertyName("Author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("AuthorULID")]
    public string AuthorUlid { get; set; } = string.Empty;

    [JsonPropertyName("TopEntry")]
    public RaGameLeaderboardTopEntry TopEntry { get; set; } = new();
}

/// <summary>
/// Represents the full response from the API_GetGameLeaderboards.php endpoint.
/// </summary>
public class RaGameLeaderboardResponse
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Total")]
    public int Total { get; set; }

    [JsonPropertyName("Results")]
    public List<RaGameLeaderboard> Results { get; set; } = new();
}
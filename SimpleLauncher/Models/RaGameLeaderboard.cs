using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents the top entry for a specific leaderboard.
/// </summary>
public class RaGameLeaderboardTopEntry
{
    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("ULID")]
    public string Ulid { get; set; } = "";

    [JsonPropertyName("Score")]
    public string Score { get; set; } = ""; // API returns as string

    [JsonPropertyName("FormattedScore")]
    public string FormattedScore { get; set; } = "";
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
    public string Title { get; set; } = "";

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("Format")]
    public string Format { get; set; } = "";

    [JsonPropertyName("Author")]
    public string Author { get; set; } = "";

    [JsonPropertyName("AuthorULID")]
    public string AuthorUlid { get; set; } = "";

    [JsonPropertyName("TopEntry")]
    public RaGameLeaderboardTopEntry TopEntry { get; set; } = new(); // Initialize to avoid null reference
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
    public List<RaGameLeaderboard> Results { get; set; } = [];
}
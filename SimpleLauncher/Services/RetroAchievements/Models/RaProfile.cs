using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a RetroAchievements user profile with points, rank, and recently played games.
/// </summary>
public record RaProfile
{
    [JsonPropertyName("User")]
    public string User { get; init; } = "";

    [JsonPropertyName("ULID")]
    public string Ulid { get; init; } = "";

    [JsonPropertyName("UserPic")]
    public string UserPic { get; init; } = "";

    [JsonPropertyName("MemberSince")]
    public string MemberSince { get; init; } = "";

    [JsonPropertyName("RichPresenceMsg")]
    public string RichPresenceMsg { get; init; } = "";

    [JsonPropertyName("LastGameID")]
    public int LastGameId { get; init; }

    [JsonPropertyName("ContribCount")]
    public int ContribCount { get; init; }

    [JsonPropertyName("ContribYield")]
    public int ContribYield { get; init; }

    [JsonPropertyName("TotalPoints")]
    public int TotalPoints { get; init; }

    [JsonPropertyName("TotalSoftcorePoints")]
    public int TotalSoftcorePoints { get; init; }

    [JsonPropertyName("TotalTruePoints")]
    public int TotalTruePoints { get; init; }

    [JsonPropertyName("Permissions")]
    public int Permissions { get; init; }

    [JsonPropertyName("Untracked")]
    public int Untracked { get; init; }

    [JsonPropertyName("ID")]
    public int Id { get; init; }

    [JsonPropertyName("UserWallActive")]
    [JsonConverter(typeof(BoolConverter))]
    public bool UserWallActive { get; init; }

    [JsonPropertyName("Motto")]
    public string Motto { get; init; } = "";

    [JsonPropertyName("Rank")]
    public string Rank { get; init; } = "";

    [JsonPropertyName("RecentlyPlayed")]
    public IReadOnlyList<RaRecentlyPlayedGame> RecentlyPlayed { get; init; } = [];
}

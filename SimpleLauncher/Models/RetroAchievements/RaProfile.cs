using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models.RetroAchievements;

public class RaProfile
{
    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("ULID")]
    public string Uuid { get; set; } = "";

    [JsonPropertyName("UserPic")]
    public string UserPic { get; set; } = "";

    [JsonPropertyName("MemberSince")]
    public string MemberSince { get; set; } = "";

    [JsonPropertyName("RichPresenceMsg")]
    public string RichPresenceMsg { get; set; } = "";

    [JsonPropertyName("LastGameID")]
    public int LastGameId { get; set; }

    [JsonPropertyName("ContribCount")]
    public int ContribCount { get; set; }

    [JsonPropertyName("ContribYield")]
    public int ContribYield { get; set; }

    [JsonPropertyName("TotalPoints")]
    public int TotalPoints { get; set; }

    [JsonPropertyName("TotalSoftcorePoints")]
    public int TotalSoftcorePoints { get; set; }

    [JsonPropertyName("TotalTruePoints")]
    public int TotalTruePoints { get; set; }

    [JsonPropertyName("Permissions")]
    public int Permissions { get; set; }

    [JsonPropertyName("Untracked")]
    public int Untracked { get; set; }

    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("UserWallActive")]
    public bool UserWallActive { get; set; }

    [JsonPropertyName("Motto")]
    public string Motto { get; set; } = "";

    [JsonPropertyName("Rank")]
    public string Rank { get; set; } = "";

    [JsonPropertyName("RecentlyPlayed")]
    public List<RaRecentlyPlayedGame> RecentlyPlayed { get; set; } = [];
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

// DTO for API_GetUserProfile.php
public class ApiUserProfile
{
    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("UserPic")]
    public string UserPic { get; set; } = "";

    [JsonPropertyName("MemberSince")]
    public string MemberSince { get; set; } = "";

    [JsonPropertyName("TotalPoints")]
    public int TotalPoints { get; set; }

    [JsonPropertyName("TotalTruePoints")]
    public int TotalTruePoints { get; set; }

    [JsonPropertyName("Rank")]
    public string Rank { get; set; } = "";

    [JsonPropertyName("Motto")]
    public string Motto { get; set; } = "";

    [JsonPropertyName("RecentlyPlayed")]
    public List<ApiRecentlyPlayedGame> RecentlyPlayed { get; set; } = [];
}

public class ApiRecentlyPlayedGame
{
    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    [JsonPropertyName("LastPlayed")]
    public string LastPlayed { get; set; } = "";

    public string GameIconUrl => $"https://retroachievements.org{ImageIcon}";
}
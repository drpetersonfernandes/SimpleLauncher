using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

// DTO for API_GetGameInfoAndUserProgress.php
public class ApiGameProgressResponse
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; set; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; set; }

    [JsonPropertyName("NumAwardedToUser")]
    public int NumAwardedToUser { get; set; }

    [JsonPropertyName("Achievements")]
    public Dictionary<string, ApiAchievement> Achievements { get; set; } = [];
}
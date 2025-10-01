using System;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class ApiAchievement
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("Points")]
    public int Points { get; set; }

    [JsonPropertyName("BadgeName")]
    public string BadgeName { get; set; } = "";

    [JsonPropertyName("DisplayOrder")]
    public int DisplayOrder { get; set; }

    [JsonPropertyName("DateEarned")]
    public DateTime? DateEarned { get; set; }

    [JsonPropertyName("DateEarnedHardcore")]
    public DateTime? DateEarnedHardcore { get; set; }
}
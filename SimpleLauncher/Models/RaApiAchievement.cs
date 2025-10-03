using System;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class RaApiAchievement
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

    [JsonPropertyName("NumAwarded")]
    public int NumAwarded { get; set; }

    [JsonPropertyName("NumAwardedHardcore")]
    public int NumAwardedHardcore { get; set; }

    [JsonPropertyName("Author")]
    public string Author { get; set; } = "";

    [JsonPropertyName("AuthorULID")]
    public string AuthorUlid { get; set; } = "";

    [JsonPropertyName("DateModified")]
    public string DateModified { get; set; } = "";

    [JsonPropertyName("DateCreated")]
    public string DateCreated { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
}
using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models.RetroAchievements;

public record RaEarnedAchievement
{
    [JsonPropertyName("Date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("HardcoreMode")]
    public int HardcoreMode { get; set; } // 1 for hardcore, 0 for casual

    [JsonPropertyName("AchievementID")]
    public int AchievementId { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("BadgeName")]
    public string BadgeName { get; set; } = "";

    [JsonPropertyName("Points")]
    public int Points { get; set; }

    [JsonPropertyName("TrueRatio")]
    public int? TrueRatio { get; set; }

    [JsonPropertyName("Type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("Author")]
    public string Author { get; set; } = "";

    [JsonPropertyName("AuthorULID")]
    public string AuthorUlid { get; set; } = "";

    [JsonPropertyName("GameTitle")]
    public string GameTitle { get; set; } = "";

    [JsonPropertyName("GameIcon")]
    public string GameIcon { get; set; } = "";

    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; set; } = "";

    [JsonPropertyName("CumulScore")]
    public int CumulScore { get; set; }

    [JsonPropertyName("BadgeURL")]
    public string BadgeUrl { get; set; } = "";

    [JsonPropertyName("GameURL")]
    public string GameUrl { get; set; } = "";

    private DateTime? UnlockedDate => DateTime.TryParse(Date, out var dt) ? dt : null;
    public string UnlockedDateDisplay => UnlockedDate?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "N/A";
    public string ModeDisplay => HardcoreMode == 1 ? "Hardcore" : "Casual";
    public string BadgeFullUrl => $"https://retroachievements.org{BadgeUrl}";
    public string GameIconFullUrl => $"https://retroachievements.org{GameIcon}";
}

using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents an achievement earned by a user within a specific time range, including game and badge metadata.
/// </summary>
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

    /// <summary>
    /// Gets a formatted display of the unlock date in local time.
    /// </summary>
    public string UnlockedDateDisplay => UnlockedDate?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "N/A";

    /// <summary>
    /// Gets a display string indicating hardcore or casual mode.
    /// </summary>
    public string ModeDisplay => HardcoreMode == 1 ? "Hardcore" : "Casual";

    /// <summary>
    /// Gets the full URL for the achievement badge image.
    /// </summary>
    public string BadgeFullUrl => $"https://retroachievements.org{BadgeUrl}";

    /// <summary>
    /// Gets the full URL for the game icon image.
    /// </summary>
    public string GameIconFullUrl => $"https://retroachievements.org{GameIcon}";
}

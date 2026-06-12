using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents an achievement earned by a user within a specific time range, including game and badge metadata.
/// </summary>
public record RaEarnedAchievement
{
    [JsonPropertyName("Date")]
    public string Date { get; init; } = "";

    [JsonPropertyName("HardcoreMode")]
    public int HardcoreMode { get; init; } // 1 for hardcore, 0 for casual

    [JsonPropertyName("AchievementID")]
    public int AchievementId { get; init; }

    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("Description")]
    public string Description { get; init; } = "";

    [JsonPropertyName("BadgeName")]
    public string BadgeName { get; init; } = "";

    [JsonPropertyName("Points")]
    public int Points { get; init; }

    [JsonPropertyName("TrueRatio")]
    public int? TrueRatio { get; init; }

    [JsonPropertyName("Type")]
    public string Type { get; init; } = "";

    [JsonPropertyName("Author")]
    public string Author { get; init; } = "";

    [JsonPropertyName("AuthorULID")]
    public string AuthorUlid { get; init; } = "";

    [JsonPropertyName("GameTitle")]
    public string GameTitle { get; init; } = "";

    [JsonPropertyName("GameIcon")]
    public string GameIcon { get; init; } = "";

    [JsonPropertyName("GameID")]
    public int GameId { get; init; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; init; } = "";

    [JsonPropertyName("CumulScore")]
    public int CumulScore { get; init; }

    [JsonPropertyName("BadgeURL")]
    public string BadgeUrl { get; init; } = "";

    [JsonPropertyName("GameURL")]
    public string GameUrl { get; init; } = "";

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

using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Models;

/// <summary>
///     Represents an achievement earned by a user within a specific time range, including game and badge metadata.
/// </summary>
public record RaEarnedAchievement
{
    /// <summary>
    ///     Gets the raw date string when the achievement was earned.
    /// </summary>
    [JsonPropertyName("Date")]
    public string Date { get; init; } = "";

    /// <summary>
    ///     Gets a value indicating whether the achievement was earned in hardcore mode (1 for hardcore, 0 for casual).
    /// </summary>
    [JsonPropertyName("HardcoreMode")]
    public int HardcoreMode { get; init; } // 1 for hardcore, 0 for casual

    /// <summary>
    ///     Gets the identifier of the earned achievement.
    /// </summary>
    [JsonPropertyName("AchievementID")]
    public int AchievementId { get; init; }

    /// <summary>
    ///     Gets the title of the earned achievement.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    /// <summary>
    ///     Gets the description of the earned achievement.
    /// </summary>
    [JsonPropertyName("Description")]
    public string Description { get; init; } = "";

    /// <summary>
    ///     Gets the badge name of the earned achievement.
    /// </summary>
    [JsonPropertyName("BadgeName")]
    public string BadgeName { get; init; } = "";

    /// <summary>
    ///     Gets the points awarded for the earned achievement.
    /// </summary>
    [JsonPropertyName("Points")]
    public int Points { get; init; }

    /// <summary>
    ///     Gets the weighted point value of the earned achievement.
    /// </summary>
    [JsonPropertyName("TrueRatio")]
    public int? TrueRatio { get; init; }

    /// <summary>
    ///     Gets the type of the earned achievement.
    /// </summary>
    [JsonPropertyName("Type")]
    public string Type { get; init; } = "";

    /// <summary>
    ///     Gets the author of the earned achievement.
    /// </summary>
    [JsonPropertyName("Author")]
    public string Author { get; init; } = "";

    /// <summary>
    ///     Gets the ULID identifier of the achievement author.
    /// </summary>
    [JsonPropertyName("AuthorULID")]
    public string AuthorUlid { get; init; } = "";

    /// <summary>
    ///     Gets the title of the game the achievement belongs to.
    /// </summary>
    [JsonPropertyName("GameTitle")]
    public string GameTitle { get; init; } = "";

    /// <summary>
    ///     Gets the icon path of the game the achievement belongs to.
    /// </summary>
    [JsonPropertyName("GameIcon")]
    public string GameIcon { get; init; } = "";

    /// <summary>
    ///     Gets the identifier of the game the achievement belongs to.
    /// </summary>
    [JsonPropertyName("GameID")]
    public int GameId { get; init; }

    /// <summary>
    ///     Gets the name of the console the game runs on.
    /// </summary>
    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; init; } = "";

    /// <summary>
    ///     Gets the cumulative score of the user.
    /// </summary>
    [JsonPropertyName("CumulScore")]
    public int CumulScore { get; init; }

    /// <summary>
    ///     Gets the relative URL of the achievement badge image.
    /// </summary>
    [JsonPropertyName("BadgeURL")]
    public string BadgeUrl { get; init; } = "";

    /// <summary>
    ///     Gets the relative URL of the game page.
    /// </summary>
    [JsonPropertyName("GameURL")]
    public string GameUrl { get; init; } = "";

    private DateTime? UnlockedDate => DateTime.TryParse(Date, CultureInfo.InvariantCulture, out var dt) ? dt : null;

    /// <summary>
    ///     Gets a formatted display of the unlock date in local time.
    /// </summary>
    public string UnlockedDateDisplay =>
        UnlockedDate?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "N/A";

    /// <summary>
    ///     Gets a display string indicating hardcore or casual mode.
    /// </summary>
    public string ModeDisplay => HardcoreMode == 1 ? "Hardcore" : "Casual";

    /// <summary>
    ///     Gets the full URL for the achievement badge image.
    /// </summary>
    public string BadgeFullUrl => $"https://retroachievements.org{BadgeUrl}";

    /// <summary>
    ///     Gets the full URL for the game icon image.
    /// </summary>
    public string GameIconFullUrl => $"https://retroachievements.org{GameIcon}";
}
using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents raw achievement data from the RetroAchievements API with parsed date helpers.
/// </summary>
public record RaApiAchievement
{
    /// <summary>
    /// Gets the unique identifier of the achievement.
    /// </summary>
    [JsonPropertyName("ID")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the title of the achievement.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    /// <summary>
    /// Gets the description of the achievement.
    /// </summary>
    [JsonPropertyName("Description")]
    public string Description { get; init; } = "";

    /// <summary>
    /// Gets the points awarded for earning the achievement.
    /// </summary>
    [JsonPropertyName("Points")]
    public int Points { get; init; }

    /// <summary>
    /// Gets the badge name of the achievement.
    /// </summary>
    [JsonPropertyName("BadgeName")]
    public string BadgeName { get; init; } = "";

    /// <summary>
    /// Gets the display order of the achievement.
    /// </summary>
    [JsonPropertyName("DisplayOrder")]
    public int DisplayOrder { get; init; }

    /// <summary>
    /// Gets the raw date string when the achievement was earned.
    /// </summary>
    [JsonPropertyName("DateEarned")]
    public string DateEarnedString { get; init; } = "";

    /// <summary>
    /// Gets the raw date string when the achievement was earned in hardcore mode.
    /// </summary>
    [JsonPropertyName("DateEarnedHardcore")]
    public string DateEarnedHardcoreString { get; init; } = "";

    /// <summary>
    /// Gets the parsed date when the achievement was earned, or null if not earned.
    /// </summary>
    [JsonIgnore]
    public DateTime? DateEarned => ParseDate(DateEarnedString);

    /// <summary>
    /// Gets the parsed date when the achievement was earned in hardcore mode, or null if not earned.
    /// </summary>
    [JsonIgnore]
    public DateTime? DateEarnedHardcore => ParseDate(DateEarnedHardcoreString);

    /// <summary>
    /// Gets the full URL for the achievement badge image.
    /// </summary>
    [JsonIgnore]
    public string BadgeUri => $"https://retroachievements.org/Badge/{BadgeName}.png";

    /// <summary>
    /// Gets the author name, defaulting to "Unknown" if empty.
    /// </summary>
    [JsonIgnore]
    public string AuthorDisplay => string.IsNullOrWhiteSpace(Author) ? "Unknown" : Author;

    private static DateTime? ParseDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
            return dt;

        return null;
    }

    /// <summary>
    /// Gets the number of users who have earned the achievement.
    /// </summary>
    [JsonPropertyName("NumAwarded")]
    public int NumAwarded { get; init; }

    /// <summary>
    /// Gets the number of users who earned the achievement in hardcore mode.
    /// </summary>
    [JsonPropertyName("NumAwardedHardcore")]
    public int NumAwardedHardcore { get; init; }

    /// <summary>
    /// Gets the author of the achievement.
    /// </summary>
    [JsonPropertyName("Author")]
    public string Author { get; init; } = "";

    /// <summary>
    /// Gets the ULID identifier of the achievement author.
    /// </summary>
    [JsonPropertyName("AuthorULID")]
    public string AuthorUlid { get; init; } = "";

    /// <summary>
    /// Gets the date the achievement was last modified.
    /// </summary>
    [JsonPropertyName("DateModified")]
    public string DateModified { get; init; } = "";

    /// <summary>
    /// Gets the date the achievement was created.
    /// </summary>
    [JsonPropertyName("DateCreated")]
    public string DateCreated { get; init; } = "";

    /// <summary>
    /// Gets the type of the achievement.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    /// <summary>
    /// Gets the weighted point value of the achievement.
    /// </summary>
    [JsonPropertyName("TrueRatio")]
    public int? TrueRatio { get; init; }
}

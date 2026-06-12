using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents raw achievement data from the RetroAchievements API with parsed date helpers.
/// </summary>
public record RaApiAchievement
{
    [JsonPropertyName("ID")]
    public int Id { get; init; }

    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("Description")]
    public string Description { get; init; } = "";

    [JsonPropertyName("Points")]
    public int Points { get; init; }

    [JsonPropertyName("BadgeName")]
    public string BadgeName { get; init; } = "";

    [JsonPropertyName("DisplayOrder")]
    public int DisplayOrder { get; init; }

    [JsonPropertyName("DateEarned")]
    public string DateEarnedString { get; init; } = "";

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

    [JsonPropertyName("NumAwarded")]
    public int NumAwarded { get; init; }

    [JsonPropertyName("NumAwardedHardcore")]
    public int NumAwardedHardcore { get; init; }

    [JsonPropertyName("Author")]
    public string Author { get; init; } = "";

    [JsonPropertyName("AuthorULID")]
    public string AuthorUlid { get; init; } = "";

    [JsonPropertyName("DateModified")]
    public string DateModified { get; init; } = "";

    [JsonPropertyName("DateCreated")]
    public string DateCreated { get; init; } = "";

    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    [JsonPropertyName("TrueRatio")]
    public int? TrueRatio { get; init; }
}

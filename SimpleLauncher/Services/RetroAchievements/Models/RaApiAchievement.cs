using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents raw achievement data from the RetroAchievements API with parsed date helpers.
/// </summary>
public record RaApiAchievement
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
    public string DateEarnedString { get; set; } = "";

    [JsonPropertyName("DateEarnedHardcore")]
    public string DateEarnedHardcoreString { get; set; } = "";

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

    [JsonPropertyName("TrueRatio")]
    public int? TrueRatio { get; set; }
}

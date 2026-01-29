using System;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.RetroAchievements.Models;

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

    [JsonIgnore]
    public DateTime? DateEarned => ParseDate(DateEarnedString);

    [JsonIgnore]
    public DateTime? DateEarnedHardcore => ParseDate(DateEarnedHardcoreString);

    [JsonIgnore]
    public string BadgeUri => $"https://retroachievements.org/Badge/{BadgeName}.png";

    [JsonIgnore]
    public string AuthorDisplay => string.IsNullOrWhiteSpace(Author) ? "Unknown" : Author;

    private static DateTime? ParseDate(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt))
            return dt;

        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new FormatException($"Failed to parse RetroAchievements API date string: '{dateString}'"), "RetroAchievements API date parsing error in RaApiAchievement.");
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
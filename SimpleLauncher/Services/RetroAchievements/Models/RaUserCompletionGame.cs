using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a game in the user's completion progress list with award and achievement counts.
/// </summary>
public record RaUserCompletionGame
{
    [JsonPropertyName("GameID")]
    public int GameId { get; init; }

    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; init; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; init; } = "";

    [JsonPropertyName("MaxPossible")]
    public int MaxPossible { get; init; }

    [JsonPropertyName("NumAwarded")]
    public int NumAwarded { get; init; }

    [JsonPropertyName("NumAwardedHardcore")]
    public int NumAwardedHardcore { get; init; }

    [JsonPropertyName("MostRecentAwardedDate")]
    public string MostRecentAwardedDate { get; init; } = "";

    [JsonPropertyName("HighestAwardKind")]
    public string HighestAwardKind { get; init; } = "";

    [JsonPropertyName("HighestAwardDate")]
    public string HighestAwardDate { get; init; } = "";

    /// <summary>
    /// Gets a formatted display string showing casual completion progress.
    /// </summary>
    public string CompletionDisplay => $"{NumAwarded}/{MaxPossible}";

    /// <summary>
    /// Gets a formatted display string showing hardcore completion progress.
    /// </summary>
    public string HardcoreCompletionDisplay => $"{NumAwardedHardcore}/{MaxPossible}";

    /// <summary>
    /// Gets a formatted display of the most recent award date in local time.
    /// </summary>
    public string MostRecentAwardedDateDisplay => DateTime.TryParse(MostRecentAwardedDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt)
        ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        : "N/A";

    /// <summary>
    /// Gets a formatted display of the highest award date in local time.
    /// </summary>
    public string HighestAwardDateDisplay => DateTime.TryParse(HighestAwardDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt)
        ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        : "N/A";

    /// <summary>
    /// Gets a capitalized display string for the highest award kind, or "None" if empty.
    /// </summary>
    public string HighestAwardKindDisplay => string.IsNullOrWhiteSpace(HighestAwardKind)
        ? "None"
        : CapitalizeFirstLetter(HighestAwardKind);

    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return char.ToUpper(input[0], CultureInfo.InvariantCulture) + input.Substring(1);
    }
}

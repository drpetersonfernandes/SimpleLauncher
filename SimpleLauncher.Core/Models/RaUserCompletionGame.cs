using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents a game in the user's completion progress list with award and achievement counts.
/// </summary>
public record RaUserCompletionGame
{
    /// <summary>
    /// Gets the identifier of the game.
    /// </summary>
    [JsonPropertyName("GameID")]
    public int GameId { get; init; }

    /// <summary>
    /// Gets the title of the game.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    /// <summary>
    /// Gets or sets the icon image path of the game.
    /// </summary>
    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    /// <summary>
    /// Gets the console identifier of the game.
    /// </summary>
    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; init; }

    /// <summary>
    /// Gets the name of the console the game runs on.
    /// </summary>
    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; init; } = "";

    /// <summary>
    /// Gets the maximum number of achievements available for the game.
    /// </summary>
    [JsonPropertyName("MaxPossible")]
    public int MaxPossible { get; init; }

    /// <summary>
    /// Gets the number of achievements the user was awarded.
    /// </summary>
    [JsonPropertyName("NumAwarded")]
    public int NumAwarded { get; init; }

    /// <summary>
    /// Gets the number of hardcore achievements the user was awarded.
    /// </summary>
    [JsonPropertyName("NumAwardedHardcore")]
    public int NumAwardedHardcore { get; init; }

    /// <summary>
    /// Gets the date of the user's most recent award in this game.
    /// </summary>
    [JsonPropertyName("MostRecentAwardedDate")]
    public string MostRecentAwardedDate { get; init; } = "";

    /// <summary>
    /// Gets the kind of the highest award the user earned in this game.
    /// </summary>
    [JsonPropertyName("HighestAwardKind")]
    public string HighestAwardKind { get; init; } = "";

    /// <summary>
    /// Gets the date of the highest award the user earned in this game.
    /// </summary>
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

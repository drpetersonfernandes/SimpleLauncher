using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

public record RaUserCompletionGame
{
    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; set; } = "";

    [JsonPropertyName("MaxPossible")]
    public int MaxPossible { get; set; }

    [JsonPropertyName("NumAwarded")]
    public int NumAwarded { get; set; }

    [JsonPropertyName("NumAwardedHardcore")]
    public int NumAwardedHardcore { get; set; }

    [JsonPropertyName("MostRecentAwardedDate")]
    public string MostRecentAwardedDate { get; set; } = "";

    [JsonPropertyName("HighestAwardKind")]
    public string HighestAwardKind { get; set; } = "";

    [JsonPropertyName("HighestAwardDate")]
    public string HighestAwardDate { get; set; } = "";

    public string CompletionDisplay => $"{NumAwarded}/{MaxPossible}";
    public string HardcoreCompletionDisplay => $"{NumAwardedHardcore}/{MaxPossible}";

    public string MostRecentAwardedDateDisplay => DateTime.TryParse(MostRecentAwardedDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt)
        ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        : "N/A";

    public string HighestAwardDateDisplay => DateTime.TryParse(HighestAwardDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var dt)
        ? dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        : "N/A";

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
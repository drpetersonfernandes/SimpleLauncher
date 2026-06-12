using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a recently played game from the RetroAchievements API with achievement progress and image URLs.
/// </summary>
public record RaRecentlyPlayedGame
{
    [JsonPropertyName("GameID")]
    public int GameId { get; init; }

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; init; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; init; } = "";

    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; init; } = "";

    [JsonPropertyName("ImageTitle")]
    public string ImageTitle { get; init; } = "";

    [JsonPropertyName("ImageIngame")]
    public string ImageIngame { get; init; } = "";

    [JsonPropertyName("ImageBoxArt")]
    public string ImageBoxArt { get; init; } = "";

    [JsonPropertyName("LastPlayed")]
    public string LastPlayed { get; init; } = "";

    [JsonPropertyName("AchievementsTotal")]
    public int AchievementsTotal { get; init; }

    [JsonPropertyName("NumPossibleAchievements")]
    public int NumPossibleAchievements { get; init; }

    [JsonPropertyName("PossibleScore")]
    public int PossibleScore { get; init; }

    [JsonPropertyName("NumAchieved")]
    public int NumAchieved { get; init; }

    [JsonPropertyName("ScoreAchieved")]
    public int ScoreAchieved { get; init; }

    [JsonPropertyName("NumAchievedHardcore")]
    public int NumAchievedHardcore { get; init; }

    [JsonPropertyName("ScoreAchievedHardcore")]
    public int ScoreAchievedHardcore { get; init; }

    /// <summary>
    /// Gets the full URL for the game icon image.
    /// </summary>
    public string GameIconUrl => !string.IsNullOrEmpty(ImageIcon) ? $"https://retroachievements.org{ImageIcon}" : "";

    /// <summary>
    /// Gets the full URL for the game title screen image.
    /// </summary>
    public string TitleUrl => !string.IsNullOrEmpty(ImageTitle) ? $"https://retroachievements.org{ImageTitle}" : "";

    /// <summary>
    /// Gets the full URL for the in-game screenshot image.
    /// </summary>
    public string IngameUrl => !string.IsNullOrEmpty(ImageIngame) ? $"https://retroachievements.org{ImageIngame}" : "";

    /// <summary>
    /// Gets the full URL for the box art image.
    /// </summary>
    public string BoxArtUrl => !string.IsNullOrEmpty(ImageBoxArt) ? $"https://retroachievements.org{ImageBoxArt}" : "";

    /// <summary>
    /// Gets a formatted display string showing casual achievement and score progress.
    /// </summary>
    public string ProgressDisplay => $"{NumAchieved}/{AchievementsTotal} ({ScoreAchieved}/{PossibleScore} pts)";

    /// <summary>
    /// Gets a formatted display string showing hardcore achievement and score progress.
    /// </summary>
    public string HardcoreProgressDisplay => $"{NumAchievedHardcore}/{AchievementsTotal} ({ScoreAchievedHardcore}/{PossibleScore} pts)";
}

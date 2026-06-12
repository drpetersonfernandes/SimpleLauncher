using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a recently played game from the RetroAchievements API with achievement progress and image URLs.
/// </summary>
public record RaRecentlyPlayedGame
{
    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; set; } = "";

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    [JsonPropertyName("ImageTitle")]
    public string ImageTitle { get; set; } = "";

    [JsonPropertyName("ImageIngame")]
    public string ImageIngame { get; set; } = "";

    [JsonPropertyName("ImageBoxArt")]
    public string ImageBoxArt { get; set; } = "";

    [JsonPropertyName("LastPlayed")]
    public string LastPlayed { get; set; } = "";

    [JsonPropertyName("AchievementsTotal")]
    public int AchievementsTotal { get; set; }

    [JsonPropertyName("NumPossibleAchievements")]
    public int NumPossibleAchievements { get; set; }

    [JsonPropertyName("PossibleScore")]
    public int PossibleScore { get; set; }

    [JsonPropertyName("NumAchieved")]
    public int NumAchieved { get; set; }

    [JsonPropertyName("ScoreAchieved")]
    public int ScoreAchieved { get; set; }

    [JsonPropertyName("NumAchievedHardcore")]
    public int NumAchievedHardcore { get; set; }

    [JsonPropertyName("ScoreAchievedHardcore")]
    public int ScoreAchievedHardcore { get; set; }

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

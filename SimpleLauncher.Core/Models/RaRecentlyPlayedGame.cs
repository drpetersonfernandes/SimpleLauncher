using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents a recently played game from the RetroAchievements API with achievement progress and image URLs.
/// </summary>
public record RaRecentlyPlayedGame
{
    /// <summary>
    /// Gets the identifier of the game.
    /// </summary>
    [JsonPropertyName("GameID")]
    public int GameId { get; init; }

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
    /// Gets the title of the game.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    /// <summary>
    /// Gets the icon image path of the game.
    /// </summary>
    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; init; } = "";

    /// <summary>
    /// Gets the title screen image path of the game.
    /// </summary>
    [JsonPropertyName("ImageTitle")]
    public string ImageTitle { get; init; } = "";

    /// <summary>
    /// Gets the in-game screenshot image path of the game.
    /// </summary>
    [JsonPropertyName("ImageIngame")]
    public string ImageIngame { get; init; } = "";

    /// <summary>
    /// Gets the box art image path of the game.
    /// </summary>
    [JsonPropertyName("ImageBoxArt")]
    public string ImageBoxArt { get; init; } = "";

    /// <summary>
    /// Gets the date the game was last played.
    /// </summary>
    [JsonPropertyName("LastPlayed")]
    public string LastPlayed { get; init; } = "";

    /// <summary>
    /// Gets the total number of achievements for the game.
    /// </summary>
    [JsonPropertyName("AchievementsTotal")]
    public int AchievementsTotal { get; init; }

    /// <summary>
    /// Gets the number of possible achievements for the game.
    /// </summary>
    [JsonPropertyName("NumPossibleAchievements")]
    public int NumPossibleAchievements { get; init; }

    /// <summary>
    /// Gets the total possible score for the game.
    /// </summary>
    [JsonPropertyName("PossibleScore")]
    public int PossibleScore { get; init; }

    /// <summary>
    /// Gets the number of achievements the user earned.
    /// </summary>
    [JsonPropertyName("NumAchieved")]
    public int NumAchieved { get; init; }

    /// <summary>
    /// Gets the score the user achieved.
    /// </summary>
    [JsonPropertyName("ScoreAchieved")]
    public int ScoreAchieved { get; init; }

    /// <summary>
    /// Gets the number of hardcore achievements the user earned.
    /// </summary>
    [JsonPropertyName("NumAchievedHardcore")]
    public int NumAchievedHardcore { get; init; }

    /// <summary>
    /// Gets the hardcore score the user achieved.
    /// </summary>
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

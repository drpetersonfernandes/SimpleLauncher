using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class RaRecentlyPlayedGame
{
    [JsonPropertyName("GameID")] // Keep JsonPropertyName for direct deserialization
    public int GameId { get; set; }

    [JsonPropertyName("ConsoleID")] // Keep JsonPropertyName for direct deserialization
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")] // Keep JsonPropertyName for direct deserialization
    public string ConsoleName { get; set; } = string.Empty;

    [JsonPropertyName("Title")] // Keep JsonPropertyName for direct deserialization
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("ImageIcon")] // Keep JsonPropertyName for direct deserialization
    public string ImageIcon { get; set; } = string.Empty; // Raw relative path

    [JsonPropertyName("ImageTitle")] // Keep JsonPropertyName for direct deserialization
    public string ImageTitle { get; set; } = string.Empty;

    [JsonPropertyName("ImageIngame")] // Keep JsonPropertyName for direct deserialization
    public string ImageIngame { get; set; } = string.Empty;

    [JsonPropertyName("ImageBoxArt")] // Keep JsonPropertyName for direct deserialization
    public string ImageBoxArt { get; set; } = string.Empty;

    [JsonPropertyName("LastPlayed")] // Keep JsonPropertyName for direct deserialization
    public string LastPlayed { get; set; } = string.Empty;

    [JsonPropertyName("AchievementsTotal")] // Keep JsonPropertyName for direct deserialization
    public int AchievementsTotal { get; set; }

    [JsonPropertyName("NumPossibleAchievements")] // Keep JsonPropertyName for direct deserialization
    public int NumPossibleAchievements { get; set; }

    [JsonPropertyName("PossibleScore")] // Keep JsonPropertyName for direct deserialization
    public int PossibleScore { get; set; }

    [JsonPropertyName("NumAchieved")] // Keep JsonPropertyName for direct deserialization
    public int NumAchieved { get; set; }

    [JsonPropertyName("ScoreAchieved")] // Keep JsonPropertyName for direct deserialization
    public int ScoreAchieved { get; set; }

    [JsonPropertyName("NumAchievedHardcore")] // Keep JsonPropertyName for direct deserialization
    public int NumAchievedHardcore { get; set; }

    [JsonPropertyName("ScoreAchievedHardcore")] // Keep JsonPropertyName for direct deserialization
    public int ScoreAchievedHardcore { get; set; }

    // Computed full URLs for images (prepend base URL)
    public string GameIconUrl => !string.IsNullOrEmpty(ImageIcon) ? $"https://retroachievements.org{ImageIcon}" : string.Empty;
    public string TitleUrl => !string.IsNullOrEmpty(ImageTitle) ? $"https://retroachievements.org{ImageTitle}" : string.Empty;
    public string IngameUrl => !string.IsNullOrEmpty(ImageIngame) ? $"https://retroachievements.org{ImageIngame}" : string.Empty;
    public string BoxArtUrl => !string.IsNullOrEmpty(ImageBoxArt) ? $"https://retroachievements.org{ImageBoxArt}" : string.Empty;

    // Computed display strings for progress
    public string ProgressDisplay => $"{NumAchieved}/{AchievementsTotal} ({ScoreAchieved}/{PossibleScore} pts)";
    public string HardcoreProgressDisplay => $"{NumAchievedHardcore}/{AchievementsTotal} ({ScoreAchievedHardcore}/{PossibleScore} pts)";
}

using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

public record RaRecentlyPlayedGame
{
    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; set; } = string.Empty;

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = string.Empty;

    [JsonPropertyName("ImageTitle")]
    public string ImageTitle { get; set; } = string.Empty;

    [JsonPropertyName("ImageIngame")]
    public string ImageIngame { get; set; } = string.Empty;

    [JsonPropertyName("ImageBoxArt")]
    public string ImageBoxArt { get; set; } = string.Empty;

    [JsonPropertyName("LastPlayed")]
    public string LastPlayed { get; set; } = string.Empty;

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

    public string GameIconUrl => !string.IsNullOrEmpty(ImageIcon) ? $"https://retroachievements.org{ImageIcon}" : string.Empty;
    public string TitleUrl => !string.IsNullOrEmpty(ImageTitle) ? $"https://retroachievements.org{ImageTitle}" : string.Empty;
    public string IngameUrl => !string.IsNullOrEmpty(ImageIngame) ? $"https://retroachievements.org{ImageIngame}" : string.Empty;
    public string BoxArtUrl => !string.IsNullOrEmpty(ImageBoxArt) ? $"https://retroachievements.org{ImageBoxArt}" : string.Empty;

    public string ProgressDisplay => $"{NumAchieved}/{AchievementsTotal} ({ScoreAchieved}/{PossibleScore} pts)";
    public string HardcoreProgressDisplay => $"{NumAchievedHardcore}/{AchievementsTotal} ({ScoreAchievedHardcore}/{PossibleScore} pts)";
}

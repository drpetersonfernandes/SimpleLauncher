namespace SimpleLauncher.Models;

// Summarized progress
public class UserGameProgress
{
    public string GameTitle { get; set; } = "";
    public string GameIconUrl { get; set; } = "";
    public string ConsoleName { get; set; } = "";
    public int AchievementsEarned { get; set; }
    public int TotalAchievements { get; set; }
    public int PointsEarned { get; set; }
    public int TotalPoints { get; set; }
}

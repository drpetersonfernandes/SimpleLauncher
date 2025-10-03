namespace SimpleLauncher.Models;

public class RaUserGameProgress
{
    public string GameTitle { get; set; } = "";
    public string GameIconUrl { get; set; } = "";
    public string ConsoleName { get; set; } = "";
    public int AchievementsEarned { get; set; }
    public int TotalAchievements { get; set; }
    public int PointsEarned { get; set; }
    public int TotalPoints { get; set; }
    public string UserCompletion { get; set; } = "";
    public string UserCompletionHardcore { get; set; } = "";
    public string HighestAwardKind { get; set; } = "";
    public string HighestAwardDate { get; set; } = "";
}
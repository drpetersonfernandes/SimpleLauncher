namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a user's achievement progress for a specific game, including points and completion percentages.
/// </summary>
public class RaUserGameProgress
{
    public string GameTitle { get; init; } = "";
    public string GameIconUrl { get; init; } = "";
    public string ConsoleName { get; init; } = "";
    public int AchievementsEarned { get; init; }
    public int TotalAchievements { get; init; }
    public int PointsEarned { get; init; }
    public int PointsEarnedHardcore { get; init; }
    public int TotalPoints { get; init; }
    public string UserCompletion { get; init; } = "";
    public string UserCompletionHardcore { get; init; } = "";
    public string HighestAwardKind { get; init; } = "";
    public string HighestAwardDate { get; init; } = "";
}

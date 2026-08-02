namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents a user's achievement progress for a specific game, including points and completion percentages.
/// </summary>
public class RaUserGameProgress
{
    /// <summary>
    /// The title of the game.
    /// </summary>
    public string GameTitle { get; init; } = "";

    /// <summary>
    /// The URL of the game's icon image.
    /// </summary>
    public string GameIconUrl { get; init; } = "";

    /// <summary>
    /// The name of the console or platform the game belongs to.
    /// </summary>
    public string ConsoleName { get; init; } = "";

    /// <summary>
    /// The number of achievements the user has earned for this game.
    /// </summary>
    public int AchievementsEarned { get; init; }

    /// <summary>
    /// The total number of achievements available for this game.
    /// </summary>
    public int TotalAchievements { get; init; }

    /// <summary>
    /// The number of points the user has earned in softcore mode.
    /// </summary>
    public int PointsEarned { get; init; }

    /// <summary>
    /// The number of points the user has earned in hardcore mode.
    /// </summary>
    public int PointsEarnedHardcore { get; init; }

    /// <summary>
    /// The total number of points available for this game.
    /// </summary>
    public int TotalPoints { get; init; }

    /// <summary>
    /// The user's softcore completion percentage for this game.
    /// </summary>
    public string UserCompletion { get; init; } = "";

    /// <summary>
    /// The user's hardcore completion percentage for this game.
    /// </summary>
    public string UserCompletionHardcore { get; init; } = "";

    /// <summary>
    /// The kind of the highest award the user has received for this game.
    /// </summary>
    public string HighestAwardKind { get; init; } = "";

    /// <summary>
    /// The date the highest award was received for this game.
    /// </summary>
    public string HighestAwardDate { get; init; } = "";
}

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents a RetroAchievements achievement with unlock status, metadata, and display helpers.
/// </summary>
public class RaAchievement
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Points { get; set; }
    public string BadgeUri { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? DateUnlocked { get; set; }
    public bool UnlockedInHardcore { get; set; }
    public int DisplayOrder { get; set; }
    public int NumAwarded { get; set; }
    public int NumAwardedHardcore { get; set; }
    public string Author { get; set; } = "";
    public string AuthorUlid { get; set; } = "";
    public string DateModified { get; set; } = "";
    public string DateCreated { get; set; } = "";
    public string BadgeName { get; set; } = "";
    public string Type { get; set; } = "";
    public DateTime? DateEarnedHardcore { get; set; }
    public DateTime? DateEarned { get; set; }
    public int? TrueRatio { get; set; }

    /// <summary>
    /// Gets a formatted display string for the unlock date, including a trophy icon for hardcore unlocks.
    /// </summary>
    public string DateUnlockedDisplay
    {
        get
        {
            if (!IsUnlocked) return "Locked";
            if (DateUnlocked == null) return "N/A";

            return UnlockedInHardcore ? $"🏆 {DateUnlocked.Value:yyyy-MM-dd}" : $"{DateUnlocked.Value:yyyy-MM-dd}";
        }
    }

    /// <summary>
    /// Gets a display string indicating whether the achievement was earned in hardcore, casual, or not earned.
    /// </summary>
    public string ModeDisplay => UnlockedInHardcore ? "Hardcore" : IsUnlocked ? "Casual" : "Not Earned";

    /// <summary>
    /// Gets the author name, defaulting to "Unknown" if empty.
    /// </summary>
    public string AuthorDisplay => string.IsNullOrWhiteSpace(Author) ? "Unknown" : Author;

    /// <summary>
    /// Gets a formatted display string showing the percentage of hardcore earners.
    /// </summary>
    public string RarityDisplay => NumAwarded > 0 && NumAwardedHardcore > 0
        ? $"{(double)NumAwardedHardcore / NumAwarded * 100:F1}% hardcore"
        : "N/A";
}

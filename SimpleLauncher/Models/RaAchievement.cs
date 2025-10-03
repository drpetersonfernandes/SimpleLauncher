using System;

namespace SimpleLauncher.Models;

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
    public string DateUnlockedDisplay => IsUnlocked ? (UnlockedInHardcore ? $"🏆 {DateUnlocked:yyyy-MM-dd}" : $"{DateUnlocked:yyyy-MM-dd}") : "Locked";
    public string ModeDisplay => UnlockedInHardcore ? "Hardcore" : (IsUnlocked ? "Casual" : "Not Earned");
    public string AuthorDisplay => string.IsNullOrWhiteSpace(Author) ? "Unknown" : Author;

    public string RarityDisplay => NumAwarded > 0 && NumAwardedHardcore > 0
        ? $"{((double)NumAwardedHardcore / NumAwarded * 100):F1}% hardcore"
        : "N/A";
}
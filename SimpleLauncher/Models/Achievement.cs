using System;

namespace SimpleLauncher.Models;

public class Achievement
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
    public string DateUnlockedDisplay => IsUnlocked ? (UnlockedInHardcore ? $"🏆 {DateUnlocked:yyyy-MM-dd}" : $"{DateUnlocked:yyyy-MM-dd}") : "Locked";
}
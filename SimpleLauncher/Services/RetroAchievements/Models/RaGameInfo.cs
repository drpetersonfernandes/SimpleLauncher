using System.Collections.Generic;
using MessagePack;

namespace SimpleLauncher.Services.RetroAchievements.Models;

[MessagePackObject]
public record RaGameInfo
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public string Title { get; set; } = string.Empty;

    [Key(2)]
    public int ConsoleId { get; set; }

    [Key(3)]
    public string ConsoleName { get; set; } = string.Empty;

    [Key(4)]
    public string ImageIcon { get; set; } = string.Empty;

    [Key(5)]
    public int NumAchievements { get; set; }

    [Key(6)]
    public int Points { get; set; }

    [Key(7)]
    public string DateModified { get; set; } = string.Empty;

    [Key(8)]
    public List<string> Hashes { get; set; } = [];
}
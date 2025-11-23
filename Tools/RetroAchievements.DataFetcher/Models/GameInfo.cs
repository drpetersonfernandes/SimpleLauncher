using System.Text.Json.Serialization;
using MessagePack;

namespace RetroAchievements.DataFetcher.Models;

/// <summary>
/// Represents a game entry from the RetroAchievements API.
/// </summary>
[MessagePackObject]
public class GameInfo
{
    [Key(0)]
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [Key(1)]
    [JsonPropertyName("Title")]
    public required string Title { get; set; }

    [Key(2)]
    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [Key(3)]
    [JsonPropertyName("ConsoleName")]
    public required string ConsoleName { get; set; }

    [Key(4)]
    [JsonPropertyName("ImageIcon")]
    public required string ImageIcon { get; set; }

    [Key(5)]
    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; set; }

    [Key(6)]
    [JsonPropertyName("Points")]
    public int Points { get; set; }

    [Key(7)]
    [JsonPropertyName("DateModified")]
    public required string DateModified { get; set; }

    [Key(8)]
    [JsonPropertyName("Hashes")]
    public List<string> Hashes { get; set; } = [];
}
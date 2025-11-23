using System.Text.Json.Serialization;
using MessagePack;

namespace RetroAchievements.DataFetcher.Models;

[MessagePackObject]
public class ConsoleInfo
{
    [Key(0)]
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [Key(1)]
    [JsonPropertyName("Name")]
    public required string Name { get; set; }

    [Key(2)]
    [JsonPropertyName("IconURL")]
    public string? IconUrl { get; set; }

    [Key(3)]
    [JsonPropertyName("Active")]
    public bool Active { get; set; }

    [Key(4)]
    [JsonPropertyName("IsGameSystem")]
    public bool IsGameSystem { get; set; }
}
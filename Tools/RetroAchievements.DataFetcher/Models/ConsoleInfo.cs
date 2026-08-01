using System.Text.Json.Serialization;
using MessagePack;

namespace RetroAchievements.DataFetcher.Models;

/// <summary>
/// Represents console/system information from the RetroAchievements API.
/// </summary>
[MessagePackObject]
public class ConsoleInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the console.
    /// </summary>
    [Key(0)]
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the console.
    /// </summary>
    [Key(1)]
    [JsonPropertyName("Name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the URL of the console's icon.
    /// </summary>
    [Key(2)]
    [JsonPropertyName("IconURL")]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the console is active.
    /// </summary>
    [Key(3)]
    [JsonPropertyName("Active")]
    public bool Active { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a game system.
    /// </summary>
    [Key(4)]
    [JsonPropertyName("IsGameSystem")]
    public bool IsGameSystem { get; set; }
}
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class RaApiRecentlyPlayedGame
{
    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    [JsonPropertyName("LastPlayed")]
    public string LastPlayed { get; set; } = "";

    public string GameIconUrl => $"https://retroachievements.org{ImageIcon}";
}
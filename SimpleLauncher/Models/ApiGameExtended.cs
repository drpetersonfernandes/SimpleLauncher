using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

// DTO for API_GetGameExtended.php
public class ApiGameExtended
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; set; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    [JsonPropertyName("Publisher")]
    public string Publisher { get; set; } = "";

    [JsonPropertyName("Developer")]
    public string Developer { get; set; } = "";

    [JsonPropertyName("Genre")]
    public string Genre { get; set; } = "";

    [JsonPropertyName("Released")]
    public string Released { get; set; } = "";

    [JsonPropertyName("RichPresencePatch")]
    public string RichPresencePatch { get; set; } = "";
}
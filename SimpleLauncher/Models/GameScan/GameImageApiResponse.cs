using System.Text.Json.Serialization;

namespace SimpleLauncher.Models.GameScan;

public class GameImageApiResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; }
}
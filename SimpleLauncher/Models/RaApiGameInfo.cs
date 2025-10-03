using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class RaApiGameInfo
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";
}
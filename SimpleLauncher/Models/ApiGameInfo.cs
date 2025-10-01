using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

// DTO for API_GetGameList.php
public class ApiGameInfo
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";
}
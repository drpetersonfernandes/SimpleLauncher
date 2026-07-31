using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.GameScan.Models;

public class GogPlayTask
{
    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!; // "FileTask" or "URLTask"

    [JsonPropertyName("path")]
    public string Path { get; set; } = null!;

    [JsonPropertyName("workingDir")]
    public string WorkingDir { get; set; } = null!;
}

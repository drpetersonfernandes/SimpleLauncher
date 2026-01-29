using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models.GameScan;

public class GogGameInfo
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; }

    [JsonPropertyName("rootGameId")]
    public string RootGameId { get; set; }

    [JsonPropertyName("playTasks")]
    public List<GogPlayTask> PlayTasks { get; set; }
}

public class GogPlayTask
{
    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } // "FileTask" or "URLTask"

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("workingDir")]
    public string WorkingDir { get; set; }
}
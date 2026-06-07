using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Services.GameScan.Models;

public class GogGameInfo
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; }

    [JsonPropertyName("rootGameId")]
    public string RootGameId { get; set; }

    [JsonPropertyName("playTasks")]
    public List<GogPlayTask> PlayTasks { get; set; }
}
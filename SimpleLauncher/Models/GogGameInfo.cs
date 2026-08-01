using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class GogGameInfo
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = null!;

    [JsonPropertyName("rootGameId")]
    public string RootGameId { get; set; } = null!;

    [JsonPropertyName("playTasks")]
    public IList<GogPlayTask> PlayTasks { get; set; } = null!;
}

using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents metadata about a GOG game retrieved from the GOG Galaxy installation.
/// </summary>
public class GogGameInfo
{
    /// <summary>
    /// Gets or sets the GOG game identifier.
    /// </summary>
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the root game identifier for multi-part GOG games.
    /// </summary>
    [JsonPropertyName("rootGameId")]
    public string RootGameId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the list of play tasks that describe how to launch the game.
    /// </summary>
    [JsonPropertyName("playTasks")]
    public IList<GogPlayTask> PlayTasks { get; set; } = null!;
}

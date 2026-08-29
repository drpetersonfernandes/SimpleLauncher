using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Models;

/// <summary>
///     Represents a launch task for a GOG game, such as a file task or URL task.
/// </summary>
public class GogPlayTask
{
    /// <summary>
    ///     Gets or sets whether this task is the primary launch task for the game.
    /// </summary>
    [JsonPropertyName("isPrimary")]
    public bool IsPrimary { get; set; }

    /// <summary>
    ///     Gets or sets the type of the play task ("FileTask" or "URLTask").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = null!; // "FileTask" or "URLTask"

    /// <summary>
    ///     Gets or sets the executable or URL path of the play task.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the working directory used when launching the task.
    /// </summary>
    [JsonPropertyName("workingDir")]
    public string WorkingDir { get; set; } = null!;
}
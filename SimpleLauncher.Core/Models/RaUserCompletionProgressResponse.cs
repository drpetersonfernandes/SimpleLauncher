using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Models;

/// <summary>
///     Represents the paginated API response for user completion progress containing a list of completed games.
/// </summary>
public record RaUserCompletionProgressResponse
{
    /// <summary>
    ///     Gets the number of results returned in this page.
    /// </summary>
    [JsonPropertyName("Count")]
    public int Count { get; init; }

    /// <summary>
    ///     Gets the total number of results available.
    /// </summary>
    [JsonPropertyName("Total")]
    public int Total { get; init; }

    /// <summary>
    ///     Gets the list of completion progress games in this page.
    /// </summary>
    [JsonPropertyName("Results")]
    public IList<RaUserCompletionGame> Results { get; init; } = [];
}
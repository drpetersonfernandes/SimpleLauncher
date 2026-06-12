using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents the paginated API response for user completion progress containing a list of completed games.
/// </summary>
public record RaUserCompletionProgressResponse
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Total")]
    public int Total { get; set; }

    [JsonPropertyName("Results")]
    public List<RaUserCompletionGame> Results { get; set; } = [];
}

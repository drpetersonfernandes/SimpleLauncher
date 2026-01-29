using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

public record RaUserCompletionProgressResponse
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Total")]
    public int Total { get; set; }

    [JsonPropertyName("Results")]
    public List<RaUserCompletionGame> Results { get; set; } = [];
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class RaUserCompletionProgressResponse
{
    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Total")]
    public int Total { get; set; }

    [JsonPropertyName("Results")]
    public List<RaUserCompletionGame> Results { get; set; } = [];
}
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

// DTO for API_GetGameRankAndScore.php
public class ApiGameRankAndScore
{
    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("Score")]
    public int Score { get; set; }

    [JsonPropertyName("TrueRatio")]
    public int TrueRatio { get; set; }

    public int Rank { get; set; }
}

public class ApiGameRankAndScoreResponse
{
    [JsonPropertyName("Top10")]
    public List<ApiGameRankAndScore> Top10 { get; set; } = [];
}
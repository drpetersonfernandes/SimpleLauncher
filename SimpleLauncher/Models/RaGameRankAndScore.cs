using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class RaGameRankAndScore
{
    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("ULID")]
    public string Ulid { get; set; } = "";

    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; set; }

    [JsonPropertyName("TotalScore")]
    public int TotalScore { get; set; }

    [JsonIgnore]
    public int Score => TotalScore;

    [JsonPropertyName("LastAward")]
    public string LastAward { get; set; } = "";

    [JsonPropertyName("TotalTruePoints")]
    public int? TotalTruePoints { get; set; }

    [JsonIgnore]
    public int TrueRatio => TotalTruePoints.HasValue && TotalScore > 0
        ? (int)((double)TotalTruePoints.Value / TotalScore * 100)
        : 0;

    [JsonIgnore]
    public int Rank { get; set; }
}
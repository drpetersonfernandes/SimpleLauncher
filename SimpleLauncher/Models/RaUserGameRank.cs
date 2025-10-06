using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class RaUserGameRank
{
    [JsonPropertyName("User")]
    public string User { get; set; } = "";

    [JsonPropertyName("ULID")]
    public string Ulid { get; set; } = "";

    [JsonPropertyName("UserRank")]
    public int? UserRank { get; set; }

    [JsonPropertyName("TotalScore")]
    public int TotalScore { get; set; }

    [JsonPropertyName("LastAward")]
    public string LastAward { get; set; } = "";
}
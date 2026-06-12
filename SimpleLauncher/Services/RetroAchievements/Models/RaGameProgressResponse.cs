using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents the API response for game progress, containing game metadata and user achievement data.
/// </summary>
public record RaGameProgressResponse
{
    [JsonPropertyName("ID")]
    public int Id { get; init; }

    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; init; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; init; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; init; } = "";

    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; init; }

    [JsonPropertyName("NumAwardedToUser")]
    public int NumAwardedToUser { get; init; }

    [JsonPropertyName("Achievements")]
    public IReadOnlyDictionary<string, RaApiAchievement> Achievements { get; init; } = new Dictionary<string, RaApiAchievement>();

    [JsonPropertyName("ForumTopicID")]
    public int? ForumTopicId { get; init; }

    [JsonPropertyName("Flags")]
    public object Flags { get; init; }

    [JsonPropertyName("ImageTitle")]
    public string ImageTitle { get; init; } = "";

    [JsonPropertyName("ImageIngame")]
    public string ImageIngame { get; init; } = "";

    [JsonPropertyName("ImageBoxArt")]
    public string ImageBoxArt { get; init; } = "";

    [JsonPropertyName("Publisher")]
    public string Publisher { get; init; } = "";

    [JsonPropertyName("Developer")]
    public string Developer { get; init; } = "";

    [JsonPropertyName("Genre")]
    public string Genre { get; init; } = "";

    [JsonPropertyName("Released")]
    public string Released { get; init; } = "";

    [JsonPropertyName("ReleasedAtGranularity")]
    public string ReleasedAtGranularity { get; init; } = "";

    [JsonPropertyName("IsFinal")]
    public bool IsFinal { get; init; }

    [JsonPropertyName("RichPresencePatch")]
    public string RichPresencePatch { get; init; } = "";

    [JsonPropertyName("GuideURL")]
    public string GuideUrl { get; init; } = "";

    [JsonPropertyName("ParentGameID")]
    public int? ParentGameId { get; init; }

    [JsonPropertyName("NumDistinctPlayers")]
    public int NumDistinctPlayers { get; init; }

    [JsonPropertyName("NumAwardedToUserHardcore")]
    public int NumAwardedToUserHardcore { get; init; }

    [JsonPropertyName("NumDistinctPlayersCasual")]
    public int NumDistinctPlayersCasual { get; init; }

    [JsonPropertyName("NumDistinctPlayersHardcore")]
    public int NumDistinctPlayersHardcore { get; init; }

    [JsonPropertyName("UserCompletion")]
    public string UserCompletion { get; init; } = "";

    [JsonPropertyName("UserCompletionHardcore")]
    public string UserCompletionHardcore { get; init; } = "";

    [JsonPropertyName("HighestAwardKind")]
    public string HighestAwardKind { get; init; } = "";

    [JsonPropertyName("HighestAwardDate")]
    public string HighestAwardDate { get; init; } = "";
}

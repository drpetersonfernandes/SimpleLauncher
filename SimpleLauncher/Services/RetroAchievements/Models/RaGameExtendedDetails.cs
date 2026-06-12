using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Represents extended game details from the RetroAchievements API, including media, metadata, and achievement data.
/// </summary>
public record RaGameExtendedDetails
{
    [JsonPropertyName("ID")]
    public int Id { get; init; }

    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; init; }

    [JsonPropertyName("ForumTopicID")]
    public int? ForumTopicId { get; init; }

    [JsonPropertyName("Flags")]
    public object Flags { get; init; }

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; init; } = "";

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
    [JsonConverter(typeof(BoolConverter))]
    public bool IsFinal { get; init; }

    [JsonPropertyName("RichPresencePatch")]
    public string RichPresencePatch { get; init; } = "";

    [JsonPropertyName("GuideURL")]
    public string GuideUrl { get; init; } = "";

    [JsonPropertyName("Updated")]
    public string Updated { get; init; } = "";

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; init; } = "";

    [JsonPropertyName("ParentGameID")]
    public int? ParentGameId { get; init; }

    [JsonPropertyName("NumDistinctPlayers")]
    public int NumDistinctPlayers { get; init; }

    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; init; }

    [JsonPropertyName("Achievements")]
    public IReadOnlyDictionary<string, RaApiAchievement> Achievements { get; init; } = new Dictionary<string, RaApiAchievement>();

    [JsonPropertyName("Claims")]
    public IReadOnlyList<object> Claims { get; init; } = [];

    [JsonPropertyName("NumDistinctPlayersCasual")]
    public int NumDistinctPlayersCasual { get; init; }

    [JsonPropertyName("NumDistinctPlayersHardcore")]
    public int NumDistinctPlayersHardcore { get; init; }
}

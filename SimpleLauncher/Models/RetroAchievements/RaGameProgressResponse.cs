using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models.RetroAchievements;

public class RaGameProgressResponse
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = "";

    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; set; }

    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; set; } = "";

    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; set; } = "";

    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; set; }

    [JsonPropertyName("NumAwardedToUser")]
    public int NumAwardedToUser { get; set; }

    [JsonPropertyName("Achievements")]
    public Dictionary<string, RaApiAchievement> Achievements { get; set; } = [];

    [JsonPropertyName("ForumTopicID")]
    public int? ForumTopicId { get; set; }

    [JsonPropertyName("Flags")]
    public object Flags { get; set; }

    [JsonPropertyName("ImageTitle")]
    public string ImageTitle { get; set; } = "";

    [JsonPropertyName("ImageIngame")]
    public string ImageIngame { get; set; } = "";

    [JsonPropertyName("ImageBoxArt")]
    public string ImageBoxArt { get; set; } = "";

    [JsonPropertyName("Publisher")]
    public string Publisher { get; set; } = "";

    [JsonPropertyName("Developer")]
    public string Developer { get; set; } = "";

    [JsonPropertyName("Genre")]
    public string Genre { get; set; } = "";

    [JsonPropertyName("Released")]
    public string Released { get; set; } = "";

    [JsonPropertyName("ReleasedAtGranularity")]
    public string ReleasedAtGranularity { get; set; } = "";

    [JsonPropertyName("IsFinal")]
    public bool IsFinal { get; set; }

    [JsonPropertyName("RichPresencePatch")]
    public string RichPresencePatch { get; set; } = "";

    [JsonPropertyName("GuideURL")]
    public string GuideUrl { get; set; } = "";

    [JsonPropertyName("ParentGameID")]
    public int? ParentGameId { get; set; }

    [JsonPropertyName("NumDistinctPlayers")]
    public int NumDistinctPlayers { get; set; }

    [JsonPropertyName("NumAwardedToUserHardcore")]
    public int NumAwardedToUserHardcore { get; set; }

    [JsonPropertyName("NumDistinctPlayersCasual")]
    public int NumDistinctPlayersCasual { get; set; }

    [JsonPropertyName("NumDistinctPlayersHardcore")]
    public int NumDistinctPlayersHardcore { get; set; }

    [JsonPropertyName("UserCompletion")]
    public string UserCompletion { get; set; } = "";

    [JsonPropertyName("UserCompletionHardcore")]
    public string UserCompletionHardcore { get; set; } = "";

    [JsonPropertyName("HighestAwardKind")]
    public string HighestAwardKind { get; set; } = "";

    [JsonPropertyName("HighestAwardDate")]
    public string HighestAwardDate { get; set; } = "";
}
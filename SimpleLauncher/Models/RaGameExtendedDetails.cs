using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents extended game details from the RetroAchievements API, including media, metadata, and achievement data.
/// </summary>
public record RaGameExtendedDetails
{
    /// <summary>
    /// Gets the identifier of the game.
    /// </summary>
    [JsonPropertyName("ID")]
    public int Id { get; init; }

    /// <summary>
    /// Gets the title of the game.
    /// </summary>
    [JsonPropertyName("Title")]
    public string Title { get; init; } = "";

    /// <summary>
    /// Gets the console identifier of the game.
    /// </summary>
    [JsonPropertyName("ConsoleID")]
    public int ConsoleId { get; init; }

    /// <summary>
    /// Gets the forum topic identifier for the game.
    /// </summary>
    [JsonPropertyName("ForumTopicID")]
    public int? ForumTopicId { get; init; }

    /// <summary>
    /// Gets the flags of the game.
    /// </summary>
    [JsonPropertyName("Flags")]
    public object Flags { get; init; } = null!;

    /// <summary>
    /// Gets the icon image path of the game.
    /// </summary>
    [JsonPropertyName("ImageIcon")]
    public string ImageIcon { get; init; } = "";

    /// <summary>
    /// Gets the title image path of the game.
    /// </summary>
    [JsonPropertyName("ImageTitle")]
    public string ImageTitle { get; init; } = "";

    /// <summary>
    /// Gets the in-game screenshot image path of the game.
    /// </summary>
    [JsonPropertyName("ImageIngame")]
    public string ImageIngame { get; init; } = "";

    /// <summary>
    /// Gets the box art image path of the game.
    /// </summary>
    [JsonPropertyName("ImageBoxArt")]
    public string ImageBoxArt { get; init; } = "";

    /// <summary>
    /// Gets the publisher of the game.
    /// </summary>
    [JsonPropertyName("Publisher")]
    public string Publisher { get; init; } = "";

    /// <summary>
    /// Gets the developer of the game.
    /// </summary>
    [JsonPropertyName("Developer")]
    public string Developer { get; init; } = "";

    /// <summary>
    /// Gets the genre of the game.
    /// </summary>
    [JsonPropertyName("Genre")]
    public string Genre { get; init; } = "";

    /// <summary>
    /// Gets the release date of the game.
    /// </summary>
    [JsonPropertyName("Released")]
    public string Released { get; init; } = "";

    /// <summary>
    /// Gets the granularity of the release date.
    /// </summary>
    [JsonPropertyName("ReleasedAtGranularity")]
    public string ReleasedAtGranularity { get; init; } = "";

    /// <summary>
    /// Gets whether the game is in its final state.
    /// </summary>
    [JsonPropertyName("IsFinal")]
    [JsonConverter(typeof(BoolConverter))]
    public bool IsFinal { get; init; }

    /// <summary>
    /// Gets the rich presence patch data of the game.
    /// </summary>
    [JsonPropertyName("RichPresencePatch")]
    public string RichPresencePatch { get; init; } = "";

    /// <summary>
    /// Gets the guide URL of the game.
    /// </summary>
    [JsonPropertyName("GuideURL")]
    public string GuideUrl { get; init; } = "";

    /// <summary>
    /// Gets the date the game data was last updated.
    /// </summary>
    [JsonPropertyName("Updated")]
    public string Updated { get; init; } = "";

    /// <summary>
    /// Gets the name of the console the game runs on.
    /// </summary>
    [JsonPropertyName("ConsoleName")]
    public string ConsoleName { get; init; } = "";

    /// <summary>
    /// Gets the identifier of the parent game, if this is a sub-game.
    /// </summary>
    [JsonPropertyName("ParentGameID")]
    public int? ParentGameId { get; init; }

    /// <summary>
    /// Gets the number of distinct players of the game.
    /// </summary>
    [JsonPropertyName("NumDistinctPlayers")]
    public int NumDistinctPlayers { get; init; }

    /// <summary>
    /// Gets the total number of achievements for the game.
    /// </summary>
    [JsonPropertyName("NumAchievements")]
    public int NumAchievements { get; init; }

    /// <summary>
    /// Gets the collection of achievements for the game, keyed by achievement ID.
    /// </summary>
    [JsonPropertyName("Achievements")]
    public IReadOnlyDictionary<string, RaApiAchievement> Achievements { get; init; } = new Dictionary<string, RaApiAchievement>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the list of claims for the game.
    /// </summary>
    [JsonPropertyName("Claims")]
    public IReadOnlyList<object> Claims { get; init; } = [];

    /// <summary>
    /// Gets the number of distinct casual players of the game.
    /// </summary>
    [JsonPropertyName("NumDistinctPlayersCasual")]
    public int NumDistinctPlayersCasual { get; init; }

    /// <summary>
    /// Gets the number of distinct hardcore players of the game.
    /// </summary>
    [JsonPropertyName("NumDistinctPlayersHardcore")]
    public int NumDistinctPlayersHardcore { get; init; }
}

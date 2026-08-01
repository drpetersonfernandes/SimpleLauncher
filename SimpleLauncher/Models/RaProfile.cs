using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a RetroAchievements user profile with points, rank, and recently played games.
/// </summary>
public record RaProfile
{
    /// <summary>
    /// Gets the username of the profile.
    /// </summary>
    [JsonPropertyName("User")]
    public string User { get; init; } = "";

    /// <summary>
    /// Gets the ULID identifier of the user.
    /// </summary>
    [JsonPropertyName("ULID")]
    public string Ulid { get; init; } = "";

    /// <summary>
    /// Gets the path to the user's avatar picture.
    /// </summary>
    [JsonPropertyName("UserPic")]
    public string UserPic { get; init; } = "";

    /// <summary>
    /// Gets the date the user joined the site.
    /// </summary>
    [JsonPropertyName("MemberSince")]
    public string MemberSince { get; init; } = "";

    /// <summary>
    /// Gets the rich presence message of the user's last played game.
    /// </summary>
    [JsonPropertyName("RichPresenceMsg")]
    public string RichPresenceMsg { get; init; } = "";

    /// <summary>
    /// Gets the identifier of the user's last played game.
    /// </summary>
    [JsonPropertyName("LastGameID")]
    public int LastGameId { get; init; }

    /// <summary>
    /// Gets the number of contributions the user has made to the site.
    /// </summary>
    [JsonPropertyName("ContribCount")]
    public int ContribCount { get; init; }

    /// <summary>
    /// Gets the contribution yield of the user.
    /// </summary>
    [JsonPropertyName("ContribYield")]
    public int ContribYield { get; init; }

    /// <summary>
    /// Gets the total points earned by the user.
    /// </summary>
    [JsonPropertyName("TotalPoints")]
    public int TotalPoints { get; init; }

    /// <summary>
    /// Gets the total softcore points earned by the user.
    /// </summary>
    [JsonPropertyName("TotalSoftcorePoints")]
    public int TotalSoftcorePoints { get; init; }

    /// <summary>
    /// Gets the total true points earned by the user.
    /// </summary>
    [JsonPropertyName("TotalTruePoints")]
    public int TotalTruePoints { get; init; }

    /// <summary>
    /// Gets the permission level of the user.
    /// </summary>
    [JsonPropertyName("Permissions")]
    public int Permissions { get; init; }

    /// <summary>
    /// Gets whether the user's account is untracked.
    /// </summary>
    [JsonPropertyName("Untracked")]
    public int Untracked { get; init; }

    /// <summary>
    /// Gets the identifier of the user account.
    /// </summary>
    [JsonPropertyName("ID")]
    public int Id { get; init; }

    /// <summary>
    /// Gets whether the user's wall is active.
    /// </summary>
    [JsonPropertyName("UserWallActive")]
    [JsonConverter(typeof(BoolConverter))]
    public bool UserWallActive { get; init; }

    /// <summary>
    /// Gets the motto of the user.
    /// </summary>
    [JsonPropertyName("Motto")]
    public string Motto { get; init; } = "";

    /// <summary>
    /// Gets the rank of the user on the site.
    /// </summary>
    [JsonPropertyName("Rank")]
    public string Rank { get; init; } = "";

    /// <summary>
    /// Gets the list of games the user recently played.
    /// </summary>
    [JsonPropertyName("RecentlyPlayed")]
    public IReadOnlyList<RaRecentlyPlayedGame> RecentlyPlayed { get; init; } = [];
}

namespace RetroAchievements.DataFetcher.Models;

/// <summary>
///     Settings for connecting to the RetroAchievements API.
/// </summary>
public class RaSettings
{
    /// <summary>
    ///     Gets or sets the RetroAchievements username.
    /// </summary>
    public string Username { get; set; } = "";

    /// <summary>
    ///     Gets or sets the RetroAchievements web API key.
    /// </summary>
    public string WebApiKey { get; set; } = "";
}
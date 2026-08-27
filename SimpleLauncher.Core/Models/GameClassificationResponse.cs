namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents the response containing a list of classified games.
/// </summary>
public class GameClassificationResponse
{
    /// <summary>
    /// Gets or sets the list of classified games in the response.
    /// </summary>
    public IList<GameClassificationItem> Games { get; set; } = [];
}
#nullable enable

namespace SimpleLauncher.Models;

/// <summary>
/// Metadata attached to a game button in the UI grid.
/// </summary>
public class GameButtonTag
{
    /// <summary>
    /// Gets or sets whether the button is displaying the default placeholder image.
    /// </summary>
    public bool IsDefaultImage { get; set; }

    /// <summary>
    /// Gets or sets the unique key used to identify this game entry.
    /// </summary>
    public string Key { get; set; } = "";
}

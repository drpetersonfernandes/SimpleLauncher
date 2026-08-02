namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents a Battle.net application definition with identifiers and executable information.
/// </summary>
public class BNetAppDef
{
    /// <summary>
    /// Gets or sets the internal identifier for the Battle.net application.
    /// </summary>
    public string InternalId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the display name of the application.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets whether the application is a classic Blizzard game.
    /// </summary>
    public bool IsClassic { get; set; }

    /// <summary>
    /// Gets or sets the executable file name for the application.
    /// </summary>
    public string Exe { get; set; } = null!;

    /// <summary>
    /// Gets or sets the product identifier used by Battle.net.
    /// </summary>
    public string ProductId { get; set; } = null!;
}

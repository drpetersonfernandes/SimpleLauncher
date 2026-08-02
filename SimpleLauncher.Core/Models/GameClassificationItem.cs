namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents a classified game entry detected on the system with its installation details.
/// </summary>
public class GameClassificationItem
{
    /// <summary>
    /// Gets or sets the display name of the game.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets the application identifier of the game.
    /// </summary>
    public string AppId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the installation location of the game.
    /// </summary>
    public string InstallLocation { get; set; } = null!;

    /// <summary>
    /// Gets or sets the package family name for MSIX packaged games.
    /// </summary>
    public string PackageFamilyName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the relative path to the game's logo image.
    /// </summary>
    public string LogoRelativePath { get; set; } = null!;
}

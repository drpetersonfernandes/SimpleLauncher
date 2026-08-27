namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to locate cover images for games.
/// </summary>
public interface IFindCoverImageService
{
    /// <summary>
    /// Finds the cover image path for a game based on its file name and system.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The game file name without its extension.</param>
    /// <param name="systemName">The name of the game system.</param>
    /// <param name="systemImageFolder">The path to the system's image folder.</param>
    /// <returns>The path to the cover image file.</returns>
    string FindCoverImagePath(string fileNameWithoutExtension, string systemName, string systemImageFolder);
}
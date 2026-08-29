namespace SimpleLauncher.Interfaces;

/// <summary>
///     Provides methods to manage check mark states for various menu items in the launcher.
/// </summary>
public interface IMenuCheckMarkService
{
    /// <summary>
    ///     Initializes the service with the specified menu check mark host.
    /// </summary>
    /// <param name="host">The host providing access to menu item references.</param>
    void Initialize(IMenuCheckMarkHost host);

    /// <summary>
    ///     Updates the check marks for thumbnail size menu items based on the selected size.
    /// </summary>
    /// <param name="selectedSize">The currently selected thumbnail size.</param>
    void UpdateThumbnailSizeCheckMarks(int selectedSize);

    /// <summary>
    ///     Updates the check marks for games-per-page menu items based on the selected count.
    /// </summary>
    /// <param name="selectedSize">The currently selected number of games per page.</param>
    void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize);

    /// <summary>
    ///     Updates the check marks for the show games filter menu items.
    /// </summary>
    /// <param name="selectedValue">The currently selected show games mode.</param>
    void UpdateShowGamesCheckMarks(string selectedValue);

    /// <summary>
    ///     Updates the check marks for button aspect ratio menu items.
    /// </summary>
    /// <param name="selectedValue">The currently selected aspect ratio.</param>
    void UpdateButtonAspectRatioCheckMarks(string selectedValue);

    /// <summary>
    ///     Updates the check marks for filename display mode menu items.
    /// </summary>
    /// <param name="selectedValue">The currently selected display mode.</param>
    void UpdateFilenameDisplayModeCheckMarks(string selectedValue);

    /// <summary>
    ///     Updates the check marks for filename font size menu items.
    /// </summary>
    /// <param name="selectedValue">The currently selected font size.</param>
    void UpdateFilenameFontSizeCheckMarks(string selectedValue);

    /// <summary>
    ///     Updates the check marks for machine name font size menu items.
    /// </summary>
    /// <param name="selectedValue">The currently selected font size.</param>
    void UpdateMachineNameFontSizeCheckMarks(string selectedValue);

    /// <summary>
    ///     Sets the view mode and updates the corresponding menu check marks.
    /// </summary>
    /// <param name="viewMode">The view mode to set (e.g., "Grid" or "List").</param>
    void SetViewMode(string viewMode);
}
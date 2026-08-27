using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

/// <summary>
/// Orchestrates menu actions, check mark management, theme changes, and language switching in the launcher.
/// </summary>
public interface IMenuOrchestrator
{
    /// <summary>
    /// Initializes the orchestrator with the required host dependencies.
    /// </summary>
    /// <param name="actionHost">The host providing menu action operations.</param>
    /// <param name="checkMarkHost">The host providing check mark menu items.</param>
    /// <param name="themeHost">The host providing theme menu access.</param>
    /// <param name="languageHost">The host providing language menu access.</param>
    void Initialize(IMenuActionHost actionHost, IMenuCheckMarkHost checkMarkHost, IThemeMenuHost themeHost,
        ILanguageMenuHost languageHost);

    /// <summary>
    /// Shows the emulator configuration window for the specified emulator.
    /// </summary>
    /// <param name="emulatorName">The name of the emulator to configure.</param>
    void ShowEmulatorConfigWindow(string emulatorName);

    /// <summary>
    /// Handles switching to easy mode.
    /// </summary>
    void HandleEasyMode();

    /// <summary>
    /// Handles switching to expert mode.
    /// </summary>
    void HandleExpertMode();

    /// <summary>
    /// Handles downloading an image pack.
    /// </summary>
    void HandleDownloadImagePack();

    /// <summary>
    /// Scans for Windows games installed on the system.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleScanForWindowsGamesAsync();

    /// <summary>
    /// Handles editing game links asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleEditLinksAsync();

    /// <summary>
    /// Handles toggling gamepad support on or off.
    /// </summary>
    /// <param name="isChecked">True to enable gamepad; false to disable.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleToggleGamepadAsync(bool isChecked);

    /// <summary>
    /// Handles setting the gamepad dead zone value.
    /// </summary>
    void HandleSetGamepadDeadZone();

    /// <summary>
    /// Handles toggling fuzzy matching on or off.
    /// </summary>
    /// <param name="isChecked">True to enable fuzzy matching; false to disable.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleToggleFuzzyMatchingAsync(bool isChecked);

    /// <summary>
    /// Handles setting the fuzzy matching threshold value.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleSetFuzzyMatchingThresholdAsync();

    /// <summary>
    /// Handles toggling annotation stripping on or off.
    /// </summary>
    /// <param name="isChecked">True to enable annotation stripping; false to disable.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleToggleAnnotationStrippingAsync(bool isChecked);

    /// <summary>
    /// Handles showing the support dialog.
    /// </summary>
    void HandleSupport();

    /// <summary>
    /// Handles showing the donation link asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleDonateAsync();

    /// <summary>
    /// Handles showing the about dialog.
    /// </summary>
    void HandleAbout();

    /// <summary>
    /// Handles exiting the application.
    /// </summary>
    void HandleExit();

    /// <summary>
    /// Handles filtering games by the specified show mode.
    /// </summary>
    /// <param name="showGamesMode">The mode to filter games by.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleShowGamesAsync(string showGamesMode);

    /// <summary>
    /// Handles changing the button size to the specified value.
    /// </summary>
    /// <param name="newSize">The new button size in pixels.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleButtonSizeAsync(int newSize);

    /// <summary>
    /// Handles changing the button aspect ratio.
    /// </summary>
    /// <param name="aspectRatio">The aspect ratio to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleButtonAspectRatioAsync(string aspectRatio);

    /// <summary>
    /// Handles changing the number of games displayed per page.
    /// </summary>
    /// <param name="newPage">The new number of games per page.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleGamesPerPageAsync(int newPage);

    /// <summary>
    /// Handles showing the global search window.
    /// </summary>
    void HandleShowGlobalSearch();

    /// <summary>
    /// Handles showing the global statistics window.
    /// </summary>
    void HandleShowGlobalStats();

    /// <summary>
    /// Handles showing the favorites window.
    /// </summary>
    void HandleShowFavorites();

    /// <summary>
    /// Handles showing the play history window.
    /// </summary>
    void HandleShowPlayHistory();

    /// <summary>
    /// Handles showing the RetroAchievements window.
    /// </summary>
    void HandleShowRetroAchievements();

    /// <summary>
    /// Handles showing favorite games for the selected system.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleShowSystemFavoritesAsync();

    /// <summary>
    /// Handles the feeling lucky action to show a random selection of games.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleFeelingLuckyAsync();

    /// <summary>
    /// Handles showing games that have RetroAchievements support.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleShowGamesWithRetroAchievementsAsync();

    /// <summary>
    /// Handles calculating RetroAchievements hashes for all game paths in the background.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleCalculateHashesForAllGamePathsAsync();

    /// <summary>
    /// Handles zooming in on the game display.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleZoomInAsync();

    /// <summary>
    /// Handles zooming out on the game display.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleZoomOutAsync();

    /// <summary>
    /// Handles toggling between grid and list view modes.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleToggleViewModeAsync();

    /// <summary>
    /// Handles changing the view mode based on the sender's context.
    /// </summary>
    /// <param name="sender">The sender that triggered the view mode change.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleChangeViewModeAsync(object sender);

    /// <summary>
    /// Handles changing the filename display mode.
    /// </summary>
    /// <param name="mode">The display mode to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleFilenameDisplayModeAsync(string mode);

    /// <summary>
    /// Handles toggling the display of machine names.
    /// </summary>
    /// <param name="isChecked">True to show machine names; false to hide.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleDisplayMachineNameAsync(bool isChecked);

    /// <summary>
    /// Handles changing the filename font size.
    /// </summary>
    /// <param name="size">The font size to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleFilenameFontSizeAsync(string size);

    /// <summary>
    /// Handles changing the machine name font size.
    /// </summary>
    /// <param name="size">The font size to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleMachineNameFontSizeAsync(string size);

    /// <summary>
    /// Handles opening the sound configuration window.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleSoundConfigurationAsync();

    /// <summary>
    /// Handles showing the RetroAchievements settings window.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleShowRetroAchievementsSettingsAsync();

    /// <summary>
    /// Handles toggling the RetroAchievement button visibility.
    /// </summary>
    /// <param name="isChecked">True to show; false to hide.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleToggleRetroAchievementButtonAsync(bool isChecked);

    /// <summary>
    /// Handles toggling the video link button visibility.
    /// </summary>
    /// <param name="isChecked">True to show; false to hide.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleToggleVideoLinkButtonAsync(bool isChecked);

    /// <summary>
    /// Handles toggling the info link button visibility.
    /// </summary>
    /// <param name="isChecked">True to show; false to hide.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleToggleInfoLinkButtonAsync(bool isChecked);

    /// <summary>
    /// Handles toggling the sort order.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleSortOrderToggleAsync();

    /// <summary>
    /// Handles a click on a top letter/number menu item for navigation.
    /// </summary>
    /// <param name="selectedLetter">The selected letter or number.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleTopLetterNumberMenuClickAsync(string selectedLetter);

    /// <summary>
    /// Handles restarting the application.
    /// </summary>
    void HandleRestart();

    /// <summary>
    /// Handles changing the application language based on the selected menu item.
    /// </summary>
    /// <param name="menuItem">The menu item representing the selected language.</param>
    void HandleChangeLanguage(MenuItem menuItem);

    /// <summary>
    /// Updates the check marks for thumbnail size menu items.
    /// </summary>
    /// <param name="selectedSize">The currently selected thumbnail size.</param>
    void UpdateThumbnailSizeCheckMarks(int selectedSize);

    /// <summary>
    /// Updates the check marks for games-per-page menu items.
    /// </summary>
    /// <param name="selectedSize">The currently selected number of games per page.</param>
    void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize);

    /// <summary>
    /// Updates the check marks for the show games filter.
    /// </summary>
    /// <param name="selectedValue">The currently selected show games mode.</param>
    void UpdateShowGamesCheckMarks(string selectedValue);

    /// <summary>
    /// Updates the check marks for button aspect ratio.
    /// </summary>
    /// <param name="selectedValue">The currently selected aspect ratio.</param>
    void UpdateButtonAspectRatioCheckMarks(string selectedValue);

    /// <summary>
    /// Updates the check marks for filename display mode.
    /// </summary>
    /// <param name="selectedValue">The currently selected display mode.</param>
    void UpdateFilenameDisplayModeCheckMarks(string selectedValue);

    /// <summary>
    /// Updates the check marks for filename font size.
    /// </summary>
    /// <param name="selectedValue">The currently selected font size.</param>
    void UpdateFilenameFontSizeCheckMarks(string selectedValue);

    /// <summary>
    /// Updates the check marks for machine name font size.
    /// </summary>
    /// <param name="selectedValue">The currently selected font size.</param>
    void UpdateMachineNameFontSizeCheckMarks(string selectedValue);

    /// <summary>
    /// Sets the view mode and updates corresponding check marks.
    /// </summary>
    /// <param name="viewMode">The view mode to set.</param>
    void SetViewMode(string viewMode);

    /// <summary>
    /// Changes the base theme using the selected menu item.
    /// </summary>
    /// <param name="menuItem">The menu item representing the selected base theme.</param>
    void ChangeBaseTheme(MenuItem menuItem);

    /// <summary>
    /// Changes the accent color using the selected menu item.
    /// </summary>
    /// <param name="menuItem">The menu item representing the selected accent color.</param>
    void ChangeAccentColor(MenuItem menuItem);

    /// <summary>
    /// Sets the checked state for the specified theme combination.
    /// </summary>
    /// <param name="baseTheme">The base theme name.</param>
    /// <param name="accentColor">The accent color name.</param>
    void SetCheckedTheme(string baseTheme, string accentColor);

    /// <summary>
    /// Changes the application language to the specified language code.
    /// </summary>
    /// <param name="languageCode">The language code to apply.</param>
    void ChangeLanguageAsync(string languageCode);

    /// <summary>
    /// Sets the check marks for the specified language.
    /// </summary>
    /// <param name="languageCode">The language code to mark as selected.</param>
    void SetLanguageCheckMarks(string languageCode);
}
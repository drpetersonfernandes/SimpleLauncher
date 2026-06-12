using System.Windows.Controls;
using SimpleLauncher.Services.LanguageMenu;
using SimpleLauncher.Services.MenuActionHandler;
using SimpleLauncher.Services.ThemeMenu;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.MenuOrchestrator;

/// <summary>
/// Orchestrates menu operations by coordinating action handlers, check mark updates, theme changes, and language switching.
/// </summary>
public class MenuOrchestrator : IMenuOrchestrator
{
    private readonly MenuActionHandlerService _menuActionHandler;
    private readonly IMenuCheckMarkService _menuCheckMark;
    private readonly ThemeMenuService _themeMenu;
    private readonly LanguageMenuService _languageMenu;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuOrchestrator"/> class with the specified service dependencies.
    /// </summary>
    /// <param name="menuActionHandler">The service that handles menu action operations.</param>
    /// <param name="menuCheckMark">The service that manages menu check mark states.</param>
    /// <param name="themeMenu">The service that manages theme selection.</param>
    /// <param name="languageMenu">The service that manages language selection.</param>
    public MenuOrchestrator(
        MenuActionHandlerService menuActionHandler,
        IMenuCheckMarkService menuCheckMark,
        ThemeMenuService themeMenu,
        LanguageMenuService languageMenu)
    {
        _menuActionHandler = menuActionHandler;
        _menuCheckMark = menuCheckMark;
        _themeMenu = themeMenu;
        _languageMenu = languageMenu;
    }

    /// <summary>
    /// Initializes all managed services with their respective host implementations.
    /// </summary>
    /// <param name="actionHost">The host providing menu action controls.</param>
    /// <param name="checkMarkHost">The host providing menu check mark controls.</param>
    /// <param name="themeHost">The host providing theme selection controls.</param>
    /// <param name="languageHost">The host providing language selection controls.</param>
    public void Initialize(IMenuActionHost actionHost, IMenuCheckMarkHost checkMarkHost, IThemeMenuHost themeHost, ILanguageMenuHost languageHost)
    {
        _menuActionHandler.Initialize(actionHost);
        _menuCheckMark.Initialize(checkMarkHost);
        _themeMenu.Initialize(themeHost);
        _languageMenu.Initialize(languageHost);
    }

    /// <summary>
    /// Opens the emulator configuration window for the specified emulator.
    /// </summary>
    /// <param name="emulatorName">The name of the emulator to configure.</param>
    public void ShowEmulatorConfigWindow(string emulatorName)
    {
        _menuActionHandler.ShowEmulatorConfigWindow(emulatorName);
    }

    /// <summary>
    /// Handles switching to Easy Mode for simplified system configuration.
    /// </summary>
    public void HandleEasyMode()
    {
        _menuActionHandler.HandleEasyMode();
    }

    /// <summary>
    /// Handles switching to Expert Mode for advanced system configuration.
    /// </summary>
    public void HandleExpertMode()
    {
        _menuActionHandler.HandleExpertMode();
    }

    /// <summary>
    /// Handles downloading the image pack for game cover art.
    /// </summary>
    public void HandleDownloadImagePack()
    {
        _menuActionHandler.HandleDownloadImagePack();
    }

    /// <summary>
    /// Handles scanning for Windows games installed on the system.
    /// </summary>
    public Task HandleScanForWindowsGames()
    {
        return _menuActionHandler.HandleScanForWindowsGames();
    }

    /// <summary>
    /// Handles editing external links associated with games.
    /// </summary>
    public Task HandleEditLinks()
    {
        return _menuActionHandler.HandleEditLinks();
    }

    /// <summary>
    /// Handles toggling gamepad support on or off.
    /// </summary>
    /// <param name="isChecked">Whether gamepad support should be enabled.</param>
    public void HandleToggleGamepad(bool isChecked)
    {
        _ = _menuActionHandler.HandleToggleGamepad(isChecked);
    }

    /// <summary>
    /// Handles setting the gamepad dead zone value.
    /// </summary>
    public void HandleSetGamepadDeadZone()
    {
        _menuActionHandler.HandleSetGamepadDeadZone();
    }

    /// <summary>
    /// Handles toggling fuzzy matching for game search on or off.
    /// </summary>
    /// <param name="isChecked">Whether fuzzy matching is enabled.</param>
    public Task HandleToggleFuzzyMatching(bool isChecked)
    {
        return _menuActionHandler.HandleToggleFuzzyMatching(isChecked);
    }

    /// <summary>
    /// Handles setting the fuzzy matching threshold value.
    /// </summary>
    public Task HandleSetFuzzyMatchingThreshold()
    {
        return _menuActionHandler.HandleSetFuzzyMatchingThreshold();
    }

    /// <summary>
    /// Opens the support page for the application.
    /// </summary>
    public void HandleSupport()
    {
        _menuActionHandler.HandleSupport();
    }

    /// <summary>
    /// Opens the donation page for the application.
    /// </summary>
    public void HandleDonate()
    {
        _ = _menuActionHandler.HandleDonate();
    }

    /// <summary>
    /// Opens the About dialog.
    /// </summary>
    public void HandleAbout()
    {
        _menuActionHandler.HandleAbout();
    }

    /// <summary>
    /// Handles application exit.
    /// </summary>
    public void HandleExit()
    {
        _menuActionHandler.HandleExit();
    }

    /// <summary>
    /// Handles changing the show-games filter mode.
    /// </summary>
    /// <param name="showGamesMode">The filter mode to apply (e.g., "ShowAll", "ShowWithCover").</param>
    public Task HandleShowGames(string showGamesMode)
    {
        return _menuActionHandler.HandleShowGames(showGamesMode);
    }

    /// <summary>
    /// Handles changing the thumbnail button size.
    /// </summary>
    /// <param name="newSize">The new thumbnail size in pixels.</param>
    public Task HandleButtonSize(int newSize)
    {
        return _menuActionHandler.HandleButtonSize(newSize);
    }

    /// <summary>
    /// Handles changing the button aspect ratio.
    /// </summary>
    /// <param name="aspectRatio">The aspect ratio identifier to apply.</param>
    public Task HandleButtonAspectRatio(string aspectRatio)
    {
        return _menuActionHandler.HandleButtonAspectRatio(aspectRatio);
    }

    /// <summary>
    /// Handles changing the number of games displayed per page.
    /// </summary>
    /// <param name="newPage">The new number of games per page.</param>
    public Task HandleGamesPerPage(int newPage)
    {
        return _menuActionHandler.HandleGamesPerPage(newPage);
    }

    /// <summary>
    /// Opens the global search window.
    /// </summary>
    public void HandleShowGlobalSearch()
    {
        _menuActionHandler.HandleShowGlobalSearch();
    }

    /// <summary>
    /// Opens the global statistics window.
    /// </summary>
    public void HandleShowGlobalStats()
    {
        _menuActionHandler.HandleShowGlobalStats();
    }

    /// <summary>
    /// Opens the favorites window.
    /// </summary>
    public void HandleShowFavorites()
    {
        _menuActionHandler.HandleShowFavorites();
    }

    /// <summary>
    /// Opens the play history window.
    /// </summary>
    public void HandleShowPlayHistory()
    {
        _menuActionHandler.HandleShowPlayHistory();
    }

    /// <summary>
    /// Opens the RetroAchievements window.
    /// </summary>
    public void HandleShowRetroAchievements()
    {
        _menuActionHandler.HandleShowRetroAchievements();
    }

    /// <summary>
    /// Opens the system-specific favorites window.
    /// </summary>
    public Task HandleShowSystemFavorites()
    {
        return _menuActionHandler.HandleShowSystemFavorites();
    }

    /// <summary>
    /// Selects and launches a random game from the current system.
    /// </summary>
    public Task HandleFeelingLucky()
    {
        return _menuActionHandler.HandleFeelingLucky();
    }

    /// <summary>
    /// Shows games that have RetroAchievements support.
    /// </summary>
    public Task HandleShowGamesWithRetroAchievements()
    {
        return _menuActionHandler.HandleShowGamesWithRetroAchievements();
    }

    /// <summary>
    /// Handles zooming in on the game grid.
    /// </summary>
    public Task HandleZoomIn()
    {
        return _menuActionHandler.HandleZoomIn();
    }

    /// <summary>
    /// Handles zooming out on the game grid.
    /// </summary>
    public Task HandleZoomOut()
    {
        return _menuActionHandler.HandleZoomOut();
    }

    /// <summary>
    /// Handles toggling between list view and grid view.
    /// </summary>
    public Task HandleToggleViewMode()
    {
        return _menuActionHandler.HandleToggleViewMode();
    }

    /// <summary>
    /// Handles changing the view mode based on the sender menu item.
    /// </summary>
    /// <param name="sender">The menu item that triggered the view mode change.</param>
    public void HandleChangeViewMode(object sender)
    {
        _ = _menuActionHandler.HandleChangeViewMode(sender);
    }

    /// <summary>
    /// Handles changing the filename display mode.
    /// </summary>
    /// <param name="mode">The display mode to apply (e.g., "Original", "CleanUp").</param>
    public Task HandleFilenameDisplayMode(string mode)
    {
        return _menuActionHandler.HandleFilenameDisplayMode(mode);
    }

    /// <summary>
    /// Handles toggling machine name display on or off.
    /// </summary>
    /// <param name="isChecked">Whether to display machine names.</param>
    public Task HandleDisplayMachineName(bool isChecked)
    {
        return _menuActionHandler.HandleDisplayMachineName(isChecked);
    }

    /// <summary>
    /// Handles changing the filename font size.
    /// </summary>
    /// <param name="size">The font size to apply (e.g., "Small", "Normal", "Big").</param>
    public Task HandleFilenameFontSize(string size)
    {
        return _menuActionHandler.HandleFilenameFontSize(size);
    }

    /// <summary>
    /// Handles changing the machine name font size.
    /// </summary>
    /// <param name="size">The font size to apply (e.g., "Small", "Normal", "Big").</param>
    public Task HandleMachineNameFontSize(string size)
    {
        return _menuActionHandler.HandleMachineNameFontSize(size);
    }

    /// <summary>
    /// Opens the sound configuration window.
    /// </summary>
    public void HandleSoundConfiguration()
    {
        _ = _menuActionHandler.HandleSoundConfiguration();
    }

    /// <summary>
    /// Opens the RetroAchievements settings window.
    /// </summary>
    public Task HandleShowRetroAchievementsSettings()
    {
        return _menuActionHandler.HandleShowRetroAchievementsSettings();
    }

    /// <summary>
    /// Handles toggling the RetroAchievement button visibility.
    /// </summary>
    /// <param name="isChecked">Whether the button should be visible.</param>
    public Task HandleToggleRetroAchievementButton(bool isChecked)
    {
        return _menuActionHandler.HandleToggleRetroAchievementButton(isChecked);
    }

    /// <summary>
    /// Handles toggling the video link button visibility.
    /// </summary>
    /// <param name="isChecked">Whether the button should be visible.</param>
    public Task HandleToggleVideoLinkButton(bool isChecked)
    {
        return _menuActionHandler.HandleToggleVideoLinkButton(isChecked);
    }

    /// <summary>
    /// Handles toggling the info link button visibility.
    /// </summary>
    /// <param name="isChecked">Whether the button should be visible.</param>
    public Task HandleToggleInfoLinkButton(bool isChecked)
    {
        return _menuActionHandler.HandleToggleInfoLinkButton(isChecked);
    }

    /// <summary>
    /// Handles toggling the sort order between ascending and descending.
    /// </summary>
    public Task HandleSortOrderToggle()
    {
        return _menuActionHandler.HandleSortOrderToggle();
    }

    /// <summary>
    /// Handles clicking a letter or number in the top navigation menu to jump to that section.
    /// </summary>
    /// <param name="selectedLetter">The letter or number clicked.</param>
    public Task HandleTopLetterNumberMenuClick(string selectedLetter)
    {
        return _menuActionHandler.HandleTopLetterNumberMenuClick(selectedLetter);
    }

    /// <summary>
    /// Handles restarting the application.
    /// </summary>
    public void HandleRestart()
    {
        _menuActionHandler.HandleRestart();
    }

    /// <summary>
    /// Handles changing the application language from a menu item selection.
    /// </summary>
    /// <param name="menuItem">The language menu item that was selected.</param>
    public void HandleChangeLanguage(MenuItem menuItem)
    {
        var languageCode = LanguageMenuService.GetLanguageCodeFromMenuItem(menuItem);
        if (languageCode != null)
        {
            _menuActionHandler.HandleChangeLanguage(languageCode);
        }
    }

    /// <summary>
    /// Updates the thumbnail size check marks to reflect the current selection.
    /// </summary>
    /// <param name="selectedSize">The currently selected thumbnail size.</param>
    public void UpdateThumbnailSizeCheckMarks(int selectedSize)
    {
        _menuCheckMark.UpdateThumbnailSizeCheckMarks(selectedSize);
    }

    /// <summary>
    /// Updates the games-per-page check marks to reflect the current selection.
    /// </summary>
    /// <param name="selectedSize">The currently selected number of games per page.</param>
    public void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize)
    {
        _menuCheckMark.UpdateNumberOfGamesPerPageCheckMarks(selectedSize);
    }

    /// <summary>
    /// Updates the show-games filter check marks to reflect the current selection.
    /// </summary>
    /// <param name="selectedValue">The currently selected filter value.</param>
    public void UpdateShowGamesCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateShowGamesCheckMarks(selectedValue);
    }

    /// <summary>
    /// Updates the button aspect ratio check marks to reflect the current selection.
    /// </summary>
    /// <param name="selectedValue">The currently selected aspect ratio.</param>
    public void UpdateButtonAspectRatioCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateButtonAspectRatioCheckMarks(selectedValue);
    }

    /// <summary>
    /// Updates the filename display mode check marks to reflect the current selection.
    /// </summary>
    /// <param name="selectedValue">The currently selected display mode.</param>
    public void UpdateFilenameDisplayModeCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateFilenameDisplayModeCheckMarks(selectedValue);
    }

    /// <summary>
    /// Updates the filename font size check marks to reflect the current selection.
    /// </summary>
    /// <param name="selectedValue">The currently selected font size.</param>
    public void UpdateFilenameFontSizeCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateFilenameFontSizeCheckMarks(selectedValue);
    }

    /// <summary>
    /// Updates the machine name font size check marks to reflect the current selection.
    /// </summary>
    /// <param name="selectedValue">The currently selected font size.</param>
    public void UpdateMachineNameFontSizeCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateMachineNameFontSizeCheckMarks(selectedValue);
    }

    /// <summary>
    /// Sets the view mode check marks to indicate the active view.
    /// </summary>
    /// <param name="viewMode">The view mode to set ("ListView" or "GridView").</param>
    public void SetViewMode(string viewMode)
    {
        _menuCheckMark.SetViewMode(viewMode);
    }

    /// <summary>
    /// Changes the base theme based on the selected menu item.
    /// </summary>
    /// <param name="menuItem">The theme menu item that was selected.</param>
    public void ChangeBaseTheme(MenuItem menuItem)
    {
        _themeMenu.ChangeBaseTheme(menuItem);
    }

    /// <summary>
    /// Changes the accent color based on the selected menu item.
    /// </summary>
    /// <param name="menuItem">The accent color menu item that was selected.</param>
    public void ChangeAccentColor(MenuItem menuItem)
    {
        _themeMenu.ChangeAccentColor(menuItem);
    }

    /// <summary>
    /// Sets the checked theme menu items for the given base theme and accent color.
    /// </summary>
    /// <param name="baseTheme">The base theme name.</param>
    /// <param name="accentColor">The accent color name.</param>
    public void SetCheckedTheme(string baseTheme, string accentColor)
    {
        _themeMenu.SetCheckedTheme(baseTheme, accentColor);
    }

    /// <summary>
    /// Changes the application language to the specified language code.
    /// </summary>
    /// <param name="languageCode">The language code to apply.</param>
    public void ChangeLanguage(string languageCode)
    {
        _languageMenu.ChangeLanguage(languageCode);
    }

    /// <summary>
    /// Sets the language menu check marks to reflect the currently active language.
    /// </summary>
    /// <param name="languageCode">The currently active language code.</param>
    public void SetLanguageCheckMarks(string languageCode)
    {
        _languageMenu.SetLanguageCheckMarks(languageCode);
    }
}

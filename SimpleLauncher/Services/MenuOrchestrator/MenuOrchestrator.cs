using System.Windows.Controls;
using SimpleLauncher.Services.LanguageMenu;
using SimpleLauncher.Services.MenuActionHandler;
using SimpleLauncher.Services.MenuCheckMark;
using SimpleLauncher.Services.ThemeMenu;

namespace SimpleLauncher.Services.MenuOrchestrator;

public class MenuOrchestrator : IMenuOrchestrator
{
    private readonly MenuActionHandlerService _menuActionHandler;
    private readonly IMenuCheckMarkService _menuCheckMark;
    private readonly ThemeMenuService _themeMenu;
    private readonly LanguageMenuService _languageMenu;

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

    public void Initialize(IMenuActionHost actionHost, IMenuCheckMarkHost checkMarkHost, IThemeMenuHost themeHost, ILanguageMenuHost languageHost)
    {
        _menuActionHandler.Initialize(actionHost);
        _menuCheckMark.Initialize(checkMarkHost);
        _themeMenu.Initialize(themeHost);
        _languageMenu.Initialize(languageHost);
    }

    // Menu actions
    public void ShowEmulatorConfigWindow(string emulatorName)
    {
        _menuActionHandler.ShowEmulatorConfigWindow(emulatorName);
    }

    public void HandleEasyMode()
    {
        _menuActionHandler.HandleEasyMode();
    }

    public void HandleExpertMode()
    {
        _menuActionHandler.HandleExpertMode();
    }

    public void HandleDownloadImagePack()
    {
        _menuActionHandler.HandleDownloadImagePack();
    }

    public Task HandleScanForWindowsGames()
    {
        return _menuActionHandler.HandleScanForWindowsGames();
    }

    public Task HandleEditLinks()
    {
        return _menuActionHandler.HandleEditLinks();
    }

    public void HandleToggleGamepad(bool isChecked)
    {
        _menuActionHandler.HandleToggleGamepad(isChecked);
    }

    public void HandleSetGamepadDeadZone()
    {
        _menuActionHandler.HandleSetGamepadDeadZone();
    }

    public Task HandleToggleFuzzyMatching(bool isChecked)
    {
        return _menuActionHandler.HandleToggleFuzzyMatching(isChecked);
    }

    public Task HandleSetFuzzyMatchingThreshold()
    {
        return _menuActionHandler.HandleSetFuzzyMatchingThreshold();
    }

    public void HandleSupport()
    {
        _menuActionHandler.HandleSupport();
    }

    public void HandleDonate()
    {
        _menuActionHandler.HandleDonate();
    }

    public void HandleAbout()
    {
        _menuActionHandler.HandleAbout();
    }

    public void HandleExit()
    {
        _menuActionHandler.HandleExit();
    }

    public Task HandleShowGames(string showGamesMode)
    {
        return _menuActionHandler.HandleShowGames(showGamesMode);
    }

    public Task HandleButtonSize(int newSize)
    {
        return _menuActionHandler.HandleButtonSize(newSize);
    }

    public Task HandleButtonAspectRatio(string aspectRatio)
    {
        return _menuActionHandler.HandleButtonAspectRatio(aspectRatio);
    }

    public Task HandleGamesPerPage(int newPage)
    {
        return _menuActionHandler.HandleGamesPerPage(newPage);
    }

    public void HandleShowGlobalSearch()
    {
        _menuActionHandler.HandleShowGlobalSearch();
    }

    public void HandleShowGlobalStats()
    {
        _menuActionHandler.HandleShowGlobalStats();
    }

    public void HandleShowFavorites()
    {
        _menuActionHandler.HandleShowFavorites();
    }

    public void HandleShowPlayHistory()
    {
        _menuActionHandler.HandleShowPlayHistory();
    }

    public void HandleShowRetroAchievements()
    {
        _menuActionHandler.HandleShowRetroAchievements();
    }

    public Task HandleShowSystemFavorites()
    {
        return _menuActionHandler.HandleShowSystemFavorites();
    }

    public Task HandleFeelingLucky()
    {
        return _menuActionHandler.HandleFeelingLucky();
    }

    public Task HandleShowGamesWithRetroAchievements()
    {
        return _menuActionHandler.HandleShowGamesWithRetroAchievements();
    }

    public Task HandleZoomIn()
    {
        return _menuActionHandler.HandleZoomIn();
    }

    public Task HandleZoomOut()
    {
        return _menuActionHandler.HandleZoomOut();
    }

    public Task HandleToggleViewMode()
    {
        return _menuActionHandler.HandleToggleViewMode();
    }

    public void HandleChangeViewMode(object sender)
    {
        _menuActionHandler.HandleChangeViewMode(sender);
    }

    public Task HandleFilenameDisplayMode(string mode)
    {
        return _menuActionHandler.HandleFilenameDisplayMode(mode);
    }

    public Task HandleDisplayMachineName(bool isChecked)
    {
        return _menuActionHandler.HandleDisplayMachineName(isChecked);
    }

    public Task HandleFilenameFontSize(string size)
    {
        return _menuActionHandler.HandleFilenameFontSize(size);
    }

    public Task HandleMachineNameFontSize(string size)
    {
        return _menuActionHandler.HandleMachineNameFontSize(size);
    }

    public void HandleSoundConfiguration()
    {
        _menuActionHandler.HandleSoundConfiguration();
    }

    public void HandleShowRetroAchievementsSettings()
    {
        _menuActionHandler.HandleShowRetroAchievementsSettings();
    }

    public Task HandleToggleRetroAchievementButton(bool isChecked)
    {
        return _menuActionHandler.HandleToggleRetroAchievementButton(isChecked);
    }

    public Task HandleToggleVideoLinkButton(bool isChecked)
    {
        return _menuActionHandler.HandleToggleVideoLinkButton(isChecked);
    }

    public Task HandleToggleInfoLinkButton(bool isChecked)
    {
        return _menuActionHandler.HandleToggleInfoLinkButton(isChecked);
    }

    public Task HandleSortOrderToggle()
    {
        return _menuActionHandler.HandleSortOrderToggle();
    }

    public Task HandleTopLetterNumberMenuClick(string selectedLetter)
    {
        return _menuActionHandler.HandleTopLetterNumberMenuClick(selectedLetter);
    }

    public void HandleRestart()
    {
        _menuActionHandler.HandleRestart();
    }

    public void HandleChangeLanguage(MenuItem menuItem)
    {
        var languageCode = LanguageMenuService.GetLanguageCodeFromMenuItem(menuItem);
        if (languageCode != null)
        {
            _menuActionHandler.HandleChangeLanguage(languageCode);
        }
    }

    // Check mark management
    public void UpdateThumbnailSizeCheckMarks(int selectedSize)
    {
        _menuCheckMark.UpdateThumbnailSizeCheckMarks(selectedSize);
    }

    public void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize)
    {
        _menuCheckMark.UpdateNumberOfGamesPerPageCheckMarks(selectedSize);
    }

    public void UpdateShowGamesCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateShowGamesCheckMarks(selectedValue);
    }

    public void UpdateButtonAspectRatioCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateButtonAspectRatioCheckMarks(selectedValue);
    }

    public void UpdateFilenameDisplayModeCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateFilenameDisplayModeCheckMarks(selectedValue);
    }

    public void UpdateFilenameFontSizeCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateFilenameFontSizeCheckMarks(selectedValue);
    }

    public void UpdateMachineNameFontSizeCheckMarks(string selectedValue)
    {
        _menuCheckMark.UpdateMachineNameFontSizeCheckMarks(selectedValue);
    }

    public void SetViewMode(string viewMode)
    {
        _menuCheckMark.SetViewMode(viewMode);
    }

    // Theme
    public void ChangeBaseTheme(MenuItem menuItem)
    {
        _themeMenu.ChangeBaseTheme(menuItem);
    }

    public void ChangeAccentColor(MenuItem menuItem)
    {
        _themeMenu.ChangeAccentColor(menuItem);
    }

    public void SetCheckedTheme(string baseTheme, string accentColor)
    {
        _themeMenu.SetCheckedTheme(baseTheme, accentColor);
    }

    // Language
    public void ChangeLanguage(string languageCode)
    {
        _languageMenu.ChangeLanguage(languageCode);
    }

    public void SetLanguageCheckMarks(string languageCode)
    {
        _languageMenu.SetLanguageCheckMarks(languageCode);
    }
}

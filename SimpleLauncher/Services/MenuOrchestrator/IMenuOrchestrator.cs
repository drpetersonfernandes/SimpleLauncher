using System.Windows.Controls;
using SimpleLauncher.Services.LanguageMenu;
using SimpleLauncher.Services.MenuActionHandler;
using SimpleLauncher.Services.MenuCheckMark;
using SimpleLauncher.Services.ThemeMenu;

namespace SimpleLauncher.Services.MenuOrchestrator;

public interface IMenuOrchestrator
{
    void Initialize(IMenuActionHost actionHost, IMenuCheckMarkHost checkMarkHost, IThemeMenuHost themeHost, ILanguageMenuHost languageHost);

    // Menu actions
    void ShowEmulatorConfigWindow(string emulatorName);
    void HandleEasyMode();
    void HandleExpertMode();
    void HandleDownloadImagePack();
    Task HandleScanForWindowsGames();
    Task HandleEditLinks();
    void HandleToggleGamepad(bool isChecked);
    void HandleSetGamepadDeadZone();
    Task HandleToggleFuzzyMatching(bool isChecked);
    Task HandleSetFuzzyMatchingThreshold();
    void HandleSupport();
    void HandleDonate();
    void HandleAbout();
    void HandleExit();
    Task HandleShowGames(string showGamesMode);
    Task HandleButtonSize(int newSize);
    Task HandleButtonAspectRatio(string aspectRatio);
    Task HandleGamesPerPage(int newPage);
    void HandleShowGlobalSearch();
    void HandleShowGlobalStats();
    void HandleShowFavorites();
    void HandleShowPlayHistory();
    void HandleShowRetroAchievements();
    Task HandleShowSystemFavorites();
    Task HandleFeelingLucky();
    Task HandleShowGamesWithRetroAchievements();
    Task HandleZoomIn();
    Task HandleZoomOut();
    Task HandleToggleViewMode();
    void HandleChangeViewMode(object sender);
    Task HandleFilenameDisplayMode(string mode);
    Task HandleDisplayMachineName(bool isChecked);
    Task HandleFilenameFontSize(string size);
    Task HandleMachineNameFontSize(string size);
    void HandleSoundConfiguration();
    Task HandleShowRetroAchievementsSettings();
    Task HandleToggleRetroAchievementButton(bool isChecked);
    Task HandleToggleVideoLinkButton(bool isChecked);
    Task HandleToggleInfoLinkButton(bool isChecked);
    Task HandleSortOrderToggle();
    Task HandleTopLetterNumberMenuClick(string selectedLetter);
    void HandleRestart();
    void HandleChangeLanguage(MenuItem menuItem);

    // Check mark management
    void UpdateThumbnailSizeCheckMarks(int selectedSize);
    void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize);
    void UpdateShowGamesCheckMarks(string selectedValue);
    void UpdateButtonAspectRatioCheckMarks(string selectedValue);
    void UpdateFilenameDisplayModeCheckMarks(string selectedValue);
    void UpdateFilenameFontSizeCheckMarks(string selectedValue);
    void UpdateMachineNameFontSizeCheckMarks(string selectedValue);
    void SetViewMode(string viewMode);

    // Theme
    void ChangeBaseTheme(MenuItem menuItem);
    void ChangeAccentColor(MenuItem menuItem);
    void SetCheckedTheme(string baseTheme, string accentColor);

    // Language
    void ChangeLanguage(string languageCode);
    void SetLanguageCheckMarks(string languageCode);
}

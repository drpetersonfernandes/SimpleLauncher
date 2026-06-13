using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

public interface IMenuOrchestrator
{
    void Initialize(IMenuActionHost actionHost, IMenuCheckMarkHost checkMarkHost, IThemeMenuHost themeHost, ILanguageMenuHost languageHost);

    // Menu actions
    void ShowEmulatorConfigWindow(string emulatorName);
    void HandleEasyMode();
    void HandleExpertMode();
    void HandleDownloadImagePack();
    Task HandleScanForWindowsGamesAsync();
    Task HandleEditLinksAsync();
    Task HandleToggleGamepadAsync(bool isChecked);
    void HandleSetGamepadDeadZone();
    Task HandleToggleFuzzyMatchingAsync(bool isChecked);
    Task HandleSetFuzzyMatchingThresholdAsync();
    Task HandleToggleAnnotationStrippingAsync(bool isChecked);
    void HandleSupport();
    Task HandleDonateAsync();
    void HandleAbout();
    void HandleExit();
    Task HandleShowGamesAsync(string showGamesMode);
    Task HandleButtonSizeAsync(int newSize);
    Task HandleButtonAspectRatioAsync(string aspectRatio);
    Task HandleGamesPerPageAsync(int newPage);
    void HandleShowGlobalSearch();
    void HandleShowGlobalStats();
    void HandleShowFavorites();
    void HandleShowPlayHistory();
    void HandleShowRetroAchievements();
    Task HandleShowSystemFavoritesAsync();
    Task HandleFeelingLuckyAsync();
    Task HandleShowGamesWithRetroAchievementsAsync();
    Task HandleZoomInAsync();
    Task HandleZoomOutAsync();
    Task HandleToggleViewModeAsync();
    Task HandleChangeViewModeAsync(object sender);
    Task HandleFilenameDisplayModeAsync(string mode);
    Task HandleDisplayMachineNameAsync(bool isChecked);
    Task HandleFilenameFontSizeAsync(string size);
    Task HandleMachineNameFontSizeAsync(string size);
    Task HandleSoundConfigurationAsync();
    Task HandleShowRetroAchievementsSettingsAsync();
    Task HandleToggleRetroAchievementButtonAsync(bool isChecked);
    Task HandleToggleVideoLinkButtonAsync(bool isChecked);
    Task HandleToggleInfoLinkButtonAsync(bool isChecked);
    Task HandleSortOrderToggleAsync();
    Task HandleTopLetterNumberMenuClickAsync(string selectedLetter);
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
    void ChangeLanguageAsync(string languageCode);
    void SetLanguageCheckMarks(string languageCode);
}

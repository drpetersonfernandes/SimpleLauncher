using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher;

using Interfaces;

/// <summary>
/// Partial MainWindow containing menu item click handlers and system management operations.
/// </summary>
public partial class MainWindow
{
    internal void SetViewMode(string viewMode)
    {
        _menuOrchestrator.SetViewMode(viewMode);
    }

    private void EasyMode_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleEasyMode();
    }

    private void ExpertMode_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleExpertMode();
    }

    private void DownloadImagePack_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleDownloadImagePack();
    }

    private async void ScanForMicrosoftWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleScanForWindowsGames();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ScanForMicrosoftWindowsGames_Click");
        }
    }

    internal async Task ResetUiAsync()
    {
        try
        {
            await UiResetService.ResetUiAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ResetUiAsync");
        }
    }

    /// <summary>
    /// Reloads the system manager and refreshes the system list.
    /// </summary>
    public void LoadOrReloadSystemManager()
    {
        _gameBrowser.LoadOrReloadSystemManager();
    }

    private async void EditLinksClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleEditLinks();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method EditLinksClickAsync.");
        }
    }

    private void ToggleGamepad_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _menuOrchestrator.HandleToggleGamepad(menuItem.IsChecked);
    }

    private void SetGamepadDeadZone_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleSetGamepadDeadZone();
    }

    private async void ToggleFuzzyMatchingClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            await _menuOrchestrator.HandleToggleFuzzyMatching(menuItem.IsChecked);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ToggleFuzzyMatchingClickAsync.");
        }
    }

    private async void SetFuzzyMatchingThresholdClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleSetFuzzyMatchingThreshold();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method SetFuzzyMatchingThresholdClickAsync.");
        }
    }

    private void Support_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleSupport();
    }

    private void Donate_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleDonate();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleAbout();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleExit();
    }

    private async void ShowAllGamesClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleShowGames("ShowAll");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ShowAllGamesClickAsync.");
        }
    }

    private async void ShowGamesWithCoverClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleShowGames("ShowWithCover");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ShowGamesWithCoverClickAsync.");
        }
    }

    private async void ShowGamesWithoutCoverClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleShowGames("ShowWithoutCover");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ShowGamesWithoutCoverClickAsync.");
        }
    }

    private async void ButtonSizeClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var sizeText = clickedItem.Name.Replace("Size", "");
            if (!int.TryParse(new string(sizeText.Where(char.IsDigit).ToArray()), out var newSize)) return;

            await _menuOrchestrator.HandleButtonSize(newSize);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ButtonSizeClickAsync.");
        }
    }

    private async void ButtonAspectRatioClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            await _menuOrchestrator.HandleButtonAspectRatio(clickedItem.Name);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ButtonAspectRatioClickAsync.");
        }
    }

    private async void GamesPerPageClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var pageText = clickedItem.Name.Replace("Page", "");
            if (!int.TryParse(new string(pageText.Where(char.IsDigit).ToArray()), out var newPage)) return;

            await _menuOrchestrator.HandleGamesPerPage(newPage);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method GamesPerPageClickAsync.");
        }
    }

    private void ShowGlobalSearchWindow_Click()
    {
        _menuOrchestrator.HandleShowGlobalSearch();
    }

    private void ShowGlobalStatsWindow_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleShowGlobalStats();
    }

    private void ShowFavoritesWindow_Click()
    {
        _menuOrchestrator.HandleShowFavorites();
    }

    private void ShowPlayHistoryWindow_Click()
    {
        _menuOrchestrator.HandleShowPlayHistory();
    }

    /// <summary>
    /// Opens the RetroAchievements window when the menu item is clicked.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The routed event arguments.</param>
    public void ShowRetroAchievementsWindowClick(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleShowRetroAchievements();
    }

    private void UpdateShowGamesCheckMarks(string selectedValue)
    {
        _menuOrchestrator.UpdateShowGamesCheckMarks(selectedValue);
    }

    private void UpdateButtonAspectRatioCheckMarks(string selectedValue)
    {
        _menuOrchestrator.UpdateButtonAspectRatioCheckMarks(selectedValue);
    }

    private async void NavToggleButtonAspectRatioClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames)
            {
                return;
            }

            CancelAndRecreateToken();

            _audioInput.PlayNotificationSound();

            // Define the array of aspect ratios in the desired order
            string[] aspectRatios = ["Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2"];

            // Get the current index of the aspect ratio
            var currentIndex = Array.IndexOf(aspectRatios, _settings.ButtonAspectRatio);

            // Calculate the next index, wrapping around to 0 if at the end
            var nextIndex = (currentIndex + 1) % aspectRatios.Length;

            // Get the new aspect ratio
            var newAspectRatio = aspectRatios[nextIndex];

            // Update the settings
            _settings.ButtonAspectRatio = newAspectRatio;
            await _settings.SaveAsync();

            UpdateButtonAspectRatioCheckMarks(newAspectRatio);
            // Notify user
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("TogglingButtonAspectRatio") ?? "Toggling button aspect ratio...");

            var (sl, sq) = GetLoadGameFilesParams();
            SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
            await Task.Yield(); // Allow UI to render the loading overlay
            await _gameBrowser.LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method NavToggleButtonAspectRatioClickAsync.";
            _logErrors.LogAndForget(ex, errorMessage);

            // Notify user
            await _messageBox.ErrorMessageBox();
        }
    }

    private async void FilenameDisplayMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var mode = clickedItem.Name switch
            {
                "FilenameDisplayOriginal" => "Original",
                "FilenameDisplayCleanUp" => "CleanUp",
                "FilenameDisplayNoFilename" => "NoFilename",
                _ => "Original"
            };

            await _menuOrchestrator.HandleFilenameDisplayMode(mode);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method FilenameDisplayMode_Click.");
        }
    }

    private async void DisplayMachineName_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            await _menuOrchestrator.HandleDisplayMachineName(menuItem.IsChecked);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method DisplayMachineName_Click.");
        }
    }

    private async void FilenameFontSize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var size = clickedItem.Name switch
            {
                "FilenameFontSizeSmall" => "Small",
                "FilenameFontSizeNormal" => "Normal",
                "FilenameFontSizeBig" => "Big",
                _ => "Normal"
            };

            await _menuOrchestrator.HandleFilenameFontSize(size);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method FilenameFontSize_Click.");
        }
    }

    private async void MachineNameFontSize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var size = clickedItem.Name switch
            {
                "MachineNameFontSizeSmall" => "Small",
                "MachineNameFontSizeNormal" => "Normal",
                "MachineNameFontSizeBig" => "Big",
                _ => "Normal"
            };

            await _menuOrchestrator.HandleMachineNameFontSize(size);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method MachineNameFontSize_Click.");
        }
    }

    private void ChangeViewMode_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleChangeViewMode(sender);
    }

    private void ApplyShowGamesSetting()
    {
        UpdateShowGamesCheckMarks(_settings.ShowGames);
        UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ApplyingGameVisibilitySettings") ?? "Applying game visibility settings...");
    }

    private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _menuOrchestrator.HandleChangeLanguage(menuItem);
    }

    private void NavRestartButton_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleRestart();
    }

    private void NavGlobalSearchButton_Click(object sender, RoutedEventArgs e)
    {
        ShowGlobalSearchWindow_Click();
    }

    private void NavFavoritesButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFavoritesWindow_Click();
    }

    private void NavHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPlayHistoryWindow_Click();
    }

    private void NavRetroAchievementsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowRetroAchievementsWindowClick(sender, e);
    }

    private void NavExpertModeButton_Click(object sender, RoutedEventArgs e)
    {
        ExpertMode_Click(sender, e);
    }

    private async void NavSelectedSystemFavoriteButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleShowSystemFavorites();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavSelectedSystemFavoriteButtonClickAsync.");
        }
    }

    private async void NavRandomLuckGameButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleFeelingLucky();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavRandomLuckGameButtonClickAsync.");
        }
    }

    private async void NavShowGamesWithRetroAchievementsButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleShowGamesWithRetroAchievements();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavShowGamesWithRetroAchievementsButtonClickAsync.");
        }
    }

    private async void NavZoomInButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleZoomIn();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavZoomInButtonClickAsync.");
        }
    }

    private async void NavZoomOutButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleZoomOut();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavZoomOutButtonClickAsync.");
        }
    }

    private async void NavToggleViewModeClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleToggleViewMode();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavToggleViewModeClickAsync.");
        }
    }

    private void SoundConfiguration_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.HandleSoundConfiguration();
    }

    private async void ShowRetroAchievementsSettingsWindow_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _menuOrchestrator.HandleShowRetroAchievementsSettings();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ShowRetroAchievementsSettingsWindow_Click.");
        }
    }

    private async void ToggleRetroAchievementButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            await _menuOrchestrator.HandleToggleRetroAchievementButton(menuItem.IsChecked);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ToggleRetroAchievementButton_Click.");
        }
    }

    private async void ToggleVideoLinkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            await _menuOrchestrator.HandleToggleVideoLinkButton(menuItem.IsChecked);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ToggleVideoLinkButton_Click.");
        }
    }

    private async void ToggleInfoLinkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            await _menuOrchestrator.HandleToggleInfoLinkButton(menuItem.IsChecked);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ToggleInfoLinkButton_Click.");
        }
    }

    // Emulator config windows
    private void ShowXeniaSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Xenia");
    }

    private void ShowMameSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Mame");
    }

    private void ShowRetroArchSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("RetroArch");
    }

    private void ShowSupermodelSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Supermodel");
    }

    private void ShowMednafenSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Mednafen");
    }

    private void ShowSegaModel2Settings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("SegaModel2");
    }

    private void ShowAresSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Ares");
    }

    private void ShowDaphneSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Daphne");
    }

    private void ShowBlastemSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Blastem");
    }

    private void ShowMesenSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Mesen");
    }

    private void ShowDuckStationSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("DuckStation");
    }

    private void ShowRPCS3Settings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("RPCS3");
    }

    private void ShowFlycastSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Flycast");
    }

    private void ShowStellaSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Stella");
    }

    private void ShowDolphinSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Dolphin");
    }

    private void ShowCemuSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Cemu");
    }

    private void ShowPcsx2Settings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("PCSX2");
    }

    private void ShowAzaharSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Azahar");
    }

    private void ShowYumirSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Yumir");
    }

    private void ShowRaineSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Raine");
    }

    private void ShowRedreamSettings_Click(object sender, RoutedEventArgs e)
    {
        _menuOrchestrator.ShowEmulatorConfigWindow("Redream");
    }

    private void ChangeBaseTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _menuOrchestrator.ChangeBaseTheme(menuItem);
    }

    private void ChangeAccentColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _menuOrchestrator.ChangeAccentColor(menuItem);
    }
}

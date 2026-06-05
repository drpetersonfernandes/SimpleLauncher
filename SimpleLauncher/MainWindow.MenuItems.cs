using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameItemFactory;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

public partial class MainWindow
{
    internal void SetViewMode(string viewMode)
    {
        if (viewMode == "ListView")
        {
            ListView.IsChecked = true;
            GridView.IsChecked = false;
        }
        else
        {
            GridView.IsChecked = true;
            ListView.IsChecked = false;
        }
    }

    private void EasyMode_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleEasyMode();
    }

    private void ExpertMode_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleExpertMode();
    }

    private void DownloadImagePack_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleDownloadImagePack();
    }

    private async void ScanForMicrosoftWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await MenuActionHandlerService.HandleScanForWindowsGames();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ScanForMicrosoftWindowsGames_Click");
        }
    }

    internal async void ResetUiAsync()
    {
        try
        {
            CancelAndRecreateToken();

            if (_isUiUpdating) return;

            _isUiUpdating = true;

            if (_isLoadingGames)
            {
                _isLoadingGames = false;
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }

            try
            {
                ResetPaginationButtons();

                SearchTextBox.Text = "";

                _currentFilter = null;
                _activeSearchQueryOrMode = null;

                _selectedSystem = null;
                PreviewImage.Source = null;
                SystemComboBox.SelectedItem = null;
                EmulatorComboBox.SelectedItem = null;
                SortOrderToggleButton.Visibility = Visibility.Collapsed;
                _mameSortOrder = "FileName";

                var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
                SelectedSystem = nosystemselected;
                PlayTime = "00:00:00";

                await DisplaySystemSelectionScreenAsync(_cancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Do nothing.
            }
            catch (Exception ex)
            {
                // Notify developer
                _logErrors.LogAndForget(ex, "Error in the method ResetUiAsync.");
            }
            finally
            {
                _isUiUpdating = false;
            }
        }
        catch (OperationCanceledException)
        {
            // Do nothing - cancellation is expected when the UI is reset multiple times
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ResetUiAsync.");
        }
    }

    public void LoadOrReloadSystemManager()
    {
        _systemManagers = SystemManager.LoadSystemManagers(_configuration);
        var sortedSystemNames = _systemManagers.Select(static manager => manager.SystemName).OrderBy(static name => name)
            .ToList();
        SystemComboBox.ItemsSource = sortedSystemNames;

        // Re-instantiate factories with the updated _systemManagers list
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, _gameFileGrid, this, _gamePadController, _gameLauncher, _playSoundEffects, _logErrors, _getListOfFiles, _findCoverImage, _imageLoader);
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, PlayHistoryManager, this, _gamePadController, _gameLauncher, _playSoundEffects, _configuration, _logErrors, _getListOfFiles, _findCoverImage, _imageLoader);
    }

    private async void EditLinksClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await MenuActionHandlerService.HandleEditLinks();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method EditLinksClickAsync.");
        }
    }

    private void ToggleGamepad_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        MenuActionHandlerService.HandleToggleGamepad(menuItem.IsChecked);
    }

    private void SetGamepadDeadZone_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleSetGamepadDeadZone();
    }

    private async void ToggleFuzzyMatchingClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            await MenuActionHandlerService.HandleToggleFuzzyMatching(menuItem.IsChecked);
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
            await MenuActionHandlerService.HandleSetFuzzyMatchingThreshold();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method SetFuzzyMatchingThresholdClickAsync.");
        }
    }

    private void Support_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleSupport();
    }

    private void Donate_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleDonate();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleAbout();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleExit();
    }

    private async void ShowAllGamesClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await MenuActionHandlerService.HandleShowGames("ShowAll");
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
            await MenuActionHandlerService.HandleShowGames("ShowWithCover");
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
            await MenuActionHandlerService.HandleShowGames("ShowWithoutCover");
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

            await MenuActionHandlerService.HandleButtonSize(newSize);
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

            await MenuActionHandlerService.HandleButtonAspectRatio(clickedItem.Name);
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

            await MenuActionHandlerService.HandleGamesPerPage(newPage);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method GamesPerPageClickAsync.");
        }
    }

    private void ShowGlobalSearchWindow_Click()
    {
        MenuActionHandlerService.HandleShowGlobalSearch();
    }

    private void ShowGlobalStatsWindow_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleShowGlobalStats();
    }

    private void ShowFavoritesWindow_Click()
    {
        MenuActionHandlerService.HandleShowFavorites();
    }

    private void ShowPlayHistoryWindow_Click()
    {
        MenuActionHandlerService.HandleShowPlayHistory();
    }

    public void ShowRetroAchievementsWindowClick(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleShowRetroAchievements();
    }

    private void UpdateThumbnailSizeCheckMarks(int selectedSize)
    {
        Size50.IsChecked = selectedSize == 50;
        Size100.IsChecked = selectedSize == 100;
        Size150.IsChecked = selectedSize == 150;
        Size200.IsChecked = selectedSize == 200;
        Size250.IsChecked = selectedSize == 250;
        Size300.IsChecked = selectedSize == 300;
        Size350.IsChecked = selectedSize == 350;
        Size400.IsChecked = selectedSize == 400;
        Size450.IsChecked = selectedSize == 450;
        Size500.IsChecked = selectedSize == 500;
        Size550.IsChecked = selectedSize == 550;
        Size600.IsChecked = selectedSize == 600;
        Size650.IsChecked = selectedSize == 650;
        Size700.IsChecked = selectedSize == 700;
        Size750.IsChecked = selectedSize == 750;
        Size800.IsChecked = selectedSize == 800;
    }

    private void UpdateNumberOfGamesPerPageCheckMarks(int selectedSize)
    {
        Page100.IsChecked = selectedSize == 100;
        Page200.IsChecked = selectedSize == 200;
        Page300.IsChecked = selectedSize == 300;
        Page400.IsChecked = selectedSize == 400;
        Page500.IsChecked = selectedSize == 500;
        Page1000.IsChecked = selectedSize == 1000;
        Page10000.IsChecked = selectedSize == 10000;
        Page1000000.IsChecked = selectedSize == 1000000;
    }

    private void UpdateShowGamesCheckMarks(string selectedValue)
    {
        ShowAll.IsChecked = selectedValue == "ShowAll";
        ShowWithCover.IsChecked = selectedValue == "ShowWithCover";
        ShowWithoutCover.IsChecked = selectedValue == "ShowWithoutCover";
    }

    private void UpdateButtonAspectRatioCheckMarks(string selectedValue)
    {
        Square.IsChecked = selectedValue == "Square";
        Wider.IsChecked = selectedValue == "Wider";
        SuperWider.IsChecked = selectedValue == "SuperWider";
        SuperWider2.IsChecked = selectedValue == "SuperWider2";
        Taller.IsChecked = selectedValue == "Taller";
        SuperTaller.IsChecked = selectedValue == "SuperTaller";
        SuperTaller2.IsChecked = selectedValue == "SuperTaller2";
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

            await MenuActionHandlerService.HandleFilenameDisplayMode(mode);
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

            await MenuActionHandlerService.HandleDisplayMachineName(menuItem.IsChecked);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method DisplayMachineName_Click.");
        }
    }

    private void UpdateFilenameDisplayModeCheckMarks(string selectedValue)
    {
        FilenameDisplayOriginal.IsChecked = selectedValue == "Original";
        FilenameDisplayCleanUp.IsChecked = selectedValue == "CleanUp";
        FilenameDisplayNoFilename.IsChecked = selectedValue == "NoFilename";
        DisplayMachineNameToggle.IsChecked = _settings.DisplayMachineName;
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

            await MenuActionHandlerService.HandleFilenameFontSize(size);
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

            await MenuActionHandlerService.HandleMachineNameFontSize(size);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method MachineNameFontSize_Click.");
        }
    }

    private void UpdateFilenameFontSizeCheckMarks(string selectedValue)
    {
        FilenameFontSizeSmall.IsChecked = selectedValue == "Small";
        FilenameFontSizeNormal.IsChecked = selectedValue == "Normal";
        FilenameFontSizeBig.IsChecked = selectedValue == "Big";
    }

    private void UpdateMachineNameFontSizeCheckMarks(string selectedValue)
    {
        MachineNameFontSizeSmall.IsChecked = selectedValue == "Small";
        MachineNameFontSizeNormal.IsChecked = selectedValue == "Normal";
        MachineNameFontSizeBig.IsChecked = selectedValue == "Big";
    }

    private void ChangeViewMode_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleChangeViewMode(sender);
    }

    private void ApplyShowGamesSetting()
    {
        UpdateShowGamesCheckMarks(_settings.ShowGames);
        UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ApplyingGameVisibilitySettings") ?? "Applying game visibility settings...", this);
    }

    private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        MenuActionHandlerService.HandleChangeLanguage(menuItem);
    }

    private void NavRestartButton_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleRestart();
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
            await MenuActionHandlerService.HandleShowSystemFavorites();
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
            await MenuActionHandlerService.HandleFeelingLucky();
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
            await MenuActionHandlerService.HandleShowGamesWithRetroAchievements();
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
            await MenuActionHandlerService.HandleZoomIn();
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
            await MenuActionHandlerService.HandleZoomOut();
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
            await MenuActionHandlerService.HandleToggleViewMode();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method NavToggleViewModeClickAsync.");
        }
    }

    private void SoundConfiguration_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleSoundConfiguration();
    }

    private void ShowRetroAchievementsSettingsWindow_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.HandleShowRetroAchievementsSettings();
    }

    private async void ToggleRetroAchievementButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            await MenuActionHandlerService.HandleToggleRetroAchievementButton(menuItem.IsChecked);
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

            await MenuActionHandlerService.HandleToggleVideoLinkButton(menuItem.IsChecked);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method TgoggleVideoLinkButton_Click.");
        }
    }

    private async void ToggleInfoLinkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem menuItem) return;

            await MenuActionHandlerService.HandleToggleInfoLinkButton(menuItem.IsChecked);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ToggleInfoLinkButton_Click.");
        }
    }

    // Emulator config windows
    private void ShowXeniaSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Xenia");
    }

    private void ShowMameSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Mame");
    }

    private void ShowRetroArchSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("RetroArch");
    }

    private void ShowSupermodelSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Supermodel");
    }

    private void ShowMednafenSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Mednafen");
    }

    private void ShowSegaModel2Settings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("SegaModel2");
    }

    private void ShowAresSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Ares");
    }

    private void ShowDaphneSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Daphne");
    }

    private void ShowBlastemSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Blastem");
    }

    private void ShowMesenSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Mesen");
    }

    private void ShowDuckStationSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("DuckStation");
    }

    private void ShowRPCS3Settings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("RPCS3");
    }

    private void ShowFlycastSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Flycast");
    }

    private void ShowStellaSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Stella");
    }

    private void ShowDolphinSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Dolphin");
    }

    private void ShowCemuSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Cemu");
    }

    private void ShowPcsx2Settings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("PCSX2");
    }

    private void ShowAzaharSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Azahar");
    }

    private void ShowYumirSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Yumir");
    }

    private void ShowRaineSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Raine");
    }

    private void ShowRedreamSettings_Click(object sender, RoutedEventArgs e)
    {
        MenuActionHandlerService.ShowEmulatorConfigWindow("Redream");
    }

    private void ChangeBaseTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _themeMenuService.ChangeBaseTheme(menuItem);
    }

    private void ChangeAccentColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _themeMenuService.ChangeAccentColor(menuItem);
    }
}
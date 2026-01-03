using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;
using SimpleLauncher.UiHelpers;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void SetViewMode(string viewMode)
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

    private void SetLanguageAndCheckMenu(string languageCode)
    {
        LanguageArabic.IsChecked = languageCode == "ar";
        LanguageBengali.IsChecked = languageCode == "bn";
        LanguageGerman.IsChecked = languageCode == "de";
        LanguageEnglish.IsChecked = languageCode == "en";
        LanguageSpanish.IsChecked = languageCode == "es";
        LanguageFrench.IsChecked = languageCode == "fr";
        LanguageHindi.IsChecked = languageCode == "hi";
        LanguageIndonesianMalay.IsChecked = languageCode == "id";
        LanguageItalian.IsChecked = languageCode == "it";
        LanguageJapanese.IsChecked = languageCode == "ja";
        LanguageKorean.IsChecked = languageCode == "ko";
        LanguageDutch.IsChecked = languageCode == "nl";
        LanguagePortugueseBr.IsChecked = languageCode == "pt-br";
        LanguageRussian.IsChecked = languageCode == "ru";
        LanguageTurkish.IsChecked = languageCode == "tr";
        LanguageUrdu.IsChecked = languageCode == "ur";
        LanguageVietnamese.IsChecked = languageCode == "vi";
        LanguageChineseSimplified.IsChecked = languageCode == "zh-hans";
        LanguageChineseTraditional.IsChecked = languageCode == "zh-hant";
    }

    private void EasyMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningEasyMode") ?? "Opening Easy Mode...", this);

            EasyModeWindow editSystemEasyModeAddSystemWindow = new();
            editSystemEasyModeAddSystemWindow.Owner = this;
            editSystemEasyModeAddSystemWindow.ShowDialog();

            LoadOrReloadSystemManager();

            ResetUiAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method EasyMode_Click.");
        }
    }

    private void ExpertMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningExpertMode") ?? "Opening Expert Mode...", this);

            EditSystemWindow editSystemWindow = new(_settings, _playSoundEffects);
            editSystemWindow.Owner = this;
            editSystemWindow.ShowDialog();

            LoadOrReloadSystemManager();

            ResetUiAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ExpertMode_Click.");
        }
    }

    private void DownloadImagePack_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningImagePackDownloader") ?? "Opening Image Pack Downloader...", this);

            ResetUiAsync();

            DownloadImagePackWindow downloadImagePack = new();
            downloadImagePack.Owner = this;
            downloadImagePack.ShowDialog();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method DownloadImagePack_Click.");
        }
    }

    private async void ScanForMicrosoftWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();

            SetUiLoadingState(true, (string)Application.Current.TryFindResource("ScanningForWindowsGames") ?? "Scanning for Windows games...");
            try
            {
                await _gameScannerService.ScanForStoreGamesAsync();
                if (_gameScannerService.WasNewSystemCreated)
                {
                    UpdateStatusBar.UpdateContent("Found new Microsoft Windows games. Refreshing system list.", this);
                    LoadOrReloadSystemManager(); // Reload to get the new system
                    // After reloading, the system selection screen needs to be updated.
                    await DisplaySystemSelectionScreenAsync();
                }
            }
            catch (Exception ex)
            {
                _ = _logErrors.LogErrorAsync(ex, "Error during initial Windows games scan.");
            }
            finally
            {
                SetUiLoadingState(false);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ScanForMicrosoftWindowsGames_Click.");
        }
    }

    private async void ResetUiAsync()
    {
        try
        {
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

                await DisplaySystemSelectionScreenAsync();
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(ex, "Error in the method ResetUiAsync.");
            }
            finally
            {
                _isUiUpdating = false;
            }
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ResetUiAsync.");
        }
    }

    public void LoadOrReloadSystemManager()
    {
        _systemManagers = SystemManager.LoadSystemManagers();
        var sortedSystemNames = _systemManagers.Select(static manager => manager.SystemName).OrderBy(static name => name)
            .ToList();
        SystemComboBox.ItemsSource = sortedSystemNames;

        // Re-instantiate factories with the updated _systemManagers list
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, _gameFileGrid, this, _gamePadController, _gameLauncher, _playSoundEffects, _logErrors);
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, PlayHistoryManager, this, _gamePadController, _gameLauncher, _playSoundEffects);
    }

    private async void EditLinksClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            try
            {
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningLinkEditor") ?? "Opening Link Editor...", this);
                _playSoundEffects.PlayNotificationSound();

                SetLinksWindow editLinksWindow = new(_settings);
                editLinksWindow.ShowDialog();

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(ex, "Error in the method EditLinksClickAsync.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method EditLinksClickAsync.");
        }
    }

    private void ToggleGamepad_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        try
        {
            _playSoundEffects.PlayNotificationSound();

            // Update the settings
            _settings.EnableGamePadNavigation = menuItem.IsChecked;
            _settings.Save();

            // Start or stop the GamePadController
            if (menuItem.IsChecked)
            {
                _gamePadController.Start();
            }
            else
            {
                _gamePadController.Stop();
            }

            // Notify user
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingGamepadNavigation") ?? "Toggling gamepad navigation...", this);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to toggle gamepad.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ToggleGamepadFailureMessageBox();
        }
    }

    private void SetGamepadDeadZone_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningGamepadDeadZoneSettings") ?? "Opening Gamepad Dead Zone settings...", this);

        SetGamepadDeadZoneWindow setGamepadDeadZoneWindow = new(_settings);
        setGamepadDeadZoneWindow.ShowDialog();

        // Update the GamePadController dead zone settings from SettingsManager
        _gamePadController.DeadZoneX = _settings.DeadZoneX;

        _gamePadController.DeadZoneY = _settings.DeadZoneY;

        if (_settings.EnableGamePadNavigation)
        {
            _gamePadController.Stop();
            _gamePadController.Start();
        }
        else
        {
            _gamePadController.Stop();
        }
    }

    private async void ToggleFuzzyMatchingClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            if (sender is not MenuItem menuItem) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                _settings.EnableFuzzyMatching = menuItem.IsChecked;
                _settings.Save();

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingFuzzyMatching") ?? "Toggling fuzzy matching...", this);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Failed to toggle fuzzy matching.";
                _ = _logErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ToggleFuzzyMatchingFailureMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ToggleFuzzyMatchingClickAsync.");
        }
    }

    private async void SetFuzzyMatchingThresholdClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();

            var setThresholdWindow = new SetFuzzyMatchingWindow(_settings);
            setThresholdWindow.ShowDialog();

            // After the dialog closes, the settings are saved within the dialog.
            if (!_settings.EnableFuzzyMatching) return;

            var (sl, sq) = GetLoadGameFilesParams();
            // Notify user
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningFuzzyMatchingSettings") ?? "Opening fuzzy matching settings...", this);
            await LoadGameFilesAsync(sl, sq);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in method SetFuzzyMatchingThresholdClickAsync");
        }
    }

    private void Support_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningSupportWindow") ?? "Opening support window...", this);
        _playSoundEffects.PlayNotificationSound();

        SupportWindow supportRequestWindow = new();
        supportRequestWindow.Owner = this;
        supportRequestWindow.ShowDialog();
    }

    private void Donate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningDonationPage") ?? "Opening donation page...", this);

            var psi = new ProcessStartInfo
            {
                FileName = "https://www.purelogiccode.com/Donate",
                UseShellExecute = true
            };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Unable to open the Donation Link from the menu.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorOpeningDonationLinkMessageBox();
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningAboutWindow") ?? "Opening About window...", this);

        AboutWindow aboutWindow = new();
        aboutWindow.Owner = this;
        aboutWindow.ShowDialog();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        Close();
    }

    private async void ShowAllGamesClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ApplyingGameVisibilityFilter") ?? "Applying game visibility filter...", this);
            if (_isLoadingGames) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                UpdateShowGamesSetting("ShowAll");
                UpdateMenuCheckMarks("ShowAll");
                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(ex, "Error in the method ShowAllGamesClickAsync.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ShowAllGamesClickAsync.");
        }
    }

    private async void ShowGamesWithCoverClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ApplyingGameVisibilityFilter") ?? "Applying game visibility filter...", this);
            if (_isLoadingGames) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                UpdateShowGamesSetting("ShowWithCover");
                UpdateMenuCheckMarks("ShowWithCover");

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithCoverClickAsync.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithCoverClickAsync.");
        }
    }

    private async void ShowGamesWithoutCoverClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ApplyingGameVisibilityFilter") ?? "Applying game visibility filter...", this);
            if (_isLoadingGames) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                UpdateShowGamesSetting("ShowWithoutCover");
                UpdateMenuCheckMarks("ShowWithoutCover");

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithoutCoverClickAsync.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithoutCoverClickAsync.");
        }
    }

    private void UpdateShowGamesSetting(string showGames)
    {
        _settings.ShowGames = showGames;
        _settings.Save();
    }

    private void UpdateMenuCheckMarks(string selectedMenu)
    {
        ShowAll.IsChecked = selectedMenu == "ShowAll";
        ShowWithCover.IsChecked = selectedMenu == "ShowWithCover";
        ShowWithoutCover.IsChecked = selectedMenu == "ShowWithoutCover";
    }

    private async void ButtonSizeClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            if (sender is not MenuItem clickedItem) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                var sizeText = clickedItem.Name.Replace("Size", "");

                if (!int.TryParse(new string(sizeText.Where(char.IsDigit).ToArray()), out var newSize)) return;

                _gameButtonFactory.ImageHeight = newSize; // Update the image height
                _settings.ThumbnailSize = newSize;
                _settings.Save();

                UpdateThumbnailSizeCheckMarks(newSize);
                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("AdjustingButtonSize") ?? "Adjusting button size...", this);

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Error in method ButtonSizeClickAsync.";
                _ = _logErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ButtonSizeClickAsync.");
        }
    }

    private async void ButtonAspectRatioClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("AdjustingButtonAspectRatio") ?? "Adjusting button aspect ratio...", this);
            if (_isLoadingGames) return;

            if (sender is not MenuItem clickedItem) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                var aspectRatio = clickedItem.Name;
                _settings.ButtonAspectRatio = aspectRatio;
                _settings.Save();

                UpdateButtonAspectRatioCheckMarks(aspectRatio);

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error in method ButtonAspectRatioClickAsync";
                _ = _logErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method ButtonAspectRatioClickAsync.");
        }
    }

    private async void GamesPerPageClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            if (sender is not MenuItem clickedItem) return;

            try
            {
                if (clickedItem.Name is "Page1000" or "Page10000" or "Page1000000")
                {
                    MessageBoxLibrary.WarnUserAboutMemoryConsumption();
                }

                _playSoundEffects.PlayNotificationSound();

                var pageText = clickedItem.Name.Replace("Page", "");
                if (!int.TryParse(new string(pageText.Where(char.IsDigit).ToArray()), out var newPage)) return;

                _filesPerPage = newPage;
                _paginationThreshold = newPage;
                _settings.GamesPerPage = newPage;

                _settings.Save();
                UpdateNumberOfGamesPerPageCheckMarks(newPage);
                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("AdjustingGamesPerPage") ?? "Adjusting games per page...", this);

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(ex, "Error in the method GamesPerPageClickAsync.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method GamesPerPageClickAsync.");
        }
    }

    private void ShowGlobalSearchWindow_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningGlobalSearch") ?? "Opening Global Search...", this);

        ResetUiAsync();

        var globalSearchWindow = new GlobalSearchWindow(_systemManagers, _machines, _mameLookup, _favoritesManager, _settings, this, _gamePadController, _gameLauncher, _playSoundEffects, _logErrors);
        globalSearchWindow.Owner = this;
        globalSearchWindow.Show();
    }

    private void ShowGlobalStatsWindow_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningGlobalStatistics") ?? "Opening Global Statistics...", this);
        _playSoundEffects.PlayNotificationSound();

        var globalStatsWindow = new GlobalStatsWindow(_systemManagers);
        globalStatsWindow.Owner = this;
        globalStatsWindow.Show();
    }

    private void ShowFavoritesWindow_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningFavorites") ?? "Opening Favorites...", this);
        _playSoundEffects.PlayNotificationSound();

        ResetUiAsync();

        var favoritesWindow = new FavoritesWindow(_settings, _systemManagers, _machines, _favoritesManager, this, _gamePadController, _gameLauncher, _playSoundEffects);
        favoritesWindow.Owner = this;
        favoritesWindow.Show();
    }

    private void ShowPlayHistoryWindow_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningPlayHistory") ?? "Opening Play History...", this);

        ResetUiAsync();

        var playHistoryWindow = new PlayHistoryWindow(_systemManagers, _machines, _settings, _favoritesManager, PlayHistoryManager, this, _gamePadController, _gameLauncher, _playSoundEffects);
        playHistoryWindow.Owner = this;
        playHistoryWindow.Show();
    }

    private void ShowRetroAchievementsWindow_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievements") ?? "Opening RetroAchievements...", this);

        var retroAchievementsWindow = new RetroAchievementsWindow();
        retroAchievementsWindow.Owner = this;
        retroAchievementsWindow.Show();
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

    private void ChangeViewMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();

            if (Equals(sender, GridView))
            {
                GridView.IsChecked = true;
                ListView.IsChecked = false;
                _settings.ViewMode = "GridView";

                GameFileGrid.Visibility = Visibility.Visible;
                ListViewPreviewArea.Visibility = Visibility.Collapsed;

                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingViewMode") ?? "Changing view mode...", this);

                ResetUiAsync();
            }
            else if (Equals(sender, ListView))
            {
                GridView.IsChecked = false;
                ListView.IsChecked = true;
                _settings.ViewMode = "ListView";

                GameFileGrid.Visibility = Visibility.Collapsed;
                ListViewPreviewArea.Visibility = Visibility.Visible;
                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingViewMode") ?? "Changing view mode...", this);

                ResetUiAsync();
            }

            _settings.Save(); // Save the updated ViewMode
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error while using the method ChangeViewMode_Click.";
            _ = _logErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ErrorChangingViewModeMessageBox();
        }
    }

    private void ApplyShowGamesSetting()
    {
        UpdateMenuCheckMarks(_settings.ShowGames);
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ApplyingGameVisibilitySettings") ?? "Applying game visibility settings...", this);
    }

    private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        _playSoundEffects.PlayNotificationSound();

        var selectedLanguage = menuItem.Name switch
        {
            "LanguageArabic" => "ar",
            "LanguageBengali" => "bn",
            "LanguageGerman" => "de",
            "LanguageEnglish" => "en",
            "LanguageSpanish" => "es",
            "LanguageFrench" => "fr",
            "LanguageHindi" => "hi",
            "LanguageIndonesianMalay" => "id",
            "LanguageItalian" => "it",
            "LanguageJapanese" => "ja",
            "LanguageKorean" => "ko",
            "LanguageDutch" => "nl",
            "LanguagePortugueseBr" => "pt-br",
            "LanguageRussian" => "ru",
            "LanguageTurkish" => "tr",
            "LanguageUrdu" => "ur",
            "LanguageVietnamese" => "vi",
            "LanguageChineseSimplified" => "zh-hans",
            "LanguageChineseTraditional" => "zh-hant",
            _ => "en"
        };

        _settings.Language = selectedLanguage;
        _settings.Save();

        SetLanguageAndCheckMenu(selectedLanguage);

        // Notify user
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingLanguage") ?? "Changing language...", this);
        SaveApplicationSettings();

        QuitApplication.RestartApplication();
    }

    private void NavRestartButton_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();

        ResetUiAsync();
    }

    private void NavToggleLightDarkMode_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ChangingBaseTheme") ?? "Changing Base Theme...", this);
        _playSoundEffects.PlayNotificationSound();

        var baseTheme = _settings.BaseTheme;
        var currentAccent = _settings.AccentColor;

        if (baseTheme == "Light")
        {
            baseTheme = "Dark";
        }
        else
        {
            baseTheme = "Light";
        }

        App.ChangeTheme(baseTheme, currentAccent);
        UncheckBaseThemes();
        SetCheckedTheme(baseTheme, currentAccent);
    }

    private void NavGlobalSearchButton_Click(object sender, RoutedEventArgs e)
    {
        ShowGlobalSearchWindow_Click(sender, e);
    }

    private void NavFavoritesButton_Click(object sender, RoutedEventArgs e)
    {
        ShowFavoritesWindow_Click(sender, e);
    }

    private void NavHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        ShowPlayHistoryWindow_Click(sender, e);
    }

    private void NavRetroAchievementsButton_Click(object sender, RoutedEventArgs e)
    {
        ShowRetroAchievementsWindow_Click(sender, e);
    }

    private void NavExpertModeButton_Click(object sender, RoutedEventArgs e)
    {
        ExpertMode_Click(sender, e);
    }

    private async void NavSelectedSystemFavoriteButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingFavoriteGamesForSystem") ?? "Loading favorite games for system...", this);
            _playSoundEffects.PlayNotificationSound();
            await ShowSystemFavoriteGamesClickAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in method NavSelectedSystemFavoriteButtonClickAsync.");
        }
    }

    private async void NavRandomLuckGameButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("PickingARandomGame") ?? "Picking a random game...", this);
            _playSoundEffects.PlayNotificationSound();
            await ShowSystemFeelingLuckyClickAsync(sender, e);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method NavRandomLuckGameButtonClickAsync.");
        }
    }

    private async void NavZoomInButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                const int zoomStep = 50;
                const int maxSize = 800;
                var newSize = Math.Min(maxSize, _settings.ThumbnailSize + zoomStep);

                if (newSize == _settings.ThumbnailSize)
                {
                    return; // No change in size, no need to reload
                }

                _gameButtonFactory.ImageHeight = newSize; // Update the image height
                _settings.ThumbnailSize = newSize;
                _settings.Save();
                UpdateThumbnailSizeCheckMarks(newSize);

                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ZoomingIn") ?? "Zooming in...", this);
                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Error in method NavZoomInButtonClickAsync.";
                _ = _logErrors.LogErrorAsync(ex, errorMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method NavZoomInButtonClickAsync.");
        }
    }

    private async void NavZoomOutButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                const int zoomStep = 50;
                const int minSize = 50;
                var newSize = Math.Max(minSize, _settings.ThumbnailSize - zoomStep);

                if (newSize == _settings.ThumbnailSize)
                {
                    return; // No change in size, no need to reload
                }

                _gameButtonFactory.ImageHeight = newSize; // Update the image height
                _settings.ThumbnailSize = newSize;
                _settings.Save();
                UpdateThumbnailSizeCheckMarks(newSize);

                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ZoomingOut") ?? "Zooming out...", this);
                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Error in method NavZoomOutButtonClickAsync.";
                _ = _logErrors.LogErrorAsync(ex, errorMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method NavZoomOutButtonClickAsync.");
        }
    }

    private async void NavToggleViewModeClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingViewMode") ?? "Toggling view mode...", this);
            if (_isLoadingGames) return;

            try
            {
                _playSoundEffects.PlayNotificationSound();

                if (_settings.ViewMode == "GridView")
                {
                    // Switch to the ListView
                    GridView.IsChecked = false;
                    ListView.IsChecked = true;
                    _settings.ViewMode = "ListView";

                    GameFileGrid.Visibility = Visibility.Collapsed;
                    ListViewPreviewArea.Visibility = Visibility.Visible;
                }
                else // Assuming it's "ListView"
                {
                    // Switch to GridView
                    GridView.IsChecked = true;
                    ListView.IsChecked = false;
                    _settings.ViewMode = "GridView";

                    GameFileGrid.Visibility = Visibility.Visible;
                    ListViewPreviewArea.Visibility = Visibility.Collapsed;
                }

                _settings.Save();

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Error while using the method NavToggleViewModeClickAsync.";
                _ = _logErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.ErrorChangingViewModeMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the method NavToggleViewModeClickAsync.");
        }
    }

    private void SoundConfiguration_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningSoundConfigurationSettings") ?? "Opening Sound Configuration settings...", this);

            var soundConfigWindow = new SoundConfigurationWindow(_settings, _playSoundEffects, _logErrors);
            soundConfigWindow.Owner = this;
            soundConfigWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error opening Sound Configuration window.");

            // Notify user
            MessageBoxLibrary.CouldNotOpenSoundConfigurationWindow();
        }
    }

    private void ShowRetroAchievementsSettingsWindow_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("OpeningRetroAchievementsSettings") ?? "Opening RetroAchievements settings...", this);

            var raSettingsWindow = new RetroAchievementsSettingsWindow(_settings);
            raSettingsWindow.Owner = this;
            raSettingsWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error opening RetroAchievements settings window.");
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    private void ToggleRetroAchievementButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingRetroAchievementsOverlayButton") ?? "Toggling RetroAchievements overlay button...", this);
        try
        {
            _playSoundEffects.PlayNotificationSound();

            _settings.OverlayRetroAchievementButton = menuItem.IsChecked;
            _settings.Save();

            // Reload game files to reflect the change in overlay buttons
            var (sl, sq) = GetLoadGameFilesParams();
            _ = LoadGameFilesAsync(sl, sq); // Use _ = to avoid blocking UI
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error toggling RetroAchievements overlay button.");
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    private void ToggleVideoLinkButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingVideoLinkOverlayButton") ?? "Toggling video link overlay button...", this);
        try
        {
            _playSoundEffects.PlayNotificationSound();

            _settings.OverlayOpenVideoButton = menuItem.IsChecked;
            _settings.Save();

            // Reload game files to reflect the change in overlay buttons
            var (sl, sq) = GetLoadGameFilesParams();
            _ = LoadGameFilesAsync(sl, sq); // Use _ = to avoid blocking UI
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error toggling video link overlay button.");
            MessageBoxLibrary.ErrorMessageBox(); // Generic error for the user
        }
    }

    private void ToggleInfoLinkButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingInfoLinkOverlayButton") ?? "Toggling info link overlay button...", this);
        try
        {
            _playSoundEffects.PlayNotificationSound();

            _settings.OverlayOpenInfoButton = menuItem.IsChecked;
            _settings.Save();

            // Reload game files to reflect the change in overlay buttons
            var (sl, sq) = GetLoadGameFilesParams();
            _ = LoadGameFilesAsync(sl, sq); // Use _ = to avoid blocking UI
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error toggling info link overlay button.");
            MessageBoxLibrary.ErrorMessageBox(); // Generic error for the user
        }
    }
}
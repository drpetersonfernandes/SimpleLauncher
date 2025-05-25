using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

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
            PlayClick.PlayNotificationSound();

            ResetUi();

            EasyModeWindow editSystemEasyModeAddSystemWindow = new();
            editSystemEasyModeAddSystemWindow.ShowDialog();

            LoadOrReloadSystemConfig();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method EasyMode_Click.");
        }
    }

    private void ExpertMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PlayClick.PlayNotificationSound();

            ResetUi();

            EditSystemWindow editSystemWindow = new(_settings);
            editSystemWindow.ShowDialog();

            LoadOrReloadSystemConfig();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ExpertMode_Click.");
        }
    }

    private void DownloadImagePack_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PlayClick.PlayNotificationSound();

            ResetUi();

            DownloadImagePackWindow downloadImagePack = new();
            downloadImagePack.ShowDialog();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method DownloadImagePack_Click.");
        }
    }

    private async void ResetUi()
    {
        try
        {
            ResetPaginationButtons();

            SearchTextBox.Text = "";

            _currentFilter = null;
            _activeSearchQueryOrMode = null; // Reset active search mode

            _selectedSystem = null;
            PreviewImage.Source = null;
            SystemComboBox.SelectedItem = null;

            var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
            SelectedSystem = nosystemselected;
            PlayTime = "00:00:00";

            await DisplaySystemSelectionScreenAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ResetUi.");
        }
    }

    private void LoadOrReloadSystemConfig()
    {
        _systemConfigs = SystemManager.LoadSystemConfigs();
        var sortedSystemNames = _systemConfigs.Select(static config => config.SystemName).OrderBy(static name => name)
            .ToList();
        SystemComboBox.ItemsSource = sortedSystemNames;
    }

    private async void EditLinks_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            try
            {
                PlayClick.PlayNotificationSound();

                SetLinksWindow editLinksWindow = new(_settings);
                editLinksWindow.ShowDialog();

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error in the method EditLinks_Click.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method EditLinks_Click.");
        }
    }

    private void ToggleGamepad_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        try
        {
            PlayClick.PlayNotificationSound();

            // Update the settings
            _settings.EnableGamePadNavigation = menuItem.IsChecked;
            _settings.Save();

            // Start or stop the GamePadController
            if (menuItem.IsChecked)
            {
                GamePadController.Instance2.Start();
            }
            else
            {
                GamePadController.Instance2.Stop();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to toggle gamepad.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ToggleGamepadFailureMessageBox();
        }
    }

    private void SetGamepadDeadZone_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        SetGamepadDeadZoneWindow setGamepadDeadZoneWindow = new(_settings);
        setGamepadDeadZoneWindow.ShowDialog();

        // Update the GamePadController dead zone settings from SettingsManager
        GamePadController.Instance2.DeadZoneX = _settings.DeadZoneX;
        GamePadController.Instance2.DeadZoneY = _settings.DeadZoneY;

        if (_settings.EnableGamePadNavigation)
        {
            GamePadController.Instance2.Stop();
            GamePadController.Instance2.Start();
        }
        else
        {
            GamePadController.Instance2.Stop();
        }
    }

    private async void ToggleFuzzyMatching_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            if (sender is not MenuItem menuItem) return;

            try
            {
                PlayClick.PlayNotificationSound();

                _settings.EnableFuzzyMatching = menuItem.IsChecked;
                _settings.Save();

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Failed to toggle fuzzy matching.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ToggleFuzzyMatchingFailureMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ToggleFuzzyMatching_Click.");
        }
    }

    private async void SetFuzzyMatchingThreshold_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PlayClick.PlayNotificationSound();

            var setThresholdWindow = new SetFuzzyMatchingWindow(_settings);
            setThresholdWindow.ShowDialog();

            // After the dialog closes, the settings are saved within the dialog.
            if (!_settings.EnableFuzzyMatching) return;

            var (sl, sq) = GetLoadGameFilesParams();
            await LoadGameFilesAsync(sl, sq);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in method SetFuzzyMatchingThreshold_Click");
        }
    }

    private void Support_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        SupportWindow supportRequestWindow = new();
        supportRequestWindow.ShowDialog();
    }

    private void Donate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PlayClick.PlayNotificationSound();

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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorOpeningDonationLinkMessageBox();
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        AboutWindow aboutWindow = new();
        aboutWindow.ShowDialog();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ShowAllGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            try
            {
                PlayClick.PlayNotificationSound();

                UpdateShowGamesSetting("ShowAll");
                UpdateMenuCheckMarks("ShowAll");
                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error in the method ShowAllGames_Click.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ShowAllGames_Click.");
        }
    }

    private async void ShowGamesWithCover_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            try
            {
                PlayClick.PlayNotificationSound();

                UpdateShowGamesSetting("ShowWithCover");
                UpdateMenuCheckMarks("ShowWithCover");

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithCover_Click.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithCover_Click.");
        }
    }

    private async void ShowGamesWithoutCover_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            try
            {
                PlayClick.PlayNotificationSound();

                UpdateShowGamesSetting("ShowWithoutCover");
                UpdateMenuCheckMarks("ShowWithoutCover");

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithoutCover_Click.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithoutCover_Click.");
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

    private async void ButtonSize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            if (sender is not MenuItem clickedItem) return;

            try
            {
                PlayClick.PlayNotificationSound();

                var sizeText = clickedItem.Name.Replace("Size", "");

                if (!int.TryParse(new string(sizeText.Where(char.IsDigit).ToArray()), out var newSize)) return;

                _gameButtonFactory.ImageHeight = newSize; // Update the image height
                _settings.ThumbnailSize = newSize;
                _settings.Save();

                UpdateThumbnailSizeCheckMarks(newSize);

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Error in method ButtonSize_Click.";
                _ = LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ButtonSize_Click.");
        }
    }

    private async void ButtonAspectRatio_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            if (sender is not MenuItem clickedItem) return;

            try
            {
                PlayClick.PlayNotificationSound();

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
                const string contextMessage = "Error in method ButtonAspectRatio_Click";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ButtonAspectRatio_Click.");
        }
    }

    private async void GamesPerPage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            if (sender is not MenuItem clickedItem) return;

            try
            {
                PlayClick.PlayNotificationSound();

                var pageText = clickedItem.Name.Replace("Page", "");
                if (!int.TryParse(new string(pageText.Where(char.IsDigit).ToArray()), out var newPage)) return;

                _filesPerPage = newPage;
                _paginationThreshold = newPage;
                _settings.GamesPerPage = newPage;

                _settings.Save();
                UpdateNumberOfGamesPerPageCheckMarks(newPage);

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error in the method GamesPerPage_Click.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method GamesPerPage_Click.");
        }
    }

    private void ShowGlobalSearchWindow_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        ResetUi();

        var globalSearchWindow =
            new GlobalSearchWindow(_systemConfigs, _machines, _mameLookup, _settings, _favoritesManager, this);
        globalSearchWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
    }

    private void ShowGlobalStatsWindow_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        var globalStatsWindow = new GlobalStatsWindow(_systemConfigs);
        globalStatsWindow.Show();
    }

    private void ShowFavoritesWindow_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        ResetUi();

        var favoritesWindow = new FavoritesWindow(_settings, _systemConfigs, _machines, _favoritesManager, this);
        favoritesWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
    }

    private void ShowPlayHistoryWindow_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        ResetUi();

        var playHistoryWindow = new PlayTimeWindow(_systemConfigs, _machines, _settings, _favoritesManager,
            _playHistoryManager, this);
        playHistoryWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
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
        Taller.IsChecked = selectedValue == "Taller";
        SuperTaller.IsChecked = selectedValue == "SuperTaller";
    }

    private void ChangeViewMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PlayClick.PlayNotificationSound();

            if (Equals(sender, GridView))
            {
                GridView.IsChecked = true;
                ListView.IsChecked = false;
                _settings.ViewMode = "GridView";

                GameFileGrid.Visibility = Visibility.Visible;
                ListViewPreviewArea.Visibility = Visibility.Collapsed;

                ResetUi();
            }
            else if (Equals(sender, ListView))
            {
                GridView.IsChecked = false;
                ListView.IsChecked = true;
                _settings.ViewMode = "ListView";

                GameFileGrid.Visibility = Visibility.Collapsed;
                ListViewPreviewArea.Visibility = Visibility.Visible;

                ResetUi();
            }

            _settings.Save(); // Save the updated ViewMode
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error while using the method ChangeViewMode_Click.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ErrorChangingViewModeMessageBox();
        }
    }

    private void ApplyShowGamesSetting()
    {
        UpdateMenuCheckMarks(_settings.ShowGames);
    }

    private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        PlayClick.PlayNotificationSound();

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

        SaveApplicationSettings();

        QuitApplication.RestartApplication();
    }

    private void NavRestartButton_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        ResetUi();
    }

    private void NavGlobalSearchButton_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        ShowGlobalSearchWindow_Click(sender, e);
    }

    private void NavFavoritesButton_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        ShowFavoritesWindow_Click(sender, e);
    }

    private void NavHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        ShowPlayHistoryWindow_Click(sender, e);
    }

    private void NavExpertModeButton_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayNotificationSound();

        ExpertMode_Click(sender, e);
    }

    private async void NavSelectedSystemFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ShowSystemFavoriteGames_Click();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in method NavSelectedSystemFavoriteButton_Click.");
        }
    }

    private async void NavRandomLuckGameButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ShowSystemFeelingLucky_Click(sender, e);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method NavRandomLuckGameButton_Click.");
        }
    }

    private async void NavZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            try
            {
                PlayClick.PlayNotificationSound();

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

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Error in method NavZoomInButton_Click.";
                _ = LogErrors.LogErrorAsync(ex, errorMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method NavZoomInButton_Click.");
        }
    }

    private async void NavZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            try
            {
                PlayClick.PlayNotificationSound();

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

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Error in method NavZoomOutButton_Click.";
                _ = LogErrors.LogErrorAsync(ex, errorMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method NavZoomOutButton_Click.");
        }
    }

    private async void NavToggleViewMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            try
            {
                PlayClick.PlayNotificationSound();

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
                const string errorMessage = "Error while using the method NavToggleViewMode_Click.";
                _ = LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.ErrorChangingViewModeMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the method NavToggleViewMode_Click.");
        }
    }
}

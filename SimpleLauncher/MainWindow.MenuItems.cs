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
            // Ensure pagination is reset at the beginning
            ResetPaginationButtons();

            // Clear SearchTextBox
            SearchTextBox.Text = "";

            // Update current filter
            _currentFilter = null;

            // Empty SystemComboBox
            _selectedSystem = null;
            SystemComboBox.SelectedItem = null;
            var nosystemselected =
                (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
            SelectedSystem = nosystemselected;
            PlayTime = "00:00:00";

            AddNoSystemMessage();

            EasyModeWindow editSystemEasyModeAddSystemWindow = new();
            editSystemEasyModeAddSystemWindow.ShowDialog();

            // ReLoad and Sort _systemConfigs
            _systemConfigs = SystemManager.LoadSystemConfigs();
            var sortedSystemNames = _systemConfigs.Select(static config => config.SystemName)
                .OrderBy(static name => name).ToList();
            SystemComboBox.ItemsSource = sortedSystemNames;

            // Refresh GameList
            // await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method EasyMode_Click.");
        }
    }

    private void ExpertMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Ensure pagination is reset at the beginning
            ResetPaginationButtons();

            // Clear SearchTextBox
            SearchTextBox.Text = "";

            // Update current filter
            _currentFilter = null;

            // Empty SystemComboBox
            _selectedSystem = null;
            SystemComboBox.SelectedItem = null;
            var nosystemselected =
                (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
            SelectedSystem = nosystemselected;
            PlayTime = "00:00:00";

            AddNoSystemMessage();

            EditSystemWindow editSystemWindow = new(_settings);
            editSystemWindow.ShowDialog();

            // ReLoad and Sort _systemConfigs
            _systemConfigs = SystemManager.LoadSystemConfigs();
            var sortedSystemNames = _systemConfigs.Select(static config => config.SystemName)
                .OrderBy(static name => name).ToList();
            SystemComboBox.ItemsSource = sortedSystemNames;

            // Refresh GameList
            // await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ExpertMode_Click.");
        }
    }

    private void DownloadImagePack_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            ResetUi();

            DownloadImagePackWindow downloadImagePack = new();
            downloadImagePack.ShowDialog();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method DownloadImagePack_Click.");
        }
    }

    private void LoadOrReloadSystemConfig()
    {
        _systemConfigs = SystemManager.LoadSystemConfigs();
        var sortedSystemNames = _systemConfigs.Select(static config => config.SystemName).OrderBy(static name => name)
            .ToList();
        SystemComboBox.ItemsSource = sortedSystemNames;
    }

    private void ResetUi()
    {
        // Ensure pagination is reset at the beginning
        ResetPaginationButtons();

        // Clear SearchTextBox
        SearchTextBox.Text = "";

        // Update current filter
        _currentFilter = null;

        // Empty SystemComboBox
        _selectedSystem = null;
        SystemComboBox.SelectedItem = null;
        var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        SelectedSystem = nosystemselected;
        PlayTime = "00:00:00";

        AddNoSystemMessage();
    }

    private async void EditLinks_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetLinksWindow editLinksWindow = new(_settings);
            editLinksWindow.ShowDialog();

            // Refresh GameList
            await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method EditLinks_Click.");
        }
    }

    private void ToggleGamepad_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem menuItem) return;

        try
        {
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
            if (sender is not MenuItem menuItem) return;

            try
            {
                _settings.EnableFuzzyMatching = menuItem.IsChecked;
                _settings.Save();

                // Re-load game files to apply the new setting
                await LoadGameFilesAsync(_currentFilter, SearchTextBox.Text.Trim());
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
            _ = LogErrors.LogErrorAsync(ex, "Error in method ToggleFuzzyMatching_Click");
        }
    }

    private void SetFuzzyMatchingThreshold_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Pass the current settings manager to the dialog
            var setThresholdWindow = new SetFuzzyMatchingWindow(_settings);
            setThresholdWindow.ShowDialog(); // Use ShowDialog() to make it modal

            // After the dialog closes, the settings are saved within the dialog.
            // No need to explicitly save here.
            // Re-load game files to apply the new threshold if fuzzy matching is enabled
            if (_settings.EnableFuzzyMatching)
            {
                _ = LoadGameFilesAsync(_currentFilter, SearchTextBox.Text.Trim());
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to open Set Fuzzy Matching Threshold window.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.SetFuzzyMatchingThresholdFailureMessageBox();
        }
    }

    private void Support_Click(object sender, RoutedEventArgs e)
    {
        SupportWindow supportRequestWindow = new();
        supportRequestWindow.ShowDialog();
    }

    private void Donate_Click(object sender, RoutedEventArgs e)
    {
        try
        {
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
            UpdateShowGamesSetting("ShowAll");
            UpdateMenuCheckMarks("ShowAll");
            await LoadGameFilesAsync(_currentFilter, SearchTextBox.Text.Trim());
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ShowAllGames_Click.");
        }
    }

    private async void ShowGamesWithCover_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateShowGamesSetting("ShowWithCover");
            UpdateMenuCheckMarks("ShowWithCover");
            await LoadGameFilesAsync(_currentFilter, SearchTextBox.Text.Trim());
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method ShowGamesWithCover_Click.");
        }
    }

    private async void ShowGamesWithoutCover_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateShowGamesSetting("ShowWithoutCover");
            UpdateMenuCheckMarks("ShowWithoutCover");
            await LoadGameFilesAsync(_currentFilter, SearchTextBox.Text.Trim());
        }
        catch (Exception ex)
        {
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
            if (sender is not MenuItem clickedItem) return;

            var sizeText = clickedItem.Name.Replace("Size", "");

            if (!int.TryParse(new string(sizeText.Where(char.IsDigit).ToArray()), out var newSize)) return;

            _gameButtonFactory.ImageHeight = newSize; // Update the image height
            _settings.ThumbnailSize = newSize;
            _settings.Save();

            UpdateThumbnailSizeCheckMarks(newSize);

            // Reload List of Games
            await LoadGameFilesAsync();
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

    private async void ButtonAspectRatio_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var aspectRatio = clickedItem.Name;
            _settings.ButtonAspectRatio = aspectRatio;
            _settings.Save();

            UpdateButtonAspectRatioCheckMarks(aspectRatio);

            // Reload List of Games
            await LoadGameFilesAsync();
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

    private async void GamesPerPage_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem clickedItem) return;

            var pageText = clickedItem.Name.Replace("Page", "");
            if (!int.TryParse(new string(pageText.Where(char.IsDigit).ToArray()), out var newPage)) return;

            _filesPerPage = newPage;
            _paginationThreshold = newPage;
            _settings.GamesPerPage = newPage;

            _settings.Save();
            UpdateNumberOfGamesPerPageCheckMarks(newPage);

            // Refresh GameList
            await LoadGameFilesAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in the method GamesPerPage_Click.");
        }
    }

    private void ShowGlobalSearchWindow_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();

        var globalSearchWindow =
            new GlobalSearchWindow(_systemConfigs, _machines, _mameLookup, _settings, _favoritesManager, this);
        globalSearchWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
    }

    private void ShowGlobalStatsWindow_Click(object sender, RoutedEventArgs e)
    {
        var globalStatsWindow = new GlobalStatsWindow(_systemConfigs);
        globalStatsWindow.Show();
    }

    private void ShowFavoritesWindow_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();

        var favoritesWindow = new FavoritesWindow(_settings, _systemConfigs, _machines, _favoritesManager, this);
        favoritesWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
    }

    private void ShowPlayHistoryWindow_Click(object sender, RoutedEventArgs e)
    {
        ResetUi();

        var playHistoryWindow = new PlayTimeWindow(_systemConfigs, _machines, _settings, _favoritesManager,
            _playHistoryManager, this);
        playHistoryWindow.Show();

        _favoritesManager = FavoritesManager.LoadFavorites();
        _playHistoryManager = PlayHistoryManager.LoadPlayHistory();
    }

    private void UpdateThumbnailSizeCheckMarks(int selectedSize)
    {
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

    private async void ChangeViewMode_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Equals(sender, GridView))
            {
                GridView.IsChecked = true;
                ListView.IsChecked = false;
                _settings.ViewMode = "GridView";

                GameFileGrid.Visibility = Visibility.Visible;
                ListViewPreviewArea.Visibility = Visibility.Collapsed;

                // Ensure pagination is reset at the beginning
                ResetPaginationButtons();

                // Clear SearchTextBox
                SearchTextBox.Text = "";

                // Update current filter
                _currentFilter = null;

                // Empty SystemComboBox
                _selectedSystem = null;
                SystemComboBox.SelectedItem = null;
                var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ??
                                       "No system selected";
                SelectedSystem = nosystemselected;
                PlayTime = "00:00:00";

                AddNoSystemMessage();
            }
            else if (Equals(sender, ListView))
            {
                GridView.IsChecked = false;
                ListView.IsChecked = true;
                _settings.ViewMode = "ListView";

                GameFileGrid.Visibility = Visibility.Collapsed;
                ListViewPreviewArea.Visibility = Visibility.Visible;

                // Ensure pagination is reset at the beginning
                ResetPaginationButtons();

                // Clear SearchTextBox
                SearchTextBox.Text = "";

                // Update current filter
                _currentFilter = null;

                // Empty SystemComboBox
                _selectedSystem = null;
                PreviewImage.Source = null;
                SystemComboBox.SelectedItem = null;

                // Set selected system
                var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ??
                                       "No system selected";
                SelectedSystem = nosystemselected;
                PlayTime = "00:00:00";

                AddNoSystemMessage();

                await LoadGameFilesAsync();
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

        // Update checked status
        SetLanguageAndCheckMenu(selectedLanguage);

        SaveApplicationSettings();

        QuitApplication.RestartApplication();
    }
}
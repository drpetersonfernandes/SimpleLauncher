﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ControlzEx.Theming;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.UiHelpers;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;

namespace SimpleLauncher;

public partial class MainWindow : INotifyPropertyChanged, IDisposable
{
    private bool _isUiUpdating;

    // Declare Controller Detection
    private DispatcherTimer _controllerCheckTimer;

    // Declare GameListItems
    // Used in ListView Mode
    public ObservableCollection<GameListViewItem> GameListItems { get; set; } = [];

    // Declare System Name and PlayTime in the Statusbar
    // _selectedSystem is the selected system from ComboBox
    public event PropertyChangedEventHandler PropertyChanged;
    private string _selectedSystem;
    private string _playTime;

    public string SelectedSystem
    {
        get => _selectedSystem;
        set
        {
            _selectedSystem = value;
            OnPropertyChanged(nameof(SelectedSystem));
        }
    }

    public string PlayTime
    {
        get => _playTime;
        set
        {
            _playTime = value;
            OnPropertyChanged(nameof(PlayTime));
        }
    }

    private void OnPropertyChanged(string propertyName) // Update UI on OnPropertyChanged
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Define Tray Icon
    private TrayIconManager _trayIconManager;

    // Define PlayHistory
    private PlayHistoryManager _playHistoryManager;

    // Define Pagination Related Variables
    private int _currentPage = 1;
    private int _filesPerPage;
    private int _totalFiles;
    private int _paginationThreshold;
    private Button _nextPageButton;
    private Button _prevPageButton;
    private string _currentFilter;

    // Define _currentSearchResults
    private List<string> _currentSearchResults = [];

    // Define and Instantiate variables
    private List<SystemManager> _systemManagers;
    private readonly FilterMenu _topLetterNumberMenu = new();
    private GameListFactory _gameListFactory;
    private readonly WrapPanel _gameFileGrid;
    private GameButtonFactory _gameButtonFactory;
    private readonly SettingsManager _settings;
    private FavoritesManager _favoritesManager;
    private readonly List<MameManager> _machines;
    private readonly Dictionary<string, string> _mameLookup;
    private string _selectedImageFolder;
    private List<string> _selectedRomFolders;

    // Define the LogPath
    private readonly string _logPath = GetLogPath.Path();

    private bool _isGameListLoading;
    private string _activeSearchQueryOrMode;

    public MainWindow()
    {
        InitializeComponent();

        // Initialize settings from App
        _settings = App.Settings;

        // DataContext set to the MainWindow instance
        DataContext = this;

        // Load and Apply _settings
        ToggleGamepad.IsChecked = _settings.EnableGamePadNavigation;
        UpdateThumbnailSizeCheckMarks(_settings.ThumbnailSize);
        UpdateButtonAspectRatioCheckMarks(_settings.ButtonAspectRatio);
        UpdateNumberOfGamesPerPageCheckMarks(_settings.GamesPerPage);
        UpdateShowGamesCheckMarks(_settings.ShowGames);
        _filesPerPage = _settings.GamesPerPage;
        _paginationThreshold = _settings.GamesPerPage;
        ToggleFuzzyMatching.IsChecked = _settings.EnableFuzzyMatching;

        // Load _machines and _mameLookup
        _machines = MameManager.LoadFromDat();
        _mameLookup = _machines
            .GroupBy(static m => m.MachineName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static g => g.Key, static g => g.First().Description, StringComparer.OrdinalIgnoreCase);

        LoadOrReloadSystemManager();

        // Initialize the GamePadController
        GamePadController.Instance2.ErrorLogger = (ex, msg) => { _ = LogErrors.LogErrorAsync(ex, msg); };
        if (_settings.EnableGamePadNavigation)
        {
            GamePadController.Instance2.Start();
        }
        else
        {
            GamePadController.Instance2.Stop();
        }

        // Add _topLetterNumberMenu to the UI
        LetterNumberMenu.Children.Clear();
        LetterNumberMenu.Children.Add(_topLetterNumberMenu.LetterPanel);

        // Create and integrate FilterMenu
        _topLetterNumberMenu.OnLetterSelected += async selectedLetter =>
        {
            await TopLetterNumberMenu_Click(selectedLetter);
        };

        // Initialize _favoritesManager
        _favoritesManager = FavoritesManager.LoadFavorites();

        // Initialize _gameFileGrid
        _gameFileGrid = FindName("GameFileGrid") as WrapPanel;
        if (_gameFileGrid == null)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(new Exception("GameFileGrid not found"), "GameFileGrid not found");
        }

        // Initialize _gameButtonFactory
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, _gameFileGrid, this);

        // Initialize _gameListFactory
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, _playHistoryManager, this);

        // Check for Updates
        Loaded += async (_, _) => await UpdateChecker.SilentCheckForUpdatesAsync(this);

        // Call Stats API
        Loaded += static (_, _) =>
        {
            _ = Stats.CallApiAsync();
        };

        // Attach the Load and Close events
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;

        Loaded += async (_, _) =>
        {
            await DisplaySystemSelectionScreenAsync();
        };
    }

    private (string startLetter, string searchQuery) GetLoadGameFilesParams()
    {
        var searchQueryToUse = _activeSearchQueryOrMode;
        var startLetterToUse = string.IsNullOrEmpty(searchQueryToUse) ? _currentFilter : null;
        return (startLetterToUse, searchQueryToUse);
    }

    private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Check if the Ctrl key is pressed
        if (Keyboard.Modifiers != ModifierKeys.Control) return;

        switch (e.Delta)
        {
            case > 0:
                // Scroll up, trigger zoom in
                NavZoomInButton_Click(null, null); // Pass null for sender and EventArgs
                break;
            case < 0:
                // Scroll down, trigger zoom out
                NavZoomOutButton_Click(null, null); // Pass null for sender and EventArgs
                break;
        }

        // Mark the event as handled to prevent scrolling the ScrollViewer
        e.Handled = true;
    }

    private async Task TopLetterNumberMenu_Click(string selectedLetter)
    {
        if (_isGameListLoading) return;

        try
        {
            PlaySoundEffects.PlayNotificationSound();

            ResetPaginationButtons(); // Ensure pagination is reset at the beginning
            SearchTextBox.Text = ""; // Clear SearchTextBox
            _currentFilter = selectedLetter; // Update current filter
            _activeSearchQueryOrMode = null; // Reset special search mode

            await LoadGameFilesAsync(selectedLetter, null); // searchQuery is null
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in TopLetterNumberMenu_Click.");
        }
    }

    private async Task ShowSystemFavoriteGames_Click()
    {
        if (_isGameListLoading) return;

        try
        {
            PlaySoundEffects.PlayNotificationSound();

            // Change the filter to ShowAll (as favorites might not have covers)
            _settings.ShowGames = "ShowAll";
            _settings.Save();
            ApplyShowGamesSetting(); // Update menu check marks

            ResetPaginationButtons();
            SearchTextBox.Text = ""; // Clear search field
            _currentFilter = null; // Clear any active letter filter
            _activeSearchQueryOrMode = "FAVORITES"; // Set special search mode

            // Filter favorites for the selected system and store them in _currentSearchResults
            var favoriteGames = GetFavoriteGamesForSelectedSystem();
            if (favoriteGames.Count != 0)
            {
                _currentSearchResults = favoriteGames.ToList(); // Store only favorite games in _currentSearchResults

                await LoadGameFilesAsync(null, "FAVORITES"); // Call LoadGameFilesAsync
            }
            else
            {
                // Notify user
                AddNoFilesMessage();
                MessageBoxLibrary.NoFavoriteFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in ShowSystemFavoriteGames_Click.");
        }
    }

    private async Task ShowSystemFeelingLucky_Click(object sender, RoutedEventArgs e)
    {
        if (_isGameListLoading) return;

        try
        {
            PlaySoundEffects.PlayNotificationSound();

            // Change the filter to ShowAll (as random might not have covers)
            _settings.ShowGames = "ShowAll";
            _settings.Save();
            ApplyShowGamesSetting(); // Update menu check marks

            // Check if a system is selected
            if (SystemComboBox.SelectedItem == null)
            {
                // Notify user
                MessageBoxLibrary.PleaseSelectASystemBeforeMessageBox();

                return;
            }

            var selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedConfig = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedConfig == null)
            {
                return;
            }

            var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var folder in selectedConfig.SystemFolders)
            {
                var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) ||
                    selectedConfig.FileFormatsToSearch == null) continue;

                var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedConfig.FileFormatsToSearch);
                foreach (var file in filesInFolder)
                {
                    uniqueFiles.TryAdd(Path.GetFileName(file), file);
                }
            }

            var gameFiles = uniqueFiles.Values.ToList();

            // Check if we have any games after filtering
            if (gameFiles.Count == 0)
            {
                // Notify user
                MessageBoxLibrary.NoGameFoundInTheRandomSelectionMessageBox();

                return;
            }

            // Randomly select a game
            var random = new Random();
            var randomIndex = random.Next(0, gameFiles.Count);
            var selectedGame = gameFiles[randomIndex];

            // Reset letter selection in the UI and current search
            _topLetterNumberMenu.DeselectLetter();
            SearchTextBox.Text = "";
            _currentFilter = null; // Clear any active letter filter
            _activeSearchQueryOrMode = "RANDOM_SELECTION"; // Set special search mode
            _currentSearchResults = [selectedGame]; // Store only the selected game

            await LoadGameFilesAsync(null, "RANDOM_SELECTION");

            // If in list view, select the game in the DataGrid
            if (_settings.ViewMode != "ListView" || GameDataGrid.Items.Count <= 0) return;

            GameDataGrid.SelectedIndex = 0;
            GameDataGrid.ScrollIntoView(GameDataGrid.SelectedItem);
            GameDataGrid.Focus();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the Feeling Lucky feature.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    public void RefreshGameListAfterPlay(string fileName, string systemName)
    {
        try
        {
            // Only update if in ListView mode
            if (_settings.ViewMode != "ListView" || GameListItems.Count == 0)
                return;

            // Re-load the latest play history data
            _playHistoryManager = PlayHistoryManager.LoadPlayHistory();

            // Get the current playtime from history
            var historyItem = _playHistoryManager.PlayHistoryList
                .FirstOrDefault(h => h.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                                     h.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (historyItem == null)
            {
                return;
            }

            // Find and update the specific item
            var gameItem = GameListItems.FirstOrDefault(item =>
                // ReSharper disable once PossibleNullReferenceException
                Path.GetFileName(item.FilePath).Equals(fileName, StringComparison.OrdinalIgnoreCase));

            if (gameItem != null)
            {
                // Update in the UI thread to ensure UI refreshes
                Dispatcher.Invoke(() =>
                {
                    // Update playtime
                    var timeSpan = TimeSpan.FromSeconds(historyItem.TotalPlayTime);
                    gameItem.PlayTime = timeSpan.TotalHours >= 1
                        ? $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
                        : $"{timeSpan.Minutes}m {timeSpan.Seconds}s";

                    // Update times played
                    gameItem.TimesPlayed = historyItem.TimesPlayed.ToString(CultureInfo.InvariantCulture);

                    // Force refresh of DataGrid
                    GameDataGrid.Items.Refresh();
                });
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error refreshing game list play time";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private void InitializeControllerDetection()
    {
        _controllerCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5) // Check every 5 seconds
        };
        _controllerCheckTimer.Tick += GamePadControllerCheckTimer_Tick;
        _controllerCheckTimer.Start();
    }

    private static void GamePadControllerCheckTimer_Tick(object sender, EventArgs e)
    {
        GamePadController.Instance2.CheckAndReconnectControllers();
    }

    private static void OpenUpdateHistory()
    {
        var updateHistoryWindow = new UpdateHistoryWindow();
        updateHistoryWindow.Show();
    }

    private void SaveApplicationSettings()
    {
        // Save application's window state
        _settings.MainWindowWidth = (int)Width;
        _settings.MainWindowHeight = (int)Height;
        _settings.MainWindowTop = (int)Top;
        _settings.MainWindowLeft = (int)Left;
        _settings.MainWindowState = WindowState.ToString();

        // Save application's current state
        _settings.ThumbnailSize = _gameButtonFactory.ImageHeight;
        _settings.GamesPerPage = _filesPerPage;
        _settings.ShowGames = _settings.ShowGames;
        _settings.EnableGamePadNavigation = ToggleGamepad.IsChecked;
        _settings.EnableFuzzyMatching = ToggleFuzzyMatching.IsChecked; // Save fuzzy matching state

        // Save theme settings
        var detectedTheme = ThemeManager.Current.DetectTheme(this);
        if (detectedTheme != null)
        {
            _settings.BaseTheme = detectedTheme.BaseColorScheme;
            _settings.AccentColor = detectedTheme.ColorScheme;
        }

        _settings.Save();
    }

    private List<string> GetFavoriteGamesForSelectedSystem()
    {
        // Reload favorites to ensure we have the latest data
        _favoritesManager = FavoritesManager.LoadFavorites();

        var selectedSystem = SystemComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedSystem))
        {
            return []; // Return an empty list if there is no favorite for that system
        }

        // Retrieve the system configuration for the selected system
        var selectedConfig = _systemManagers.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
        if (selectedConfig == null)
        {
            return []; // Return an empty list if there is no favorite for that system
        }

        // Filter the favorites and build the full file path for each favorite game
        var favoriteGamePaths = _favoritesManager.FavoriteList
            .Where(fav => fav.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase))
            .Select(fav => PathHelper.FindFileInSystemFolders(selectedConfig, fav.FileName))
            .Where(static path => !string.IsNullOrEmpty(path))
            .ToList();

        return favoriteGamePaths;
    }

    private static Task ShowPleaseWaitWindowAsync(Window window)
    {
        return Task.Run(() =>
        {
            window.Dispatcher.Invoke(window.Show);
        });
    }

    private static Task ClosePleaseWaitWindowAsync(Window window)
    {
        return Task.Run(() =>
        {
            window.Dispatcher.Invoke(window.Close);
        });
    }

    private void GameListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GameDataGrid.SelectedItem is not GameListViewItem selectedItem) return;

        var gameListViewFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines, _settings, _favoritesManager, _playHistoryManager, this);
        gameListViewFactory.HandleSelectionChanged(selectedItem);
    }

    private async void GameListDoubleClickOnSelectedItem(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (GameDataGrid.SelectedItem is GameListViewItem selectedItem)
            {
                // Delegate the double-click handling to GameListFactory
                await _gameListFactory.HandleDoubleClick(selectedItem);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error while using the method GameListDoubleClickOnSelectedItem.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }

    private async void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (_isUiUpdating) return; // Prevent re-entrance

            _isUiUpdating = true;
            try
            {
                SearchTextBox.Text = "";
                EmulatorComboBox.ItemsSource = null;
                EmulatorComboBox.SelectedIndex = -1;
                PreviewImage.Source = null;

                _currentSearchResults.Clear();
                _currentFilter = null;
                _activeSearchQueryOrMode = null;

                GameFileGrid.Visibility = Visibility.Visible;
                ListViewPreviewArea.Visibility = Visibility.Collapsed;

                if (SystemComboBox.SelectedItem == null)
                {
                    return;
                }

                var selectedSystem = SystemComboBox.SelectedItem?.ToString();
                var selectedConfig = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystem);

                if (selectedSystem == null || selectedConfig == null) // Combine null checks
                {
                    // Notify developer
                    const string errorMessage = "Selected system or its configuration is null.";
                    _ = LogErrors.LogErrorAsync(null, errorMessage);

                    // Notify user
                    MessageBoxLibrary.InvalidSystemConfigMessageBox();

                    await DisplaySystemSelectionScreenAsync();
                    return;
                }

                EmulatorComboBox.ItemsSource = selectedConfig.Emulators.Select(static emulator => emulator.EmulatorName).ToList();
                if (EmulatorComboBox.Items.Count > 0)
                {
                    EmulatorComboBox.SelectedIndex = 0;
                }

                SelectedSystem = selectedSystem;

                var systemPlayTime = _settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystem);
                PlayTime = systemPlayTime != null ? systemPlayTime.PlayTime : "00:00:00";

                var uniqueFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var folder in selectedConfig.SystemFolders)
                {
                    var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                    if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) ||
                        selectedConfig.FileFormatsToSearch == null) continue;

                    var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedConfig.FileFormatsToSearch);
                    foreach (var file in filesInFolder)
                    {
                        uniqueFileNames.Add(Path.GetFileName(file));
                    }
                }

                var gameCount = uniqueFileNames.Count;

                // Display SystemInfo for that system (pass the raw string for display, resolved for logic within DisplaySystemInfo)
                await DisplaySystemInformation.DisplaySystemInfo(selectedConfig.PrimarySystemFolder, gameCount, selectedConfig, _gameFileGrid);

                // Resolve the system image folder path using PathHelper
                var resolvedSystemImageFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedConfig.SystemImageFolder);

                _selectedRomFolders = selectedConfig.SystemFolders.Select(PathHelper.ResolveRelativeToAppDirectory).ToList();
                _selectedImageFolder = string.IsNullOrWhiteSpace(resolvedSystemImageFolderPath)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedConfig.SystemName) // Use the default resolved path
                    : resolvedSystemImageFolderPath; // Use resolved configured path

                _topLetterNumberMenu.DeselectLetter();
                ResetPaginationButtons();
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Error in the method SystemComboBox_SelectionChanged.";
                _ = LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.InvalidSystemConfigMessageBox();
            }
            finally
            {
                _isUiUpdating = false;
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in SystemComboBox_SelectionChanged.");
        }
    }

    private void AddNoFilesMessage()
    {
        var noGamesMatched = (string)Application.Current.TryFindResource("nogamesmatched") ?? "Unfortunately, no games matched your search query or the selected button.";

        // Check the current view mode
        if (_settings.ViewMode == "GridView")
        {
            // Clear existing content in Grid view and add the message
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\n{noGamesMatched}",
                Padding = new Thickness(10)
            });
        }
        else
        {
            // For List view, clear GameListItems
            GameListItems.Clear();
            GameListItems.Add(new GameListViewItem
            {
                FileName = noGamesMatched,
                MachineDescription = string.Empty
            });
        }

        // Deselect any selected letter when no system is selected
        _topLetterNumberMenu.DeselectLetter();
    }

    public void SetGameButtonsEnabled(bool isEnabled)
    {
        if (_gameFileGrid == null) return;

        foreach (var child in _gameFileGrid.Children)
        {
            if (child is Button button)
            {
                button.IsEnabled = isEnabled;
            }
        }
    }

    public async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null)
    {
        Dispatcher.Invoke(() => SetUiLoadingState(true));
        await SetUiBeforeLoadGameFilesAsync();

        try
        {
            if (SystemComboBox.SelectedItem == null)
            {
                await DisplaySystemSelectionScreenAsync();
                return;
            }

            var selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedConfig is null.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.InvalidSystemConfigMessageBox();

                await DisplaySystemSelectionScreenAsync();

                return;
            }

            List<string> allFiles;

            switch (searchQuery)
            {
                case "FAVORITES" when _currentSearchResults != null && _currentSearchResults.Count != 0:
                case "RANDOM_SELECTION" when _currentSearchResults != null && _currentSearchResults.Count != 0:
                    allFiles = new List<string>(_currentSearchResults);
                    break;
                default:
                {
                    var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var folder in selectedManager.SystemFolders)
                    {
                        var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                        if (string.IsNullOrEmpty(resolvedSystemFolderPath) ||
                            !Directory.Exists(resolvedSystemFolderPath)) continue;

                        var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch);
                        foreach (var file in filesInFolder)
                        {
                            uniqueFiles.TryAdd(Path.GetFileName(file), file);
                        }
                    }

                    allFiles = uniqueFiles.Values.ToList();

                    if (!string.IsNullOrWhiteSpace(startLetter))
                    {
                        // Filter the list of resolved paths
                        allFiles = await FilterFilesAsync(allFiles, startLetter);
                    }

                    if (!string.IsNullOrWhiteSpace(searchQuery) && searchQuery != "RANDOM_SELECTION" && searchQuery != "FAVORITES")
                    {
                        var systemIsMame = selectedManager.SystemIsMame;
                        var lowerQuery = searchQuery.ToLowerInvariant();
                        allFiles = await Task.Run(() =>
                            allFiles.FindAll(file => // 'file' here is already a resolved absolute path
                            {
                                var fileName = Path.GetFileNameWithoutExtension(file);
                                var filenameMatch = fileName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                if (filenameMatch) return true;

                                if (systemIsMame && _mameLookup != null && _mameLookup.TryGetValue(fileName, out var description))
                                {
                                    return description.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                }

                                return false;
                            }));

                        _currentSearchResults = new List<string>(allFiles);
                    }
                    // If no search query and no start letter, allFiles is already the full cached list for the system.
                    // _currentSearchResults remain empty in this case.

                    break;
                }
            }

            allFiles = allFiles.OrderBy(static f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase).ToList();

            allFiles = await FilterFilesByShowGamesSettingAsync(allFiles, selectedSystem, selectedManager);

            allFiles = SetPaginationOfListOfFiles(allFiles);

            _favoritesManager = FavoritesManager.LoadFavorites();

            // GameButtonFactory and GameListFactory now use the resolved file paths directly
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines,
                _settings, _favoritesManager, _gameFileGrid, this);

            _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemManagers, _machines,
                _settings, _favoritesManager, _playHistoryManager, this);

            foreach (var filePath in allFiles) // 'filePath' is already resolved here
            {
                if (_settings.ViewMode == "GridView")
                {
                    var gameButton =
                        await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedManager);
                    GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Add(gameButton));
                }
                else // ListView
                {
                    var gameListViewItem =
                        await _gameListFactory.CreateGameListViewItemAsync(filePath, selectedSystem, selectedManager);
                    await Dispatcher.InvokeAsync(() => GameListItems.Add(gameListViewItem));
                }
            }

            switch (_settings.ViewMode)
            {
                case "GridView":
                    Scroller.Focus();
                    break;
                case "ListView":
                    GameDataGrid.Focus();
                    break;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method LoadGameFilesAsync.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMethodLoadGameFilesAsyncMessageBox();
        }
        finally
        {
            Dispatcher.Invoke(() => SetUiLoadingState(false));
        }
    }

    private List<string> SetPaginationOfListOfFiles(List<string> allFiles)
    {
        // Count the collection of files (this should be the total before pagination)
        // If allFiles is already paginated, _totalFiles needs to be set from the unpaginated list.
        // _totalFiles should be set based on the count of files *before* pagination.
        // For FAV/RANDOM/Search, _currentSearchResults holds the full list.
        // For letter/all, the 'allFiles' passed here (before this method's Skip/Take) is the full list for that filter.
        _totalFiles = allFiles.Count;


        // Calculate the indices of files displayed on the current page
        var startIndex = (_currentPage - 1) * _filesPerPage + 1; // +1 because we are dealing with a 1-based index for displaying
        var endIndex = Math.Min(startIndex + _filesPerPage - 1, _totalFiles); // Actual number of files loaded on this page

        // Pagination related
        if (_totalFiles > _paginationThreshold)
        {
            // Enable pagination and adjust file list based on the current page
            allFiles = allFiles.Skip((_currentPage - 1) * _filesPerPage).Take(_filesPerPage).ToList();

            // Update or create pagination controls
            UpdatePaginationButtons();
        }
        else
        {
            // If total files are not enough for pagination, ensure buttons are disabled.
            _prevPageButton.IsEnabled = false;
            _nextPageButton.IsEnabled = false;
        }


        // Display message if the number of files == 0 (after potential pagination, so check the paginated list)
        if (allFiles.Count == 0 && _totalFiles == 0) // Check if the original list was also empty
        {
            AddNoFilesMessage();
        }

        // Update the UI to reflect the current pagination status and the indices of files being displayed
        var displayingfiles0To = (string)Application.Current.TryFindResource("Displayingfiles0to") ?? "Displaying files 0 to";
        var outOf = (string)Application.Current.TryFindResource("outof") ?? "out of";
        var total = (string)Application.Current.TryFindResource("total") ?? "total";
        var displayingfiles = (string)Application.Current.TryFindResource("Displayingfiles") ?? "Displaying files";
        var to = (string)Application.Current.TryFindResource("to") ?? "to";

        TotalFilesLabel.Dispatcher.Invoke(() =>
            TotalFilesLabel.Content = _totalFiles == 0 ? $"{displayingfiles0To} 0 {outOf} 0 {total}" : $"{displayingfiles} {(_totalFiles > 0 ? startIndex : 0)} {to} {endIndex} {outOf} {_totalFiles} {total}"
        );
        return allFiles;
    }

    private async Task SetUiBeforeLoadGameFilesAsync()
    {
        // Move scroller to top
        Scroller.Dispatcher.Invoke(() => Scroller.ScrollToTop());

        // Clear PreviewImage
        PreviewImage.Source = null;

        // Clear Game Grid
        GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Clear());

        // Clear the Game List
        await Dispatcher.InvokeAsync(() => GameListItems.Clear());

        // Set ViewMode based on user preference
        if (_settings.ViewMode == "GridView")
        {
            // Allow GridView
            GameFileGrid.Visibility = Visibility.Visible;
            ListViewPreviewArea.Visibility = Visibility.Collapsed;
        }
        else
        {
            // Allow ListView
            GameFileGrid.Visibility = Visibility.Collapsed;
            ListViewPreviewArea.Visibility = Visibility.Visible;
        }
    }

    private async Task<List<string>> FilterFilesByShowGamesSettingAsync(List<string> files, string selectedSystem, SystemManager selectedConfig)
    {
        if (files.Count == 0 || _settings.ShowGames == "ShowAll")
            return files;

        var filteredFiles = new List<string>();

        var filteringPleasewait = (string)Application.Current.TryFindResource("Filteringpleasewait") ?? "Filtering, please wait...";
        var pleaseWaitWindow = new PleaseWaitWindow(filteringPleasewait);

        try
        {
            await ShowPleaseWaitWindowAsync(pleaseWaitWindow);

            foreach (var filePath in files) // 'filePath' is already resolved here
            {
                var fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(filePath);

                var imagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, selectedConfig);

                bool isDefaultImage;
                if (string.IsNullOrEmpty(imagePath) || imagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase))
                {
                    isDefaultImage = true;
                }
                else
                {
                    // Resolve the found image path before checking existence
                    var resolvedImagePath = PathHelper.ResolveRelativeToAppDirectory(imagePath);
                    isDefaultImage = string.IsNullOrEmpty(resolvedImagePath) || !File.Exists(resolvedImagePath) || resolvedImagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase);
                }

                switch (_settings.ShowGames)
                {
                    case "ShowWithCover" when !isDefaultImage:
                    case "ShowWithoutCover" when isDefaultImage:
                        filteredFiles.Add(filePath);
                        break;
                }
            }

            return filteredFiles;
        }
        finally
        {
            await ClosePleaseWaitWindowAsync(pleaseWaitWindow);
        }
    }

    private void SetUiLoadingState(bool isLoading)
    {
        _isGameListLoading = isLoading;

        // Disable/Enable main interaction controls
        SystemComboBox.IsEnabled = !isLoading;
        EmulatorComboBox.IsEnabled = !isLoading;
        SearchTextBox.IsEnabled = !isLoading;
        SearchButton.IsEnabled = !isLoading;
        SelectedSystemFavoriteButton.IsEnabled = !isLoading;
        RandomLuckGameButton.IsEnabled = !isLoading;
        ToggleViewMode.IsEnabled = !isLoading;
        ToggleButtonAspectRatio.IsEnabled = !isLoading;
        ZoomInButton.IsEnabled = !isLoading;
        ZoomOutButton.IsEnabled = !isLoading;

        // Disable/Enable Letter/Number/Favorites/Lucky buttons via FilterMenu helper
        _topLetterNumberMenu.SetButtonsEnabled(!isLoading);

        // Disable/Enable pagination buttons (UpdatePaginationButtons already checks _isGameListLoading)
        UpdatePaginationButtons();
    }

    private static Task<List<string>> FilterFilesAsync(List<string> files, string startLetter)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(startLetter))
                return files;

            if (startLetter == "#")
            {
                return files.Where(static file => !string.IsNullOrEmpty(file) &&
                                                  file.Length > 0 &&
                                                  char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }

            return files.Where(file => !string.IsNullOrEmpty(file) &&
                                       Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
        });
    }
}

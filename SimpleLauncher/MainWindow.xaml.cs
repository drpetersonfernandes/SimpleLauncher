using System;
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
    // Declare Controller Detection
    private DispatcherTimer _controllerCheckTimer;

    // Declare CacheManager and CacheFiles
    private readonly CacheManager _cacheManager = new();
    private List<string> _cachedFiles;

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
    private List<SystemManager> _systemConfigs;
    private readonly FilterMenu _topLetterNumberMenu = new();
    private GameListFactory _gameListFactory;
    private readonly WrapPanel _gameFileGrid;
    private GameButtonFactory _gameButtonFactory;
    private readonly SettingsManager _settings;
    private FavoritesManager _favoritesManager;
    private readonly List<MameManager> _machines;
    private readonly Dictionary<string, string> _mameLookup; // Used for faster lookup of MAME machine names
    private string _selectedImageFolder;
    private string _selectedRomFolder;

    // Define the LogPath
    private readonly string _logPath = GetLogPath.Path();

    private bool _isGameListLoading;
    private string _activeSearchQueryOrMode;

    public MainWindow()
    {
        InitializeComponent();

        // Initialize settings from App
        _settings = App.Settings;

        // Check for Command-line Args
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("whatsnew"))
        {
            Loaded += static (_, _) => OpenUpdateHistory();
        }

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

        LoadOrReloadSystemConfig();

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
        _topLetterNumberMenu.OnFavoritesSelected += async () =>
        {
            await ShowSystemFavoriteGames_Click();
        };
        _topLetterNumberMenu.OnFeelingLuckySelected += async () =>
        {
            await ShowSystemFeelingLucky_Click(null, null);
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
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _gameFileGrid, this);

        // Initialize _gameListFactory
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _playHistoryManager, this);

        // Check for Updates
        Loaded += async (_, _) => await UpdateChecker.CheckForUpdatesAsync(this);

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
            PlayClick.PlayNotificationSound();

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
            PlayClick.PlayNotificationSound();

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
            PlayClick.PlayNotificationSound();

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
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedConfig == null)
            {
                return;
            }

            // Determine which game list to use
            List<string> gameFiles;

            // Otherwise, use the cached files for the selected system
            if (_cachedFiles is { Count: > 0 })
            {
                gameFiles = _cachedFiles;
            }
            // If needed, rescan the system folder
            else
            {
                var systemFolderPath = selectedConfig.SystemFolder;
                var fileExtensions = selectedConfig.FileFormatsToSearch;
                gameFiles = await GetFilePaths.GetFilesAsync(systemFolderPath, fileExtensions);
            }

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
        var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
        if (selectedConfig == null)
        {
            return []; // Return an empty list if there is no favorite for that system
        }

        var systemFolderPath = selectedConfig.SystemFolder;

        // Filter the favorites and build the full file path for each favorite game
        var favoriteGamePaths = _favoritesManager.FavoriteList
            .Where(fav => fav.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase))
            .Select(fav => Path.Combine(systemFolderPath, fav.FileName))
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

        var gameListViewFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _playHistoryManager, this);
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
            SearchTextBox.Text = ""; // Empty search field
            EmulatorComboBox.ItemsSource = null; // Null selected emulator
            EmulatorComboBox.SelectedIndex = -1; // No emulator selected
            PreviewImage.Source = null; // Empty PreviewImage

            // Clear search results and active filters
            _currentSearchResults.Clear();
            _currentFilter = null;
            _activeSearchQueryOrMode = null;

            // Hide ListView
            GameFileGrid.Visibility = Visibility.Visible;
            ListViewPreviewArea.Visibility = Visibility.Collapsed;

            if (SystemComboBox.SelectedItem == null)
            {
                return;
            }

            var selectedSystem = SystemComboBox.SelectedItem?.ToString();
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedSystem == null)
            {
                // Notify developer
                const string errorMessage = "selectedSystem is null.";
                var ex = new Exception(errorMessage);
                _ = LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.InvalidSystemConfigMessageBox();

                await DisplaySystemSelectionScreenAsync();

                return;
            }

            if (selectedConfig == null)
            {
                // Notify developer
                const string errorMessage = "selectedConfig is null.";
                var ex = new Exception(errorMessage);
                _ = LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.InvalidSystemConfigMessageBox();

                await DisplaySystemSelectionScreenAsync();

                return;
            }

            // Populate EmulatorComboBox
            EmulatorComboBox.ItemsSource = selectedConfig.Emulators.Select(static emulator => emulator.EmulatorName).ToList();

            // Select the first emulator
            if (EmulatorComboBox.Items.Count > 0)
            {
                EmulatorComboBox.SelectedIndex = 0;
            }

            // Update the selected system property
            SelectedSystem = selectedSystem;

            // Retrieve the playtime for the selected system
            var systemPlayTime = _settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystem);
            PlayTime = systemPlayTime != null ? systemPlayTime.PlayTime : "00:00:00";

            // Count files for that system
            var systemFolderPath = selectedConfig.SystemFolder;
            var fileExtensions = selectedConfig.FileFormatsToSearch; // Pass just the extensions to CountFiles
            var gameCount = CountFiles.CountFilesAsync(systemFolderPath, fileExtensions);

            // Display SystemInfo for that system
            await DisplaySystemInformation.DisplaySystemInfo(systemFolderPath, await gameCount, selectedConfig, _gameFileGrid);

            // Update Image Folder and Rom Folder Variables
            _selectedRomFolder = selectedConfig.SystemFolder;
            _selectedImageFolder = string.IsNullOrWhiteSpace(selectedConfig.SystemImageFolder)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedConfig.SystemName)
                : selectedConfig.SystemImageFolder;

            // Call DeselectLetter to clear any selected letter
            _topLetterNumberMenu.DeselectLetter();

            // Reset pagination
            ResetPaginationButtons();

            // Load files from cache or rescan if needed
            // Pass just the extensions to LoadSystemFilesAsync
            _cachedFiles = await _cacheManager.LoadSystemFilesAsync(selectedSystem, systemFolderPath, fileExtensions, await gameCount);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method SystemComboBox_SelectionChanged.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.InvalidSystemConfigMessageBox();
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
            var selectedManager = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.InvalidSystemConfigMessageBox();

                await DisplaySystemSelectionScreenAsync();
                return;
            }

            // Create allFiles
            List<string> allFiles;

            switch (searchQuery)
            {
                // If we are in "FAVORITES" mode, use '_currentSearchResults'
                case "FAVORITES" when _currentSearchResults != null && _currentSearchResults.Count != 0:
                // If we are in "RANDOM_SELECTION" mode, use '_currentSearchResults'
                case "RANDOM_SELECTION" when _currentSearchResults != null && _currentSearchResults.Count != 0:
                    allFiles = new List<string>(_currentSearchResults); // Use a copy for manipulation
                    break;
                // Regular behavior: load files based on startLetter or searchQuery
                default:
                {
                    allFiles = await TryToUseCachedListOfFiles(selectedSystem, selectedManager);

                    // Filter by TopMenu Letter if specified
                    if (!string.IsNullOrWhiteSpace(startLetter))
                    {
                        allFiles = await GetFilePaths.FilterFilesAsync(allFiles, startLetter);
                    }

                    // Process search query (from SearchBox)
                    if (!string.IsNullOrWhiteSpace(searchQuery))
                    {
                        // If _currentSearchResults already exists from a previous identical text search, use it to avoid re-filtering.
                        // This check is subtle. If _activeSearchQueryOrMode matches searchQuery, and it's a text search,
                        // _currentSearchResults should already hold the full unpaginated list.
                        // However, LoadGameFilesAsync is also responsible for populating _currentSearchResults for new text searches.
                        // The existing logic:
                        // if (_currentSearchResults != null && _currentSearchResults.Count != 0 &&
                        //     searchQuery != "RANDOM_SELECTION" &&
                        //     searchQuery != "FAVORITES")
                        // {
                        //     allFiles = _currentSearchResults; // This assumes _currentSearchResults is for THIS searchQuery
                        // }
                        // This part might be redundant if ExecuteSearch clears _currentSearchResults before calling.
                        // Let's assume _currentSearchResults is either pre-filled for FAV/RANDOM, or needs to be filled for text search.

                        // Perform the search if it's a text search (not FAV/RANDOM)
                        if (searchQuery != "RANDOM_SELECTION" && searchQuery != "FAVORITES")
                        {
                            var systemIsMame = selectedManager.SystemIsMame;
                            if (systemIsMame && _mameLookup != null)
                            {
                                var lowerQuery = searchQuery.ToLowerInvariant();
                                allFiles = await Task.Run(() =>
                                    allFiles.FindAll(file =>
                                    {
                                        var fileName = Path.GetFileNameWithoutExtension(file);
                                        var filenameMatch = fileName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase); // Check if the filename contains the search query.
                                        if (filenameMatch) return true;

                                        if (_mameLookup.TryGetValue(fileName, out var description)) // Lookup in the dictionary.
                                        {
                                            return description.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                        }

                                        return false;
                                    }));
                            }
                            else
                            {
                                // For non-MAME systems, use the original filtering by filename.
                                allFiles = await Task.Run(() =>
                                    allFiles.FindAll(file =>
                                    {
                                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                                        return fileNameWithoutExtension.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
                                    }));
                            }

                            // Store the full results of this text search in _currentSearchResults
                            _currentSearchResults = new List<string>(allFiles);
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(startLetter)) // Neither search nor letter filter
                    {
                        // If no search query and no start letter, _currentSearchResults should be empty
                        // unless explicitly set by favorites/random (handled by switch cases above).
                        // For "All" games (no filter, no search), _currentSearchResults should be cleared
                        // if it held previous search results.
                        // This is typically handled by the calling context (e.g., TopLetterNumberMenu_Click("All") clears SearchTextBox and _activeSearchQueryOrMode).
                        // If LoadGameFilesAsync(null,null) is called, it means "All" for the system.
                        // _currentSearchResults should not interfere here.
                        // The _currentSearchResults is cleared in ExecuteSearch, ResetUI, SystemComboBox_SelectionChanged.
                        // For letter filters, _currentSearchResults is not touched by LoadGameFilesAsync.
                    }

                    break;
                }
            }

            // Sort the collection of files
            allFiles.Sort();

            // Apply ShowGames filter before pagination
            allFiles = await FilterFilesByShowGamesSettingAsync(allFiles, selectedSystem, selectedManager);

            allFiles = SetPaginationOfListOfFiles(allFiles); // This paginates the 'allFiles' list

            // Reload the FavoritesConfig
            _favoritesManager = FavoritesManager.LoadFavorites();

            // Initialize GameButtonFactory with updated FavoritesConfig
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines,
                _settings, _favoritesManager, _gameFileGrid, this);

            // Initialize GameListFactory with updated FavoritesConfig
            _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines,
                _settings, _favoritesManager, _playHistoryManager, this);

            // Display files based on ViewMode
            foreach (var filePath in allFiles) // 'allFiles' is now the paginated list
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

            // Set focus
            switch (_settings.ViewMode)
            {
                // Set focus to the ScrollViewer
                case "GridView":
                    Scroller.Focus();
                    break;
                // Set focus to the GameDataGrid
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

    private async Task<List<string>> TryToUseCachedListOfFiles(string selectedSystem, SystemManager selectedManager)
    {
        List<string> allFiles;
        // Attempt to use the cached file list first
        _cachedFiles = _cacheManager.GetCachedFiles(selectedSystem);

        // Recount the number of files in the system folder
        var systemFolderPath = selectedManager.SystemFolder;
        var fileExtensions = selectedManager.FileFormatsToSearch; // Pass just the extensions to CountFiles
        var gameCount = CountFiles.CountFilesAsync(systemFolderPath, fileExtensions);
        var cachedFilesCount = _cachedFiles?.Count ?? 0;

        // Check the total number of games
        if (cachedFilesCount != await gameCount)
        {
            // If the cached file list is not up to date, rescan the system folder
            // Pass just the extensions to LoadSystemFilesAsync
            _cachedFiles = await _cacheManager.LoadSystemFilesAsync(selectedSystem, systemFolderPath, fileExtensions, await gameCount);
        }

        if (_cachedFiles is { Count: > 0 })
        {
            allFiles = new List<string>(_cachedFiles); // Return a copy
        }
        else
        {
            // Fall back to scanning the folder if no cache is available
            allFiles = await GetFilePaths.GetFilesAsync(systemFolderPath, fileExtensions); // Pass just the extensions to GetFilesAsync
        }

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
        // If there are no files or showing all, no filtering needed
        if (files.Count == 0 || _settings.ShowGames == "ShowAll")
            return files;

        var filteredFiles = new List<string>();

        // Create a pleaseWaitWindow for longer operations
        var filteringPleasewait = (string)Application.Current.TryFindResource("Filteringpleasewait") ?? "Filtering, please wait...";
        var pleaseWaitWindow = new PleaseWaitWindow(filteringPleasewait);

        try
        {
            await ShowPleaseWaitWindowAsync(pleaseWaitWindow);

            foreach (var filePath in files)
            {
                var fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(filePath);

                // Find the image path for this file
                var imagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, selectedConfig);

                // Check if the image is named "default.png"
                bool isDefaultImage;

                if (string.IsNullOrEmpty(imagePath) || imagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase))
                {
                    isDefaultImage = true;
                }
                else
                {
                    isDefaultImage = false;
                }

                switch (_settings.ShowGames)
                {
                    // Filter based on the showGames setting
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
}
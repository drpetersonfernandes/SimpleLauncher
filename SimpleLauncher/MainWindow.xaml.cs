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
using System.Windows.Media;
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
    private readonly GameListFactory _gameListFactory;
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
            await ShowFavoriteGames_Click();
        };
        _topLetterNumberMenu.OnFeelingLuckySelected += async () =>
        {
            await FeelingLucky_Click(null, null);
        };

        // Initialize _favoritesManager
        _favoritesManager = FavoritesManager.LoadFavorites();

        // Initialize _gameFileGrid
        _gameFileGrid = FindName("GameFileGrid") as WrapPanel;

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

    private Task TopLetterNumberMenu_Click(string selectedLetter)
    {
        ResetPaginationButtons(); // Ensure pagination is reset at the beginning
        SearchTextBox.Text = ""; // Clear SearchTextBox
        _currentFilter = selectedLetter; // Update current filter
        return LoadGameFilesAsync(selectedLetter); // Load games
    }

    private Task ShowFavoriteGames_Click()
    {
        // Change filter to ShowAll
        _settings.ShowGames = "ShowAll";
        _settings.Save();
        ApplyShowGamesSetting();

        ResetPaginationButtons();
        SearchTextBox.Text = ""; // Clear search field
        _currentFilter = null; // Clear any active filter

        // Filter favorites for the selected system and store them in _currentSearchResults
        var favoriteGames = GetFavoriteGamesForSelectedSystem();
        if (favoriteGames.Count != 0)
        {
            _currentSearchResults = favoriteGames.ToList(); // Store only favorite games in _currentSearchResults
            return LoadGameFilesAsync(null, "FAVORITES"); // Call LoadGameFilesAsync
        }
        else
        {
            AddNoFilesMessage();
            MessageBoxLibrary.NoFavoriteFoundMessageBox();
        }

        return Task.CompletedTask;
    }

    private async Task FeelingLucky_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Change filter to ShowAll
            _settings.ShowGames = "ShowAll";
            _settings.Save();
            ApplyShowGamesSetting();

            // Check if a system is selected
            if (SystemComboBox.SelectedItem == null)
            {
                MessageBoxLibrary.PleaseSelectASystemBeforeMessageBox();
                return;
            }

            var selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedConfig == null)
            {
                return;
            }

            try
            {
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
                    // Pass just the extensions
                    var fileExtensions = selectedConfig.FileFormatsToSearch;
                    gameFiles = await GetFilePaths.GetFilesAsync(systemFolderPath, fileExtensions);
                }

                // Check if we have any games after filtering
                if (gameFiles.Count == 0)
                {
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
                _currentSearchResults = [selectedGame];

                // Load just this game to display it
                await LoadGameFilesAsync(null, "RANDOM_SELECTION");

                // If in list view, select the game in the DataGrid
                if (_settings.ViewMode != "ListView" || GameDataGrid.Items.Count <= 0) return;

                GameDataGrid.SelectedIndex = 0;
                GameDataGrid.ScrollIntoView(GameDataGrid.SelectedItem);
                GameDataGrid.Focus();
            }
            catch (Exception)
            {
                throw;
            }
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
                return;

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

    // Used in Game List Mode
    private void GameListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GameDataGrid.SelectedItem is not GameListViewItem selectedItem) return;

        var gameListViewFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _playHistoryManager, this);
        gameListViewFactory.HandleSelectionChanged(selectedItem);
    }

    // Used in Game List Mode
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

            // Clear search results
            _currentSearchResults.Clear();

            // Hide ListView
            GameFileGrid.Visibility = Visibility.Visible;
            ListViewPreviewArea.Visibility = Visibility.Collapsed;

            if (SystemComboBox.SelectedItem == null)
            {
                // // Notify developer
                // const string errorMessage = "SystemComboBox.SelectedItem is null.";
                // var ex = new Exception(errorMessage);
                // _ = LogErrors.LogErrorAsync(ex, errorMessage);

                await DisplaySystemSelectionScreenAsync();

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

    private async Task DisplaySystemSelectionScreenAsync()
    {
        GameFileGrid.Children.Clear();
        GameListItems.Clear();
        PreviewImage.Source = null;
        TotalFilesLabel.Content = null;
        _prevPageButton.IsEnabled = false;
        _nextPageButton.IsEnabled = false;
        _currentFilter = null;
        SearchTextBox.Text = "";

        GameFileGrid.Visibility = Visibility.Visible;
        ListViewPreviewArea.Visibility = Visibility.Collapsed;

        if (_systemConfigs == null || _systemConfigs.Count == 0)
        {
            var noSystemsConfiguredMsg = (string)Application.Current.TryFindResource("NoSystemsConfiguredMessage") ?? "No systems configured. Please use the 'Edit System' menu to add systems.";
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\n{noSystemsConfiguredMsg}",
                Padding = new Thickness(10),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }
        else
        {
            await PopulateSystemSelectionGridAsync();
        }

        _topLetterNumberMenu.DeselectLetter();
    }

    private async Task PopulateSystemSelectionGridAsync()
    {
        GameFileGrid.Children.Clear();

        foreach (var config in _systemConfigs.OrderBy(static s => s.SystemName))
        {
            var imagePath = await GetSystemDisplayImagePathAsync(config);
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

            var buttonContentPanel = new StackPanel { Orientation = Orientation.Vertical };

            var image = new Image
            {
                Source = loadedImage,
                Height = _settings.ThumbnailSize * 1.3,
                Width = _settings.ThumbnailSize * 1.3 * 1.6,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };
            buttonContentPanel.Children.Add(image);

            var textBlock = new TextBlock
            {
                Text = config.SystemName,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 5, 0, 0)
            };
            buttonContentPanel.Children.Add(textBlock);

            var systemButton = new Button
            {
                Content = buttonContentPanel,
                Tag = config.SystemName,
                Width = _settings.ThumbnailSize * 1.3 * 1.6 + 20,
                Height = _settings.ThumbnailSize * 1.3 + 40 + 20, // +40 for text, +20 for padding
                Margin = new Thickness(10),
                Padding = new Thickness(5)
            };
            systemButton.Click += SystemButton_Click;
            GameFileGrid.Children.Add(systemButton);
        }
    }

    private void SystemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string systemName)
        {
            SystemComboBox.SelectedItem = systemName;
        }
    }

    private static Task<string> GetSystemDisplayImagePathAsync(SystemManager config)
    {
        var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = Path.Combine(appBaseDir, "images", "systems");
        var systemName = config.SystemName;

        // Check for system-specific image files (png, jpg, jpeg)
        var possibleExtensions = new[] { ".png", ".jpg", ".jpeg" };
        foreach (var ext in possibleExtensions)
        {
            var systemImagePath = Path.Combine(systemImageFolder, systemName + ext);
            if (File.Exists(systemImagePath))
            {
                return Task.FromResult(systemImagePath);
            }
        }

        // Fallback to the global default image if no system-specific image is found
        return Task.FromResult(Path.Combine(systemImageFolder, "default.png"));
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

            // If we are in "FAVORITES" mode, use '_currentSearchResults'
            if (searchQuery == "FAVORITES" && _currentSearchResults != null && _currentSearchResults.Count != 0)
            {
                allFiles = _currentSearchResults;
            }

            // Regular behavior: load files based on startLetter or searchQuery
            else
            {
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
                    allFiles = _cachedFiles;
                }
                else
                {
                    // Fall back to scanning the folder if no cache is available
                    allFiles = await GetFilePaths.GetFilesAsync(systemFolderPath, fileExtensions); // Pass just the extensions to GetFilesAsync
                }

                // Filter by TopMenu Letter if specified
                if (!string.IsNullOrWhiteSpace(startLetter))
                {
                    allFiles = await GetFilePaths.FilterFilesAsync(allFiles, startLetter);
                }

                // Process search query (from SearchBox)
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    // If _currentSearchResults already exists, use it
                    if (_currentSearchResults != null && _currentSearchResults.Count != 0)
                    {
                        allFiles = _currentSearchResults;
                    }
                    else
                    {
                        // Check if the system is MAME-based
                        var systemIsMame = selectedManager.SystemIsMame;

                        // If a system is MAME-based, use the pre-built _mameLookup dictionary for faster lookups.
                        if (systemIsMame && _mameLookup != null)
                        {
                            // Use a case-insensitive comparison.
                            var lowerQuery = searchQuery.ToLowerInvariant();
                            allFiles = await Task.Run(() =>
                                allFiles.FindAll(file =>
                                {
                                    var fileName = Path.GetFileNameWithoutExtension(file);
                                    var filenameMatch = fileName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase); // Check if the filename contains the search query.
                                    if (filenameMatch)
                                        return true;

                                    // Lookup in the dictionary.
                                    if (_mameLookup.TryGetValue(fileName, out var description))
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

                        // Create the search results
                        _currentSearchResults = allFiles;
                    }
                }
            }

            // Sort the collection of files
            allFiles.Sort();

            // Apply ShowGames filter before pagination
            allFiles = await FilterFilesByShowGamesSettingAsync(allFiles, selectedSystem, selectedManager);

            // Count the collection of files
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
                InitializePaginationButtons();
            }

            // Display message if the number of files == 0
            if (allFiles.Count == 0)
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
                TotalFilesLabel.Content = allFiles.Count == 0 ? $"{displayingfiles0To} {endIndex} {outOf} {_totalFiles} {total}" : $"{displayingfiles} {startIndex} {to} {endIndex} {outOf} {_totalFiles} {total}"
            );

            // Reload the FavoritesConfig
            _favoritesManager = FavoritesManager.LoadFavorites();

            // Initialize GameButtonFactory with updated FavoritesConfig
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _gameFileGrid, this);

            // Initialize GameListFactory with updated FavoritesConfig
            var gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesManager, _playHistoryManager, this);

            // Display files based on ViewMode
            foreach (var filePath in allFiles)
            {
                if (_settings.ViewMode == "GridView")
                {
                    var gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedManager);
                    GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Add(gameButton));
                }
                else // ListView
                {
                    var gameListViewItem = await gameListFactory.CreateGameListViewItemAsync(filePath, selectedSystem, selectedManager);
                    await Dispatcher.InvokeAsync(() => GameListItems.Add(gameListViewItem));
                }
            }

            // Update the UI to reflect the current pagination status
            UpdatePaginationButtons();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method LoadGameFilesAsync.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMethodLoadGameFilesAsyncMessageBox();
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
}

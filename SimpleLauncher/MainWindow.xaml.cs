using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using ControlzEx.Theming;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace SimpleLauncher;

public partial class MainWindow : INotifyPropertyChanged
{
    public ObservableCollection<GameListFactory.GameListViewItem> GameListItems { get; set; } = new();
        
    // System Name and PlayTime in the Statusbar
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

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Declare _gameListFactory
    private readonly GameListFactory _gameListFactory;
        
    // tray icon
    private NotifyIcon _trayIcon;
    private ContextMenuStrip _trayMenu;
        
    // pagination related
    private int _currentPage = 1;
    private int _filesPerPage;
    private int _totalFiles;
    private int _paginationThreshold;
    private readonly Button _nextPageButton;
    private readonly Button _prevPageButton;
    private string _currentFilter;
    private List<string> _currentSearchResults = new();
        
    // Instance variables
    private readonly List<SystemConfig> _systemConfigs;
    private readonly LetterNumberMenu _letterNumberMenu;
    private readonly WrapPanel _gameFileGrid;
    private GameButtonFactory _gameButtonFactory;
    private readonly SettingsConfig _settings;
    private readonly List<MameConfig> _machines;
    private FavoritesConfig _favoritesConfig;
    private readonly FavoritesManager _favoritesManager;
        
    // Selected Image folder and Rom folder
    private string _selectedImageFolder;
    private string _selectedRomFolder;
    
    static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");
        
    public MainWindow()
    {
        InitializeComponent();
            
        DataContext = this; // Ensure the DataContext is set to the current MainWindow instance for binding

        InitializeTrayIcon();
            
        // Load settings.xml
        _settings = new SettingsConfig();
        
        // Set the initial theme
        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
        
        // Apply language
        SetLanguageMenuChecked(_settings.Language);
            
        // Load mame.xml
        _machines = MameConfig.LoadFromXml();
            
        // Load system.xml
        try
        {
            _systemConfigs = SystemConfig.LoadSystemConfigs();
            var sortedSystemNames = _systemConfigs.Select(config => config.SystemName).OrderBy(name => name).ToList();
            SystemComboBox.ItemsSource = sortedSystemNames;
        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"'system.xml' is corrupted.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            SystemXmlCorruptedMessageBox();

            // Shutdown current application instance
            Application.Current.Shutdown();
            Environment.Exit(0);
        }

        // Apply settings to application from settings.xml
        ToggleGamepad.IsChecked = _settings.EnableGamePadNavigation;
        UpdateMenuCheckMarks(_settings.ThumbnailSize);
        UpdateMenuCheckMarks2(_settings.GamesPerPage);
        UpdateMenuCheckMarks3(_settings.ShowGames);
        _filesPerPage = _settings.GamesPerPage;
        _paginationThreshold = _settings.GamesPerPage;

        // Initialize the GamePadController
        // Setting the error logger for GamePad
        GamePadController.Instance2.ErrorLogger = (ex, msg) => LogErrors.LogErrorAsync(ex, msg).Wait();

        // Start GamePad if enable
        if (_settings.EnableGamePadNavigation)
        {
            GamePadController.Instance2.Start();
        }
        else
        {
            GamePadController.Instance2.Stop();
        }

        // Initialize _gameFileGrid
        _gameFileGrid = FindName("GameFileGrid") as WrapPanel;
            
        // Initialize LetterNumberMenu and add it to the UI
        _letterNumberMenu = new LetterNumberMenu();
        LetterNumberMenu.Children.Clear(); // Clear first
        LetterNumberMenu.Children.Add(_letterNumberMenu.LetterPanel); // Add the LetterPanel
            
        // Create and integrate LetterNumberMenu
        _letterNumberMenu.OnLetterSelected += async selectedLetter =>
        {
            ResetPaginationButtons(); // Ensure pagination is reset at the beginning
            SearchTextBox.Text = "";  // Clear SearchTextBox
            _currentFilter = selectedLetter; // Update current filter
            await LoadGameFilesAsync(selectedLetter); // Load games
        };
            
        _letterNumberMenu.OnFavoritesSelected += async () =>
        {
            ResetPaginationButtons();
            SearchTextBox.Text = ""; // Clear search field
            _currentFilter = null; // Clear any active filter

            // Filter favorites for the selected system and store them in _currentSearchResults
            var favoriteGames = GetFavoriteGamesForSelectedSystem();
            if (favoriteGames.Any())
            {
                _currentSearchResults = favoriteGames.ToList(); // Store only favorite games in _currentSearchResults
                await LoadGameFilesAsync(null, "FAVORITES"); // Call LoadGameFilesAsync
            }
            else
            {
                AddNoFilesMessage();

                NoFavoriteFoundMessageBox();
            }
        };
            
        // Initialize favorite's manager and load favorites
        _favoritesManager = new FavoritesManager();
        _favoritesConfig = _favoritesManager.LoadFavorites();
            
        // Set Pagination
        PrevPageButton.IsEnabled = false;
        NextPageButton.IsEnabled = false;
        _prevPageButton = PrevPageButton;
        _nextPageButton = NextPageButton;

        // Initialize _gameButtonFactory with settings
        _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, _gameFileGrid, this);
            
        // Initialize _gameListFactory with required parameters
        _gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, this);

        // Check if a system is already selected, otherwise show the message
        if (SystemComboBox.SelectedItem == null)
        {
            AddNoSystemMessage();
        }

        // Check for Updates
        Loaded += async (_, _) => await UpdateChecker.CheckForUpdatesAsync(this);
            
        // Call Stats API
        Loaded += async (_, _) => await Stats.CallApiAsync();

        // Check for Command-line Args
        var args = Environment.GetCommandLineArgs();
        if (args.Contains("whatsnew"))
        {
            // Show UpdateHistory after the MainWindow is fully loaded
            Loaded += (_, _) => OpenUpdateHistory();
        }
        
        // Attach the Load and Close event handler.
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        void SystemXmlCorruptedMessageBox()
        {
            string thefilesystemxmliscorrupted2 = (string)Application.Current.TryFindResource("Thefilesystemxmliscorrupted") ?? "The file 'system.xml' is corrupted.";
            string youneedtofixitmanually2 = (string)Application.Current.TryFindResource("Youneedtofixitmanually") ?? "You need to fix it manually or delete it.";
            string theapplicationwillbeshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillbeshutdown") ?? "The application will be shutdown.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{thefilesystemxmliscorrupted2}\n\n" +
                            $"{youneedtofixitmanually2}\n\n" +
                            $"{theapplicationwillbeshutdown2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void NoFavoriteFoundMessageBox()
        {
            string nofavoritegamesfoundfortheselectedsystem = (string)Application.Current.TryFindResource("Nofavoritegamesfoundfortheselectedsystem") ?? "No favorite games found for the selected system.";
            string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
            MessageBox.Show(nofavoritegamesfoundfortheselectedsystem, 
                info2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    
    private void SetLanguageMenuChecked(string languageCode)
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

    private void OpenUpdateHistory()
    {
        var updateHistoryWindow = new UpdateHistory();
        updateHistoryWindow.Show();
    }

    private void SaveApplicationSettings()
    {
        _settings.MainWindowWidth = Width;
        _settings.MainWindowHeight = Height;
        _settings.MainWindowTop = Top;
        _settings.MainWindowLeft = Left;
        _settings.MainWindowState = WindowState.ToString();

        // Set other settings from the application's current state
        _settings.ThumbnailSize = _gameButtonFactory.ImageHeight;
        _settings.GamesPerPage = _filesPerPage;
        _settings.ShowGames = _settings.ShowGames;
        _settings.EnableGamePadNavigation = ToggleGamepad.IsChecked;

        // Save theme settings
        var detectedTheme = ThemeManager.Current.DetectTheme(this);
        if (detectedTheme != null)
        {
            _settings.BaseTheme = detectedTheme.BaseColorScheme;
            _settings.AccentColor = detectedTheme.ColorScheme;
        }
        _settings.Save();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Load windows state
        Width = _settings.MainWindowWidth;
        Height = _settings.MainWindowHeight;
        Top = _settings.MainWindowTop;
        Left = _settings.MainWindowLeft;
        WindowState = (WindowState)Enum.Parse(typeof(WindowState), _settings.MainWindowState);

        string nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
        // SelectedSystem and PlayTime
        SelectedSystem = nosystemselected;
        PlayTime = "00:00:00";

        // Theme settings
        App.ChangeTheme(_settings.BaseTheme, _settings.AccentColor);
        SetCheckedTheme(_settings.BaseTheme, _settings.AccentColor);
            
        // ViewMode State
        SetViewMode(_settings.ViewMode);
        
        // Check if application has write access
        if (!IsWritableDirectory(AppDomain.CurrentDomain.BaseDirectory))
        {
            MoveToWritableFolderMessageBox();
        }

        void MoveToWritableFolderMessageBox()
        {
            string itlookslikeSimpleLauncherisinstalled2 = (string)Application.Current.TryFindResource("ItlookslikeSimpleLauncheris2") ?? "It looks like 'Simple Launcher' is installed in a restricted folder (e.g., Program Files), where it does not have write access.";
            string itneedswriteaccesstoitsfolder2 = (string)Application.Current.TryFindResource("Itneedswriteaccesstoitsfolder2") ?? "It needs write access to its folder.";
            string pleasemovetheapplicationfolder2 = (string)Application.Current.TryFindResource("Pleasemovetheapplicationfolder2") ?? "Please move the application folder to a writable location like the 'Documents' folder.";
            string ifpossiblerunitwithadministrative2 = (string)Application.Current.TryFindResource("Ifpossiblerunitwithadministrative") ?? "If possible, run it with administrative privileges.";
            string accessIssue2 = (string)Application.Current.TryFindResource("AccessIssue") ?? "Access Issue";
            MessageBox.Show(
                $"{itlookslikeSimpleLauncherisinstalled2}\n\n" +
                $"{itneedswriteaccesstoitsfolder2}\n\n" +
                $"{pleasemovetheapplicationfolder2}\n\n" +
                $"{ifpossiblerunitwithadministrative2}",
                accessIssue2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        SaveApplicationSettings();
    }
    
    private void CurrentDomain_ProcessExit(object sender, EventArgs e)
    {
        // // Delete generated temp files before close.
        ExtractCompressedFile.Instance2.CleanupTempFolders();
        
        // Delete temp folders and files before close.
        CleanSimpleLauncherFolder.CleanupTrash();
        
        // Dispose gamepad resources
        GamePadController.Instance2.Stop();
        GamePadController.Instance2.Dispose();
    }

    // Used in cases that need to reload system.xml or update the pagination settings
    public void MainWindow_Restart()
    {
        SaveApplicationSettings();

        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule != null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = processModule.FileName,
                UseShellExecute = true
            };

            Process.Start(startInfo);

            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
        
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
        
    private List<string> GetFavoriteGamesForSelectedSystem()
    {
        // Reload favorites to ensure we have the latest data
        _favoritesConfig = _favoritesManager.LoadFavorites();
            
        string selectedSystem = SystemComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedSystem))
        {
            return new List<string>();
        }

        // Retrieve the system configuration for the selected system
        var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
        if (selectedConfig == null)
        {
            return new List<string>();
        }

        string systemFolderPath = selectedConfig.SystemFolder;

        // Filter the favorites and build the full file path for each favorite game
        var favoriteGamePaths = _favoritesConfig.FavoriteList
            .Where(fav => fav.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase))
            .Select(fav => Path.Combine(systemFolderPath, fav.FileName))
            .ToList();

        return favoriteGamePaths;
    }
    
    private Task ShowPleaseWaitWindowAsync(Window window)
    {
        return Task.Run(() =>
        {
            window.Dispatcher.Invoke(window.Show);
        });
    }

    private Task ClosePleaseWaitWindowAsync(Window window)
    {
        return Task.Run(() =>
        {
            window.Dispatcher.Invoke(window.Close);
        });
    }
    
    private void GameDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (GameDataGrid.SelectedItem is GameListFactory.GameListViewItem selectedItem)
        {
            var gameListViewFactory = new GameListFactory(
                EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, this
            );
            gameListViewFactory.HandleSelectionChanged(selectedItem);
        }
    }

    private async void GameDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (GameDataGrid.SelectedItem is GameListFactory.GameListViewItem selectedItem)
            {
                // Delegate the double-click handling to GameListFactory
                await _gameListFactory.HandleDoubleClick(selectedItem);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Error while using the method GameDataGrid_MouseDoubleClick.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MethodErrorMessageBox();
        }
    }

    private static bool IsWritableDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            // Generate a unique temporary file path
            string testFile = Path.Combine(path, Guid.NewGuid().ToString() + ".tmp");

            // Attempt to create and delete the file
            using (FileStream fs = new FileStream(testFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                fs.Close();
            }
            File.Delete(testFile);

            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private void SystemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        if (SystemComboBox.SelectedItem != null)
        {
            string selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);

            if (selectedConfig != null)
            {
                // Populate EmulatorComboBox with the emulators for the selected system
                EmulatorComboBox.ItemsSource = selectedConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();

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

                // Display the system info
                string systemFolderPath = selectedConfig.SystemFolder;
                var fileExtensions = selectedConfig.FileFormatsToSearch.Select(ext => $"{ext}").ToList();
                int gameCount = FileManager.CountFiles(systemFolderPath, fileExtensions);
                
                // SystemInfo
                SystemManager.DisplaySystemInfo(systemFolderPath, gameCount, selectedConfig, _gameFileGrid);
                    
                // Update Image Folder and Rom Folder Variables
                _selectedRomFolder = selectedConfig.SystemFolder;
                _selectedImageFolder = string.IsNullOrWhiteSpace(selectedConfig.SystemImageFolder) 
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedConfig.SystemName) 
                    : selectedConfig.SystemImageFolder;
                    
                // Call DeselectLetter to clear any selected letter
                _letterNumberMenu.DeselectLetter();
                    
                ResetPaginationButtons();
            }
            else
            {
                AddNoSystemMessage();
            }
        }
        else
        {
            AddNoSystemMessage();
        }
    }

    private void AddNoSystemMessage()
    {
        string noSystemMessage = (string)Application.Current.TryFindResource("NoSystemMessage") ?? "Please select a System";
        // Check the current view mode
        if (_settings.ViewMode == "GridView")
        {
            GameFileGrid.Children.Clear();
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\n{noSystemMessage}",
                Padding = new Thickness(10)
            });
        }
        else
        {
            // For List view, clear existing items in the ObservableCollection
            GameListItems.Clear();
            GameListItems.Add(new GameListFactory.GameListViewItem
            {
                FileName = noSystemMessage,
                MachineDescription = string.Empty
            });
        }

        // Deselect any selected letter when no system is selected
        _letterNumberMenu.DeselectLetter();
    }
        
    private void AddNoFilesMessage()
    {
        string noGamesMatched = (string)Application.Current.TryFindResource("nogamesmatched") ?? "Unfortunately, no games matched your search query or the selected button.";
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
            // For List view, clear existing items in the ObservableCollection
            GameListItems.Clear();
            GameListItems.Add(new GameListFactory.GameListViewItem
            {
                FileName = noGamesMatched,
                MachineDescription = string.Empty
            });
        }

        // Deselect any selected letter when no system is selected
        _letterNumberMenu.DeselectLetter();
    }

    public async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null)
    {
        // Move scroller to top
        Scroller.Dispatcher.Invoke(() => Scroller.ScrollToTop());
            
        // Clear PreviewImage
        PreviewImage.Source = null;

        // Clear FileGrid
        GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Clear());
            
        // Clear the ListItems
        await Dispatcher.InvokeAsync(() => GameListItems.Clear());
            
        // Check ViewMode and apply it to the UI
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
                AddNoSystemMessage();
                return;
            }
 
            string selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);
            if (selectedConfig == null)
            {
                // Notify developer
                string errorMessage = "Invalid system configuration.\n\n" +
                                      "Method: LoadGameFilesAsync";
                Exception ex = new Exception(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                InvalidSystemConfigMessageBox();

                return;
            }
            List<string> allFiles;
                
            // Check if we are in "FAVORITES" mode
            if (searchQuery == "FAVORITES" && _currentSearchResults != null && _currentSearchResults.Any())
            {
                allFiles = _currentSearchResults;
            }
            // Regular behavior: load files based on startLetter or searchQuery
            else
            {
                string systemFolderPath = selectedConfig.SystemFolder;
                var fileExtensions = selectedConfig.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList();

                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    // Use stored search results if available
                    if (_currentSearchResults != null && _currentSearchResults.Count != 0)
                    {
                        allFiles = _currentSearchResults;
                    }
                    else
                    {
                        // List of files that match the system extensions
                        allFiles = await FileManager.GetFilesAsync(systemFolderPath, fileExtensions);

                        if (!string.IsNullOrWhiteSpace(startLetter))
                        {
                            allFiles = await FileManager.FilterFilesAsync(allFiles, startLetter);
                        }

                        bool systemIsMame = selectedConfig.SystemIsMame;

                        allFiles = await Task.Run(() => allFiles.Where(file =>
                        {
                            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                            // Search in filename
                            bool filenameMatch = fileNameWithoutExtension.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;

                            if (!systemIsMame) // If not a MAME system, return match based on filename only
                            {
                                return filenameMatch;
                            }

                            // For MAME systems, additionally check the machine description for a match
                            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
                            bool descriptionMatch = machine != null && machine.Description.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;

                            return filenameMatch || descriptionMatch;

                        }).ToList());

                        // Store the search results
                        _currentSearchResults = allFiles;
                    }
                }
                else
                {
                    _currentSearchResults?.Clear();
    
                    // List of files with that match the system extensions
                    allFiles = await FileManager.GetFilesAsync(systemFolderPath, fileExtensions);

                    if (!string.IsNullOrWhiteSpace(startLetter))
                    {
                        allFiles = await FileManager.FilterFilesAsync(allFiles, startLetter);
                    }
                }
            }

            // Sort the collection of files
            allFiles.Sort();

            // Count the collection of files
            _totalFiles = allFiles.Count;

            // Calculate the indices of files displayed on the current page
            int startIndex = (_currentPage - 1) * _filesPerPage + 1; // +1 because we are dealing with a 1-based index for displaying
            int endIndex = startIndex + _filesPerPage; // Actual number of files loaded on this page
            if (endIndex > _totalFiles)
            {
                endIndex = _totalFiles;
            }

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
            string displayingfiles0To = (string)Application.Current.TryFindResource("Displayingfiles0to") ?? "Displaying files 0 to";
            string outOf = (string)Application.Current.TryFindResource("outof") ?? "out of";
            string total = (string)Application.Current.TryFindResource("total") ?? "total";
            string displayingfiles = (string)Application.Current.TryFindResource("Displayingfiles") ?? "Displaying files";
            string to = (string)Application.Current.TryFindResource("to") ?? "to";
            
            TotalFilesLabel.Dispatcher.Invoke(() => 
                TotalFilesLabel.Content = allFiles.Count == 0 ? $"{displayingfiles0To} {endIndex} {outOf} {_totalFiles} {total}" : $"{displayingfiles} {startIndex} {to} {endIndex} {outOf} {_totalFiles} {total}"
            );

            // Reload the FavoritesConfig
            _favoritesConfig = _favoritesManager.LoadFavorites();
                
            // Initialize GameButtonFactory with updated FavoritesConfig
            _gameButtonFactory = new GameButtonFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, _gameFileGrid, this);
                
            // Initialize GameListFactory with updated FavoritesConfig
            var gameListFactory = new GameListFactory(EmulatorComboBox, SystemComboBox, _systemConfigs, _machines, _settings, _favoritesConfig, this);

            // Display files based on ViewMode
            foreach (var filePath in allFiles)
            {
                if (_settings.ViewMode == "GridView")
                {
                    Button gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedConfig);
                    GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Add(gameButton));
                }
                else // For list view
                {
                    var gameListViewItem = await gameListFactory.CreateGameListViewItemAsync(filePath, selectedSystem, selectedConfig);
                    await Dispatcher.InvokeAsync(() => GameListItems.Add(gameListViewItem));
                }
            }
                
            // Apply visibility settings to each button based on _settings.ShowGames
            ApplyShowGamesSetting();

            // Update the UI to reflect the current pagination status
            UpdatePaginationButtons();
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Error in the method LoadGameFilesAsync.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                
            // Notify user
            ErrorMethodLoadGameFilesAsyncMessageBox();
        }

        void InvalidSystemConfigMessageBox()
        {
            string therewasanerrorwhileloading2 = (string)Application.Current.TryFindResource("Therewasanerrorwhileloading") ?? "There was an error while loading the system configuration for this system.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{therewasanerrorwhileloading2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void ErrorMethodLoadGameFilesAsyncMessageBox()
        {
            string therewasanerrorloadingthegame2 = (string)Application.Current.TryFindResource("Therewasanerrorloadingthegame") ?? "There was an error loading the game list.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{therewasanerrorloadingthegame2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    #region Menu Items
        
    private void EasyMode_Click(object sender, RoutedEventArgs e)
    {
        SaveApplicationSettings();
                
        EditSystemEasyMode editSystemEasyModeWindow = new(_settings);
        editSystemEasyModeWindow.ShowDialog();
    }

    private void ExpertMode_Click(object sender, RoutedEventArgs e)
    {
        SaveApplicationSettings();
                
        EditSystem editSystemWindow = new(_settings);
        editSystemWindow.ShowDialog();
    }
    
    private void DownloadImagePack_Click(object sender, RoutedEventArgs e)
    {
        DownloadImagePack downloadImagePack = new();
        downloadImagePack.ShowDialog();
    }
        
    private void EditLinks_Click(object sender, RoutedEventArgs e)
    {
        SaveApplicationSettings();
                
        EditLinks editLinksWindow = new(_settings);
        editLinksWindow.ShowDialog();
    }
    private void BugReport_Click(object sender, RoutedEventArgs e)
    {
        BugReport bugReportWindow = new();
        bugReportWindow.ShowDialog();
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
            string contextMessage = $"Unable to open the Donation Link from the menu.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            ErrorOpeningDonationLinkMessageBox();
        }

        void ErrorOpeningDonationLinkMessageBox()
        {
            string therewasanerroropeningthedonation2 = (string)Application.Current.TryFindResource("Therewasanerroropeningthedonation") ?? "There was an error opening the Donation Link.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{therewasanerroropeningthedonation2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        About aboutWindow = new();
        aboutWindow.ShowDialog();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
        
    private void ShowAllGames_Click(object sender, RoutedEventArgs e)
    {
        UpdateGameVisibility(visibilityCondition: _ => true); // Show all games
        UpdateShowGamesSetting("ShowAll");
        UpdateMenuCheckMarks("ShowAll");
    }
        
    private void ShowGamesWithCover_Click(object sender, RoutedEventArgs e)
    {
        UpdateGameVisibility(visibilityCondition: btn => btn.Tag?.ToString() != "DefaultImage"); // Show games with covers only
        UpdateShowGamesSetting("ShowWithCover");
        UpdateMenuCheckMarks("ShowWithCover");
    }

    private void ShowGamesWithoutCover_Click(object sender, RoutedEventArgs e)
    {
        UpdateGameVisibility(visibilityCondition: btn => btn.Tag?.ToString() == "DefaultImage"); // Show games without covers only
        UpdateShowGamesSetting("ShowWithoutCover");
        UpdateMenuCheckMarks("ShowWithoutCover");
    }

    private void UpdateGameVisibility(Func<Button, bool> visibilityCondition)
    {
        foreach (var child in _gameFileGrid.Children)
        {
            if (child is Button btn)
            {
                btn.Visibility = visibilityCondition(btn) ? Visibility.Visible : Visibility.Collapsed;
            }
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
        
    private async void ToggleGamepad_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuItem menuItem)
            {
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
                    string formattedException = $"Failed to toggle gamepad.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, formattedException);
                    
                    // Notify user
                    ToggleGamepadFailureMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"Failed to toggle gamepad.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
        }

        void ToggleGamepadFailureMessageBox()
        {
            string failedtotogglegamepad2 = (string)Application.Current.TryFindResource("Failedtotogglegamepad") ?? "Failed to toggle gamepad.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{failedtotogglegamepad2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ThumbnailSize_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is MenuItem clickedItem)
            {
                var sizeText = clickedItem.Name.Replace("Size", "");
                if (int.TryParse(new string(sizeText.Where(char.IsDigit).ToArray()), out int newSize))
                {
                    _gameButtonFactory.ImageHeight = newSize; // Update the image height
                    _settings.ThumbnailSize = newSize;
                    _settings.Save();
                    
                    UpdateMenuCheckMarks(newSize);
                    
                    // Reload List of Games
                    await LoadGameFilesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Error in method ThumbnailSize_Click.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MethodErrorMessageBox();
        }
    }
        
    private void GamesPerPage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem clickedItem)
        {
            var pageText = clickedItem.Name.Replace("Page", "");
            if (int.TryParse(new string(pageText.Where(char.IsDigit).ToArray()), out int newPage))
            {
                _filesPerPage = newPage; 
                _paginationThreshold = newPage; 
                _settings.GamesPerPage = newPage; 
                    
                _settings.Save(); 
                UpdateMenuCheckMarks2(newPage);
                    
                SaveApplicationSettings();
                MainWindow_Restart();
            }
        }
    }
        
    private void GlobalSearch_Click(object sender, RoutedEventArgs e)
    {
        var globalSearchWindow = new GlobalSearch(_systemConfigs, _machines, _settings, this);
        globalSearchWindow.Show();
    }
        
    private void GlobalStats_Click(object sender, RoutedEventArgs e)
    {
        var globalStatsWindow = new GlobalStats(_systemConfigs);
        globalStatsWindow.Show();
    }
        
    private void Favorites_Click(object sender, RoutedEventArgs e)
    {
        SaveApplicationSettings();
            
        var favoritesWindow = new Favorites(_settings, _systemConfigs, _machines, this);
        favoritesWindow.Show();
    }
        
    private void OrganizeSystemImages_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string findRomCoverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "FindRomCover", "FindRomCover.exe");

            if (File.Exists(findRomCoverPath))
            {
                string absoluteImageFolder = null;
                string absoluteRomFolder = null;

                // Check if _selectedImageFolder and _selectedRomFolder are set
                if (!string.IsNullOrEmpty(_selectedImageFolder))
                {
                    absoluteImageFolder = Path.GetFullPath(Path.IsPathRooted(_selectedImageFolder)
                        ? _selectedImageFolder
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedImageFolder));
                }

                if (!string.IsNullOrEmpty(_selectedRomFolder))
                {
                    absoluteRomFolder = Path.GetFullPath(Path.IsPathRooted(_selectedRomFolder)
                        ? _selectedRomFolder
                        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _selectedRomFolder));
                }

                // Determine arguments based on available folders
                string arguments = string.Empty;
                if (absoluteImageFolder != null && absoluteRomFolder != null)
                {
                    arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
                }

                // Start the process with or without arguments
                Process.Start(new ProcessStartInfo
                {
                    FileName = findRomCoverPath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    WorkingDirectory = findRomCoverPath
                });
            }
            else
            {
                // Notify developer
                string formattedException = "The file 'FindRomCover.exe' is missing.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                FindRomCoverMissingMessageBox();
            }
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // Notify developer
            string formattedException = $"The operation was canceled by the user while trying to launch 'FindRomCover.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            FindRomCoverLaunchWasCanceledByUserMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'FindRomCover.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            FindRomCoverLaunchWasBlockedMessageBox();
        }

        void FindRomCoverMissingMessageBox()
        {
            string findRomCoverexewasnotfound = (string)Application.Current.TryFindResource("FindRomCoverexewasnotfound") ?? "'FindRomCover.exe' was not found in the expected path.";
            string doyouwanttoreinstall = (string)Application.Current.TryFindResource("Doyouwanttoreinstall") ?? "Do you want to reinstall 'Simple Launcher' to fix it?";
            string fileNotFound = (string)Application.Current.TryFindResource("FileNotFound") ?? "File Not Found";
            MessageBoxResult reinstall = MessageBox.Show(
                $"{findRomCoverexewasnotfound}\n\n" +
                $"{doyouwanttoreinstall}",
                fileNotFound, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                string pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually.";
                string pleaseReinstall = (string)Application.Current.TryFindResource("PleaseReinstall") ?? "Please Reinstall";
                MessageBox.Show(pleasereinstallSimpleLaunchermanually,
                    pleaseReinstall, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void FindRomCoverLaunchWasCanceledByUserMessageBox()
        {
            string thelaunchofFindRomCoverexewascanceled = (string)Application.Current.TryFindResource("ThelaunchofFindRomCoverexewascanceled") ?? "The launch of 'FindRomCover.exe' was canceled by the user.";
            string operationCanceled = (string)Application.Current.TryFindResource("OperationCanceled") ?? "Operation Canceled";
            MessageBox.Show(thelaunchofFindRomCoverexewascanceled,
                operationCanceled, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void FindRomCoverLaunchWasBlockedMessageBox()
        {
            string anerroroccurredwhiletryingtolaunch = (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtolaunch") ?? "An error occurred while trying to launch 'FindRomCover.exe'.";
            string yourcomputermaynothavegranted = (string)Application.Current.TryFindResource("Yourcomputermaynothavegranted") ?? "Your computer may not have granted the necessary permissions for 'Simple Launcher' to execute the file 'FindRomCover.exe'. Please ensure that 'Simple Launcher' has the required administrative privileges.";
            string alternativelythelaunchmayhavebeenblockedby = (string)Application.Current.TryFindResource("Alternativelythelaunchmayhavebeenblockedby") ?? "Alternatively, the launch may have been blocked by your antivirus software. If so, please configure your antivirus settings to allow 'FindRomCover.exe' to run.";
            string wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
            string error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show(
                $"{anerroroccurredwhiletryingtolaunch}\n\n" +
                $"{yourcomputermaynothavegranted}\n\n" +
                $"{alternativelythelaunchmayhavebeenblockedby}\n\n" +
                $"{wouldyouliketoopentheerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = LogPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception)
                {
                    string thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                    MessageBox.Show(thefileerroruserlog,
                        error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void CreateBatchFilesForPS3Games_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForPs3GamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.exe");

            if (File.Exists(createBatchFilesForPs3GamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForPs3GamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'CreateBatchFilesForPS3Games.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForPS3Games.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }
    
    private void BatchConvertIsoToXiso_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchConvertIsoToXisoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertIsoToXiso", "BatchConvertIsoToXiso.exe");

            if (File.Exists(createBatchConvertIsoToXisoPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchConvertIsoToXisoPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'BatchConvertIsoToXiso.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'BatchConvertIsoToXiso.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }

    private void BatchConvertToCHD_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchConvertToChdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", "BatchConvertToCHD.exe");

            if (File.Exists(createBatchConvertToChdPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchConvertToChdPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'BatchConvertToCHD.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'BatchConvertToCHD.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }
    
    private void BatchConvertTo7z_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string batchConvertTo7ZPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertTo7z", "BatchConvertTo7z.exe");

            if (File.Exists(batchConvertTo7ZPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = batchConvertTo7ZPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'BatchConvertTo7z.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'BatchConvertTo7z.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }
    
    private void BatchConvertToZip_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string batchConvertToZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToZip", "BatchConvertToZip.exe");

            if (File.Exists(batchConvertToZipPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = batchConvertToZipPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'BatchConvertToZip.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'BatchConvertToZip.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }

    private void CreateBatchFilesForScummVMGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForScummVmGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.exe");

            if (File.Exists(createBatchFilesForScummVmGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForScummVmGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'CreateBatchFilesForScummVMGames.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForScummVMGames.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }
        
    private void CreateBatchFilesForSegaModel3Games_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForSegaModel3Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.exe");

            if (File.Exists(createBatchFilesForSegaModel3Path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForSegaModel3Path,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'CreateBatchFilesForSegaModel3Games.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForSegaModel3Games.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }

    private void CreateBatchFilesForWindowsGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForWindowsGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.exe");

            if (File.Exists(createBatchFilesForWindowsGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForWindowsGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'CreateBatchFilesForWindowsGames.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForWindowsGames.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }
    
    private void CreateBatchFilesForXbox360XBLAGames_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string createBatchFilesForXbox360XblaGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.exe");

            if (File.Exists(createBatchFilesForXbox360XblaGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForXbox360XblaGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                string formattedException = "'CreateBatchFilesForXbox360XBLAGames.exe' was not found.";
                Exception ex = new Exception(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"An error occurred while launching 'CreateBatchFilesForXbox360XBLAGames.exe'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            // Notify user
            ErrorLaunchingToolMessageBox();
        }
    }
        
    private void UpdateMenuCheckMarks(int selectedSize)
    {
        Size100.IsChecked = (selectedSize == 100);
        Size150.IsChecked = (selectedSize == 150);
        Size200.IsChecked = (selectedSize == 200);
        Size250.IsChecked = (selectedSize == 250);
        Size300.IsChecked = (selectedSize == 300);
        Size350.IsChecked = (selectedSize == 350);
        Size400.IsChecked = (selectedSize == 400);
        Size450.IsChecked = (selectedSize == 450);
        Size500.IsChecked = (selectedSize == 500);
        Size550.IsChecked = (selectedSize == 550);
        Size600.IsChecked = (selectedSize == 600);
    }
        
    private void UpdateMenuCheckMarks2(int selectedSize)
    {
        Page100.IsChecked = (selectedSize == 100);
        Page200.IsChecked = (selectedSize == 200);
        Page300.IsChecked = (selectedSize == 300);
        Page400.IsChecked = (selectedSize == 400);
        Page500.IsChecked = (selectedSize == 500);
        Page1000.IsChecked = (selectedSize == 1000);
    }
        
    private void UpdateMenuCheckMarks3(string selectedValue)
    {
        ShowAll.IsChecked = (selectedValue == "ShowAll");
        ShowWithCover.IsChecked = (selectedValue == "ShowWithCover");
        ShowWithoutCover.IsChecked = (selectedValue == "ShowWithoutCover");
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
                string nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
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
                string nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
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
            string errorMessage = $"Error while using the method ChangeViewMode_Click.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            ErrorChangingViewModeMessageBox();
        }

        void ErrorChangingViewModeMessageBox()
        {
            string therewasanerrorwhilechangingtheviewmode2 = (string)Application.Current.TryFindResource("Therewasanerrorwhilechangingtheviewmode") ?? "There was an error while changing the view mode.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{therewasanerrorwhilechangingtheviewmode2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    private void ApplyShowGamesSetting()
    {
        switch (_settings.ShowGames)
        {
            case "ShowAll":
                ShowAllGames_Click(ShowAll, null);
                break;
            case "ShowWithCover":
                ShowGamesWithCover_Click(ShowWithCover, null);
                break;
            case "ShowWithoutCover":
                ShowGamesWithoutCover_Click(ShowWithoutCover, null);
                break;
        }
    }
    
    private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            string selectedLanguage = menuItem.Name switch
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
            SetLanguageMenuChecked(selectedLanguage);
            
            MainWindow_Restart();
        }
    }

    #endregion
        
    #region Theme Options
        
    private void ChangeBaseTheme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            string baseTheme = menuItem.Name;
            string currentAccent = ThemeManager.Current.DetectTheme(this)?.ColorScheme;
            App.ChangeTheme(baseTheme, currentAccent);

            UncheckBaseThemes();
            menuItem.IsChecked = true;
        }
    }

    private void ChangeAccentColor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem)
        {
            string accentColor = menuItem.Name;
            string currentBaseTheme = ThemeManager.Current.DetectTheme(this)?.BaseColorScheme;
            App.ChangeTheme(currentBaseTheme, accentColor);
            
            UncheckAccentColors();
            menuItem.IsChecked = true;
        }
    }

    private void UncheckBaseThemes()
    {
        Light.IsChecked = false;
        Dark.IsChecked = false;
    }

    private void UncheckAccentColors()
    {
        Red.IsChecked = false;
        Green.IsChecked = false;
        Blue.IsChecked = false;
        Purple.IsChecked = false;
        Orange.IsChecked = false;
        Lime.IsChecked = false;
        Emerald.IsChecked = false;
        Teal.IsChecked = false;
        Cyan.IsChecked = false;
        Cobalt.IsChecked = false;
        Indigo.IsChecked = false;
        Violet.IsChecked = false;
        Pink.IsChecked = false;
        Magenta.IsChecked = false;
        Crimson.IsChecked = false;
        Amber.IsChecked = false;
        Yellow.IsChecked = false;
        Brown.IsChecked = false;
        Olive.IsChecked = false;
        Steel.IsChecked = false;
        Mauve.IsChecked = false;
        Taupe.IsChecked = false;
        Sienna.IsChecked = false;
    }

    private void SetCheckedTheme(string baseTheme, string accentColor)
    {
        switch (baseTheme)
        {
            case "Light":
                Light.IsChecked = true;
                break;
            case "Dark":
                Dark.IsChecked = true;
                break;
        }

        switch (accentColor)
        {
            case "Red":
                Red.IsChecked = true;
                break;
            case "Green":
                Green.IsChecked = true;
                break;
            case "Blue":
                Blue.IsChecked = true;
                break;
            case "Purple":
                Purple.IsChecked = true;
                break;
            case "Orange":
                Orange.IsChecked = true;
                break;
            case "Lime":
                Lime.IsChecked = true;
                break;
            case "Emerald":
                Emerald.IsChecked = true;
                break;
            case "Teal":
                Teal.IsChecked = true;
                break;
            case "Cyan":
                Cyan.IsChecked = true;
                break;
            case "Cobalt":
                Cobalt.IsChecked = true;
                break;
            case "Indigo":
                Indigo.IsChecked = true;
                break;
            case "Violet":
                Violet.IsChecked = true;
                break;
            case "Pink":
                Pink.IsChecked = true;
                break;
            case "Magenta":
                Magenta.IsChecked = true;
                break;
            case "Crimson":
                Crimson.IsChecked = true;
                break;
            case "Amber":
                Amber.IsChecked = true;
                break;
            case "Yellow":
                Yellow.IsChecked = true;
                break;
            case "Brown":
                Brown.IsChecked = true;
                break;
            case "Olive":
                Olive.IsChecked = true;
                break;
            case "Steel":
                Steel.IsChecked = true;
                break;
            case "Mauve":
                Mauve.IsChecked = true;
                break;
            case "Taupe":
                Taupe.IsChecked = true;
                break;
            case "Sienna":
                Sienna.IsChecked = true;
                break;
        }
    }
    #endregion
    
    #region TrayIcon
        
    private void InitializeTrayIcon()
    {
        // Create a context menu for the tray icon
        _trayMenu = new ContextMenuStrip();
        
        string open = (string)Application.Current.TryFindResource("Open") ?? "Open";
        string exit = (string)Application.Current.TryFindResource("Exit") ?? "Exit";
        _trayMenu.Items.Add(open, null, OnOpen);
        _trayMenu.Items.Add(exit, null, OnExit);

        // Load the embedded icon from resources
        var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/SimpleLauncher;component/icon/icon.ico"))?.Stream;

        // Create the tray icon using the embedded icon
        if (iconStream != null)
        {
            _trayIcon = new NotifyIcon
            {
                Icon = new Icon(iconStream),
                ContextMenuStrip = _trayMenu,
                Text = @"Simple Launcher",
                Visible = true
            };

            // Handle tray icon events
            _trayIcon.DoubleClick += OnOpen;
        }
    }
        
    // Handle "Open" context menu item or tray icon double-click
    private void OnOpen(object sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    // Handle "Exit" context menu item
    private void OnExit(object sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        Application.Current.Shutdown();
    }

    // Override the OnStateChanged method to hide the window when minimized
    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            // Retrieve the dynamic resource string
            string isminimizedtothetray = (string)Application.Current.TryFindResource("isminimizedtothetray") ?? "is minimized to the tray.";
            ShowTrayMessage($"Simple Launcher {isminimizedtothetray}");
        }
        base.OnStateChanged(e);
    }

    // Display a balloon message
    private void ShowTrayMessage(string message)
    {
        _trayIcon.BalloonTipTitle = @"Simple Launcher";
        _trayIcon.BalloonTipText = message;
        _trayIcon.ShowBalloonTip(3000); // Display for 3 seconds
    }

    // Clean up resources when closing the application
    protected override void OnClosing(CancelEventArgs e)
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        base.OnClosing(e);
    }
        
    #endregion
    
    #region Pagination

    private void ResetPaginationButtons()
    {
        _prevPageButton.IsEnabled = false;
        _nextPageButton.IsEnabled = false;
        _currentPage = 1;
        Scroller.ScrollToTop();
        TotalFilesLabel.Content = null;
    }
    private void InitializePaginationButtons()
    {
        _prevPageButton.IsEnabled = _currentPage > 1;
        _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
        Scroller.ScrollToTop();
    }
        
    private async void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                if (_currentSearchResults.Any())
                {
                    await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
                }
                else
                {
                    await LoadGameFilesAsync(_currentFilter);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Previous page button error.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            NavigationButtonErrorMessageBox();
        }
    }

    private static void NavigationButtonErrorMessageBox()
    {
        string therewasanerrorinthenavigationbutton2 = (string)Application.Current.TryFindResource("Therewasanerrorinthenavigationbutton") ?? "There was an error in the navigation button.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorinthenavigationbutton2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            int totalPages = (int)Math.Ceiling(_totalFiles / (double)_filesPerPage);

            if (_currentPage < totalPages)
            {
                _currentPage++;
                if (_currentSearchResults.Any())
                {
                    await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
                }
                else
                {
                    await LoadGameFilesAsync(_currentFilter);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Next page button error.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            NavigationButtonErrorMessageBox();
        }
    }
        
    private void UpdatePaginationButtons()
    {
        _prevPageButton.IsEnabled = _currentPage > 1;
        _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
    }
        
    #endregion
    
    #region MainWindow Search
        
    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ExecuteSearch();
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Error in the method SearchButton_Click.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MainWindowSearchEngineErrorMessageBox();
        }
    }

    private static void MainWindowSearchEngineErrorMessageBox()
    {
        string therewasanerrorwiththesearchengine2 = (string)Application.Current.TryFindResource("Therewasanerrorwiththesearchengine") ?? "There was an error with the search engine.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwiththesearchengine2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter)
            {
                await ExecuteSearch();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Error in the method SearchTextBox_KeyDown.\n\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
 
            // Notify user
            MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async Task ExecuteSearch()
    {
        ResetPaginationButtons();
            
        _currentSearchResults.Clear();
    
        // Call DeselectLetter to clear any selected letter
        _letterNumberMenu.DeselectLetter();

        var searchQuery = SearchTextBox.Text.Trim();

        if (SystemComboBox.SelectedItem == null)
        {
            // Notify user
            SelectSystemBeforeSearchMessageBox();

            return;
        }
        
        if (string.IsNullOrEmpty(searchQuery))
        {
            // Notify user
            EnterSearchQueryMessageBox();

            return;
        }

        var pleaseWaitWindow = new PleaseWaitSearch();
        await ShowPleaseWaitWindowAsync(pleaseWaitWindow);

        var startTime = DateTime.Now;

        try
        {
            await LoadGameFilesAsync(null, searchQuery);
        }
        finally
        {
            var elapsed = DateTime.Now - startTime;
            var remainingTime = TimeSpan.FromSeconds(1) - elapsed;
            if (remainingTime > TimeSpan.Zero)
            {
                await Task.Delay(remainingTime);
            }
            await ClosePleaseWaitWindowAsync(pleaseWaitWindow);
        }

        void SelectSystemBeforeSearchMessageBox()
        {
            string pleaseselectasystembeforesearching = (string)Application.Current.TryFindResource("Pleaseselectasystembeforesearching") ?? "Please select a system before searching.";
            string systemNotSelected = (string)Application.Current.TryFindResource("SystemNotSelected") ?? "System Not Selected";
            MessageBox.Show(pleaseselectasystembeforesearching, systemNotSelected, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        void EnterSearchQueryMessageBox()
        {
            string pleaseenterasearchquery = (string)Application.Current.TryFindResource("Pleaseenterasearchquery") ?? "Please enter a search query.";
            string searchQueryRequired = (string)Application.Current.TryFindResource("SearchQueryRequired") ?? "Search Query Required";
            MessageBox.Show(pleaseenterasearchquery, searchQueryRequired, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
        
    #endregion
    
    private static void ErrorLaunchingToolMessageBox()
    {
        string anerroroccurredwhilelaunchingtheselectedtool2 = (string)Application.Current.TryFindResource("Anerroroccurredwhilelaunchingtheselectedtool") ?? "An error occurred while launching the selected tool.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
        string dowanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{anerroroccurredwhilelaunchingtheselectedtool2}\n\n" +
                                     $"{theerrorwasreportedtothedeveloper2}\n\n" +
                                     $"{dowanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                // Notify user
                string thefileerroruserlogwasnotfound2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwasnotfound2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }

    private static void SelectedToolNotFoundMessageBox()
    {
        string theselectedtoolwasnotfound2 = (string)Application.Current.TryFindResource("Theselectedtoolwasnotfound") ?? "The selected tool was not found in the expected path.";
        string doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix it?";
        string fileNotFound2 = (string)Application.Current.TryFindResource("FileNotFound") ?? "File Not Found";
        MessageBoxResult reinstall = MessageBox.Show(
            $"{theselectedtoolwasnotfound2}\n\n" +
            $"{doyouwanttoreinstallSimpleLauncher2}",
            fileNotFound2, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (reinstall == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually.";
            string pleaseReinstall2 = (string)Application.Current.TryFindResource("PleaseReinstall") ?? "Please Reinstall";
            MessageBox.Show(pleasereinstallSimpleLaunchermanually2,
                pleaseReinstall2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private static void MethodErrorMessageBox()
    {
        string therewasanerrorwiththismethod2 = (string)Application.Current.TryFindResource("Therewasanerrorwiththismethod") ?? "There was an error with this method.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer that will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwiththismethod2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

}
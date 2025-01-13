using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

public partial class GlobalSearch
{
    private readonly List<SystemConfig> _systemConfigs;
    private readonly List<MameConfig> _machines;
    private readonly SettingsConfig _settings;
    private ObservableCollection<SearchResult> _searchResults;
    private PleaseWaitSearch _pleaseWaitWindow;
    private DispatcherTimer _closeTimer;
    private readonly FavoritesManager _favoritesManager;
    private readonly MainWindow _mainWindow;
    private readonly ComboBox _mockSystemComboBox = new();
    private readonly ComboBox _mockEmulatorComboBox = new();

    public GlobalSearch(List<SystemConfig> systemConfigs, List<MameConfig> machines, SettingsConfig settings, MainWindow mainWindow)
    {
        InitializeComponent();
        _systemConfigs = systemConfigs;
        _machines = machines;
        _settings = settings;
        _searchResults = [];
        ResultsDataGrid.ItemsSource = _searchResults;
        _mainWindow = mainWindow;
        _favoritesManager = new FavoritesManager();
        Closed += GlobalSearch_Closed;
            
        // Apply the theme to this window
        App.ApplyThemeToWindow(this);
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        string searchTerm = SearchTextBox.Text;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            string pleaseenterasearchterm2 = (string)Application.Current.TryFindResource("Pleaseenterasearchterm") ?? "Please enter a search term.";
            string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            MessageBox.Show(pleaseenterasearchterm2,
                warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        LaunchButton.IsEnabled = false;
        _searchResults.Clear();

        _pleaseWaitWindow = new PleaseWaitSearch
        {
            Owner = this
        };
        _pleaseWaitWindow.Show();

        _closeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _closeTimer.Tick += (_, _) => _closeTimer.Stop();

        var backgroundWorker = new BackgroundWorker();
        backgroundWorker.DoWork += (_, args) => args.Result = PerformSearch(searchTerm);
        backgroundWorker.RunWorkerCompleted += (_, args) =>
        {
            if (args.Error != null)
            {
                    
                string formattedException = $"That was an error using the SearchButton_Click method.\n\n" +
                                            $"Exception details: {args.Error.Message}";
                Exception ex = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));

                MessageBox.Show($"There was an error using the Global Search.\n\n" +
                                $"The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                if (args.Result is List<SearchResult> results && results.Any())
                {
                    foreach (var result in results)
                    {
                        _searchResults.Add(result);
                    }
                    LaunchButton.IsEnabled = true;
                }
                else
                {
                    string noresultsfound2 = (string)Application.Current.TryFindResource("Noresultsfound") ?? "No results found.";
                    _searchResults.Add(new SearchResult
                    {
                        FileName = noresultsfound2,
                        FolderName = "",
                        Size = 0
                    });
                }
            }

            if (!_closeTimer.IsEnabled)
            {
                _pleaseWaitWindow.Close();
            }
            else
            {
                _closeTimer.Tick += (_, _) => _pleaseWaitWindow.Close();
            }
        };

        _closeTimer.Start();
        backgroundWorker.RunWorkerAsync();
    }

    private List<SearchResult> PerformSearch(string searchTerm)
    {
        var results = new List<SearchResult>();

        var searchTerms = ParseSearchTerms(searchTerm);

        // Search in machine descriptions first
        var machinesWithMatchingDescriptions = _machines
            .Where(m => MatchesSearchQuery(m.Description.ToLower(), searchTerms))
            .Select(m => m.MachineName)
            .ToList();

        // Search in filenames within all systems
        foreach (var systemConfig in _systemConfigs)
        {
            string systemFolderPath = GetFullPath(systemConfig.SystemFolder);

            if (Directory.Exists(systemFolderPath))
            {
                var files = Directory.GetFiles(systemFolderPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => systemConfig.FileFormatsToSearch.Contains(Path.GetExtension(file).TrimStart('.').ToLower()))
                    .Where(file => MatchesSearchQuery(Path.GetFileName(file).ToLower(), searchTerms) ||
                                   (systemConfig.SystemIsMame && machinesWithMatchingDescriptions.Any(machineName => Path.GetFileNameWithoutExtension(file).Equals(machineName, StringComparison.OrdinalIgnoreCase))))
                    .Select(file => new SearchResult
                    {
                        FileName = Path.GetFileName(file),
                        FolderName = Path.GetDirectoryName(file)?.Split(Path.DirectorySeparatorChar).Last(),
                        FilePath = file,
                        Size = Math.Round(new FileInfo(file).Length / 1024.0, 2),
                        MachineName = GetMachineDescription(Path.GetFileNameWithoutExtension(file)),
                        SystemName = systemConfig.SystemName,
                        EmulatorConfig = systemConfig.Emulators.FirstOrDefault(),
                        CoverImage = GetCoverImagePath(systemConfig.SystemName, Path.GetFileName(file)) // Set cover image path
                    })
                    .ToList();

                results.AddRange(files);
            }
        }

        var scoredResults = ScoreResults(results, searchTerms);
        return scoredResults;
    }
        
    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        if (systemConfig == null)
        {
            return Path.Combine(baseDirectory, "images", "default.png");
        }

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var systemImageFolder = systemConfig.SystemImageFolder;
        
        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null) systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
        }
        
        var globalDirectory = Path.Combine(baseDirectory, "images", systemName);
        string[] imageExtensions = [".png", ".jpg", ".jpeg"];

        // Function to search for the image file
        bool TryFindImage(string directory, out string foundPath)
        {
            foreach (var extension in imageExtensions)
            {
                string imagePath = Path.Combine(directory, fileNameWithoutExtension + extension);
                if (File.Exists(imagePath))
                {
                    foundPath = imagePath;
                    return true;
                }
            }
            foundPath = null;
            return false;
        }

        // First try to find the image in the systemImageFolder
        if (TryFindImage(systemImageFolder, out var foundImagePath))
        {
            return foundImagePath;
        }
        // If not found, try the globalImageDirectory
        else if (TryFindImage(globalDirectory, out foundImagePath))
        {
            return foundImagePath;
        }
        else
        {
            return Path.Combine(baseDirectory, "images", "default.png");
        }
    }

    private List<SearchResult> ScoreResults(List<SearchResult> results, List<string> searchTerms)
    {
        foreach (var result in results)
        {
            result.Score = CalculateScore(result.FileName.ToLower(), searchTerms);
        }

        return results.OrderByDescending(r => r.Score).ThenBy(r => r.FileName).ToList();
    }

    private int CalculateScore(string text, List<string> searchTerms)
    {
        int score = 0;

        foreach (var term in searchTerms)
        {
            int index = text.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                score += 10;
                score += (text.Length - index);
            }
        }

        return score;
    }

    private bool MatchesSearchQuery(string text, List<string> searchTerms)
    {
        bool hasAnd = searchTerms.Contains("and");
        bool hasOr = searchTerms.Contains("or");

        if (hasAnd)
        {
            return searchTerms.Where(term => term != "and").All(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        if (hasOr)
        {
            return searchTerms.Where(term => term != "or").Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return searchTerms.All(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private List<string> ParseSearchTerms(string searchTerm)
    {
        var terms = new List<string>();
        var matches = Regex.Matches(searchTerm, @"[\""].+?[\""]|[^ ]+");

        foreach (Match match in matches)
        {
            terms.Add(match.Value.Trim('"').ToLower());
        }

        return terms;
    }

    private string GetMachineDescription(string fileNameWithoutExtension)
    {
        var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
        return machine?.Description ?? string.Empty;
    }

    private string GetFullPath(string path)
    {
        if (path.StartsWith(@".\"))
        {
            path = path.Substring(2);
        }

        if (Path.IsPathRooted(path))
        {
            return path;
        }

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }

    private async void LaunchGameFromSearchResult(string filePath, string systemName, SystemConfig.Emulator emulatorConfig)
    {
        try
        {
            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
                
            if (string.IsNullOrEmpty(systemName) || emulatorConfig == null)
            {
                string formattedException = $"That was an error trying to launch a game from the search result in the Global Search window.\n\n" +
                                            $"There is no System or Emulator associated with the game.";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                                        
                MessageBox.Show("There was an error launching the selected game.\n\n" +
                                "The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (systemConfig == null)
            {
                string formattedException = $"That was an error trying to launch a game from the search result in the Global Search window.\n\n" +
                                            $"System configuration not found for the selected game.";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);
                    
                MessageBox.Show("There was an error launching the selected game.\n\n" +
                                "The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _mockSystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
            _mockSystemComboBox.SelectedItem = systemConfig.SystemName;

            _mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();
            _mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

            await GameLauncher.HandleButtonClick(filePath, _mockEmulatorComboBox, _mockSystemComboBox, _systemConfigs, _settings, _mainWindow);
        }
        catch (Exception ex)
        {
            string formattedException = $"There was an error launching the game from the Global Search window.\n\n" +
                                        $"File Path: {filePath}\n" +
                                        $"System Name: {systemName}\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            MessageBox.Show($"There was an error launching the selected game.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
            {
                PlayClick.PlayClickSound();
                LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
            }
            else
            {
                MessageBox.Show("Please select a game to launch.",
                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"That was an error launching a game from the Global Search window.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            MessageBox.Show($"There was an error launching the selected game.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResultsDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedResult.FileName);
                string fileNameWithExtension = selectedResult.FileName;
                string filePath = selectedResult.FilePath;
                var systemConfig = _systemConfigs.FirstOrDefault(config =>
                    config.SystemName.Equals(selectedResult.SystemName, StringComparison.OrdinalIgnoreCase));

                if (systemConfig == null)
                {
                    string formattedException = "systemConfig is null in method ResultsDataGrid_MouseRightButtonUp.";
                    Exception ex = new(formattedException);
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    MessageBox.Show($"There was an error loading the systemConfig.\n\n" +
                                    $"The error was reported to the developer that will try to fix the issue.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return;
                }

                var contextMenu = new ContextMenu();

                // "Launch Selected Game" MenuItem
                var launchIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png")),
                    Width = 16,
                    Height = 16
                };
                string launchSelectedGame2 = (string)Application.Current.TryFindResource("LaunchSelectedGame") ?? "Launch Selected Game";
                var launchMenuItem = new MenuItem
                {
                    Header = launchSelectedGame2,
                    Icon = launchIcon
                };
                launchMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName,
                        selectedResult.EmulatorConfig);
                };

                // "Add To Favorites" MenuItem
                var addToFavoritesIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/heart.png")),
                    Width = 16,
                    Height = 16
                };
                string addToFavorites2 = (string)Application.Current.TryFindResource("AddToFavorites") ?? "Add To Favorites";
                var addToFavoritesMenuItem = new MenuItem
                {
                    Header = addToFavorites2,
                    Icon = addToFavoritesIcon
                };
                addToFavoritesMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    AddToFavorites(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Open Video Link" MenuItem
                var videoLinkIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
                    Width = 16,
                    Height = 16
                };
                string openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
                var videoLinkMenuItem = new MenuItem
                {
                    Header = openVideoLink2,
                    Icon = videoLinkIcon
                };
                videoLinkMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenVideoLink(selectedResult.SystemName, selectedResult.FileName, selectedResult.MachineName);
                };

                // "Open Info Link" MenuItem
                var infoLinkIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png")),
                    Width = 16,
                    Height = 16
                };
                string openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
                var infoLinkMenuItem = new MenuItem
                {
                    Header = openInfoLink2,
                    Icon = infoLinkIcon
                };
                infoLinkMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenInfoLink(selectedResult.SystemName, selectedResult.FileName, selectedResult.MachineName);
                };

                // "Open ROM History" MenuItem
                var openHistoryIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png",
                        UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                string openRomHistory2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
                var openHistoryMenuItem = new MenuItem
                {
                    Header = openRomHistory2,
                    Icon = openHistoryIcon
                };
                openHistoryMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenRomHistoryWindow(selectedResult.SystemName, fileNameWithoutExtension, systemConfig);
                };

                // "Cover" MenuItem
                var coverIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
                    Width = 16,
                    Height = 16
                };
                string cover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
                var coverMenuItem = new MenuItem
                {
                    Header = cover2,
                    Icon = coverIcon
                };
                coverMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenCover(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Title Snapshot" MenuItem
                var titleSnapshotIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
                    Width = 16,
                    Height = 16
                };
                string titleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
                var titleSnapshotMenuItem = new MenuItem
                {
                    Header = titleSnapshot2,
                    Icon = titleSnapshotIcon
                };
                titleSnapshotMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenTitleSnapshot(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Gameplay Snapshot" MenuItem
                var gameplaySnapshotIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
                    Width = 16,
                    Height = 16
                };
                string gameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
                var gameplaySnapshotMenuItem = new MenuItem
                {
                    Header = gameplaySnapshot2,
                    Icon = gameplaySnapshotIcon
                };
                gameplaySnapshotMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenGameplaySnapshot(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Cart" MenuItem
                var cartIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png")),
                    Width = 16,
                    Height = 16
                };
                string cart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
                var cartMenuItem = new MenuItem
                {
                    Header = cart2,
                    Icon = cartIcon
                };
                cartMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenCart(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Video" MenuItem
                var videoIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
                    Width = 16,
                    Height = 16
                };
                string video2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
                var videoMenuItem = new MenuItem
                {
                    Header = video2,
                    Icon = videoIcon
                };
                videoMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    PlayVideo(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Manual" MenuItem
                var manualIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png")),
                    Width = 16,
                    Height = 16
                };
                string manual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
                var manualMenuItem = new MenuItem
                {
                    Header = manual2,
                    Icon = manualIcon
                };
                manualMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenManual(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Walkthrough" MenuItem
                var walkthroughIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png")),
                    Width = 16,
                    Height = 16
                };
                string walkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
                var walkthroughMenuItem = new MenuItem
                {
                    Header = walkthrough2,
                    Icon = walkthroughIcon
                };
                walkthroughMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenWalkthrough(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Cabinet" MenuItem
                var cabinetIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png")),
                    Width = 16,
                    Height = 16
                };
                string cabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
                var cabinetMenuItem = new MenuItem
                {
                    Header = cabinet2,
                    Icon = cabinetIcon
                };
                cabinetMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenCabinet(selectedResult.SystemName, selectedResult.FileName);
                };

                // "Flyer" MenuItem
                var flyerIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png")),
                    Width = 16,
                    Height = 16
                };
                string flyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
                var flyerMenuItem = new MenuItem
                {
                    Header = flyer2,
                    Icon = flyerIcon
                };
                flyerMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenFlyer(selectedResult.SystemName, selectedResult.FileName);
                };

                // "PCB" MenuItem
                var pcbIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png")),
                    Width = 16,
                    Height = 16
                };
                string pCb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
                var pcbMenuItem = new MenuItem
                {
                    Header = pCb2,
                    Icon = pcbIcon
                };
                pcbMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    OpenPcb(selectedResult.SystemName, selectedResult.FileName);
                };

                // Take Screenshot Context Menu
                var takeScreenshotIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
                    Width = 16,
                    Height = 16
                };
                string takeScreenshot2 = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";
                var takeScreenshot = new MenuItem
                {
                    Header = takeScreenshot2,
                    Icon = takeScreenshotIcon
                };
                string thegamewilllaunchnow2 = (string)Application.Current.TryFindResource("Thegamewilllaunchnow") ?? "The game will launch now.";
                string setthegamewindowto2 = (string)Application.Current.TryFindResource("Setthegamewindowto") ?? "Set the game window to non-fullscreen. This is important.";
                string youshouldchangetheemulatorparameters2 = (string)Application.Current.TryFindResource("Youshouldchangetheemulatorparameters") ?? "You should change the emulator parameters to prevent the emulator from starting in fullscreen.";
                string aselectionwindowwillopenin2 = (string)Application.Current.TryFindResource("Aselectionwindowwillopenin") ?? "A selection window will open in";
                string allowingyoutochoosethe2 = (string)Application.Current.TryFindResource("allowingyoutochoosethe") ?? "allowing you to choose the desired window to capture.";
                string assoonasyouselectawindow2 = (string)Application.Current.TryFindResource("assoonasyouselectawindow") ?? "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.";
                takeScreenshot.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    MessageBox.Show($"{thegamewilllaunchnow2}\n\n{setthegamewindowto2}\n\n{youshouldchangetheemulatorparameters2}\n\n" +
                                    $"{aselectionwindowwillopenin2} 'Simple Launcher,' {allowingyoutochoosethe2}\n\n{assoonasyouselectawindow2}", takeScreenshot2, MessageBoxButton.OK, MessageBoxImage.Information);

                    _ = TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig.SystemName);
                    LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);

                };

                // Delete Game Context Menu
                var deleteGameIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/delete.png")),
                    Width = 16,
                    Height = 16
                };
                string deleteGame2 = (string)Application.Current.TryFindResource("DeleteGame") ?? "Delete Game";
                var deleteGame = new MenuItem
                {
                    Header = deleteGame2,
                    Icon = deleteGameIcon
                };
                string areyousureyouwanttodeletethefile2 = (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethefile") ?? "Are you sure you want to delete the file";
                string thisactionwilldelete2 = (string)Application.Current.TryFindResource("Thisactionwilldelete") ?? "This action will delete the file from the HDD and cannot be undone.";
                string confirmDeletion2 = (string)Application.Current.TryFindResource("ConfirmDeletion") ?? "Confirm Deletion";
                deleteGame.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    var result = MessageBox.Show($"{areyousureyouwanttodeletethefile2} \"{fileNameWithExtension}\"?\n\n{thisactionwilldelete2}",
                        confirmDeletion2, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        DeleteFile(filePath, fileNameWithExtension);
                        RemoveFromFavorites2(selectedResult.SystemName, fileNameWithExtension);
                    }
                };

                contextMenu.Items.Add(launchMenuItem);
                contextMenu.Items.Add(addToFavoritesMenuItem);
                contextMenu.Items.Add(videoLinkMenuItem);
                contextMenu.Items.Add(infoLinkMenuItem);
                contextMenu.Items.Add(openHistoryMenuItem);
                contextMenu.Items.Add(coverMenuItem);
                contextMenu.Items.Add(titleSnapshotMenuItem);
                contextMenu.Items.Add(gameplaySnapshotMenuItem);
                contextMenu.Items.Add(cartMenuItem);
                contextMenu.Items.Add(videoMenuItem);
                contextMenu.Items.Add(manualMenuItem);
                contextMenu.Items.Add(walkthroughMenuItem);
                contextMenu.Items.Add(cabinetMenuItem);
                contextMenu.Items.Add(flyerMenuItem);
                contextMenu.Items.Add(pcbMenuItem);
                contextMenu.Items.Add(takeScreenshot);
                contextMenu.Items.Add(deleteGame);
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            string formattedException =
                $"There was an error in the right-click context menu in the ResultsDataGrid_MouseRightButtonUp method.\n\n" +
                $"Exception type: {ex.GetType().Name}\n" +
                $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            MessageBox.Show("There was an error in the right-click context menu.\n\n" +
                            "The error was reported to the developer, who will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    private void AddToFavorites(string systemName, string fileNameWithoutExtension)
    {
        try
        {
            // Load existing favorites
            FavoritesConfig favorites = _favoritesManager.LoadFavorites();

            // Add the new favorite if it doesn't already exist
            if (!favorites.FavoriteList.Any(f => f.FileName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase)
                                                 && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase)))
            {
                favorites.FavoriteList.Add(new Favorite
                {
                    FileName = fileNameWithoutExtension,
                    SystemName = systemName
                });

                // Save the updated favorites list
                _favoritesManager.SaveFavorites(favorites);

                string hasbeenaddedtofavorites2 = (string)Application.Current.TryFindResource("hasbeenaddedtofavorites") ?? "has been added to favorites.";
                string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
                MessageBox.Show($"{fileNameWithoutExtension} {hasbeenaddedtofavorites2}",
                    success2, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                string isalreadyinfavorites2 = (string)Application.Current.TryFindResource("isalreadyinfavorites") ?? "is already in favorites.";
                string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
                MessageBox.Show($"{fileNameWithoutExtension} {isalreadyinfavorites2}", info2, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"An error occurred while adding game to favorites in the Global Search window.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"An error occurred while adding the game to the favorites.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void RemoveFromFavorites2(string systemName, string fileNameWithExtension)
    {
        try
        {
            FavoritesConfig favorites = _favoritesManager.LoadFavorites();

            var favoriteToRemove = favorites.FavoriteList.FirstOrDefault(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase)
                                                                              && f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (favoriteToRemove != null)
            {
                favorites.FavoriteList.Remove(favoriteToRemove);
                _favoritesManager.SaveFavorites(favorites);
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"An error occurred in the RemoveFromFavorites2 method in the GlobalSearch class.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }

    private void OpenVideoLink(string systemName, string fileName, string machineDescription = null)
    {
        var searchTerm =
            // Check if machineDescription is provided and not empty
            !string.IsNullOrEmpty(machineDescription) ? $"{machineDescription} {systemName}" : $"{Path.GetFileNameWithoutExtension(fileName)} {systemName}";

        string searchUrl = $"{_settings.VideoUrl}{Uri.EscapeDataString(searchTerm)}";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = searchUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            string formattedException = $"There was a problem opening the Video Link in the Global Search window.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"There was a problem opening the Video Link.\n\n" +
                            $"The problem was reported to the developer that will try to fix the issue.", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenInfoLink(string systemName, string fileName, string machineDescription = null)
    {
        var searchTerm =
            // Check if machineDescription is provided and not empty
            !string.IsNullOrEmpty(machineDescription) ? $"{machineDescription} {systemName}" : $"{Path.GetFileNameWithoutExtension(fileName)} {systemName}";

        string searchUrl = $"{_settings.InfoUrl}{Uri.EscapeDataString(searchTerm)}";

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = searchUrl,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            string formattedException = $"There was a problem opening the Info Link in the Global Search window.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"There was a problem opening the Info Link.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
        
    private void OpenRomHistoryWindow(string systemName, string fileNameWithoutExtension, SystemConfig systemConfig)
    {
        string romName = fileNameWithoutExtension.ToLowerInvariant();
           
        // Attempt to find a matching machine description
        string searchTerm = fileNameWithoutExtension;
        var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
        if (machine != null && !string.IsNullOrWhiteSpace(machine.Description))
        {
            searchTerm = machine.Description;
        }

        try
        {
            var historyWindow = new RomHistoryWindow(romName, systemName, searchTerm, systemConfig);
            historyWindow.Show();

        }
        catch (Exception ex)
        {
            string contextMessage = $"There was a problem opening the History window.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"There was a problem opening the History window.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenCover(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        if (systemConfig == null)
        {
            const string formattedException = $"System configuration not found for the selected game in the GlobalSearch window while using the OpenCover method.";
            Exception ex = new(formattedException);
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show("There was a problem opening the Cover Image for this game.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var systemImageFolder = systemConfig.SystemImageFolder;
        
        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null) systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
        }
        
        var globalImageDirectory = Path.Combine(baseDirectory, "images", systemName);
        string[] imageExtensions = [".png", ".jpg", ".jpeg"];

        // Function to search for the image file
        bool TryFindImage(string directory, out string foundPath)
        {
            foreach (var extension in imageExtensions)
            {
                string imagePath = Path.Combine(directory, fileNameWithoutExtension + extension);
                if (File.Exists(imagePath))
                {
                    foundPath = imagePath;
                    return true;
                }
            }
            foundPath = null;
            return false;
        }
        
        // First try to find the image in the systemImageFolder
        if (TryFindImage(systemImageFolder, out var foundImagePath))
        {
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(foundImagePath);
            imageViewerWindow.Show();
        }
        // If not found, try the globalImageDirectory
        else if (TryFindImage(globalImageDirectory, out foundImagePath))
        {
            var imageViewerWindow = new ImageViewerWindow();
            imageViewerWindow.LoadImage(foundImagePath);
            imageViewerWindow.Show();
        }
        else
        {
            string thereisnocoverfileassociated2 = (string)Application.Current.TryFindResource("Thereisnocoverfileassociated") ?? "There is no cover file associated with this game.";
            string covernotfound2 = (string)Application.Current.TryFindResource("Covernotfound") ?? "Cover not found";
            MessageBox.Show(thereisnocoverfileassociated2, covernotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void OpenTitleSnapshot(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string titleSnapshotDirectory = Path.Combine(baseDirectory, "title_snapshots", systemName);
        string[] titleSnapshotExtensions = [".png", ".jpg", ".jpeg"];
            
        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        foreach (var extension in titleSnapshotExtensions)
        {
            string titleSnapshotPath = Path.Combine(titleSnapshotDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(titleSnapshotPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(titleSnapshotPath);
                imageViewerWindow.Show();
                return;
            }
        }

        string thereisnotitlesnapshot2 = (string)Application.Current.TryFindResource("Thereisnotitlesnapshot") ?? "There is no title snapshot file associated with this game.";
        string titleSnapshotnotfound2 = (string)Application.Current.TryFindResource("TitleSnapshotnotfound") ?? "Title Snapshot not found";
        MessageBox.Show(thereisnotitlesnapshot2, titleSnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenGameplaySnapshot(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string gameplaySnapshotDirectory = Path.Combine(baseDirectory, "gameplay_snapshots", systemName);
        string[] gameplaySnapshotExtensions = [".png", ".jpg", ".jpeg"];
            
        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        foreach (var extension in gameplaySnapshotExtensions)
        {
            string gameplaySnapshotPath = Path.Combine(gameplaySnapshotDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(gameplaySnapshotPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(gameplaySnapshotPath);
                imageViewerWindow.Show();
                return;
            }
        }
        string thereisnogameplaysnapshot2 = (string)Application.Current.TryFindResource("Thereisnogameplaysnapshot") ?? "There is no gameplay snapshot file associated with this game.";
        string gameplaySnapshotnotfound2 = (string)Application.Current.TryFindResource("GameplaySnapshotnotfound") ?? "Gameplay Snapshot not found";
        MessageBox.Show(thereisnogameplaysnapshot2, gameplaySnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenCart(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string cartDirectory = Path.Combine(baseDirectory, "carts", systemName);
        string[] cartExtensions = [".png", ".jpg", ".jpeg"];

        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            
        foreach (var extension in cartExtensions)
        {
            string cartPath = Path.Combine(cartDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(cartPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(cartPath);
                imageViewerWindow.Show();
                return;
            }
        }
        
        string thereisnocartfile2 = (string)Application.Current.TryFindResource("Thereisnocartfile") ?? "There is no cart file associated with this game.";
        string cartnotfound2 = (string)Application.Current.TryFindResource("Cartnotfound") ?? "Cart not found";
        MessageBox.Show(thereisnocartfile2, cartnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PlayVideo(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string videoDirectory = Path.Combine(baseDirectory, "videos", systemName);
        string[] videoExtensions = [".mp4", ".avi", ".mkv"];
            
        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        foreach (var extension in videoExtensions)
        {
            string videoPath = Path.Combine(videoDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(videoPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = videoPath,
                    UseShellExecute = true
                });
                return;
            }
        }
        string thereisnovideofile2 = (string)Application.Current.TryFindResource("Thereisnovideofile") ?? "There is no video file associated with this game.";
        string videonotfound2 = (string)Application.Current.TryFindResource("Videonotfound") ?? "Video not found";
        MessageBox.Show(thereisnovideofile2, videonotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenManual(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string manualDirectory = Path.Combine(baseDirectory, "manuals", systemName);
        string[] manualExtensions = [".pdf"];
            
        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        foreach (var extension in manualExtensions)
        {
            string manualPath = Path.Combine(manualDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(manualPath))
            {
                try
                {
                    // Use the default PDF viewer to open the file
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = manualPath,
                        UseShellExecute = true
                    });
                    return;
                }
                catch (Exception ex)
                {
                    string formattedException = $"Failed to open the manual in the Global Search window.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                        
                    MessageBox.Show($"Failed to open the manual for this game.\n\n" +
                                    $"The error was reported to the developer that will try to fix the issue.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
        string thereisnomanual2 = (string)Application.Current.TryFindResource("Thereisnomanual") ?? "There is no manual associated with this file.";
        string manualNotFound2 = (string)Application.Current.TryFindResource("ManualNotFound") ?? "Manual Not Found";
        MessageBox.Show(thereisnomanual2, manualNotFound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenWalkthrough(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string walkthroughDirectory = Path.Combine(baseDirectory, "walkthrough", systemName);
        string[] walkthroughExtensions = [".pdf"];

        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            
        foreach (var extension in walkthroughExtensions)
        {
            string walkthroughPath = Path.Combine(walkthroughDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(walkthroughPath))
            {
                try
                {
                    // Use the default PDF viewer to open the file
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = walkthroughPath,
                        UseShellExecute = true
                    });
                    return;
                }
                catch (Exception ex)
                {
                    string formattedException = $"Failed to open the walkthrough file in the Global Search window.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                        
                    MessageBox.Show($"Failed to open the walkthrough file for this game.\n\n" +
                                    $"The error was reported to the developer that will try to fix the issue.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
        string thereisnowalkthrough2 = (string)Application.Current.TryFindResource("Thereisnowalkthrough") ?? "There is no walkthrough file associated with this game.";
        string walkthroughnotfound2 = (string)Application.Current.TryFindResource("Walkthroughnotfound") ?? "Walkthrough not found";
        MessageBox.Show(thereisnowalkthrough2, walkthroughnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenCabinet(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string cabinetDirectory = Path.Combine(baseDirectory, "cabinets", systemName);
        string[] cabinetExtensions = [".png", ".jpg", ".jpeg"];

        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        foreach (var extension in cabinetExtensions)
        {
            string cabinetPath = Path.Combine(cabinetDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(cabinetPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(cabinetPath);
                imageViewerWindow.Show();
                return;
            }
        }
        string thereisnocabinetfile2 = (string)Application.Current.TryFindResource("Thereisnocabinetfile") ?? "There is no cabinet file associated with this game.";
        string cabinetnotfound2 = (string)Application.Current.TryFindResource("Cabinetnotfound") ?? "Cabinet not found";
        MessageBox.Show(thereisnocabinetfile2, cabinetnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenFlyer(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string flyerDirectory = Path.Combine(baseDirectory, "flyers", systemName);
        string[] flyerExtensions = [".png", ".jpg", ".jpeg"];

        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            
        foreach (var extension in flyerExtensions)
        {
            string flyerPath = Path.Combine(flyerDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(flyerPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(flyerPath);
                imageViewerWindow.Show();
                return;
            }
        }
        string thereisnoflyer2 = (string)Application.Current.TryFindResource("Thereisnoflyer") ?? "There is no flyer file associated with this game.";
        string flyernotfound2 = (string)Application.Current.TryFindResource("Flyernotfound") ?? "Flyer not found";
        MessageBox.Show(thereisnoflyer2, flyernotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OpenPcb(string systemName, string fileName)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string pcbDirectory = Path.Combine(baseDirectory, "pcbs", systemName);
        string[] pcbExtensions = [".png", ".jpg", ".jpeg"];

        // Remove the original file extension
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        foreach (var extension in pcbExtensions)
        {
            string pcbPath = Path.Combine(pcbDirectory, fileNameWithoutExtension + extension);
            if (File.Exists(pcbPath))
            {
                var imageViewerWindow = new ImageViewerWindow();
                imageViewerWindow.LoadImage(pcbPath);
                imageViewerWindow.Show();
                return;
            }
        }
        string thereisnoPcBfile2 = (string)Application.Current.TryFindResource("ThereisnoPCBfile") ?? "There is no PCB file associated with this game.";
        string pCBnotfound2 = (string)Application.Current.TryFindResource("PCBnotfound") ?? "PCB not found";
        MessageBox.Show(thereisnoPcBfile2,pCBnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
        
    private async Task TakeScreenshotOfSelectedWindow(string fileNameWithoutExtension, string systemName)
    {
        try
        {
            // Clear the PreviewImage
            PreviewImage.Source = null;
            
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                string formattedException = $"System configuration not found for the selected game in the GlobalSearch window while using the TakeScreenshotOfSelectedWindow method.";
                Exception ex = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show("There was a problem getting the systemConfig for this game.\n\n" +
                                "The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
    
            string systemImageFolder = systemConfig.SystemImageFolder;
            if (string.IsNullOrEmpty(systemImageFolder))
            {
                systemImageFolder = Path.Combine(baseDirectory, "images", systemName);
                Directory.CreateDirectory(systemImageFolder);
            }
            
            // Wait for 4 seconds
            await Task.Delay(4000);
                
            // Get the list of open windows
            var openWindows = WindowManager.GetOpenWindows();

            // Show the selection dialog
            var dialog = new WindowSelectionDialog(openWindows);
            if (dialog.ShowDialog() != true || dialog.SelectedWindowHandle == IntPtr.Zero)
            {
                //MessageBox.Show("No window selected for the screenshot.", "Cancelled", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IntPtr hWnd = dialog.SelectedWindowHandle;
                
            WindowScreenshot.Rect rect;

            // Try to get the client area dimensions
            if (!WindowScreenshot.GetClientAreaRect(hWnd, out var clientRect))
            {
                // If the client area fails, fall back to the full window dimensions
                if (!WindowScreenshot.GetWindowRect(hWnd, out rect))
                {
                    throw new Exception("Failed to retrieve window dimensions.");
                }
            }
            else
            {
                // Successfully retrieved client area
                rect = clientRect;
            }

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            string screenshotPath = Path.Combine(systemImageFolder, $"{fileNameWithoutExtension}.png");

            // Capture the window into a bitmap
            using (var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(
                        new System.Drawing.Point(rect.Left, rect.Top),
                        System.Drawing.Point.Empty,
                        new System.Drawing.Size(width, height));
                }

                // Save the screenshot
                bitmap.Save(screenshotPath, ImageFormat.Png);
            }

            PlayClick.PlayShutterSound();
            
            // Wait
            await Task.Delay(1000);
            
            // Show the flash effect
            var flashWindow = new FlashOverlayWindow();
            await flashWindow.ShowFlashAsync();
                
            // Notify the user of success
            string screenshotsavedsuccessfullyat2 = (string)Application.Current.TryFindResource("Screenshotsavedsuccessfullyat") ?? "Screenshot saved successfully at:";
            string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
            MessageBox.Show($"{screenshotsavedsuccessfullyat2}\n\n{screenshotPath}",
                success2, MessageBoxButton.OK, MessageBoxImage.Information);

        }
        catch (Exception ex)
        {
            // Handle any errors
            MessageBox.Show($"Failed to save screenshot.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Send log to the developer
            string formattedException = $"Failed to save screenshot in the Global Search window.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
        
    private void DeleteFile(string filePath, string fileNameWithExtension)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                    
                PlayClick.PlayTrashSound();
                
                string thefile2 = (string)Application.Current.TryFindResource("Thefile") ?? "The file";
                string hasbeensuccessfullydeleted2 = (string)Application.Current.TryFindResource("hasbeensuccessfullydeleted") ?? "has been successfully deleted.";
                string fileDeleted2 = (string)Application.Current.TryFindResource("FileDeleted") ?? "File Deleted";
                MessageBox.Show($"{thefile2} \"{fileNameWithExtension}\" {hasbeensuccessfullydeleted2}",
                    fileDeleted2, MessageBoxButton.OK, MessageBoxImage.Information);

                // Redo the search after deletion
                SearchButton_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while trying to delete the file \"{fileNameWithExtension}\"." +
                                $"The error was reported to the developer that will try to fix the issue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Notify developer
                string errorMessage = $"An error occurred while trying to delete the file \"{fileNameWithExtension}\"." +
                                      $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
            }
        }
        else
        {
            MessageBox.Show($"The file \"{fileNameWithExtension}\" could not be found.",
                "File Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResultsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
            {
                PlayClick.PlayClickSound();
                LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"There was an error while using the method MouseDoubleClick in the Global Search window.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));

            MessageBox.Show($"The application could not launch this game.\n\n" +
                            $"The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchButton_Click(sender, e);
        }
    }

    public class SearchResult
    {
        public string FileName { get; init; }
        public string MachineName { get; init; }
        public string FolderName { get; init; }
        public string FilePath { get; init; }
        public double Size { get; set; }
        public string SystemName { get; init; }
        public SystemConfig.Emulator EmulatorConfig { get; init; }
        public int Score { get; set; }
        public string CoverImage { get; init; }
    }
        
    private void ResultsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(selectedResult.CoverImage, UriKind.Absolute);
            bitmap.EndInit();
            PreviewImage.Source = bitmap;
        }
        else
        {
            PreviewImage.Source = null;
        }
    }

    private void GlobalSearch_Closed(object sender, EventArgs e)
    {
        _searchResults = null;
    }
}
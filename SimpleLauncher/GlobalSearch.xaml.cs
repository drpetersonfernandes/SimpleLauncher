using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
    private readonly SettingsConfig _settings;
    private ObservableCollection<SearchResult> _searchResults;
    private PleaseWaitSearch _pleaseWaitWindow;
    private DispatcherTimer _closeTimer;
    private readonly FavoritesManager _favoritesManager = new();
    private readonly MainWindow _mainWindow;
    private readonly ComboBox _mockSystemComboBox = new();
    private readonly ComboBox _mockEmulatorComboBox = new();
    private readonly List<MameConfig> _machines;
    private readonly Dictionary<string, string> _mameLookup;
    private readonly WrapPanel _fakeGameFileGrid = new();
    private readonly Button _fakeButton = new();

    public GlobalSearch(List<SystemConfig> systemConfigs, List<MameConfig> machines, Dictionary<string,string> mameLookup , SettingsConfig settings, MainWindow mainWindow)
    {
        InitializeComponent();
        
        _systemConfigs = systemConfigs;
        _machines = machines;
        _mameLookup = mameLookup;
        _settings = settings;
        _searchResults = [];
        ResultsDataGrid.ItemsSource = _searchResults;
        _mainWindow = mainWindow;
        
        Closed += GlobalSearch_Closed;
            
        App.ApplyThemeToWindow(this);
    }

    private void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        string searchTerm = SearchTextBox.Text;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            // Notify user
            MessageBoxLibrary.PleaseEnterSearchTermMessageBox();

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
                // Notify developer
                string formattedException = $"That was an error using the SearchButton_Click.\n\n" +
                                            $"Error details: {args.Error.Message}";
                Exception ex = new(formattedException);
                LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

                // Notify user
                MessageBoxLibrary.GlobalSearchErrorMessageBox();
            }
            else
            {
                if (args.Result is List<SearchResult> results && results.Count != 0)
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

        foreach (var systemConfig in _systemConfigs)
        {
            string systemFolderPath = GetFullPath(systemConfig.SystemFolder);
            if (!Directory.Exists(systemFolderPath))
                continue;

            // Get all files matching the file's extensions for this system
            var files = Directory.GetFiles(systemFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => systemConfig.FileFormatsToSearch.Contains(Path.GetExtension(file).TrimStart('.').ToLower()));

            // If the system is MAME-based and the lookup is available, use it to filter files.
            if (systemConfig.SystemIsMame && _mameLookup != null)
            {
                files = files.Where(file =>
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                    // First check: Does the filename itself match the search terms?
                    if (MatchesSearchQuery(fileNameWithoutExtension.ToLower(), searchTerms))
                        return true;

                    // Second check: Look up the machine description using the dictionary.
                    if (_mameLookup.TryGetValue(fileNameWithoutExtension, out var description))
                    {
                        return MatchesSearchQuery(description.ToLower(), searchTerms);
                    }
                    return false;
                });
            }
            else
            {
                // For non-MAME systems, filter by filename.
                files = files.Where(file => MatchesSearchQuery(Path.GetFileName(file).ToLower(), searchTerms));
            }

            // Map each file into a SearchResult object.
            var fileResults = files.Select(file => new SearchResult
            {
                FileName = Path.GetFileName(file),
                FolderName = Path.GetDirectoryName(file)?.Split(Path.DirectorySeparatorChar).Last(),
                FilePath = file,
                Size = Math.Round(new FileInfo(file).Length / 1024.0, 2),

                MachineName = GetMachineDescription(Path.GetFileNameWithoutExtension(file)),
                SystemName = systemConfig.SystemName,
                EmulatorConfig = systemConfig.Emulators.FirstOrDefault(),
                CoverImage = GetCoverImagePath(systemConfig.SystemName, Path.GetFileName(file))
            }).ToList();

            results.AddRange(fileResults);
        }

        // Score and order the results before returning.
        var scoredResults = ScoreResults(results, searchTerms);
        return scoredResults;
        
        string GetMachineDescription(string fileNameWithoutExtension)
        {
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            return machine?.Description ?? string.Empty;
        }
        
        string GetFullPath(string path)
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

        // Search for the image file
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

    private static bool MatchesSearchQuery(string text, List<string> searchTerms)
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

    private static List<string> ParseSearchTerms(string searchTerm)
    {
        var terms = new List<string>();
        var matches = Regex.Matches(searchTerm, @"[\""].+?[\""]|[^ ]+");

        foreach (Match match in matches)
        {
            terms.Add(match.Value.Trim('"').ToLower());
        }

        return terms;
    }

    private async void LaunchGameFromSearchResult(string filePath, string systemName, SystemConfig.Emulator emulatorConfig)
    {
        try
        {
            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
                
            if (string.IsNullOrEmpty(systemName) || emulatorConfig == null)
            {
                // Notify developer
                string formattedException = "That was an error trying to launch a game from the search result.\n\n" +
                                            "systemName or emulatorConfig is null or empty.";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox();
                
                return;
            }

            if (systemConfig == null)
            {
                // Notify developer
                string formattedException = "That was an error trying to launch a game from the search result.\n\n" +
                                            "systemConfig is null.";
                Exception exception = new(formattedException);
                await LogErrors.LogErrorAsync(exception, formattedException);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox();
                
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
            // Notify developer
            string formattedException = $"There was an error launching the game.\n\n" +
                                        $"File Path: {filePath}\n" +
                                        $"System Name: {systemName}\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox();
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
                // Notify user
                MessageBoxLibrary.SelectAGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"That was an error launching a game.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox();
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
                    // Notify developer
                    string formattedException = "systemConfig is null.";
                    Exception ex = new(formattedException);
                    LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

                    // Notify user
                    MessageBoxLibrary.ErrorLoadingSystemConfigMessageBox();

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
                    RightClickContextMenu.AddToFavorites(selectedResult.SystemName, selectedResult.FileName, _favoritesManager, _fakeGameFileGrid, _mainWindow);
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
                    RightClickContextMenu.OpenVideoLink(selectedResult.SystemName, selectedResult.FileName, _machines, _settings);
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
                    RightClickContextMenu.OpenInfoLink(selectedResult.SystemName, selectedResult.FileName, _machines, _settings);
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
                    RightClickContextMenu.OpenHistoryWindow(selectedResult.SystemName, fileNameWithoutExtension, systemConfig, _machines);
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
                    RightClickContextMenu.OpenCover(selectedResult.SystemName, selectedResult.FileName, systemConfig);
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
                    RightClickContextMenu.OpenTitleSnapshot(selectedResult.SystemName, selectedResult.FileName);
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
                    RightClickContextMenu.OpenGameplaySnapshot(selectedResult.SystemName, selectedResult.FileName);
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
                    RightClickContextMenu.OpenCart(selectedResult.SystemName, selectedResult.FileName);
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
                    RightClickContextMenu.PlayVideo(selectedResult.SystemName, selectedResult.FileName);
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
                    RightClickContextMenu.OpenManual(selectedResult.SystemName, selectedResult.FileName);
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
                    RightClickContextMenu.OpenWalkthrough(selectedResult.SystemName, selectedResult.FileName);
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
                    RightClickContextMenu.OpenCabinet(selectedResult.SystemName, selectedResult.FileName);
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
                    RightClickContextMenu.OpenFlyer(selectedResult.SystemName, selectedResult.FileName);
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
                    RightClickContextMenu.OpenPcb(selectedResult.SystemName, selectedResult.FileName);
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
                takeScreenshot.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();

                    // Notify user
                    MessageBoxLibrary.TakeScreenShotMessageBox();

                    _ = RightClickContextMenu.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig, _fakeButton, _mainWindow);
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
                deleteGame.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();

                    // Notify user
                    DoYouWanToDeleteMessageBox();
                    void DoYouWanToDeleteMessageBox()
                    {
                        var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

                        if (result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, _fakeButton, _fakeGameFileGrid, _mainWindow);
                            }
                            catch (Exception ex)
                            {
                                // Notify developer
                                string formattedException = $"Error deleting the file.\n\n" +
                                                            $"Exception type: {ex.GetType().Name}\n" +
                                                            $"Exception details: {ex.Message}";
                                LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));
                                
                                // Notify user
                                MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                            }
                            RightClickContextMenu.RemoveFromFavorites(selectedResult.SystemName, fileNameWithExtension, _favoritesManager, _fakeGameFileGrid, _mainWindow);
                        }
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
            // Notify developer
            string formattedException =
                $"There was an error in the right-click context menu.\n\n" +
                $"Exception type: {ex.GetType().Name}\n" +
                $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorRightClickContextMenuMessageBox();
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
            // Notify developer
            string formattedException = $"There was an error while using the method MouseDoubleClick.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox();
        }
    }

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchButton_Click(sender, e);
        }
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
    
}
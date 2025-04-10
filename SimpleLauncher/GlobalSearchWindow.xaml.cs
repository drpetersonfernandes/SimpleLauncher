using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

public partial class GlobalSearchWindow
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");
    private readonly List<SystemConfig> _systemConfigs;
    private readonly SettingsManager _settings;
    private ObservableCollection<SearchResult> _searchResults;
    private PleaseWaitSearchWindow _pleaseWaitWindow;
    private readonly MainWindow _mainWindow;
    private readonly List<MameManager> _machines;
    private readonly Dictionary<string, string> _mameLookup;
    private readonly FavoritesManager _favoritesManager;

    private readonly WrapPanel _fakeGameFileGrid = new();
    private readonly Button _fakeButton = new();
    private readonly ComboBox _mockSystemComboBox = new();
    private readonly ComboBox _mockEmulatorComboBox = new();

    public GlobalSearchWindow(List<SystemConfig> systemConfigs, List<MameManager> machines, Dictionary<string, string> mameLookup, SettingsManager settings, FavoritesManager favoritesManager, MainWindow mainWindow)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Closed += GlobalSearch_Closed;

        _systemConfigs = systemConfigs;
        _machines = machines;
        _mameLookup = mameLookup;
        _settings = settings;
        _favoritesManager = favoritesManager;
        _searchResults = [];
        ResultsDataGrid.ItemsSource = _searchResults;
        _mainWindow = mainWindow;
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        var searchTerm = SearchTextBox.Text;
        if (CheckIfSearchTermIsEmpty(searchTerm)) return;

        LaunchButton.IsEnabled = false;
        _searchResults.Clear();

        // Show a "Please Wait" window.
        _pleaseWaitWindow = new PleaseWaitSearchWindow
        {
            Owner = this
        };
        _pleaseWaitWindow.Show();

        try
        {
            var results = await Task.Run(() => PerformSearch(searchTerm));

            if (results != null && results.Count != 0)
            {
                foreach (var result in results)
                {
                    _searchResults.Add(result);
                }

                LaunchButton.IsEnabled = true;
            }
            else
            {
                var noresultsfound2 = (string)Application.Current.TryFindResource("Noresultsfound") ??
                                      "No results found.";
                _searchResults.Add(new SearchResult
                {
                    FileName = noresultsfound2,
                    FolderName = "",
                    Size = 0
                });
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "That was an error using the SearchButton_Click.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.GlobalSearchErrorMessageBox();
        }
        finally
        {
            // Close the "Please Wait" window
            _pleaseWaitWindow.Close();
        }
    }

    private List<SearchResult> PerformSearch(string searchTerm)
    {
        var results = new List<SearchResult>();
        var searchTerms = ParseSearchTerms(searchTerm);

        foreach (var systemConfig in _systemConfigs)
        {
            var systemFolderPath = GetFullPath(systemConfig.SystemFolder);
            if (!Directory.Exists(systemFolderPath))
                continue;

            // Get all files matching the file's extensions for this system
            var files = Directory.GetFiles(systemFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => systemConfig.FileFormatsToSearch.Contains(Path.GetExtension(file).TrimStart('.').ToLowerInvariant()));

            // If the system is MAME-based and the lookup is available, use it to filter files.
            if (systemConfig.SystemIsMame && _mameLookup != null)
            {
                files = files.Where(file =>
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                    // First check: Does the filename itself match the search terms?
                    if (MatchesSearchQuery(fileNameWithoutExtension.ToLowerInvariant(), searchTerms))
                        return true;

                    // Second check: Look up the machine description using the dictionary.
                    if (_mameLookup.TryGetValue(fileNameWithoutExtension, out var description))
                    {
                        return MatchesSearchQuery(description.ToLowerInvariant(), searchTerms);
                    }

                    return false;
                });
            }
            else
            {
                // For non-MAME systems, filter by filename.
                files = files.Where(file => MatchesSearchQuery(Path.GetFileName(file).ToLowerInvariant(), searchTerms));
            }

            // Map each file into a SearchResult object.
            var fileResults = files.Select(file => new SearchResult
            {
                FileName = Path.GetFileNameWithoutExtension(file),
                FileNameWithExtension = Path.GetFileName(file),
                FolderName = Path.GetDirectoryName(file)?.Split(Path.DirectorySeparatorChar).Last(),
                FilePath = file,
                Size = Math.Round(new FileInfo(file).Length / 1024.0, 2),

                MachineName = GetMachineDescription(Path.GetFileNameWithoutExtension(file)),
                SystemName = systemConfig.SystemName,
                EmulatorConfig = systemConfig.Emulators.FirstOrDefault(),
                CoverImage = FindCoverImage.FindCoverImagePath(Path.GetFileNameWithoutExtension(file), systemConfig.SystemName, systemConfig)
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
            if (path.StartsWith(@".\", StringComparison.Ordinal))
            {
                path = path.Substring(2);
            }

            return Path.IsPathRooted(path) ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }
    }

    private static List<SearchResult> ScoreResults(List<SearchResult> results, List<string> searchTerms)
    {
        foreach (var result in results)
        {
            result.Score = CalculateScore(result.FileName.ToLowerInvariant(), searchTerms);
        }

        return results.OrderByDescending(r => r.Score).ThenBy(r => r.FileName).ToList();
    }

    private static int CalculateScore(string text, List<string> searchTerms)
    {
        var score = 0;

        foreach (var index in searchTerms.Select(term => text.IndexOf(term, StringComparison.OrdinalIgnoreCase)).Where(index => index >= 0))
        {
            score += 10;
            score += (text.Length - index);
        }

        return score;
    }

    private static bool MatchesSearchQuery(string text, List<string> searchTerms)
    {
        var hasAnd = searchTerms.Contains("and");
        var hasOr = searchTerms.Contains("or");

        if (hasAnd)
        {
            return searchTerms.Where(term => term != "and").All(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return hasOr ? searchTerms.Where(term => term != "or").Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase)) : searchTerms.All(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> ParseSearchTerms(string searchTerm)
    {
        var terms = new List<string>();
        var matches = MyRegex().Matches(searchTerm);

        foreach (Match match in matches)
        {
            terms.Add(match.Value.Trim('"').ToLowerInvariant());
        }

        return terms;
    }

    private async void LaunchGameFromSearchResult(string filePath, string systemName, SystemConfig.Emulator emulatorConfig)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                // Notify developer
                const string contextMessage = "filePath is null or empty.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            if (string.IsNullOrEmpty(systemName))
            {
                // Notify developer
                const string contextMessage = "systemName is null or empty.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            if (emulatorConfig == null)
            {
                // Notify developer
                const string contextMessage = "emulatorConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (systemConfig == null)
            {
                // Notify developer
                const string contextMessage = "systemConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

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
            var contextMessage = $"There was an error launching the game.\n" +
                                 $"File Path: {filePath}\n" +
                                 $"System Name: {systemName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);
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
            const string contextMessage = "That was an error launching a game.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);
        }
    }

    private void GlobalSearchRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult) return;
            var fileNameWithoutExtension = selectedResult.FileName;
            var fileNameWithExtension = selectedResult.FileNameWithExtension;
            var filePath = selectedResult.FilePath;
            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(selectedResult.SystemName, StringComparison.OrdinalIgnoreCase));

            if (CheckSystemConfig(systemConfig)) return;

            AddRightClickContextMenuGlobalSearchWindow(selectedResult, fileNameWithoutExtension, systemConfig, fileNameWithExtension, filePath);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error in the right-click context menu.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorRightClickContextMenuMessageBox();
        }
    }

    private void AddRightClickContextMenuGlobalSearchWindow(SearchResult selectedResult, string fileNameWithoutExtension,
        SystemConfig systemConfig, string fileNameWithExtension, string filePath)
    {
        var contextMenu = new ContextMenu();

        // "Launch Selected Game" MenuItem
        var launchIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png")),
            Width = 16,
            Height = 16
        };
        var launchSelectedGame2 = (string)Application.Current.TryFindResource("LaunchSelectedGame") ?? "Launch Selected Game";
        var launchMenuItem = new MenuItem
        {
            Header = launchSelectedGame2,
            Icon = launchIcon
        };
        launchMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
        };

        // "Add To Favorites" MenuItem
        var addToFavoritesIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/heart.png")),
            Width = 16,
            Height = 16
        };
        var addToFavorites2 = (string)Application.Current.TryFindResource("AddToFavorites") ?? "Add To Favorites";
        var addToFavoritesMenuItem = new MenuItem
        {
            Header = addToFavorites2,
            Icon = addToFavoritesIcon
        };
        addToFavoritesMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.AddToFavorites(selectedResult.SystemName, selectedResult.FileNameWithExtension, _favoritesManager, _fakeGameFileGrid, _mainWindow);
        };

        // "Open Video Link" MenuItem
        var videoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
        var videoLinkMenuItem = new MenuItem
        {
            Header = openVideoLink2,
            Icon = videoLinkIcon
        };
        videoLinkMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenVideoLink(selectedResult.SystemName, fileNameWithoutExtension, _machines, _settings);
        };

        // "Open Info Link" MenuItem
        var infoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png")),
            Width = 16,
            Height = 16
        };
        var openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
        var infoLinkMenuItem = new MenuItem
        {
            Header = openInfoLink2,
            Icon = infoLinkIcon
        };
        infoLinkMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenInfoLink(selectedResult.SystemName, fileNameWithoutExtension, _machines, _settings);
        };

        // "Open ROM History" MenuItem
        var openHistoryIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png",
                UriKind.RelativeOrAbsolute)),
            Width = 16,
            Height = 16
        };
        var openRomHistory2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
        var openHistoryMenuItem = new MenuItem
        {
            Header = openRomHistory2,
            Icon = openHistoryIcon
        };
        openHistoryMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenRomHistoryWindow(selectedResult.SystemName, fileNameWithoutExtension, systemConfig, _machines);
        };

        // "Cover" MenuItem
        var coverIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
            Width = 16,
            Height = 16
        };
        var cover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
        var coverMenuItem = new MenuItem
        {
            Header = cover2,
            Icon = coverIcon
        };
        coverMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenCover(selectedResult.SystemName, fileNameWithoutExtension, systemConfig);
        };

        // "Title Snapshot" MenuItem
        var titleSnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var titleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
        var titleSnapshotMenuItem = new MenuItem
        {
            Header = titleSnapshot2,
            Icon = titleSnapshotIcon
        };
        titleSnapshotMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenTitleSnapshot(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Gameplay Snapshot" MenuItem
        var gameplaySnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var gameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
        var gameplaySnapshotMenuItem = new MenuItem
        {
            Header = gameplaySnapshot2,
            Icon = gameplaySnapshotIcon
        };
        gameplaySnapshotMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenGameplaySnapshot(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Cart" MenuItem
        var cartIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png")),
            Width = 16,
            Height = 16
        };
        var cart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
        var cartMenuItem = new MenuItem
        {
            Header = cart2,
            Icon = cartIcon
        };
        cartMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenCart(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Video" MenuItem
        var videoIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var video2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
        var videoMenuItem = new MenuItem
        {
            Header = video2,
            Icon = videoIcon
        };
        videoMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.PlayVideo(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Manual" MenuItem
        var manualIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png")),
            Width = 16,
            Height = 16
        };
        var manual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
        var manualMenuItem = new MenuItem
        {
            Header = manual2,
            Icon = manualIcon
        };
        manualMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenManual(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Walkthrough" MenuItem
        var walkthroughIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png")),
            Width = 16,
            Height = 16
        };
        var walkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
        var walkthroughMenuItem = new MenuItem
        {
            Header = walkthrough2,
            Icon = walkthroughIcon
        };
        walkthroughMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenWalkthrough(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Cabinet" MenuItem
        var cabinetIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png")),
            Width = 16,
            Height = 16
        };
        var cabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
        var cabinetMenuItem = new MenuItem
        {
            Header = cabinet2,
            Icon = cabinetIcon
        };
        cabinetMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenCabinet(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Flyer" MenuItem
        var flyerIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png")),
            Width = 16,
            Height = 16
        };
        var flyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
        var flyerMenuItem = new MenuItem
        {
            Header = flyer2,
            Icon = flyerIcon
        };
        flyerMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenFlyer(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "PCB" MenuItem
        var pcbIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png")),
            Width = 16,
            Height = 16
        };
        var pCb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
        var pcbMenuItem = new MenuItem
        {
            Header = pCb2,
            Icon = pcbIcon
        };
        pcbMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            RightClickContextMenu.OpenPcb(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // Take Screenshot Context Menu
        var takeScreenshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var takeScreenshot2 = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";
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
        var deleteGame2 = (string)Application.Current.TryFindResource("DeleteGame") ?? "Delete Game";
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
            return;

            void DoYouWanToDeleteMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

                if (result != MessageBoxResult.Yes) return;
                try
                {
                    RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, _fakeButton, _fakeGameFileGrid, _mainWindow);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error deleting the file.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                }

                RightClickContextMenu.RemoveFromFavorites(selectedResult.SystemName, fileNameWithExtension, _favoritesManager, _fakeGameFileGrid, _mainWindow);
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

    private void ResultsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult) return;
            PlayClick.PlayClickSound();
            LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error while using the method MouseDoubleClick.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void SearchWhenPressEnterKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchButton_Click(sender, e);
        }
    }

    private void ActionsWhenUserSelectAResultItem(object sender, SelectionChangedEventArgs e)
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
        // Empty results
        _searchResults = null;
    }

    public class SearchResult
    {
        public string FileName { get; init; }
        public string FileNameWithExtension { get; init; }
        public string MachineName { get; init; }
        public string FolderName { get; init; }
        public string FilePath { get; init; }
        public double Size { get; set; }
        public string SystemName { get; init; }
        public SystemConfig.Emulator EmulatorConfig { get; init; }
        public int Score { get; set; }
        public string CoverImage { get; init; }
        public string DefaultEmulator => EmulatorConfig?.EmulatorName ?? "No Default Emulator";
    }

    private static bool CheckSystemConfig(SystemConfig systemConfig)
    {
        if (systemConfig != null) return false;

        // Notify developer
        const string contextMessage = "systemConfig is null.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.ErrorLoadingSystemConfigMessageBox();

        return true;
    }

    private static bool CheckIfSearchTermIsEmpty(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm)) return false;

        // Notify user
        MessageBoxLibrary.PleaseEnterSearchTermMessageBox();

        return true;
    }

    [GeneratedRegex(@"[\""].+?[\""]|[^ ]+")]
    private static partial Regex MyRegex();
}
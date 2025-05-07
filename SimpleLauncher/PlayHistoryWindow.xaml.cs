using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

public partial class PlayHistoryWindow
{
    private const string TimeFormat = "HH:mm:ss";
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");
    private readonly PlayHistoryManager _playHistoryManager;
    private ObservableCollection<PlayHistoryItem> _playHistoryList;
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemConfigs;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;
    private readonly FavoritesManager _favoritesManager;

    private readonly Button _fakebutton = new();
    private readonly WrapPanel _fakeGameFileGrid = new();

    public PlayHistoryWindow(List<SystemManager> systemConfigs, List<MameManager> machines, SettingsManager settings, FavoritesManager favoritesManager, PlayHistoryManager playHistoryManager, MainWindow mainWindow)
    {
        InitializeComponent();

        _systemConfigs = systemConfigs;
        _machines = machines;
        _settings = settings;
        _favoritesManager = favoritesManager;
        _playHistoryManager = playHistoryManager;
        _mainWindow = mainWindow;

        App.ApplyThemeToWindow(this);
        LoadPlayHistory();
    }

    private void LoadPlayHistory()
    {
        var playHistoryConfig = PlayHistoryManager.LoadPlayHistory();
        _playHistoryList = new ObservableCollection<PlayHistoryItem>();
        foreach (var historyItem in playHistoryConfig.PlayHistoryList)
        {
            // Find machine description if available
            var machine = _machines.FirstOrDefault(m =>
                m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItem.FileName), StringComparison.OrdinalIgnoreCase));
            var machineDescription = machine?.Description ?? string.Empty;

            // Retrieve the system configuration for the history item
            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(historyItem.SystemName, StringComparison.OrdinalIgnoreCase));

            // Get the default emulator. The first one in the list
            var defaultEmulator = systemConfig?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

            var playHistoryItem = new PlayHistoryItem
            {
                FileName = historyItem.FileName,
                SystemName = historyItem.SystemName,
                TotalPlayTime = historyItem.TotalPlayTime,
                TimesPlayed = historyItem.TimesPlayed,
                LastPlayDate = historyItem.LastPlayDate,
                LastPlayTime = historyItem.LastPlayTime,
                MachineDescription = machineDescription,
                DefaultEmulator = defaultEmulator,
                CoverImage = GetCoverImagePath(historyItem.SystemName, historyItem.FileName)
            };
            _playHistoryList.Add(playHistoryItem);
        }

        // Sort the list by date and time
        SortByDateSafely();

        // Add to the DataGrid
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private static DateTime TryParseDateTime(string dateStr, string timeStr)
    {
        try
        {
            // First, try to parse using current culture (most likely to succeed)
            if (DateTime.TryParse($"{dateStr} {timeStr}", out var result))
            {
                return result;
            }

            // If that fails, try with invariant culture
            if (DateTime.TryParse($"{dateStr} {timeStr}", InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            // As a fallback, try common formats
            string[] dateFormats = ["MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d", "D"];
            foreach (var df in dateFormats)
            {
                if (DateTime.TryParseExact($"{dateStr} {timeStr}",
                        $"{df} {TimeFormat}", InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }
            }

            // If all parsing attempts fail, return DateTime.MinValue
            // This will put unparseable dates at the end of the sorted list
            return DateTime.MinValue;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error parsing date and time.\n" +
                                            $"dateStr: {dateStr}\n" +
                                            $"timeStr: {timeStr}");

            // In case of any exception, return a reasonable default
            return DateTime.MinValue;
        }
    }

    private void SortByDateSafely()
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item =>
                TryParseDateTime(item.LastPlayDate, item.LastPlayTime))
        );
        _playHistoryList = sorted;
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var defaultCoverImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemConfig == null)
        {
            return defaultCoverImagePath;
        }
        else
        {
            // Use FindCoverImage which already handles system-specific paths and fuzzy matching
            return FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemConfig);
        }
    }

    private void RemoveHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
        {
            _playHistoryList.Remove(selectedItem);
            _playHistoryManager.PlayHistoryList = _playHistoryList; // Keep the instance in sync
            _playHistoryManager.SavePlayHistory(); // Save using the existing instance

            PlayClick.PlayTrashSound();
            PreviewImage.Source = null;
        }
        else
        {
            // Notify the user to select a history item first
            MessageBoxLibrary.SelectAHistoryItemToRemoveMessageBox();
        }
    }

    private void RemoveAllHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        // Ask for confirmation before removing all items
        var result = MessageBoxLibrary.ReallyWantToRemoveAllPlayHistoryMessageBox();

        if (result != MessageBoxResult.Yes) return;

        // Clear all items from the collection
        _playHistoryList.Clear();

        // Update the manager and save changes
        _playHistoryManager.PlayHistoryList = _playHistoryList;
        _playHistoryManager.SavePlayHistory();

        // Play sound effect
        PlayClick.PlayTrashSound();

        // Clear preview image
        PreviewImage.Source = null;
    }

    private void AddRightClickContextMenuPlayHistoryWindow(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem) return;

            // Check filename
            if (selectedItem.FileName == null)
            {
                // Notify developer
                const string contextMessage = "History item filename is null";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            // Check systemConfig
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                // Notify developer
                const string contextMessage = "systemConfig is null";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var fileNameWithExtension = selectedItem.FileName;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedItem.FileName);
            var filePath = PathHelper.CombineAndResolveRelativeToCurrentDirectory(systemConfig.SystemFolder, selectedItem.FileName);

            var contextMenu = new ContextMenu();

            // "Launch Selected Game" MenuItem
            var launchIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png", UriKind.RelativeOrAbsolute)),
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
                _ = LaunchGameFromHistory(fileNameWithExtension, selectedItem.SystemName);
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
                RightClickContextMenu.AddToFavorites(selectedItem.SystemName, fileNameWithExtension, _favoritesManager, _fakeGameFileGrid, _mainWindow);
            };

            // "Open Video Link" MenuItem
            var videoLinkIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png", UriKind.RelativeOrAbsolute)),
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
                RightClickContextMenu.OpenVideoLink(selectedItem.SystemName, fileNameWithoutExtension, _machines, _settings);
            };

            // "Open Info Link" MenuItem
            var infoLinkIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png", UriKind.RelativeOrAbsolute)),
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
                RightClickContextMenu.OpenInfoLink(selectedItem.SystemName, fileNameWithoutExtension, _machines, _settings);
            };

            // Open ROM History Context Menu
            var openHistoryIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png", UriKind.RelativeOrAbsolute)),
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
                RightClickContextMenu.OpenRomHistoryWindow(selectedItem.SystemName, fileNameWithoutExtension, systemConfig, _machines);
            };

            // Open Cover Context Menu
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
                RightClickContextMenu.OpenCover(selectedItem.SystemName, fileNameWithoutExtension, systemConfig);
            };

            // Open Title Snapshot Context Menu
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
                RightClickContextMenu.OpenTitleSnapshot(selectedItem.SystemName, fileNameWithoutExtension);
            };

            // Open Gameplay Snapshot Context Menu
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
                RightClickContextMenu.OpenGameplaySnapshot(selectedItem.SystemName, fileNameWithoutExtension);
            };

            // Open Cart Context Menu
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
                RightClickContextMenu.OpenCart(selectedItem.SystemName, fileNameWithoutExtension);
            };

            // Open Video Context Menu
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
                RightClickContextMenu.PlayVideo(selectedItem.SystemName, fileNameWithoutExtension);
            };

            // Open Manual Context Menu
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
                RightClickContextMenu.OpenManual(selectedItem.SystemName, fileNameWithoutExtension);
            };

            // Open Walkthrough Context Menu
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
                RightClickContextMenu.OpenWalkthrough(selectedItem.SystemName, fileNameWithoutExtension);
            };

            // Open Cabinet Context Menu
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
                RightClickContextMenu.OpenCabinet(selectedItem.SystemName, fileNameWithoutExtension);
            };

            // Open Flyer Context Menu
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
                RightClickContextMenu.OpenFlyer(selectedItem.SystemName, fileNameWithoutExtension);
            };

            // Open PCB Context Menu
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
                RightClickContextMenu.OpenPcb(selectedItem.SystemName, fileNameWithoutExtension);
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

                _ = RightClickContextMenu.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemConfig, _fakebutton, _mainWindow);
                _ = LaunchGameFromHistory(fileNameWithExtension, selectedItem.SystemName);
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
            deleteGame.Click += async (_, _) =>
            {
                PlayClick.PlayClickSound();

                // Notify user
                await DoYouWanToDeleteMessageBox();
                return;

                async Task DoYouWanToDeleteMessageBox()
                {
                    var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

                    if (result != MessageBoxResult.Yes) return;

                    try
                    {
                        await RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, _mainWindow);
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        const string contextMessage = "Error deleting the file.";
                        _ = LogErrors.LogErrorAsync(ex, contextMessage);

                        // Notify user
                        MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                    }

                    RightClickContextMenu.RemoveFromFavorites(selectedItem.SystemName, fileNameWithExtension, _favoritesManager, _fakeGameFileGrid, _mainWindow);
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
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error in the right-click context menu.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private async void LaunchGame_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
            {
                PlayClick.PlayClickSound();
                await LaunchGameFromHistory(selectedItem.FileName, selectedItem.SystemName);
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
            const string contextMessage = "Error in the LaunchGame_Click method.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private async Task LaunchGameFromHistory(string fileName, string systemName)
    {
        try
        {
            // Check systemConfig
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                // Notify developer
                const string contextMessage = "systemConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            // Check emulatorConfig
            var emulatorConfig = systemConfig.Emulators.FirstOrDefault();
            if (emulatorConfig == null)
            {
                // Notify developer
                const string contextMessage = "emulatorConfig is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var fullPath = PathHelper.ResolveRelativeToAppDirectory(PathHelper.CombineAndResolveRelativeToCurrentDirectory(systemConfig.SystemFolder, fileName));
            // Check if the file exists
            if (!File.Exists(fullPath))
            {
                // Auto remove the history item from the list since the file no longer exists
                var itemToRemove = _playHistoryList.FirstOrDefault(item => item.FileName == fileName && item.SystemName == systemName);
                if (itemToRemove != null)
                {
                    _playHistoryList.Remove(itemToRemove);
                    _playHistoryManager.PlayHistoryList = _playHistoryList;
                    _playHistoryManager.SavePlayHistory();
                }

                // Notify developer
                var contextMessage = $"History item file does not exist: {fullPath}";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.GameFileDoesNotExistMessageBox();
            }
            else // File exists
            {
                var mockSystemComboBox = new ComboBox();
                var mockEmulatorComboBox = new ComboBox();

                mockSystemComboBox.ItemsSource = _systemConfigs.Select(static config => config.SystemName).ToList();
                mockSystemComboBox.SelectedItem = systemConfig.SystemName;

                mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(static emulator => emulator.EmulatorName).ToList();
                mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

                // Store currently selected item to restore selection after refresh
                var selectedItem = PlayHistoryDataGrid.SelectedItem as PlayHistoryItem;

                // Launch Game
                await GameLauncher.HandleButtonClick(fullPath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs, _settings, _mainWindow);

                // Refresh play history data in UI after game ends
                RefreshPlayHistoryData();

                // Try to restore the selection if the item still exists
                if (selectedItem != null)
                {
                    // Find the same item in the refreshed list
                    var updatedItem = _playHistoryList.FirstOrDefault(item =>
                        item.FileName == selectedItem.FileName &&
                        item.SystemName == selectedItem.SystemName);

                    if (updatedItem != null)
                    {
                        PlayHistoryDataGrid.SelectedItem = updatedItem;
                        PlayHistoryDataGrid.ScrollIntoView(updatedItem);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"There was an error launching the game from Play History.\n" +
                                 $"File Path: {fileName}\n" +
                                 $"System Name: {systemName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void RefreshPlayHistoryData()
    {
        try
        {
            // Store current selected index if any
            var selectedIndex = PlayHistoryDataGrid.SelectedIndex;

            // Get updated play history data
            var playHistoryConfig = PlayHistoryManager.LoadPlayHistory();
            _playHistoryList = new ObservableCollection<PlayHistoryItem>();

            foreach (var historyItem in playHistoryConfig.PlayHistoryList)
            {
                // Find machine description if available
                var machine = _machines.FirstOrDefault(m =>
                    m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItem.FileName), StringComparison.OrdinalIgnoreCase));
                var machineDescription = machine?.Description ?? string.Empty;

                // Retrieve the system configuration for the history item
                var systemConfig = _systemConfigs.FirstOrDefault(config =>
                    config.SystemName.Equals(historyItem.SystemName, StringComparison.OrdinalIgnoreCase));

                // Get the default emulator. The first one in the list
                var defaultEmulator = systemConfig?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

                var playHistoryItem = new PlayHistoryItem
                {
                    FileName = historyItem.FileName,
                    SystemName = historyItem.SystemName,
                    TotalPlayTime = historyItem.TotalPlayTime,
                    TimesPlayed = historyItem.TimesPlayed,
                    LastPlayDate = historyItem.LastPlayDate,
                    LastPlayTime = historyItem.LastPlayTime,
                    MachineDescription = machineDescription,
                    DefaultEmulator = defaultEmulator,
                    CoverImage = GetCoverImagePath(historyItem.SystemName, historyItem.FileName)
                };
                _playHistoryList.Add(playHistoryItem);
            }

            // Sort the list by date and time using the safe parsing method
            SortByDateSafely();

            // Update the DataGrid
            PlayHistoryDataGrid.ItemsSource = _playHistoryList;

            // Try to restore selection if possible
            if (selectedIndex < 0 || selectedIndex >= _playHistoryList.Count) return;

            PlayHistoryDataGrid.SelectedIndex = selectedIndex;
            PlayHistoryDataGrid.ScrollIntoView(PlayHistoryDataGrid.SelectedItem);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error refreshing play history data.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private async void LaunchGameWithDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem) return;

            PlayClick.PlayClickSound();
            await LaunchGameFromHistory(selectedItem.FileName, selectedItem.SystemName);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method MouseDoubleClick.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private async void SetPreviewImageOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem)
            {
                PreviewImage.Source = null; // Clear preview if nothing is selected
                return;
            }

            var imagePath = selectedItem.CoverImage;
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

            // Assign the loaded image to the PreviewImage control.
            // loadedImage will be null if even the default image failed to load.
            PreviewImage.Source = loadedImage;
        }
        catch (Exception ex)
        {
            // This catch block handles exceptions *not* caught by ImageLoader.LoadImageAsync
            // (which should be rare, as ImageLoader catches most file/loading issues).
            PreviewImage.Source = null; // Ensure image is cleared on error

            // Log the error
            _ = LogErrors.LogErrorAsync(ex, "Error in the SetPreviewImageOnSelectionChanged method.");
        }
    }

    private void DeleteHistoryItemWithDelButton(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;

        if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
        {
            PlayClick.PlayTrashSound();

            _playHistoryList.Remove(selectedItem);
            _playHistoryManager.PlayHistoryList = _playHistoryList;
            _playHistoryManager.SavePlayHistory();
        }
        else
        {
            MessageBoxLibrary.SelectAHistoryItemToRemoveMessageBox();
        }
    }

    private void SortByDate_Click(object sender, RoutedEventArgs e)
    {
        SortByDateSafely();
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private void SortByTotalPlayTime_Click(object sender, RoutedEventArgs e)
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item => item.TotalPlayTime)
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private void SortByTimesPlayed_Click(object sender, RoutedEventArgs e)
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item => item.TimesPlayed)
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }
}

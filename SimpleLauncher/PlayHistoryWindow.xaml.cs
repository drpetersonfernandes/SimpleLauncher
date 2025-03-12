using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace SimpleLauncher;

public partial class PlayHistoryWindow
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");
    private readonly PlayHistoryManager _playHistoryManager;
    private ObservableCollection<PlayHistoryItem> _playHistoryList;
    private readonly SettingsManager _settings;
    private readonly List<SystemConfig> _systemConfigs;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;
    private readonly FavoritesManager _favoritesManager;

    private readonly Button _fakebutton = new();
    private readonly WrapPanel _fakeGameFileGrid = new();

    public PlayHistoryWindow(List<SystemConfig> systemConfigs, List<MameManager> machines, SettingsManager settings, FavoritesManager favoritesManager, PlayHistoryManager playHistoryManager, MainWindow mainWindow)
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
        Closing += PlayHistory_Closing;
    }

    private void PlayHistory_Closing(object sender, CancelEventArgs e)
    {
        _playHistoryList = null;
    }

    private void LoadPlayHistory()
    {
        var playHistoryConfig = PlayHistoryManager.LoadPlayHistory();
        _playHistoryList = [];
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

        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
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
            // Notify user to select a history item first
            MessageBoxLibrary.SelectAHistoryItemMessageBox();
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
            if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
            {
                # region Variables

                if (CheckFilenameOfSelectedHistoryItem(selectedItem)) return;
                Debug.Assert(selectedItem.FileName != null);

                var fileNameWithExtension = selectedItem.FileName;
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedItem.FileName);

                var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));
                if (CheckForSystemConfig(systemConfig)) return;
                Debug.Assert(systemConfig?.SystemFolder != null);

                var filePath = GetFullPath(Path.Combine(systemConfig.SystemFolder, selectedItem.FileName));

                # endregion

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
                            RightClickContextMenu.DeleteFile(filePath, fileNameWithExtension, _fakebutton, _fakeGameFileGrid, _mainWindow);
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

    private static void AddAdditionalMenuItems(ContextMenu contextMenu, PlayHistoryItem selectedItem, string fileNameWithoutExtension, string filePath, SystemConfig systemConfig)
    {
    }

    private static string GetFullPath(string path)
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
                MessageBoxLibrary.SelectGameToLaunchMessageBox();
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
            var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (await CheckSystemConfig(systemConfig)) return;

            var emulatorConfig = systemConfig?.Emulators.FirstOrDefault();
            if (await CheckEmulatorConfig(emulatorConfig)) return;

            Debug.Assert(systemConfig?.SystemFolder != null);
            var fullPath = GetFullPath(Path.Combine(systemConfig.SystemFolder, fileName));

            // Check if the file exists
            if (await CheckFilePathDeleteHistoryItemIfInvalid(fileName, systemName, fullPath)) return;

            var mockSystemComboBox = new ComboBox();
            var mockEmulatorComboBox = new ComboBox();

            mockSystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
            mockSystemComboBox.SelectedItem = systemConfig.SystemName;

            mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();
            Debug.Assert(emulatorConfig != null, nameof(emulatorConfig) + " != null");
            mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

            // Launch Game
            await GameLauncher.HandleButtonClick(fullPath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs, _settings, _mainWindow);
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

    private void RemoveHistoryItemFromXmlAndEmptyPreviewImage(PlayHistoryItem selectedItem)
    {
        _playHistoryList.Remove(selectedItem);
        _playHistoryManager.PlayHistoryList = _playHistoryList;
        _playHistoryManager.SavePlayHistory();

        PreviewImage.Source = null;
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

    private void SetPreviewImageOnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem) return;

        var imagePath = selectedItem.CoverImage;
        PreviewImage.Source = File.Exists(imagePath)
            ? new BitmapImage(new Uri(imagePath, UriKind.Absolute))
            :
            // Set a default image if the selected image doesn't exist
            new BitmapImage(new Uri("pack://application:,,,/images/default.png"));
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
    
    private bool GetSystemConfigOfSelectedHistoryItem(PlayHistoryItem selectedItem, out SystemConfig systemConfig)
    {
        systemConfig = _systemConfigs?.FirstOrDefault(config =>
            config.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));

        if (systemConfig != null) return false;

        // Notify developer
        const string contextMessage = "systemConfig is null.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.ErrorOpeningCoverImageMessageBox();

        return true;
    }

    private static bool CheckForSystemConfig(SystemConfig systemConfig)
    {
        if (systemConfig != null) return false;

        // Notify developer
        const string contextMessage = "systemConfig is null";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

        return true;
    }

    private static bool CheckFilenameOfSelectedHistoryItem(PlayHistoryItem selectedItem)
    {
        if (selectedItem.FileName != null) return false;

        // Notify developer
        const string contextMessage = "History item filename is null";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

        return true;
    }

    private Task<bool> CheckFilePathDeleteHistoryItemIfInvalid(string fileName, string systemName, string fullPath)
    {
        if (File.Exists(fullPath)) return Task.FromResult(false);

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

        return Task.FromResult(true);
    }

    private static Task<bool> CheckEmulatorConfig(SystemConfig.Emulator emulatorConfig)
    {
        if (emulatorConfig != null) return Task.FromResult(false);

        // Notify developer
        const string contextMessage = "emulatorConfig is null.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

        return Task.FromResult(true);
    }

    private static Task<bool> CheckSystemConfig(SystemConfig systemConfig)
    {
        if (systemConfig != null) return Task.FromResult(false);

        // Notify developer
        const string contextMessage = "systemConfig is null.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

        return Task.FromResult(true);
    }

    private void SortByDate_Click(object sender, RoutedEventArgs e)
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(item =>
                DateTime.Parse($"{item.LastPlayDate} {item.LastPlayTime}"))
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private void SortByPlayTime_Click(object sender, RoutedEventArgs e)
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(item => item.TotalPlayTime)
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private void SortByTimesPlayed_Click(object sender, RoutedEventArgs e)
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(item => item.TimesPlayed)
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private bool GetSystemConfigOfSelectedFavorite(Favorite selectedFavorite, out SystemConfig systemConfig)
    {
        systemConfig = _systemConfigs?.FirstOrDefault(config =>
            config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));

        if (systemConfig != null) return false;

        // Notify developer
        const string contextMessage = "systemConfig is null.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.ErrorOpeningCoverImageMessageBox();

        return true;
    }
}
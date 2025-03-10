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
    private PlayHistoryManager _playHistoryManager;
    private ObservableCollection<PlayHistoryItem> _playHistoryList;
    private readonly SettingsConfig _settings;
    private readonly List<SystemConfig> _systemConfigs;
    private readonly List<MameConfig> _machines;
    private readonly MainWindow _mainWindow;

    private readonly Button _fakebutton = new();
    private readonly WrapPanel _fakeGameFileGrid = new();

    public PlayHistoryWindow(SettingsConfig settings, List<SystemConfig> systemConfigs, List<MameConfig> machines, PlayHistoryManager playHistoryManager, MainWindow mainWindow)
    {
        InitializeComponent();

        _settings = settings;
        _systemConfigs = systemConfigs;
        _machines = machines;
        _mainWindow = mainWindow;
        _playHistoryManager = playHistoryManager;

        App.ApplyThemeToWindow(this);
        LoadPlayHistory();
        Closing += PlayHistory_Closing;
    }

    private void PlayHistory_Closing(object sender, CancelEventArgs e)
    {
        _playHistoryManager = null;
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
                m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItem.FileName),
                    StringComparison.OrdinalIgnoreCase));
            var machineDescription = machine?.Description ?? string.Empty;

            // Retrieve the system configuration for the history item
            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(historyItem.SystemName, StringComparison.OrdinalIgnoreCase));

            // Get the default emulator, e.g., the first one in the list
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
        return systemConfig == null ? Path.Combine(baseDirectory, "images", "default.png") : FindCoverImagePath(systemName, fileName, baseDirectory, systemConfig.SystemImageFolder);
    }

    private void RemoveHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
        {
            _playHistoryList.Remove(selectedItem);
            _playHistoryManager.PlayHistoryList = _playHistoryList; // Keep the instance in sync
            _playHistoryManager.SavePlayHistory(); // Save using the existing instance

            PlayClick.PlayClickSound();
            PreviewImage.Source = null;
        }
        else
        {
            // Notify user to select a history item first
            var message = (string)Application.Current.TryFindResource("SelectAHistoryItemToRemove") ?? "Please select a history item to remove.";
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void PlayHistoryWindowRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
            {
                if (CheckFilenameOfSelectedHistoryItem(selectedItem)) return;
                Debug.Assert(selectedItem.FileName != null);

                var fileNameWithExtension = selectedItem.FileName;
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedItem.FileName);

                var systemConfig = _systemConfigs.FirstOrDefault(config => config.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));
                if (CheckForSystemConfig(systemConfig)) return;
                Debug.Assert(systemConfig?.SystemFolder != null);

                var filePath = GetFullPath(Path.Combine(systemConfig.SystemFolder, selectedItem.FileName));

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

                // "Remove from History" MenuItem
                var removeIcon = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/images/brokenheart.png", UriKind.RelativeOrAbsolute)),
                    Width = 16,
                    Height = 16
                };
                var removeFromHistory = (string)Application.Current.TryFindResource("RemovefromHistory") ?? "Remove From History";
                var removeMenuItem = new MenuItem
                {
                    Header = removeFromHistory,
                    Icon = removeIcon
                };
                removeMenuItem.Click += (_, _) =>
                {
                    PlayClick.PlayClickSound();
                    RemoveHistoryItemFromXmlAndEmptyPreviewImage(selectedItem);
                };

                // Add all menu items to the context menu
                contextMenu.Items.Add(launchMenuItem);
                contextMenu.Items.Add(removeMenuItem);

                // Add the same menu items as in the FavoritesWindow for consistency
                AddAdditionalMenuItems(contextMenu, selectedItem, fileNameWithoutExtension, filePath, systemConfig);

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

    private void AddAdditionalMenuItems(ContextMenu contextMenu, PlayHistoryItem selectedItem, string fileNameWithoutExtension, string filePath, SystemConfig systemConfig)
    {
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

        // Add other menu items like in FavoritesWindow (history, cover, snapshots, etc.)
        // These are just a few examples, you'd add all the relevant ones
        contextMenu.Items.Add(videoLinkMenuItem);
        contextMenu.Items.Add(infoLinkMenuItem);

        // Add ROM History, Cover, Title Snapshot, etc. like in FavoritesWindow
        // For brevity, I'm not including all of them, but you would add them here

        // For example, adding the ROM History menu item:
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
            RightClickContextMenu.OpenHistoryWindow(selectedItem.SystemName, fileNameWithoutExtension, systemConfig, _machines);
        };

        contextMenu.Items.Add(openHistoryMenuItem);
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

        PlayClick.PlayTrashSound();

        if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
        {
            _playHistoryList.Remove(selectedItem);
            _playHistoryManager.PlayHistoryList = _playHistoryList;
            _playHistoryManager.SavePlayHistory();
        }
        else
        {
            var message = (string)Application.Current.TryFindResource("SelectAHistoryItemToRemove") ?? "Please select a history item to remove.";
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private static string FindCoverImagePath(string systemName, string fileName, string baseDirectory, string systemImageFolder)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        // Ensure the systemImageFolder considers both absolute and relative paths
        if (!Path.IsPathRooted(systemImageFolder))
        {
            if (systemImageFolder != null) systemImageFolder = Path.Combine(baseDirectory, systemImageFolder);
        }

        var globalDirectory = Path.Combine(baseDirectory, "images", systemName);
        string[] imageExtensions = [".png", ".jpg", ".jpeg"];

        // First try to find the image in the specific directory
        if (TryFindImage(systemImageFolder, out var foundImagePath))
        {
            return foundImagePath;
        }

        // If not found, try the global directory
        return TryFindImage(globalDirectory, out foundImagePath)
            ? foundImagePath
            :

            // If not found, use default image
            Path.Combine(baseDirectory, "images", "default.png");

        // Search for the image file
        bool TryFindImage(string directory, out string foundPath)
        {
            foreach (var extension in imageExtensions)
            {
                var imagePath = Path.Combine(directory, fileNameWithoutExtension + extension);
                if (!File.Exists(imagePath)) continue;
                foundPath = imagePath;
                return true;
            }

            foundPath = null;
            return false;
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
        const string contextMessage = "systemConfig is null for the selected play history item";
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
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.UiHelpers;

public class GameListFactory(
    ComboBox emulatorComboBox,
    ComboBox systemComboBox,
    List<SystemManager> systemConfigs,
    List<MameManager> machines,
    SettingsManager settings,
    FavoritesManager favoritesManager,
    PlayHistoryManager playHistoryManager,
    MainWindow mainWindow)
{
    private readonly ComboBox _emulatorComboBox = emulatorComboBox;
    private readonly ComboBox _systemComboBox = systemComboBox;
    private readonly List<SystemManager> _systemConfigs = systemConfigs;
    private readonly List<MameManager> _machines = machines;
    private readonly SettingsManager _settings = settings;
    private readonly FavoritesManager _favoritesManager = favoritesManager;
    private readonly PlayHistoryManager _playHistoryManager = playHistoryManager;
    private readonly MainWindow _mainWindow = mainWindow;

    public Task<GameListViewItem> CreateGameListViewItemAsync(string filePath, string systemName, SystemManager systemManager)
    {
        var absoluteFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath);

        var fileNameWithExtension = PathHelper.GetFileName(absoluteFilePath);
        var fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(absoluteFilePath);
        // var selectedSystemName = systemName; // Parameter 'systemName' is already this

        var machineDescription = systemManager.SystemIsMame ? GetMachineDescription(fileNameWithoutExtension) : string.Empty;

        var isFavorite = _favoritesManager.FavoriteList
            .Any(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase) &&
                      f.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

        var playHistoryItem = _playHistoryManager.PlayHistoryList
            .FirstOrDefault(h => h.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase) &&
                                 h.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

        var timesPlayed = "0";
        var playTime = "0m 0s";

        if (playHistoryItem != null)
        {
            var timeSpan = TimeSpan.FromSeconds(playHistoryItem.TotalPlayTime);
            playTime = timeSpan.TotalHours >= 1
                ? $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
                : $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            timesPlayed = playHistoryItem.TimesPlayed.ToString(CultureInfo.InvariantCulture);
        }

        // Create the GameListViewItem with initial data. FileSize will show "Calculating..."
        var gameListViewItem = new GameListViewItem
        {
            FileName = fileNameWithoutExtension,
            MachineDescription = machineDescription,
            FilePath = absoluteFilePath,
            FolderPath = Path.GetDirectoryName(absoluteFilePath),
            ContextMenu = ContextMenu.AddRightClickReturnContextMenu(absoluteFilePath, fileNameWithExtension, fileNameWithoutExtension, systemName,
                _emulatorComboBox, _favoritesManager, systemManager, _machines, _settings, _mainWindow),
            IsFavorite = isFavorite,
            TimesPlayed = timesPlayed,
            PlayTime = playTime,
            FileSizeBytes = -1 // Initialize to "Calculating..." state via the backing field
        };

        // Asynchronously calculate and set the file size.
        // Capture the necessary variables for the closure.

        _ = Task.Run(() => // Fire and forget; UI updates via INotifyPropertyChanged
        {
            long sizeToSet;
            try
            {
                if (File.Exists(absoluteFilePath))
                {
                    sizeToSet = new FileInfo(absoluteFilePath).Length;
                }
                else
                {
                    sizeToSet = -2; // Indicate N/A or Error
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, $"Error getting file size for {absoluteFilePath}");

                sizeToSet = -2; // Indicate Error
            }

            // This assignment will trigger OnPropertyChanged in GameListViewItem, updating the UI.
            gameListViewItem.FileSizeBytes = sizeToSet;
        });

        // Return the item immediately. The file size will update in the UI when the task completes.
        return Task.FromResult(gameListViewItem);
    }

    public async void HandleSelectionChanged(GameListViewItem selectedItem)
    {
        try
        {
            // Ensure the MainWindow and its PreviewImage control are available.
            if (_mainWindow == null)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new InvalidOperationException("_mainWindow is null in GameListFactory.HandleSelectionChanged."), "MainWindow instance is null. Cannot update preview.");

                return;
            }

            if (_mainWindow.PreviewImage == null)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new InvalidOperationException("_mainWindow.PreviewImage is null in GameListFactory.HandleSelectionChanged."), "PreviewImage control in MainWindow is null. Cannot update preview.");

                return;
            }

            try
            {
                if (selectedItem == null)
                {
                    // No item selected, clear the preview image.
                    _mainWindow.PreviewImage.Source = null;
                    return;
                }

                var filePath = selectedItem.FilePath;
                if (string.IsNullOrEmpty(filePath))
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new ArgumentException("selectedItem.FilePath is null or empty."), "Selected item has an invalid file path. Cannot load preview.");

                    _mainWindow.PreviewImage.Source = null; // Clear preview
                    var (defaultImg, _) = await ImageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = defaultImg);

                    return;
                }

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

                var selectedSystem = _systemComboBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedSystem))
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new InvalidOperationException("Selected system name is null or empty from ComboBox."), "No system selected or system name is invalid. Cannot load preview.");

                    _mainWindow.PreviewImage.Source = null; // Clear preview
                    var (defaultImg, _) = await ImageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = defaultImg);

                    return;
                }

                var systemConfig = _systemConfigs?.FirstOrDefault(c => c.SystemName == selectedSystem); // Added ?. for robustness
                if (systemConfig == null)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new InvalidOperationException($"System configuration not found for '{selectedSystem}'."), $"No system configuration for {selectedSystem}. Cannot load preview.");

                    _mainWindow.PreviewImage.Source = null; // Clear preview
                    var (defaultImg, _) = await ImageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = defaultImg);

                    return;
                }

                var previewImagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, systemConfig);

                _mainWindow.PreviewImage.Source = null; // Clear existing image before loading new one

                if (!string.IsNullOrEmpty(previewImagePath))
                {
                    previewImagePath = PathHelper.ResolveRelativeToAppDirectory(previewImagePath);
                }

                var (imageSource, _) = await ImageLoader.LoadImageAsync(previewImagePath); // LoadImageAsync handles null/empty path by returning default

                _mainWindow.Dispatcher.Invoke(() =>
                {
                    _mainWindow.PreviewImage.Source = imageSource;
                });
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error loading preview image.");

                // Attempt to set a default image in case of any error during the process
                try
                {
                    var (defaultImageSource, _) = await ImageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        if (_mainWindow?.PreviewImage != null) // Check again to be safe
                        {
                            _mainWindow.PreviewImage.Source = defaultImageSource;
                        }
                    });
                }
                catch (Exception fallbackEx)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(fallbackEx, "Error loading fallback preview image after an initial error.");
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in method GameListFactory.HandleSelectionChanged.");
        }
    }

    private string GetMachineDescription(string fileName)
    {
        var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        return machine?.Description ?? string.Empty;
    }

    public async Task HandleDoubleClick(GameListViewItem selectedItem)
    {
        if (selectedItem == null)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(null, "selectedItem is null.");

            return;
        }

        var filePath = selectedItem.FilePath;
        var selectedEmulatorName = _emulatorComboBox.SelectedItem as string;
        var selectedSystemName = _systemComboBox.SelectedItem as string;
        var selectedSystemManager = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystemName);

        if (selectedSystemManager == null)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(null, "selectedSystemManager is null.");

            return;
        }

        await GameLauncher.HandleButtonClick(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
    }
}

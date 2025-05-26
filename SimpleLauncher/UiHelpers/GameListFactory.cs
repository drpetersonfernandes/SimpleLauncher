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
                    // Notify developer
                    // _ = LogErrors.LogErrorAsync(new FileNotFoundException($"File not found for size calc: {absoluteFilePath}", absoluteFilePath),
                    //     $"File not found for size calc: {absoluteFilePath}");
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
            if (selectedItem == null) return;

            var filePath = selectedItem.FilePath;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var selectedSystem = _systemComboBox.SelectedItem as string;
            var systemConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);
            if (systemConfig == null) return;

            var previewImagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, systemConfig);

            _mainWindow.PreviewImage.Source = null;

            if (!string.IsNullOrEmpty(previewImagePath))
            {
                previewImagePath = PathHelper.ResolveRelativeToAppDirectory(previewImagePath);
            }

            var (imageSource, _) = await ImageLoader.LoadImageAsync(previewImagePath);

            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.PreviewImage.Source = imageSource;
            });
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error loading preview image.");
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
            var ex = new Exception("selectedItem is null.");
            _ = LogErrors.LogErrorAsync(ex, "selectedItem is null.");

            return;
        }

        var filePath = selectedItem.FilePath;
        var selectedEmulatorName = _emulatorComboBox.SelectedItem as string;
        var selectedSystemName = _systemComboBox.SelectedItem as string;
        var selectedSystemManager = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystemName);

        if (selectedSystemManager == null)
        {
            var ex = new Exception("selectedSystemManager is null.");
            _ = LogErrors.LogErrorAsync(ex, "selectedSystemManager is null.");
            return;
        }

        await GameLauncher.HandleButtonClick(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
    }
}
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
        var fileNameWithExtension = PathHelper.GetFileName(filePath);
        var fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(filePath);
        var selectedSystemName = systemName;

        var machineDescription = systemManager.SystemIsMame ? GetMachineDescription(fileNameWithoutExtension) : string.Empty;

        // Check if this file is a favorite
        var isFavorite = _favoritesManager.FavoriteList
            .Any(f => f.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase) &&
                      f.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));

        // Get playtime from playHistoryManager
        var playHistoryItem = _playHistoryManager.PlayHistoryList
            .FirstOrDefault(h => h.FileName.Equals(fileNameWithExtension, StringComparison.OrdinalIgnoreCase) &&
                                 h.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));

        var timesPlayed = "0"; // Default
        var playTime = "0m 0s"; // Default

        if (playHistoryItem != null)
        {
            var timeSpan = TimeSpan.FromSeconds(playHistoryItem.TotalPlayTime);
            playTime = timeSpan.TotalHours >= 1
                ? $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
                : $"{timeSpan.Minutes}m {timeSpan.Seconds}s";

            // Get times played
            timesPlayed = playHistoryItem.TimesPlayed.ToString(CultureInfo.InvariantCulture);
        }

        // Calculate and format file size
        var fileSize = "N/A";
        try
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                fileSize = FormatFileSize.Format(fileInfo.Length);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail item creation
            _ = LogErrors.LogErrorAsync(ex, $"Error getting file size for {filePath}");
            fileSize = "Error"; // Indicate error in UI
        }

        // Create the GameListViewItem with file details
        var gameListViewItem = new GameListViewItem
        {
            FileName = fileNameWithoutExtension,
            MachineDescription = machineDescription,
            FilePath = filePath,
            ContextMenu = ContextMenu.AddRightClickReturnContextMenu(filePath, fileNameWithExtension, fileNameWithoutExtension, selectedSystemName,
                _emulatorComboBox, _favoritesManager, systemManager, _machines, _settings, _mainWindow),
            IsFavorite = isFavorite,
            TimesPlayed = timesPlayed,
            PlayTime = playTime,
            FileSize = fileSize
        };

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

            // Normalize the path
            if (!string.IsNullOrEmpty(previewImagePath))
            {
                previewImagePath = PathHelper.ResolveRelativeToAppDirectory(previewImagePath);
            }
            // Note: If previewImagePath is null/empty, ImageLoader.LoadImageAsync will handle it by trying the default.

            // Use ImageLoader to load the image asynchronously.
            // ImageLoader handles file existence, reading, memory stream, freezing, background thread loading, error logging, and default image fallback.
            // Use discard (_) for the 'isDefault' variable as it's not used here.
            var (imageSource, _) = await ImageLoader.LoadImageAsync(previewImagePath);

            _mainWindow.Dispatcher.Invoke(() =>
            {
                _mainWindow.PreviewImage.Source = imageSource;
            });
        }
        catch (Exception ex)
        {
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
            // Notify developer
            var ex = new Exception("selectedSystemManager is null.");
            _ = LogErrors.LogErrorAsync(ex, "selectedSystemManager is null.");

            return;
        }

        await GameLauncher.HandleButtonClick(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
    }
}
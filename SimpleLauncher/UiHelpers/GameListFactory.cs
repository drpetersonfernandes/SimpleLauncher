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

    private readonly WrapPanel _fakeFileGrid = new();
    private readonly Button _fakeButton = new();

    private string _filePath;
    private string _fileNameWithExtension;
    private string _fileNameWithoutExtension;
    private string _selectedSystemName;
    private SystemManager _selectedSystemManager;

    public Task<GameListViewItem> CreateGameListViewItemAsync(string filePath, string systemName, SystemManager systemManager)
    {
        _filePath = filePath;
        _fileNameWithExtension = PathHelper.GetFileName(filePath);
        _fileNameWithoutExtension = PathHelper.GetFileNameWithoutExtension(filePath);
        _selectedSystemName = systemName;
        _selectedSystemManager = systemManager;

        var machineDescription = _selectedSystemManager.SystemIsMame ? GetMachineDescription(_fileNameWithoutExtension) : string.Empty;

        // Check if this file is a favorite
        var isFavorite = _favoritesManager.FavoriteList
            .Any(f => f.FileName.Equals(_fileNameWithExtension, StringComparison.OrdinalIgnoreCase) &&
                      f.SystemName.Equals(_selectedSystemName, StringComparison.OrdinalIgnoreCase));

        // Get playtime from playHistoryManager
        var playHistoryItem = _playHistoryManager.PlayHistoryList
            .FirstOrDefault(h => h.FileName.Equals(_fileNameWithExtension, StringComparison.OrdinalIgnoreCase) &&
                                 h.SystemName.Equals(_selectedSystemName, StringComparison.OrdinalIgnoreCase));

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

        // Create the GameListViewItem with file details
        var gameListViewItem = new GameListViewItem
        {
            FileName = _fileNameWithoutExtension,
            MachineDescription = machineDescription,
            FilePath = _filePath,
            ContextMenu = ContextMenu.AddRightClickReturnContextMenu(_filePath, _emulatorComboBox, _systemComboBox,
                _systemConfigs, _settings, _mainWindow, _selectedSystemName, _fileNameWithExtension, _favoritesManager, _fakeFileGrid,
                _fileNameWithoutExtension, _selectedSystemManager, _fakeButton, _machines),
            IsFavorite = isFavorite,
            TimesPlayed = timesPlayed,
            PlayTime = playTime
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
                previewImagePath = PathHelper.ResolveRelativeToCurrentDirectory(previewImagePath);
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
        if (selectedItem == null) return;

        var selectedSystem = _systemComboBox.SelectedItem as string;
        var systemConfig = _systemConfigs.FirstOrDefault(c => c.SystemName == selectedSystem);
        if (systemConfig != null)
        {
            await GameLauncher.HandleButtonClick(selectedItem.FilePath, _emulatorComboBox, _systemComboBox,
                _systemConfigs, _settings, _mainWindow);
        }
    }
}
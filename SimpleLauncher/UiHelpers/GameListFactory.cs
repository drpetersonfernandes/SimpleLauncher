using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.UiHelpers;

public class GameListFactory(
    ComboBox emulatorComboBox,
    ComboBox systemComboBox,
    List<SystemManager> systemManagers,
    List<MameManager> machines,
    SettingsManager settings,
    FavoritesManager favoritesManager,
    PlayHistoryManager playHistoryManager,
    MainWindow mainWindow,
    GamePadController gamePadController,
    GameLauncher gameLauncher,
    PlaySoundEffects playSoundEffects)
{
    private readonly ComboBox _emulatorComboBox = emulatorComboBox;
    private readonly ComboBox _systemComboBox = systemComboBox;
    private readonly List<SystemManager> _systemManagers = systemManagers;
    private readonly List<MameManager> _machines = machines;
    private readonly SettingsManager _settings = settings;
    private readonly FavoritesManager _favoritesManager = favoritesManager;
    private readonly PlayHistoryManager _playHistoryManager = playHistoryManager;
    private readonly MainWindow _mainWindow = mainWindow;
    private readonly GamePadController _gamePadController = gamePadController;
    private readonly GameLauncher _gameLauncher = gameLauncher;
    private readonly PlaySoundEffects _playSoundEffects = playSoundEffects;

    public Task<GameListViewItem> CreateGameListViewItemAsync(string entityPath, string systemName, SystemManager systemManager)
    {
        var isDirectory = Directory.Exists(entityPath);
        string fileNameWithExtension;
        string fileNameWithoutExtension;
        string folderPath;

        if (isDirectory)
        {
            fileNameWithExtension = Path.GetFileName(entityPath);
            fileNameWithoutExtension = fileNameWithExtension;
            folderPath = entityPath;
        }
        else
        {
            fileNameWithExtension = Path.GetFileName(entityPath);
            fileNameWithoutExtension = Path.GetFileNameWithoutExtension(entityPath);
            folderPath = Path.GetDirectoryName(entityPath);
        }

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
            FilePath = entityPath,
            FolderPath = folderPath,
            ContextMenu = ContextMenu.AddRightClickReturnContextMenu(
                new RightClickContext(
                    entityPath,
                    fileNameWithExtension,
                    fileNameWithoutExtension,
                    systemName,
                    systemManager,
                    _machines,
                    _favoritesManager,
                    _settings,
                    _emulatorComboBox,
                    null,
                    null,
                    null,
                    null,
                    _mainWindow,
                    _gamePadController,
                    null,
                    _gameLauncher,
                    _playSoundEffects
                )
            ),
            IsFavorite = isFavorite,
            TimesPlayed = timesPlayed,
            PlayTime = playTime,
            FileSizeBytes = -1 // Initialize to "Calculating..." state via the backing field
        };

        _ = Task.Run(() => // Fire and forget; UI updates via INotifyPropertyChanged
        {
            long sizeToSet;
            try
            {
                if (isDirectory)
                {
                    // Sum up the size of all files in the directory and its subdirectories
                    sizeToSet = new DirectoryInfo(entityPath).EnumerateFiles("*", SearchOption.AllDirectories).Sum(static fi => fi.Length);
                }
                else if (File.Exists(entityPath))
                {
                    sizeToSet = new FileInfo(entityPath).Length;
                }
                else
                {
                    sizeToSet = -2; // "N/A"
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error getting file size for {entityPath}");

                sizeToSet = -2; // Indicate N/A or Error
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new InvalidOperationException("_mainWindow is null in GameListFactory.HandleSelectionChanged."), "MainWindow instance is null. Cannot update preview.");

                return;
            }

            if (_mainWindow.PreviewImage == null)
            {
                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new InvalidOperationException("_mainWindow.PreviewImage is null in GameListFactory.HandleSelectionChanged."), "PreviewImage control in MainWindow is null. Cannot update preview.");

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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new ArgumentException("selectedItem.FilePath is null or empty."), "Selected item has an invalid file path. Cannot load preview.");

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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new InvalidOperationException("Selected system name is null or empty from ComboBox."), "No system selected or system name is invalid. Cannot load preview.");

                    _mainWindow.PreviewImage.Source = null; // Clear preview
                    var (defaultImg, _) = await ImageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = defaultImg);

                    return;
                }

                var systemManager = _systemManagers?.FirstOrDefault(c => c.SystemName == selectedSystem);
                if (systemManager == null)
                {
                    // Notify developer
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new InvalidOperationException($"System configuration not found for '{selectedSystem}'."), $"No system configuration for {selectedSystem}. Cannot load preview.");

                    _mainWindow.PreviewImage.Source = null; // Clear preview
                    var (defaultImg, _) = await ImageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = defaultImg);

                    return;
                }

                string previewImagePath;
                var isDirectory = Directory.Exists(filePath);

                if (isDirectory) // GroupByFolder is true
                {
                    // First, try to find an image with the same name as the folder name.
                    previewImagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, systemManager, _settings);

                    // If the found path is a default image, try the fallback logic.
                    if (previewImagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase))
                    {
                        // Fallback to current logic: look inside the folder for a file to use as a name.
                        var filesInFolder = await GetListOfFiles.GetFilesAsync(filePath, systemManager.FileFormatsToSearch);
                        if (filesInFolder.Count != 0)
                        {
                            var representativeFileName = Path.GetFileNameWithoutExtension(filesInFolder.First());
                            // Now search again with the new name.
                            previewImagePath = FindCoverImage.FindCoverImagePath(representativeFileName, selectedSystem, systemManager, _settings);
                        }
                    }
                }
                else
                {
                    // This is the logic for non-grouped files, which remains the same.
                    previewImagePath = FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, systemManager, _settings);
                }

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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading preview image.");

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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(fallbackEx, "Error loading fallback preview image after an initial error.");
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in method GameListFactory.HandleSelectionChanged.");
        }
    }

    private string GetMachineDescription(string fileName)
    {
        var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        return machine?.Description ?? string.Empty;
    }

    public async Task HandleDoubleClickAsync(GameListViewItem selectedItem)
    {
        if (selectedItem == null)
        {
            // Notify developer
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "selectedItem is null.");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

            return;
        }

        var filePath = selectedItem.FilePath;
        var selectedEmulatorName = _emulatorComboBox.SelectedItem as string;
        var selectedSystemName = _systemComboBox.SelectedItem as string;
        var selectedSystemManager = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystemName);

        if (string.IsNullOrEmpty(filePath))
        {
            // Notify developer
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "filepath is null or empty.");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

            return;
        }

        if (string.IsNullOrEmpty(selectedEmulatorName))
        {
            // Notify developer
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "[HandleDoubleClickAsync] selectedEmulatorName is null or empty.");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

            return;
        }

        if (string.IsNullOrEmpty(selectedSystemName))
        {
            // Notify developer
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "selectedSystemName is null or empty.");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

            return;
        }

        if (selectedSystemManager == null)
        {
            // Notify developer
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "selectedSystemManager is null.");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

            return;
        }

        await _gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow, _gamePadController);
    }
}
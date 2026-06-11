using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.WpfServices;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameItemFactory;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class GameListFactory(
    ComboBox emulatorComboBox,
    ComboBox systemComboBox,
    List<SystemManager.SystemManager> systemManagers,
    List<MameManager.MameManager> machines,
    SettingsManager.SettingsManager settings,
    FavoritesManager favoritesManager,
    PlayHistoryManager playHistoryManager,
    MainWindow mainWindow,
    GamePadController gamePadController,
    GameLauncher.GameLauncher gameLauncher,
    PlaySoundEffects playSoundEffects,
    IConfiguration configuration,
    ILogErrors logErrors,
    IGetListOfFilesService getListOfFiles,
    IFindCoverImageService findCoverImage,
    IImageLoader imageLoader,
    IMessageBoxLibraryService messageBox)
{
    private readonly ComboBox _emulatorComboBox = emulatorComboBox;
    private readonly ComboBox _systemComboBox = systemComboBox;
    private readonly List<SystemManager.SystemManager> _systemManagers = systemManagers;
    private readonly List<MameManager.MameManager> _machines = machines;
    private readonly SettingsManager.SettingsManager _settings = settings;
    private readonly FavoritesManager _favoritesManager = favoritesManager;
    private readonly PlayHistoryManager _playHistoryManager = playHistoryManager;
    private readonly MainWindow _mainWindow = mainWindow;
    private readonly GamePadController _gamePadController = gamePadController;
    private readonly GameLauncher.GameLauncher _gameLauncher = gameLauncher;
    private readonly PlaySoundEffects _playSoundEffects = playSoundEffects;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogErrors _logErrors = logErrors;
    private readonly IGetListOfFilesService _getListOfFiles = getListOfFiles;
    private readonly IFindCoverImageService _findCoverImage = findCoverImage;
    private readonly IImageLoader _imageLoader = imageLoader;
    private readonly IMessageBoxLibraryService _messageBox = messageBox;

    public Task<GameListViewItem> CreateGameListViewItemAsync(string entityPath, string systemName, SystemManager.SystemManager systemManager)
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
            folderPath = Path.GetDirectoryName(entityPath) ?? "";
        }

        var machineDescription = GetMachineDescription(fileNameWithoutExtension);

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
            SystemName = systemName,
            IsFavorite = isFavorite,
            TimesPlayed = timesPlayed,
            PlayTime = playTime
        };

        // Return the item immediately. The file size will update in the UI when the task completes.
        return Task.FromResult(gameListViewItem);
    }

    public async void HandleSelectionChangedAsync(GameListViewItem selectedItem)
    {
        try
        {
            // Ensure the MainWindow and its PreviewImage control are available.
            if (_mainWindow == null)
            {
                // Notify developer
                _logErrors.LogAndForget(new InvalidOperationException("_mainWindow is null in GameListFactory.HandleSelectionChangedAsync."), "MainWindow instance is null. Cannot update preview.");

                return;
            }

            if (_mainWindow.PreviewImage == null)
            {
                // Notify developer
                _logErrors.LogAndForget(new InvalidOperationException("_mainWindow.PreviewImage is null in GameListFactory.HandleSelectionChangedAsync."), "PreviewImage control in MainWindow is null. Cannot update preview.");

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
                    _logErrors.LogAndForget(new ArgumentException("selectedItem.FilePath is null or empty."), "Selected item has an invalid file path. Cannot load preview.");

                    _mainWindow.PreviewImage.Source = null; // Clear preview
                    var (defaultStream, _) = await _imageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = defaultStream.ToBitmapImage());

                    return;
                }

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

                var selectedSystem = _systemComboBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedSystem))
                {
                    // Notify developer
                    _logErrors.LogAndForget(new InvalidOperationException("Selected system name is null or empty from ComboBox."), "No system selected or system name is invalid. Cannot load preview.");

                    _mainWindow.PreviewImage.Source = null; // Clear preview
                    var (defaultStream, _) = await _imageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = defaultStream.ToBitmapImage());

                    return;
                }

                var systemManager = _systemManagers?.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
                if (systemManager == null)
                {
                    // Notify developer
                    _logErrors.LogAndForget(new InvalidOperationException($"System configuration not found for '{selectedSystem}'."), $"No system configuration for {selectedSystem}. Cannot load preview.");

                    _mainWindow.PreviewImage.Source = null; // Clear preview
                    var (defaultStream, _) = await _imageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = defaultStream.ToBitmapImage());

                    return;
                }

                string previewImagePath;
                var isDirectory = Directory.Exists(filePath);

                if (isDirectory) // GroupByFolder is true
                {
                    // First, try to find an image with the same name as the folder name.
                    previewImagePath = _findCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, systemManager.SystemImageFolder);

                    // If the found path is a default image, try the fallback logic.
                    if (previewImagePath.EndsWith("default.png", StringComparison.OrdinalIgnoreCase))
                    {
                        // Fallback to current logic: look inside the folder for a file to use as a name.
                        var filesInFolder = await _getListOfFiles.GetFilesAsync(filePath, systemManager.FileFormatsToSearch, systemManager.DisableRecursiveSearch, systemManager.GroupByFolder);
                        if (filesInFolder.Count != 0)
                        {
                            var representativeFileName = Path.GetFileNameWithoutExtension(filesInFolder.First());
                            // Now search again with the new name.
                            previewImagePath = _findCoverImage.FindCoverImagePath(representativeFileName, selectedSystem, systemManager.SystemImageFolder);
                        }
                    }
                }
                else
                {
                    // This is the logic for non-grouped files, which remains the same.
                    previewImagePath = _findCoverImage.FindCoverImagePath(fileNameWithoutExtension, selectedSystem, systemManager.SystemImageFolder);
                }

                _mainWindow.PreviewImage.Source = null; // Clear existing image before loading new one

                if (!string.IsNullOrEmpty(previewImagePath))
                {
                    previewImagePath = PathHelper.ResolveRelativeToAppDirectory(previewImagePath);
                }

                var (imageStream, _) = await _imageLoader.LoadImageAsync(previewImagePath); // LoadImageAsync handles null/empty path by returning default

                _mainWindow.Dispatcher.Invoke(() =>
                {
                    // Race condition check: Only assign if the selected item hasn't changed
                    if (_mainWindow.GameDataGrid.SelectedItem == selectedItem)
                    {
                        _mainWindow.PreviewImage.Source = imageStream.ToBitmapImage();
                    }
                });
            }
            catch (Exception ex)
            {
                // Notify developer
                _logErrors.LogAndForget(ex, "Error loading preview image.");

                // Attempt to set a default image in case of any error during the process
                try
                {
                    var (defaultImageStream, _) = await _imageLoader.LoadImageAsync(null); // Load global default
                    _mainWindow.Dispatcher.Invoke(() =>
                    {
                        // Race condition check: Only assign if the selected item hasn't changed (or is now null)
                        if (_mainWindow.GameDataGrid.SelectedItem == selectedItem || _mainWindow.GameDataGrid.SelectedItem == null)
                        {
                            _mainWindow?.PreviewImage?.Source = defaultImageStream.ToBitmapImage();
                        }
                    });
                }
                catch (Exception fallbackEx)
                {
                    // Notify developer
                    _logErrors.LogAndForget(fallbackEx, "Error loading fallback preview image after an initial error.");
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Error in method GameListFactory.HandleSelectionChangedAsync.");
        }
    }

    private string GetMachineDescription(string fileName)
    {
        var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        return machine?.Description ?? "";
    }

    public async Task HandleDoubleClickAsync(GameListViewItem selectedItem)
    {
        if (selectedItem == null)
        {
            // Notify developer
            _logErrors.LogAndForget(null, "selectedItem is null.");

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

            return;
        }

        var filePath = selectedItem.FilePath;
        var selectedEmulatorName = _emulatorComboBox.SelectedItem as string;
        var selectedSystemName = _systemComboBox.SelectedItem as string;
        var selectedSystemManager = _systemManagers.FirstOrDefault(c => c.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(filePath))
        {
            // Notify developer
            _logErrors.LogAndForget(null, "[HandleDoubleClickAsync] filepath is null or empty.");

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

            return;
        }

        if (string.IsNullOrEmpty(selectedEmulatorName))
        {
            // Notify developer
            _logErrors.LogAndForget(null, "[HandleDoubleClickAsync] selectedEmulatorName is null or empty.");

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

            return;
        }

        if (string.IsNullOrEmpty(selectedSystemName))
        {
            // Notify developer
            _logErrors.LogAndForget(null, "[HandleDoubleClickAsync] selectedSystemName is null or empty.");

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

            return;
        }

        if (selectedSystemManager == null)
        {
            // Notify developer
            _logErrors.LogAndForget(null, "[HandleDoubleClickAsync] selectedSystemManager is null.");

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

            return;
        }

        await _gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, WpfWindowContext.FromMainWindow(_mainWindow), _gamePadController, null);
    }
}
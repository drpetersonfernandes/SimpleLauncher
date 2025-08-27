using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class FavoritesWindow
{
    private static readonly string LogPath = GetLogPath.Path();
    private readonly FavoritesManager _favoritesManager;
    private readonly ObservableCollection<Favorite> _favoriteList = new();
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemManagers;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;

    public FavoritesWindow(
        SettingsManager settings,
        List<SystemManager> systemManagers,
        List<MameManager> machines,
        FavoritesManager favoritesManager,
        MainWindow mainWindow
    )
    {
        InitializeComponent();

        _settings = settings;
        _systemManagers = systemManagers;
        _machines = machines;
        _mainWindow = mainWindow;
        _favoritesManager = favoritesManager;

        App.ApplyThemeToWindow(this);
        _ = LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        var favoritesConfig = FavoritesManager.LoadFavorites();
        FavoritesDataGrid.ItemsSource = _favoriteList;

        if (_machines == null || _systemManagers == null)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(new Exception("_machines or _systemManagers are null."), "_machines or _systemManagers are null.");

            return;
        }

        var itemsToProcess = new List<Favorite>();

        // Phase 1: Create all Favorite objects and add to _favoriteList
        foreach (var favoriteConfigItem in favoritesConfig.FavoriteList)
        {
            // Find machine description if available
            var machine = _machines.FirstOrDefault(m =>
                m.MachineName.Equals(Path.GetFileNameWithoutExtension(favoriteConfigItem.FileName),
                    StringComparison.OrdinalIgnoreCase));
            var machineDescription = machine?.Description ?? string.Empty;

            // Retrieve the system configuration for the favorite
            var systemManager = _systemManagers.FirstOrDefault(config =>
                config.SystemName.Equals(favoriteConfigItem.SystemName, StringComparison.OrdinalIgnoreCase));

            // Get the default emulator, e.g., the first one in the list
            var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

            var coverImagePath = GetCoverImagePath(favoriteConfigItem.SystemName, favoriteConfigItem.FileName);

            var favoriteItem = new Favorite
            {
                FileName = favoriteConfigItem.FileName,
                SystemName = favoriteConfigItem.SystemName,
                MachineDescription = machineDescription,
                DefaultEmulator = defaultEmulator,
                CoverImage = coverImagePath,
                FileSizeBytes = -1 // Initial value: "Calculating..."
            };

            _favoriteList.Add(favoriteItem);

            if (systemManager != null)
            {
                itemsToProcess.Add(favoriteItem);
            }
            else
            {
                var contextMessage = $"System manager not found for favorite: {favoriteConfigItem.SystemName} - {favoriteConfigItem.FileName}. File size not calculated.";
                _ = LogErrors.LogErrorAsync(new Exception(contextMessage), contextMessage);

                favoriteItem.FileSizeBytes = -2; // "N/A"
            }
        }

        // Phase 2: Use Parallel.ForEachAsync to load file sizes efficiently
        var itemsToDelete = new ConcurrentBag<Favorite>();

        await Parallel.ForEachAsync(
            itemsToProcess,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            async (favoriteItem, cancellationToken) =>
            {
                var systemManager = _systemManagers.FirstOrDefault(config =>
                    config.SystemName.Equals(favoriteItem.SystemName, StringComparison.OrdinalIgnoreCase));

                // This should not happen since we filtered above, but double-check
                if (systemManager == null)
                {
                    itemsToDelete.Add(favoriteItem);
                    return;
                }

                var filePath = PathHelper.FindFileInSystemFolders(systemManager, favoriteItem.FileName);

                try
                {
                    if (File.Exists(filePath))
                    {
                        var sizeToSet = new FileInfo(filePath).Length;
                        await Dispatcher.InvokeAsync(() =>
                        {
                            favoriteItem.FileSizeBytes = sizeToSet;
                        });
                    }
                    else
                    {
                        // File not found — mark for deletion
                        itemsToDelete.Add(favoriteItem);
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    var contextMessage = $"Error getting file size for favorite: {filePath}";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    await Dispatcher.InvokeAsync(() =>
                    {
                        favoriteItem.FileSizeBytes = -2; // "N/A"
                    });
                }
            });

        // Phase 3: Remove invalid items from UI and persist changes
        if (!itemsToDelete.IsEmpty)
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    var result = MessageBox.Show("Some favorites were not found in the HDD. Do you want to delete them?", "Invalid Favorites", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result != MessageBoxResult.Yes)
                    {
                        foreach (var item in itemsToDelete)
                        {
                            _favoriteList.Remove(item);
                        }

                        _favoritesManager.FavoriteList = _favoriteList;
                        _favoritesManager.SaveFavorites();
                    }
                });
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error during batch deletion of invalid favorites.");
            }
        }
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var systemConfig = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var defaultImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemConfig == null)
        {
            return defaultImagePath;
        }
        else
        {
            return FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemConfig);
        }
    }

    private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.FavoriteList = _favoriteList;
            _favoritesManager.SaveFavorites();

            PlaySoundEffects.PlayTrashSound();
            PreviewImage.Source = null;
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
        }
    }

    private void FavoritesPrepareForRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite)
            {
                return;
            }

            var systemManager = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemManager == null)
            {
                // Notify developer
                const string contextMessage = "systemManager is null for the selected favorite";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(systemManager, selectedFavorite.FileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == selectedFavorite.FileName && fav.SystemName == systemManager.SystemName);
                if (favoriteToRemove != null)
                {
                    var result = MessageBox.Show("Favorite file was not found on the HDD! Do you want to remove the favorite?", "Invalid Favorite", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        _favoriteList.Remove(favoriteToRemove);
                        _favoritesManager.FavoriteList = _favoriteList;
                        _favoritesManager.SaveFavorites();
                    }
                }

                return;
            }

            var emulatorManager = systemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "emulatorManager is null.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var context = new RightClickContext(
                PathHelper.FindFileInSystemFolders(systemManager, selectedFavorite.FileName),
                selectedFavorite.FileName,
                Path.GetFileNameWithoutExtension(selectedFavorite.FileName),
                selectedFavorite.SystemName,
                systemManager,
                _machines,
                _favoritesManager,
                _settings,
                null,
                selectedFavorite,
                emulatorManager,
                null,
                null,
                _mainWindow
            );

            UiHelpers.ContextMenu.AddRightClickReturnContextMenu(context);
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
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                PlaySoundEffects.PlayNotificationSound();
                await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
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

    private async Task LaunchGameFromFavorite(string fileName, string selectedSystemName)
    {
        try
        {
            var selectedSystemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
            if (selectedSystemManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedSystemManager is null.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(selectedSystemManager, fileName);

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) // Add null/empty check for resolved path
            {
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == selectedSystemName);
                if (favoriteToRemove != null)
                {
                    _favoriteList.Remove(favoriteToRemove);
                    _favoritesManager.FavoriteList = _favoriteList;
                    _favoritesManager.SaveFavorites();
                }

                // Notify developer
                var contextMessage = $"Favorite file does not exist or path resolution failed: {filePath}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.GameFileDoesNotExistMessageBox();

                return;
            }

            var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "emulatorManager is null.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var selectedEmulatorName = emulatorManager.EmulatorName;
            await GameLauncher.HandleButtonClick(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"There was an error launching the game from Favorites.\n" +
                                 $"File Path: {fileName}\n" +
                                 $"System Name: {selectedSystemName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void RemoveFavoriteFromXmlAndEmptyPreviewImage(Favorite selectedFavorite)
    {
        _favoriteList.Remove(selectedFavorite);
        _favoritesManager.FavoriteList = _favoriteList;
        _favoritesManager.SaveFavorites();

        PreviewImage.Source = null;
    }

    private async void LaunchGameWithDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            PlaySoundEffects.PlayNotificationSound();
            await LaunchGameFromFavorite(selectedFavorite.FileName, selectedFavorite.SystemName);
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
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite)
            {
                PreviewImage.Source = null; // Clear preview if nothing is selected
                return;
            }

            var imagePath = selectedFavorite.CoverImage;
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath); // Use the new ImageLoader to load the image

            PreviewImage.Source = loadedImage; // Assign the loaded image to the PreviewImage control
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the SetPreviewImageOnSelectionChanged method.");
        }
    }

    private void DeleteFavoriteWithDelButton(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;

        PlaySoundEffects.PlayTrashSound();

        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _favoriteList.Remove(selectedFavorite);
            _favoritesManager.FavoriteList = _favoriteList;
            _favoritesManager.SaveFavorites();
            PreviewImage.Source = null;
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
        }
    }
}
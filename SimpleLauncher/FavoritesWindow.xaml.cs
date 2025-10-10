using System;
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

        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _systemManagers = systemManagers ?? throw new ArgumentNullException(nameof(systemManagers));
        _machines = machines ?? throw new ArgumentNullException(nameof(machines));
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _favoritesManager = favoritesManager ?? throw new ArgumentNullException(nameof(favoritesManager));

        App.ApplyThemeToWindow(this);

        // Set the ItemsSource immediately to the empty collection
        FavoritesDataGrid.ItemsSource = _favoriteList;

        Loaded += FavoritesWindow_Loaded;
    }

    private async void FavoritesWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            LoadingMessage.Text = (string)Application.Current.TryFindResource("LoadingFavorites") ?? "Loading favorites...";
            await Task.Yield(); // Allow the UI to render the loading overlay before starting work

            try
            {
                // Step 1: Load and process all favorite data in a background thread
                var processedFavorites = await LoadAndProcessFavoritesAsync();

                // Step 2: Populate the UI collection on the UI thread
                _favoriteList.Clear();
                foreach (var fav in processedFavorites)
                {
                    _favoriteList.Add(fav);
                }

                // Step 3: Check for and remove entries with missing files, also in the background
                await DeleteMissingFavoritesAsync();

                // Step 4: Asynchronously calculate file sizes for the visible items
                _ = CalculateFileSizeAsync();
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error loading favorites data in FavoritesWindow_Loaded.");

                // Notify user
                MessageBoxLibrary.ErrorWhileAddingFavoritesMessageBox();
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the FavoritesWindow_Loaded method.");
        }
    }

    private Task<List<Favorite>> LoadAndProcessFavoritesAsync()
    {
        return Task.Run(() =>
        {
            var processedList = new List<Favorite>();
            foreach (var favoriteConfigItem in _favoritesManager.FavoriteList)
            {
                // Find machine description if available
                var machine = _machines.FirstOrDefault(m =>
                    m.MachineName.Equals(Path.GetFileNameWithoutExtension(favoriteConfigItem.FileName),
                        StringComparison.OrdinalIgnoreCase));
                var machineDescription = machine?.Description ?? string.Empty;

                // Retrieve the system manager for the favorite
                var systemManager = _systemManagers.FirstOrDefault(config =>
                    config.SystemName.Equals(favoriteConfigItem.SystemName, StringComparison.OrdinalIgnoreCase));

                // Get the default emulator (the first one in the list)
                var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

                // Get the cover image path for the favorite
                var coverImagePath = GetCoverImagePath(favoriteConfigItem.SystemName, favoriteConfigItem.FileName);

                var favoriteItem = new Favorite
                {
                    FileName = favoriteConfigItem.FileName,
                    SystemName = favoriteConfigItem.SystemName,
                    MachineDescription = machineDescription,
                    DefaultEmulator = defaultEmulator,
                    CoverImage = coverImagePath,
                    FileSizeBytes = -1
                };
                processedList.Add(favoriteItem);
            }

            return processedList;
        });
    }

    private async Task DeleteMissingFavoritesAsync()
    {
        var itemsToRemove = await Task.Run(() =>
        {
            var toRemove = new List<Favorite>();
            var currentFavorites = _favoriteList.ToList();
            foreach (var item in currentFavorites)
            {
                // Get System Manager for Favorite
                var systemManager = _systemManagers.FirstOrDefault(manager =>
                    manager.SystemName.Equals(item.SystemName, StringComparison.OrdinalIgnoreCase));

                if (systemManager == null) continue;

                var filePath = PathHelper.FindFileInSystemFolders(systemManager, item.FileName);
                if (!File.Exists(filePath))
                {
                    toRemove.Add(item);
                    DebugLogger.Log("Invalid Favorite queued for removal: " + item.FileName);
                }
            }

            return toRemove;
        });

        if (itemsToRemove.Count == 0) return;

        foreach (var item in itemsToRemove)
        {
            var itemInList = _favoriteList.FirstOrDefault(f => f.FileName == item.FileName && f.SystemName == item.SystemName);
            if (itemInList != null)
                _favoriteList.Remove(itemInList);
        }

        // // Update the injected manager with the current collection and save
        // _favoritesManager.FavoriteList = _favoriteList;
        _favoritesManager.SaveFavorites();

        // Explicitly refresh the data grid binding to ensure UI updates
        FavoritesDataGrid.Items.Refresh();
    }

    private async Task CalculateFileSizeAsync()
    {
        var itemsToProcess = _favoriteList.ToList(); // Create a snapshot to avoid collection modification during iteration

        await Parallel.ForEachAsync(itemsToProcess, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            async (favoriteItem, cancellationToken) =>
            {
                var systemManager = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(favoriteItem.SystemName, StringComparison.OrdinalIgnoreCase));

                if (systemManager == null)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        favoriteItem.FileSizeBytes = -2; // "N/A"
                    });
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
                        await Dispatcher.InvokeAsync(() =>
                        {
                            favoriteItem.FileSizeBytes = -2; // "N/A"
                        });
                    }
                }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        favoriteItem.FileSizeBytes = -2; // "N/A"
                    });

                    // Notify developer
                    var contextMessage = $"Error getting file size for favorite: {filePath}";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);
                }
            });
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var systemConfig = _systemManagers.FirstOrDefault(config =>
            config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var defaultImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemConfig == null)
        {
            return defaultImagePath;
        }
        else
        {
            return FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemConfig, _settings);
        }
    }

    private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            RemoveFavoriteFromXmlAndEmptyPreviewImage(selectedFavorite);
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

            var systemManager = _systemManagers.FirstOrDefault(config =>
                config.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));

            if (systemManager == null)
            {
                const string contextMessage = "systemManager is null for the selected favorite";
                _ = LogErrors.LogErrorAsync(null, contextMessage);
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(systemManager, selectedFavorite.FileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav =>
                    fav.FileName == selectedFavorite.FileName && fav.SystemName == systemManager.SystemName);

                if (favoriteToRemove != null)
                {
                    RemoveFavoriteFromXmlAndEmptyPreviewImage(favoriteToRemove);
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

            void OnRemovedCallback()
            {
                if (selectedFavorite != null)
                {
                    _favoriteList.Remove(selectedFavorite);
                    PreviewImage.Source = null; // Also clear the preview image
                }
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
                _mainWindow,
                OnRemovedCallback // *** FIX: Pass the callback to the context ***
            );

            var contextMenu = UiHelpers.ContextMenu.AddRightClickReturnContextMenu(context);
            if (contextMenu != null)
            {
                FavoritesDataGrid.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "There was an error in the right-click context menu.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
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
                await LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName);
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

    private async Task LaunchGameFromFavoriteAsync(string fileName, string selectedSystemName)
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
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav =>
                    fav.FileName == fileName && fav.SystemName == selectedSystemName);

                if (favoriteToRemove != null)
                {
                    RemoveFavoriteFromXmlAndEmptyPreviewImage(favoriteToRemove);
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
            await GameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
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
        // _favoritesManager.FavoriteList = _favoriteList;
        _favoritesManager.SaveFavorites();

        PreviewImage.Source = null;
    }

    private async void LaunchGameWithDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            PlaySoundEffects.PlayNotificationSound();
            await LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName);
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
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

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
            RemoveFavoriteFromXmlAndEmptyPreviewImage(selectedFavorite);
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
        }
    }
}

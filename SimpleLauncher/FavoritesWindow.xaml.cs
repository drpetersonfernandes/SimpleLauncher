using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.ContextMenu.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.Favorites.Models;
using SimpleLauncher.Services.FindAndLoadImages;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadAppSettings;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

internal partial class FavoritesWindow
{
    private static readonly string LogPath = GetLogPath.Path();
    private readonly FavoritesManager _favoritesManager;
    private readonly ObservableCollection<Favorite> _favoriteList = new();
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemManagers;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher _gameLauncher;
    private readonly PlaySoundEffects _playSoundEffects;

    internal FavoritesWindow(
        SettingsManager settings,
        List<SystemManager> systemManagers,
        List<MameManager> machines,
        FavoritesManager favoritesManager,
        MainWindow mainWindow,
        GamePadController gamePadController,
        GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects
    )
    {
        InitializeComponent();

        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _systemManagers = systemManagers ?? throw new ArgumentNullException(nameof(systemManagers));
        _machines = machines ?? throw new ArgumentNullException(nameof(machines));
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _favoritesManager = favoritesManager ?? throw new ArgumentNullException(nameof(favoritesManager));
        _gamePadController = gamePadController ?? throw new ArgumentNullException(nameof(gamePadController));
        _gameLauncher = gameLauncher ?? throw new ArgumentNullException(nameof(gameLauncher));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));

        App.ApplyThemeToWindow(this);

        // Set the ItemsSource immediately to the empty collection
        FavoritesDataGrid.ItemsSource = _favoriteList;

        Loaded += FavoritesWindowLoadedAsync;
    }

    private async void FavoritesWindowLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            LoadingMessage.Text = (string)Application.Current.TryFindResource("LoadingFavorites") ?? "Loading favorites...";
            await Task.Yield();

            // ✅ CAPTURE on UI thread before background work
            var favoritesSnapshot = _favoritesManager.FavoriteList.ToList();
            var systemManagersSnapshot = _systemManagers.ToList();
            var machinesSnapshot = _machines.ToList();

            try
            {
                // Step 1: Pass the snapshot to background processing
                var processedFavorites = await LoadAndProcessFavoritesAsync(
                    favoritesSnapshot,
                    systemManagersSnapshot,
                    machinesSnapshot);

                // Step 2: Populate UI collection on UI thread
                _favoriteList.Clear();
                foreach (var fav in processedFavorites)
                {
                    _favoriteList.Add(fav);
                }

                // Step 3: Clean up missing files (also using snapshots)
                await DeleteMissingFavoritesAsync(systemManagersSnapshot);
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading favorites data in FavoritesWindowLoadedAsync method.");
                MessageBoxLibrary.ErrorWhileAddingFavoritesMessageBox();
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in the FavoritesWindowLoadedAsync method.");
        }
    }

    private Task<List<Favorite>> LoadAndProcessFavoritesAsync(
        List<Favorite> favoritesToProcess,
        IReadOnlyCollection<SystemManager> systemManagers,
        IReadOnlyCollection<MameManager> machines)
    {
        return Task.Run(() =>
        {
            var processedList = new List<Favorite>();
            foreach (var favoriteConfigItem in favoritesToProcess)
            {
                var machine = machines.FirstOrDefault(m =>
                    m.MachineName.Equals(Path.GetFileNameWithoutExtension(favoriteConfigItem.FileName), StringComparison.OrdinalIgnoreCase));

                var machineDescription = machine?.Description ?? string.Empty;

                var systemManager = systemManagers.FirstOrDefault(manager =>
                    manager.SystemName.Equals(favoriteConfigItem.SystemName, StringComparison.OrdinalIgnoreCase));

                var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

                var coverImagePath = GetCoverImagePath(favoriteConfigItem.SystemName, favoriteConfigItem.FileName);

                var favoriteItem = new Favorite
                {
                    FileName = favoriteConfigItem.FileName,
                    SystemName = favoriteConfigItem.SystemName,
                    MachineDescription = machineDescription,
                    DefaultEmulator = defaultEmulator,
                    CoverImage = coverImagePath
                };
                processedList.Add(favoriteItem);
            }

            return processedList;
        });
    }

    private async Task DeleteMissingFavoritesAsync(IReadOnlyCollection<SystemManager> systemManagers)
    {
        var itemsToRemove = await Task.Run(() =>
        {
            var toRemove = new List<Favorite>();
            // ✅ Work with a snapshot captured on UI thread
            var currentFavorites = _favoriteList.ToList();

            foreach (var item in currentFavorites)
            {
                var systemManager = systemManagers.FirstOrDefault(manager =>
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

        // ✅ All UI modifications happen on UI thread
        foreach (var item in itemsToRemove)
        {
            var itemInList = _favoriteList.FirstOrDefault(f =>
                f.FileName == item.FileName && f.SystemName == item.SystemName);

            if (itemInList != null)
            {
                _favoriteList.Remove(itemInList);
            }
        }

        _favoritesManager.FavoriteList = new ObservableCollection<Favorite>(_favoriteList);
        _favoritesManager.SaveFavorites();
        FavoritesDataGrid.Items.Refresh();
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var defaultImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemManager == null)
        {
            return defaultImagePath;
        }
        else
        {
            return FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemManager, _settings);
        }
    }

    private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
        {
            _playSoundEffects.PlayTrashSound();
            RemoveFavoriteFromDatabaseAndEmptyPreviewImage(selectedFavorite);
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
            // --- Only show context menu when right-clicking on an actual row ---
            var hitTestResult = VisualTreeHelper.HitTest(FavoritesDataGrid, e.GetPosition(FavoritesDataGrid));
            if (hitTestResult?.VisualHit == null) return;

            // Walk up the visual tree to find a DataGridRow
            var visual = hitTestResult.VisualHit;
            DataGridRow dataGridRow = null;
            while (visual != null && visual != FavoritesDataGrid)
            {
                if (visual is DataGridRow row)
                {
                    dataGridRow = row;
                    break;
                }

                visual = VisualTreeHelper.GetParent(visual);
            }

            if (dataGridRow == null) return; // Not clicking on a row - exit early

            // Select the row that was right-clicked
            dataGridRow.IsSelected = true;

            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite)
            {
                return;
            }

            var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedFavorite.SystemName, StringComparison.OrdinalIgnoreCase));

            if (systemManager == null)
            {
                const string contextMessage = "systemManager is null for the selected favorite";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(systemManager, selectedFavorite.FileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == selectedFavorite.FileName && fav.SystemName == systemManager.SystemName);

                if (favoriteToRemove != null)
                {
                    RemoveFavoriteFromDatabaseAndEmptyPreviewImage(favoriteToRemove);
                }

                return;
            }

            var emulatorManager = systemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "emulatorManager is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            void OnRemovedCallback()
            {
                if (true)
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
                _gamePadController,
                OnRemovedCallback,
                _gameLauncher,
                _playSoundEffects
            );

            var contextMenu = Services.ContextMenu.ContextMenu.AddRightClickReturnContextMenu(context);
            if (contextMenu != null)
            {
                FavoritesDataGrid.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "There was an error in the right-click context menu.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private async void LaunchGameClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
            {
                _playSoundEffects.PlayNotificationSound();
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
            const string contextMessage = "Error in the LaunchGameClickAsync method.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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
                const string contextMessage = "[LaunchGameFromFavoritesAsync] selectedSystemManager is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(selectedSystemManager, fileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName == fileName && fav.SystemName == selectedSystemName);

                if (favoriteToRemove != null)
                {
                    RemoveFavoriteFromDatabaseAndEmptyPreviewImage(favoriteToRemove);
                }

                // Notify developer
                var contextMessage = $"[LaunchGameFromFavoritesAsync] Favorite file does not exist or path resolution failed: {filePath}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.GameFileDoesNotExistMessageBox();

                return;
            }

            var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "[LaunchGameFromFavoritesAsync] emulatorManager is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var selectedEmulatorName = emulatorManager.EmulatorName;
            await _gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow, _gamePadController);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"[LaunchGameFromFavoritesAsync] There was an error launching the game from Favorites.\n" +
                                 $"File Path: {fileName}\n" +
                                 $"System Name: {selectedSystemName}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void RemoveFavoriteFromDatabaseAndEmptyPreviewImage(Favorite selectedFavorite)
    {
        _favoriteList.Remove(selectedFavorite);
        // Update the FavoritesManager's internal list with the cleaned local list
        _favoritesManager.FavoriteList = new ObservableCollection<Favorite>(_favoriteList);
        _favoritesManager.SaveFavorites();

        PreviewImage.Source = null;
        FavoritesDataGrid.ContextMenu = null; // Clear context menu after deletion
    }

    private async void LaunchGameWithDoubleClickAsync(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            _playSoundEffects.PlayNotificationSound();
            await LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method MouseDoubleClick.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private async void SetPreviewImageOnSelectionChangedAsync(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite)
            {
                PreviewImage.Source = null; // Clear preview if nothing is selected
                FavoritesDataGrid.ContextMenu = null; // Clear context menu when no selection
                return;
            }

            var imagePath = selectedFavorite.CoverImage;
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

            PreviewImage.Source = loadedImage; // Assign the loaded image to the PreviewImage control
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in the SetPreviewImageOnSelectionChangedAsync method.");
        }
    }

    private void FavoritesDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            switch (e.Key)
            {
                case Key.Delete:
                {
                    e.Handled = true; // Prevent DataGrid from handling Delete key

                    if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
                    {
                        _playSoundEffects.PlayTrashSound();
                        RemoveFavoriteFromDatabaseAndEmptyPreviewImage(selectedFavorite);
                    }
                    else
                    {
                        // Notify user
                        MessageBoxLibrary.SelectAFavoriteToRemoveMessageBox();
                    }

                    break;
                }
                case Key.Enter:
                {
                    e.Handled = true; // Prevent DataGrid from moving to next row
                    if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
                    {
                        _playSoundEffects.PlayNotificationSound();
                        _ = LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName);
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error handling key press in FavoritesDataGrid.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }
    }
}
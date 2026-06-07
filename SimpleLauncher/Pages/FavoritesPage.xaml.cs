using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.FindCoverImage;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using ILoadingState = SimpleLauncher.Core.Services.LoadingInterface.ILoadingState;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher.Pages;

using ILoadingState = ILoadingState;

internal partial class FavoritesPage : ILoadingState
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly FavoritesManager _favoritesManager;
    private readonly ObservableCollection<Favorite> _favoriteList = [];
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemManagers;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher _gameLauncher;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IFindCoverImage _findCoverImage;
    private readonly IImageLoader _imageLoader;

    internal FavoritesPage(
        SettingsManager settings,
        List<SystemManager> systemManagers,
        List<MameManager> machines,
        FavoritesManager favoritesManager,
        MainWindow mainWindow,
        GamePadController gamePadController,
        GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects,
        IConfiguration configuration,
        ILogErrors logErrors,
        IFindCoverImage findCoverImage,
        IImageLoader imageLoader)
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
        _configuration = configuration;
        _logErrors = logErrors;
        _findCoverImage = findCoverImage ?? throw new ArgumentNullException(nameof(findCoverImage));
        _imageLoader = imageLoader ?? throw new ArgumentNullException(nameof(imageLoader));

        // Set the ItemsSource immediately to the empty collection
        FavoritesDataGrid.ItemsSource = _favoriteList;

        Loaded += FavoritesPageLoadedAsync;
    }

    private async void FavoritesPageLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingFavorites") ?? "Loading favorites...");
            await Task.Yield();

            // Wire Emergency Return Button from Template
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }

            // CAPTURE on UI thread before background work
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
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error loading favorites data in FavoritesPageLoadedAsync method.");
                MessageBoxLibrary.ErrorWhileAddingFavoritesMessageBox();
            }
            finally
            {
                SetLoadingState(false);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the FavoritesPageLoadedAsync method.");
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

                var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? (string)Application.Current.TryFindResource("UnknownString") ?? "Unknown";

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

    private void UpdateFavoritesManagerList()
    {
        _favoritesManager.FavoriteList.Clear();
        foreach (var favorite in _favoriteList)
        {
            _favoritesManager.FavoriteList.Add(favorite);
        }

        _favoritesManager.SaveFavoritesAsync();
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
            return _findCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemManager, _settings);
        }
    }

    private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = FavoritesDataGrid.SelectedItems.Cast<Favorite>().ToList();

        if (selectedItems.Count > 0)
        {
            _playSoundEffects.PlayTrashSound();

            foreach (var favorite in selectedItems)
            {
                _favoriteList.Remove(favorite);
            }

            UpdateFavoritesManagerList();

            PreviewImage.Source = null;
            FavoritesDataGrid.ContextMenu = null; // Clear context menu after deletion
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
                _logErrors.LogAndForget(null, contextMessage);
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(systemManager, selectedFavorite.FileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                // Ask user if they want to delete the favorite
                var result = MessageBoxLibrary.FavoriteFileDoesNotExistAskToDeleteMessageBox(filePath ?? selectedFavorite.FileName);
                if (result == MessageBoxResult.Yes)
                {
                    var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName.Equals(selectedFavorite.FileName, StringComparison.OrdinalIgnoreCase) && fav.SystemName.Equals(systemManager.SystemName, StringComparison.OrdinalIgnoreCase));

                    if (favoriteToRemove != null)
                    {
                        RemoveFavoriteFromDatabaseAndEmptyPreviewImage(favoriteToRemove);
                    }
                }

                return;
            }

            var emulatorManager = systemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "emulatorManager is null.";
                _logErrors.LogAndForget(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

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
                _playSoundEffects,
                this
            );

            var contextMenu = Services.ContextMenu.ContextMenu.AddRightClickReturnContextMenu(context, _logErrors, _findCoverImage);
            if (contextMenu != null)
            {
                FavoritesDataGrid.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "There was an error in the right-click context menu.";
            _logErrors.LogAndForget(ex, contextMessage);
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
                await LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName, this);
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
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    private async Task LaunchGameFromFavoriteAsync(string fileName, string selectedSystemName, ILoadingState loadingStateProvider)
    {
        try
        {
            var selectedSystemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
            if (selectedSystemManager == null)
            {
                // Notify developer
                const string contextMessage = "[LaunchGameFromFavoritesAsync] selectedSystemManager is null.";
                _logErrors.LogAndForget(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(selectedSystemManager, fileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                // Ask user if they want to delete the favorite
                var result = MessageBoxLibrary.FavoriteFileDoesNotExistAskToDeleteMessageBox(filePath ?? fileName);
                if (result == MessageBoxResult.Yes)
                {
                    var favoriteToRemove = _favoriteList.FirstOrDefault(fav => fav.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) && fav.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));

                    if (favoriteToRemove != null)
                    {
                        RemoveFavoriteFromDatabaseAndEmptyPreviewImage(favoriteToRemove);
                    }
                }

                // Notify developer
                var contextMessage = $"[LaunchGameFromFavoritesAsync] Favorite file does not exist or path resolution failed: {filePath}";
                _logErrors.LogAndForget(null, contextMessage);

                return;
            }

            var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "[LaunchGameFromFavoritesAsync] emulatorManager is null.";
                _logErrors.LogAndForget(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

                return;
            }

            var selectedEmulatorName = emulatorManager.EmulatorName;
            await _gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow, _gamePadController, loadingStateProvider);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"[LaunchGameFromFavoritesAsync] There was an error launching the game from Favorites.\n" +
                                 $"File Path: {fileName}\n" +
                                 $"System Name: {selectedSystemName}";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    private void RemoveFavoriteFromDatabaseAndEmptyPreviewImage(Favorite selectedFavorite)
    {
        _favoriteList.Remove(selectedFavorite);
        UpdateFavoritesManagerList();

        PreviewImage.Source = null;
        FavoritesDataGrid.ContextMenu = null; // Clear context menu after deletion
    }

    private async void LaunchGameWithDoubleClickAsync(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            _playSoundEffects.PlayNotificationSound();
            await LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName, this);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method MouseDoubleClick.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
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
            var (loadedImage, _) = await _imageLoader.LoadImageAsync(imagePath);

            // Race condition check: Only assign if the selected item hasn't changed
            if (FavoritesDataGrid.SelectedItem == selectedFavorite)
            {
                PreviewImage.Source = loadedImage; // Assign the loaded image to the PreviewImage control
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Error in the SetPreviewImageOnSelectionChangedAsync method.");
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

                    var selectedItems = FavoritesDataGrid.SelectedItems.Cast<Favorite>().ToList();

                    if (selectedItems.Count > 0)
                    {
                        _playSoundEffects.PlayTrashSound();

                        foreach (var favorite in selectedItems)
                        {
                            _favoriteList.Remove(favorite);
                        }

                        UpdateFavoritesManagerList();

                        PreviewImage.Source = null;
                        FavoritesDataGrid.ContextMenu = null; // Clear context menu after deletion
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
                        _ = LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName, this);
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error handling key press in FavoritesDataGrid.";
            _logErrors.LogAndForget(ex, contextMessage);
        }
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        if (isLoading)
        {
            LoadingOverlay.Content = message ?? (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
        }
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        LoadingOverlay.Visibility = Visibility.Collapsed;

        DebugLogger.Log("[Emergency] User forced overlay dismissal in FavoritesPage.");
        _mainWindow.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }
}
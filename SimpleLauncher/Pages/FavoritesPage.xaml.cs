using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;
using SimpleLauncher.WpfServices;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;

namespace SimpleLauncher.Pages;

internal partial class FavoritesPage : ILoadingState
{
    private readonly FavoritesViewModel _viewModel;
    private readonly MainWindow _mainWindow;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher _gameLauncher;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly List<MameManager> _machines;
    private readonly FavoritesManager _favoritesManager;
    private readonly SettingsManager _settings;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IConfiguration _configuration;
    private readonly IContextMenuFunctions _contextMenuFunctions;
    private readonly IDebugLogger _debugLogger;
    private readonly IContextMenuService _contextMenuService;

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
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        IContextMenuFunctions contextMenuFunctions,
        IDebugLogger debugLogger,
        IContextMenuService contextMenuService)
    {
        InitializeComponent();

        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _gamePadController = gamePadController ?? throw new ArgumentNullException(nameof(gamePadController));
        _gameLauncher = gameLauncher ?? throw new ArgumentNullException(nameof(gameLauncher));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
        _findCoverImage = findCoverImage ?? throw new ArgumentNullException(nameof(findCoverImage));
        _machines = machines ?? throw new ArgumentNullException(nameof(machines));
        _favoritesManager = favoritesManager ?? throw new ArgumentNullException(nameof(favoritesManager));
        _contextMenuFunctions = contextMenuFunctions ?? throw new ArgumentNullException(nameof(contextMenuFunctions));
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _contextMenuService = contextMenuService ?? throw new ArgumentNullException(nameof(contextMenuService));
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();

        _viewModel = new FavoritesViewModel(
            configuration,
            logErrors,
            favoritesManager,
            settings,
            systemManagers,
            machines,
            playSoundEffects,
            findCoverImage,
            imageLoader,
            _messageBox,
            App.ServiceProvider.GetRequiredService<IResourceProvider>());

        DataContext = _viewModel;

        Loaded += FavoritesPageLoadedAsync;
    }

    private async void FavoritesPageLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            // Wire Emergency Return Button from Template
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }

            await _viewModel.LoadFavoritesAsync();

            // Bind the loaded favorites to the DataGrid
            FavoritesDataGrid.ItemsSource = _viewModel.Favorites;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the FavoritesPageLoadedAsync method.");
        }
    }

    private async void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedItems = FavoritesDataGrid.SelectedItems.Cast<Favorite>().ToList();
            if (selectedItems.Count == 0)
            {
                await _messageBox.SelectAFavoriteToRemoveMessageBox();
                return;
            }

            _playSoundEffects.PlayTrashSound();

            foreach (var favorite in selectedItems)
            {
                _viewModel.RemoveFavoriteFromCollection(favorite);
            }

            PreviewImage.Source = null;
            FavoritesDataGrid.ContextMenu = null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method RemoveFavoriteButton_Click");
        }
    }

    private async void FavoritesPrepareForRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var hitTestResult = VisualTreeHelper.HitTest(FavoritesDataGrid, e.GetPosition(FavoritesDataGrid));
            if (hitTestResult?.VisualHit == null) return;

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

            if (dataGridRow == null) return;

            dataGridRow.IsSelected = true;

            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite) return;

            var systemManager = _viewModel.GetSystemManager(selectedFavorite.SystemName);
            if (systemManager == null)
            {
                _logErrors.LogAndForget(null, "systemManager is null for the selected favorite");
                await _messageBox.RightClickContextMenuErrorMessageBox();
                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(systemManager.SystemFolders, selectedFavorite.FileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var result = await _messageBox.FavoriteFileDoesNotExistAskToDeleteMessageBox(filePath ?? selectedFavorite.FileName);
                if (result == CoreMessageBoxResult.Yes)
                {
                    _viewModel.RemoveFavoriteFromCollection(selectedFavorite);
                }

                return;
            }

            var emulatorManager = systemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                _logErrors.LogAndForget(null, "emulatorManager is null.");
                await _messageBox.CouldNotLaunchThisGameMessageBox(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
                return;
            }

            void OnRemovedCallback()
            {
                _viewModel.RemoveFavoriteFromCollection(selectedFavorite);
            }

            var context = new RightClickContext(
                filePath,
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

            var contextMenu = _contextMenuService.AddRightClickReturnContextMenu(context, _findCoverImage, _contextMenuFunctions);
            if (contextMenu != null)
            {
                // Close the previous context menu before assigning a new one to prevent leaks.
                if (FavoritesDataGrid.ContextMenu is { IsOpen: true } oldMenu)
                {
                    oldMenu.IsOpen = false;
                }

                FavoritesDataGrid.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "There was an error in the right-click context menu.");
            await _messageBox.RightClickContextMenuErrorMessageBox();
        }
    }

    private async void LaunchGameClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite)
            {
                await _messageBox.SelectAGameToLaunchMessageBox();
                return;
            }

            _playSoundEffects.PlayNotificationSound();
            await LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method LaunchGameClickAsync");
        }
    }

    private async Task LaunchGameFromFavoriteAsync(string fileName, string selectedSystemName)
    {
        try
        {
            var selectedSystemManager = _viewModel.GetSystemManager(selectedSystemName);
            if (selectedSystemManager == null)
            {
                _logErrors.LogAndForget(null, "[LaunchGameFromFavoritesAsync] selectedSystemManager is null.");
                await _messageBox.CouldNotLaunchThisGameMessageBox(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(selectedSystemManager.SystemFolders, fileName);
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                var result = await _messageBox.FavoriteFileDoesNotExistAskToDeleteMessageBox(filePath ?? fileName);
                if (result == CoreMessageBoxResult.Yes)
                {
                    var favoriteToRemove = _viewModel.Favorites.FirstOrDefault(fav => fav.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                                                                                      && fav.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
                    if (favoriteToRemove != null)
                    {
                        _viewModel.RemoveFavoriteFromCollection(favoriteToRemove);
                    }
                }

                _logErrors.LogAndForget(null, $"[LaunchGameFromFavoritesAsync] File does not exist: {filePath}");
                return;
            }

            var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
            if (emulatorManager == null)
            {
                _logErrors.LogAndForget(null, "[LaunchGameFromFavoritesAsync] emulatorManager is null.");
                await _messageBox.CouldNotLaunchThisGameMessageBox(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
                return;
            }

            var selectedEmulatorName = emulatorManager.EmulatorName;
            await _gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, WpfWindowContext.FromMainWindow(_mainWindow), _gamePadController, this);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"[LaunchGameFromFavoritesAsync] Error launching: {fileName}, {selectedSystemName}");
            await _messageBox.CouldNotLaunchThisGameMessageBox(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
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
            _logErrors.LogAndForget(ex, "Error in the method MouseDoubleClick.");
            await _messageBox.CouldNotLaunchThisGameMessageBox(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    private async void SetPreviewImageOnSelectionChangedAsync(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (FavoritesDataGrid.SelectedItem is not Favorite selectedFavorite)
            {
                PreviewImage.Source = null;
                FavoritesDataGrid.ContextMenu = null;
                return;
            }

            await _viewModel.UpdatePreviewImageAsync(selectedFavorite.CoverImage);

            // Convert Stream to BitmapImage for WPF display
            if (FavoritesDataGrid.SelectedItem == selectedFavorite)
            {
                PreviewImage.Source = _viewModel.PreviewImageSource?.ToBitmapImage();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the SetPreviewImageOnSelectionChangedAsync method.");
        }
    }

    private async void FavoritesDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            switch (e.Key)
            {
                case Key.Delete:
                    {
                        e.Handled = true;

                        var selectedItems = FavoritesDataGrid.SelectedItems.Cast<Favorite>().ToList();
                        if (selectedItems.Count > 0)
                        {
                            _playSoundEffects.PlayTrashSound();
                            foreach (var favorite in selectedItems)
                            {
                                _viewModel.RemoveFavoriteFromCollection(favorite);
                            }

                            PreviewImage.Source = null;
                            FavoritesDataGrid.ContextMenu = null;
                        }
                        else
                        {
                            await _messageBox.SelectAFavoriteToRemoveMessageBox();
                        }

                        break;
                    }
                case Key.Enter:
                    {
                        e.Handled = true;
                        if (FavoritesDataGrid.SelectedItem is Favorite selectedFavorite)
                        {
                            _playSoundEffects.PlayNotificationSound();
                            await LaunchGameFromFavoriteAsync(selectedFavorite.FileName, selectedFavorite.SystemName);
                        }

                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error handling key press in FavoritesDataGrid.");
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

        _debugLogger.Log("[Emergency] User forced overlay dismissal in FavoritesPage.");
        _mainWindow.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }
}
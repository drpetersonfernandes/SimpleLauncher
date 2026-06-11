using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.ContextMenu;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;
using SimpleLauncher.WpfServices;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;
using ILoadingState = SimpleLauncher.Services.LoadingInterface.ILoadingState;

#nullable enable

namespace SimpleLauncher.Pages;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class PlayHistoryPage : ILoadingState
{
    private readonly PlayHistoryViewModel _viewModel;
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
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly IContextMenuFunctions _contextMenuFunctions;
    private readonly IDebugLogger _debugLogger;
    private CancellationTokenSource? _cancellationTokenSource;

    public PlayHistoryPage(List<SystemManager> systemManagers,
        List<MameManager> machines,
        SettingsManager settings,
        FavoritesManager favoritesManager,
        PlayHistoryManager playHistoryManager,
        MainWindow mainWindow,
        GamePadController gamePadController,
        GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects,
        IConfiguration configuration,
        ILogErrors logErrors,
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        IContextMenuFunctions contextMenuFunctions,
        IDebugLogger debugLogger)
    {
        InitializeComponent();

        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _gamePadController = gamePadController ?? throw new ArgumentNullException(nameof(gamePadController));
        _gameLauncher = gameLauncher ?? throw new ArgumentNullException(nameof(gameLauncher));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
        _findCoverImage = findCoverImage ?? throw new ArgumentNullException(nameof(findCoverImage));
        _machines = machines ?? throw new ArgumentNullException(nameof(machines));
        _favoritesManager = favoritesManager ?? throw new ArgumentNullException(nameof(favoritesManager));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _playHistoryManager = playHistoryManager ?? throw new ArgumentNullException(nameof(playHistoryManager));
        _contextMenuFunctions = contextMenuFunctions ?? throw new ArgumentNullException(nameof(contextMenuFunctions));
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();

        _viewModel = new PlayHistoryViewModel(
            configuration,
            logErrors,
            playHistoryManager,
            settings,
            systemManagers,
            machines,
            playSoundEffects,
            findCoverImage,
            imageLoader,
            _messageBox,
            App.ServiceProvider.GetRequiredService<IResourceProvider>());

        DataContext = _viewModel;

        _cancellationTokenSource = new CancellationTokenSource();

        Loaded += PlayHistoryPageLoadedAsync;
        Unloaded += PlayHistoryPage_Unloaded;
    }

    private void PlayHistoryPage_Unloaded(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private async void PlayHistoryPageLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            // Wire Emergency Return Button from Template
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }

            await _viewModel.LoadHistoryAsync();

            // Bind to DataGrid
            PlayHistoryDataGrid.ItemsSource = _viewModel.PlayHistoryList;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the PlayHistoryPageLoadedAsync method.");
        }
    }

    private async void PlayHistoryPrepareForRightClickContext(object sender, MouseButtonEventArgs e)
    {
        try
        {
            var clickedElement = e.OriginalSource as FrameworkElement;
            if (clickedElement?.DataContext is not PlayHistoryItem selectedItem)
            {
                PlayHistoryDataGrid.ContextMenu = null;
                return;
            }

            if (selectedItem.FileName == null)
            {
                _logErrors.LogAndForget(null, "History item filename is null");
                await _messageBox.RightClickContextMenuErrorMessageBox();
                return;
            }

            var systemManager = _viewModel.GetSystemManager(selectedItem.SystemName);
            if (systemManager == null)
            {
                _logErrors.LogAndForget(null, "systemManager is null");
                await _messageBox.RightClickContextMenuErrorMessageBox();
                return;
            }

            if (!File.Exists(selectedItem.FileName))
            {
                var result = await _messageBox.GameFileDoesNotExistAskToDeleteMessageBox(selectedItem.FileName);
                if (result == CoreMessageBoxResult.Yes)
                {
                    _viewModel.RemoveItem(selectedItem);
                    _debugLogger.Log($"The entry {selectedItem} was removed from the history by user request.");
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

            var context = new RightClickContext(
                selectedItem.FileName,
                selectedItem.FileName,
                Path.GetFileNameWithoutExtension(selectedItem.FileName),
                selectedItem.SystemName,
                systemManager,
                _machines,
                _favoritesManager,
                _settings,
                null,
                null,
                emulatorManager,
                null,
                null,
                _mainWindow,
                _gamePadController,
                null,
                _gameLauncher,
                _playSoundEffects,
                this
            );

            var contextMenu = Services.ContextMenu.ContextMenu.AddRightClickReturnContextMenu(context, _logErrors, _findCoverImage, _contextMenuFunctions);
            if (contextMenu != null)
            {
                PlayHistoryDataGrid.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "There was an error in the method PlayHistoryPrepareForRightClickContext.");
            await _messageBox.RightClickContextMenuErrorMessageBox();
        }
    }

    private async Task LaunchGameFromHistoryAsync(string fileName, string selectedSystemName)
    {
        var selectedSystemManager = _viewModel.GetSystemManager(selectedSystemName);
        if (selectedSystemManager == null)
        {
            _logErrors.LogAndForget(null, "[LaunchGameFromHistoryAsync] systemManager is null.");
            await _messageBox.CouldNotLaunchThisGameMessageBox(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
            return;
        }

        if (!File.Exists(fileName))
        {
            var result = await _messageBox.GameFileDoesNotExistAskToDeleteMessageBox(fileName);
            if (result == CoreMessageBoxResult.Yes)
            {
                var itemToRemove = _viewModel.PlayHistoryList.FirstOrDefault(item => item.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                                                                                     && item.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
                if (itemToRemove != null)
                {
                    _viewModel.RemoveItem(itemToRemove);
                }
            }

            return;
        }

        var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
        if (emulatorManager == null)
        {
            _logErrors.LogAndForget(null, "[LaunchGameFromHistoryAsync] emulatorManager is null.");
            await _messageBox.CouldNotLaunchThisGameMessageBox(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
            return;
        }

        // Store current selection for restore after refresh
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selected
            ? (selected.FileName, selected.SystemName)
            : (FileName: null, SystemName: null);

        var selectedEmulatorName = emulatorManager.EmulatorName;
        await _gameLauncher.HandleButtonClickAsync(fileName, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, WpfWindowContext.FromMainWindow(_mainWindow), _gamePadController, this);

        // Refresh data and restore selection
        _viewModel.RefreshAfterGameLaunch();
        PlayHistoryDataGrid.ItemsSource = _viewModel.PlayHistoryList;

        if (selectedItemIdentifier is (not null, not null))
        {
            var updatedItem = _viewModel.PlayHistoryList.FirstOrDefault(item =>
                item.FileName.Equals(selectedItemIdentifier.FileName, StringComparison.OrdinalIgnoreCase) &&
                item.SystemName.Equals(selectedItemIdentifier.SystemName, StringComparison.OrdinalIgnoreCase));
            if (updatedItem != null)
            {
                PlayHistoryDataGrid.SelectedItem = updatedItem;
                PlayHistoryDataGrid.ScrollIntoView(updatedItem);
            }
        }
    }

    private async void LaunchGameWithDoubleClickAsync(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem) return;

            _playSoundEffects.PlayNotificationSound();
            await LaunchGameFromHistoryAsync(selectedItem.FileName, selectedItem.SystemName);
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
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem)
            {
                PreviewImage.Source = null;
                return;
            }

            await _viewModel.UpdatePreviewImageAsync(selectedItem.CoverImage);

            if (PlayHistoryDataGrid.SelectedItem == selectedItem)
            {
                PreviewImage.Source = _viewModel.PreviewImageSource?.ToBitmapImage();
            }
        }
        catch (Exception ex)
        {
            PreviewImage.Source = null;
            _logErrors.LogAndForget(ex, "Error in the SetPreviewImageOnSelectionChangedAsync method.");
        }
    }

    private async void DeleteHistoryItemWithDelButton(object sender, KeyEventArgs e)
    {
        try
        {
            switch (e.Key)
            {
                case Key.Delete:
                {
                    var selectedItems = PlayHistoryDataGrid.SelectedItems.Cast<PlayHistoryItem>().ToList();
                    if (selectedItems.Count > 0)
                    {
                        _playSoundEffects.PlayTrashSound();
                        _viewModel.RemoveItems(selectedItems);
                        e.Handled = true;
                        PreviewImage.Source = null;
                    }
                    else
                    {
                        await _messageBox.SelectAHistoryItemToRemoveMessageBox();
                    }

                    PreviewImage.Source = null;
                    break;
                }
                case Key.Enter when PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem:
                    _playSoundEffects.PlayNotificationSound();
                    _ = LaunchGameFromHistoryAsync(selectedItem.FileName, selectedItem.SystemName);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    await _messageBox.SelectAGameToLaunchMessageBox();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method DeleteHistoryItemWithDelButton.");
        }
    }

    private void RestoreSelectionAfterSort((string? FileName, string? SystemName) selectedItemIdentifier)
    {
        if (selectedItemIdentifier.FileName == null || selectedItemIdentifier.SystemName == null) return;

        var updatedItem = _viewModel.PlayHistoryList.FirstOrDefault(item =>
            item.FileName.Equals(selectedItemIdentifier.FileName, StringComparison.OrdinalIgnoreCase) &&
            item.SystemName.Equals(selectedItemIdentifier.SystemName, StringComparison.OrdinalIgnoreCase));

        if (updatedItem != null)
        {
            PlayHistoryDataGrid.SelectedItem = updatedItem;
            PlayHistoryDataGrid.ScrollIntoView(updatedItem);
        }
    }

    private (string? FileName, string? SystemName) GetSelectedIdentifier()
    {
        return PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null);
    }

    private void SortByDate_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...");
        var identifier = GetSelectedIdentifier();

        _playSoundEffects.PlayNotificationSound();
        _viewModel.SortByDate();
        PlayHistoryDataGrid.ItemsSource = _viewModel.PlayHistoryList;

        RestoreSelectionAfterSort(identifier);
    }

    private void SortByTotalPlayTime_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...");
        var identifier = GetSelectedIdentifier();

        _playSoundEffects.PlayNotificationSound();
        _viewModel.SortByTotalPlayTime();
        PlayHistoryDataGrid.ItemsSource = _viewModel.PlayHistoryList;

        RestoreSelectionAfterSort(identifier);
    }

    private void SortByTimesPlayed_Click(object sender, RoutedEventArgs e)
    {
        _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...");
        var identifier = GetSelectedIdentifier();

        _playSoundEffects.PlayNotificationSound();
        _viewModel.SortByTimesPlayed();
        PlayHistoryDataGrid.ItemsSource = _viewModel.PlayHistoryList;

        RestoreSelectionAfterSort(identifier);
    }

    private async void RemoveHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedItems = PlayHistoryDataGrid.SelectedItems.Cast<PlayHistoryItem>().ToList();
            if (selectedItems.Count > 0)
            {
                _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("RemovingHistoryItem") ?? "Removing history item...");
                _playSoundEffects.PlayTrashSound();
                _viewModel.RemoveItems(selectedItems);
                PreviewImage.Source = null;
            }
            else
            {
                await _messageBox.SelectAHistoryItemToRemoveMessageBox();
            }

            PreviewImage.Source = null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method RemoveHistoryItemButton_Click.");
        }
    }

    private async void RemoveAllHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("RemovingAllHistoryItems") ?? "Removing all history items...");
            await _viewModel.RemoveAllCommand.ExecuteAsync(null);
            PreviewImage.Source = null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method RemoveAllHistoryItemButton_Click.");
        }
    }

    private async void LaunchGameClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
            {
                _playSoundEffects.PlayNotificationSound();
                await LaunchGameFromHistoryAsync(selectedItem.FileName, selectedItem.SystemName);
            }
            else
            {
                _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingGameFromHistory") ?? "Launching game from history...");
                await _messageBox.SelectAGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the LaunchGameClickAsync method.");
            await _messageBox.CouldNotLaunchThisGameMessageBox(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    public void SetLoadingState(bool isLoading, string? message = null)
    {
        LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        if (isLoading)
        {
            LoadingOverlay.Content = message;
        }
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        _cancellationTokenSource?.Cancel();
        LoadingOverlay.Visibility = Visibility.Collapsed;

        _debugLogger.Log("[Emergency] User forced overlay dismissal in PlayHistoryPage.");
        _mainWindow.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }
}
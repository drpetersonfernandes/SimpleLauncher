using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.FindCoverImage;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.WpfServices;
using SimpleLauncher.Core.Services.SettingsManager;
using ILoadingState = SimpleLauncher.Core.Services.LoadingInterface.ILoadingState;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;
using CoreMessageBoxResult = SimpleLauncher.Core.Interfaces.MessageBoxResult;

namespace SimpleLauncher.Pages;

using ILoadingState = ILoadingState;

public partial class PlayHistoryPage : ILoadingState
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private CancellationTokenSource _cancellationTokenSource;
    private const string TimeFormat = "HH:mm:ss";
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    private readonly PlayHistoryManager _playHistoryManager;
    private ObservableCollection<PlayHistoryItem> _playHistoryList;
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemManagers;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;
    private readonly FavoritesManager _favoritesManager;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher _gameLauncher;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IFindCoverImage _findCoverImage;
    private readonly IImageLoader _imageLoader;
    private readonly IMessageBoxLibraryService _messageBox;

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
        IFindCoverImage findCoverImage,
        IImageLoader imageLoader)
    {
        InitializeComponent();

        _systemManagers = systemManagers;
        _machines = machines;
        _settings = settings;
        _favoritesManager = favoritesManager;
        _playHistoryManager = playHistoryManager;
        _mainWindow = mainWindow;
        _gamePadController = gamePadController;
        _gameLauncher = gameLauncher;
        _playSoundEffects = playSoundEffects;
        _configuration = configuration;
        _logErrors = logErrors;
        _findCoverImage = findCoverImage;
        _imageLoader = imageLoader;
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();

        Loaded += PlayHistoryPageLoadedAsync;

        Unloaded += PlayHistoryPage_Unloaded;
    }

    private void PlayHistoryPage_Unloaded(object sender, RoutedEventArgs e)
    {
        // Cancel any pending background tasks
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

            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingHistory") ?? "Loading history...");
            await Task.Yield(); // Allow the UI to render the loading overlay

            try
            {
                // Step 1: Load and process all history data in a background thread
                var processedHistory = await LoadAndProcessHistoryAsync();

                // Step 2: Populate the UI collection on the UI thread
                _playHistoryList = new ObservableCollection<PlayHistoryItem>(processedHistory);

                // Step 3: Sort the data now that it's in the collection and bind to DataGrid
                SortByDate();
            }
            catch (Exception ex)
            {
                // Notify developer
                _logErrors.LogAndForget(ex, "Error loading play history data in PlayHistoryPageLoadedAsync.");

                // Notify user
                await _messageBox.ErrorLoadingRomHistoryMessageBox();
            }
            finally
            {
                SetLoadingState(false);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Error in the PlayHistoryPageLoadedAsync method.");
        }
    }

    private Task<List<PlayHistoryItem>> LoadAndProcessHistoryAsync()
    {
        return Task.Run(() =>
        {
            var processedList = new List<PlayHistoryItem>();
            foreach (var historyItemConfig in _playHistoryManager.PlayHistoryList)
            {
                var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItemConfig.FileName), StringComparison.OrdinalIgnoreCase));
                var machineDescription = machine?.Description ?? string.Empty;
                var systemManager = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(historyItemConfig.SystemName, StringComparison.OrdinalIgnoreCase));
                var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? (string)Application.Current.TryFindResource("UnknownString") ?? "Unknown";
                var coverImagePath = GetCoverImagePath(historyItemConfig.SystemName, historyItemConfig.FileName);

                var playHistoryItem = new PlayHistoryItem
                {
                    FileName = historyItemConfig.FileName,
                    SystemName = historyItemConfig.SystemName,
                    TotalPlayTime = historyItemConfig.TotalPlayTime,
                    TimesPlayed = historyItemConfig.TimesPlayed,
                    LastPlayDate = historyItemConfig.LastPlayDate,
                    LastPlayTime = historyItemConfig.LastPlayTime,
                    MachineDescription = machineDescription,
                    DefaultEmulator = defaultEmulator,
                    CoverImage = coverImagePath
                };
                processedList.Add(playHistoryItem);
            }

            return processedList;
        });
    }

    private void SortByDate()
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(item =>
                TryParseDateTime(item.LastPlayDate, item.LastPlayTime))
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private DateTime TryParseDateTime(string dateStr, string timeStr)
    {
        try
        {
            // Try ISO 8601 format first (culture-invariant, unambiguous: yyyy-MM-dd HH:mm:ss)
            if (DateTime.TryParseExact($"{dateStr} {timeStr}", "yyyy-MM-dd HH:mm:ss",
                    InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }

            // Try explicit unambiguous formats using InvariantCulture only.
            // We avoid current culture parsing to prevent incorrect interpretation
            // when users switch OS region settings (e.g., US vs UK date formats).
            string[] dateFormats =
            [
                "yyyy/MM/dd", "yyyy.MM.dd", "dd.MM.yyyy",
                "MM/dd/yyyy", "dd/MM/yyyy", "dd-MM-yyyy",
                "d", "D"
            ];
            foreach (var df in dateFormats)
            {
                if (!DateTime.TryParseExact($"{dateStr} {timeStr}",
                        $"{df} {TimeFormat}", InvariantCulture, DateTimeStyles.None, out result)) continue;

                return result;
            }

            // Fallback: Try with InvariantCulture (assumes US format for ambiguous dates)
            if (DateTime.TryParse($"{dateStr} {timeStr}", InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            // If all parsing attempts fail, return DateTime.MinValue
            // This will put unparseable dates at the end of the sorted list
            return DateTime.MinValue;
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Error parsing date and time.\n" +
                                        $"dateStr: {dateStr}\n" +
                                        $"timeStr: {timeStr}");

            // In case of any exception, return a reasonable default
            return DateTime.MinValue;
        }
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var defaultCoverImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemManager == null)
        {
            return defaultCoverImagePath;
        }
        else
        {
            // Use FindCoverImage which already handles system-specific paths and fuzzy matching
            return _findCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemManager, _settings);
        }
    }

    private async void PlayHistoryPrepareForRightClickContext(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // Check if click was on an actual row with data
            var clickedElement = e.OriginalSource as FrameworkElement;
            if (clickedElement?.DataContext is not PlayHistoryItem selectedItem)
            {
                // Click was on empty space or header, don't show context menu
                PlayHistoryDataGrid.ContextMenu = null;
                return;
            }

            if (selectedItem.FileName == null)
            {
                // Notify developer
                const string contextMessage = "History item filename is null";
                _logErrors.LogAndForget(null, contextMessage);

                // Notify user
                await _messageBox.RightClickContextMenuErrorMessageBox();

                return;
            }

            var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemManager == null)
            {
                // Notify developer
                const string contextMessage = "systemManager is null";
                _logErrors.LogAndForget(null, contextMessage);

                // Notify user
                await _messageBox.RightClickContextMenuErrorMessageBox();

                return;
            }

            if (!File.Exists(selectedItem.FileName))
            {
                // Show message box asking user if they want to delete the entry
                var result = await _messageBox.GameFileDoesNotExistAskToDeleteMessageBox(selectedItem.FileName);
                if (result == CoreMessageBoxResult.Yes)
                {
                    var itemToRemove = _playHistoryList.FirstOrDefault(item => item.FileName.Equals(selectedItem.FileName, StringComparison.OrdinalIgnoreCase) && item.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));
                    if (itemToRemove != null)
                    {
                        _playHistoryList.Remove(itemToRemove);
                        _playHistoryManager.PlayHistoryList = _playHistoryList;
                        _ = _playHistoryManager.SavePlayHistoryAsync();

                        DebugLogger.Log("The entry " + itemToRemove + " was removed from the history by user request.");
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
                await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

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

            var contextMenu = Services.ContextMenu.ContextMenu.AddRightClickReturnContextMenu(context, _logErrors, _findCoverImage);
            if (contextMenu != null)
            {
                PlayHistoryDataGrid.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error in the method PlayHistoryPrepareForRightClickContext.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await _messageBox.RightClickContextMenuErrorMessageBox();
        }
    }

    private async Task LaunchGameFromHistoryAsync(string fileName, string selectedSystemName, ILoadingState loadingStateProvider)
    {
        var selectedSystemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
        if (selectedSystemManager == null)
        {
            // Notify developer
            const string contextMessage = "[LaunchGameFromHistoryAsync] systemManager is null.";
            _logErrors.LogAndForget(null, contextMessage);

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

            return;
        }

        if (!File.Exists(fileName))
        {
            // Ask user if they want to delete the entry from history
            var result = await _messageBox.GameFileDoesNotExistAskToDeleteMessageBox(fileName);
            if (result == CoreMessageBoxResult.Yes)
            {
                var itemToRemove = _playHistoryList.FirstOrDefault(item => item.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) && item.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
                if (itemToRemove != null)
                {
                    _playHistoryList.Remove(itemToRemove);
                    _playHistoryManager.PlayHistoryList = _playHistoryList;
                    await _playHistoryManager.SavePlayHistoryAsync();
                }
            }

            return;
        }

        var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
        if (emulatorManager == null)
        {
            // Notify developer
            const string contextMessage = "[LaunchGameFromHistoryAsync] emulatorManager is null.";
            _logErrors.LogAndForget(null, contextMessage);

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));

            return;
        }

        var selectedEmulatorName = emulatorManager.EmulatorName;

        // Store currently selected item's identifier to restore selection after refresh
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null); // Use null elements if nothing is selected

        await _gameLauncher.HandleButtonClickAsync(fileName, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, WpfWindowContext.FromMainWindow(_mainWindow), _gamePadController, loadingStateProvider);

        RefreshPlayHistoryData(selectedItemIdentifier); // Restore selection after refresh
    }

    private void RefreshPlayHistoryData((string FileName, string SystemName) previousSelectedItemIdentifier = default)
    {
        try
        {
            if (_playHistoryManager == null)
            {
                DebugLogger.Log("PlayHistoryManager is null in RefreshPlayHistoryData");
                return;
            }

            var newPlayHistoryList = new ObservableCollection<PlayHistoryItem>();

            foreach (var historyItemConfig in _playHistoryManager.PlayHistoryList)
            {
                var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItemConfig.FileName), StringComparison.OrdinalIgnoreCase));
                var machineDescription = machine?.Description ?? string.Empty;
                var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(historyItemConfig.SystemName, StringComparison.OrdinalIgnoreCase));
                var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? (string)Application.Current.TryFindResource("UnknownString") ?? "Unknown";
                var coverImagePath = GetCoverImagePath(historyItemConfig.SystemName, historyItemConfig.FileName);

                var playHistoryItem = new PlayHistoryItem
                {
                    FileName = historyItemConfig.FileName,
                    SystemName = historyItemConfig.SystemName,
                    TotalPlayTime = historyItemConfig.TotalPlayTime,
                    TimesPlayed = historyItemConfig.TimesPlayed,
                    LastPlayDate = historyItemConfig.LastPlayDate,
                    LastPlayTime = historyItemConfig.LastPlayTime,
                    MachineDescription = machineDescription,
                    DefaultEmulator = defaultEmulator,
                    CoverImage = coverImagePath
                };
                newPlayHistoryList.Add(playHistoryItem);
            }

            _playHistoryList = newPlayHistoryList;

            SortByDate();

            if (previousSelectedItemIdentifier.FileName == null || previousSelectedItemIdentifier.SystemName == null)
            {
                return;
            }

            var (prevFileName, prevSystemName) = previousSelectedItemIdentifier;
            var updatedItem = _playHistoryList.FirstOrDefault(item =>
                item.FileName.Equals(prevFileName, StringComparison.OrdinalIgnoreCase) &&
                item.SystemName.Equals(prevSystemName, StringComparison.OrdinalIgnoreCase));

            if (updatedItem == null)
            {
                return;
            }

            PlayHistoryDataGrid.SelectedItem = updatedItem;
            PlayHistoryDataGrid.ScrollIntoView(updatedItem);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error refreshing play history data.";
            _logErrors.LogAndForget(ex, contextMessage);
        }
    }

    private async void LaunchGameWithDoubleClickAsync(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem)
            {
                return;
            }

            _playSoundEffects.PlayNotificationSound();
            await LaunchGameFromHistoryAsync(selectedItem.FileName, selectedItem.SystemName, this);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method MouseDoubleClick.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    private async void SetPreviewImageOnSelectionChangedAsync(object sender, SelectionChangedEventArgs e) // Changed to async void
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem)
            {
                PreviewImage.Source = null; // Clear preview if nothing is selected
                return;
            }

            var imagePath = selectedItem.CoverImage;
            var (imageStream, _) = await _imageLoader.LoadImageAsync(imagePath); // <--- Changed to await

            // Race condition check: Only assign if the selected item hasn't changed
            if (PlayHistoryDataGrid.SelectedItem == selectedItem)
            {
                PreviewImage.Source = imageStream.ToBitmapImage();
            }
        }
        catch (Exception ex)
        {
            // This catch block handles exceptions *not* caught by ImageLoader.LoadImageAsync
            // (which should be rare, as ImageLoader catches most file/loading issues).
            PreviewImage.Source = null; // Ensure image is cleared on error

            // Notify developer
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
                    // Get all selected items
                    var selectedItems = PlayHistoryDataGrid.SelectedItems.Cast<PlayHistoryItem>().ToList();

                    if (selectedItems.Count > 0)
                    {
                        _playSoundEffects.PlayTrashSound();

                        // Remove all selected items
                        foreach (var item in selectedItems)
                            _playHistoryList.Remove(item);

                        _playHistoryManager.PlayHistoryList = _playHistoryList;
                        await _playHistoryManager.SavePlayHistoryAsync();
                        e.Handled = true; // Prevent DataGrid from handling Delete key
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
                    _ = LaunchGameFromHistoryAsync(selectedItem.FileName, selectedItem.SystemName, this);
                    e.Handled = true; // Prevent DataGrid from moving selection to next row
                    break;
                case Key.Enter:
                    await _messageBox.SelectAGameToLaunchMessageBox();
                    break;
                default:
                    return;
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method DeleteHistoryItemWithDelButton.");
        }
    }

    private void SortByDate_Click(object sender, RoutedEventArgs e)
    {
        // Capture current selection identifier before sorting
        _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...");
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null);

        _playSoundEffects.PlayNotificationSound();
        SortByDate();

        if (selectedItemIdentifier.FileName == null || selectedItemIdentifier.SystemName == null) return;

        var (prevFileName, prevSystemName) = selectedItemIdentifier;
        var updatedItem = _playHistoryList.FirstOrDefault(item =>
            item.FileName.Equals(prevFileName, StringComparison.OrdinalIgnoreCase) &&
            item.SystemName.Equals(prevSystemName, StringComparison.OrdinalIgnoreCase));

        if (updatedItem == null) return;

        PlayHistoryDataGrid.SelectedItem = updatedItem;
        PlayHistoryDataGrid.ScrollIntoView(updatedItem);
    }

    private void SortByTotalPlayTime_Click(object sender, RoutedEventArgs e)
    {
        // Capture current selection identifier before sorting
        _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...");
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null);

        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item => item.TotalPlayTime)
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;

        _playSoundEffects.PlayNotificationSound();

        if (selectedItemIdentifier.FileName == null || selectedItemIdentifier.SystemName == null) return;

        {
            var (prevFileName, prevSystemName) = selectedItemIdentifier;
            var updatedItem = _playHistoryList.FirstOrDefault(item =>
                item.FileName.Equals(prevFileName, StringComparison.OrdinalIgnoreCase) &&
                item.SystemName.Equals(prevSystemName, StringComparison.OrdinalIgnoreCase));

            if (updatedItem == null) return;

            PlayHistoryDataGrid.SelectedItem = updatedItem;
            PlayHistoryDataGrid.ScrollIntoView(updatedItem);
        }
    }

    private async void RemoveHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get all selected items
            var selectedItems = PlayHistoryDataGrid.SelectedItems.Cast<PlayHistoryItem>().ToList();

            if (selectedItems.Count > 0)
            {
                _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("RemovingHistoryItem") ?? "Removing history item...");

                _playSoundEffects.PlayTrashSound();

                // Remove all selected items
                foreach (var item in selectedItems)
                {
                    _playHistoryList.Remove(item);
                }

                _playHistoryManager.PlayHistoryList = _playHistoryList;
                _ = _playHistoryManager.SavePlayHistoryAsync();

                PreviewImage.Source = null;
            }
            else
            {
                // Notify the user to select a history item first
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
            var result = await _messageBox.ReallyWantToRemoveAllPlayHistoryMessageBox();
            _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("RemovingAllHistoryItems") ?? "Removing all history items...");

            if (result == CoreMessageBoxResult.Yes)
            {
                _playHistoryList.Clear();
                _playHistoryManager.PlayHistoryList = _playHistoryList;
                await _playHistoryManager.SavePlayHistoryAsync();

                _playSoundEffects.PlayTrashSound();

                PreviewImage.Source = null;
            }
            else
            {
                return;
            }

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
                await LaunchGameFromHistoryAsync(selectedItem.FileName, selectedItem.SystemName, this);
            }
            else
            {
                // Notify user
                _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LaunchingGameFromHistory") ?? "Launching game from history...");
                await _messageBox.SelectAGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the LaunchGameClickAsync method.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    private void SortByTimesPlayed_Click(object sender, RoutedEventArgs e)
    {
        // Capture current selection identifier before sorting
        _mainWindow.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...");
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null);

        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item => item.TimesPlayed)
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;

        _playSoundEffects.PlayNotificationSound();

        // Restore selection based on identifier
        // Check if the identifier tuple has non-null elements
        if (selectedItemIdentifier.FileName == null || selectedItemIdentifier.SystemName == null) return;

        {
            var (prevFileName, prevSystemName) = selectedItemIdentifier;
            var updatedItem = _playHistoryList.FirstOrDefault(item =>
                item.FileName.Equals(prevFileName, StringComparison.OrdinalIgnoreCase) &&
                item.SystemName.Equals(prevSystemName, StringComparison.OrdinalIgnoreCase));

            if (updatedItem == null) return;

            PlayHistoryDataGrid.SelectedItem = updatedItem;
            PlayHistoryDataGrid.ScrollIntoView(updatedItem);
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
        _cancellationTokenSource?.Cancel();
        LoadingOverlay.Visibility = Visibility.Collapsed;

        DebugLogger.Log("[Emergency] User forced overlay dismissal in PlayHistoryPage.");
        _mainWindow.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }
}
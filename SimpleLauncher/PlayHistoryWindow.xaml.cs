using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.FindAndLoadImages;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.UpdateStatusBar;
using SimpleLauncher.SharedModels;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

using ILoadingState = Services.LoadingInterface.ILoadingState;

public partial class PlayHistoryWindow : ILoadingState
{
    private readonly IConfiguration _configuration;
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

    public PlayHistoryWindow(List<SystemManager> systemManagers,
        List<MameManager> machines,
        SettingsManager settings,
        FavoritesManager favoritesManager,
        PlayHistoryManager playHistoryManager,
        MainWindow mainWindow,
        GamePadController gamePadController,
        GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects,
        IConfiguration configuration)
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

        App.ApplyThemeToWindow(this);
        Closed += PlayHistoryWindow_Closed;

        Loaded += PlayHistoryWindowLoadedAsync;
    }

    private async void PlayHistoryWindowLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingHistory") ?? "Loading history...");
            await Task.Yield(); // Allow the UI to render the loading overlay

            try
            {
                // Step 1: Load and process all history data in a background thread
                var processedHistory = await LoadAndProcessHistoryAsync();

                // Step 2: Populate the UI collection on the UI thread
                _playHistoryList = new ObservableCollection<PlayHistoryItem>(processedHistory);

                // Step 3: Check for and remove entries with missing files in the background
                await DeleteMissingEntriesAsync();

                // Step 4: Sort the data now that it's in the collection and bind to DataGrid
                SortByDate();
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading play history data in PlayHistoryWindowLoadedAsync.");

                // Notify user
                MessageBoxLibrary.ErrorLoadingRomHistoryMessageBox();
            }
            finally
            {
                SetLoadingState(false);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in the PlayHistoryWindowLoadedAsync method.");
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
                var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";
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

    private async Task DeleteMissingEntriesAsync()
    {
        var itemsToRemove = await Task.Run(() =>
        {
            var toRemove = new List<PlayHistoryItem>();
            var currentHistory = _playHistoryList.ToList();
            foreach (var item in currentHistory)
            {
                // Since FileName now stores the full path, we check existence directly
                if (!File.Exists(item.FileName))
                {
                    toRemove.Add(item);
                }
            }

            return toRemove;
        });

        if (itemsToRemove.Count == 0) return;

        foreach (var itemToRemove in itemsToRemove)
        {
            var itemInList = _playHistoryList.FirstOrDefault(i => i.FileName == itemToRemove.FileName && i.SystemName == itemToRemove.SystemName);
            if (itemInList != null)
            {
                _playHistoryList.Remove(itemInList);
                DebugLogger.Log("Invalid Play History entry removed: " + itemToRemove.FileName);
            }
        }

        _playHistoryManager.PlayHistoryList = _playHistoryList;
        _playHistoryManager.SavePlayHistory();

        // Explicitly refresh the data grid binding to ensure UI updates
        PlayHistoryDataGrid.ItemsSource = null;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private void SortByDate()
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item =>
                TryParseDateTime(item.LastPlayDate, item.LastPlayTime))
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;
    }

    private static DateTime TryParseDateTime(string dateStr, string timeStr)
    {
        try
        {
            // First, try to parse using current culture (most likely to succeed)
            if (DateTime.TryParse($"{dateStr} {timeStr}", out var result))
            {
                return result;
            }

            // If that fails, try with invariant culture
            if (DateTime.TryParse($"{dateStr} {timeStr}", InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            // As a fallback, try common formats
            string[] dateFormats = ["MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "d", "D"];
            foreach (var df in dateFormats)
            {
                if (!DateTime.TryParseExact($"{dateStr} {timeStr}",
                        $"{df} {TimeFormat}", InvariantCulture, DateTimeStyles.None, out result)) continue;

                return result;
            }

            // If all parsing attempts fail, return DateTime.MinValue
            // This will put unparseable dates at the end of the sorted list
            return DateTime.MinValue;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error parsing date and time.\n" +
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
            return FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemManager, _settings);
        }
    }

    private void PlayHistoryPrepareForRightClickContext(object sender, MouseButtonEventArgs e)
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemManager == null)
            {
                // Notify developer
                const string contextMessage = "systemManager is null";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            if (!File.Exists(selectedItem.FileName))
            {
                var itemToRemove = _playHistoryList.FirstOrDefault(item => item.FileName == selectedItem.FileName && item.SystemName == selectedItem.SystemName);
                if (itemToRemove != null)
                {
                    _playHistoryList.Remove(itemToRemove);
                    _playHistoryManager.PlayHistoryList = _playHistoryList;
                    _playHistoryManager.SavePlayHistory();

                    DebugLogger.Log("The entry " + itemToRemove + " was removed from the history.");
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
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_configuration["LogPath"]);

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

            var contextMenu = Services.ContextMenu.ContextMenu.AddRightClickReturnContextMenu(context);
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private async Task LaunchGameFromHistoryAsync(string fileName, string selectedSystemName, ILoadingState loadingStateProvider)
    {
        var selectedSystemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
        if (selectedSystemManager == null)
        {
            // Notify developer
            const string contextMessage = "[LaunchGameFromHistoryAsync] systemManager is null.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_configuration["LogPath"]);

            return;
        }

        if (!File.Exists(fileName))
        {
            // Auto remove the history item from the list since the file no longer exists
            var itemToRemove = _playHistoryList.FirstOrDefault(item => item.FileName == fileName && item.SystemName == selectedSystemName);
            if (itemToRemove != null)
            {
                _playHistoryList.Remove(itemToRemove);
                _playHistoryManager.PlayHistoryList = _playHistoryList;
                _playHistoryManager.SavePlayHistory();
            }

            // Notify user
            MessageBoxLibrary.GameFileDoesNotExistMessageBox();

            return;
        }

        var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
        if (emulatorManager == null)
        {
            // Notify developer
            const string contextMessage = "[LaunchGameFromHistoryAsync] emulatorManager is null.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_configuration["LogPath"]);

            return;
        }

        var selectedEmulatorName = emulatorManager.EmulatorName;

        // Store currently selected item's identifier to restore selection after refresh
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null); // Use null elements if nothing is selected

        await _gameLauncher.HandleButtonClickAsync(fileName, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow, _gamePadController, loadingStateProvider);

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

                var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_configuration["LogPath"]);
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
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath); // <--- Changed to await

            PreviewImage.Source = loadedImage;
        }
        catch (Exception ex)
        {
            // This catch block handles exceptions *not* caught by ImageLoader.LoadImageAsync
            // (which should be rare, as ImageLoader catches most file/loading issues).
            PreviewImage.Source = null; // Ensure image is cleared on error

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in the SetPreviewImageOnSelectionChangedAsync method.");
        }
    }

    private void DeleteHistoryItemWithDelButton(object sender, KeyEventArgs e)
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
                    _playHistoryManager.SavePlayHistory();
                    e.Handled = true; // Prevent DataGrid from handling Delete key
                    PreviewImage.Source = null;
                }
                else
                {
                    MessageBoxLibrary.SelectAHistoryItemToRemoveMessageBox();
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
                MessageBoxLibrary.SelectAGameToLaunchMessageBox();
                break;
            default:
                return;
        }
    }

    private void SortByDate_Click(object sender, RoutedEventArgs e)
    {
        // Capture current selection identifier before sorting
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...", _mainWindow);
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...", _mainWindow);
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

    private void RemoveHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        // Get all selected items
        var selectedItems = PlayHistoryDataGrid.SelectedItems.Cast<PlayHistoryItem>().ToList();

        if (selectedItems.Count > 0)
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("RemovingHistoryItem") ?? "Removing history item...", _mainWindow);

            _playSoundEffects.PlayTrashSound();

            // Remove all selected items
            foreach (var item in selectedItems)
            {
                _playHistoryList.Remove(item);
            }

            _playHistoryManager.PlayHistoryList = _playHistoryList;
            _playHistoryManager.SavePlayHistory();

            PreviewImage.Source = null;
        }
        else
        {
            // Notify the user to select a history item first
            MessageBoxLibrary.SelectAHistoryItemToRemoveMessageBox();
        }

        PreviewImage.Source = null;
    }

    private void RemoveAllHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBoxLibrary.ReallyWantToRemoveAllPlayHistoryMessageBox();
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("RemovingAllHistoryItems") ?? "Removing all history items...", _mainWindow);

        if (result == MessageBoxResult.Yes)
        {
            _playHistoryList.Clear();
            _playHistoryManager.PlayHistoryList = _playHistoryList;
            _playHistoryManager.SavePlayHistory();

            _playSoundEffects.PlayTrashSound();

            PreviewImage.Source = null;
        }
        else
        {
            return;
        }

        PreviewImage.Source = null;
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
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LaunchingGameFromHistory") ?? "Launching game from history...", _mainWindow);
                MessageBoxLibrary.SelectAGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the LaunchGameClickAsync method.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_configuration["LogPath"]);
        }
    }

    private void SortByTimesPlayed_Click(object sender, RoutedEventArgs e)
    {
        // Capture current selection identifier before sorting
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SortingPlayHistory") ?? "Sorting play history...", _mainWindow);
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

    private void PlayHistoryWindow_Closed(object sender, EventArgs e)
    {
        // Cancel any pending background tasks
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
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

        DebugLogger.Log("[Emergency] User forced overlay dismissal in PlayHistoryWindow.");
        UpdateStatusBar.UpdateContent("Emergency reset performed.", Application.Current.MainWindow as MainWindow);
    }
}
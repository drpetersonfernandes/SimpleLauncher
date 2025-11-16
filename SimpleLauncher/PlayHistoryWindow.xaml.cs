using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

public partial class PlayHistoryWindow
{
    private const string TimeFormat = "HH:mm:ss";
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    private static readonly string LogPath = GetLogPath.Path();
    private readonly PlayHistoryManager _playHistoryManager;
    private ObservableCollection<PlayHistoryItem> _playHistoryList;
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemManagers;
    private readonly List<MameManager> _machines;
    private readonly MainWindow _mainWindow;
    private readonly FavoritesManager _favoritesManager;
    private readonly GamePadController _gamePadController;

    public PlayHistoryWindow(List<SystemManager> systemManagers, List<MameManager> machines, SettingsManager settings, FavoritesManager favoritesManager, PlayHistoryManager playHistoryManager, MainWindow mainWindow, GamePadController gamePadController)
    {
        InitializeComponent();

        _systemManagers = systemManagers;
        _machines = machines;
        _settings = settings;
        _favoritesManager = favoritesManager;
        _playHistoryManager = playHistoryManager;
        _mainWindow = mainWindow;
        _gamePadController = gamePadController;

        App.ApplyThemeToWindow(this);

        Loaded += PlayHistoryWindow_Loaded;
    }

    private async void PlayHistoryWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            LoadingMessage.Text = (string)Application.Current.TryFindResource("LoadingHistory") ?? "Loading history...";
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

                // Step 5: Asynchronously calculate file sizes for the visible items
                _ = LoadFileSizesAsync(_playHistoryList.ToList());
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error loading play history data in PlayHistoryWindow_Loaded.");

                // Notify user
                MessageBoxLibrary.ErrorLoadingRomHistoryMessageBox();
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the PlayHistoryWindow_Loaded method.");
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
                    CoverImage = coverImagePath,
                    FileSizeBytes = -1
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
                var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(item.SystemName, StringComparison.OrdinalIgnoreCase));
                if (systemManager == null) continue;

                var filePath = PathHelper.FindFileInSystemFolders(systemManager, item.FileName);
                if (!File.Exists(filePath))
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

    private async Task LoadFileSizesAsync(List<PlayHistoryItem> itemsToProcess)
    {
        await Parallel.ForEachAsync(itemsToProcess, async (item, cancellationToken) =>
        {
            var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(item.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemManager != null)
            {
                var filePath = PathHelper.FindFileInSystemFolders(systemManager, item.FileName);
                if (File.Exists(filePath))
                {
                    var sizeToSet = new FileInfo(filePath).Length;
                    await Dispatcher.InvokeAsync(() => { item.FileSizeBytes = sizeToSet; });
                }
                else
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        item.FileSizeBytes = -2; // Error state ("N/A")
                    });
                }
            }
            else
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    item.FileSizeBytes = -2; // Error state ("N/A")
                });

                // Notify developer
                var contextMessage = $"System manager not found for history item: {item.SystemName} - {item.FileName}. The item will be removed from history.";
                _ = LogErrors.LogErrorAsync(new Exception(contextMessage), contextMessage);
            }
        });
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
            _ = LogErrors.LogErrorAsync(ex, "Error parsing date and time.\n" +
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
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem)
            {
                return;
            }

            if (selectedItem.FileName == null)
            {
                // Notify developer
                const string contextMessage = "History item filename is null";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemManager == null)
            {
                // Notify developer
                const string contextMessage = "systemManager is null";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var filePath = PathHelper.FindFileInSystemFolders(systemManager, selectedItem.FileName);
            if (!File.Exists(filePath))
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
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

                return;
            }

            var context = new RightClickContext(
                PathHelper.FindFileInSystemFolders(systemManager, selectedItem.FileName),
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
                _gamePadController
            );

            var contextMenu = UiHelpers.ContextMenu.AddRightClickReturnContextMenu(context);
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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private async Task LaunchGameFromHistoryAsync(string fileName, string selectedSystemName)
    {
        var selectedSystemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
        if (selectedSystemManager == null)
        {
            // Notify developer
            const string contextMessage = "systemManager is null.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

            return;
        }

        var filePath = PathHelper.FindFileInSystemFolders(selectedSystemManager, fileName);
        if (!File.Exists(filePath))
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
            const string contextMessage = "emulatorManager is null.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

            return;
        }

        var selectedEmulatorName = emulatorManager.EmulatorName;

        // Store currently selected item's identifier to restore selection after refresh
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null); // Use null elements if nothing is selected

        await GameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow, _gamePadController);

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
                    CoverImage = coverImagePath,
                    FileSizeBytes = -1
                };
                newPlayHistoryList.Add(playHistoryItem);
            }

            _playHistoryList = newPlayHistoryList;

            SortByDate();

            _ = LoadFileSizesAsync(_playHistoryList.ToList());

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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private async void LaunchGameWithDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem)
            {
                return;
            }

            PlaySoundEffects.PlayNotificationSound();
            await LaunchGameFromHistoryAsync(selectedItem.FileName, selectedItem.SystemName);
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
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem)
            {
                PreviewImage.Source = null; // Clear preview if nothing is selected
                return;
            }

            var imagePath = selectedItem.CoverImage;
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

            PreviewImage.Source = loadedImage;
        }
        catch (Exception ex)
        {
            // This catch block handles exceptions *not* caught by ImageLoader.LoadImageAsync
            // (which should be rare, as ImageLoader catches most file/loading issues).
            PreviewImage.Source = null; // Ensure image is cleared on error

            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the SetPreviewImageOnSelectionChanged method.");
        }
    }

    private void DeleteHistoryItemWithDelButton(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete)
        {
            return;
        }

        if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
        {
            PlaySoundEffects.PlayTrashSound();

            _playHistoryList.Remove(selectedItem);
            _playHistoryManager.PlayHistoryList = _playHistoryList;
            _playHistoryManager.SavePlayHistory();
        }
        else
        {
            MessageBoxLibrary.SelectAHistoryItemToRemoveMessageBox();
        }
    }

    private void SortByDate_Click(object sender, RoutedEventArgs e)
    {
        // Capture current selection identifier before sorting
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null);

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
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null);

        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item => item.TotalPlayTime)
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;

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
        if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
        {
            _playHistoryList.Remove(selectedItem);
            _playHistoryManager.PlayHistoryList = _playHistoryList;
            _playHistoryManager.SavePlayHistory();

            PlaySoundEffects.PlayTrashSound();
            PreviewImage.Source = null;
        }
        else
        {
            // Notify the user to select a history item first
            MessageBoxLibrary.SelectAHistoryItemToRemoveMessageBox();
        }
    }

    private void RemoveAllHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBoxLibrary.ReallyWantToRemoveAllPlayHistoryMessageBox();

        if (result == MessageBoxResult.Yes)
        {
            _playHistoryList.Clear();
            _playHistoryManager.PlayHistoryList = _playHistoryList;
            _playHistoryManager.SavePlayHistory();

            PlaySoundEffects.PlayTrashSound();

            // Clear preview image
            PreviewImage.Source = null;
        }
        else
        {
            return;
        }
    }

    private async void LaunchGame_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
            {
                PlaySoundEffects.PlayNotificationSound();
                await LaunchGameFromHistoryAsync(selectedItem.FileName, selectedItem.SystemName);
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

    private void SortByTimesPlayed_Click(object sender, RoutedEventArgs e)
    {
        // Capture current selection identifier before sorting
        // Use a non-nullable tuple with nullable elements
        var selectedItemIdentifier = PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem
            ? (selectedItem.FileName, selectedItem.SystemName)
            : (FileName: null, SystemName: null);

        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item => item.TimesPlayed)
        );
        _playHistoryList = sorted;
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;

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
}
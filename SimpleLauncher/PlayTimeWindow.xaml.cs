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

public partial class PlayTimeWindow
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

    public PlayTimeWindow(List<SystemManager> systemManagers, List<MameManager> machines, SettingsManager settings, FavoritesManager favoritesManager, PlayHistoryManager playHistoryManager, MainWindow mainWindow)
    {
        InitializeComponent();

        _systemManagers = systemManagers;
        _machines = machines;
        _settings = settings;
        _favoritesManager = favoritesManager;
        _playHistoryManager = playHistoryManager;
        _mainWindow = mainWindow;

        App.ApplyThemeToWindow(this);
        LoadPlayHistory();
    }

    private void LoadPlayHistory()
    {
        var playHistoryConfig = PlayHistoryManager.LoadPlayHistory();
        _playHistoryList = new ObservableCollection<PlayHistoryItem>();
        foreach (var historyItem in playHistoryConfig.PlayHistoryList)
        {
            // Find machine description if available
            var machine = _machines.FirstOrDefault(m =>
                m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItem.FileName), StringComparison.OrdinalIgnoreCase));
            var machineDescription = machine?.Description ?? string.Empty;

            // Retrieve the system manager for the history item
            var systemManager = _systemManagers.FirstOrDefault(config =>
                config.SystemName.Equals(historyItem.SystemName, StringComparison.OrdinalIgnoreCase));

            // Get the default emulator. The first one in the list
            var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

            // Construct the file path and get the size
            long fileSizeBytes = 0; // Default to 0 if file not found or system config missing
            var coverImagePath = GetCoverImagePath(historyItem.SystemName, historyItem.FileName); // Get cover path here

            if (systemManager != null)
            {
                var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemManager.SystemFolder);
                var filePath = Path.Combine(systemFolderPath, historyItem.FileName);

                try
                {
                    if (File.Exists(filePath))
                    {
                        fileSizeBytes = new FileInfo(filePath).Length;
                    }
                    else
                    {
                        // Log a warning if a history file is missing during load
                        var contextMessage = $"History item file not found during load: {filePath}";
                        _ = LogErrors.LogErrorAsync(new FileNotFoundException(contextMessage, filePath), contextMessage);
                        // The LaunchGameFromHistory method will handle removal if the user tries to launch it.
                    }
                }
                catch (Exception ex)
                {
                    // Log other potential file access errors
                    var contextMessage = $"Error getting file size for history item: {filePath}";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);
                }
            }
            else
            {
                // Log a warning if the system config for a history item is missing
                var contextMessage = $"System config not found for history item: {historyItem.SystemName} - {historyItem.FileName}";
                _ = LogErrors.LogErrorAsync(new Exception(contextMessage), contextMessage);
            }

            var playHistoryItem = new PlayHistoryItem
            {
                FileName = historyItem.FileName,
                SystemName = historyItem.SystemName,
                TotalPlayTime = historyItem.TotalPlayTime,
                TimesPlayed = historyItem.TimesPlayed,
                LastPlayDate = historyItem.LastPlayDate,
                LastPlayTime = historyItem.LastPlayTime,
                MachineDescription = machineDescription,
                DefaultEmulator = defaultEmulator,
                CoverImage = coverImagePath,
                FileSizeBytes = fileSizeBytes
            };
            _playHistoryList.Add(playHistoryItem);
        }

        // Sort the list by date and time
        SortByDateSafely();

        // Add to the DataGrid
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
                if (DateTime.TryParseExact($"{dateStr} {timeStr}",
                        $"{df} {TimeFormat}", InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }
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

    private void SortByDateSafely()
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            _playHistoryList.OrderByDescending(static item =>
                TryParseDateTime(item.LastPlayDate, item.LastPlayTime))
        );
        _playHistoryList = sorted;
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemConfig = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var defaultCoverImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemConfig == null)
        {
            return defaultCoverImagePath;
        }
        else
        {
            // Use FindCoverImage which already handles system-specific paths and fuzzy matching
            return FindCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemConfig);
        }
    }

    private void RemoveHistoryItemButton_Click(object sender, RoutedEventArgs e)
    {
        if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
        {
            _playHistoryList.Remove(selectedItem);
            _playHistoryManager.PlayHistoryList = _playHistoryList; // Keep the instance in sync
            _playHistoryManager.SavePlayHistory(); // Save using the existing instance

            PlayClick.PlayTrashSound();
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
        // Ask for confirmation before removing all items
        var result = MessageBoxLibrary.ReallyWantToRemoveAllPlayHistoryMessageBox();

        if (result != MessageBoxResult.Yes) return;

        // Clear all items from the collection
        _playHistoryList.Clear();

        // Update the manager and save changes
        _playHistoryManager.PlayHistoryList = _playHistoryList;
        _playHistoryManager.SavePlayHistory();

        // Play sound effect
        PlayClick.PlayTrashSound();

        // Clear preview image
        PreviewImage.Source = null;
    }

    private void AddRightClickContextMenuPlayHistoryWindow(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem) return;

            // Check filename
            if (selectedItem.FileName == null)
            {
                // Notify developer
                const string contextMessage = "History item filename is null";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            // Check systemConfig
            var systemConfig = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(selectedItem.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemConfig == null)
            {
                // Notify developer
                const string contextMessage = "systemConfig is null";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.RightClickContextMenuErrorMessageBox();

                return;
            }

            var fileNameWithExtension = selectedItem.FileName;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedItem.FileName);
            var filePath = PathHelper.CombineAndResolveRelativeToAppDirectory(systemConfig.SystemFolder, selectedItem.FileName);

            AddRightClickContextMenuPlayHistoryWindow(fileNameWithExtension, selectedItem, fileNameWithoutExtension, systemConfig, filePath);
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
            if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
            {
                PlayClick.PlayNotificationSound();
                await LaunchGameFromHistory(selectedItem.FileName, selectedItem.SystemName);
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

    private async Task LaunchGameFromHistory(string fileName, string selectedSystemName)
    {
        var selectedSystemManager = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
        if (selectedSystemManager == null)
        {
            // Notify developer
            const string contextMessage = "systemManager is null.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);

            return;
        }

        var filePath = PathHelper.CombineAndResolveRelativeToAppDirectory(selectedSystemManager.SystemFolder, fileName);
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

            // Notify developer
            var contextMessage = $"History item file does not exist: {filePath}";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.GameFileDoesNotExistMessageBox();
        }

        var emulatorManager = selectedSystemManager.Emulators.FirstOrDefault();
        if (emulatorManager == null)
        {
            // Notify developer
            const string contextMessage = "emulatorManager is null.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

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

        await GameLauncher.HandleButtonClick(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);

        RefreshPlayHistoryData(selectedItemIdentifier);
    }

    /// <summary>
    /// Refreshes the play history data and attempts to restore the selection
    /// based on the unique identifier of the previously selected item.
    /// </summary>
    /// <param name="previousSelectedItemIdentifier">The (FileName, SystemName) tuple of the item that was selected before the refresh. Elements can be null if no item was selected.</param>
    private void RefreshPlayHistoryData((string FileName, string SystemName) previousSelectedItemIdentifier = default) // Use default for the tuple
    {
        try
        {
            // Get updated play history data
            var playHistoryConfig = PlayHistoryManager.LoadPlayHistory();
            var newPlayHistoryList = new ObservableCollection<PlayHistoryItem>();

            foreach (var historyItem in playHistoryConfig.PlayHistoryList)
            {
                // Find machine description if available
                var machine = _machines.FirstOrDefault(m =>
                    m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItem.FileName), StringComparison.OrdinalIgnoreCase));
                var machineDescription = machine?.Description ?? string.Empty;

                // Retrieve the system configuration for the history item
                var systemConfig = _systemManagers.FirstOrDefault(config =>
                    config.SystemName.Equals(historyItem.SystemName, StringComparison.OrdinalIgnoreCase));

                // Get the default emulator. The first one in the list
                var defaultEmulator = systemConfig?.Emulators.FirstOrDefault()?.EmulatorName ?? "Unknown";

                // Construct the file path and get the size
                long fileSizeBytes = 0; // Default to 0 if file not found or system config missing
                var coverImagePath = GetCoverImagePath(historyItem.SystemName, historyItem.FileName); // Get cover path here

                if (systemConfig != null)
                {
                    var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemConfig.SystemFolder);
                    var filePath = Path.Combine(systemFolderPath, historyItem.FileName);

                    try
                    {
                        if (File.Exists(filePath))
                        {
                            fileSizeBytes = new FileInfo(filePath).Length;
                        }
                        else
                        {
                            // Log a warning if a history file is missing during refresh
                            var contextMessage = $"History item file not found during refresh: {filePath}";
                            _ = LogErrors.LogErrorAsync(new FileNotFoundException(contextMessage, filePath), contextMessage);
                            // File will not be added to the new list if it doesn't exist, effectively removing it.
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log other potential file access errors
                        var contextMessage = $"Error getting file size during refresh for history item: {filePath}";
                        _ = LogErrors.LogErrorAsync(ex, contextMessage);
                    }
                }
                else
                {
                    // Log a warning if the system config for a history item is missing during refresh
                    var contextMessage = $"System config not found during refresh for history item: {historyItem.SystemName} - {historyItem.FileName}";
                    _ = LogErrors.LogErrorAsync(new Exception(contextMessage), contextMessage);
                }

                // Only add the item if the file exists (or if we couldn't determine the size but want to keep it anyway - current logic removes missing files)
                // The current logic implicitly removes missing files because fileSizeBytes remains 0 and we don't add it if systemConfig is null or file doesn't exist.
                // Let's explicitly check if systemConfig was found and the file existed to add it.
                if (systemConfig == null ||
                    !File.Exists(Path.Combine(PathHelper.ResolveRelativeToAppDirectory(systemConfig.SystemFolder),
                        historyItem.FileName))) continue;

                var playHistoryItem = new PlayHistoryItem
                {
                    FileName = historyItem.FileName,
                    SystemName = historyItem.SystemName,
                    TotalPlayTime = historyItem.TotalPlayTime,
                    TimesPlayed = historyItem.TimesPlayed,
                    LastPlayDate = historyItem.LastPlayDate,
                    LastPlayTime = historyItem.LastPlayTime,
                    MachineDescription = machineDescription,
                    DefaultEmulator = defaultEmulator,
                    CoverImage = coverImagePath, // Assign the retrieved cover path
                    FileSizeBytes = fileSizeBytes // Assign the calculated size in bytes
                };
                newPlayHistoryList.Add(playHistoryItem);
            }

            // Replace the old collection with the new one
            _playHistoryList = newPlayHistoryList;

            // Sort the list by date and time using the safe parsing method
            SortByDateSafely();

            // Update the DataGrid
            PlayHistoryDataGrid.ItemsSource = _playHistoryList;

            // Try to restore selection based on identifier
            // Check if the identifier tuple has non-null elements
            if (previousSelectedItemIdentifier.FileName == null ||
                previousSelectedItemIdentifier.SystemName == null) return;

            var (prevFileName, prevSystemName) = previousSelectedItemIdentifier; // Deconstruct the non-nullable tuple

            // Find the item in the refreshed list using the identifier
            var updatedItem = _playHistoryList.FirstOrDefault(item =>
                item.FileName.Equals(prevFileName, StringComparison.OrdinalIgnoreCase) &&
                item.SystemName.Equals(prevSystemName, StringComparison.OrdinalIgnoreCase));

            if (updatedItem == null) return;

            PlayHistoryDataGrid.SelectedItem = updatedItem;
            PlayHistoryDataGrid.ScrollIntoView(updatedItem);
            // If updatedItem is null, the previously selected item was likely removed
            // (e.g., if the file no longer exists and was auto-removed).
            // In this case, no item will be selected, which is the desired behavior.
            // If previousSelectedItemIdentifier had null elements (nothing was selected before),
            // the selection remains null, which is also correct.
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
            if (PlayHistoryDataGrid.SelectedItem is not PlayHistoryItem selectedItem) return;

            PlayClick.PlayNotificationSound();
            await LaunchGameFromHistory(selectedItem.FileName, selectedItem.SystemName);
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

            // Assign the loaded image to the PreviewImage control.
            // loadedImage will be null if even the default image failed to load.
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
        if (e.Key != Key.Delete) return;

        if (PlayHistoryDataGrid.SelectedItem is PlayHistoryItem selectedItem)
        {
            PlayClick.PlayTrashSound();

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

        SortByDateSafely();
        PlayHistoryDataGrid.ItemsSource = _playHistoryList;

        // Restore selection based on identifier
        // Check if the identifier tuple has non-null elements
        if (selectedItemIdentifier.FileName == null || selectedItemIdentifier.SystemName == null) return;

        var (prevFileName, prevSystemName) = selectedItemIdentifier; // Deconstruct the non-nullable tuple
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

        // Restore selection based on identifier
        // Check if the identifier tuple has non-null elements
        if (selectedItemIdentifier.FileName == null || selectedItemIdentifier.SystemName == null) return;

        {
            var (prevFileName, prevSystemName) = selectedItemIdentifier; // Deconstruct the non-nullable tuple
            var updatedItem = _playHistoryList.FirstOrDefault(item =>
                item.FileName.Equals(prevFileName, StringComparison.OrdinalIgnoreCase) &&
                item.SystemName.Equals(prevSystemName, StringComparison.OrdinalIgnoreCase));

            if (updatedItem == null) return;

            PlayHistoryDataGrid.SelectedItem = updatedItem;
            PlayHistoryDataGrid.ScrollIntoView(updatedItem);
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
            var (prevFileName, prevSystemName) = selectedItemIdentifier; // Deconstruct the non-nullable tuple
            var updatedItem = _playHistoryList.FirstOrDefault(item =>
                item.FileName.Equals(prevFileName, StringComparison.OrdinalIgnoreCase) &&
                item.SystemName.Equals(prevSystemName, StringComparison.OrdinalIgnoreCase));

            if (updatedItem == null) return;

            PlayHistoryDataGrid.SelectedItem = updatedItem;
            PlayHistoryDataGrid.ScrollIntoView(updatedItem);
        }
    }
}

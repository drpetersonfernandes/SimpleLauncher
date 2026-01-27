using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class MainWindow
{
    internal async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken cancellationToken = default)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("Loading") ?? "Loading...", this);
        Dispatcher.Invoke(() => SetUiLoadingState(true, (string)Application.Current.TryFindResource("LoadingGames") ?? "Loading Games..."));
        await SetUiBeforeLoadGameFilesAsync();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (SystemComboBox.SelectedItem == null)
            {
                await DisplaySystemSelectionScreenAsync(cancellationToken);
                return;
            }

            var selectedSystem = SystemComboBox.SelectedItem.ToString();
            var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystem);
            if (selectedManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedConfig is null.";
                _ = _logErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.InvalidSystemConfigMessageBox();

                await DisplaySystemSelectionScreenAsync(cancellationToken);

                return;
            }

            var allFiles = await BuildListOfAllFilesToLoad(selectedManager, startLetter, searchQuery, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (selectedManager.GroupByFolder)
            {
                static string EnsureTrailingSlash(string path)
                {
                    if (string.IsNullOrEmpty(path))
                        return path;

                    return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                }

                var rootFolders = selectedManager.SystemFolders
                    .Select(PathHelper.ResolveRelativeToAppDirectory)
                    .Where(static p => !string.IsNullOrEmpty(p))
                    .Select(EnsureTrailingSlash)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var groupedFiles = allFiles
                    .GroupBy(f =>
                    {
                        var fileDir = Path.GetDirectoryName(f);
                        var normalizedFileDir = EnsureTrailingSlash(fileDir);

                        // If the file's directory is one of the main system folders, it's a "root" file.
                        // Its group key will be its own full path.
                        if (rootFolders.Contains(normalizedFileDir))
                        {
                            return f;
                        }

                        // Otherwise, its group key is its parent directory.
                        return fileDir;
                    })
                    .Select(static g => g.Key) // This gives us a list of unique file paths (for root files) and directory paths (for subfolders)
                    .ToList();
                allFiles = groupedFiles;
            }

            allFiles = ProcessListOfAllFilesWithMachineDescription(selectedManager, allFiles);
            cancellationToken.ThrowIfCancellationRequested();

            allFiles = await FilterFilesByShowGamesSettingAsync(allFiles, selectedSystem, selectedManager);
            cancellationToken.ThrowIfCancellationRequested();

            allFiles = SetPaginationOfListOfFiles(allFiles);
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var filePath in allFiles) // 'filePath' is already resolved here
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_settings.ViewMode == "GridView") // GridView
                {
                    var gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedManager, this);
                    GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Add(gameButton));
                }
                else // ListView
                {
                    var gameListViewItem = await _gameListFactory.CreateGameListViewItemAsync(filePath, selectedSystem, selectedManager);
                    await Dispatcher.InvokeAsync(() => GameListItems.Add(gameListViewItem));
                }
            }

            switch (_settings.ViewMode)
            {
                case "GridView":
                    Scroller.Focus();
                    break;
                case "ListView":
                    GameDataGrid.Focus();
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            DebugLogger.Log("[LoadGameFilesAsync] Operation was canceled.");
            // Clear the UI to prevent showing partial results from the canceled operation.
            GameFileGrid.Dispatcher.Invoke(() => GameFileGrid.Children.Clear());
            await Dispatcher.InvokeAsync(() => GameListItems.Clear());
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method LoadGameFilesAsync.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMethodLoadGameFilesAsyncMessageBox();
        }
        finally
        {
            // Only reset the loading state if this is still the active task.
            // If cancelled, a newer task has taken over and will manage the state.
            if (cancellationToken == _cancellationSource.Token)
            {
                Dispatcher.Invoke(() => SetUiLoadingState(false));
            }
        }

        return;

        async Task<List<string>> BuildListOfAllFilesToLoad(SystemManager selectedManager, string startLetter2, string searchQuery2, CancellationToken token)
        {
            List<string> allFiles;

            switch (searchQuery2)
            {
                case "FAVORITES" or "RANDOM_SELECTION":
                    await _allGamesLock.WaitAsync(token);
                    try
                    {
                        if (_currentSearchResults != null && _currentSearchResults.Count != 0)
                        {
                            allFiles = new List<string>(_currentSearchResults);
                        }
                        else
                        {
                            allFiles = [];
                        }
                    }
                    finally
                    {
                        _allGamesLock.Release();
                    }

                    break;
                default: // This branch handles initial load, letter filter, and text search
                {
                    // If no specific filter (letter or search query), and _allGamesForCurrentSystem is already populated for this system,
                    // use it directly. Otherwise, perform a full disk scan.
                    // The _selectedSystem field ensures _allGamesForCurrentSystem is for the *currently active* system.
                    await _allGamesLock.WaitAsync(token); // Acquire lock before accessing _allGamesForCurrentSystem
                    try
                    {
                        if (string.IsNullOrWhiteSpace(startLetter2) && string.IsNullOrWhiteSpace(searchQuery2) &&
                            _allGamesForCurrentSystem != null && _allGamesForCurrentSystem.Count != 0 &&
                            _selectedSystem == selectedManager.SystemName)
                        {
                            allFiles = new List<string>(_allGamesForCurrentSystem); // READ
                            DebugLogger.Log($"[BuildListOfAllFilesToLoad] Reusing cached _allGamesForCurrentSystem for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                        }
                        else
                        {
                            // If _allGamesForCurrentSystem is not suitable or not yet populated, perform a full disk scan.
                            // This part is outside the lock because it involves I/O and doesn't modify _allGamesForCurrentSystem yet.
                            // The lock is acquired only when _allGamesForCurrentSystem is read or written.
                            _allGamesLock.Release(); // Temporarily release lock for disk scan

                            try
                            {
                                var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                foreach (var folder in selectedManager.SystemFolders)
                                {
                                    token.ThrowIfCancellationRequested();

                                    var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                                    if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath)) continue;

                                    var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch, token);
                                    foreach (var file in filesInFolder)
                                    {
                                        uniqueFiles.TryAdd(Path.GetFileName(file), file);
                                    }
                                }

                                allFiles = uniqueFiles.Values.ToList(); // This is the full list from disk for the system
                            }
                            finally
                            {
                                await _allGamesLock.WaitAsync(token); // Re-acquire lock before potentially writing to _allGamesForCurrentSystem
                            }

                            // If no specific filter (letter or search query), this is the "all games" list.
                            // Cache it for future "Feeling Lucky" calls and direct "All" view loads.
                            if (string.IsNullOrWhiteSpace(startLetter2) && string.IsNullOrWhiteSpace(searchQuery2))
                            {
                                _allGamesForCurrentSystem = new List<string>(allFiles); // WRITE
                                DebugLogger.Log($"[BuildListOfAllFilesToLoad] Populated _allGamesForCurrentSystem for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                            }
                        }
                    }
                    finally
                    {
                        _allGamesLock.Release(); // Ensure lock is released
                    }

                    // ... filtering by startLetter ...
                    if (!string.IsNullOrWhiteSpace(startLetter2))
                    {
                        allFiles = await FilterFilesAsync(allFiles, startLetter2);
                    }

                    // ... filtering by searchQuery ...
                    if (!string.IsNullOrWhiteSpace(searchQuery2) && searchQuery2 != "RANDOM_SELECTION" && searchQuery2 != "FAVORITES")
                    {
                        var systemIsMame = selectedManager.SystemIsMame;
                        var lowerQuery = searchQuery2.ToLowerInvariant();
                        allFiles = await Task.Run(() =>
                            allFiles.FindAll(file =>
                            {
                                var fileName = Path.GetFileNameWithoutExtension(file);
                                var filenameMatch = fileName.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                if (filenameMatch) return true;

                                if (systemIsMame && _mameLookup != null && _mameLookup.TryGetValue(fileName, out var description))
                                {
                                    return description.Contains(lowerQuery, StringComparison.OrdinalIgnoreCase);
                                }

                                return false;
                            }), token);

                        await _allGamesLock.WaitAsync(token);
                        try
                        {
                            _currentSearchResults = new List<string>(allFiles); // Store search results
                        }
                        finally
                        {
                            _allGamesLock.Release();
                        }
                    }

                    break;
                }
            }

            return allFiles;
        }

        List<string> ProcessListOfAllFilesWithMachineDescription(SystemManager selectedManager, List<string> allFiles)
        {
            if (selectedManager.SystemIsMame && _mameSortOrder == "MachineDescription")
            {
                allFiles = allFiles.OrderBy(f =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(f);
                    return _mameLookup.TryGetValue(fileName, out var description) && !string.IsNullOrWhiteSpace(description)
                        ? description
                        : fileName;
                }, StringComparer.OrdinalIgnoreCase).ToList();
            }
            else
            {
                allFiles = allFiles.OrderBy(static f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase).ToList();
            }

            return allFiles;
        }
    }
}
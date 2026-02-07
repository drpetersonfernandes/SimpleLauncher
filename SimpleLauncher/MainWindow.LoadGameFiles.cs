using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GetListOfFiles;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.UpdateStatusBar;
using SimpleLauncher.SharedModels;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

public partial class MainWindow
{
    internal async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken cancellationToken = default)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("Loading") ?? "Loading...", this);

        // Only set generic message if not already loading a specific action
        if (!_isLoadingGames)
        {
            Dispatcher.Invoke(() => SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingGames") ?? "Loading Games..."));
        }

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

            const int batchSize = 100;

            if (_settings.ViewMode == "GridView")
            {
                var buttonBatch = new List<Button>(Math.Min(batchSize, allFiles.Count));

                foreach (var filePath in allFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedManager, this);
                    buttonBatch.Add(gameButton);

                    if (buttonBatch.Count >= batchSize)
                    {
                        GameFileGrid.Dispatcher.Invoke(() =>
                        {
                            foreach (var btn in buttonBatch)
                                GameFileGrid.Children.Add(btn);
                        });
                        buttonBatch.Clear();
                    }
                }

                if (buttonBatch.Count > 0)
                {
                    GameFileGrid.Dispatcher.Invoke(() =>
                    {
                        foreach (var btn in buttonBatch)
                            GameFileGrid.Children.Add(btn);
                    });
                }
            }
            else // ListView
            {
                var itemBatch = new List<GameListViewItem>(Math.Min(batchSize, allFiles.Count));

                foreach (var filePath in allFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var gameListViewItem = await _gameListFactory.CreateGameListViewItemAsync(filePath, selectedSystem, selectedManager);
                    itemBatch.Add(gameListViewItem);

                    if (itemBatch.Count >= batchSize)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            foreach (var item in itemBatch)
                                GameListItems.Add(item);
                        });
                        itemBatch.Clear();
                    }
                }

                if (itemBatch.Count > 0)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        foreach (var item in itemBatch)
                            GameListItems.Add(item);
                    });
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
                Dispatcher.Invoke(() => SetLoadingState(false));
            }
        }

        return;

        async Task<List<string>> BuildListOfAllFilesToLoad(SystemManager selectedManager, string startLetter2, string searchQuery2, CancellationToken token)
        {
            if (_isResortOperation)
            {
                await _allGamesLock.WaitAsync(token);
                try
                {
                    // If a search or filter is active, _currentSearchResults holds the full (unpaginated) list.
                    // Otherwise, the full list is in _allGamesForCurrentSystem.
                    var sourceList = !string.IsNullOrEmpty(_activeSearchQueryOrMode) || !string.IsNullOrEmpty(_currentFilter)
                        ? _currentSearchResults
                        : _allGamesForCurrentSystem;

                    if (sourceList != null)
                    {
                        DebugLogger.Log($"[BuildListOfAllFilesToLoad] Re-sorting existing list. Count: {sourceList.Count}");
                        return [..sourceList];
                    }
                }
                finally
                {
                    _allGamesLock.Release();
                }
            }

            var allFiles = new List<string>();

            switch (searchQuery2)
            {
                case "FAVORITES":
                    // Build favorites list on-demand
                    var favoriteGames = GetFavoriteGamesForSelectedSystem(_favoritesManager);

                    await _allGamesLock.WaitAsync(token);
                    try
                    {
                        _currentSearchResults = favoriteGames.ToList();
                        allFiles = _currentSearchResults;
                    }
                    finally
                    {
                        _allGamesLock.Release();
                    }

                    if (allFiles.Count == 0)
                    {
                        // await Dispatcher.BeginInvoke(static () => MessageBoxLibrary.NoFavoriteFoundMessageBox());
                        // ignore
                    }

                    break;

                case "RETRO_ACHIEVEMENTS":
                    // Ensure cache is populated
                    await EnsureAllGamesCachePopulatedAsync(selectedManager, token);

                    var systemId = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetSystemId(selectedManager.SystemName);
                    // Use the threshold from settings (e.g., 0.8)
                    var threshold = _settings.FuzzyMatchingThreshold;

                    await _allGamesLock.WaitAsync(token);
                    try
                    {
                        // 1. Handle null service or manager
                        if (_retroAchievementsService?.RaManager?.AllGames == null)
                        {
                            _currentSearchResults = [];
                            allFiles = [];
                            await Dispatcher.BeginInvoke(static () => MessageBoxLibrary.ErrorMessageBox());
                            break;
                        }

                        // Filter RA database by the current system ID to speed up matching
                        var raGamesForSystem = _retroAchievementsService.RaManager.AllGames
                            .Where(g => g.ConsoleId == systemId)
                            .ToList();

                        // 2. Handle case where system has no achievements in the database
                        // or no local games are found to match against
                        if (raGamesForSystem.Count == 0 || _allGamesForCurrentSystem == null || _allGamesForCurrentSystem.Count == 0)
                        {
                            _currentSearchResults = [];
                            allFiles = [];
                            // await Dispatcher.BeginInvoke(static () => MessageBoxLibrary.ErrorMessageBox());
                            break;
                        }

                        // Match filenames against RA Titles using Jaro-Winkler Fuzzy Matching
                        _currentSearchResults = _allGamesForCurrentSystem.Where(filePath =>
                        {
                            var fileName = Path.GetFileNameWithoutExtension(filePath).ToLowerInvariant();

                            return raGamesForSystem.Any(ra =>
                            {
                                var raTitle = ra.Title.ToLowerInvariant();

                                // 1. Fast Check: Direct containment (handles "Game Name (USA)" vs "Game Name")
                                if (fileName.Contains(raTitle) || raTitle.Contains(fileName))
                                    return true;

                                // 2. Fuzzy Check: Jaro-Winkler Similarity
                                var similarity = Services.FindAndLoadImages.FindCoverImage.CalculateJaroWinklerSimilarity(fileName, raTitle);
                                return similarity >= threshold;
                            });
                        }).ToList();

                        allFiles = _currentSearchResults;
                    }
                    catch (Exception ex)
                    {
                        allFiles = [];
                        DebugLogger.Log($"[BuildListOfAllFilesToLoad] Error matching RA games against local files: {ex}");
                        _ = _logErrors.LogErrorAsync(ex, $"[BuildListOfAllFilesToLoad] Error matching RA games against local files: {ex}");
                    }
                    finally
                    {
                        _allGamesLock.Release();
                    }

                    if (allFiles.Count == 0)
                    {
                        // ignore
                    }

                    break;

                case "RANDOM_SELECTION":
                    // Ensure cache is populated for random selection
                    await EnsureAllGamesCachePopulatedAsync(selectedManager, token);

                    await _allGamesLock.WaitAsync(token);
                    try
                    {
                        if (_allGamesForCurrentSystem.Count == 0)
                        {
                            allFiles = [];
                            // await Dispatcher.BeginInvoke(static () => MessageBoxLibrary.NoGameFoundInTheRandomSelectionMessageBox());
                            // ignore
                        }
                        else
                        {
                            // Randomly select one game
                            var random = new Random();
                            var randomIndex = random.Next(0, _allGamesForCurrentSystem.Count);
                            var selectedGame = _allGamesForCurrentSystem[randomIndex];
                            _currentSearchResults = [selectedGame];
                            allFiles = _currentSearchResults;
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
                    // use it directly. Otherwise, perform a full disk scan. The _selectedSystem field ensures the cache is for the *currently active* system.
                    var useCache = false;
                    await _allGamesLock.WaitAsync(token);
                    try
                    {
                        if (string.IsNullOrWhiteSpace(startLetter2) && string.IsNullOrWhiteSpace(searchQuery2) &&
                            _allGamesForCurrentSystem != null && _allGamesForCurrentSystem.Count != 0 &&
                            _selectedSystem == selectedManager.SystemName)
                        {
                            allFiles = new List<string>(_allGamesForCurrentSystem);
                            useCache = true;
                            DebugLogger.Log($"[BuildListOfAllFilesToLoad] Reusing cached list for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                        }
                    }
                    finally
                    {
                        _allGamesLock.Release();
                    }

                    if (!useCache)
                    {
                        // Disk scan happens here, completely outside the lock.
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

                        allFiles = uniqueFiles.Values.ToList();

                        // If this is an initial load (no filters), update the cache.
                        if (string.IsNullOrWhiteSpace(startLetter2) && string.IsNullOrWhiteSpace(searchQuery2))
                        {
                            await _allGamesLock.WaitAsync(token);
                            try
                            {
                                _allGamesForCurrentSystem = new List<string>(allFiles);
                                DebugLogger.Log($"[BuildListOfAllFilesToLoad] Populated cache for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                            }
                            finally
                            {
                                _allGamesLock.Release();
                            }
                        }
                    }

                    // ... filtering by startLetter ...
                    if (!string.IsNullOrWhiteSpace(startLetter2))
                    {
                        allFiles = await FilterFilesAsync(allFiles, startLetter2);
                        // After filtering by letter, store the results so they can be re-sorted.
                        await _allGamesLock.WaitAsync(token);
                        try
                        {
                            _currentSearchResults = new List<string>(allFiles);
                        }
                        finally
                        {
                            _allGamesLock.Release();
                        }
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

        async Task EnsureAllGamesCachePopulatedAsync(SystemManager selectedManager, CancellationToken token)
        {
            await _allGamesLock.WaitAsync(token);
            try
            {
                if (_allGamesForCurrentSystem?.Count > 0 && _selectedSystem == selectedManager.SystemName)
                {
                    DebugLogger.Log($"[EnsureAllGamesCachePopulatedAsync] Using cached list for '{selectedManager.SystemName}'. Count: {_allGamesForCurrentSystem.Count}");
                    return;
                }

                DebugLogger.Log($"[EnsureAllGamesCachePopulatedAsync] Populating from disk for '{selectedManager.SystemName}'.");
                var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var folder in selectedManager.SystemFolders)
                {
                    token.ThrowIfCancellationRequested();

                    var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                    if (string.IsNullOrEmpty(resolvedSystemFolderPath) ||
                        !Directory.Exists(resolvedSystemFolderPath) ||
                        selectedManager.FileFormatsToSearch == null) continue;

                    var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch, token);
                    foreach (var file in filesInFolder)
                    {
                        uniqueFiles.TryAdd(Path.GetFileName(file), file);
                    }
                }

                _allGamesForCurrentSystem = uniqueFiles.Values.ToList();
                _selectedSystem = selectedManager.SystemName;
                DebugLogger.Log($"[EnsureAllGamesCachePopulatedAsync] Populated {_allGamesForCurrentSystem.Count} games.");
            }
            finally
            {
                _allGamesLock.Release();
            }
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
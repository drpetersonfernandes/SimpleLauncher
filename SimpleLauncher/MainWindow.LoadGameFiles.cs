using System.IO;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.UIReset;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

public partial class MainWindow
{
    internal async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken cancellationToken = default)
    {
        UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("Loading") ?? "Loading...");

        // Note: Loading overlay should be shown by the caller before invoking this method
        // to ensure immediate UI feedback. This prevents the overlay from flickering or not showing.

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
            var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
            if (selectedManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedConfig is null.";
                _logErrors.LogAndForget(null, contextMessage);

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

            allFiles = _gameFilterService.SortByMameDescription(allFiles, ((IUiResetHost)this).MameSortOrder, _mameDataService.Lookup);
            cancellationToken.ThrowIfCancellationRequested();

            allFiles = await _gameFilterService.FilterByShowGamesSettingAsync(allFiles, selectedSystem, selectedManager);
            cancellationToken.ThrowIfCancellationRequested();

            allFiles = SetPaginationOfListOfFiles(allFiles);
            cancellationToken.ThrowIfCancellationRequested();

            if (_settings.ViewMode == "GridView")
            {
                var buttonBatch = new List<Button>(Math.Min(BatchSize, allFiles.Count));

                foreach (var filePath in allFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var gameButton = await _gameButtonFactory.CreateGameButtonAsync(filePath, selectedSystem, selectedManager);
                    buttonBatch.Add(gameButton);

                    if (buttonBatch.Count >= BatchSize)
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
                var itemBatch = new List<GameListViewItem>(Math.Min(BatchSize, allFiles.Count));

                foreach (var filePath in allFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var gameListViewItem = await _gameListFactory.CreateGameListViewItemAsync(filePath, selectedSystem, selectedManager);
                    itemBatch.Add(gameListViewItem);

                    if (itemBatch.Count >= BatchSize)
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
            // Also clear image sources to prevent memory leaks from BitmapImage references.
            GameFileGrid.Dispatcher.Invoke(() =>
            {
                ClearGameButtonImages(GameFileGrid);
                GameFileGrid.Children.Clear();
            });
            await Dispatcher.InvokeAsync(() => GameListItems.Clear());
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method LoadGameFilesAsync.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorMethodLoadGameFilesAsyncMessageBox();
        }
        finally
        {
            // Always decrement the loading counter when this operation completes.
            // The reference counting in SetLoadingState ensures the overlay stays visible
            // if there are other concurrent operations still running.
            Dispatcher.Invoke(() => SetLoadingState(false));
        }

        return;

        async Task<List<string>> BuildListOfAllFilesToLoad(SystemManager selectedManager, string startLetter2, string searchQuery2, CancellationToken token)
        {
            if (_isResortOperation)
            {
                var hasActiveFilter = !string.IsNullOrEmpty(((IUiResetHost)this).ActiveSearchQueryOrMode) ||
                                      !string.IsNullOrEmpty(((IUiResetHost)this).CurrentFilter);
                var (sourceList, _) = await _gameCacheService.GetResortSourceAsync(hasActiveFilter, token);

                if (sourceList.Count > 0)
                {
                    DebugLogger.Log($"[BuildListOfAllFilesToLoad] Re-sorting existing list. Count: {sourceList.Count}");
                    return sourceList;
                }
            }

            List<string> allFiles;

            switch (searchQuery2)
            {
                case "FAVORITES":
                    // Build favorites list on-demand
                    var favoriteGames = GetFavoriteGamesForSelectedSystem(_favoritesManager);
                    allFiles = favoriteGames.ToList();
                    await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                    break;

                case "RETRO_ACHIEVEMENTS":
                    // Ensure cache is populated
                    await _gameCacheService.PopulateFromDiskAsync(selectedManager, _getListOfFiles, token);

                    var systemId = Services.RetroAchievements.RetroAchievementsSystemMatcher.GetSystemId(selectedManager.SystemName);
                    var threshold = _settings.FuzzyMatchingThreshold;

                    try
                    {
                        // 1. Handle null service or manager
                        if (_retroAchievementsService?.RaManager?.AllGames == null)
                        {
                            allFiles = [];
                            await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                            await Dispatcher.BeginInvoke(static () => MessageBoxLibrary.ErrorMessageBox());
                            break;
                        }

                        // Filter RA database by the current system ID to speed up matching
                        var raGamesForSystem = _retroAchievementsService.RaManager.AllGames
                            .Where(g => g.ConsoleId == systemId)
                            .ToList();

                        var cachedGames = await _gameCacheService.GetAllGamesAsync(token);

                        // 2. Handle case where system has no achievements in the database
                        // or no local games are found to match against
                        if (raGamesForSystem.Count == 0 || cachedGames.Count == 0)
                        {
                            allFiles = [];
                            await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                            break;
                        }

                        // Match filenames against RA Titles using Jaro-Winkler Fuzzy Matching
                        allFiles = cachedGames.Where(filePath =>
                        {
                            var fileName = Path.GetFileNameWithoutExtension(filePath);

                            return raGamesForSystem.Any(ra =>
                            {
                                var raTitle = ra.Title;

                                // 1. Fast Check: Direct containment (handles "Game Name (USA)" vs "Game Name")
                                if (fileName.Contains(raTitle, StringComparison.OrdinalIgnoreCase) ||
                                    raTitle.Contains(fileName, StringComparison.OrdinalIgnoreCase))
                                    return true;

                                // 2. Fuzzy Check: Jaro-Winkler Similarity (handles case-insensitive comparison internally)
                                var similarity = _findCoverImage.CalculateJaroWinklerSimilarity(fileName, raTitle);
                                return similarity >= threshold;
                            });
                        }).ToList();

                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                    }
                    catch (Exception ex)
                    {
                        allFiles = [];
                        DebugLogger.Log($"[BuildListOfAllFilesToLoad] Error matching RA games against local files: {ex}");
                        _logErrors.LogAndForget(ex, $"[BuildListOfAllFilesToLoad] Error matching RA games against local files: {ex}");
                    }

                    break;

                case "RANDOM_SELECTION":
                    // Ensure cache is populated for random selection
                    await _gameCacheService.PopulateFromDiskAsync(selectedManager, _getListOfFiles, token);

                    var allGamesForRandom = await _gameCacheService.GetAllGamesAsync(token);
                    if (allGamesForRandom.Count == 0)
                    {
                        allFiles = [];
                    }
                    else
                    {
                        // Randomly select one game
                        var randomIndex = Random.Shared.Next(0, allGamesForRandom.Count);
                        var selectedGame = allGamesForRandom[randomIndex];
                        allFiles = [selectedGame];
                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                    }

                    break;

                default: // This branch handles initial load, letter filter, and text search
                {
                    // If no specific filter (letter or search query), and cache is already populated for this system,
                    // use it directly. Otherwise, perform a full disk scan.
                    if (string.IsNullOrWhiteSpace(startLetter2) && string.IsNullOrWhiteSpace(searchQuery2))
                    {
                        var isPopulated = await _gameCacheService.IsCachePopulatedForSystemAsync(selectedManager.SystemName, token);
                        if (isPopulated)
                        {
                            allFiles = await _gameCacheService.GetAllGamesAsync(token);
                            DebugLogger.Log($"[BuildListOfAllFilesToLoad] Reusing cached list for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                        }
                        else
                        {
                            // Disk scan
                            var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var folder in selectedManager.SystemFolders)
                            {
                                token.ThrowIfCancellationRequested();
                                var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                                if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath)) continue;

                                var filesInFolder = await _getListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch, selectedManager, token);
                                foreach (var file in filesInFolder)
                                {
                                    uniqueFiles.TryAdd(Path.GetFileName(file), file);
                                }
                            }

                            allFiles = uniqueFiles.Values.ToList();
                            await _gameCacheService.SetAllGamesAsync(allFiles, selectedManager.SystemName, token);
                            DebugLogger.Log($"[BuildListOfAllFilesToLoad] Populated cache for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                        }
                    }
                    else
                    {
                        // When filtering, ensure cache is populated first
                        var isPopulated = await _gameCacheService.IsCachePopulatedForSystemAsync(selectedManager.SystemName, token);
                        if (!isPopulated)
                        {
                            var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var folder in selectedManager.SystemFolders)
                            {
                                token.ThrowIfCancellationRequested();
                                var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                                if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath)) continue;

                                var filesInFolder = await _getListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch, selectedManager, token);
                                foreach (var file in filesInFolder)
                                {
                                    uniqueFiles.TryAdd(Path.GetFileName(file), file);
                                }
                            }

                            allFiles = uniqueFiles.Values.ToList();
                            await _gameCacheService.SetAllGamesAsync(allFiles, selectedManager.SystemName, token);
                        }
                        else
                        {
                            allFiles = await _gameCacheService.GetAllGamesAsync(token);
                        }
                    }

                    // ... filtering by startLetter ...
                    if (!string.IsNullOrWhiteSpace(startLetter2))
                    {
                        allFiles = await _gameFilterService.FilterByLetterAsync(allFiles, startLetter2);
                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                    }

                    // ... filtering by searchQuery ...
                    if (!string.IsNullOrWhiteSpace(searchQuery2) && searchQuery2 != "RANDOM_SELECTION" && searchQuery2 != "FAVORITES")
                    {
                        allFiles = await _gameFilterService.FilterBySearchQueryAsync(allFiles, searchQuery2, _mameDataService.Lookup);
                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                    }

                    break;
                }
            }

            return allFiles;
        }
    }
}

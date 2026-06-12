using System.Windows;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.FindCoverImage;
using SimpleLauncher.Services.RetroAchievements;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameFileLoadingOrchestrator;

/// <summary>
/// Orchestrates the game file loading pipeline: building file lists from disk or cache,
/// applying filters (letter, search, favorites, RetroAchievements), sorting, and rendering.
/// </summary>
public class GameFileLoadingOrchestrator : IGameFileLoadingOrchestrator
{
    private IGameFileLoadingHost _host;
    private readonly IGameCacheService _gameCacheService;
    private readonly IGameFilterService _gameFilterService;
    private readonly IMameDataService _mameDataService;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly FavoritesManager _favoritesManager;
    private readonly RetroAchievementsService _retroAchievementsService;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IGameItemRenderService _gameItemRenderService;
    private readonly SettingsManager.SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IUpdateStatusBar _updateStatusBarService;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IDebugLogger _debugLogger;
    private readonly IRetroAchievementsSystemMatcher _systemMatcher;

    /// <summary>
    /// Initializes a new instance of <see cref="GameFileLoadingOrchestrator"/> with all required dependencies.
    /// </summary>
    public GameFileLoadingOrchestrator(
        IGameCacheService gameCacheService,
        IGameFilterService gameFilterService,
        IMameDataService mameDataService,
        IGetListOfFilesService getListOfFiles,
        FavoritesManager favoritesManager,
        RetroAchievementsService retroAchievementsService,
        IFindCoverImageService findCoverImage,
        IGameItemRenderService gameItemRenderService,
        SettingsManager.SettingsManager settings,
        ILogErrors logErrors,
        IUpdateStatusBar updateStatusBarService,
        IMessageBoxLibraryService messageBox,
        IDebugLogger debugLogger,
        IRetroAchievementsSystemMatcher systemMatcher)
    {
        _gameCacheService = gameCacheService;
        _gameFilterService = gameFilterService;
        _mameDataService = mameDataService;
        _getListOfFiles = getListOfFiles;
        _favoritesManager = favoritesManager;
        _retroAchievementsService = retroAchievementsService;
        _findCoverImage = findCoverImage;
        _gameItemRenderService = gameItemRenderService;
        _settings = settings;
        _logErrors = logErrors;
        _updateStatusBarService = updateStatusBarService;
        _messageBox = messageBox;
        _debugLogger = debugLogger;
        _systemMatcher = systemMatcher;
    }

    /// <summary>
    /// Binds the orchestrator to the host that provides UI controls and system context.
    /// </summary>
    public void Initialize(IGameFileLoadingHost host)
    {
        _host = host;
    }

    /// <summary>
    /// Loads game files for the currently selected system, applying letter/search/favorites/RA filters,
    /// sorting, pagination, and rendering the results to the UI.
    /// </summary>
    public async Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken cancellationToken = default)
    {
        _updateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("Loading") ?? "Loading...");

        await _host.SetUiBeforeLoadGameFilesAsync();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_host.SystemComboBox.SelectedItem == null)
            {
                await _host.DisplaySystemSelectionScreenAsync(cancellationToken);
                return;
            }

            var selectedSystem = _host.SystemComboBox.SelectedItem.ToString();
            var systemManagers = _host.GetSystemManagers();
            var selectedManager = systemManagers.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
            if (selectedManager == null)
            {
                const string contextMessage = "selectedConfig is null.";
                _logErrors.LogAndForget(null, contextMessage);

                await _messageBox.InvalidSystemConfigMessageBox();

                await _host.DisplaySystemSelectionScreenAsync(cancellationToken);

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

                        if (rootFolders.Contains(normalizedFileDir))
                        {
                            return f;
                        }

                        return fileDir;
                    })
                    .Select(static g => g.Key)
                    .ToList();
                allFiles = groupedFiles;
            }

            allFiles = _gameFilterService.SortByMameDescription(allFiles, _host.GetMameSortOrder(), _mameDataService.Lookup);
            cancellationToken.ThrowIfCancellationRequested();

            allFiles = await _gameFilterService.FilterByShowGamesSettingAsync(allFiles, selectedSystem, selectedManager);
            cancellationToken.ThrowIfCancellationRequested();

            allFiles = _host.SetPaginationOfListOfFiles(allFiles);
            cancellationToken.ThrowIfCancellationRequested();

            await _gameItemRenderService.RenderGameItemsAsync(allFiles, selectedSystem, selectedManager, cancellationToken);

            switch (_settings.ViewMode)
            {
                case "GridView":
                    _host.Scroller.Focus();
                    break;
                case "ListView":
                    _host.GameDataGrid.Focus();
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            _debugLogger.Log("[LoadGameFilesAsync] Operation was canceled.");
            _gameItemRenderService.ClearRenderedItems();
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error in the method LoadGameFilesAsync.";
            _logErrors.LogAndForget(ex, contextMessage);

            await _messageBox.ErrorMethodLoadGameFilesAsyncMessageBox();
        }
        finally
        {
            _host.Dispatcher.Invoke(() => _host.SetLoadingState(false));
        }
    }

    /// <summary>
    /// Invalidates all game file caches, forcing a fresh disk scan on the next load.
    /// </summary>
    public Task InvalidateGameFileCachesAsync(CancellationToken cancellationToken = default)
    {
        return _gameCacheService.InvalidateAsync(cancellationToken);
    }

    /// <summary>
    /// Handles file system change notifications for a system by invalidating caches and reloading game files.
    /// </summary>
    public async void OnGameFilesChanged(string systemName)
    {
        try
        {
            var currentSystem = _host.SystemComboBox.SelectedItem?.ToString();
            if (!string.Equals(currentSystem, systemName, StringComparison.OrdinalIgnoreCase))
            {
                _debugLogger.Log($"[OnGameFilesChanged] Ignoring change for system '{systemName}' (current: '{currentSystem}').");
                return;
            }

            _debugLogger.Log($"[OnGameFilesChanged] File change detected for system '{systemName}'. Reloading game list.");

            await InvalidateGameFileCachesAsync();
            await LoadGameFilesAsync(cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[OnGameFilesChanged] Error reloading game list: {ex.Message}");
        }
    }

    /// <summary>
    /// build list of all files to load.
    /// </summary>
    /// <param name="selectedManager">The selected manager.</param>
    /// <param name="startLetter">The start letter.</param>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="token">The token.</param>
    private async Task<List<string>> BuildListOfAllFilesToLoad(SystemManager.SystemManager selectedManager, string startLetter, string searchQuery, CancellationToken token)
    {
        if (_host.IsResortOperation)
        {
            var hasActiveFilter = !string.IsNullOrEmpty(_host.GetActiveSearchQueryOrMode()) ||
                                  !string.IsNullOrEmpty(_host.GetCurrentFilter());
            var (sourceList, _) = await _gameCacheService.GetResortSourceAsync(hasActiveFilter, token);

            if (sourceList.Count > 0)
            {
                _debugLogger.Log($"[BuildListOfAllFilesToLoad] Re-sorting existing list. Count: {sourceList.Count}");
                return sourceList;
            }
        }

        List<string> allFiles;

        switch (searchQuery)
        {
            case "FAVORITES":
                var favoriteGames = GetFavoriteGamesForSelectedSystem(selectedManager);
                allFiles = favoriteGames.ToList();
                await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                break;

            case "RETRO_ACHIEVEMENTS":
                await _gameCacheService.PopulateFromDiskAsync(selectedManager, _getListOfFiles, token);

                var systemId = _systemMatcher.GetSystemId(selectedManager.SystemName);
                var threshold = _settings.FuzzyMatchingThreshold;

                try
                {
                    if (_retroAchievementsService?.RaManager?.AllGames == null)
                    {
                        allFiles = [];
                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                        await _host.Dispatcher.BeginInvoke(() => _messageBox.ErrorMessageBox());
                        break;
                    }

                    var raGamesForSystem = _retroAchievementsService.RaManager.AllGames
                        .Where(g => g.ConsoleId == systemId)
                        .ToList();

                    var cachedGames = await _gameCacheService.GetAllGamesAsync(token);

                    if (raGamesForSystem.Count == 0 || cachedGames.Count == 0)
                    {
                        allFiles = [];
                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                        break;
                    }

                    allFiles = cachedGames.Where(filePath =>
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        return raGamesForSystem.Any(ra =>
                        {
                            var raTitle = ra.Title;

                            if (fileName.Contains(raTitle, StringComparison.OrdinalIgnoreCase) ||
                                raTitle.Contains(fileName, StringComparison.OrdinalIgnoreCase))
                                return true;

                            var similarity = FindCoverImageService.CalculateJaroWinklerSimilarity(fileName, raTitle);
                            return similarity >= threshold;
                        });
                    }).ToList();

                    await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                }
                catch (Exception ex)
                {
                    allFiles = [];
                    _debugLogger.Log($"[BuildListOfAllFilesToLoad] Error matching RA games against local files: {ex}");
                    _logErrors.LogAndForget(ex, $"[BuildListOfAllFilesToLoad] Error matching RA games against local files: {ex}");
                }

                break;

            case "RANDOM_SELECTION":
                await _gameCacheService.PopulateFromDiskAsync(selectedManager, _getListOfFiles, token);

                var allGamesForRandom = await _gameCacheService.GetAllGamesAsync(token);
                if (allGamesForRandom.Count == 0)
                {
                    allFiles = [];
                }
                else
                {
                    var randomIndex = Random.Shared.Next(0, allGamesForRandom.Count);
                    var selectedGame = allGamesForRandom[randomIndex];
                    allFiles = [selectedGame];
                    await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                }

                break;

            default:
                {
                    if (string.IsNullOrWhiteSpace(startLetter) && string.IsNullOrWhiteSpace(searchQuery))
                    {
                        var isPopulated = await _gameCacheService.IsCachePopulatedForSystemAsync(selectedManager.SystemName, token);
                        if (isPopulated)
                        {
                            allFiles = await _gameCacheService.GetAllGamesAsync(token);
                            _debugLogger.Log($"[BuildListOfAllFilesToLoad] Reusing cached list for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                        }
                        else
                        {
                            var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var folder in selectedManager.SystemFolders)
                            {
                                token.ThrowIfCancellationRequested();
                                var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                                if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath)) continue;

                                var filesInFolder = await _getListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch, selectedManager.DisableRecursiveSearch, selectedManager.GroupByFolder, token);
                                foreach (var file in filesInFolder)
                                {
                                    uniqueFiles.TryAdd(Path.GetFileName(file), file);
                                }
                            }

                            allFiles = uniqueFiles.Values.ToList();
                            await _gameCacheService.SetAllGamesAsync(allFiles, selectedManager.SystemName, token);
                            _debugLogger.Log($"[BuildListOfAllFilesToLoad] Populated cache for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                        }
                    }
                    else
                    {
                        var isPopulated = await _gameCacheService.IsCachePopulatedForSystemAsync(selectedManager.SystemName, token);
                        if (!isPopulated)
                        {
                            var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            foreach (var folder in selectedManager.SystemFolders)
                            {
                                token.ThrowIfCancellationRequested();
                                var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                                if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath)) continue;

                                var filesInFolder = await _getListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch, selectedManager.DisableRecursiveSearch, selectedManager.GroupByFolder, token);
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

                    if (!string.IsNullOrWhiteSpace(startLetter))
                    {
                        allFiles = await _gameFilterService.FilterByLetterAsync(allFiles, startLetter);
                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                    }

                    if (!string.IsNullOrWhiteSpace(searchQuery) && searchQuery != "RANDOM_SELECTION" && searchQuery != "FAVORITES")
                    {
                        allFiles = await _gameFilterService.FilterBySearchQueryAsync(allFiles, searchQuery, _mameDataService.Lookup);
                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                    }

                    break;
                }
        }

        return allFiles;
    }

    /// <summary>
    /// get favorite games for selected system.
    /// </summary>
    /// <param name="selectedManager">The selected manager.</param>
    private List<string> GetFavoriteGamesForSelectedSystem(SystemManager.SystemManager selectedManager)
    {
        var favorites = _favoritesManager.FavoriteList;

        var selectedSystem = _host.SystemComboBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedSystem) || selectedManager == null)
        {
            return [];
        }

        var favoriteGamePaths = favorites
            .Where(fav => fav.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase))
            .Select(fav => PathHelper.FindFileInSystemFolders(selectedManager.SystemFolders, fav.FileName))
            .Where(static path => !string.IsNullOrEmpty(path))
            .ToList();

        return favoriteGamePaths;
    }
}
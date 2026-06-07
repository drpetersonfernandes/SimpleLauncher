using System.IO;
using System.Windows;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.FindCoverImage;
using SimpleLauncher.Services.GameCache;
using SimpleLauncher.Services.GameFilter;
using SimpleLauncher.Services.GameItemRender;
using SimpleLauncher.Services.GetListOfFiles;
using SimpleLauncher.Services.MameData;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Services.UpdateStatusBar;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameFileLoadingOrchestrator;

public class GameFileLoadingOrchestrator : IGameFileLoadingOrchestrator
{
    private IGameFileLoadingHost _host;
    private readonly IGameCacheService _gameCacheService;
    private readonly IGameFilterService _gameFilterService;
    private readonly IMameDataService _mameDataService;
    private readonly IGetListOfFiles _getListOfFiles;
    private readonly FavoritesManager _favoritesManager;
    private readonly RetroAchievementsService _retroAchievementsService;
    private readonly IFindCoverImage _findCoverImage;
    private readonly IGameItemRenderService _gameItemRenderService;
    private readonly SettingsManager.SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IUpdateStatusBar _updateStatusBarService;

    public GameFileLoadingOrchestrator(
        IGameCacheService gameCacheService,
        IGameFilterService gameFilterService,
        IMameDataService mameDataService,
        IGetListOfFiles getListOfFiles,
        FavoritesManager favoritesManager,
        RetroAchievementsService retroAchievementsService,
        IFindCoverImage findCoverImage,
        IGameItemRenderService gameItemRenderService,
        SettingsManager.SettingsManager settings,
        ILogErrors logErrors,
        IUpdateStatusBar updateStatusBarService)
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
    }

    public void Initialize(IGameFileLoadingHost host)
    {
        _host = host;
    }

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

                MessageBoxLibrary.InvalidSystemConfigMessageBox();

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
            DebugLogger.Log("[LoadGameFilesAsync] Operation was canceled.");
            _gameItemRenderService.ClearRenderedItems();
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error in the method LoadGameFilesAsync.";
            _logErrors.LogAndForget(ex, contextMessage);

            MessageBoxLibrary.ErrorMethodLoadGameFilesAsyncMessageBox();
        }
        finally
        {
            _host.Dispatcher.Invoke(() => _host.SetLoadingState(false));
        }
    }

    public Task InvalidateGameFileCachesAsync(CancellationToken cancellationToken = default)
    {
        return _gameCacheService.InvalidateAsync(cancellationToken);
    }

    public async void OnGameFilesChanged(string systemName)
    {
        try
        {
            var currentSystem = _host.SystemComboBox.SelectedItem?.ToString();
            if (!string.Equals(currentSystem, systemName, StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"[OnGameFilesChanged] Ignoring change for system '{systemName}' (current: '{currentSystem}').");
                return;
            }

            DebugLogger.Log($"[OnGameFilesChanged] File change detected for system '{systemName}'. Reloading game list.");

            await InvalidateGameFileCachesAsync();
            await LoadGameFilesAsync(cancellationToken: CancellationToken.None);
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[OnGameFilesChanged] Error reloading game list: {ex.Message}");
        }
    }

    private async Task<List<string>> BuildListOfAllFilesToLoad(SystemManager.SystemManager selectedManager, string startLetter, string searchQuery, CancellationToken token)
    {
        if (_host.IsResortOperation)
        {
            var hasActiveFilter = !string.IsNullOrEmpty(_host.GetActiveSearchQueryOrMode()) ||
                                  !string.IsNullOrEmpty(_host.GetCurrentFilter());
            var (sourceList, _) = await _gameCacheService.GetResortSourceAsync(hasActiveFilter, token);

            if (sourceList.Count > 0)
            {
                DebugLogger.Log($"[BuildListOfAllFilesToLoad] Re-sorting existing list. Count: {sourceList.Count}");
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

                var systemId = RetroAchievementsSystemMatcher.GetSystemId(selectedManager.SystemName);
                var threshold = _settings.FuzzyMatchingThreshold;

                try
                {
                    if (_retroAchievementsService?.RaManager?.AllGames == null)
                    {
                        allFiles = [];
                        await _gameCacheService.SetSearchResultsAsync(allFiles, token);
                        await _host.Dispatcher.BeginInvoke(static () => MessageBoxLibrary.ErrorMessageBox());
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
                        DebugLogger.Log($"[BuildListOfAllFilesToLoad] Reusing cached list for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
                    }
                    else
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
                        DebugLogger.Log($"[BuildListOfAllFilesToLoad] Populated cache for '{selectedManager.SystemName}'. Count: {allFiles.Count}");
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
            .Select(fav => PathHelper.FindFileInSystemFolders(selectedManager, fav.FileName))
            .Where(static path => !string.IsNullOrEmpty(path))
            .ToList();

        return favoriteGamePaths;
    }
}

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.UsageStats;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Avalonia.Services.Favorites;
using SimpleLauncher.Avalonia.Services.GameLauncher;
using SimpleLauncher.Avalonia.Services.PlayHistory;
using SimpleLauncher.Avalonia.Services.SystemManager;
using SimpleLauncher.Avalonia.Services;

namespace SimpleLauncher.Avalonia.ViewModels;

/// <summary>
/// Main ViewModel for the game browser.
/// Phase 6: Wired to real SystemManagerService, ILauncherService, and game scanning.
/// </summary>
public partial class MainViewModel : ObservableObject, ILoadingState
{
    private readonly FavoritesManager _favoritesManager;
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly SystemManagerService _systemManager;
    private readonly MinimalLauncherService _launcher;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly Stats _stats;
    private readonly SettingsManagerService _settings;
    private readonly IPaginationService _pagination;
    private readonly AvaloniaGameFileLoadingOrchestrator _loadingOrchestrator;

    private CancellationTokenSource? _searchCts;
    private HashSet<string> _favoritePaths;
    private List<SystemManagerConfig> _allSystems;

    /// <summary>
    /// The full (un-paginated) game list of the current view. Pagination slices this
    /// into <see cref="Games"/> when the total exceeds the configured page size.
    /// </summary>
    private List<GameCardViewModel> _currentAllGames = [];

    [ObservableProperty] private ObservableCollection<GameCardViewModel> _games = new();

    [ObservableProperty] private string _selectedSystem = "";

    [ObservableProperty] private bool _isGridView = true;

    [ObservableProperty] private bool _isMixedView = true;

    [ObservableProperty] private bool _isShowingFavorites;

    [ObservableProperty] private string _searchText = "";

    [ObservableProperty] private string _gameCountText = "0 games";

    [ObservableProperty] private string _statusText = "Ready";

    [ObservableProperty] private string _toolbarTitle = "SimpleLauncher";

    [ObservableProperty] private double _cardWidth = 168;

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = "Loading…";

    /// <summary>
    /// Font size for the game title caption on cards (from the Filename Font Size setting).
    /// </summary>
    [ObservableProperty] private double _captionFontSize = 13;

    /// <summary>
    /// ILoadingState implementation for the launcher: shows the overlay and updates
    /// the message ("Mounting CHD...", "Extracting...", ...) during long operations.
    /// </summary>
    public void SetLoadingState(bool isLoading, string? message = null)
    {
        IsLoading = isLoading;
        if (!string.IsNullOrEmpty(message))
        {
            LoadingMessage = message;
        }
    }

    public bool IsEmpty => Games.Count == 0;

    /// <summary>
    /// Sidebar state (system groups, icons, live counts). Populated by the window after load.
    /// </summary>
    public SidebarViewModel Sidebar { get; } = new();

    /// <summary>
    /// Gets the number of games per system name. Updated after each navigation/scan.
    /// </summary>
    public Dictionary<string, int> SystemGameCounts { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    public MainViewModel(
        FavoritesManager favoritesManager,
        PlayHistoryManager playHistoryManager,
        SystemManagerService systemManager,
        MinimalLauncherService launcher,
        IFindCoverImageService findCoverImage,
        Stats stats,
        SettingsManagerService settings,
        IPaginationService pagination,
        AvaloniaGameFileLoadingOrchestrator loadingOrchestrator)
    {
        _favoritesManager = favoritesManager;
        _playHistoryManager = playHistoryManager;
        _systemManager = systemManager;
        _launcher = launcher;
        _findCoverImage = findCoverImage;
        _stats = stats;
        _settings = settings;
        _pagination = pagination;
        _loadingOrchestrator = loadingOrchestrator;

        _favoritePaths = _favoritesManager.GetFavoritePaths();
        _allSystems = _systemManager.LoadSystems();

        // Apply the saved preferences (settings.xml): default view mode and card size
        IsGridView = !string.Equals(settings.ViewMode, "ListView", StringComparison.OrdinalIgnoreCase);
        if (settings.ThumbnailSize is >= 50 and <= 800)
        {
            CardWidth = settings.ThumbnailSize;
        }

        CaptionFontSize = settings.FilenameFontSize switch
        {
            "Small" => 11,
            "Big" => 16,
            _ => 13
        };

        // NOTE: game loading is deferred to InitializeAsync() (called after the window
        // loads) so this constructor never blocks the UI thread scanning large ROM collections.
    }

    /// <summary>
    /// Reloads the current game list, reapplying the Show Games filter, filename
    /// display mode, and card sizing (called after menu-driven setting changes).
    /// </summary>
    public void ReloadGames()
    {
        LoadAllGames();
    }

    /// <summary>
    /// Applies the saved Games Per Page setting to the pagination service (called once
    /// after the main window loads, and again whenever the setting changes via the menu).
    /// </summary>
    public void ConfigurePagination(int filesPerPage)
    {
        _pagination.FilesPerPage = filesPerPage;
        _pagination.PaginationThreshold = filesPerPage;
    }

    /// <summary>
    /// Navigates to the previous page of the current view (called by the status-bar button).
    /// </summary>
    public void GoToPreviousPage()
    {
        _pagination.GoToPreviousPage();
        ReapplyPagination();
    }

    /// <summary>
    /// Navigates to the next page of the current view (called by the status-bar button).
    /// </summary>
    public void GoToNextPage()
    {
        _pagination.GoToNextPage();
        ReapplyPagination();
    }

    /// <summary>
    /// Re-applies pagination to <see cref="_currentAllGames"/> and updates the displayed
    /// <see cref="Games"/> collection plus the game count label.
    /// </summary>
    private void ReapplyPagination()
    {
        var pageFiles = _pagination.ApplyPagination(_currentAllGames.Select(g => g.FilePath).ToList());
        var pageSet = pageFiles.ToHashSet(StringComparer.OrdinalIgnoreCase);

        Games = new ObservableCollection<GameCardViewModel>(
            _currentAllGames.Where(g => pageSet.Contains(g.FilePath)));
        UpdateGameCount();
    }

    /// <summary>
    /// Stores the full list for the current view and displays it through pagination
    /// (every navigation/search/refresh path routes through this method).
    /// </summary>
    private void ShowGames(List<GameCardViewModel> fullList)
    {
        _currentAllGames = fullList;
        ReapplyPagination();
    }

    /// <summary>
    /// Reloads the current view (search / favorites / selected system / all games)
    /// after the game file watcher detects changes on disk. Keeps the user where
    /// they were instead of resetting to the All Games view.
    /// </summary>
    public void RefreshCurrentView()
    {
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            ExecuteSearch(SearchText);
            return;
        }

        if (IsShowingFavorites)
        {
            NavigateToFavoritesCommand.Execute(null);
            return;
        }

        if (!string.IsNullOrEmpty(SelectedSystem))
        {
            NavigateToSystemCommand.Execute(SelectedSystem);
            return;
        }

        LoadAllGames();
    }

    /// <summary>
    /// Returns every game across all configured systems without applying any
    /// visibility filter (used by "Calculate Hashes For All Game Paths").
    /// </summary>
    public List<GameCardViewModel> GetAllGamesForHashing()
    {
        return ScanGames(_systemManager.LoadSystems());
    }

    /// <summary>
    /// Loads all games asynchronously. Called once after the main window loads.
    /// Heavy work (file enumeration + cover checks) runs on the thread pool;
    /// UI-bound properties are updated on the captured UI context.
    /// </summary>
    public async Task InitializeAsync()
    {
        IsLoading = true;
        try
        {
            // Reconcile favorites against the current system configuration (mirrors the WPF app):
            // entries whose system no longer exists (renamed without migration, or deleted)
            // are dropped so they never linger in the favorites filter.
            var validSystemNames = _allSystems.Select(static s => s.SystemName).ToList();
            var removedCount = await _favoritesManager.RemoveFavoritesForMissingSystemsAsync(validSystemNames);
            if (removedCount > 0)
            {
                _favoritePaths = _favoritesManager.GetFavoritePaths();
            }

            var (systems, counts, games) = await Task.Run(() =>
            {
                var loadedSystems = _systemManager.LoadSystems();
                var loadedCounts = ComputeSystemCounts(loadedSystems);
                var loadedGames = ScanGames(loadedSystems);
                ApplyFavoritesAndHistory(loadedGames);
                return (loadedSystems, loadedCounts, loadedGames);
            });

            _allSystems = systems;
            SystemGameCounts = counts;
            IsShowingFavorites = false;
            IsMixedView = true;
            SelectedSystem = "";
            ShowGames(games);
            StatusText = "All Games";
            ToolbarTitle = "SimpleLauncher";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize game library");
            StatusText = "Error loading games";
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = DebounceSearchAsync(value);
    }

    private async Task DebounceSearchAsync(string query)
    {
        try
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(180, token);
                if (token.IsCancellationRequested) return;

                if (string.IsNullOrWhiteSpace(query))
                {
                    LoadAllGames();
                    StatusText = "Ready";
                }
                else
                {
                    ExecuteSearch(query);
                }
            }
            catch (TaskCanceledException)
            {
                Log.Debug("Search debounce cancelled by newer input");
            }
        }
        catch (Exception ex)
        {
            // async Task: exceptions never escape to the process — they are
            // observed by the awaiting fire-and-forget discard
            Log.Error(ex, "Search debounce error");
        }
    }

    private void ExecuteSearch(string query)
    {
        var results = ScanGames(_allSystems)
            .Where(g => g.DisplayTitle.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToList();

        ApplyFavoritesAndHistory(results);
        ShowGames(results);
        StatusText = $"{results.Count} result{(results.Count == 1 ? "" : "s")} for \"{query}\"";
    }

    [RelayCommand]
    private void NavigateToSystem(string systemName)
    {
        try
        {
            SelectedSystem = systemName;
            IsMixedView = string.IsNullOrEmpty(systemName);

            var systems = string.IsNullOrEmpty(systemName)
                ? _allSystems
                : _allSystems.Where(s => string.Equals(s.SystemName, systemName, StringComparison.OrdinalIgnoreCase)).ToList();

            var games = ScanGames(systems);
            ApplyFavoritesAndHistory(games);
            ShowGames(games);
            var count = games.Count;
            StatusText = string.IsNullOrEmpty(systemName) ? "All Games" : systemName;
            ToolbarTitle = string.IsNullOrEmpty(systemName) ? "SimpleLauncher" : $"SimpleLauncher — {systemName} ({count} game{(count == 1 ? "" : "s")})";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to navigate to system {System}", systemName);
            StatusText = "Error loading games";
        }
    }

    [RelayCommand]
    private void NavigateToAllGames()
    {
        try
        {
            LoadAllGames();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to navigate to All Games");
            StatusText = "Error loading games";
        }
    }

    [RelayCommand]
    private void NavigateToFavorites()
    {
        try
        {
            IsShowingFavorites = true;
            _favoritePaths = _favoritesManager.GetFavoritePaths();

            var allGames = ScanGames(_allSystems);
            ApplyFavoritesAndHistory(allGames);
            var favorites = allGames.Where(g => g.IsFavorite).ToList();

            ShowGames(favorites);
            StatusText = "Favorites";
            ToolbarTitle = "SimpleLauncher — Favorites";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to navigate to Favorites");
            StatusText = "Error loading favorites";
        }
    }

    [RelayCommand]
    private void NavigateToRecentlyPlayed()
    {
        try
        {
            var historyLookup = _playHistoryManager.GetHistoryLookup();
            var allGames = ScanGames(_allSystems);
            ApplyFavoritesAndHistory(allGames);

            var recent = allGames
                .Where(g => historyLookup.ContainsKey(g.FilePath))
                .OrderByDescending(g => historyLookup[g.FilePath].LastPlayDate)
                .Take(20)
                .ToList();

            ShowGames(recent);
            StatusText = "Recently Played";
            ToolbarTitle = "SimpleLauncher — Recently Played";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to navigate to Recently Played");
            StatusText = "Error loading recently played";
        }
    }

    [RelayCommand]
    private void NavigateToRecentlyAdded()
    {
        try
        {
            var allGames = ScanGames(_allSystems);
            ApplyFavoritesAndHistory(allGames);

            // Sort by file creation/modification date (newest first)
            var recent = allGames
                .Where(g => File.Exists(g.FilePath))
                .OrderByDescending(g =>
                {
                    try
                    {
                        return new FileInfo(g.FilePath).LastWriteTime;
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, "Failed to read LastWriteTime for {Path}", g.FilePath);
                        return DateTime.MinValue;
                    }
                })
                .Take(50)
                .ToList();

            ShowGames(recent);
            StatusText = "Recently Added";
            ToolbarTitle = "SimpleLauncher — Recently Added";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to navigate to Recently Added");
            StatusText = "Error loading recently added";
        }
    }

    [RelayCommand]
    private void ToggleView()
    {
        IsGridView = !IsGridView;
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(GameCardViewModel? game)
    {
        if (game is null) return;

        var isNowFavorite = await _favoritesManager.ToggleAsync(game.FilePath, game.SystemName);
        game.IsFavorite = isNowFavorite;

        if (isNowFavorite)
            _favoritePaths.Add(game.FilePath);
        else
            _favoritePaths.Remove(game.FilePath);

        StatusText = isNowFavorite
            ? $"Added to favorites: {game.DisplayTitle}"
            : $"Removed from favorites: {game.DisplayTitle}";
    }

    [RelayCommand]
    private async Task PlayGameAsync(GameCardViewModel? game)
    {
        if (game is null) return;

        var playTime = await LaunchGameAtPathAsync(game.FilePath, game.SystemName);
        if (playTime.TotalSeconds >= 5)
        {
            game.PlayCount++;
            game.LastPlayed = DateTime.Now.ToString("d");
        }
    }

    /// <summary>
    /// Launches a game by file path and system name (shared by the game grid and the
    /// Favorites / Play History / Global Search sections). Records play history and
    /// usage stats; returns the measured play time so callers can update their rows.
    /// </summary>
    public async Task<TimeSpan> LaunchGameAtPathAsync(string filePath, string systemName)
    {
        var system = _systemManager.GetSystem(systemName);
        var emulator = system?.Emulators.FirstOrDefault();
        var windowContext = App.ServiceProvider.GetRequiredService<IWindowContext>();

        if (system is null || emulator is null)
        {
            StatusText = $"Cannot launch: no emulator configured for {systemName}";
            return TimeSpan.Zero;
        }

        IsLoading = true;
        StatusText = $"Launching: {Path.GetFileNameWithoutExtension(filePath)}...";

        try
        {
            await _launcher.HandleButtonClickAsync(
                filePath,
                emulator.EmulatorName,
                system.SystemName,
                system,
                emulator,
                emulator.EmulatorParameters,
                windowContext,
                this);

            // Real play-time tracking (from the original launcher): only sessions
            // longer than 5 seconds count toward play history.
            var playSeconds = (long)_launcher.LastPlayTime.TotalSeconds;
            if (playSeconds >= 5)
            {
                await _playHistoryManager.RecordPlayAsync(filePath, systemName, playSeconds);
            }

            // Fire-and-forget usage stats (emulator launch event)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _stats.CallApiAsync(emulator.EmulatorName);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Stats API call failed after launching {Game}", filePath);
                }
            });

            StatusText = $"Played: {Path.GetFileNameWithoutExtension(filePath)}";
            return _launcher.LastPlayTime;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to launch game {Game}", filePath);
            StatusText = $"Launch error: {ex.Message}";
            return TimeSpan.Zero;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Re-applies favorites and play-history state to the currently displayed games
    /// (called after the Favorites / Play History sections mutate their data).
    /// </summary>
    public void RefreshFavoritesAndHistory()
    {
        ApplyFavoritesAndHistory(_currentAllGames);
        ReapplyPagination();
    }

    /// <summary>
    /// Removes a game from the currently displayed list without touching the file on
    /// disk (the library is filesystem-derived, so the game reappears on the next
    /// scan/navigation). Used by the Game Detail window's Remove button.
    /// </summary>
    public void RemoveGameFromCurrentList(GameCardViewModel game)
    {
        if (game is null) return;

        _currentAllGames.Remove(game);
        Games.Remove(game);
        StatusText = $"Removed from view: {game.DisplayTitle}";
        UpdateGameCount();
    }

    /// <summary>
    /// Scans ROM folders for games and returns card ViewModels with cover art resolved
    /// from each system's image folder. Folder paths are resolved (%BASEFOLDER% / relative) first.
    /// File enumeration goes through the loading orchestrator, which caches the file
    /// list per system so repeat navigation does not re-enumerate the disk.
    /// </summary>
    private List<GameCardViewModel> ScanGames(List<SystemManagerConfig> systems)
    {
        var games = new List<GameCardViewModel>();

        foreach (var system in systems)
        {
            foreach (var file in _loadingOrchestrator.GetGameFiles(system))
            {
                var coverPath = _findCoverImage.FindCoverImagePath(
                    Path.GetFileNameWithoutExtension(file),
                    system.SystemName,
                    system.SystemImageFolder);

                games.Add(new GameCardViewModel
                {
                    DisplayTitle = GetDisplayTitle(file),
                    FilePath = file,
                    SystemName = system.SystemName,
                    CoverPath = coverPath,
                    // Show art only when the file actually exists (the service falls back
                    // to default.png, which may itself be missing → placeholder instead)
                    HasCover = File.Exists(coverPath),
                    IsRaSupported = GameCardViewModel.IsSystemRaSupported(system.SystemName)
                });
            }
        }

        return games;
    }

    /// <summary>
    /// Applies the Filename Preferences setting (Original / CleanUp / NoFilename)
    /// to a game file path, mirroring the WPF game button titles.
    /// </summary>
    private string GetDisplayTitle(string filePath)
    {
        var name = Path.GetFileNameWithoutExtension(filePath) ?? filePath;

        return _settings.FilenameDisplayMode switch
        {
            "CleanUp" => CleanUpTitle(name),
            "NoFilename" => "",
            _ => name
        };
    }

    /// <summary>
    /// Cleans a filename for display: strips bracketed/parenthesized annotations
    /// (e.g. "[USA]", "(Rev 1)"), replaces separators with spaces, collapses whitespace.
    /// </summary>
    private static string CleanUpTitle(string name)
    {
        var cleaned = System.Text.RegularExpressions.Regex.Replace(name, @"\s*[\[\(][^\]\)]*[\]\)]", "");
        cleaned = cleaned.Replace('_', ' ').Replace('.', ' ');
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim();
        return cleaned;
    }

    /// <summary>
    /// Invalidates the cached file list for one system (called when the game file
    /// watcher detects changes on disk, so the next scan picks them up).
    /// </summary>
    /// <param name="systemName">The affected system name.</param>
    public void InvalidateGameFileCacheForSystem(string systemName)
    {
        _loadingOrchestrator.InvalidateSystem(systemName);
    }

    /// <summary>
    /// Invalidates all cached file lists (called when the system configuration
    /// changes, e.g. after adding a system in Easy Mode).
    /// </summary>
    public void InvalidateAllGameFileCaches()
    {
        _loadingOrchestrator.InvalidateAll();
    }

    private void LoadAllGames()
    {
        try
        {
            IsShowingFavorites = false;
            IsMixedView = true;
            SelectedSystem = "";
            _allSystems = _systemManager.LoadSystems();

            RefreshSystemCounts();

            var games = ScanGames(_allSystems);
            ApplyFavoritesAndHistory(games);
            ShowGames(games);
            StatusText = "All Games";
            ToolbarTitle = "SimpleLauncher";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load all games");
            StatusText = "Error loading games";
        }
    }

    private void ApplyFavoritesAndHistory(List<GameCardViewModel> games)
    {
        _favoritePaths = _favoritesManager.GetFavoritePaths();
        var historyLookup = _playHistoryManager.GetHistoryLookup();

        foreach (var game in games)
        {
            game.IsFavorite = _favoritePaths.Contains(game.FilePath);

            if (historyLookup.TryGetValue(game.FilePath, out var history))
            {
                game.PlayCount = history.TimesPlayed;
                game.LastPlayed = history.LastPlayDate;
            }
        }

        // Show Games filter (settings.xml): all / with cover / without cover
        var showGamesMode = _settings.ShowGames;
        if (showGamesMode is not "ShowAll")
        {
            games.RemoveAll(g => showGamesMode == "ShowWithCover" ? !g.HasCover : g.HasCover);
        }
    }

    partial void OnGamesChanged(ObservableCollection<GameCardViewModel> value)
    {
        UpdateGameCount(value.Count);
        OnPropertyChanged(nameof(IsEmpty));
    }

    /// <summary>
    /// Recomputes per-system game counts from a full scan of all configured system folders
    /// (resolving %BASEFOLDER% / relative paths), independent of the current view.
    /// </summary>
    private void RefreshSystemCounts()
    {
        try
        {
            SystemGameCounts = ComputeSystemCounts(_allSystems);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to refresh system game counts");
        }
    }

    /// <summary>
    /// Computes per-system game counts via the loading orchestrator (cached file lists).
    /// Pure computation — safe on any thread.
    /// </summary>
    private Dictionary<string, int> ComputeSystemCounts(List<SystemManagerConfig> systems)
    {
        return _loadingOrchestrator.ComputeSystemCounts(systems);
    }

    private void UpdateGameCount(int? count = null)
    {
        var c = count ?? Games.Count;
        GameCountText = $"{c} game{(c == 1 ? "" : "s")}";
    }
}

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class MainWindowViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly IMessageDialogService _messageDialog;
    private readonly IResourceProvider _resources;
    private readonly IConfiguration _configuration;
    private readonly ICoreSystemConfigurationService _systemConfigService;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IImageLoader _imageLoader;
    private readonly IDispatcherService _dispatcher;
    private readonly GameLauncherService _gameLauncher;
    private readonly ILogErrors _logErrors;
    private readonly IThemeService _themeService;
    private readonly IApplicationLifetime _appLifetime;

    private List<Core.Interfaces.ISystemManager> _systemManagers = [];
    private List<string> _allGameFiles = [];
    private CancellationTokenSource? _loadCts;

    public MainWindowViewModel(
        SettingsManager settings,
        IMessageDialogService messageDialog,
        IResourceProvider resources,
        IConfiguration configuration,
        ICoreSystemConfigurationService systemConfigService,
        IGetListOfFilesService getListOfFiles,
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        IDispatcherService dispatcher,
        GameLauncherService gameLauncher,
        ILogErrors logErrors,
        IThemeService themeService,
        IApplicationLifetime appLifetime,
        FavoritesViewModel favoritesViewModel,
        GlobalSearchViewModel globalSearchViewModel,
        PlayHistoryViewModel playHistoryViewModel)
    {
        _settings = settings;
        _messageDialog = messageDialog;
        _resources = resources;
        _configuration = configuration;
        _systemConfigService = systemConfigService;
        _getListOfFiles = getListOfFiles;
        _findCoverImage = findCoverImage;
        _imageLoader = imageLoader;
        _dispatcher = dispatcher;
        _gameLauncher = gameLauncher;
        _logErrors = logErrors;
        _themeService = themeService;
        _appLifetime = appLifetime;
        FavoritesVm = favoritesViewModel;
        GlobalSearchVm = globalSearchViewModel;
        PlayHistoryVm = playHistoryViewModel;

        // Load initial state from settings
        ViewMode = _settings.ViewMode ?? "GridView";
        ThumbnailSize = _settings.ThumbnailSize;

        // Initialize letter filters
        InitializeLetterFilters();

        // Load systems on creation
        _ = LoadSystemsAsync();
    }

    // ── Title / Status ──────────────────────────────────────────

    [ObservableProperty] private string _title = "Simple Launcher";

    [ObservableProperty] private string _statusText = "Ready";

    [ObservableProperty] private string _playTimeText = string.Empty;

    [ObservableProperty] private bool _isPlayTimeVisible;

    // ── System / Emulator Selection ─────────────────────────────

    [ObservableProperty] private ObservableCollection<string> _systems = [];

    [ObservableProperty] private string? _selectedSystem;

    [ObservableProperty] private ObservableCollection<string> _emulators = [];

    [ObservableProperty] private string? _selectedEmulator;

    // ── Search ──────────────────────────────────────────────────

    [ObservableProperty] private string _searchText = string.Empty;

    // ── View Mode ───────────────────────────────────────────────

    [ObservableProperty] private string _viewMode = "GridView";

    [ObservableProperty] private bool _isGridView = true;

    [ObservableProperty] private bool _isListView;

    // ── Thumbnail Size ──────────────────────────────────────────

    [ObservableProperty] private int _thumbnailSize = 250;

    // ── Games Collection ────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<GameItemViewModel> _games = [];

    [ObservableProperty] private int _totalGames;

    [ObservableProperty] private int _currentPage = 1;

    [ObservableProperty] private int _totalPages = 1;

    [ObservableProperty] private bool _hasPreviousPage;

    [ObservableProperty] private bool _hasNextPage;

    // ── Loading State ───────────────────────────────────────────

    [ObservableProperty] private bool _isLoading;

    // ── Sub-page Navigation ─────────────────────────────────────

    [ObservableProperty] private bool _isSubPageActive;

    [ObservableProperty] private object? _currentSubPage;

    // ── Letter Filter ───────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<string> _letterFilters = [];

    [ObservableProperty] private string? _selectedLetterFilter;

    // ── Sub-page ViewModels (for DataContext binding) ────────────

    public FavoritesViewModel FavoritesVm { get; }

    public GlobalSearchViewModel GlobalSearchVm { get; }

    public PlayHistoryViewModel PlayHistoryVm { get; }

    // ── Pagination Constants ────────────────────────────────────

    private const int GamesPerPage = 50;

    // ── System Loading ──────────────────────────────────────────

    private async Task LoadSystemsAsync()
    {
        try
        {
            StatusText = "Loading systems...";

            _systemManagers = _systemConfigService.LoadSystemManagers();

            var systemNames = _systemManagers
                .Select(s => s.SystemName)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            await _dispatcher.InvokeAsync(() =>
            {
                Systems = new ObservableCollection<string>(systemNames);
            });

            StatusText = _systemManagers.Count > 0
                ? $"{_systemManagers.Count} systems loaded. Select a system to begin."
                : "No systems found. Use Easy Mode to add systems.";

            // Auto-select first system if available
            if (Systems.Count > 0)
            {
                SelectedSystem = Systems[0];
            }
        }
        catch (Exception ex)
        {
            StatusText = "Error loading systems.";
            await _messageDialog.ShowErrorAsync($"Failed to load systems: {ex.Message}", "Error");
        }
    }

    // ── System Selection Changed ────────────────────────────────

    partial void OnSelectedSystemChanged(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;

        _ = OnSystemSelectionChangedAsync(value);
    }

    private async Task OnSystemSelectionChangedAsync(string systemName)
    {
        try
        {
            // Cancel any previous load operation
            _loadCts?.Cancel();
            _loadCts = new CancellationTokenSource();
            var ct = _loadCts.Token;

            IsLoading = true;
            StatusText = $"Loading {systemName}...";

            // Find the selected system manager
            var selectedManager = _systemManagers.FirstOrDefault(s =>
                s.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (selectedManager == null)
            {
                StatusText = $"System '{systemName}' not found.";
                IsLoading = false;
                return;
            }

            // Populate emulators
            var emulatorNames = selectedManager.Emulators
                .Select(e => e.EmulatorName)
                .ToList();

            await _dispatcher.InvokeAsync(() =>
            {
                Emulators = new ObservableCollection<string>(emulatorNames);
            });

            if (Emulators.Count > 0)
            {
                SelectedEmulator = Emulators[0];
            }

            // Scan game files
            await LoadGamesAsync(selectedManager, ct);

            ct.ThrowIfCancellationRequested();

            StatusText = $"{TotalGames} games found in {systemName}";
        }
        catch (OperationCanceledException)
        {
            // Expected when switching systems
        }
        catch (Exception ex)
        {
            StatusText = $"Error loading system: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Game File Loading ───────────────────────────────────────

    private async Task LoadGamesAsync(Core.Interfaces.ISystemManager systemManager, CancellationToken ct)
    {
        var allFiles = new List<string>();

        // Scan all system folders
        foreach (var folder in systemManager.SystemFolders)
        {
            ct.ThrowIfCancellationRequested();

            var resolvedFolder = PathHelper.ResolveRelativeToAppDirectory(folder);
            if (string.IsNullOrEmpty(resolvedFolder) || !Directory.Exists(resolvedFolder))
                continue;

            var files = await _getListOfFiles.GetFilesAsync(
                resolvedFolder,
                systemManager.FileFormatsToSearch,
                systemManager.DisableRecursiveSearch,
                systemManager.GroupByFolder,
                ct);

            allFiles.AddRange(files);
        }

        // Deduplicate by filename
        var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in allFiles)
        {
            var fileName = Path.GetFileName(file);
            if (!uniqueFiles.ContainsKey(fileName))
            {
                uniqueFiles[fileName] = file;
            }
        }

        _allGameFiles = uniqueFiles.Values
            .OrderBy(f => Path.GetFileNameWithoutExtension(f), StringComparer.OrdinalIgnoreCase)
            .ToList();

        TotalGames = _allGameFiles.Count;
        CurrentPage = 1;

        // Apply letter filter and pagination
        await ApplyFilterAndPaginationAsync(systemManager, ct);
    }

    // ── Filter and Pagination ───────────────────────────────────

    private async Task ApplyFilterAndPaginationAsync(Core.Interfaces.ISystemManager systemManager, CancellationToken ct)
    {
        var filteredFiles = _allGameFiles;

        // Apply letter filter
        if (!string.IsNullOrEmpty(SelectedLetterFilter) && SelectedLetterFilter != "ALL")
        {
            filteredFiles = _allGameFiles
                .Where(f =>
                {
                    var name = Path.GetFileNameWithoutExtension(f);
                    if (string.IsNullOrEmpty(name)) return false;

                    if (SelectedLetterFilter == "#")
                        return char.IsDigit(name[0]);

                    return name.StartsWith(SelectedLetterFilter, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
        }

        // Calculate pagination
        TotalGames = filteredFiles.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)filteredFiles.Count / GamesPerPage));

        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        HasPreviousPage = CurrentPage > 1;
        HasNextPage = CurrentPage < TotalPages;

        // Get paginated subset
        var skip = (CurrentPage - 1) * GamesPerPage;
        var pageFiles = filteredFiles.Skip(skip).Take(GamesPerPage).ToList();

        // Create game items
        var favoritePaths = await LoadFavoritePathsAsync();
        var gameItems = new List<GameItemViewModel>();
        foreach (var filePath in pageFiles)
        {
            ct.ThrowIfCancellationRequested();

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var coverImagePath = _findCoverImage.FindCoverImagePath(
                fileName,
                systemManager.SystemName,
                systemManager.SystemImageFolder);

            gameItems.Add(new GameItemViewModel
            {
                FileName = Path.GetFileName(filePath),
                MachineName = fileName,
                FolderPath = Path.GetDirectoryName(filePath) ?? string.Empty,
                CoverImagePath = coverImagePath,
                FilePath = filePath,
                IsFavorite = favoritePaths.Contains(filePath, StringComparer.OrdinalIgnoreCase)
            });
        }

        await _dispatcher.InvokeAsync(() =>
        {
            Games = new ObservableCollection<GameItemViewModel>(gameItems);
        });
    }

    // ── Letter Filter ───────────────────────────────────────────

    private void InitializeLetterFilters()
    {
        var filters = new List<string> { "ALL" };
        for (var c = 'A'; c <= 'Z'; c++)
        {
            filters.Add(c.ToString());
        }
        filters.Add("#");

        LetterFilters = new ObservableCollection<string>(filters);
        SelectedLetterFilter = "ALL";
    }

    partial void OnSelectedLetterFilterChanged(string? value)
    {
        if (_systemManagers.Count == 0) return;

        var selectedManager = _systemManagers.FirstOrDefault(s =>
            s.SystemName.Equals(SelectedSystem, StringComparison.OrdinalIgnoreCase));

        if (selectedManager == null) return;

        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        CurrentPage = 1;
        _ = ApplyFilterAndPaginationAsync(selectedManager, _loadCts.Token);
    }

    // ── Commands ────────────────────────────────────────────────

    [RelayCommand]
    private void ToggleViewMode()
    {
        if (ViewMode == "GridView")
        {
            ViewMode = "ListView";
            IsGridView = false;
            IsListView = true;
        }
        else
        {
            ViewMode = "GridView";
            IsGridView = true;
            IsListView = false;
        }

        _settings.ViewMode = ViewMode;
    }

    [RelayCommand]
    private void ZoomIn()
    {
        if (ThumbnailSize < 800)
        {
            ThumbnailSize += 50;
            _settings.ThumbnailSize = ThumbnailSize;
        }
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (ThumbnailSize > 50)
        {
            ThumbnailSize -= 50;
            _settings.ThumbnailSize = ThumbnailSize;
        }
    }

    [RelayCommand]
    private void NavigateHome()
    {
        IsSubPageActive = false;
        CurrentSubPage = null;
        StatusText = "Ready";
    }

    [RelayCommand]
    private Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return Task.CompletedTask;

        // Navigate to global search with the search text
        GlobalSearchVm.SearchText = SearchText;
        GoToGlobalSearch();
        return GlobalSearchVm.SearchCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private Task PreviousPageAsync()
    {
        if (CurrentPage <= 1) return Task.CompletedTask;

        CurrentPage--;

        var selectedManager = _systemManagers.FirstOrDefault(s =>
            s.SystemName.Equals(SelectedSystem, StringComparison.OrdinalIgnoreCase));

        if (selectedManager == null) return Task.CompletedTask;

        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        return ApplyFilterAndPaginationAsync(selectedManager, _loadCts.Token);
    }

    [RelayCommand]
    private Task NextPageAsync()
    {
        if (CurrentPage >= TotalPages) return Task.CompletedTask;

        CurrentPage++;

        var selectedManager = _systemManagers.FirstOrDefault(s =>
            s.SystemName.Equals(SelectedSystem, StringComparison.OrdinalIgnoreCase));

        if (selectedManager == null) return Task.CompletedTask;

        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();
        return ApplyFilterAndPaginationAsync(selectedManager, _loadCts.Token);
    }

    [RelayCommand]
    private Task GoToFavorites()
    {
        IsSubPageActive = true;
        CurrentSubPage = FavoritesVm;
        StatusText = "Favorites";
        return FavoritesVm.LoadFavoritesAsync();
    }

    [RelayCommand]
    private void GoToGlobalSearch()
    {
        IsSubPageActive = true;
        CurrentSubPage = GlobalSearchVm;
        StatusText = "Global Search";
    }

    [RelayCommand]
    private Task GoToPlayHistory()
    {
        IsSubPageActive = true;
        CurrentSubPage = PlayHistoryVm;
        StatusText = "Play History";
        return PlayHistoryVm.LoadHistoryAsync();
    }

    [RelayCommand]
    private Task FeelingLuckyAsync()
    {
        if (Games.Count == 0) return Task.CompletedTask;

        var random = new Random();
        var index = random.Next(Games.Count);
        var game = Games[index];
        return _messageDialog.ShowInfoAsync($"Feeling lucky! Selected: {game.MachineName}", "Feeling Lucky");
    }

    [RelayCommand]
    private Task SelectLetterFilterAsync(string? letter)
    {
        SelectedLetterFilter = letter;
        return Task.CompletedTask;
    }

    // ── Menu Commands ──────────────────────────────────────────

    [RelayCommand]
    private void SetTheme(string? theme)
    {
        if (string.IsNullOrEmpty(theme)) return;
        _settings.BaseTheme = theme;
        _themeService.ApplyTheme(theme, _settings.AccentColor);
    }

    [RelayCommand]
    private void SetAccentColor(string? color)
    {
        if (string.IsNullOrEmpty(color)) return;
        _settings.AccentColor = color;
        _themeService.ApplyTheme(_settings.BaseTheme, color);
    }

    [RelayCommand]
    private void ExitApplication()
    {
        _appLifetime.Shutdown();
    }

    [RelayCommand]
    private Task OpenSettingsAsync()
    {
        var settingsWindow = new Views.SettingsWindow
        {
            DataContext = App.ServiceProvider.GetRequiredService<SettingsViewModel>()
        };
        settingsWindow.Show();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenAboutAsync()
    {
        var aboutWindow = new Views.AboutWindow();
        aboutWindow.Show();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenEasyModeAsync()
    {
        var easyModeWindow = new Views.EasyModeWindow
        {
            DataContext = App.ServiceProvider.GetRequiredService<EasyModeViewModel>()
        };
        easyModeWindow.Show();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenEditSystemAsync()
    {
        var editSystemWindow = new Views.EditSystemWindow
        {
            DataContext = App.ServiceProvider.GetRequiredService<EditSystemViewModel>()
        };
        editSystemWindow.Show();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task OpenGameFolderAsync(GameItemViewModel? game)
    {
        if (game == null) return;

        var folderPath = game.FolderPath;
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            await _messageDialog.ShowErrorAsync("Game folder not found.", "Error");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            await _messageDialog.ShowErrorAsync($"Failed to open folder: {ex.Message}", "Error");
        }
    }

    [RelayCommand]
    private async Task LaunchGameAsync(GameItemViewModel? game)
    {
        if (game == null) return;

        var selectedManager = _systemManagers.FirstOrDefault(s =>
            s.SystemName.Equals(SelectedSystem, StringComparison.OrdinalIgnoreCase));

        if (selectedManager == null)
        {
            await _messageDialog.ShowErrorAsync("No system selected.", "Launch Error");
            return;
        }

        if (string.IsNullOrEmpty(SelectedEmulator))
        {
            await _messageDialog.ShowErrorAsync("No emulator selected.", "Launch Error");
            return;
        }

        StatusText = $"Launching {game.MachineName}...";

        var playTime = await _gameLauncher.LaunchGameAsync(
            game.FilePath,
            SelectedEmulator,
            selectedManager,
            _settings);

        if (playTime.TotalSeconds > 5)
        {
            // Record play history
            await RecordPlayHistoryAsync(game.FilePath, SelectedSystem!, playTime);
            StatusText = $"Played {game.MachineName} for {playTime:mm\\:ss}";
        }
        else
        {
            StatusText = "Ready";
        }
    }

    [RelayCommand]
    private async Task AddFavoriteAsync(GameItemViewModel? game)
    {
        if (game == null || string.IsNullOrEmpty(SelectedSystem)) return;

        try
        {
            var favoritesPath = GetFavoritesPath();
            var favorites = new List<SimpleLauncher.Core.Models.Favorite>();

            if (File.Exists(favoritesPath))
            {
                var bytes = await File.ReadAllBytesAsync(favoritesPath);
                favorites = MessagePack.MessagePackSerializer.Deserialize<List<SimpleLauncher.Core.Models.Favorite>>(bytes);
            }

            // Check if already a favorite
            if (favorites.Any(f => f.FileName.Equals(game.FilePath, StringComparison.OrdinalIgnoreCase)))
            {
                StatusText = $"'{game.MachineName}' is already in favorites.";
                return;
            }

            favorites.Add(new SimpleLauncher.Core.Models.Favorite
            {
                FileName = game.FilePath,
                SystemName = SelectedSystem
            });

            var directory = Path.GetDirectoryName(favoritesPath);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var newBytes = MessagePack.MessagePackSerializer.Serialize(favorites);
            await File.WriteAllBytesAsync(favoritesPath, newBytes);

            game.IsFavorite = true;
            StatusText = $"Added '{game.MachineName}' to favorites.";
        }
        catch (Exception ex)
        {
            StatusText = $"Error adding to favorites: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RemoveFavoriteAsync(GameItemViewModel? game)
    {
        if (game == null) return;

        try
        {
            var favoritesPath = GetFavoritesPath();
            if (!File.Exists(favoritesPath)) return;

            var bytes = await File.ReadAllBytesAsync(favoritesPath);
            var favorites = MessagePack.MessagePackSerializer.Deserialize<List<SimpleLauncher.Core.Models.Favorite>>(bytes);

            var removed = favorites.RemoveAll(f => f.FileName.Equals(game.FilePath, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
            {
                var newBytes = MessagePack.MessagePackSerializer.Serialize(favorites);
                await File.WriteAllBytesAsync(favoritesPath, newBytes);

                game.IsFavorite = false;
                StatusText = $"Removed '{game.MachineName}' from favorites.";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error removing from favorites: {ex.Message}";
        }
    }

    private async Task RecordPlayHistoryAsync(string filePath, string systemName, TimeSpan playTime)
    {
        try
        {
            var historyPath = GetPlayHistoryPath();
            var history = new List<SimpleLauncher.Core.Models.PlayHistoryItem>();

            if (File.Exists(historyPath))
            {
                var bytes = await File.ReadAllBytesAsync(historyPath);
                history = MessagePack.MessagePackSerializer.Deserialize<List<SimpleLauncher.Core.Models.PlayHistoryItem>>(bytes);
            }

            var existing = history.FirstOrDefault(h =>
                h.FileName.Equals(filePath, StringComparison.OrdinalIgnoreCase) &&
                h.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.TotalPlayTime += (long)playTime.TotalSeconds;
                existing.TimesPlayed++;
                existing.LastPlayDate = DateTime.Now.ToString("yyyy-MM-dd");
                existing.LastPlayTime = DateTime.Now.ToString("HH:mm:ss");
            }
            else
            {
                history.Add(new SimpleLauncher.Core.Models.PlayHistoryItem
                {
                    FileName = filePath,
                    SystemName = systemName,
                    TotalPlayTime = (long)playTime.TotalSeconds,
                    TimesPlayed = 1,
                    LastPlayDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    LastPlayTime = DateTime.Now.ToString("HH:mm:ss")
                });
            }

            var directory = Path.GetDirectoryName(historyPath);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var newBytes = MessagePack.MessagePackSerializer.Serialize(history);
            await File.WriteAllBytesAsync(historyPath, newBytes);
        }
        catch (Exception ex)
        {
            _logErrors?.LogAndForget(ex, "Error recording play history.");
        }
    }

    private string GetFavoritesPath()
    {
        var basePath = _configuration.GetValue<string>("FavoritesPath") ?? "favorites";
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath, "favorites.bin");
    }

    private string GetPlayHistoryPath()
    {
        var basePath = _configuration.GetValue<string>("PlayHistoryPath") ?? "history";
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath, "playhistory.bin");
    }

    private async Task<HashSet<string>> LoadFavoritePathsAsync()
    {
        try
        {
            var favoritesPath = GetFavoritesPath();
            if (!File.Exists(favoritesPath))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var bytes = await File.ReadAllBytesAsync(favoritesPath);
            var favorites = MessagePack.MessagePackSerializer.Deserialize<List<SimpleLauncher.Core.Models.Favorite>>(bytes);
            return new HashSet<string>(favorites.Select(f => f.FileName), StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

/// <summary>
/// Represents a single game item in the grid/list.
/// </summary>
public partial class GameItemViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName = string.Empty;

    [ObservableProperty] private string _machineName = string.Empty;

    [ObservableProperty] private string _folderPath = string.Empty;

    [ObservableProperty] private string? _coverImagePath;

    [ObservableProperty] private bool _isFavorite;

    [ObservableProperty] private bool _hasAchievements;

    [ObservableProperty] private int _timesPlayed;

    [ObservableProperty] private string _playTime = string.Empty;

    [ObservableProperty] private string _filePath = string.Empty;
}

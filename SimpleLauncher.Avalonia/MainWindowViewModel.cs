using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class MainWindowViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly IMessageDialogService _messageDialog;
    private readonly IResourceProvider _resources;
    private readonly IConfiguration _configuration;

    public MainWindowViewModel(
        SettingsManager settings,
        IMessageDialogService messageDialog,
        IResourceProvider resources,
        IConfiguration configuration)
    {
        _settings = settings;
        _messageDialog = messageDialog;
        _resources = resources;
        _configuration = configuration;

        // Load initial state from settings
        ViewMode = _settings.ViewMode ?? "GridView";
        ThumbnailSize = _settings.ThumbnailSize;
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
    }

    [RelayCommand]
    private Task SearchAsync()
    {
        // TODO: Implement search functionality
        StatusText = $"Searching for '{SearchText}'...";
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            // TODO: Load games for page
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            // TODO: Load games for page
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void GoToFavorites()
    {
        // TODO: Navigate to favorites view
        IsSubPageActive = true;
        StatusText = "Favorites";
    }

    [RelayCommand]
    private void GoToGlobalSearch()
    {
        // TODO: Navigate to global search view
        IsSubPageActive = true;
        StatusText = "Global Search";
    }

    [RelayCommand]
    private void GoToPlayHistory()
    {
        // TODO: Navigate to play history view
        IsSubPageActive = true;
        StatusText = "Play History";
    }

    [RelayCommand]
    private Task FeelingLuckyAsync()
    {
        if (Games.Count == 0) return Task.CompletedTask;

        var random = new Random();
        var index = random.Next(Games.Count);
        var game = Games[index];
        // TODO: Launch the game
        return _messageDialog.ShowInfoAsync($"Feeling lucky! Selected: {game.FileName}", "Feeling Lucky");
    }

    [RelayCommand]
    private Task SelectLetterFilterAsync(string? letter)
    {
        SelectedLetterFilter = letter;
        // TODO: Filter games by letter
        return Task.CompletedTask;
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
}

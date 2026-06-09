using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class GlobalSearchViewModel : ObservableObject, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IImageLoader _imageLoader;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IMessageDialogService _messageDialog;
    private readonly SettingsManager _settings;
    private CancellationTokenSource? _searchCts;

    public GlobalSearchViewModel(
        IConfiguration configuration,
        IImageLoader imageLoader,
        IMessageBoxLibraryService messageBox,
        IMessageDialogService messageDialog,
        SettingsManager settings)
    {
        _configuration = configuration;
        _imageLoader = imageLoader;
        _messageBox = messageBox;
        _messageDialog = messageDialog;
        _settings = settings;
    }

    // ── Search Results ──────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<SearchResultItem> _searchResults = [];

    [ObservableProperty] private SearchResultItem? _selectedResult;

    // ── Search State ────────────────────────────────────────────

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private bool _isSearching;

    [ObservableProperty] private string _searchStatus = string.Empty;

    [ObservableProperty] private bool _noResults;

    [ObservableProperty] private bool _hasResults;

    // ── Preview Image ───────────────────────────────────────────

    [ObservableProperty] private Stream? _previewImageSource;

    // ── Loading State ───────────────────────────────────────────

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = string.Empty;

    // ── Commands ────────────────────────────────────────────────

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await _messageDialog.ShowInfoAsync("Please enter a search term.", "Search");
            return;
        }

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        IsSearching = true;
        SearchStatus = "Searching...";
        NoResults = false;
        HasResults = false;
        SearchResults.Clear();

        try
        {
            // TODO: Implement actual search across systems
            // For now, show a placeholder message
            SearchStatus = "Search functionality will be implemented when SystemManager is available in Core.";
            NoResults = true;
        }
        catch (OperationCanceledException)
        {
            SearchStatus = "Search cancelled.";
        }
        catch (Exception ex)
        {
            SearchStatus = $"Search error: {ex.Message}";
            NoResults = true;
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private void CancelSearch()
    {
        _searchCts?.Cancel();
        IsSearching = false;
        SearchStatus = "Search cancelled.";
    }

    [RelayCommand]
    private async Task LaunchGameAsync()
    {
        if (SelectedResult == null)
        {
            await _messageDialog.ShowInfoAsync("Please select a game to launch.", "Launch Game");
            return;
        }

        // TODO: Implement game launching
        await _messageDialog.ShowInfoAsync("Game launching will be implemented soon.", "Launch Game");
    }

    // ── Public Methods ──────────────────────────────────────────

    public async Task UpdatePreviewImageAsync(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            PreviewImageSource = null;
            return;
        }

        try
        {
            var (stream, _) = await _imageLoader.LoadImageAsync(imagePath);
            PreviewImageSource = stream;
        }
        catch
        {
            PreviewImageSource = null;
        }
    }

    // ── IDisposable ─────────────────────────────────────────────

    public void Dispose()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        PreviewImageSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Represents a search result item.
/// </summary>
public partial class SearchResultItem : ObservableObject
{
    [ObservableProperty] private string _fileName = string.Empty;

    [ObservableProperty] private string _fileNameWithExtension = string.Empty;

    [ObservableProperty] private string _machineName = string.Empty;

    [ObservableProperty] private string _folderName = string.Empty;

    [ObservableProperty] private string _filePath = string.Empty;

    [ObservableProperty] private string _systemName = string.Empty;

    [ObservableProperty] private string? _coverImage;

    [ObservableProperty] private string? _defaultEmulator;

    [ObservableProperty] private int _score;
}

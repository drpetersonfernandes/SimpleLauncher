using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CheckPaths;
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
    private readonly GameLauncherService _gameLauncher;
    private readonly ICoreSystemConfigurationService _systemConfigService;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly IFindCoverImageService _findCoverImage;
    private CancellationTokenSource? _searchCts;

    public GlobalSearchViewModel(
        IConfiguration configuration,
        IImageLoader imageLoader,
        IMessageBoxLibraryService messageBox,
        IMessageDialogService messageDialog,
        SettingsManager settings,
        GameLauncherService gameLauncher,
        ICoreSystemConfigurationService systemConfigService,
        IGetListOfFilesService getListOfFiles,
        IFindCoverImageService findCoverImage)
    {
        _configuration = configuration;
        _imageLoader = imageLoader;
        _messageBox = messageBox;
        _messageDialog = messageDialog;
        _settings = settings;
        _gameLauncher = gameLauncher;
        _systemConfigService = systemConfigService;
        _getListOfFiles = getListOfFiles;
        _findCoverImage = findCoverImage;
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

    [ObservableProperty] private bool _hasSearched;

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
        var ct = _searchCts.Token;

        IsSearching = true;
        SearchStatus = "Searching...";
        NoResults = false;
        HasResults = false;
        SearchResults.Clear();

        try
        {
            var systemManagers = _systemConfigService.LoadSystemManagers();
            var results = new List<SearchResultItem>();
            var searchLower = SearchText.ToLowerInvariant();

            foreach (var system in systemManagers)
            {
                ct.ThrowIfCancellationRequested();

                foreach (var folder in system.SystemFolders)
                {
                    ct.ThrowIfCancellationRequested();

                    var resolvedFolder = PathHelper.ResolveRelativeToAppDirectory(folder);
                    if (string.IsNullOrEmpty(resolvedFolder) || !Directory.Exists(resolvedFolder))
                        continue;

                    var files = await _getListOfFiles.GetFilesAsync(
                        resolvedFolder,
                        system.FileFormatsToSearch,
                        system.DisableRecursiveSearch,
                        system.GroupByFolder,
                        ct);

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        if (fileName.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
                        {
                            var coverImage = _findCoverImage.FindCoverImagePath(fileName, system.SystemName, system.SystemImageFolder);

                            results.Add(new SearchResultItem
                            {
                                FileName = fileName,
                                FileNameWithExtension = Path.GetFileName(file),
                                MachineName = fileName,
                                FolderName = Path.GetDirectoryName(file) ?? string.Empty,
                                FilePath = file,
                                SystemName = system.SystemName,
                                DefaultEmulator = system.Emulators.FirstOrDefault()?.EmulatorName,
                                CoverImage = coverImage
                            });
                        }
                    }
                }
            }

            SearchResults = new ObservableCollection<SearchResultItem>(results);
            HasResults = results.Count > 0;
            NoResults = results.Count == 0;
            HasSearched = true;
            SearchStatus = results.Count > 0
                ? $"Found {results.Count} result(s) for '{SearchText}'"
                : $"No results found for '{SearchText}'";
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

        // Find the system manager for this search result
        var systemManagers = _systemConfigService.LoadSystemManagers();
        var systemManager = systemManagers.FirstOrDefault(s =>
            s.SystemName.Equals(SelectedResult.SystemName, StringComparison.OrdinalIgnoreCase));

        if (systemManager == null)
        {
            await _messageDialog.ShowErrorAsync($"System '{SelectedResult.SystemName}' not found.", "Launch Error");
            return;
        }

        var emulatorName = SelectedResult.DefaultEmulator ?? systemManager.Emulators.FirstOrDefault()?.EmulatorName;
        if (string.IsNullOrEmpty(emulatorName))
        {
            await _messageDialog.ShowErrorAsync("No emulator configured for this system.", "Launch Error");
            return;
        }

        await _gameLauncher.LaunchGameAsync(SelectedResult.FilePath, emulatorName, systemManager, _settings);
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

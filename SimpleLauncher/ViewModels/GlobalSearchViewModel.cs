#nullable enable

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GlobalSearch.Models;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class GlobalSearchViewModel : ObservableObject, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemManagers;
    private readonly List<MameManager> _machines;
    private readonly Dictionary<string, string> _mameLookup;
    private readonly FavoritesManager _favoritesManager;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IImageLoader _imageLoader;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private CancellationTokenSource _cancellationTokenSource;

    [ObservableProperty] private ObservableCollection<SearchResult> _searchResults = [];

    [ObservableProperty] private SearchResult? _selectedResult;

    [ObservableProperty] private Stream? _previewImageSource;

    partial void OnPreviewImageSourceChanging(Stream? value)
    {
        value?.Dispose();
    }

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = "";

    [ObservableProperty] private bool _noResultsVisible;

    [ObservableProperty] private bool _launchButtonEnabled;

    [ObservableProperty] private List<string> _systemNames = [];

    [ObservableProperty] private int _selectedSystemIndex;

    public GlobalSearchViewModel(
        IConfiguration configuration,
        ILogErrors logErrors,
        SettingsManager settings,
        List<SystemManager> systemManagers,
        List<MameManager> machines,
        Dictionary<string, string> mameLookup,
        FavoritesManager favoritesManager,
        PlaySoundEffects playSoundEffects,
        IGetListOfFilesService getListOfFiles,
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _settings = settings;
        _systemManagers = systemManagers;
        _machines = machines;
        _mameLookup = mameLookup;
        _favoritesManager = favoritesManager;
        _playSoundEffects = playSoundEffects;
        _getListOfFiles = getListOfFiles;
        _findCoverImage = findCoverImage;
        _imageLoader = imageLoader;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _cancellationTokenSource = new CancellationTokenSource();

        InitializeSystemNames();
    }

    private void InitializeSystemNames()
    {
        var allSystemsString = _resourceProvider.GetString("AllSystems", "All Systems");
        var names = new List<string> { allSystemsString };
        names.AddRange(_systemManagers.Select(static sm => sm.SystemName).OrderBy(static name => name));
        SystemNames = names;
        SelectedSystemIndex = 0;
    }

    public async Task SearchAsync(string searchTerm, string? selectedSystem,
        bool searchFilename, bool searchMameDescription, bool searchFolderName, bool searchRecursively)
    {
        try
        {
            // Cancel previous search
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();

            // Validate search terms
            var parsedTerms = ParseSearchTerms(searchTerm);
            var hasMeaningfulKeywords = parsedTerms
                .Any(static t => !t.Equals("and", StringComparison.OrdinalIgnoreCase) &&
                                 !t.Equals("or", StringComparison.OrdinalIgnoreCase));

            if (!hasMeaningfulKeywords)
            {
                await _messageBox.EnterValidSearchTermsMessageBox();
                return;
            }

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                await _messageBox.PleaseEnterSearchTermMessageBox();
                return;
            }

            LaunchButtonEnabled = false;
            PreviewImageSource = null;
            IsLoading = true;
            LoadingMessage = _resourceProvider.GetString("Searchingpleasewait", "Searching... Please wait.");
            NoResultsVisible = false;

            await Task.Yield();

            try
            {
                var results = await PerformSearchAsync(
                    searchTerm, selectedSystem, searchFilename, searchMameDescription,
                    searchFolderName, searchRecursively, _cancellationTokenSource.Token);

                if (results.Count > 0)
                {
                    SearchResults = new ObservableCollection<SearchResult>(results);
                    NoResultsVisible = false;
                }
                else
                {
                    SearchResults = [];
                    NoResultsVisible = true;
                    PreviewImageSource = null;
                }
            }
            catch (OperationCanceledException)
            {
                // Search was canceled - ignore
            }
            catch (Exception ex)
            {
                await _logErrors.LogErrorAsync(ex, "Error during search operation.");
                await _messageBox.GlobalSearchErrorMessageBox();
                NoResultsVisible = true;
            }
            finally
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    IsLoading = false;
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "Error in SearchAsync.");
        }
    }

    private async Task<List<SearchResult>> PerformSearchAsync(string searchTerm, string? selectedSystem,
        bool searchFilename, bool searchMameDescription, bool searchFolderName, bool searchRecursively,
        CancellationToken token)
    {
        var results = new List<SearchResult>();
        var searchTerms = ParseSearchTerms(searchTerm);

        var allSystemsString = _resourceProvider.GetString("AllSystems", "All Systems");
        IEnumerable<SystemManager> systemsToSearch = _systemManagers;
        if (selectedSystem != allSystemsString)
        {
            systemsToSearch = _systemManagers.Where(sm =>
                sm.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var systemManager in systemsToSearch)
        {
            token.ThrowIfCancellationRequested();

            var effectiveSystemManager = searchRecursively switch
            {
                true when systemManager.DisableRecursiveSearch => new SystemManager
                {
                    SystemName = systemManager.SystemName,
                    SystemFolders = systemManager.SystemFolders,
                    SystemImageFolder = systemManager.SystemImageFolder,
                    FileFormatsToSearch = systemManager.FileFormatsToSearch,
                    ExtractFileBeforeLaunch = systemManager.ExtractFileBeforeLaunch,
                    FileFormatsToLaunch = systemManager.FileFormatsToLaunch,
                    Emulators = systemManager.Emulators,
                    GroupByFolder = systemManager.GroupByFolder,
                    DisableRecursiveSearch = false
                },
                false when !systemManager.DisableRecursiveSearch => new SystemManager
                {
                    SystemName = systemManager.SystemName,
                    SystemFolders = systemManager.SystemFolders,
                    SystemImageFolder = systemManager.SystemImageFolder,
                    FileFormatsToSearch = systemManager.FileFormatsToSearch,
                    ExtractFileBeforeLaunch = systemManager.ExtractFileBeforeLaunch,
                    FileFormatsToLaunch = systemManager.FileFormatsToLaunch,
                    Emulators = systemManager.Emulators,
                    GroupByFolder = systemManager.GroupByFolder,
                    DisableRecursiveSearch = true
                },
                _ => systemManager
            };

            foreach (var systemFolderPathRaw in systemManager.SystemFolders)
            {
                token.ThrowIfCancellationRequested();

                var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemFolderPathRaw);
                if (string.IsNullOrEmpty(systemFolderPath) || !Directory.Exists(systemFolderPath) ||
                    systemManager.FileFormatsToSearch == null)
                {
                    continue;
                }

                var matchedFilesList = await _getListOfFiles.GetFilesAsync(
                    systemFolderPath, systemManager.FileFormatsToSearch, effectiveSystemManager.DisableRecursiveSearch, effectiveSystemManager.GroupByFolder, token);

                var filesInSystemFolder = matchedFilesList.Where(file =>
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                    var filenameMatch = searchFilename &&
                                        MatchesSearchQuery(fileNameWithoutExtension.ToLowerInvariant(), searchTerms);

                    var mameDescriptionMatch = searchMameDescription &&
                                               _mameLookup.TryGetValue(fileNameWithoutExtension, out var description) &&
                                               MatchesSearchQuery(description.ToLowerInvariant(), searchTerms);

                    var folderNameMatch = searchFolderName;
                    if (folderNameMatch)
                    {
                        var dir = Path.GetDirectoryName(file);
                        var directoryName = dir is null ? null : new DirectoryInfo(dir).Name;
                        folderNameMatch = MatchesSearchQuery(directoryName?.ToLowerInvariant(), searchTerms);
                    }

                    return filenameMatch || mameDescriptionMatch || folderNameMatch;
                });

                var matchedFilePaths = filesInSystemFolder.ToList();
                foreach (var filePath in matchedFilePaths)
                {
                    token.ThrowIfCancellationRequested();

                    var machine = _machines.FirstOrDefault(m =>
                        m.MachineName.Equals(Path.GetFileNameWithoutExtension(filePath), StringComparison.OrdinalIgnoreCase));

                    results.Add(new SearchResult
                    {
                        FileName = Path.GetFileNameWithoutExtension(filePath),
                        FileNameWithExtension = Path.GetFileName(filePath),
                        FolderName = Path.GetDirectoryName(filePath)?.Split(Path.DirectorySeparatorChar).LastOrDefault(),
                        FilePath = filePath,
                        MachineName = machine?.Description ?? "",
                        SystemName = systemManager.SystemName,
                        EmulatorManager = systemManager.Emulators.FirstOrDefault(),
                        CoverImage = _findCoverImage.FindCoverImagePath(
                            Path.GetFileNameWithoutExtension(filePath), systemManager.SystemName, systemManager.SystemImageFolder)
                    });
                }
            }
        }

        return ScoreResults(results, searchTerms);
    }

    public async Task UpdatePreviewImageAsync(string? imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                PreviewImageSource = null;
                return;
            }

            var (imageStream, _) = await _imageLoader.LoadImageAsync(imagePath);
            PreviewImageSource = imageStream;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error loading preview image.");
            PreviewImageSource = null;
        }
    }

    public SystemManager? GetSystemManager(string systemName)
    {
        return _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
    }

    public void CancelSearch()
    {
        _cancellationTokenSource.Cancel();
    }

    private static List<SearchResult> ScoreResults(List<SearchResult> results, List<string> searchTerms)
    {
        foreach (var result in results)
        {
            result.Score = CalculateScore(result.FileName.ToLowerInvariant(), searchTerms);
        }

        return results.OrderByDescending(static r => r.Score)
            .ThenBy(static r => r.FileName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static int CalculateScore(string text, List<string> searchTerms)
    {
        var score = 0;
        foreach (var term in searchTerms)
        {
            var index = text.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (index < 0) continue;

            score += 10;
            score += text.Length - index;
        }

        return score;
    }

    private static bool MatchesSearchQuery(string? text, IReadOnlyCollection<string> searchTerms)
    {
        if (text == null) return false;

        var keywords = searchTerms
            .Where(static t => !t.Equals("and", StringComparison.OrdinalIgnoreCase) &&
                               !t.Equals("or", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (keywords.Count == 0) return true;

        var hasAndOperator = searchTerms.Any(static t => t.Equals("and", StringComparison.OrdinalIgnoreCase));
        var hasOrOperator = searchTerms.Any(static t => t.Equals("or", StringComparison.OrdinalIgnoreCase));

        if (hasAndOperator)
        {
            return keywords.All(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (hasOrOperator)
        {
            return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        return keywords.All(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> ParseSearchTerms(string searchTerm)
    {
        var terms = new List<string>();
        var matches = MyRegex().Matches(searchTerm);
        foreach (Match match in matches)
        {
            terms.Add(match.Value.Trim('"').ToLowerInvariant());
        }

        return terms.Where(static t => !string.IsNullOrWhiteSpace(t)).ToList();
    }

    [GeneratedRegex("""[\"](.+?)[\"]|([^ ]+)""", RegexOptions.Compiled)]
    private static partial Regex MyRegex();

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
        PreviewImageSource?.Dispose();
        PreviewImageSource = null;
        GC.SuppressFinalize(this);
    }
}
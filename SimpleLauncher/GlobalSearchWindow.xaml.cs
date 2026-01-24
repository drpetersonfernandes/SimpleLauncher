using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher;

internal partial class GlobalSearchWindow : IDisposable
{
    private CancellationTokenSource _cancellationTokenSource;
    private static readonly string LogPath = GetLogPath.Path();
    private readonly List<SystemManager> _systemManagers;
    private readonly SettingsManager _settings;
    private ObservableCollection<SearchResult> _searchResults;
    private readonly MainWindow _mainWindow;
    private readonly List<MameManager> _machines;
    private readonly Dictionary<string, string> _mameLookup;
    private readonly FavoritesManager _favoritesManager;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher _gameLauncher;
    private readonly ILogErrors _logErrors;

    public GlobalSearchWindow(
        List<SystemManager> systemManagers,
        List<MameManager> machines,
        Dictionary<string, string> mameLookup,
        FavoritesManager favoritesManager,
        SettingsManager settings,
        MainWindow mainWindow,
        GamePadController gamePadController,
        GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects,
        ILogErrors logErrors
    )
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Closed += GlobalSearchClosedAsync;

        _cancellationTokenSource = new CancellationTokenSource();
        _systemManagers = systemManagers;
        _machines = machines;
        _mameLookup = mameLookup;
        _favoritesManager = favoritesManager;
        _settings = settings;
        _mainWindow = mainWindow;
        _gamePadController = gamePadController;
        _gameLauncher = gameLauncher;
        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _searchResults = [];
        ResultsDataGrid.ItemsSource = _searchResults;
        NoResultsMessageOverlay.Visibility = Visibility.Collapsed;

        // Populate the System ComboBox
        var allSystemsString = (string)Application.Current.TryFindResource("AllSystems") ?? "All Systems";
        var systemNames = new List<string> { allSystemsString };
        systemNames.AddRange(_systemManagers.Select(static sm => sm.SystemName).OrderBy(static name => name));
        SystemComboBox.ItemsSource = systemNames;
        SystemComboBox.SelectedIndex = 0;
    }

    private async void SearchButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            // Cancel the previous source before disposing to stop running tasks
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = new CancellationTokenSource();

            var searchTerm = SearchTextBox.Text;

            // Validate search terms
            var parsedTerms = ParseSearchTerms(searchTerm);
            var hasMeaningfulKeywords = parsedTerms
                .Any(static t => !t.Equals("and", StringComparison.OrdinalIgnoreCase) &&
                                 !t.Equals("or", StringComparison.OrdinalIgnoreCase));

            if (!hasMeaningfulKeywords)
            {
                MessageBoxLibrary.EnterValidSearchTerms();
                return;
            }

            if (CheckIfSearchTermIsEmpty(searchTerm)) return;

            LaunchButton.IsEnabled = false;
            PreviewImage.Source = null;

            LoadingOverlay.Visibility = Visibility.Visible;
            NoResultsMessageOverlay.Visibility = Visibility.Collapsed;
            await Task.Yield();

            try
            {
                var selectedSystem = SystemComboBox.SelectedItem as string;
                var searchFilename = SearchFilenameCheckBox.IsChecked == true;
                var searchMameDescription = SearchMameDescriptionCheckBox.IsChecked == true;
                var searchFolderName = SearchFolderNameCheckBox.IsChecked == true;
                var searchRecursively = SearchRecursivelyCheckBox.IsChecked == true;

                // Pass the token from the NEW source
                var results = await Task.Run(() => PerformSearch(
                    searchTerm, selectedSystem, searchFilename, searchMameDescription,
                    searchFolderName, searchRecursively), _cancellationTokenSource.Token);

                _searchResults.Clear();
                if (results?.Count > 0)
                {
                    _searchResults = new ObservableCollection<SearchResult>(results);
                    ResultsDataGrid.ItemsSource = _searchResults;
                }
                else
                {
                    NoResultsMessageOverlay.Visibility = Visibility.Visible;
                    PreviewImage.Source = null;
                }
            }
            catch (OperationCanceledException)
            {
                // Search was canceled because a new search started or window closed - ignore
            }
            catch (Exception ex)
            {
                await _logErrors.LogErrorAsync(ex, "Error during search operation.");
                MessageBoxLibrary.GlobalSearchErrorMessageBox();
                NoResultsMessageOverlay.Visibility = Visibility.Visible;
            }
            finally
            {
                // Only hide overlay if this specific task wasn't cancelled
                // (prevents flickering if multiple searches are queued)
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "Error in SearchButtonClickAsync.");
        }
    }

    private List<SearchResult> PerformSearch(string searchTerm, string selectedSystem, bool searchFilename, bool searchMameDescription, bool searchFolderName, bool searchRecursively)
    {
        var results = new List<SearchResult>();
        var searchTerms = ParseSearchTerms(searchTerm);
        var token = _cancellationTokenSource.Token;

        var allSystemsString = (string)Application.Current.TryFindResource("AllSystems") ?? "All Systems";
        IEnumerable<SystemManager> systemsToSearch = _systemManagers;
        if (selectedSystem != allSystemsString)
        {
            systemsToSearch = _systemManagers.Where(sm => sm.SystemName == selectedSystem);
        }

        var searchOption = searchRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (var systemManager in systemsToSearch)
        {
            token.ThrowIfCancellationRequested();

            foreach (var systemFolderPathRaw in systemManager.SystemFolders)
            {
                token.ThrowIfCancellationRequested();

                var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemFolderPathRaw);

                if (string.IsNullOrEmpty(systemFolderPath) || !Directory.Exists(systemFolderPath) || systemManager.FileFormatsToSearch == null)
                {
                    continue;
                }

                var filesInSystemFolder = Directory.EnumerateFiles(systemFolderPath, "*.*", searchOption)
                    .Where(file => systemManager.FileFormatsToSearch.Contains(Path.GetExtension(file).TrimStart('.').ToLowerInvariant()));

                filesInSystemFolder = filesInSystemFolder.Where(file =>
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                    var filenameMatch = false;
                    if (searchFilename)
                    {
                        filenameMatch = MatchesSearchQuery(fileNameWithoutExtension.ToLowerInvariant(), searchTerms);
                    }

                    var mameDescriptionMatch = false;
                    if (searchMameDescription && systemManager.SystemIsMame && _mameLookup != null && _mameLookup.TryGetValue(fileNameWithoutExtension, out var description))
                    {
                        mameDescriptionMatch = MatchesSearchQuery(description.ToLowerInvariant(), searchTerms);
                    }

                    var folderNameMatch = false;
                    if (searchFolderName)
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

                    var searchResultItem = new SearchResult
                    {
                        FileName = Path.GetFileNameWithoutExtension(filePath),
                        FileNameWithExtension = Path.GetFileName(filePath),
                        FolderName = Path.GetDirectoryName(filePath)?.Split(Path.DirectorySeparatorChar).LastOrDefault(),
                        FilePath = filePath,
                        MachineName = GetMachineDescription(Path.GetFileNameWithoutExtension(filePath)),
                        SystemName = systemManager.SystemName,
                        EmulatorManager = systemManager.Emulators.FirstOrDefault(),
                        CoverImage = FindCoverImage.FindCoverImagePath(Path.GetFileNameWithoutExtension(filePath), systemManager.SystemName, systemManager, _settings)
                    };
                    results.Add(searchResultItem);
                }
            }
        }

        var scoredResults = ScoreResults(results, searchTerms);
        return scoredResults;

        string GetMachineDescription(string fileNameWithoutExtension)
        {
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            return machine?.Description ?? string.Empty;
        }
    }

    private static List<SearchResult> ScoreResults(List<SearchResult> results, List<string> searchTerms)
    {
        foreach (var result in results)
        {
            result.Score = CalculateScore(result.FileName.ToLowerInvariant(), searchTerms);
        }

        return results.OrderByDescending(static r => r.Score).ThenBy(static r => r.FileName, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static int CalculateScore(string text, List<string> searchTerms)
    {
        var score = 0;
        foreach (var term in searchTerms)
        {
            var index = text.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (index < 0) continue;

            score += 10; // Base score for match
            score += text.Length - index; // Higher score for earlier match
        }

        return score;
    }

    private static bool MatchesSearchQuery(string text, IReadOnlyCollection<string> searchTerms)
    {
        // Filter out "and" and "or" to get the actual search keywords
        var keywords = searchTerms
            .Where(static t => !t.Equals("and", StringComparison.OrdinalIgnoreCase) &&
                               !t.Equals("or", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // If there are no actual keywords after filtering, and the original search had terms,
        // it might mean the search was just "and" or "or", which is not a valid search on its own.
        // However, if searchTerms was empty to begin with, it's like an empty filter (match all).
        // For simplicity here, if no keywords, we assume it means no specific filtering by keywords.
        // The original logic `if (!relevantTerms.Any()) return true;` handled this.
        if (keywords.Count == 0)
        {
            // If the original searchTerms was also empty, it's a match (no filter).
            // If original searchTerms had only "and"/"or", it's effectively no keywords to match.
            return true;
        }

        // Check for the presence of "and" or "or" operators in the original search terms
        var hasAndOperator = searchTerms.Any(static term => term.Equals("and", StringComparison.OrdinalIgnoreCase));
        var hasOrOperator = searchTerms.Any(static term => term.Equals("or", StringComparison.OrdinalIgnoreCase));

        // If both "and" and "or" are present, "and" typically takes precedence, or it's an invalid query.
        // For this implementation, let's assume if "and" is present, it's an AND operation.
        // If only "or" is present, it's an OR operation.
        // If neither, it defaults to an AND operation for the keywords.

        if (hasAndOperator)
        {
            // All keywords must be present in the text
            return keywords.All(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        else if (hasOrOperator)
        {
            // Any of the keywords must be present in the text
            return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            // Default behavior (no "and" / "or" operators, or only keywords): all keywords must be present
            return keywords.All(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static List<string> ParseSearchTerms(string searchTerm)
    {
        var terms = new List<string>();
        // Regex to find quoted strings or non-space sequences
        var matches = MyRegex().Matches(searchTerm);
        foreach (Match match in matches)
        {
            terms.Add(match.Value.Trim('"').ToLowerInvariant());
        }

        return terms.Where(static t => !string.IsNullOrWhiteSpace(t)).ToList();
    }

    private async void LaunchGameFromSearchResultAsync(string filePath, string selectedSystemName, SystemManager.Emulator selectedEmulatorManager)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(selectedSystemName) || selectedEmulatorManager == null)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(null, "[LaunchGameFromSearchResultAsync] filePath or selectedSystemName or selectedEmulatorManager is null.");

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            var selectedSystemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
            if (selectedSystemManager == null)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(null, "[LaunchGameFromSearchResultAsync] System manager not found for launching game from search.");

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            await _gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorManager.EmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow, _gamePadController);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"[LaunchGameFromSearchResultAsync] Error launching game from search: {filePath}, System: {selectedSystemName}";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);
        }
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult && !string.IsNullOrEmpty(selectedResult.FilePath))
            {
                _playSoundEffects.PlayNotificationSound();
                LaunchGameFromSearchResultAsync(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorManager);
            }
            else
            {
                MessageBoxLibrary.SelectAGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in LaunchButton_Click (GlobalSearch).";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);
        }
    }

    private void GlobalSearchPrepareForRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // This check handles cases where no item is selected or the selected item is not a valid game.
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult || string.IsNullOrEmpty(selectedResult.FilePath))
            {
                return;
            }

            var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(selectedResult.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemManager == null)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(null, "SystemManager is null");

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            if (string.IsNullOrEmpty(selectedResult.FilePath))
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(null, "FilePath is null.");

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            if (string.IsNullOrEmpty(selectedResult.SystemName))
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(null, "SystemName is null.");

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            if (selectedResult.EmulatorManager == null)
            {
                // Notify developer
                _ = _logErrors.LogErrorAsync(null, "EmulatorManager is null.");

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            var context = new RightClickContext(
                selectedResult.FilePath,
                selectedResult.FileNameWithExtension,
                selectedResult.FileName,
                selectedResult.SystemName,
                systemManager,
                _machines,
                _favoritesManager,
                _settings,
                null,
                null,
                selectedResult.EmulatorManager,
                null,
                null,
                _mainWindow,
                _gamePadController,
                null,
                _gameLauncher,
                _playSoundEffects
            );

            var contextMenu = UiHelpers.ContextMenu.AddRightClickReturnContextMenu(context);
            if (contextMenu != null)
            {
                ResultsDataGrid.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in GlobalSearch right-click context menu.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private void ResultsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // This check correctly handles cases where no item is selected or the selected item is not a valid game.
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult || string.IsNullOrEmpty(selectedResult.FilePath)) return;

            _playSoundEffects.PlayNotificationSound();
            LaunchGameFromSearchResultAsync(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorManager);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in ResultsDataGrid_MouseDoubleClick (GlobalSearch).";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void SearchWhenPressEnterKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchButtonClickAsync(sender, e);
        }
    }

    private async void ActionsWhenUserSelectAResultItemAsync(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult && !string.IsNullOrEmpty(selectedResult.FilePath))
            {
                LaunchButton.IsEnabled = true; // Enable launch button when a valid item is selected
                var (loadedImage, _) = await ImageLoader.LoadImageAsync(selectedResult.CoverImage);
                PreviewImage.Source = loadedImage;
            }
            else
            {
                // This branch will be hit if ResultsDataGrid.SelectedItem is null (no selection)
                LaunchButton.IsEnabled = false; // Disable if no item is selected
                PreviewImage.Source = null;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error loading image in ActionsWhenUserSelectAResultItemAsync (GlobalSearch).");

            PreviewImage.Source = null; // Ensure preview is cleared on error
        }
    }

    private void GlobalSearchClosedAsync(object sender, EventArgs e)
    {
        try
        {
            // Cancel all operations first
            _cancellationTokenSource?.Cancel();

            // Cleanup resources
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            _searchResults?.Clear();
            _searchResults = null;
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error cleaning up resources on window close.");
        }
    }

    private static bool CheckIfSearchTermIsEmpty(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            return false;
        }

        // Notify user
        MessageBoxLibrary.PleaseEnterSearchTermMessageBox();

        return true;
    }

    [GeneratedRegex("""[\"](.+?)[\"]|([^ ]+)""", RegexOptions.Compiled)]
    private static partial Regex MyRegex();

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}
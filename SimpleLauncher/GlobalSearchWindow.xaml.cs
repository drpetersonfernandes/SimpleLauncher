using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class GlobalSearchWindow
{
    private static readonly string LogPath = GetLogPath.Path();
    private readonly List<SystemManager> _systemManagers;
    private readonly SettingsManager _settings;
    private ObservableCollection<SearchResult> _searchResults;
    private PleaseWaitWindow _pleaseWaitWindow;
    private readonly MainWindow _mainWindow;
    private readonly List<MameManager> _machines;
    private readonly Dictionary<string, string> _mameLookup;
    private readonly FavoritesManager _favoritesManager;

    public GlobalSearchWindow(List<SystemManager> systemManagers, List<MameManager> machines, Dictionary<string, string> mameLookup, SettingsManager settings, FavoritesManager favoritesManager, MainWindow mainWindow)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Closed += GlobalSearch_Closed;

        _systemManagers = systemManagers;
        _machines = machines;
        _mameLookup = mameLookup;
        _settings = settings;
        _favoritesManager = favoritesManager;
        _searchResults = [];
        ResultsDataGrid.ItemsSource = _searchResults;
        _mainWindow = mainWindow;
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var searchTerm = SearchTextBox.Text;
            if (CheckIfSearchTermIsEmpty(searchTerm)) return;

            LaunchButton.IsEnabled = false;
            PreviewImage.Source = null; // Clear preview on new search
            _searchResults.Clear();

            var searchingpleasewait = (string)Application.Current.TryFindResource("Searchingpleasewait") ?? "Searching, please wait...";
            _pleaseWaitWindow = new PleaseWaitWindow(searchingpleasewait) { Owner = this };
            _pleaseWaitWindow.Show();

            try
            {
                // PerformSearch itself runs on a background thread.
                // It will now also spawn subtasks for file sizes.
                var results = await Task.Run(() => PerformSearch(searchTerm));

                if (results != null && results.Count != 0)
                {
                    foreach (var result in results)
                    {
                        // Adding to ObservableCollection will make items appear in the UI.
                        // FileSizeBytes will initially show "Calculating..."
                        _searchResults.Add(result);
                    }
                    // LaunchButton.IsEnabled = true; // Enable only if items are selected or always if results exist
                }
                else
                {
                    var noresultsfound2 = (string)Application.Current.TryFindResource("Noresultsfound") ?? "No results found.";
                    // Create a dummy result for "No results found"
                    // No need for async size calculation here.
                    _searchResults.Add(new SearchResult
                    {
                        FileName = noresultsfound2,
                        FileNameWithExtension = noresultsfound2,
                        FolderName = "",
                        FileSizeBytes = 0, // Or -2 to show N/A if preferred for a "no result" item
                        MachineName = "",
                        FilePath = "",
                        SystemName = "",
                        CoverImage = ""
                    });
                    LaunchButton.IsEnabled = false; // No results, so disable the launch button
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error during search operation.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.GlobalSearchErrorMessageBox();
            }
            finally
            {
                _pleaseWaitWindow.Close();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in SearchButton_Click.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private List<SearchResult> PerformSearch(string searchTerm) // This method runs on a background thread
    {
        var results = new List<SearchResult>();
        var searchTerms = ParseSearchTerms(searchTerm);

        foreach (var systemManager in _systemManagers)
        {
            var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemManager.SystemFolder);

            if (!Directory.Exists(systemFolderPath) || systemManager.FileFormatsToSearch == null)
                continue;

            var filesInSystemFolder = Directory.EnumerateFiles(systemFolderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => systemManager.FileFormatsToSearch.Contains(Path.GetExtension(file).TrimStart('.').ToLowerInvariant()));

            if (systemManager.SystemIsMame && _mameLookup != null)
            {
                filesInSystemFolder = filesInSystemFolder.Where(file =>
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    if (MatchesSearchQuery(fileNameWithoutExtension.ToLowerInvariant(), searchTerms))
                        return true;
                    if (_mameLookup.TryGetValue(fileNameWithoutExtension, out var description))
                        return MatchesSearchQuery(description.ToLowerInvariant(), searchTerms);

                    return false;
                });
            }
            else
            {
                filesInSystemFolder = filesInSystemFolder.Where(file => MatchesSearchQuery(Path.GetFileName(file).ToLowerInvariant(), searchTerms));
            }

            var matchedFilePaths = filesInSystemFolder.ToList();

            foreach (var filePath in matchedFilePaths)
            {
                var searchResultItem = new SearchResult
                {
                    FileName = Path.GetFileNameWithoutExtension(filePath),
                    FileNameWithExtension = Path.GetFileName(filePath),
                    FolderName = Path.GetDirectoryName(filePath)?.Split(Path.DirectorySeparatorChar).LastOrDefault(),
                    FilePath = filePath,
                    FileSizeBytes = -1, // Initialize with placeholder for "Calculating..."
                    MachineName = GetMachineDescription(Path.GetFileNameWithoutExtension(filePath)),
                    SystemName = systemManager.SystemName,
                    EmulatorConfig = systemManager.Emulators.FirstOrDefault(),
                    CoverImage = FindCoverImage.FindCoverImagePath(Path.GetFileNameWithoutExtension(filePath), systemManager.SystemName, systemManager)
                };
                results.Add(searchResultItem);

                // Start a background task to get the file size for this specific searchResultItem.
                // Capture the item for the closure.
                _ = Task.Run(() => // Fire and forget, UI updates via INotifyPropertyChanged
                {
                    try
                    {
                        if (File.Exists(searchResultItem.FilePath))
                        {
                            searchResultItem.FileSizeBytes = new FileInfo(searchResultItem.FilePath).Length;
                        }
                        else
                        {
                            // Notify developer
                            var contextMessage = $"GlobalSearch: File not found during async size calculation: {searchResultItem.FilePath}";
                            _ = LogErrors.LogErrorAsync(new FileNotFoundException(contextMessage, searchResultItem.FilePath), contextMessage);

                            searchResultItem.FileSizeBytes = -2; // Indicate Not Found/Error (will show "N/A")
                        }
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        var contextMessage = $"GlobalSearch: Error getting file size async for: {searchResultItem.FilePath}";
                        _ = LogErrors.LogErrorAsync(ex, contextMessage);

                        searchResultItem.FileSizeBytes = -2; // Indicate Not Found/Error
                    }
                });
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

        return results.OrderByDescending(r => r.Score).ThenBy(r => r.FileName).ToList();
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

    private static bool MatchesSearchQuery(string text, List<string> searchTerms)
    {
        // Filter out "and" and "or" to get the actual search keywords
        var keywords = searchTerms
            .Where(t => !t.Equals("and", StringComparison.OrdinalIgnoreCase) &&
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
        var hasAndOperator = searchTerms.Any(term => term.Equals("and", StringComparison.OrdinalIgnoreCase));
        var hasOrOperator = searchTerms.Any(term => term.Equals("or", StringComparison.OrdinalIgnoreCase));

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

    private async void LaunchGameFromSearchResult(string filePath, string selectedSystemName, SystemManager.Emulator selectedEmulatorManager)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(selectedSystemName) || selectedEmulatorManager == null)
            {
                // Notify developer
                var ex = new Exception("filePath or selectedSystemName or selectedEmulatorManager is null.");
                _ = LogErrors.LogErrorAsync(ex, "Invalid parameters for launching game from search.");

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            var selectedSystemManager = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
            if (selectedSystemManager == null)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new Exception("selectedSystemManager is null."), "System manager not found for launching game from search.");
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            await GameLauncher.HandleButtonClick(filePath, selectedEmulatorManager.EmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Error launching game from search: {filePath}, System: {selectedSystemName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
                PlayClick.PlayNotificationSound();
                LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);
        }
    }

    private void GlobalSearchRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult || string.IsNullOrEmpty(selectedResult.FilePath)) return;

            var systemConfig = _systemManagers.FirstOrDefault(config =>
                config.SystemName.Equals(selectedResult.SystemName, StringComparison.OrdinalIgnoreCase));

            if (CheckSystemConfig(systemConfig)) return;

            AddRightClickContextMenuGlobalSearchWindow(selectedResult, selectedResult.FileName, systemConfig, selectedResult.FileNameWithExtension, selectedResult.FilePath);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in GlobalSearch right-click context menu.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private void ResultsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult ||
                string.IsNullOrEmpty(selectedResult.FilePath)) return;

            PlayClick.PlayNotificationSound();
            LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in ResultsDataGrid_MouseDoubleClick (GlobalSearch).";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(LogPath);
        }
    }

    private void SearchWhenPressEnterKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchButton_Click(sender, e);
        }
    }

    private async void ActionsWhenUserSelectAResultItem(object sender, SelectionChangedEventArgs e)
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
                LaunchButton.IsEnabled = false; // Disable if no item or dummy item is selected
                PreviewImage.Source = null;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error loading image in ActionsWhenUserSelectAResultItem (GlobalSearch).");

            PreviewImage.Source = null; // Ensure preview is cleared on error
        }
    }

    private void GlobalSearch_Closed(object sender, EventArgs e)
    {
        _searchResults?.Clear(); // Clear the collection
        _searchResults = null; // Allow GC
    }

    private static bool CheckSystemConfig(SystemManager systemManager)
    {
        if (systemManager != null)
        {
            return false;
        }

        // Notify developer
        const string contextMessage = "systemManager is null in GlobalSearch.";
        _ = LogErrors.LogErrorAsync(new ArgumentNullException(nameof(systemManager), contextMessage), contextMessage);

        // Notify user
        MessageBoxLibrary.ErrorLoadingSystemConfigMessageBox();

        return true;
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
}
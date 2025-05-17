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
            _searchResults.Clear();

            // Show a "Please Wait" window.
            var searchingpleasewait = (string)Application.Current.TryFindResource("Searchingpleasewait") ?? "Searching, please wait...";
            _pleaseWaitWindow = new PleaseWaitWindow(searchingpleasewait)
            {
                Owner = this
            };
            _pleaseWaitWindow.Show();

            try
            {
                var results = await Task.Run(() => PerformSearch(searchTerm));

                if (results != null && results.Count != 0)
                {
                    foreach (var result in results)
                    {
                        _searchResults.Add(result);
                    }

                    LaunchButton.IsEnabled = true;
                }
                else
                {
                    var noresultsfound2 = (string)Application.Current.TryFindResource("Noresultsfound") ??
                                          "No results found.";
                    _searchResults.Add(new SearchResult
                    {
                        FileName = noresultsfound2,
                        FolderName = "",
                        FileSizeBytes = 0
                    });
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "That was an error using the SearchButton_Click.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.GlobalSearchErrorMessageBox();
            }
            finally
            {
                // Close the "Please Wait" window
                _pleaseWaitWindow.Close();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "That was an error using the SearchButton_Click.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private List<SearchResult> PerformSearch(string searchTerm)
    {
        var results = new List<SearchResult>();
        var searchTerms = ParseSearchTerms(searchTerm);

        foreach (var systemConfig in _systemManagers)
        {
            var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemConfig.SystemFolder);

            if (!Directory.Exists(systemFolderPath))
                continue;

            // Get all files matching the file's extensions for this system
            var files = Directory.EnumerateFiles(systemFolderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => systemConfig.FileFormatsToSearch.Contains(Path.GetExtension(file).TrimStart('.').ToLowerInvariant()));

            // If the system is MAME-based and the lookup is available, use it to filter files.
            if (systemConfig.SystemIsMame && _mameLookup != null)
            {
                files = files.Where(file =>
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                    // First check: Does the filename itself match the search terms?
                    if (MatchesSearchQuery(fileNameWithoutExtension.ToLowerInvariant(), searchTerms))
                        return true;

                    // Second check: Look up the machine description using the dictionary.
                    if (_mameLookup.TryGetValue(fileNameWithoutExtension, out var description))
                    {
                        return MatchesSearchQuery(description.ToLowerInvariant(), searchTerms);
                    }

                    return false;
                });
            }
            else
            {
                // For non-MAME systems, filter by filename.
                files = files.Where(file => MatchesSearchQuery(Path.GetFileName(file).ToLowerInvariant(), searchTerms));
            }

            // Map each file into a SearchResult object.
            var fileResults = files.Select(file => new SearchResult
            {
                FileName = Path.GetFileNameWithoutExtension(file),
                FileNameWithExtension = Path.GetFileName(file),
                FolderName = Path.GetDirectoryName(file)?.Split(Path.DirectorySeparatorChar).Last(),
                FilePath = file,
                FileSizeBytes = new FileInfo(file).Length,

                MachineName = GetMachineDescription(Path.GetFileNameWithoutExtension(file)),
                SystemName = systemConfig.SystemName,
                EmulatorConfig = systemConfig.Emulators.FirstOrDefault(),
                CoverImage = FindCoverImage.FindCoverImagePath(Path.GetFileNameWithoutExtension(file), systemConfig.SystemName, systemConfig)
            }).ToList();

            results.AddRange(fileResults);
        }

        // Score and order the results before returning.
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

        return results.OrderByDescending(static r => r.Score).ThenBy(static r => r.FileName).ToList();
    }

    private static int CalculateScore(string text, List<string> searchTerms)
    {
        var score = 0;

        foreach (var index in searchTerms.Select(term => text.IndexOf(term, StringComparison.OrdinalIgnoreCase)).Where(static index => index >= 0))
        {
            score += 10;
            score += text.Length - index;
        }

        return score;
    }

    private static bool MatchesSearchQuery(string text, List<string> searchTerms)
    {
        var hasAnd = searchTerms.Contains("and");
        var hasOr = searchTerms.Contains("or");

        if (hasAnd)
        {
            return searchTerms.Where(static term => term != "and").All(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return hasOr ? searchTerms.Where(static term => term != "or").Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase)) : searchTerms.All(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> ParseSearchTerms(string searchTerm)
    {
        var terms = new List<string>();
        var matches = MyRegex().Matches(searchTerm);

        foreach (Match match in matches)
        {
            terms.Add(match.Value.Trim('"').ToLowerInvariant());
        }

        return terms;
    }

    private async void LaunchGameFromSearchResult(string filePath, string selectedSystemName, SystemManager.Emulator selectedEmulatorManager)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                // Notify developer
                const string contextMessage = "filePath is null or empty.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            if (string.IsNullOrEmpty(selectedSystemName))
            {
                // Notify developer
                const string contextMessage = "selectedSystemName is null or empty.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            if (selectedEmulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedEmulatorManager is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            var selectedSystemManager = _systemManagers.FirstOrDefault(config => config.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));
            if (selectedSystemManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedSystemManager is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);

                return;
            }

            var selectedEmulatorName = selectedEmulatorManager.EmulatorName;

            await GameLauncher.HandleButtonClick(filePath, selectedEmulatorName, selectedSystemName, selectedSystemManager, _settings, _mainWindow);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"There was an error launching the game.\n" +
                                 $"File Path: {filePath}\n" +
                                 $"System Name: {selectedSystemName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);
        }
    }

    private void LaunchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
            {
                PlayClick.PlayNotificationSound();
                LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
            }
            else
            {
                // Notify user
                MessageBoxLibrary.SelectAGameToLaunchMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "That was an error launching a game.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingGameMessageBox(LogPath);
        }
    }

    private void GlobalSearchRightClickContextMenu(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult) return;

            var fileNameWithoutExtension = selectedResult.FileName;
            var fileNameWithExtension = selectedResult.FileNameWithExtension;
            var filePath = selectedResult.FilePath;
            var systemConfig = _systemManagers.FirstOrDefault(config =>
                config.SystemName.Equals(selectedResult.SystemName, StringComparison.OrdinalIgnoreCase));

            if (CheckSystemConfig(systemConfig)) return;

            AddRightClickContextMenuGlobalSearchWindow(selectedResult, fileNameWithoutExtension, systemConfig, fileNameWithExtension, filePath);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error in the right-click context menu.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.RightClickContextMenuErrorMessageBox();
        }
    }

    private void ResultsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult) return;

            PlayClick.PlayNotificationSound();
            LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error while using the method MouseDoubleClick.";
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
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
            {
                // Use the new ImageLoader to load the image
                var (loadedImage, _) = await ImageLoader.LoadImageAsync(selectedResult.CoverImage);

                // Assign the loaded image to the PreviewImage control
                PreviewImage.Source = loadedImage;
            }
            else
            {
                // Clear the image if no item is selected or loading failed
                PreviewImage.Source = null;
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error loading image.");
        }
    }

    private void GlobalSearch_Closed(object sender, EventArgs e)
    {
        // Empty results
        _searchResults = null;
    }

    private static bool CheckSystemConfig(SystemManager systemManager)
    {
        if (systemManager != null) return false;

        // Notify developer
        const string contextMessage = "systemManager is null.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.ErrorLoadingSystemConfigMessageBox();

        return true;
    }

    private static bool CheckIfSearchTermIsEmpty(string searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm)) return false;

        // Notify user
        MessageBoxLibrary.PleaseEnterSearchTermMessageBox();

        return true;
    }

    [GeneratedRegex("""[\"].+?[\"]|[^ ]+""")]
    private static partial Regex MyRegex();
}

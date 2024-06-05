using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SimpleLauncher
{
    public partial class GlobalSearch
    {
        private readonly List<SystemConfig> _systemConfigs;
        private readonly List<MameConfig> _machines;
        private ObservableCollection<SearchResult> _searchResults;
        private PleaseWaitSearch _pleaseWaitWindow;
        private DispatcherTimer _closeTimer;

        public GlobalSearch(List<SystemConfig> systemConfigs, List<MameConfig> machines)
        {
            InitializeComponent();
            _systemConfigs = systemConfigs;
            _machines = machines;
            _searchResults = new ObservableCollection<SearchResult>();
            ResultsDataGrid.ItemsSource = _searchResults;
            Closed += GlobalSearch_Closed;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                MessageBox.Show("Please enter a search term.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LaunchButton.IsEnabled = false;
            _searchResults.Clear();

            // Show the PleaseWaitSearch window
            _pleaseWaitWindow = new PleaseWaitSearch
            {
                Owner = this
            };
            _pleaseWaitWindow.Show();

            // Start a timer to ensure the window stays open for at least 1 second
            _closeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _closeTimer.Tick += (_, _) => _closeTimer.Stop();

            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += (_, args) => args.Result = PerformSearch(searchTerm);
            backgroundWorker.RunWorkerCompleted += (_, args) =>
            {
                if (args.Error != null)
                {
                    MessageBox.Show($"An error occurred during the search: {args.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (args.Result is List<SearchResult> results && results.Any())
                    {
                        foreach (var result in results)
                        {
                            _searchResults.Add(result);
                        }
                        LaunchButton.IsEnabled = true;
                    }
                    else
                    {
                        _searchResults.Add(new SearchResult
                        {
                            FileName = "No results found.",
                            FolderName = "",
                            Size = 0
                        });
                    }
                }

                // Close the PleaseWaitSearch window after 1 second if the search is already done
                if (!_closeTimer.IsEnabled)
                {
                    _pleaseWaitWindow.Close();
                }
                else
                {
                    _closeTimer.Tick += (_, _) => _pleaseWaitWindow.Close();
                }
            };

            _closeTimer.Start();
            backgroundWorker.RunWorkerAsync();
        }

        private List<SearchResult> PerformSearch(string searchTerm)
        {
            var results = new List<SearchResult>();

            // Split the search term into individual terms or quoted phrases
            var searchTerms = ParseSearchTerms(searchTerm);

            // Search through system files
            foreach (var systemConfig in _systemConfigs)
            {
                string systemFolderPath = GetFullPath(systemConfig.SystemFolder);

                if (Directory.Exists(systemFolderPath))
                {
                    var files = Directory.GetFiles(systemFolderPath, "*.*", SearchOption.AllDirectories)
                        .Where(file => systemConfig.FileFormatsToSearch.Contains(Path.GetExtension(file).TrimStart('.').ToLower()))
                        .Where(file => MatchesSearchQuery(Path.GetFileName(file).ToLower(), searchTerms))
                        .Select(file => new SearchResult
                        {
                            FileName = Path.GetFileName(file),
                            FolderName = Path.GetDirectoryName(file)?.Split(Path.DirectorySeparatorChar).Last(),
                            FilePath = file,
                            Size = Math.Round(new FileInfo(file).Length / 1024.0, 2), // Size in KB with 2 decimal places
                            MachineName = GetMachineDescription(Path.GetFileNameWithoutExtension(file)),
                            SystemName = systemConfig.SystemName, // Associate the SystemName
                            EmulatorConfig = systemConfig.Emulators.FirstOrDefault() // Associate the first EmulatorConfig
                        })
                        .ToList();

                    results.AddRange(files);
                }
            }

            // Score and sort the results
            var scoredResults = ScoreResults(results, searchTerms);
            return scoredResults;
        }

        private List<SearchResult> ScoreResults(List<SearchResult> results, List<string> searchTerms)
        {
            foreach (var result in results)
            {
                result.Score = CalculateScore(result.FileName.ToLower(), searchTerms);
            }

            return results.OrderByDescending(r => r.Score).ThenBy(r => r.FileName).ToList();
        }

        private int CalculateScore(string text, List<string> searchTerms)
        {
            int score = 0;

            foreach (var term in searchTerms)
            {
                int index = text.IndexOf(term, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    // Increase score for each matched term
                    score += 10;

                    // Additional score based on the position of the match (earlier matches score higher)
                    score += (text.Length - index);
                }
            }

            return score;
        }

        private bool MatchesSearchQuery(string text, List<string> searchTerms)
        {
            // Ensure at least one search term or quoted phrase is matched
            return searchTerms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private List<string> ParseSearchTerms(string searchTerm)
        {
            var terms = new List<string>();
            var matches = Regex.Matches(searchTerm, @"[\""].+?[\""]|[^ ]+");

            foreach (Match match in matches)
            {
                terms.Add(match.Value.Trim('"').ToLower());
            }

            return terms;
        }

        private string GetMachineDescription(string fileNameWithoutExtension)
        {
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            return machine?.Description ?? string.Empty;
        }

        private string GetFullPath(string path)
        {
            // Remove any leading .\ from the path
            if (path.StartsWith(@".\"))
            {
                path = path.Substring(2);
            }

            // Check if the path is already absolute
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            // If not, treat it as relative to the application's base directory
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
        }

        private async void LaunchGameFromSearchResult(string filePath, string systemName, SystemConfig.Emulator emulatorConfig)
        {
            try
            {
                if (string.IsNullOrEmpty(systemName) || emulatorConfig == null)
                {
                    MessageBox.Show("There is no System or Emulator associated with that file. I cannot launch that file from this window.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var systemConfig = _systemConfigs.FirstOrDefault(config =>
                    config.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

                if (systemConfig == null)
                {
                    MessageBox.Show("System configuration not found for the selected file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Create mock ComboBox objects
                var mockSystemComboBox = new ComboBox();
                var mockEmulatorComboBox = new ComboBox();

                // Populate mock ComboBoxes
                mockSystemComboBox.ItemsSource = _systemConfigs.Select(config => config.SystemName).ToList();
                mockSystemComboBox.SelectedItem = systemConfig.SystemName;

                mockEmulatorComboBox.ItemsSource = systemConfig.Emulators.Select(emulator => emulator.EmulatorName).ToList();
                mockEmulatorComboBox.SelectedItem = emulatorConfig.EmulatorName;

                // Use GameLauncher to handle the button click
                await GameLauncher.HandleButtonClick(filePath, mockEmulatorComboBox, mockSystemComboBox, _systemConfigs);
            }
            catch (Exception ex)
            {
                string formattedException = $"There was an error launching the game from Global Search Window.\n\nException Details: {ex.Message}\n\nFile Path: {filePath}\n\nSystem Name: {systemName}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                MessageBox.Show($"{formattedException}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
                {
                    LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
                }
                else
                {
                    MessageBox.Show("Please select a game to launch.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResultsDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
                {
                    var contextMenu = new ContextMenu();
                    var launchMenuItem = new MenuItem { Header = "Launch Game" };
                    launchMenuItem.Click += (_, _) => LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
                    contextMenu.Items.Add(launchMenuItem);

                    contextMenu.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResultsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
                {
                    LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorConfig);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Click(sender, e);
            }
        }

        public class SearchResult
        {
            public string FileName { get; init; }
            public string MachineName { get; init; }
            public string FolderName { get; init; }
            public string FilePath { get; init; }
            public double Size { get; set; } // Size in KB
            public string SystemName { get; init; } // Add SystemName property
            public SystemConfig.Emulator EmulatorConfig { get; init; } // Add EmulatorConfig property
            public int Score { get; set; } // Add Score property
        }

        private void GlobalSearch_Closed(object sender, EventArgs e)
        {
            _searchResults = null;
        }
    }
}

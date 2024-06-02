using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleLauncher
{
    public partial class GlobalSearch
    {
        private readonly List<SystemConfig> _systemConfigs;
        private readonly List<MameConfig> _machines;
        private ObservableCollection<SearchResult> _searchResults;
        private PleaseWaitSearch _pleaseWaitWindow;

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
            string searchTerm = SearchTextBox.Text.ToLower();
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
                _pleaseWaitWindow.Close();
            };

            backgroundWorker.RunWorkerAsync();
        }

        private List<SearchResult> PerformSearch(string searchTerm)
        {
            var results = new List<SearchResult>();

            foreach (var systemConfig in _systemConfigs)
            {
                string systemFolderPath = GetFullPath(systemConfig.SystemFolder);

                if (Directory.Exists(systemFolderPath))
                {
                    var files = Directory.GetFiles(systemFolderPath, "*.*", SearchOption.AllDirectories)
                        .Where(file => systemConfig.FileFormatsToSearch.Contains(Path.GetExtension(file).TrimStart('.').ToLower()))
                        .Where(file => MatchesSearchQuery(Path.GetFileName(file).ToLower(), searchTerm))
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
                        .OrderBy(x => x.FileName)
                        .ToList();

                    results.AddRange(files);
                }
            }

            return results;
        }

        private string GetMachineDescription(string fileNameWithoutExtension)
        {
            var machine = _machines.FirstOrDefault(m => m.MachineName.Equals(fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase));
            return machine?.Description ?? string.Empty;
        }

        private bool MatchesSearchQuery(string fileName, string searchQuery)
        {
            var terms = searchQuery.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool result = false;
            bool isAnd = false;
            bool isOr = false;
            bool isNot = false;

            foreach (var term in terms)
            {
                if (term == "and")
                {
                    isAnd = true;
                    continue;
                }
                if (term == "or")
                {
                    isOr = true;
                    continue;
                }
                if (term == "not")
                {
                    isNot = true;
                    continue;
                }

                var currentCondition = fileName.Contains(term);

                if (isNot)
                {
                    currentCondition = !currentCondition;
                    isNot = false;
                }

                if (isAnd)
                {
                    result = result && currentCondition;
                    isAnd = false;
                }
                else if (isOr)
                {
                    result = result || currentCondition;
                    isOr = false;
                }
                else
                {
                    result = currentCondition;
                }
            }

            return result;
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
        }

        private void GlobalSearch_Closed(object sender, EventArgs e)
        {
            _searchResults = null;
        }
    }
}

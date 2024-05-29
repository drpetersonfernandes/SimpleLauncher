using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher
{
    public partial class GlobalSearch : Window
    {
        private readonly List<SystemConfig> _systemConfigs;

        public GlobalSearch()
        {
            InitializeComponent();
            string systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");
            _systemConfigs = SystemConfig.LoadSystemConfigs(systemXmlPath);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.ToLower();
            ResultsDataGrid.Items.Clear();
            LaunchButton.IsEnabled = false;

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                MessageBox.Show("Please enter a search term.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                            FolderName = Path.GetDirectoryName(file).Split(Path.DirectorySeparatorChar).Last(),
                            FilePath = file,
                            Size = Math.Round(new FileInfo(file).Length / 1024.0, 2) // Size in KB with 2 decimal places
                        })
                        .OrderBy(x => x.FileName)
                        .ToList();

                    results.AddRange(files);
                }
            }

            if (results.Any())
            {
                foreach (var result in results)
                {
                    ResultsDataGrid.Items.Add(result);
                }
                LaunchButton.IsEnabled = true;
            }
            else
            {
                ResultsDataGrid.Items.Add(new SearchResult
                {
                    FileName = "No results found.",
                    FolderName = "",
                    Size = 0
                });
            }
        }

        private bool MatchesSearchQuery(string fileName, string searchQuery)
        {
            var terms = searchQuery.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            bool result = false;
            bool currentCondition = true;
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

                currentCondition = fileName.Contains(term);

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

        private async void LaunchGameFromSearchResult(string filePath, string folderName)
        {
            var systemConfig = _systemConfigs.FirstOrDefault(config =>
                config.SystemName.Equals(folderName, StringComparison.OrdinalIgnoreCase));

            if (systemConfig == null)
            {
                MessageBox.Show("System configuration not found for the selected file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var emulatorConfig = systemConfig.Emulators.FirstOrDefault();

            if (emulatorConfig == null)
            {
                MessageBox.Show("Emulator configuration not found for the selected file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string programLocation = emulatorConfig.EmulatorLocation;
            string parameters = emulatorConfig.EmulatorParameters;
            string arguments = $"{parameters} \"{filePath}\"";

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = programLocation,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = new Process { StartInfo = psi };
                process.Start();

                // Read the output streams
                await process.StandardOutput.ReadToEndAsync();
                await process.StandardError.ReadToEndAsync();

                // Wait for the process to exit
                await process.WaitForExitAsync();

                if (process.ExitCode != 0 && process.ExitCode != -1073741819)
                {
                    string errorMessage = $"The emulator could not open this file.\n\nExit code: {process.ExitCode}\n\nEmulator: {psi.FileName}\n\nParameters: {psi.Arguments}";
                    Exception exception = new(errorMessage);
                    await LogErrors.LogErrorAsync(exception, errorMessage);
                    MessageBox.Show($"{errorMessage}\n\nPlease visit the Simple Launcher Wiki on GitHub. There, you will find a list of parameters for each emulator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string formattedException = $"Exception Details: {ex.Message}\n\nEmulator: {programLocation}\n\nParameters: {arguments}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                MessageBox.Show($"{formattedException}\n\nPlease visit the Simple Launcher Wiki on GitHub. There, you will find a list of parameters for each emulator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
            {
                LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.FolderName);
            }
            else
            {
                MessageBox.Show("Please select a game to launch.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ResultsDataGrid_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult)
            {
                var contextMenu = new ContextMenu();
                var launchMenuItem = new MenuItem { Header = "Launch Game" };
                launchMenuItem.Click += (s, args) => LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.FolderName);
                contextMenu.Items.Add(launchMenuItem);

                contextMenu.IsOpen = true;
            }
        }

        public class SearchResult
        {
            public string FileName { get; set; }
            public string FolderName { get; set; }
            public string FilePath { get; set; } // Full file path
            public double Size { get; set; } // Size in KB
        }
    }
}

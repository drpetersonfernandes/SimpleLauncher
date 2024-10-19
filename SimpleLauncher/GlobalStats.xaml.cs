using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher
{
    public partial class GlobalStats
    {
        private readonly List<SystemConfig> _systemConfigs;

        public GlobalStats(List<SystemConfig> systemConfigs)
        {
            InitializeComponent();
            _systemConfigs = systemConfigs;
            Loaded += GlobalStats_Loaded;
            
            // Apply the theme to this window
            App.ApplyThemeToWindow(this);
        }

        private async void GlobalStats_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;

            try
            {
                var globalStats = await CalculateGlobalStats();

                GlobalInfoTextBlock.Text = $"Total Number of Systems: {globalStats.TotalSystems}\n" +
                                           $"Total Number of Emulators: {globalStats.TotalEmulators}\n" +
                                           $"Total Number of Games: {globalStats.TotalGames:N0}\n" +
                                           $"Total Number of Images: {globalStats.TotalImages:N0}\n" +
                                           $"Application Folder: {AppDomain.CurrentDomain.BaseDirectory}\n" +
                                           $"Disk Size of all Games: {globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n";

                var systemStats = await PopulateSystemStatsTable();

                ProgressBar.Visibility = Visibility.Collapsed;
                
                // Ask the user if they want to save a report
                var result = MessageBox.Show("Would you like to save a report with the results?", "Save Report", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveReport(globalStats, systemStats);
                }
            }
            catch (Exception ex)
            {
                string formattedException = $"An error occurred while calculating Global Statistics.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);

                MessageBox.Show($"An error occurred while calculating the Global Statistics.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async Task<List<SystemStatsData>> PopulateSystemStatsTable()
        {
            var systemStats = new List<SystemStatsData>(); // Create a list for DataGrid binding

            foreach (var config in _systemConfigs)
            {
                // Asynchronous file count
                int numberOfFiles = (await MainWindow.GetFilesAsync(config.SystemFolder, config.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList())).Count;

                string systemImagePath = config.SystemImageFolder;
                systemImagePath = string.IsNullOrEmpty(systemImagePath) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", config.SystemName) : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, systemImagePath.TrimStart('.', '\\'));

                int numberOfImages = 0;
                if (Directory.Exists(systemImagePath))
                {
                    // Only count .png, .jpg, .jpeg files
                    numberOfImages = Directory
                        .EnumerateFiles(systemImagePath, "*.*", SearchOption.TopDirectoryOnly)
                        .Count(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                       file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                       file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));
                }

                // Add to systemStats list
                systemStats.Add(new SystemStatsData
                {
                    SystemName = config.SystemName,
                    NumberOfFiles = numberOfFiles,
                    NumberOfImages = numberOfImages
                });
            }

            // Bind the data to the DataGrid
            SystemStatsDataGrid.ItemsSource = systemStats;

            return systemStats; // Return the system stats to be included in the report
        }

        // Class for binding data to DataGrid
        public class SystemStatsData
        {
            public string SystemName { get; init; }
            public int NumberOfFiles { get; init; }
            public int NumberOfImages { get; init; }
            
            // This property checks if the number of files and images are equal
            public bool AreFilesAndImagesEqual => NumberOfFiles == NumberOfImages;
        }

        private async Task<GlobalStatsData> CalculateGlobalStats()
        {
            return await Task.Run(async () =>
            {
                int totalSystems = _systemConfigs.Count;
                int totalEmulators = _systemConfigs.Sum(config => config.Emulators.Count);
                int totalGames = 0;
                int totalImages = 0;
                long totalDiskSize = 0;

                foreach (var config in _systemConfigs)
                {
                    var gameFiles = await MainWindow.GetFilesAsync(config.SystemFolder, config.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList());
                    totalGames += gameFiles.Count;

                    string systemImagePath = config.SystemImageFolder;
                    systemImagePath = string.IsNullOrEmpty(systemImagePath) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", config.SystemName) : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, systemImagePath.TrimStart('.', '\\'));

                    if (Directory.Exists(systemImagePath))
                    {
                        // Only count .png, .jpg, .jpeg files
                        var imageFiles = Directory.EnumerateFiles(systemImagePath, "*.*", SearchOption.TopDirectoryOnly)
                            .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                           file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                           file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        totalImages += imageFiles.Count;
                    }

                    foreach (var gameFile in gameFiles)
                    {
                        totalDiskSize += new FileInfo(gameFile).Length;
                    }
                }

                return new GlobalStatsData
                {
                    TotalSystems = totalSystems,
                    TotalEmulators = totalEmulators,
                    TotalGames = totalGames,
                    TotalImages = totalImages,
                    TotalDiskSize = totalDiskSize
                };
            });
        }

        private class GlobalStatsData
        {
            public int TotalSystems { get; init; }
            public int TotalEmulators { get; init; }
            public int TotalGames { get; init; }
            public int TotalImages { get; init; }
            public long TotalDiskSize { get; init; }
        }
        
        private void SaveReport(GlobalStatsData globalStats, List<SystemStatsData> systemStats)
        {
            // Create a SaveFileDialog to allow the user to select the location
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "GlobalStatsReport", // Default file name
                DefaultExt = ".txt", // Default file extension
                Filter = "Text documents (.txt)|*.txt" // Filter files by extension
            };

            // Show save file dialog box
            bool? result = saveFileDialog.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save the report to the specified path
                string filePath = saveFileDialog.FileName;
                try
                {
                    File.WriteAllText(filePath, GenerateReportText(globalStats, systemStats));
                    MessageBox.Show("Report saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string formattedException = $"Failed to save the report in the Global Stats window.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));
                    
                    MessageBox.Show($"Failed to save the report.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GenerateReportText(GlobalStatsData globalStats, List<SystemStatsData> systemStats)
        {
            // Global statistics
            var report = $"Global Stats Report\n" +
                         $"-------------------\n" +
                         $"Total Number of Systems: {globalStats.TotalSystems}\n" +
                         $"Total Number of Emulators: {globalStats.TotalEmulators}\n" +
                         $"Total Number of Games: {globalStats.TotalGames:N0}\n" +
                         $"Total Number of Images: {globalStats.TotalImages:N0}\n" +
                         $"Application Folder: {AppDomain.CurrentDomain.BaseDirectory}\n" +
                         $"Disk Size of all Games: {globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n\n";

            // System-specific statistics
            report += "System-Specific Stats\n";
            report += "---------------------\n";
            foreach (var system in systemStats)
            {
                report += $"System Name: {system.SystemName}\n" +
                          $"Number of ROMs or ISOs: {system.NumberOfFiles}\n" +
                          $"Number of Cover Images: {system.NumberOfImages}\n\n";
            }

            return report;
        }
    }
}
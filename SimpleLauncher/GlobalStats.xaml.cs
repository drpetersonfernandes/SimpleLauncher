// Ensure you have this namespace for MetroWindow
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher
{
    public partial class GlobalStats // Inheriting from MetroWindow
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

                await PopulateSystemStatsTable(); // Ensure it's awaited
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while calculating global stats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }
        
        private async Task PopulateSystemStatsTable()
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
        }

        // Class for binding data to DataGrid
        public class SystemStatsData
        {
            public string SystemName { get; set; }
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
    }
}

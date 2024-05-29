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
        }

        private async void GlobalStats_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var globalStats = await CalculateGlobalStats();

                GlobalInfoTextBlock.Text = $"\nTotal Number of Systems: {globalStats.TotalSystems}\n" +
                                           $"Total Number of Emulators: {globalStats.TotalEmulators}\n" +
                                           $"Total Number of Games: {globalStats.TotalGames:N0}\n" +
                                           $"Total Number of Images: {globalStats.TotalImages:N0}\n" +
                                           $"Application Folder: {AppDomain.CurrentDomain.BaseDirectory}\n" +
                                           $"Disk Size of all Games: {globalStats.TotalDiskSize / (1024.0 * 1024 * 1024):N2} TB\n";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while calculating global stats: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                    Console.WriteLine($@"Processing system: {config.SystemName}");
                    Console.WriteLine($@"System folder: {config.SystemFolder}");

                    // Await the result of GetFilesAsync to get the list of game files
                    var gameFiles = await LoadFiles.GetFilesAsync(config.SystemFolder, config.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList());
                    totalGames += gameFiles.Count;
                    Console.WriteLine($@"Found {gameFiles.Count} games in {config.SystemFolder}");

                    if (!string.IsNullOrEmpty(config.SystemImageFolder))
                    {
                        // Remove the leading dot and combine with the base directory to get the correct path
                        string systemImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.SystemImageFolder.TrimStart('.', '\\'));
                        Console.WriteLine($@"Checking images in {systemImagePath}");

                        // Ensure the directory exists before enumerating files
                        if (Directory.Exists(systemImagePath))
                        {
                            var imageFiles = Directory.EnumerateFiles(systemImagePath, "*.*", SearchOption.TopDirectoryOnly).ToList();
                            totalImages += imageFiles.Count;
                            Console.WriteLine($@"Found {imageFiles.Count} images in {systemImagePath}");
                        }
                        else
                        {
                            Console.WriteLine($@"Directory does not exist: {systemImagePath}");
                        }
                    }
                    else
                    {
                        Console.WriteLine(@"SystemImageFolder is empty or null.");
                    }

                    foreach (var gameFile in gameFiles)
                    {
                        totalDiskSize += new FileInfo(gameFile).Length;
                    }
                }

                Console.WriteLine($@"Total Systems: {totalSystems}");
                Console.WriteLine($@"Total Emulators: {totalEmulators}");
                Console.WriteLine($@"Total Games: {totalGames}");
                Console.WriteLine($@"Total Images: {totalImages}");
                Console.WriteLine($@"Total Disk Size: {totalDiskSize / (1024.0 * 1024):N2} MB");

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

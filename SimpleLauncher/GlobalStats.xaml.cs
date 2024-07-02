using OxyPlot;
using OxyPlot.Series;
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

                UpdatePieChart(globalStats);
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
                    var gameFiles = await LoadFiles.GetFilesAsync(config.SystemFolder, config.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList());
                    totalGames += gameFiles.Count;

                    string systemImagePath = config.SystemImageFolder;
                    systemImagePath = string.IsNullOrEmpty(systemImagePath) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", config.SystemName) : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, systemImagePath.TrimStart('.', '\\'));

                    if (Directory.Exists(systemImagePath))
                    {
                        var imageFiles = Directory.EnumerateFiles(systemImagePath, "*.*", SearchOption.TopDirectoryOnly).ToList();
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

        private void UpdatePieChart(GlobalStatsData globalStats)
        {
            var model = new PlotModel { Title = "Games vs Images" };

            var pieSeries = new PieSeries
            {
                StrokeThickness = 1.0,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0
            };

            pieSeries.Slices.Add(new PieSlice("Games", globalStats.TotalGames) { IsExploded = true });
            pieSeries.Slices.Add(new PieSlice("Images", globalStats.TotalImages));

            model.Series.Add(pieSeries);

            PlotView.Model = model;
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

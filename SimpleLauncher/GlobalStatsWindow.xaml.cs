using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GetListOfFiles;
using SimpleLauncher.Services.GlobalStats.Models;
using SimpleLauncher.Services.MessageBox;
using Application = System.Windows.Application;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

internal partial class GlobalStatsWindow : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly List<SystemManager> _systemManagers;
    private GlobalStatsData _globalStats;
    private List<SystemStatsData> _systemStats;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _forceClose;
    private readonly object _processingLock = new();
    private bool _isProcessing;

    internal GlobalStatsWindow(List<SystemManager> systemManagers, IConfiguration configuration)
    {
        InitializeComponent();
        _systemManagers = systemManagers;
        _configuration = configuration;

        App.ApplyThemeToWindow(this);
    }

    private async void StartButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isProcessing) return;

            _isProcessing = true;
            _forceClose = false;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                StartButton.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Collapsed;
                BusyServiceOverlay.Visibility = Visibility.Visible;
                CancelOverlayContainer.Visibility = Visibility.Visible;

                await Task.Yield();

                await ProcessGlobalStatsAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                if (!_forceClose)
                {
                    MessageBoxLibrary.OperationCancelled();
                }

                ResetUiAfterProcessing();
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "An error occurred while calculating Global Statistics.");
                if (!_forceClose)
                {
                    MessageBoxLibrary.ErrorCalculatingStatsMessageBox();
                }

                ResetUiAfterProcessing();
            }
            finally
            {
                _isProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                // Close window if user requested it during processing
                if (_forceClose)
                {
                    await Application.Current.Dispatcher.InvokeAsync(Close);
                }
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "An error occurred while calculating Global Statistics.");
        }
    }

    private async Task ProcessGlobalStatsAsync(CancellationToken cancellationToken)
    {
        // Sequential calculation
        _systemStats = await CalculateSystemStatsSequentialAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        _globalStats = CalculateGlobalStats(_systemStats);
        cancellationToken.ThrowIfCancellationRequested();

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            SystemStatsDataGrid.ItemsSource = _systemStats;

            var explanation = TryFindResource("GlobalStatsExplanation") as string ?? "Statistics calculated:";
            var statsText = $"{TryFindResource("TotalSystems") as string ?? "Total Systems:"} {_globalStats.TotalSystems}\n" +
                            $"{TryFindResource("TotalEmulators") as string ?? "Total Emulators:"} {_globalStats.TotalEmulators}\n" +
                            $"{TryFindResource("TotalGames") as string ?? "Total Games:"} {_globalStats.TotalGames:N0}\n" +
                            $"{TryFindResource("TotalImages") as string ?? "Total Matched Images:"} {_globalStats.TotalImages:N0}\n" +
                            $"{TryFindResource("TotalSystemsWithMissingImages") as string ?? "Systems with Missing Images:"} {_globalStats.TotalSystemsWithMissingImages}\n" +
                            $"{TryFindResource("ApplicationFolder") as string ?? "Folder:"} {AppDomain.CurrentDomain.BaseDirectory}\n" +
                            $"{TryFindResource("TotalDiskSize") as string ?? "Disk Size:"} {_globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n";

            GlobalInfoTextBlock.Text = explanation + "\n\n" + statsText;

            BusyServiceOverlay.Visibility = Visibility.Collapsed;
            CancelOverlayContainer.Visibility = Visibility.Collapsed;
            SaveButton.Visibility = Visibility.Visible;
        });

        DoYouWantToSaveTheReportMessageBox();

        lock (_processingLock)
        {
            if (_forceClose)
            {
                Application.Current.Dispatcher.InvokeAsync(Close);
            }
        }
    }

    private Task<List<SystemStatsData>> CalculateSystemStatsSequentialAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            var results = new List<SystemStatsData>();
            var imageExtensions = _configuration.GetSection("ImageExtensions").Get<string[]>() ?? [];
            var processingText = (string)Application.Current.TryFindResource("Processingpleasewait") ?? "Processing";
            var processingText2 = (string)Application.Current.TryFindResource("ProcessingSystem") ?? "Processing system";

            foreach (var systemManager in _systemManagers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Update UI overlay text to show current system
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    BusyServiceOverlay.Content = $"{processingText}\n" +
                                                 $"{processingText2} {systemManager.SystemName}";
                });

                var allRomFiles = new List<string>();
                foreach (var folderRaw in systemManager.SystemFolders)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var path = PathHelper.ResolveRelativeToAppDirectory(folderRaw);
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path) && systemManager.FileFormatsToSearch != null)
                    {
                        var files = await GetListOfFiles.GetFilesAsync(path, systemManager.FileFormatsToSearch, cancellationToken);
                        allRomFiles.AddRange(files);
                    }
                }

                // Sequential Disk Size Calculation (File by File)
                long totalDiskSize = 0;
                foreach (var file in allRomFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var longPath = file.StartsWith(@"\\?\", StringComparison.Ordinal) ? file : @"\\?\" + file;
                        totalDiskSize += new FileInfo(longPath).Length;
                    }
                    catch
                    {
                        /* Skip inaccessible files */
                    }
                }

                // Image Matching
                var romFileBaseNames = new HashSet<string>(allRomFiles.Select(Path.GetFileNameWithoutExtension), StringComparer.OrdinalIgnoreCase);
                var systemImageFolder = systemManager.SystemImageFolder;
                var resolvedImagePath = string.IsNullOrEmpty(systemImageFolder)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemManager.SystemName)
                    : PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);

                var numberOfImages = 0;
                if (Directory.Exists(resolvedImagePath))
                {
                    var imageFiles = Directory.EnumerateFiles(resolvedImagePath, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(f => imageExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                        .Select(Path.GetFileNameWithoutExtension);

                    numberOfImages = imageFiles.Count(img => romFileBaseNames.Contains(img));
                }

                results.Add(new SystemStatsData
                {
                    SystemName = systemManager.SystemName,
                    NumberOfFiles = allRomFiles.Count,
                    NumberOfImages = numberOfImages,
                    TotalDiskSize = totalDiskSize
                });
            }

            return results.OrderBy(static s => s.SystemName).ToList();
        }, cancellationToken);
    }

    private GlobalStatsData CalculateGlobalStats(List<SystemStatsData> systemStats)
    {
        return new GlobalStatsData
        {
            TotalSystems = systemStats.Count,
            TotalEmulators = _systemManagers.Sum(static c => c.Emulators.Count),
            TotalGames = systemStats.Sum(static s => s.NumberOfFiles),
            TotalImages = systemStats.Sum(static s => s.NumberOfImages),
            TotalDiskSize = systemStats.Sum(static s => s.TotalDiskSize),
            TotalSystemsWithMissingImages = systemStats.Count(static s => s.NumberOfFiles > s.NumberOfImages)
        };
    }

    private void ResetUiAfterProcessing()
    {
        BusyServiceOverlay.Visibility = Visibility.Collapsed;
        CancelOverlayContainer.Visibility = Visibility.Collapsed;
        StartButton.Visibility = Visibility.Visible;
        SaveButton.Visibility = Visibility.Collapsed;
        SystemStatsDataGrid.ItemsSource = null;
        GlobalInfoTextBlock.Text = TryFindResource("GlobalStatsExplanation") as string;
        BusyServiceOverlay.Content = TryFindResource("Processingpleasewait");
    }

    private void DoYouWantToSaveTheReportMessageBox()
    {
        if (MessageBoxLibrary.WoulYouLikeToSaveAReportMessageBox() == MessageBoxResult.Yes)
        {
            SaveReport(_globalStats, _systemStats);
        }
    }

    private static void SaveReport(GlobalStatsData globalStats, IEnumerable<SystemStatsData> systemStats)
    {
        var saveFileDialog = new SaveFileDialog { FileName = "GlobalStatsReport", DefaultExt = ".txt", Filter = "Text documents (.txt)|*.txt" };
        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                File.WriteAllText(saveFileDialog.FileName, GenerateReportText(globalStats, systemStats));
                MessageBoxLibrary.ReportSavedMessageBox();
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to save report.");
                MessageBoxLibrary.FailedSaveReportMessageBox();
            }
        }
    }

    private static string GenerateReportText(GlobalStatsData globalStats, IEnumerable<SystemStatsData> systemStats)
    {
        var report = $"Global Stats Report\n-------------------\n" +
                     $"Total Systems: {globalStats.TotalSystems}\n" +
                     $"Total Games: {globalStats.TotalGames:N0}\n" +
                     $"Total Disk Size: {globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n\n" +
                     $"System Specifics:\n-------------------\n";

        return systemStats.Aggregate(report, static (current, s) => current + $"{s.SystemName}: {s.NumberOfFiles} Games, {s.NumberOfImages} Images\n");
    }

    private void SaveReport_Click(object sender, RoutedEventArgs e)
    {
        SaveReport(_globalStats, _systemStats);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource is { IsCancellationRequested: false })
        {
            _cancellationTokenSource.Cancel();
            BusyServiceOverlay.Content = TryFindResource("CancellingPleasewait") ?? "Cancelling...";
            CancelOverlayContainer.Visibility = Visibility.Collapsed; // Hide button once clicked
        }
    }

    private void GlobalStatsWindow_Closing(object sender, CancelEventArgs e)
    {
        lock (_processingLock)
        {
            if (!_isProcessing)
            {
                // Not processing, allow normal close
                Dispose();
                return;
            }

            // Processing is active - ask user to confirm
            e.Cancel = true;
            if (MessageBoxLibrary.DoYouWantToCancelAndClose() == MessageBoxResult.Yes)
            {
                _forceClose = true;
                CancelButton_Click(null, null);
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }
}

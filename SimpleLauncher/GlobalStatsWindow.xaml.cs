using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class GlobalStatsWindow
{
    private readonly List<SystemManager> _systemManagers;
    private GlobalStatsData _globalStats;
    private List<SystemStatsData> _systemStats;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _forceClose;
    private readonly object _processingLock = new();
    private bool _isProcessing;

    public GlobalStatsWindow(List<SystemManager> systemManagers)
    {
        InitializeComponent();

        _systemManagers = systemManagers;
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
                ProgressBar.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;

                await ProcessGlobalStatsAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                // Operation was cancelled, just reset UI
                // Only show message if we're not force closing
                if (!_forceClose)
                {
                    MessageBox.Show(TryFindResource("OperationCancelledMessage") as string ?? "The operation was cancelled.",
                        TryFindResource("OperationCancelled") as string ?? "Operation Cancelled", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ResetUiAfterProcessing();
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "An error occurred while calculating Global Statistics.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                // Notify user
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
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "An error occurred while calculating Global Statistics.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }
    }

    private async Task ProcessGlobalStatsAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Execute the long-running operations asynchronously with cancellation support
            _systemStats = await PopulateSystemStatsTableAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            // Update the global stats asynchronously
            _globalStats = await Task.Run(() => CalculateGlobalStats(_systemStats), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Get the explanation text and append stats to it
                var explanation = TryFindResource("GlobalStatsExplanation") as string ?? "This window calculates comprehensive statistics about your game library.";
                var statsText = $"{TryFindResource("TotalSystems") as string ?? "Total Number of Systems:"} {_globalStats.TotalSystems}\n" +
                                $"{TryFindResource("TotalEmulators") as string ?? "Total Number of Emulators:"} {_globalStats.TotalEmulators}\n" +
                                $"{TryFindResource("TotalGames") as string ?? "Total Number of Games:"} {_globalStats.TotalGames:N0}\n" +
                                $"{TryFindResource("TotalImages") as string ?? "Total Number of Matched Images:"} {_globalStats.TotalImages:N0}\n" +
                                $"{TryFindResource("TotalSystemsWithMissingImages") as string ?? "Total Systems with Missing Images:"} {_globalStats.TotalSystemsWithMissingImages}\n" +
                                $"{TryFindResource("ApplicationFolder") as string ?? "Application Folder:"} {AppDomain.CurrentDomain.BaseDirectory}\n" +
                                $"{TryFindResource("TotalDiskSize") as string ?? "Disk Size of all Games:"} {_globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n";

                GlobalInfoTextBlock.Text = explanation + "\n\n" + statsText;

                ProgressBar.Visibility = Visibility.Collapsed;
                SaveButton.Visibility = Visibility.Visible;
            });

            // Ask the user if they want to save a report
            DoYouWantToSaveTheReportMessageBox();
        }
        catch (OperationCanceledException)
        {
            throw; // Re-throw to be handled by caller
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while calculating Global Statistics.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorCalculatingStatsMessageBox();
            throw; // Re-throw to be handled by caller
        }
        finally
        {
            lock (_processingLock)
            {
                _isProcessing = false;
                if (_forceClose)
                {
                    Application.Current.Dispatcher.InvokeAsync(Close);
                }
            }
        }
    }

    private void DoYouWantToSaveTheReportMessageBox()
    {
        var result = MessageBoxLibrary.WoulYouLikeToSaveAReportMessageBox();
        if (result == MessageBoxResult.Yes)
        {
            SaveReport(_globalStats, _systemStats);
        }
    }

    private void ResetUiAfterProcessing()
    {
        ProgressBar.Visibility = Visibility.Collapsed;
        StartButton.Visibility = Visibility.Visible;
        SaveButton.Visibility = Visibility.Collapsed;

        // Reset the cancel button and progress ring visibility
        GlobalCancellingOverlay.Visibility = Visibility.Collapsed;

        SystemStatsDataGrid.ItemsSource = null;

        // Restore the original explanation text
        var explanation = TryFindResource("GlobalStatsExplanation") as string ?? "This window calculates comprehensive statistics about your game library, including total systems, emulators, games, and matched images. Click 'Start Process' to begin the analysis.";
        GlobalInfoTextBlock.Text = explanation;
    }

    private async Task<List<SystemStatsData>> PopulateSystemStatsTableAsync(CancellationToken cancellationToken)
    {
        // Run the entire heavy calculation on a background thread
        var systemStats = await Task.Run(() => CalculateSystemStats(cancellationToken), cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        // Once the background task is complete, update the UI on the dispatcher thread
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            SystemStatsDataGrid.ItemsSource = systemStats;
        });

        return systemStats;
    }

    private List<SystemStatsData> CalculateSystemStats(CancellationToken cancellationToken)
    {
        // Use parallel processing to speed up calculations
        // Use a thread-safe collection for parallel operations
        var systemStats = new ConcurrentBag<SystemStatsData>();
        var imageExtensionsFromSettings = GetImageExtensions.GetExtensions();

        // To address the concern about HDD thrashing, we limit the degree of parallelism.
        // A value of 2-4 is a good balance for I/O-bound tasks on a single mechanical drive.
        // We'll use half the processor count, but cap it at 4 to be safe.
        var maxDop = Math.Max(1, Math.Min(Environment.ProcessorCount / 2, 4));
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDop,
            CancellationToken = cancellationToken
        };

        try
        {
            Parallel.ForEach(_systemManagers, parallelOptions, systemManager =>
            {
                // The CancellationToken is checked automatically by Parallel.ForEach,
                // but checking it manually inside the loop can make cancellation more responsive.
                cancellationToken.ThrowIfCancellationRequested();

                var allRomFiles = new List<string>();

                foreach (var systemFolderPathRaw in systemManager.SystemFolders)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemFolderPathRaw);

                    if (!string.IsNullOrEmpty(systemFolderPath) && Directory.Exists(systemFolderPath) && systemManager.FileFormatsToSearch != null)
                    {
                        var filesInFolder = GetListOfFiles.GetFilesAsync(systemFolderPath, systemManager.FileFormatsToSearch, cancellationToken).GetAwaiter().GetResult();
                        cancellationToken.ThrowIfCancellationRequested();
                        allRomFiles.AddRange(filesInFolder);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                var romFileBaseNames = new HashSet<string>(
                    allRomFiles.Select(Path.GetFileNameWithoutExtension),
                    StringComparer.OrdinalIgnoreCase);

                var totalDiskSize = allRomFiles.Sum(static file =>
                {
                    try
                    {
                        // Prepend long path prefix if not already present
                        var longPath = file.StartsWith(@"\\?\", StringComparison.Ordinal) ? file : @"\\?\" + file;
                        return new FileInfo(longPath).Length;
                    }
                    catch
                    {
                        return 0L; // If getting file info fails, return 0 for this file
                    }
                });

                cancellationToken.ThrowIfCancellationRequested();

                var systemImageFolder = systemManager.SystemImageFolder;
                var resolvedSystemImagePath = string.IsNullOrEmpty(systemImageFolder)
                    ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemManager.SystemName)
                    : PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);

                var numberOfImages = 0;
                if (!string.IsNullOrEmpty(resolvedSystemImagePath) && Directory.Exists(resolvedSystemImagePath))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var imageFiles = Directory.EnumerateFiles(resolvedSystemImagePath, "*.*", SearchOption.TopDirectoryOnly)
                        .Where(file => imageExtensionsFromSettings.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                        .Select(Path.GetFileNameWithoutExtension)
                        .ToList();

                    cancellationToken.ThrowIfCancellationRequested();

                    numberOfImages = imageFiles.Count(imageBaseName => romFileBaseNames.Contains(imageBaseName));
                }
                else if (!string.IsNullOrEmpty(systemManager.SystemImageFolder))
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"GlobalStats: System image folder path invalid or not found for system '{systemManager.SystemName}': '{systemManager.SystemImageFolder}' -> '{resolvedSystemImagePath}'. Cannot count images.");
                }

                cancellationToken.ThrowIfCancellationRequested();

                systemStats.Add(new SystemStatsData
                {
                    SystemName = systemManager.SystemName,
                    NumberOfFiles = allRomFiles.Count,
                    NumberOfImages = numberOfImages,
                    TotalDiskSize = totalDiskSize
                });
            });
        }
        catch (OperationCanceledException)
        {
            // The operation was canceled, which is expected. Re-throw to be handled by the caller.
            throw;
        }

        // Convert the ConcurrentBag to a List and sort it to ensure a consistent order in the UI.
        return systemStats.OrderBy(static s => s.SystemName).ToList();
    }

    private GlobalStatsData CalculateGlobalStats(List<SystemStatsData> systemStats)
    {
        try
        {
            var totalSystems = systemStats.Count;
            var totalEmulators = _systemManagers.Sum(static config => config.Emulators.Count);
            var totalGames = systemStats.Sum(static stats => stats.NumberOfFiles);
            var totalImages = systemStats.Sum(static stats => stats.NumberOfImages);
            var totalDiskSize = systemStats.Sum(static stats => stats.TotalDiskSize);
            var totalSystemsWithMissingImages = systemStats.Count(static stats => stats.NumberOfFiles > stats.NumberOfImages);

            return new GlobalStatsData
            {
                TotalSystems = totalSystems,
                TotalEmulators = totalEmulators,
                TotalGames = totalGames,
                TotalImages = totalImages,
                TotalDiskSize = totalDiskSize,
                TotalSystemsWithMissingImages = totalSystemsWithMissingImages
            };
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in the CalculateGlobalStats method.");

            // Notify user
            MessageBoxLibrary.ErrorCalculatingStatsMessageBox();

            return new GlobalStatsData(); // Return an empty object if an error occurs
        }
    }

    private static void SaveReport(GlobalStatsData globalStats, List<SystemStatsData> systemStats)
    {
        // Create a SaveFileDialog to allow the user to select the location
        var saveFileDialog = new SaveFileDialog
        {
            FileName = "GlobalStatsReport", // Default file name
            DefaultExt = ".txt", // Default file extension
            Filter = "Text documents (.txt)|*.txt" // Filter files by extension
        };

        // Show the save file dialog box
        var result = saveFileDialog.ShowDialog();

        // Process save file dialog box results
        if (result != true) return;

        // Save the report to the specified path
        var filePath = saveFileDialog.FileName;
        try
        {
            File.WriteAllText(filePath, GenerateReportText(globalStats, systemStats));

            // Notify user
            MessageBoxLibrary.ReportSavedMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to save the report in the Global Stats window.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FailedSaveReportMessageBox();
        }
    }

    private static string GenerateReportText(GlobalStatsData globalStats, List<SystemStatsData> systemStats)
    {
        // Global statistics
        var report = $"Global Stats Report\n" +
                     $"-------------------\n" +
                     $"Total Number of Systems: {globalStats.TotalSystems}\n" +
                     $"Total Number of Emulators: {globalStats.TotalEmulators}\n" +
                     $"Total Number of Games: {globalStats.TotalGames:N0}\n" +
                     $"Total Number of Matched Images: {globalStats.TotalImages:N0}\n" +
                     $"Total Systems with Missing Images: {globalStats.TotalSystemsWithMissingImages}\n" +
                     $"Application Folder: {AppDomain.CurrentDomain.BaseDirectory}\n" +
                     $"Disk Size of all Games: {globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n\n";

        // System-specific statistics
        report += "System-Specific Stats\n";
        report += "---------------------\n";

        return systemStats.Aggregate(report, static (current, system) => current + $"System Name: {system.SystemName}\n" + $"Number of ROMs or ISOs: {system.NumberOfFiles}\n" + $"Number of Matched Images: {system.NumberOfImages}\n\n");
    }

    private void SaveReport_Click(object sender, RoutedEventArgs routedEventArgs)
    {
        if (_globalStats != null && _systemStats != null)
        {
            SaveReport(_globalStats, _systemStats);
        }
        else
        {
            // Notify user
            MessageBoxLibrary.NoStatsToSaveMessageBox();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                GlobalCancellingOverlay.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "An error occurred while cancelling the operation.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }
    }

    private void GlobalStatsWindow_Closing(object sender, CancelEventArgs e)
    {
        lock (_processingLock)
        {
            if (!_isProcessing) return;

            e.Cancel = true;

            // Show message box on UI thread
            var result = MessageBox.Show(TryFindResource("ProcessingStillRunningMessage") as string ?? "Processing is still running. Do you want to cancel and close?",
                TryFindResource("ConfirmClose") as string ?? "Confirm Close", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _forceClose = true;
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();

                    // Show cancelling UI
                    GlobalCancellingOverlay.Visibility = Visibility.Visible;
                }
            }
        }
    }

    private static Task RenameImagesToMatchRomCaseAsync(string systemImagePath, HashSet<string> romFileBaseNames)
    {
        if (!Directory.Exists(systemImagePath))
            return Task.CompletedTask;

        var imageExtensionsFromSettings = GetImageExtensions.GetExtensions(); // Get extensions from service
        var imageFiles = Directory.EnumerateFiles(systemImagePath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => imageExtensionsFromSettings.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))) // Use the dynamic list
            .ToList();

        foreach (var imageFile in imageFiles)
        {
            var imageFileName = Path.GetFileNameWithoutExtension(imageFile);
            var matchedRomName = romFileBaseNames.FirstOrDefault(rom => string.Equals(rom, imageFileName, StringComparison.OrdinalIgnoreCase));

            if (matchedRomName == null || matchedRomName.Equals(imageFileName, StringComparison.Ordinal)) continue;

            var newImagePath = Path.Combine(Path.GetDirectoryName(imageFile) ?? throw new InvalidOperationException("Could not get the directory of the imageFile"),
                matchedRomName + Path.GetExtension(imageFile));
            try
            {
                File.Move(imageFile, newImagePath, true);
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"Error renaming image file: {imageFile}\n" +
                                     $"New file name: {newImagePath}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            }
        }

        return Task.CompletedTask;
    }
}
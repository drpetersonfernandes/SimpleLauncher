using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class GlobalStatsWindow
{
    private readonly List<SystemManager> _systemConfigs;
    private GlobalStatsData _globalStats;
    private List<SystemStatsData> _systemStats;

    public GlobalStatsWindow(List<SystemManager> systemConfigs)
    {
        InitializeComponent();

        _systemConfigs = systemConfigs;
        App.ApplyThemeToWindow(this);
        Loaded += GlobalStats_Loaded;
    }

    private async void GlobalStats_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            try
            {
                ProgressBar.Visibility = Visibility.Visible;
                SaveButton.Visibility = Visibility.Collapsed;

                try
                {
                    // Execute the long-running operations asynchronously
                    _systemStats = await Task.Run(PopulateSystemStatsTable);

                    // Update the global stats asynchronously
                    _globalStats = await Task.Run(() => CalculateGlobalStats(_systemStats));

                    GlobalInfoTextBlock.Text = $"{TryFindResource("TotalSystems") as string ?? "Total Number of Systems:"} {_globalStats.TotalSystems}\n" +
                                               $"{TryFindResource("TotalEmulators") as string ?? "Total Number of Emulators:"} {_globalStats.TotalEmulators}\n" +
                                               $"{TryFindResource("TotalGames") as string ?? "Total Number of Games:"} {_globalStats.TotalGames:N0}\n" +
                                               $"{TryFindResource("TotalImages") as string ?? "Total Number of Matched Images:"} {_globalStats.TotalImages:N0}\n" +
                                               $"{TryFindResource("ApplicationFolder") as string ?? "Application Folder:"} {AppDomain.CurrentDomain.BaseDirectory}\n" +
                                               $"{TryFindResource("TotalDiskSize") as string ?? "Disk Size of all Games:"} {_globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n";

                    ProgressBar.Visibility = Visibility.Collapsed;

                    // Ask the user if they want to save a report
                    DoYouWantToSaveTheReportMessageBox();

                    SaveButton.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "An error occurred while calculating Global Statistics.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ErrorCalculatingStatsMessageBox();
                }
                finally
                {
                    // Ensure that ProgressBar is hidden even if an error occurs
                    ProgressBar.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error in the GlobalStats_Loaded method.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }

            return;

            void DoYouWantToSaveTheReportMessageBox()
            {
                var result = MessageBoxLibrary.WoulYouLikeToSaveAReportMessageBox();

                if (result == MessageBoxResult.Yes)
                {
                    SaveReport(_globalStats, _systemStats);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the GlobalStats_Loaded method.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private async Task<List<SystemStatsData>> PopulateSystemStatsTable()
    {
        _systemStats = []; // Create a list for DataGrid binding
        var imageExtensionsFromSettings = GetImageExtensions.GetExtensions(); // Get extensions from service

        foreach (var config in _systemConfigs)
        {
            // Asynchronous file count and base filenames of ROMs/ISOs
            var romFiles = await GetFilePaths.GetFilesAsync(config.SystemFolder, config.FileFormatsToSearch.Select(static ext => $"*.{ext}").ToList());

            // Create a case-insensitive HashSet for ROM base filenames
            var romFileBaseNames = new HashSet<string>(
                romFiles.Select(Path.GetFileNameWithoutExtension),
                StringComparer.OrdinalIgnoreCase);

            // Calculate the total disk size for the ROM/ISO files
            var totalDiskSize = romFiles.Sum(static file => new FileInfo(file).Length);

            var systemImagePath = config.SystemImageFolder;
            systemImagePath = string.IsNullOrEmpty(systemImagePath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", config.SystemName)
                : PathHelper.ResolveRelativeToAppDirectory(systemImagePath); // Use PathHelper for robust resolution


            var numberOfImages = 0;
            if (Directory.Exists(systemImagePath))
            {
                await RenameImagesToMatchRomCaseAsync(systemImagePath, romFileBaseNames);

                // Get image files using extensions from appsettings.json
                var imageFiles = Directory.EnumerateFiles(systemImagePath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => imageExtensionsFromSettings.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();

                // Count images that have matching base filenames in romFileBaseNames
                numberOfImages = imageFiles.Count(imageBaseName => romFileBaseNames.Contains(imageBaseName));
            }

            // Add to the systemStats list with total disk size
            _systemStats.Add(new SystemStatsData
            {
                SystemName = config.SystemName,
                NumberOfFiles = romFiles.Count,
                NumberOfImages = numberOfImages,
                TotalDiskSize = totalDiskSize // Set the disk size here
            });
        }

        // Bind the data to the DataGrid (on UI thread)
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            SystemStatsDataGrid.ItemsSource = _systemStats;
        });

        return _systemStats; // Return the system stats to be included in the report
    }

    private GlobalStatsData CalculateGlobalStats(List<SystemStatsData> systemStats)
    {
        var totalSystems = systemStats.Count;
        var totalEmulators = _systemConfigs.Sum(static config => config.Emulators.Count);
        var totalGames = systemStats.Sum(static stats => stats.NumberOfFiles);
        var totalImages = systemStats.Sum(static stats => stats.NumberOfImages);
        var totalDiskSize = systemStats.Sum(static stats => stats.TotalDiskSize); // Summing pre-calculated disk sizes

        return new GlobalStatsData
        {
            TotalSystems = totalSystems,
            TotalEmulators = totalEmulators,
            TotalGames = totalGames,
            TotalImages = totalImages,
            TotalDiskSize = totalDiskSize
        };
    }

    private void SaveReport(GlobalStatsData globalStats, List<SystemStatsData> systemStats)
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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FailedSaveReportMessageBox();
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
                     $"Total Number of Matched Images: {globalStats.TotalImages:N0}\n" +
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
            var matchedRomName = romFileBaseNames.FirstOrDefault(rom =>
                string.Equals(rom, imageFileName, StringComparison.OrdinalIgnoreCase));

            if (matchedRomName == null || matchedRomName.Equals(imageFileName, StringComparison.Ordinal)) continue;

            var newImagePath = Path.Combine(Path.GetDirectoryName(imageFile) ?? throw new InvalidOperationException("Could not get the directory of the imageFile"),
                matchedRomName + Path.GetExtension(imageFile));
            try
            {
                IoOperations.FileMove(imageFile, newImagePath, true);
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"Error renaming image file: {imageFile}\n" +
                                     $"New file name: {newImagePath}";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }

        return Task.CompletedTask;
    }
}

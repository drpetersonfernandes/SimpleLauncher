using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public GlobalStatsWindow(List<SystemManager> systemManagers)
    {
        InitializeComponent();

        _systemManagers = systemManagers;
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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            }

            return;

            void DoYouWantToSaveTheReportMessageBox()
            {
                var result = MessageBoxLibrary.WoulYouLikeToSaveAReportMessageBox();
                if (result == MessageBoxResult.Yes)
                {
                    SaveReport(_globalStats, _systemStats);
                }
                else
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the GlobalStats_Loaded method.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }
    }

    private async Task<List<SystemStatsData>> PopulateSystemStatsTable()
    {
        _systemStats = [];
        var imageExtensionsFromSettings = GetImageExtensions.GetExtensions();

        foreach (var systemManager in _systemManagers)
        {
            var allRomFiles = new List<string>();

            foreach (var systemFolderPathRaw in systemManager.SystemFolders)
            {
                var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemFolderPathRaw);

                if (string.IsNullOrEmpty(systemFolderPath) || !Directory.Exists(systemFolderPath) || systemManager.FileFormatsToSearch == null)
                {
                    if (!string.IsNullOrEmpty(systemFolderPathRaw)) // Only log if a path was configured
                    {
                        // Notify developer
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"GlobalStats: System folder path invalid or not found for system '{systemManager.SystemName}': '{systemFolderPathRaw}' -> '{systemFolderPath}'. Cannot count files.");
                    }
                }
                else
                {
                    var filesInFolder = await GetListOfFiles.GetFilesAsync(systemFolderPath, systemManager.FileFormatsToSearch);
                    allRomFiles.AddRange(filesInFolder);
                }
            }

            var romFileBaseNames = new HashSet<string>(
                allRomFiles.Select(Path.GetFileNameWithoutExtension),
                StringComparer.OrdinalIgnoreCase);

            var totalDiskSize = allRomFiles.Sum(static file => new FileInfo(file).Length);

            var systemImageFolder = systemManager.SystemImageFolder;
            // Resolve the system image path using PathHelper
            var resolvedSystemImagePath = string.IsNullOrEmpty(systemImageFolder)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", systemManager.SystemName) // Default path
                : PathHelper.ResolveRelativeToAppDirectory(systemImageFolder); // Resolve configured path


            var numberOfImages = 0;
            // Check if the resolved image path is valid before proceeding
            if (!string.IsNullOrEmpty(resolvedSystemImagePath) && Directory.Exists(resolvedSystemImagePath))
            {
                // await RenameImagesToMatchRomCaseAsync(resolvedSystemImagePath, romFileBaseNames);

                var imageFiles = Directory.EnumerateFiles(resolvedSystemImagePath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => imageExtensionsFromSettings.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .Select(Path.GetFileNameWithoutExtension)
                    .ToList();

                numberOfImages = imageFiles.Count(imageBaseName => romFileBaseNames.Contains(imageBaseName));
            }
            else if (!string.IsNullOrEmpty(systemManager.SystemImageFolder)) // Only log if a path was actually configured
            {
                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"GlobalStats: System image folder path invalid or not found for system '{systemManager.SystemName}': '{systemManager.SystemImageFolder}' -> '{resolvedSystemImagePath}'. Cannot count images.");
            }

            _systemStats.Add(new SystemStatsData
            {
                SystemName = systemManager.SystemName,
                NumberOfFiles = allRomFiles.Count,
                NumberOfImages = numberOfImages,
                TotalDiskSize = totalDiskSize
            });
        }

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            SystemStatsDataGrid.ItemsSource = _systemStats;
        });

        return _systemStats;
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

            return new GlobalStatsData
            {
                TotalSystems = totalSystems,
                TotalEmulators = totalEmulators,
                TotalGames = totalGames,
                TotalImages = totalImages,
                TotalDiskSize = totalDiskSize
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
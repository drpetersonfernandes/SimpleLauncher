using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.GlobalStats.Models;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaGlobalStatsViewModel : ObservableObject, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly IFilePickerService _filePicker;
    private List<ISystemManager> _systemManagers = [];
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly object _processingLock = new();

    [ObservableProperty] private ObservableCollection<SystemStatsData> _systemStats = [];
    [ObservableProperty] private string _infoText = string.Empty;
    [ObservableProperty] private string _busyOverlayText = string.Empty;
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private bool _isBusyOverlayVisible;
    [ObservableProperty] private bool _isCancelOverlayVisible;
    [ObservableProperty] private bool _isSaveButtonVisible;
    [ObservableProperty] private bool _isStartButtonVisible = true;

    private GlobalStatsData? _globalStats;
    private bool _forceClose;

    public AvaloniaGlobalStatsViewModel(
        IConfiguration configuration,
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider,
        IFilePickerService filePicker)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _filePicker = filePicker;
    }

    public event Action? CloseRequested;

    public void Initialize(List<ISystemManager> systemManagers)
    {
        _systemManagers = systemManagers;
        InfoText = _resourceProvider.GetString("GlobalStatsExplanation", "Click Start to calculate statistics.");
        BusyOverlayText = _resourceProvider.GetString("Processingpleasewait", "Processing, please wait...");
    }

    private bool CanStart => !IsProcessing;
    private bool CanCancel => IsProcessing;
    private bool CanSaveReport => IsSaveButtonVisible;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        if (IsProcessing) return;

        IsProcessing = true;
        _forceClose = false;
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsStartButtonVisible = false;
            IsSaveButtonVisible = false;
            IsBusyOverlayVisible = true;
            IsCancelOverlayVisible = true;

            await Task.Yield();
            await ProcessGlobalStatsAsync(_cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            if (!_forceClose)
                await _messageBox.OperationCancelledMessageBox();
            ResetUiAfterProcessing();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "An error occurred while calculating Global Statistics.");
            if (!_forceClose)
                await _messageBox.ErrorCalculatingStatsMessageBox();
            ResetUiAfterProcessing();
        }
        finally
        {
            IsProcessing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (_forceClose)
                CloseRequested?.Invoke();
        }
    }

    private async Task ProcessGlobalStatsAsync(CancellationToken cancellationToken)
    {
        var systemStatsList = await CalculateSystemStatsSequentialAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        _globalStats = CalculateGlobalStats(systemStatsList);
        cancellationToken.ThrowIfCancellationRequested();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SystemStats = new ObservableCollection<SystemStatsData>(systemStatsList);

            var explanation = _resourceProvider.GetString("GlobalStatsExplanation", "Statistics calculated:");
            var totalSystemsText = _resourceProvider.GetString("TotalSystems", "Total Systems:");
            var totalEmulatorsText = _resourceProvider.GetString("TotalEmulators", "Total Emulators:");
            var totalGamesText = _resourceProvider.GetString("TotalGames", "Total Games:");
            var totalImagesText = _resourceProvider.GetString("TotalImages", "Total Matched Images:");
            var totalSystemsWithMissingImagesText = _resourceProvider.GetString("TotalSystemsWithMissingImages", "Systems with Missing Images:");
            var appFolderText = _resourceProvider.GetString("ApplicationFolder", "Folder:");
            var totalDiskSizeText = _resourceProvider.GetString("TotalDiskSize", "Disk Size:");

            var statsText = $"{totalSystemsText} {_globalStats.TotalSystems}\n" +
                            $"{totalEmulatorsText} {_globalStats.TotalEmulators}\n" +
                            $"{totalGamesText} {_globalStats.TotalGames:N0}\n" +
                            $"{totalImagesText} {_globalStats.TotalImages:N0}\n" +
                            $"{totalSystemsWithMissingImagesText} {_globalStats.TotalSystemsWithMissingImages}\n" +
                            $"{appFolderText} {AppDomain.CurrentDomain.BaseDirectory}\n" +
                            $"{totalDiskSizeText} {_globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n";

            InfoText = explanation + "\n\n" + statsText;

            IsBusyOverlayVisible = false;
            IsCancelOverlayVisible = false;
            IsSaveButtonVisible = true;
        });

        lock (_processingLock)
        {
            IsProcessing = false;

            if (_forceClose)
            {
                CloseRequested?.Invoke();
                return;
            }
        }

        await AskSaveReportAsync();
    }

    private Task<List<SystemStatsData>> CalculateSystemStatsSequentialAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            var results = new List<SystemStatsData>();
            var imageExtensions = _configuration.GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];
            var processingText = _resourceProvider.GetString("Processingpleasewait", "Processing");
            var processingSystemText = _resourceProvider.GetString("ProcessingSystem", "Processing system");

            foreach (var systemManager in _systemManagers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    BusyOverlayText = $"{processingText}\n{processingSystemText} {systemManager.SystemName}";
                });

                var allRomFiles = new List<string>();
                foreach (var folderRaw in systemManager.SystemFolders)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var path = PathHelper.ResolveRelativeToAppDirectory(folderRaw);
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path) && systemManager.FileFormatsToSearch != null)
                    {
                        var files = GetFilesAsync(path, systemManager.FileFormatsToSearch, systemManager, cancellationToken);
                        allRomFiles.AddRange(files);
                    }
                }

                long totalDiskSize = 0;
                foreach (var file in allRomFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var longPath = PathHelper.GetLongPath(file);
                        totalDiskSize += new FileInfo(longPath).Length;
                    }
                    catch
                    {
                        // Skip inaccessible files
                    }
                }

                var romFileBaseNames = new HashSet<string>(allRomFiles.Select(Path.GetFileNameWithoutExtension)!, StringComparer.OrdinalIgnoreCase);
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

                    numberOfImages = imageFiles.Count(f => f != null && romFileBaseNames.Contains(f));
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

    private static List<string> GetFilesAsync(string path, List<string> extensions, ISystemManager systemManager, CancellationToken cancellationToken)
    {
        var result = new List<string>();
        var searchOption = systemManager.DisableRecursiveSearch ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;

        foreach (var ext in extensions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var pattern = ext.StartsWith("*.", StringComparison.Ordinal) ? ext : $"*.{ext}";
                result.AddRange(Directory.EnumerateFiles(path, pattern, searchOption));
            }
            catch
            {
                // Skip inaccessible directories
            }
        }

        return result.Distinct().ToList();
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
        IsBusyOverlayVisible = false;
        IsCancelOverlayVisible = false;
        IsStartButtonVisible = true;
        IsSaveButtonVisible = false;
        SystemStats.Clear();
        InfoText = _resourceProvider.GetString("GlobalStatsExplanation", "Click Start to calculate statistics.");
        BusyOverlayText = _resourceProvider.GetString("Processingpleasewait", "Processing, please wait...");
    }

    private async Task AskSaveReportAsync()
    {
        try
        {
            var result = await _messageBox.CustomQuestionMessageBox("Save Report", "Do you want to save the report?");
            if (result)
                await SaveReportAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in AskSaveReportAsync");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveReport))]
    private async Task SaveReportAsync()
    {
        if (_globalStats == null) return;

        var filePath = await _filePicker.SaveFileAsync("Save Report", "Text files|*.txt");
        if (filePath != null)
        {
            try
            {
                var systemStatsList = SystemStats.ToList();
                File.WriteAllText(filePath, GenerateReportText(_globalStats, systemStatsList));
                await _messageBox.ReportSavedMessageBox();
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Failed to save report.");
                await _messageBox.FailedSaveReportMessageBox();
            }
        }
    }

    private string GenerateReportText(GlobalStatsData globalStats, IEnumerable<SystemStatsData> systemStats)
    {
        var titleText = _resourceProvider.GetString("GlobalStatsReportTitle", "Global Stats Report");
        var totalSystemsText = _resourceProvider.GetString("TotalSystems", "Total Systems:");
        var totalGamesText = _resourceProvider.GetString("TotalGames", "Total Games:");
        var totalDiskSizeText = _resourceProvider.GetString("TotalDiskSize", "Disk Size:");
        var systemSpecificsText = _resourceProvider.GetString("SystemSpecifics", "System Specifics:");
        var gamesText = _resourceProvider.GetString("Games", "Games");
        var imagesText = _resourceProvider.GetString("Images", "Images");

        var report = $"{titleText}\n-------------------\n" +
                     $"{totalSystemsText} {globalStats.TotalSystems}\n" +
                     $"{totalGamesText} {globalStats.TotalGames:N0}\n" +
                     $"{totalDiskSizeText} {globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n\n" +
                     $"{systemSpecificsText}\n-------------------\n";

        return systemStats.Aggregate(report, (current, s) => current + $"{s.SystemName}: {s.NumberOfFiles} {gamesText}, {s.NumberOfImages} {imagesText}\n");
    }

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        if (_cancellationTokenSource is { IsCancellationRequested: false })
        {
            _cancellationTokenSource.Cancel();
            BusyOverlayText = _resourceProvider.GetString("CancellingPleasewait", "Cancelling...");
            IsCancelOverlayVisible = false;
        }
    }

    [RelayCommand]
    private async Task ClosingAsync()
    {
        lock (_processingLock)
        {
            if (!IsProcessing)
            {
                Dispose();
                return;
            }
        }

        var result = await _messageBox.CustomQuestionMessageBox("Cancel Processing", "Processing is in progress. Do you want to cancel?");
        if (result)
        {
            _forceClose = true;
            Cancel();
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GetListOfFiles;
using SimpleLauncher.Services.GlobalStats.Models;
using SimpleLauncher.Services.MessageBox;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the GlobalStatsWindow.
/// </summary>
public class GlobalStatsViewModel : ViewModelBase, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly List<SystemManager> _systemManagers;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly object _processingLock = new();

    private ObservableCollection<SystemStatsData> _systemStats = [];
    private GlobalStatsData _globalStats;
    private string _infoText = string.Empty;
    private string _busyOverlayText = string.Empty;
    private bool _isProcessing;
    private bool _isBusyOverlayVisible;
    private bool _isCancelOverlayVisible;
    private bool _isSaveButtonVisible;
    private bool _isStartButtonVisible = true;
    private bool _forceClose;

    public GlobalStatsViewModel(List<SystemManager> systemManagers, IConfiguration configuration)
    {
        _systemManagers = systemManagers ?? throw new ArgumentNullException(nameof(systemManagers));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        // Initialize info text
        InfoText = Application.Current?.TryFindResource("GlobalStatsExplanation") as string ?? string.Empty;
        BusyOverlayText = Application.Current?.TryFindResource("Processingpleasewait") as string ?? "Processing";

        StartCommand = new RelayCommand(_ => StartProcessing(), _ => !IsProcessing);
        CancelCommand = new RelayCommand(_ => CancelProcessing(), _ => IsProcessing);
        SaveReportCommand = new RelayCommand(_ => SaveReport(), _ => IsSaveButtonVisible);
        ClosingCommand = new RelayCommand<CancelEventArgs>(OnClosing);
    }

    #region Properties

    /// <summary>
    /// Gets or sets the system statistics data.
    /// </summary>
    public ObservableCollection<SystemStatsData> SystemStats
    {
        get => _systemStats;
        private set => SetProperty(ref _systemStats, value);
    }

    /// <summary>
    /// Gets or sets the info text displayed at the top.
    /// </summary>
    public string InfoText
    {
        get => _infoText;
        private set => SetProperty(ref _infoText, value);
    }

    /// <summary>
    /// Gets or sets the busy overlay text.
    /// </summary>
    public string BusyOverlayText
    {
        get => _busyOverlayText;
        private set => SetProperty(ref _busyOverlayText, value);
    }

    /// <summary>
    /// Gets or sets whether processing is in progress.
    /// </summary>
    public bool IsProcessing
    {
        get => _isProcessing;
        private set
        {
            if (SetProperty(ref _isProcessing, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the busy overlay is visible.
    /// </summary>
    public bool IsBusyOverlayVisible
    {
        get => _isBusyOverlayVisible;
        private set => SetProperty(ref _isBusyOverlayVisible, value);
    }

    /// <summary>
    /// Gets or sets whether the cancel overlay is visible.
    /// </summary>
    public bool IsCancelOverlayVisible
    {
        get => _isCancelOverlayVisible;
        private set => SetProperty(ref _isCancelOverlayVisible, value);
    }

    /// <summary>
    /// Gets or sets whether the save button is visible.
    /// </summary>
    public bool IsSaveButtonVisible
    {
        get => _isSaveButtonVisible;
        private set => SetProperty(ref _isSaveButtonVisible, value);
    }

    /// <summary>
    /// Gets or sets whether the start button is visible.
    /// </summary>
    public bool IsStartButtonVisible
    {
        get => _isStartButtonVisible;
        private set => SetProperty(ref _isStartButtonVisible, value);
    }

    #endregion

    #region Commands

    /// <summary>
    /// Command to start the processing.
    /// </summary>
    public ICommand StartCommand { get; }

    /// <summary>
    /// Command to cancel the processing.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Command to save the report.
    /// </summary>
    public ICommand SaveReportCommand { get; }

    /// <summary>
    /// Command called when the window is closing.
    /// </summary>
    public ICommand ClosingCommand { get; }

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the window should be closed.
    /// </summary>
    public event Action CloseRequested;

    /// <summary>
    /// Event raised when a message box should be shown.
    /// </summary>
    public event Func<MessageBoxResult> ConfirmSaveReportRequested;

    /// <summary>
    /// Event raised when a message box for cancellation should be shown.
    /// </summary>
    public event Func<MessageBoxResult> ConfirmCancelRequested;

    #endregion

    private async void StartProcessing()
    {
        try
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
                {
                    MessageBoxLibrary.OperationCancelledMessageBox();
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
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                // Close window if user requested it during processing
                if (_forceClose)
                {
                    CloseRequested?.Invoke();
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
        var systemStatsList = await CalculateSystemStatsSequentialAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        _globalStats = CalculateGlobalStats(systemStatsList);
        cancellationToken.ThrowIfCancellationRequested();

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            SystemStats = new ObservableCollection<SystemStatsData>(systemStatsList);

            var explanation = Application.Current.TryFindResource("GlobalStatsExplanation") as string ?? "Statistics calculated:";
            var totalSystemsText = Application.Current.TryFindResource("TotalSystems") as string ?? "Total Systems:";
            var totalEmulatorsText = Application.Current.TryFindResource("TotalEmulators") as string ?? "Total Emulators:";
            var totalGamesText = Application.Current.TryFindResource("TotalGames") as string ?? "Total Games:";
            var totalImagesText = Application.Current.TryFindResource("TotalImages") as string ?? "Total Matched Images:";
            var totalSystemsWithMissingImagesText = Application.Current.TryFindResource("TotalSystemsWithMissingImages") as string ?? "Systems with Missing Images:";
            var appFolderText = Application.Current.TryFindResource("ApplicationFolder") as string ?? "Folder:";
            var totalDiskSizeText = Application.Current.TryFindResource("TotalDiskSize") as string ?? "Disk Size:";

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
            // Mark processing as complete BEFORE showing the save dialog.
            // MessageBox.Show pumps UI messages, allowing the user to click the
            // close button while the dialog is open. If _isProcessing were still
            // true, GlobalStatsWindow_Closing would incorrectly prompt to cancel
            // an operation that has already finished.
            IsProcessing = false;

            if (_forceClose)
            {
                CloseRequested?.Invoke();
                return;
            }
        }

        DoYouWantToSaveTheReportMessageBox();
    }

    private Task<List<SystemStatsData>> CalculateSystemStatsSequentialAsync(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            var results = new List<SystemStatsData>();
            var imageExtensions = _configuration.GetValue<string[]>("ImageExtensions") ?? [".png", ".jpg", ".jpeg"];
            var processingText = (string)Application.Current.TryFindResource("Processingpleasewait") ?? "Processing";
            var processingSystemText = (string)Application.Current.TryFindResource("ProcessingSystem") ?? "Processing system";

            foreach (var systemManager in _systemManagers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Update UI overlay text to show current system
                await Application.Current.Dispatcher.InvokeAsync(() =>
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
                        var files = await GetListOfFiles.GetFilesAsync(path, systemManager.FileFormatsToSearch, systemManager, cancellationToken);
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
                        var longPath = PathHelper.GetLongPath(file);
                        totalDiskSize += new FileInfo(longPath).Length;
                    }
                    catch
                    {
                        // Skip inaccessible files
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

                    numberOfImages = imageFiles.Count(romFileBaseNames.Contains);
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
        IsBusyOverlayVisible = false;
        IsCancelOverlayVisible = false;
        IsStartButtonVisible = true;
        IsSaveButtonVisible = false;
        SystemStats.Clear();
        InfoText = Application.Current.TryFindResource("GlobalStatsExplanation") as string ?? string.Empty;
        BusyOverlayText = Application.Current.TryFindResource("Processingpleasewait") as string ?? "Processing";
    }

    private void DoYouWantToSaveTheReportMessageBox()
    {
        var result = ConfirmSaveReportRequested?.Invoke();
        if (result == MessageBoxResult.Yes)
        {
            SaveReport();
        }
    }

    private void SaveReport()
    {
        if (_globalStats == null) return;

        var saveFileDialog = new SaveFileDialog
        {
            FileName = "GlobalStatsReport",
            DefaultExt = ".txt",
            Filter = "Text documents (.txt)|*.txt"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                var systemStatsList = SystemStats.ToList();
                File.WriteAllText(saveFileDialog.FileName, GenerateReportText(_globalStats, systemStatsList));
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
        var titleText = Application.Current.TryFindResource("GlobalStatsReportTitle") as string ?? "Global Stats Report";
        var totalSystemsText = Application.Current.TryFindResource("TotalSystems") as string ?? "Total Systems:";
        var totalGamesText = Application.Current.TryFindResource("TotalGames") as string ?? "Total Games:";
        var totalDiskSizeText = Application.Current.TryFindResource("TotalDiskSize") as string ?? "Disk Size:";
        var systemSpecificsText = Application.Current.TryFindResource("SystemSpecifics") as string ?? "System Specifics:";
        var gamesText = Application.Current.TryFindResource("Games") as string ?? "Games";
        var imagesText = Application.Current.TryFindResource("Images") as string ?? "Images";

        var report = $"{titleText}\n-------------------\n" +
                     $"{totalSystemsText} {globalStats.TotalSystems}\n" +
                     $"{totalGamesText} {globalStats.TotalGames:N0}\n" +
                     $"{totalDiskSizeText} {globalStats.TotalDiskSize / (1024.0 * 1024):N2} MB\n\n" +
                     $"{systemSpecificsText}\n-------------------\n";

        return systemStats.Aggregate(report, (current, s) => current + $"{s.SystemName}: {s.NumberOfFiles} {gamesText}, {s.NumberOfImages} {imagesText}\n");
    }

    private void CancelProcessing()
    {
        if (_cancellationTokenSource is { IsCancellationRequested: false })
        {
            _cancellationTokenSource.Cancel();
            BusyOverlayText = Application.Current.TryFindResource("CancellingPleasewait") as string ?? "Cancelling...";
            IsCancelOverlayVisible = false;
        }
    }

    private void OnClosing(CancelEventArgs e)
    {
        lock (_processingLock)
        {
            if (!IsProcessing)
            {
                // Not processing, allow normal close
                Dispose();
                return;
            }

            // Processing is active - ask user to confirm
            e.Cancel = true;
            var result = ConfirmCancelRequested?.Invoke();
            if (result == MessageBoxResult.Yes)
            {
                _forceClose = true;
                CancelProcessing();
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}

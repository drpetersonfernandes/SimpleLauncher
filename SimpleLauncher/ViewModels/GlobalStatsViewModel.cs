using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.GlobalStats.Models;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the GlobalStatsWindow.
/// </summary>
public partial class GlobalStatsViewModel : ObservableObject, IDisposable
{
    private readonly IConfiguration _configuration;
    private List<SystemManager> _systemManagers;
    private readonly ILogErrors _logErrors;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly object _processingLock = new();

    private ObservableCollection<SystemStatsData> _systemStats = [];
    private GlobalStatsData _globalStats;
    private string _infoText = "";
    private string _busyOverlayText = "";
    private bool _isProcessing;
    private bool _isBusyOverlayVisible;
    private bool _isCancelOverlayVisible;
    private bool _isSaveButtonVisible;
    private bool _isStartButtonVisible = true;
    private bool _forceClose;

    public GlobalStatsViewModel(IConfiguration configuration, ILogErrors logErrors, IGetListOfFilesService getListOfFiles, IMessageBoxLibraryService messageBox, IResourceProvider resourceProvider)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logErrors = logErrors;
        _getListOfFiles = getListOfFiles;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
    }

    public void Initialize(List<SystemManager> systemManagers)
    {
        _systemManagers = systemManagers ?? throw new ArgumentNullException(nameof(systemManagers));

        // Initialize info text
        InfoText = _resourceProvider.GetString("GlobalStatsExplanation", "");
        BusyOverlayText = _resourceProvider.GetString("Processingpleasewait", "Processing");
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
                StartCommand.NotifyCanExecuteChanged();
                CancelCommand.NotifyCanExecuteChanged();
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
        private set
        {
            if (SetProperty(ref _isSaveButtonVisible, value))
            {
                SaveReportCommand.NotifyCanExecuteChanged();
            }
        }
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

    #region CanExecute Properties

    private bool CanStart => !IsProcessing;
    private bool CanCancel => IsProcessing;
    private bool CanSaveReport => IsSaveButtonVisible;

    #endregion

    #region Events

    /// <summary>
    /// Event raised when the window should be closed.
    /// </summary>
    public event Action CloseRequested;

    #endregion

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
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
                    await _messageBox.OperationCancelledMessageBox();
                }

                ResetUiAfterProcessing();
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "An error occurred while calculating Global Statistics.");
                if (!_forceClose)
                {
                    await _messageBox.ErrorCalculatingStatsMessageBox();
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
            _logErrors.LogAndForget(ex, "An error occurred while calculating Global Statistics.");
        }
    }

    private async Task ProcessGlobalStatsAsync(CancellationToken cancellationToken)
    {
        // Sequential calculation
        var systemStatsList = await CalculateSystemStatsSequentialAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        _globalStats = CalculateGlobalStats(systemStatsList);
        cancellationToken.ThrowIfCancellationRequested();

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null) return;

        await dispatcher.InvokeAsync(() =>
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
            var processingText = _resourceProvider.GetString("Processingpleasewait", "Processing");
            var processingSystemText = _resourceProvider.GetString("ProcessingSystem", "Processing system");

            foreach (var systemManager in _systemManagers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Update UI overlay text to show current system
                var dispatcher = Application.Current?.Dispatcher;
                if (dispatcher != null)
                {
                    await dispatcher.InvokeAsync(() =>
                    {
                        BusyOverlayText = $"{processingText}\n{processingSystemText} {systemManager.SystemName}";
                    });
                }

                var allRomFiles = new List<string>();
                foreach (var folderRaw in systemManager.SystemFolders)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var path = PathHelper.ResolveRelativeToAppDirectory(folderRaw);
                    if (!string.IsNullOrEmpty(path) && Directory.Exists(path) && systemManager.FileFormatsToSearch != null)
                    {
                        var files = await _getListOfFiles.GetFilesAsync(path, systemManager.FileFormatsToSearch, systemManager.DisableRecursiveSearch, systemManager.GroupByFolder, cancellationToken);
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
                        if (longPath != null)
                        {
                            totalDiskSize += new FileInfo(longPath).Length;
                        }
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
        InfoText = _resourceProvider.GetString("GlobalStatsExplanation", "");
        BusyOverlayText = _resourceProvider.GetString("Processingpleasewait", "Processing");
    }

    private async void DoYouWantToSaveTheReportMessageBox()
    {
        try
        {
            var result = await _messageBox.WouldYouLikeToSaveAReportMessageBox();
            if (result == Interfaces.MessageBoxResult.Yes)
            {
                await SaveReport();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method DoYouWantToSaveTheReportMessageBox");
        }
    }

    [RelayCommand(CanExecute = nameof(CanSaveReport))]
    private async Task SaveReport()
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
                await File.WriteAllTextAsync(saveFileDialog.FileName, GenerateReportText(_globalStats, systemStatsList));
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
    private async Task Closing(CancelEventArgs e)
    {
        try
        {
            bool needsConfirmation;
            lock (_processingLock)
            {
                if (!IsProcessing)
                {
                    // Not processing, allow normal close
                    Dispose();
                    return;
                }

                // Processing is active - cancel the close and ask user to confirm
                e.Cancel = true;
                needsConfirmation = true;
            }

            if (needsConfirmation)
            {
                var result = await _messageBox.DoYouWantToCancelAndCloseMessageBox();
                if (result == Interfaces.MessageBoxResult.Yes)
                {
                    _forceClose = true;
                    Cancel();
                }
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method Closing.");
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}

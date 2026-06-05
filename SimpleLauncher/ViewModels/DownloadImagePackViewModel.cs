using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.DownloadService;
using SimpleLauncher.Services.DownloadService.Models;
using SimpleLauncher.Services.EasyMode;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;
using Application = System.Windows.Application;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the image pack download window.
/// </summary>
public partial class DownloadImagePackViewModel : ObservableObject, IDisposable
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly DownloadManager _downloadManager;
    private readonly ILogErrors _logErrors;
    private EasyModeManager _manager;
    private bool _disposed;
    private int _operationInProgressFlag;

    private bool _isOperationInProgress;
    private bool _isStopEnabled;
    private double _progressPercentage;
    private string _statusMessage;
    private bool _isLoading;
    private string _loadingMessage;
    private bool _isSystemDropdownEnabled = true;

    public DownloadImagePackViewModel(PlaySoundEffects playSoundEffects, ILogErrors logErrors)
    {
        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _downloadManager = App.ServiceProvider.GetRequiredService<DownloadManager>();
        _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;

        ImagePacksToDisplay = [];
        SystemNames = [];

        DownloadImagePackCommand = new RelayCommand<object>(ExecuteDownloadAsync, _ => !IsOperationInProgress);
    }

    /// <summary>Gets the collection of system names available for image pack download.</summary>
    public ObservableCollection<string> SystemNames { get; }

    /// <summary>Gets the collection of image pack items to display for the selected system.</summary>
    public ObservableCollection<ImagePackDownloadItem> ImagePacksToDisplay { get; }

    /// <summary>Gets whether a download or extraction operation is currently in progress.</summary>
    public bool IsOperationInProgress
    {
        get => _isOperationInProgress;
        private set
        {
            if (SetProperty(ref _isOperationInProgress, value))
            {
                OnPropertyChanged(nameof(IsMainContentEnabled));
                DownloadImagePackCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>Gets whether the stop button is enabled.</summary>
    public bool IsStopEnabled
    {
        get => _isStopEnabled;
        private set => SetProperty(ref _isStopEnabled, value);
    }

    /// <summary>Gets the current download progress percentage (0-100).</summary>
    public double ProgressPercentage
    {
        get => _progressPercentage;
        private set => SetProperty(ref _progressPercentage, value);
    }

    /// <summary>Gets the current status message displayed to the user.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>Gets whether a loading indicator should be shown.</summary>
    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
                OnPropertyChanged(nameof(IsMainContentEnabled));
        }
    }

    /// <summary>Gets the message displayed during loading.</summary>
    public string LoadingMessage
    {
        get => _loadingMessage;
        private set => SetProperty(ref _loadingMessage, value);
    }

    /// <summary>Gets whether the system selection dropdown is enabled.</summary>
    public bool IsSystemDropdownEnabled
    {
        get => _isSystemDropdownEnabled;
        private set => SetProperty(ref _isSystemDropdownEnabled, value);
    }

    private string _selectedSystemName;

    /// <summary>Gets or sets the currently selected system name.</summary>
    public string SelectedSystemName
    {
        get => _selectedSystemName;
        set
        {
            if (SetProperty(ref _selectedSystemName, value))
            {
                OnSystemSelectionChanged();
            }
        }
    }

    /// <summary>Gets whether the main content area is enabled (no operation or loading in progress).</summary>
    public bool IsMainContentEnabled => !IsOperationInProgress && !IsLoading;

    /// <summary>Gets the command to download the selected image pack.</summary>
    public IRelayCommand<object> DownloadImagePackCommand { get; }

    private bool TryStartOperation()
    {
        if (Interlocked.CompareExchange(ref _operationInProgressFlag, 1, 0) != 0)
            return false;

        IsOperationInProgress = true;
        return true;
    }

    private void EndOperation()
    {
        IsOperationInProgress = false;
        Interlocked.Exchange(ref _operationInProgressFlag, 0);
    }

    /// <summary>
    /// Initializes the ViewModel by loading the Easy Mode configuration.
    /// </summary>
    public async Task InitializeAsync()
    {
        LoadingMessage = (string)Application.Current.TryFindResource("Loadingconfiguration") ?? "Loading configuration...";
        IsLoading = true;
        await Task.Yield();

        _manager = await EasyModeManager.LoadAsync();

        IsLoading = false;

        if (_manager is not { Systems.Count: > 0 })
        {
            MessageBoxLibrary.ImagePackDownloaderUnavailableMessageBox();
            IsSystemDropdownEnabled = false;
            return;
        }

        PopulateSystemDropdown();
    }

    private void PopulateSystemDropdown()
    {
        try
        {
            SystemNames.Clear();

            if (_manager?.Systems == null) return;

            var systemsWithImagePacks = _manager.Systems
                .Where(static system =>
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink) ||
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink2) ||
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink3) ||
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink4) ||
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink5))
                .Select(static system => system.SystemName)
                .OrderBy(static name => name)
                .ToList();

            foreach (var name in systemsWithImagePacks)
            {
                SystemNames.Add(name);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error populating system dropdown.");
            SystemNames.Clear();
        }
    }

    private void OnSystemSelectionChanged()
    {
        ImagePacksToDisplay.Clear();

        if (string.IsNullOrEmpty(SelectedSystemName)) return;

        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return;

        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack1") ?? "Image Pack 1");
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack2") ?? "Image Pack 2");
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack3") ?? "Image Pack 3");
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack4") ?? "Image Pack 4");
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack5") ?? "Image Pack 5");
    }

    private void AddImagePackItemIfValid(string downloadLink, string extractPath, string displayName)
    {
        if (!string.IsNullOrEmpty(downloadLink) && !string.IsNullOrEmpty(extractPath))
        {
            ImagePacksToDisplay.Add(new ImagePackDownloadItem
            {
                DisplayName = displayName,
                DownloadUrl = downloadLink,
                ExtractPath = extractPath,
                State = DownloadButtonState.Idle
            });
        }
    }

    private async void ExecuteDownloadAsync(object parameter)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            if (_disposed) return;
            if (!TryStartOperation()) return;

            try
            {
                if (parameter is not ImagePackDownloadItem clickedItem)
                {
                    EndOperation();
                    return;
                }

                clickedItem.State = DownloadButtonState.Downloading;

                try
                {
                    await HandleDownloadAndExtractComponentAsync(clickedItem);
                }
                catch (Exception ex)
                {
                    if (_disposed)
                    {
                        EndOperation();
                        return;
                    }

                    _logErrors.LogAndForget(ex, $"Error in DownloadImagePackButtonClickAsync for {clickedItem.DisplayName}.");
                    clickedItem.State = DownloadButtonState.Failed;
                    EndOperation();
                }
            }
            catch (Exception ex)
            {
                if (!_disposed)
                {
                    EndOperation();
                    _logErrors.LogAndForget(ex, "Error in DownloadImagePackButtonClickAsync.");
                }
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Critical error in DownloadImagePackButtonClickAsync.");
            EndOperation();
        }
    }

    private async Task HandleDownloadAndExtractComponentAsync(ImagePackDownloadItem item)
    {
        if (_disposed) return;

        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null)
        {
            EndOperation();
            item.State = DownloadButtonState.Failed;
            return;
        }

        var downloadUrl = item.DownloadUrl;
        var componentName = item.DisplayName;
        var easyModeExtractPath = item.ExtractPath;

        var destinationPath = PathHelper.ResolveRelativeToAppDirectory(easyModeExtractPath);

        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            if (_disposed) return;

            StatusMessage = $"{errorNodownloadUrLfor} {componentName}";
            EndOperation();
            item.State = DownloadButtonState.Failed;
            return;
        }

        if (string.IsNullOrEmpty(destinationPath))
        {
            var errorInvalidDestinationPath = (string)Application.Current.TryFindResource("ErrorInvalidDestinationPath") ?? "Error: Invalid destination path for";
            if (_disposed) return;

            StatusMessage = $"{errorInvalidDestinationPath} {componentName}";

            _logErrors.LogAndForget(null, $"[HandleDownloadAndExtractComponentAsync] Invalid destination path for {componentName}: {easyModeExtractPath}");
            EndOperation();
            item.State = DownloadButtonState.Failed;
            return;
        }

        try
        {
            var preparingtodownload = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            if (_disposed) return;

            StatusMessage = $"{preparingtodownload} {componentName}...";

            if (_disposed) return;

            ProgressPercentage = 0;
            IsStopEnabled = true;

            var success = false;

            var downloading = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            if (_disposed) return;

            StatusMessage = $"{downloading} {componentName}...";

            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (_disposed) return;

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                var extracting = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                if (_disposed) return;

                StatusMessage = $"{extracting} {componentName}...";

                LoadingMessage = $"{extracting} {componentName}...";
                IsLoading = true;
                await Task.Yield();

                success = await _downloadManager.ExtractFileAsync(downloadedFile, destinationPath);
                IsLoading = false;
                await Task.Yield();
            }

            if (_disposed) return;

            if (success)
            {
                var hasbeensuccessfullydownloadedandinstalled = (string)Application.Current.TryFindResource("hasbeensuccessfullydownloadedandinstalled") ?? "has been successfully downloaded and installed.";
                StatusMessage = $"{componentName} {hasbeensuccessfullydownloadedandinstalled}";

                MessageBoxLibrary.DownloadAndExtrationWereSuccessfulMessageBox();

                IsStopEnabled = false;
                EndOperation();
                item.State = DownloadButtonState.Downloaded;
            }
            else
            {
                if (_disposed) return;

                if (_downloadManager.IsUserCancellation)
                {
                    var downloadof = (string)Application.Current.TryFindResource("Downloadof") ?? "Download of";
                    var wascanceled = (string)Application.Current.TryFindResource("wascanceled") ?? "was canceled.";
                    StatusMessage = $"{downloadof} {componentName} {wascanceled}";
                    IsStopEnabled = false;
                    EndOperation();
                    item.State = DownloadButtonState.Failed;
                }
                else if (_downloadManager.IsDownloadCompleted)
                {
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    StatusMessage = $"{errorFailedtoextract} {componentName}.";
                    await MessageBoxLibrary.ShowExtractionFailedMessageBoxAsync(_downloadManager.TempFolder);
                    EndOperation();
                    item.State = DownloadButtonState.Failed;
                }
                else
                {
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    StatusMessage = $"{errorFailedtoextract} {componentName}.";

                    await MessageBoxLibrary.ShowImagePackDownloadErrorMessageBoxAsync(selectedSystem);
                    if (_disposed) return;

                    EndOperation();
                    item.State = DownloadButtonState.Failed;
                }

                IsStopEnabled = false;
            }
        }
        catch (Exception ex)
        {
            if (_disposed) return;

            var errorduring2 = (string)Application.Current.TryFindResource("Errorduring") ?? "Error during";
            var downloadprocess2 = (string)Application.Current.TryFindResource("downloadprocess") ?? "download process.";
            StatusMessage = $"{errorduring2} {componentName} {downloadprocess2}";

            if (!(ex is IOException ioEx && (ioEx.Message.Contains("Insufficient disk space") || ioEx.Message.Contains("Cannot check disk space"))))
            {
                var contextMessage = $"Error downloading {componentName}.\n" +
                                     $"URL: {downloadUrl}";
                _logErrors.LogAndForget(ex, contextMessage);
            }

            if (_downloadManager.IsDownloadCompleted)
            {
                await MessageBoxLibrary.ShowExtractionFailedMessageBoxAsync(_downloadManager.TempFolder);
                EndOperation();
            }
            else
            {
                await MessageBoxLibrary.ShowImagePackDownloadErrorMessageBoxAsync(selectedSystem);
                EndOperation();
            }

            if (_disposed) return;

            IsStopEnabled = false;
            item.State = DownloadButtonState.Failed;
        }

        EndOperation();
    }

    private EasyModeSystemConfig GetSelectedSystem()
    {
        return !string.IsNullOrEmpty(SelectedSystemName)
            ? _manager?.Systems?.FirstOrDefault(system => system.SystemName.Equals(SelectedSystemName, StringComparison.OrdinalIgnoreCase))
            : null;
    }

    private void DownloadManager_ProgressChanged(object sender, DownloadProgressEventArgs e)
    {
        Application.Current?.Dispatcher?.InvokeAsync(() =>
        {
            if (_disposed) return;

            ProgressPercentage = e.ProgressPercentage;
            StatusMessage = e.StatusMessage;
        });
    }

    [RelayCommand]
    private void StopDownload()
    {
        _playSoundEffects.PlayNotificationSound();

        if (_disposed) return;

        _downloadManager.CancelDownload();
        IsStopEnabled = false;
        ProgressPercentage = 0;

        var downloadcanceled2 = (string)Application.Current.TryFindResource("Downloadcanceled") ?? "Download canceled";
        StatusMessage = downloadcanceled2;

        foreach (var item in ImagePacksToDisplay)
        {
            if (_disposed) break;

            if (item.State == DownloadButtonState.Downloading)
            {
                item.State = DownloadButtonState.Failed;
            }
        }

        EndOperation();
    }

    [RelayCommand]
    private void HyperlinkNavigate(string url)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error opening the download link.");

            MessageBoxLibrary.CouldNotOpenTheDownloadLinkMessageBox();
        }
    }

    /// <summary>
    /// Forces release of the busy overlay and cancels any in-progress download.
    /// </summary>
    public void EmergencyOverlayRelease()
    {
        _playSoundEffects.PlayNotificationSound();

        _downloadManager?.CancelDownload();

        EndOperation();

        IsLoading = false;

        DebugLogger.Log("[Emergency] User forced overlay dismissal in DownloadImagePackWindow.");
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent("Emergency reset performed.", Application.Current.MainWindow as MainWindow);
    }

    /// <summary>
    /// Performs cleanup when the window is closing.
    /// </summary>
    public async Task CloseWindowRoutineAsync()
    {
        try
        {
            if (IsStopEnabled)
            {
                StopDownload();
                await Task.Delay(200);
            }

            _manager = null;
            Dispose();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error closing the Add System window.");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _downloadManager.DownloadProgressChanged -= DownloadManager_ProgressChanged;
            _downloadManager.Dispose();
        }

        _disposed = true;
    }

    ~DownloadImagePackViewModel()
    {
        Dispose(false);
    }
}

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
using SimpleLauncher.Services.UpdateStatusBar;
using Application = System.Windows.Application;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.ViewModels;

public partial class DownloadImagePackViewModel : ObservableObject, IDisposable
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly DownloadManager _downloadManager;
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

    public DownloadImagePackViewModel(PlaySoundEffects playSoundEffects)
    {
        _playSoundEffects = playSoundEffects;
        _downloadManager = App.ServiceProvider.GetRequiredService<DownloadManager>();
        _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;

        ImagePacksToDisplay = [];
        SystemNames = [];

        DownloadImagePackCommand = new RelayCommand<object>(ExecuteDownloadAsync, _ => !IsOperationInProgress);
    }

    public ObservableCollection<string> SystemNames { get; }
    public ObservableCollection<ImagePackDownloadItem> ImagePacksToDisplay { get; }

    public bool IsOperationInProgress
    {
        get => _isOperationInProgress;
        private set
        {
            if (SetProperty(ref _isOperationInProgress, value))
                OnPropertyChanged(nameof(IsMainContentEnabled));
        }
    }

    public bool IsStopEnabled
    {
        get => _isStopEnabled;
        private set => SetProperty(ref _isStopEnabled, value);
    }

    public double ProgressPercentage
    {
        get => _progressPercentage;
        private set => SetProperty(ref _progressPercentage, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
                OnPropertyChanged(nameof(IsMainContentEnabled));
        }
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        private set => SetProperty(ref _loadingMessage, value);
    }

    public bool IsSystemDropdownEnabled
    {
        get => _isSystemDropdownEnabled;
        private set => SetProperty(ref _isSystemDropdownEnabled, value);
    }

    private string _selectedSystemName;

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

    public bool IsMainContentEnabled => !IsOperationInProgress && !IsLoading;

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error populating system dropdown.");
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

                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error in DownloadImagePackButtonClickAsync for {clickedItem.DisplayName}.");
                    clickedItem.State = DownloadButtonState.Failed;
                    EndOperation();
                }
            }
            catch (Exception ex)
            {
                if (!_disposed)
                {
                    EndOperation();
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButtonClickAsync.");
                }
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider?.GetService<ILogErrors>()?.LogErrorAsync(ex, "Critical error in DownloadImagePackButtonClickAsync.");
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

            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[HandleDownloadAndExtractComponentAsync] Invalid destination path for {componentName}: {easyModeExtractPath}");
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error opening the download link.");

            MessageBoxLibrary.CouldNotOpenTheDownloadLinkMessageBox();
        }
    }

    public void EmergencyOverlayRelease()
    {
        _playSoundEffects.PlayNotificationSound();

        _downloadManager?.CancelDownload();

        EndOperation();

        IsLoading = false;

        DebugLogger.Log("[Emergency] User forced overlay dismissal in DownloadImagePackWindow.");
        UpdateStatusBar.UpdateContent("Emergency reset performed.", Application.Current.MainWindow as MainWindow);
    }

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error closing the Add System window.");
        }
    }

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

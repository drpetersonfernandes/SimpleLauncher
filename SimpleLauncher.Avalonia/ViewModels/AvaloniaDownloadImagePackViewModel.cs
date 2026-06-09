using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.DownloadService.Models;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Avalonia.Services;

namespace SimpleLauncher.Avalonia.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class AvaloniaDownloadImagePackViewModel : ObservableObject, IDisposable
{
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly SettingsManager _settings;
    private readonly AvaloniaDownloadManager _downloadManager;
    private bool _disposed;

    [ObservableProperty] private bool _isMainContentEnabled = true;
    [ObservableProperty] private bool _isSystemDropdownEnabled = true;
    [ObservableProperty] private bool _isStopEnabled;
    [ObservableProperty] private double _progressPercentage;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _loadingMessage = "Loading...";
    [ObservableProperty] private string? _selectedSystemName;

    public AvaloniaDownloadImagePackViewModel(
        ILogErrors logErrors,
        IDebugLogger debugLogger,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider,
        SettingsManager settings)
    {
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _settings = settings;
        _downloadManager = new AvaloniaDownloadManager();

        SystemNames = [];
        ImagePacksToDisplay = [];
    }

    public ObservableCollection<string> SystemNames { get; }
    public ObservableCollection<ImagePackDownloadItem> ImagePacksToDisplay { get; }

    public Task InitializeAsync()
    {
        IsLoading = false;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DownloadImagePackAsync(ImagePackDownloadItem? item)
    {
        if (item is not { CanStartDownload: true }) return;

        try
        {
            IsMainContentEnabled = false;
            IsStopEnabled = true;
            StatusMessage = $"Downloading {item.DisplayName}...";

            await _downloadManager.DownloadAndExtractImagePackAsync(item, progress =>
            {
                ProgressPercentage = progress;
            }, CancellationToken.None);

            StatusMessage = $"{item.DisplayName} downloaded successfully.";
            await _messageBox.SettingsSavedSuccessfullyMessageBox();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _logErrors.LogAndForget(ex, $"Error downloading image pack: {item.DisplayName}");
        }
        finally
        {
            IsMainContentEnabled = true;
            IsStopEnabled = false;
        }
    }

    [RelayCommand]
    private void StopDownload()
    {
        _downloadManager.Cancel();
        StatusMessage = "Download cancelled.";
        IsMainContentEnabled = true;
        IsStopEnabled = false;
    }

    [RelayCommand]
    private void HyperlinkNavigate(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error opening URL: {url}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _downloadManager.Dispose();
        GC.SuppressFinalize(this);
    }
}

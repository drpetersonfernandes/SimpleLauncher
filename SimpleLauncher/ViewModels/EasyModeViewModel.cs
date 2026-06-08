#nullable enable

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.DownloadService;
using SimpleLauncher.Services.EasyMode;
using SimpleLauncher.Services.PlaySound;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class EasyModeViewModel : ObservableObject, IDisposable
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly IDebugLogger _debugLogger;
    private readonly EasyModeManager _easyModeManager;
    private readonly IConfiguration _configuration;
    private readonly DownloadManager _downloadManager;

    private EasyModeManager? _manager;
    private bool _disposed;
    private int _operationInProgressFlag;

    // System selection
    [ObservableProperty] private ObservableCollection<string> _systemNames = [];

    [ObservableProperty] private int _selectedSystemIndex = -1;

    [ObservableProperty] private string _systemFolder = string.Empty;

    // Download states
    [ObservableProperty] private bool _isEmulatorDownloaded = true;

    [ObservableProperty] private bool _isCoreDownloaded = true;

    [ObservableProperty] private bool _isImagePack1Downloaded = true;

    [ObservableProperty] private bool _isImagePack2Downloaded = true;

    [ObservableProperty] private bool _isImagePack3Downloaded = true;

    [ObservableProperty] private bool _isImagePack4Downloaded = true;

    [ObservableProperty] private bool _isImagePack5Downloaded = true;

    // Image pack availability
    [ObservableProperty] private bool _isImagePack1Available;

    [ObservableProperty] private bool _isImagePack2Available;

    [ObservableProperty] private bool _isImagePack3Available;

    [ObservableProperty] private bool _isImagePack4Available;

    [ObservableProperty] private bool _isImagePack5Available;

    // Operation state
    [ObservableProperty] private bool _isOperationInProgress;

    [ObservableProperty] private bool _isAddSystemEnabled;

    // Loading state
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = string.Empty;

    // Download progress
    [ObservableProperty] private string _downloadStatus = string.Empty;

    // Download states tracking
    private readonly Dictionary<string, DownloadButtonState> _downloadStates = new();

    public EasyModeViewModel(
        PlaySoundEffects playSoundEffects,
        IConfiguration configuration,
        ILogErrors logErrors,
        DownloadManager downloadManager,
        EasyModeManager easyModeManager,
        IDebugLogger debugLogger,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider)
    {
        _playSoundEffects = playSoundEffects;
        _configuration = configuration;
        _logErrors = logErrors;
        _easyModeManager = easyModeManager;
        _debugLogger = debugLogger;
        _downloadManager = downloadManager;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;

        _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        LoadingMessage = _resourceProvider.GetString("Loadingconfiguration", "Loading configuration...");

        await Task.Yield();

        _manager = await _easyModeManager.LoadAsync();

        IsLoading = false;

        if (_manager is not { Systems.Count: > 0 })
        {
            await _messageBox.EasyModeUnavailableMessageBox();
            return;
        }

        PopulateSystemDropdown();
    }

    private void PopulateSystemDropdown()
    {
        try
        {
            if (_manager?.Systems == null)
            {
                SystemNames = [];
                return;
            }

            var sortedSystemNames = _manager.Systems
                .Where(static system => !string.IsNullOrEmpty(system.Emulators?.Emulator?.EmulatorDownloadLink))
                .Select(static system => system.SystemName)
                .OrderBy(static name => name)
                .ToList();

            SystemNames = new ObservableCollection<string>(sortedSystemNames);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error populating system dropdown.");
            SystemNames = [];
        }
    }

    public void OnSystemSelectionChanged(string? selectedSystemName)
    {
        if (string.IsNullOrEmpty(selectedSystemName) || _manager?.Systems == null)
        {
            ResetDownloadStates();
            SystemFolder = string.Empty;
            return;
        }

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));

        if (selectedSystem == null) return;

        var emulator = selectedSystem.Emulators?.Emulator;

        // Determine image pack availability
        IsImagePack1Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);
        IsImagePack2Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink2) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);
        IsImagePack3Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink3) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);
        IsImagePack4Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink4) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);
        IsImagePack5Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink5) && !string.IsNullOrEmpty(emulator.ImagePackDownloadExtractPath);

        // Check emulator download state
        var emulatorLocation = selectedSystem.Emulators?.Emulator?.EmulatorLocation;
        if (!string.IsNullOrEmpty(emulatorLocation))
        {
            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(emulatorLocation);
            SetDownloadState(EasyModeManager.DownloadType.Emulator,
                File.Exists(resolvedPath) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        }
        else
        {
            SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Idle);
        }

        // Check core download state
        var coreLocation = selectedSystem.Emulators?.Emulator?.CoreLocation;
        var coreDownloadLink = selectedSystem.Emulators?.Emulator?.CoreDownloadLink;
        if (!string.IsNullOrEmpty(coreLocation))
        {
            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(coreLocation);
            SetDownloadState(EasyModeManager.DownloadType.Core,
                File.Exists(resolvedPath) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        }
        else
        {
            SetDownloadState(EasyModeManager.DownloadType.Core,
                string.IsNullOrEmpty(coreDownloadLink) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        }

        // Reset image pack states
        SetDownloadState(EasyModeManager.DownloadType.ImagePack1,
            string.IsNullOrEmpty(emulator?.ImagePackDownloadLink) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack2,
            string.IsNullOrEmpty(emulator?.ImagePackDownloadLink2) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack3,
            string.IsNullOrEmpty(emulator?.ImagePackDownloadLink3) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack4,
            string.IsNullOrEmpty(emulator?.ImagePackDownloadLink4) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack5,
            string.IsNullOrEmpty(emulator?.ImagePackDownloadLink5) ? DownloadButtonState.Downloaded : DownloadButtonState.Idle);

        SystemFolder = PathHelper.ResolveRelativeToAppDirectory(selectedSystem.SystemFolder) ?? string.Empty;
        UpdateAddSystemButtonState();
    }

    [RelayCommand]
    private async Task DownloadEmulatorAsync()
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed || !TryStartOperation()) return;

        try
        {
            SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Downloading);
            await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.Emulator);
        }
        catch (Exception ex)
        {
            SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Failed);
            if (!_disposed) _logErrors.LogAndForget(ex, "Error downloading emulator.");
        }
        finally
        {
            if (!_disposed && GetDownloadState(EasyModeManager.DownloadType.Emulator) == DownloadButtonState.Downloading)
                SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Failed);
        }
    }

    [RelayCommand]
    private async Task DownloadCoreAsync()
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed || !TryStartOperation()) return;

        try
        {
            SetDownloadState(EasyModeManager.DownloadType.Core, DownloadButtonState.Downloading);
            await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.Core);
        }
        catch (Exception ex)
        {
            SetDownloadState(EasyModeManager.DownloadType.Core, DownloadButtonState.Failed);
            if (!_disposed) _logErrors.LogAndForget(ex, "Error downloading core.");
        }
        finally
        {
            if (!_disposed && GetDownloadState(EasyModeManager.DownloadType.Core) == DownloadButtonState.Downloading)
                SetDownloadState(EasyModeManager.DownloadType.Core, DownloadButtonState.Failed);
        }
    }

    [RelayCommand]
    private async Task DownloadImagePack1Async()
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed || !TryStartOperation()) return;

        try
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack1, DownloadButtonState.Downloading);
            await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack1);
        }
        catch (Exception ex)
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack1, DownloadButtonState.Failed);
            if (!_disposed) _logErrors.LogAndForget(ex, "Error downloading image pack 1.");
        }
        finally
        {
            if (!_disposed && GetDownloadState(EasyModeManager.DownloadType.ImagePack1) == DownloadButtonState.Downloading)
                SetDownloadState(EasyModeManager.DownloadType.ImagePack1, DownloadButtonState.Failed);
        }
    }

    [RelayCommand]
    private async Task DownloadImagePack2Async()
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed || !TryStartOperation()) return;

        try
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack2, DownloadButtonState.Downloading);
            await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack2);
        }
        catch (Exception ex)
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack2, DownloadButtonState.Failed);
            if (!_disposed) _logErrors.LogAndForget(ex, "Error downloading image pack 2.");
        }
        finally
        {
            if (!_disposed && GetDownloadState(EasyModeManager.DownloadType.ImagePack2) == DownloadButtonState.Downloading)
                SetDownloadState(EasyModeManager.DownloadType.ImagePack2, DownloadButtonState.Failed);
        }
    }

    [RelayCommand]
    private async Task DownloadImagePack3Async()
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed || !TryStartOperation()) return;

        try
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack3, DownloadButtonState.Downloading);
            await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack3);
        }
        catch (Exception ex)
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack3, DownloadButtonState.Failed);
            if (!_disposed) _logErrors.LogAndForget(ex, "Error downloading image pack 3.");
        }
        finally
        {
            if (!_disposed && GetDownloadState(EasyModeManager.DownloadType.ImagePack3) == DownloadButtonState.Downloading)
                SetDownloadState(EasyModeManager.DownloadType.ImagePack3, DownloadButtonState.Failed);
        }
    }

    [RelayCommand]
    private async Task DownloadImagePack4Async()
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed || !TryStartOperation()) return;

        try
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack4, DownloadButtonState.Downloading);
            await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack4);
        }
        catch (Exception ex)
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack4, DownloadButtonState.Failed);
            if (!_disposed) _logErrors.LogAndForget(ex, "Error downloading image pack 4.");
        }
        finally
        {
            if (!_disposed && GetDownloadState(EasyModeManager.DownloadType.ImagePack4) == DownloadButtonState.Downloading)
                SetDownloadState(EasyModeManager.DownloadType.ImagePack4, DownloadButtonState.Failed);
        }
    }

    [RelayCommand]
    private async Task DownloadImagePack5Async()
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed || !TryStartOperation()) return;

        try
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack5, DownloadButtonState.Downloading);
            await HandleDownloadAndExtractComponentAsync(EasyModeManager.DownloadType.ImagePack5);
        }
        catch (Exception ex)
        {
            SetDownloadState(EasyModeManager.DownloadType.ImagePack5, DownloadButtonState.Failed);
            if (!_disposed) _logErrors.LogAndForget(ex, "Error downloading image pack 5.");
        }
        finally
        {
            if (!_disposed && GetDownloadState(EasyModeManager.DownloadType.ImagePack5) == DownloadButtonState.Downloading)
                SetDownloadState(EasyModeManager.DownloadType.ImagePack5, DownloadButtonState.Failed);
        }
    }

    [RelayCommand]
    private async Task AddSystemAsync()
    {
        _playSoundEffects.PlayNotificationSound();
        if (_disposed || !TryStartOperation()) return;

        try
        {
            IsLoading = true;
            LoadingMessage = _resourceProvider.GetString("AddingSystem", "Adding system...");

            // Implementation depends on EasyModeManager.AddSystemAsync
            // This would need to be called from the code-behind with the selected system
        }
        catch (Exception ex)
        {
            if (!_disposed) _logErrors.LogAndForget(ex, "Error adding system.");
        }
        finally
        {
            IsLoading = false;
            EndOperation();
        }
    }

    // ReSharper disable once UnusedParameter.Local
    private static Task HandleDownloadAndExtractComponentAsync(string downloadType)
    {
        // This method needs to be implemented based on the existing logic in EasyModeWindow
        // The actual download and extraction logic would be called here
        return Task.CompletedTask;
    }

    private void DownloadManager_ProgressChanged(object? sender, DownloadProgressEventArgs e)
    {
        DownloadStatus = $"{e.ProgressPercentage}%";
    }

    private DownloadButtonState GetDownloadState(string type)
    {
        return _downloadStates.GetValueOrDefault(type, DownloadButtonState.Idle);
    }

    private void SetDownloadState(string type, DownloadButtonState state)
    {
        _downloadStates[type] = state;
        switch (type)
        {
            case EasyModeManager.DownloadType.Emulator:
                IsEmulatorDownloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.Core:
                IsCoreDownloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack1:
                IsImagePack1Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack2:
                IsImagePack2Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack3:
                IsImagePack3Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack4:
                IsImagePack4Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
            case EasyModeManager.DownloadType.ImagePack5:
                IsImagePack5Downloaded = state is DownloadButtonState.Downloaded or DownloadButtonState.Downloading;
                break;
        }
    }

    private void ResetDownloadStates()
    {
        SetDownloadState(EasyModeManager.DownloadType.Emulator, DownloadButtonState.Downloaded);
        SetDownloadState(EasyModeManager.DownloadType.Core, DownloadButtonState.Downloaded);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack1, DownloadButtonState.Downloaded);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack2, DownloadButtonState.Downloaded);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack3, DownloadButtonState.Downloaded);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack4, DownloadButtonState.Downloaded);
        SetDownloadState(EasyModeManager.DownloadType.ImagePack5, DownloadButtonState.Downloaded);
        IsImagePack1Available = false;
        IsImagePack2Available = false;
        IsImagePack3Available = false;
        IsImagePack4Available = false;
        IsImagePack5Available = false;
    }

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

    private void UpdateAddSystemButtonState()
    {
        IsAddSystemEnabled = !IsOperationInProgress && (IsEmulatorDownloaded || IsCoreDownloaded);
    }

    public void Dispose()
    {
        _disposed = true;
        _downloadManager.DownloadProgressChanged -= DownloadManager_ProgressChanged;
        GC.SuppressFinalize(this);
    }
}
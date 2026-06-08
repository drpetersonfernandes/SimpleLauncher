using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the BlastEm emulator configuration injection window.
/// </summary>
public partial class InjectBlastemConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _scanlines;
    [ObservableProperty] private string _aspect;
    [ObservableProperty] private string _scaling;
    [ObservableProperty] private string _audioRate;
    [ObservableProperty] private string _syncSource;
    [ObservableProperty] private bool _showBeforeLaunch;

    public InjectBlastemConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        _settings = settings;
        _logErrors = logErrors;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the BlastEm emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available aspect ratio options for BlastEm.
    /// </summary>
    public List<string> AspectOptions { get; } = ["4:3", "16:9", "stretch"];

    /// <summary>
    /// Available scaling options for BlastEm.
    /// </summary>
    public List<string> ScalingOptions { get; } = ["linear", "nearest"];

    /// <summary>
    /// Available audio sample rate options for BlastEm.
    /// </summary>
    public List<string> AudioRateOptions { get; } = ["48000", "44100", "22050"];

    /// <summary>
    /// Available sync source options for BlastEm.
    /// </summary>
    public List<string> SyncSourceOptions { get; } = ["audio", "video"];

    /// <summary>
    /// Gets whether the configuration is being injected from launcher mode.
    /// </summary>
    public bool IsLauncherMode { get; private set; }

    /// <summary>
    /// Gets whether the emulator should be launched after configuration injection.
    /// </summary>
    public bool ShouldRun { get; private set; }

    /// <summary>
    /// Raised when the window should be closed.
    /// </summary>
    public event Action CloseRequested;

    /// <summary>
    /// Requests the user to provide the emulator executable path.
    /// </summary>
    public event Func<string> RequestEmulatorPath;

    /// <summary>
    /// Gets the owner window for dialog display.
    /// </summary>
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        Fullscreen = _settings.Blastem.Fullscreen;
        Vsync = _settings.Blastem.Vsync;
        Scanlines = _settings.Blastem.Scanlines;
        Aspect = _settings.Blastem.Aspect;
        Scaling = _settings.Blastem.Scaling;
        AudioRate = _settings.Blastem.AudioRate.ToString(CultureInfo.InvariantCulture);
        SyncSource = _settings.Blastem.SyncSource;
        ShowBeforeLaunch = _settings.Blastem.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Blastem.Fullscreen = Fullscreen;
        _settings.Blastem.Vsync = Vsync;
        _settings.Blastem.Scanlines = Scanlines;
        _settings.Blastem.Aspect = Aspect;
        _settings.Blastem.Scaling = Scaling;
        _settings.Blastem.AudioRate = int.Parse(AudioRate, CultureInfo.InvariantCulture);
        _settings.Blastem.SyncSource = SyncSource;
        _settings.Blastem.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Blastem", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.BlastemEmulatorNotFoundMessageBox();

        var result = RequestEmulatorPath?.Invoke();
        if (string.IsNullOrEmpty(result)) return null;

        _emulatorPath = result;
        return _emulatorPath;
    }

    private async Task<bool> InjectConfig()
    {
        var path = await EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            BlastemConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (FileNotFoundException ex)
        {
            var errorMsg = $"Configuration file not found for Blastem at: {path}. Details: {ex.Message}";
            _logErrors.LogAndForget(ex, errorMsg);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            var errorMsg = $"Permission denied accessing Blastem configuration at: {path}. Details: {ex.Message}";
            _logErrors.LogAndForget(ex, errorMsg);
            return false;
        }
        catch (IOException ex)
        {
            var errorMsg = $"I/O error while accessing Blastem configuration at: {path}. Details: {ex.Message}";
            _logErrors.LogAndForget(ex, errorMsg);
            return false;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Blastem configuration injection failed for path: {path}. Details: {ex.Message}";
            _logErrors.LogAndForget(ex, errorMsg);
            return false;
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfig())
            {
                ShouldRun = true;
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBox();
                CloseRequested?.Invoke();
                ShouldRun = true;
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectBlastemConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
            ShouldRun = true;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfig())
            {
                await _messageBox.BlastemConfigurationSavedSuccessfullyMessageBox();
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBox();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectBlastemConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

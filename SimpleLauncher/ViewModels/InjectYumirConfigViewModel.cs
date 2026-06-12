using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Yumir emulator configuration injection window.
/// </summary>
public partial class InjectYumirConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private bool _yumirFullscreen;
    [ObservableProperty] private bool _yumirForceAspectRatio;
    [ObservableProperty] private bool _yumirReduceLatency;
    [ObservableProperty] private bool _yumirMute;
    [ObservableProperty] private double _yumirVolume;
    [ObservableProperty] private bool _yumirAutoDetectRegion;
    [ObservableProperty] private string _yumirVideoStandard;
    [ObservableProperty] private bool _yumirPauseWhenUnfocused;
    [ObservableProperty] private double _yumirForcedAspect;
    [ObservableProperty] private bool _yumirShowSettingsBeforeLaunch;

    public InjectYumirConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Yumir emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available video standard options for Yumir.
    /// </summary>
    public List<string> VideoStandardOptions { get; } = ["PAL", "NTSC"];

    /// <summary>
    /// Available forced aspect ratio display options for Yumir.
    /// </summary>
    public List<string> ForcedAspectOptions { get; } = ["16:9", "4:3"];

    /// <summary>
    /// Tags corresponding to the forced aspect ratio options for Yumir.
    /// </summary>
    public List<string> ForcedAspectTags { get; } = ["1.7777777777777777", "1.3333333333333333"];

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

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke();

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
        YumirFullscreen = _settings.Yumir.Fullscreen;
        YumirForceAspectRatio = _settings.Yumir.ForceAspectRatio;
        YumirReduceLatency = _settings.Yumir.ReduceLatency;
        YumirMute = _settings.Yumir.Mute;
        YumirVolume = _settings.Yumir.Volume;
        YumirAutoDetectRegion = _settings.Yumir.AutoDetectRegion;
        YumirVideoStandard = _settings.Yumir.VideoStandard;
        YumirPauseWhenUnfocused = _settings.Yumir.PauseWhenUnfocused;
        YumirForcedAspect = _settings.Yumir.ForcedAspect;
        YumirShowSettingsBeforeLaunch = _settings.Yumir.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Yumir.Fullscreen = YumirFullscreen;
        _settings.Yumir.ForceAspectRatio = YumirForceAspectRatio;
        _settings.Yumir.ReduceLatency = YumirReduceLatency;
        _settings.Yumir.Mute = YumirMute;
        _settings.Yumir.Volume = YumirVolume;
        _settings.Yumir.AutoDetectRegion = YumirAutoDetectRegion;
        _settings.Yumir.VideoStandard = YumirVideoStandard;
        _settings.Yumir.PauseWhenUnfocused = YumirPauseWhenUnfocused;
        _settings.Yumir.ForcedAspect = YumirForcedAspect;
        _settings.Yumir.ShowSettingsBeforeLaunch = YumirShowSettingsBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Yumir", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.YumirEmulatorNotFoundMessageBoxAsync();

        var result = RequestEmulatorPath?.Invoke();
        if (string.IsNullOrEmpty(result)) return null;

        _emulatorPath = result;
        return _emulatorPath;
    }

    private async Task<bool> InjectConfigAsync()
    {
        var path = await EnsureEmulatorPathAsync();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            YumirConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"Yumir configuration injection failed for path: {path}");
            return false;
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfigAsync())
            {
                ShouldRun = true;
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectYumirConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfigAsync())
            {
                await _messageBox.YumirConfigurationSavedSuccessfullyMessageBoxAsync();
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectYumirConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

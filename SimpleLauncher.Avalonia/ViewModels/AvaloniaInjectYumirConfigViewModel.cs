using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectYumirConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private bool _yumirFullscreen;
    [ObservableProperty] private bool _yumirForceAspectRatio;
    [ObservableProperty] private bool _yumirReduceLatency;
    [ObservableProperty] private bool _yumirMute;
    [ObservableProperty] private double _yumirVolume = 1.0;
    [ObservableProperty] private bool _yumirAutoDetectRegion = true;
    [ObservableProperty] private string _yumirVideoStandard = "NTSC";
    [ObservableProperty] private bool _yumirPauseWhenUnfocused;
    [ObservableProperty] private double _yumirForcedAspect = 1.7777777777777777;
    [ObservableProperty] private bool _yumirShowSettingsBeforeLaunch;

    public AvaloniaInjectYumirConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    public void Initialize(string? emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    public List<string> VideoStandardOptions { get; } = ["PAL", "NTSC"];
    public List<string> ForcedAspectOptions { get; } = ["16:9", "4:3"];
    public List<string> ForcedAspectTags { get; } = ["1.7777777777777777", "1.3333333333333333"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

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
        _settings.SaveAsync();
    }

    private string? EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

        var path = RequestEmulatorPath?.Invoke();
        if (!string.IsNullOrEmpty(path))
        {
            _emulatorPath = path;
        }

        return _emulatorPath;
    }

    private async Task InjectConfigAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_emulatorPath)) return;

            YumirConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox(ex.Message, "Error");
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return;

        await InjectConfigAsync();
        ShouldRun = true;
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return;

        await InjectConfigAsync();
        await _messageBox.SettingsSavedSuccessfullyMessageBox();
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}

using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectBlastemConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _scanlines;
    [ObservableProperty] private string _aspect = "4:3";
    [ObservableProperty] private string _scaling = "linear";
    [ObservableProperty] private string _audioRate = "48000";
    [ObservableProperty] private string _syncSource = "audio";
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectBlastemConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> AspectOptions { get; } = ["4:3", "16:9", "stretch"];
    public List<string> ScalingOptions { get; } = ["linear", "nearest"];
    public List<string> AudioRateOptions { get; } = ["48000", "44100", "22050"];
    public List<string> SyncSourceOptions { get; } = ["audio", "video"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

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

    private async Task<string?> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

        await _messageBox.BlastemEmulatorNotFoundMessageBox();
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

            BlastemConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.FailedToInjectBlastemConfigurationMessageBox();
            _logErrors.LogAndForget(ex, "Error in method InjectConfigAsync");
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        var path = await EnsureEmulatorPathAsync();
        if (string.IsNullOrEmpty(path)) return;

        await InjectConfigAsync();
        ShouldRun = true;
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        var path = await EnsureEmulatorPathAsync();
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectMesenConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private string _aspectRatio = "NoStretching";
    [ObservableProperty] private bool _bilinear;
    [ObservableProperty] private string _videoFilter = "None";
    [ObservableProperty] private bool _enableAudio = true;
    [ObservableProperty] private int _masterVolume = 100;
    [ObservableProperty] private bool _rewind;
    [ObservableProperty] private int _runAhead;
    [ObservableProperty] private bool _pauseInBackground;
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectMesenConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> AspectRatioOptions { get; } = ["NoStretching", "4:3", "16:9", "Auto"];
    public List<string> VideoFilterOptions { get; } = ["None", "NTSC", "CRT", "LCD"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        Fullscreen = _settings.Mesen.Fullscreen;
        Vsync = _settings.Mesen.Vsync;
        AspectRatio = _settings.Mesen.AspectRatio;
        Bilinear = _settings.Mesen.Bilinear;
        VideoFilter = _settings.Mesen.VideoFilter;
        EnableAudio = _settings.Mesen.EnableAudio;
        MasterVolume = _settings.Mesen.MasterVolume;
        Rewind = _settings.Mesen.Rewind;
        RunAhead = _settings.Mesen.RunAhead;
        PauseInBackground = _settings.Mesen.PauseInBackground;
        ShowBeforeLaunch = _settings.Mesen.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Mesen.Fullscreen = Fullscreen;
        _settings.Mesen.Vsync = Vsync;
        _settings.Mesen.AspectRatio = AspectRatio;
        _settings.Mesen.Bilinear = Bilinear;
        _settings.Mesen.VideoFilter = VideoFilter;
        _settings.Mesen.EnableAudio = EnableAudio;
        _settings.Mesen.MasterVolume = MasterVolume;
        _settings.Mesen.Rewind = Rewind;
        _settings.Mesen.RunAhead = RunAhead;
        _settings.Mesen.PauseInBackground = PauseInBackground;
        _settings.Mesen.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
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

            MesenConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox("Emulator not found. Please locate the emulator executable.", "Emulator Not Found");
            _logErrors.LogAndForget(ex, "Error in method InjectConfigAsync");
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

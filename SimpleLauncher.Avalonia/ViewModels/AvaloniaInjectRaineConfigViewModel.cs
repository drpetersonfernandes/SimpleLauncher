using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectRaineConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private bool _raineFullscreen;
    [ObservableProperty] private bool _raineFixAspectRatio;
    [ObservableProperty] private bool _raineVsync;
    [ObservableProperty] private int _raineResX = 640;
    [ObservableProperty] private int _raineResY = 480;
    [ObservableProperty] private string _raineSoundDriver = "directsound";
    [ObservableProperty] private int _raineSampleRate = 44100;
    [ObservableProperty] private bool _raineShowSettingsBeforeLaunch;
    [ObservableProperty] private bool _raineShowFps;
    [ObservableProperty] private int _raineFrameSkip;
    [ObservableProperty] private string _raineNeoCdBios = "";
    [ObservableProperty] private int _raineMusicVolume = 100;
    [ObservableProperty] private int _raineSfxVolume = 100;
    [ObservableProperty] private bool _raineMuteSfx;
    [ObservableProperty] private bool _raineMuteMusic;
    [ObservableProperty] private string _raineRomDirectory = "";

    public AvaloniaInjectRaineConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> SoundDriverOptions { get; } = ["directsound", "sdl"];
    public List<string> SampleRateOptions { get; } = ["22050", "44100", "48000"];
    public List<string> FrameSkipOptions { get; } = ["0", "1", "2", "3", "4", "5"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        RaineFullscreen = _settings.Raine.Fullscreen;
        RaineFixAspectRatio = _settings.Raine.FixAspectRatio;
        RaineVsync = _settings.Raine.Vsync;
        RaineResX = _settings.Raine.ResX;
        RaineResY = _settings.Raine.ResY;
        RaineSoundDriver = _settings.Raine.SoundDriver;
        RaineSampleRate = _settings.Raine.SampleRate;
        RaineShowSettingsBeforeLaunch = _settings.Raine.ShowSettingsBeforeLaunch;
        RaineShowFps = _settings.Raine.ShowFps;
        RaineFrameSkip = _settings.Raine.FrameSkip;
        RaineNeoCdBios = _settings.Raine.NeoCdBios;
        RaineMusicVolume = _settings.Raine.MusicVolume;
        RaineSfxVolume = _settings.Raine.SfxVolume;
        RaineMuteSfx = _settings.Raine.MuteSfx;
        RaineMuteMusic = _settings.Raine.MuteMusic;
        RaineRomDirectory = _settings.Raine.RomDirectory;
    }

    private void SaveSettings()
    {
        _settings.Raine.Fullscreen = RaineFullscreen;
        _settings.Raine.FixAspectRatio = RaineFixAspectRatio;
        _settings.Raine.Vsync = RaineVsync;
        _settings.Raine.ResX = RaineResX;
        _settings.Raine.ResY = RaineResY;
        _settings.Raine.SoundDriver = RaineSoundDriver;
        _settings.Raine.SampleRate = RaineSampleRate;
        _settings.Raine.ShowSettingsBeforeLaunch = RaineShowSettingsBeforeLaunch;
        _settings.Raine.ShowFps = RaineShowFps;
        _settings.Raine.FrameSkip = RaineFrameSkip;
        _settings.Raine.NeoCdBios = RaineNeoCdBios;
        _settings.Raine.MusicVolume = RaineMusicVolume;
        _settings.Raine.SfxVolume = RaineSfxVolume;
        _settings.Raine.MuteSfx = RaineMuteSfx;
        _settings.Raine.MuteMusic = RaineMuteMusic;
        _settings.Raine.RomDirectory = RaineRomDirectory;
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

            RaineConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox($"Error injecting Raine configuration: {ex.Message}", "Configuration Error");
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

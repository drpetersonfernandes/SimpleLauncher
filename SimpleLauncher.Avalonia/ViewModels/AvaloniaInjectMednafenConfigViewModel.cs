using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectMednafenConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _mednafenVideoDriver = "opengl";
    [ObservableProperty] private string _mednafenStretch = "aspect";
    [ObservableProperty] private string _mednafenShader = "none";
    [ObservableProperty] private string _mednafenSpecial = "none";
    [ObservableProperty] private bool _mednafenFullscreen;
    [ObservableProperty] private bool _mednafenVsync;
    [ObservableProperty] private bool _mednafenBilinear;
    [ObservableProperty] private int _mednafenScanlines;
    [ObservableProperty] private int _mednafenVolume = 100;
    [ObservableProperty] private bool _mednafenCheats;
    [ObservableProperty] private bool _mednafenRewind;
    [ObservableProperty] private bool _mednafenShowSettingsBeforeLaunch;

    public AvaloniaInjectMednafenConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> VideoDriverOptions { get; } = ["opengl", "soft", "default"];
    public List<string> StretchOptions { get; } = ["0", "full", "aspect", "aspect_int"];
    public List<string> ShaderOptions { get; } = ["none", "ip", "ipsharper", "scale2x", "snes_ntsc", "goat"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        MednafenVideoDriver = _settings.Mednafen.VideoDriver;
        MednafenStretch = _settings.Mednafen.Stretch;
        MednafenFullscreen = _settings.Mednafen.Fullscreen;
        MednafenVsync = _settings.Mednafen.Vsync;
        MednafenBilinear = _settings.Mednafen.Bilinear;
        MednafenScanlines = _settings.Mednafen.Scanlines;
        MednafenVolume = _settings.Mednafen.Volume;
        MednafenCheats = _settings.Mednafen.Cheats;
        MednafenRewind = _settings.Mednafen.Rewind;
        MednafenShowSettingsBeforeLaunch = _settings.Mednafen.ShowSettingsBeforeLaunch;

        if (_settings.Mednafen.Special != "none")
        {
            MednafenShader = _settings.Mednafen.Special;
        }
        else
        {
            MednafenShader = _settings.Mednafen.Shader;
        }

        MednafenSpecial = _settings.Mednafen.Special;
    }

    private void SaveSettings()
    {
        _settings.Mednafen.VideoDriver = MednafenVideoDriver;
        _settings.Mednafen.Stretch = MednafenStretch;
        _settings.Mednafen.Fullscreen = MednafenFullscreen;
        _settings.Mednafen.Vsync = MednafenVsync;
        _settings.Mednafen.Bilinear = MednafenBilinear;
        _settings.Mednafen.Scanlines = MednafenScanlines;
        _settings.Mednafen.Volume = MednafenVolume;
        _settings.Mednafen.Cheats = MednafenCheats;
        _settings.Mednafen.Rewind = MednafenRewind;
        _settings.Mednafen.ShowSettingsBeforeLaunch = MednafenShowSettingsBeforeLaunch;

        if (MednafenShader is "scale2x" or "snes_ntsc")
        {
            _settings.Mednafen.Special = MednafenShader;
            _settings.Mednafen.Shader = "none";
        }
        else
        {
            _settings.Mednafen.Special = "none";
            _settings.Mednafen.Shader = MednafenShader;
        }

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

            MednafenConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox($"Error injecting Mednafen configuration: {ex.Message}", "Configuration Error");
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

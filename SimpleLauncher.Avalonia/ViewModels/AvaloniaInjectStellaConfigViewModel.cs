using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectStellaConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _correctAspect = true;
    [ObservableProperty] private string _videoDriver = "direct3d";
    [ObservableProperty] private string _tvFilter = "0";
    [ObservableProperty] private int _scanlines;
    [ObservableProperty] private bool _audioEnabled = true;
    [ObservableProperty] private int _audioVolume = 100;
    [ObservableProperty] private bool _timeMachine;
    [ObservableProperty] private bool _confirmExit;
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectStellaConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> VideoDriverOptions { get; } = ["direct3d", "opengl", "software"];
    public List<string> TvFilterOptions { get; } = ["0", "1", "2", "3"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        Fullscreen = _settings.Stella.Fullscreen;
        Vsync = _settings.Stella.Vsync;
        CorrectAspect = _settings.Stella.CorrectAspect;
        VideoDriver = _settings.Stella.VideoDriver;
        TvFilter = _settings.Stella.TvFilter.ToString(CultureInfo.InvariantCulture);
        Scanlines = _settings.Stella.Scanlines;
        AudioEnabled = _settings.Stella.AudioEnabled;
        AudioVolume = _settings.Stella.AudioVolume;
        TimeMachine = _settings.Stella.TimeMachine;
        ConfirmExit = _settings.Stella.ConfirmExit;
        ShowBeforeLaunch = _settings.Stella.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Stella.Fullscreen = Fullscreen;
        _settings.Stella.Vsync = Vsync;
        _settings.Stella.CorrectAspect = CorrectAspect;
        _settings.Stella.VideoDriver = VideoDriver;
        _settings.Stella.TvFilter = int.Parse(TvFilter, CultureInfo.InvariantCulture);
        _settings.Stella.Scanlines = Scanlines;
        _settings.Stella.AudioEnabled = AudioEnabled;
        _settings.Stella.AudioVolume = AudioVolume;
        _settings.Stella.TimeMachine = TimeMachine;
        _settings.Stella.ConfirmExit = ConfirmExit;
        _settings.Stella.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
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

            StellaConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
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

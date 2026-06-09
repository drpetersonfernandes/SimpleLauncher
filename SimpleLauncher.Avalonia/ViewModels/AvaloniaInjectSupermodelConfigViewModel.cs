using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectSupermodelConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private bool _new3DEngine;
    [ObservableProperty] private bool _quadRendering;
    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _wideScreen;
    [ObservableProperty] private bool _stretch;
    [ObservableProperty] private int _resX = 640;
    [ObservableProperty] private int _resY = 480;
    [ObservableProperty] private int _musicVolume = 100;
    [ObservableProperty] private int _soundVolume = 100;
    [ObservableProperty] private bool _throttle = true;
    [ObservableProperty] private bool _multiThreaded;
    [ObservableProperty] private string _inputSystem = "xinput";
    [ObservableProperty] private string _powerPcFrequency = "100";
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectSupermodelConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> InputSystemOptions { get; } = ["xinput", "dinput", "rawinput"];
    public List<string> PpcFrequencyOptions { get; } = ["50", "60", "75", "100"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        New3DEngine = _settings.Supermodel.New3DEngine;
        QuadRendering = _settings.Supermodel.QuadRendering;
        Fullscreen = _settings.Supermodel.Fullscreen;
        Vsync = _settings.Supermodel.Vsync;
        WideScreen = _settings.Supermodel.WideScreen;
        Stretch = _settings.Supermodel.Stretch;
        ResX = _settings.Supermodel.ResX;
        ResY = _settings.Supermodel.ResY;
        MusicVolume = _settings.Supermodel.MusicVolume;
        SoundVolume = _settings.Supermodel.SoundVolume;
        Throttle = _settings.Supermodel.Throttle;
        MultiThreaded = _settings.Supermodel.MultiThreaded;
        InputSystem = _settings.Supermodel.InputSystem;
        PowerPcFrequency = _settings.Supermodel.PowerPcFrequency.ToString(CultureInfo.InvariantCulture);
        ShowBeforeLaunch = _settings.Supermodel.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Supermodel.New3DEngine = New3DEngine;
        _settings.Supermodel.QuadRendering = QuadRendering;
        _settings.Supermodel.Fullscreen = Fullscreen;
        _settings.Supermodel.Vsync = Vsync;
        _settings.Supermodel.WideScreen = WideScreen;
        _settings.Supermodel.Stretch = Stretch;
        _settings.Supermodel.ResX = ResX;
        _settings.Supermodel.ResY = ResY;
        _settings.Supermodel.MusicVolume = MusicVolume;
        _settings.Supermodel.SoundVolume = SoundVolume;
        _settings.Supermodel.Throttle = Throttle;
        _settings.Supermodel.MultiThreaded = MultiThreaded;
        _settings.Supermodel.InputSystem = InputSystem;
        _settings.Supermodel.PowerPcFrequency = int.Parse(PowerPcFrequency, CultureInfo.InvariantCulture);
        _settings.Supermodel.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
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

            SupermodelConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
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

using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

public partial class InjectSupermodelConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    [ObservableProperty] private bool _new3DEngine;
    [ObservableProperty] private bool _quadRendering;
    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _wideScreen;
    [ObservableProperty] private bool _stretch;
    [ObservableProperty] private int _resX;
    [ObservableProperty] private int _resY;
    [ObservableProperty] private int _musicVolume;
    [ObservableProperty] private int _soundVolume;
    [ObservableProperty] private bool _throttle;
    [ObservableProperty] private bool _multiThreaded;
    [ObservableProperty] private string _inputSystem;
    [ObservableProperty] private string _powerPcFrequency;
    [ObservableProperty] private bool _showBeforeLaunch;

    public InjectSupermodelConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public List<string> InputSystemOptions { get; } = ["xinput", "dinput", "rawinput"];
    public List<string> PpcFrequencyOptions { get; } = ["50", "60", "75", "100"];

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        New3DEngine = _settings.SupermodelNew3DEngine;
        QuadRendering = _settings.SupermodelQuadRendering;
        Fullscreen = _settings.SupermodelFullscreen;
        Vsync = _settings.SupermodelVsync;
        WideScreen = _settings.SupermodelWideScreen;
        Stretch = _settings.SupermodelStretch;
        ResX = _settings.SupermodelResX;
        ResY = _settings.SupermodelResY;
        MusicVolume = _settings.SupermodelMusicVolume;
        SoundVolume = _settings.SupermodelSoundVolume;
        Throttle = _settings.SupermodelThrottle;
        MultiThreaded = _settings.SupermodelMultiThreaded;
        InputSystem = _settings.SupermodelInputSystem;
        PowerPcFrequency = _settings.SupermodelPowerPcFrequency.ToString(CultureInfo.InvariantCulture);
        ShowBeforeLaunch = _settings.SupermodelShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.SupermodelNew3DEngine = New3DEngine;
        _settings.SupermodelQuadRendering = QuadRendering;
        _settings.SupermodelFullscreen = Fullscreen;
        _settings.SupermodelVsync = Vsync;
        _settings.SupermodelWideScreen = WideScreen;
        _settings.SupermodelStretch = Stretch;
        _settings.SupermodelResX = ResX;
        _settings.SupermodelResY = ResY;
        _settings.SupermodelMusicVolume = MusicVolume;
        _settings.SupermodelSoundVolume = SoundVolume;
        _settings.SupermodelThrottle = Throttle;
        _settings.SupermodelMultiThreaded = MultiThreaded;
        _settings.SupermodelInputSystem = InputSystem;
        _settings.SupermodelPowerPcFrequency = int.Parse(PowerPcFrequency, CultureInfo.InvariantCulture);
        _settings.SupermodelShowSettingsBeforeLaunch = ShowBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Supermodel", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.SupermodelEmulatorNotFoundMessageBox();

        var result = RequestEmulatorPath?.Invoke();
        if (string.IsNullOrEmpty(result)) return null;

        _emulatorPath = result;
        return _emulatorPath;
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            SupermodelConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Supermodel configuration injection failed for path: {path}");
            return false;
        }
    }

    [RelayCommand]
    private void Run()
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                ShouldRun = true;
                CloseRequested?.Invoke();
            }
            else
            {
                MessageBoxLibrary.InjectionFailedGenericMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectSupermodelConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
            ShouldRun = true;
        }
    }

    [RelayCommand]
    private void Save()
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                MessageBoxLibrary.SupermodelConfigurationSavedSuccessfullyMessageBox();
                CloseRequested?.Invoke();
            }
            else
            {
                MessageBoxLibrary.InjectionFailedGenericMessageBox();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectSupermodelConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

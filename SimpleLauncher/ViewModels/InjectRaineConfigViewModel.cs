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

public partial class InjectRaineConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;
    private readonly string _gameFilePath;
    private readonly string _systemRomPath;

    [ObservableProperty] private bool _raineFullscreen;
    [ObservableProperty] private bool _raineFixAspectRatio;
    [ObservableProperty] private bool _raineVsync;
    [ObservableProperty] private int _raineResX;
    [ObservableProperty] private int _raineResY;
    [ObservableProperty] private string _raineSoundDriver;
    [ObservableProperty] private int _raineSampleRate;
    [ObservableProperty] private bool _raineShowSettingsBeforeLaunch;
    [ObservableProperty] private bool _raineShowFps;
    [ObservableProperty] private int _raineFrameSkip;
    [ObservableProperty] private string _raineNeoCdBios;
    [ObservableProperty] private int _raineMusicVolume;
    [ObservableProperty] private int _raineSfxVolume;
    [ObservableProperty] private bool _raineMuteSfx;
    [ObservableProperty] private bool _raineMuteMusic;
    [ObservableProperty] private string _raineRomDirectory;

    public InjectRaineConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode, string gameFilePath = null, string systemRomPath = null)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        _gameFilePath = gameFilePath;
        _systemRomPath = systemRomPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public List<string> SoundDriverOptions { get; } = ["directsound", "sdl"];
    public List<string> SampleRateOptions { get; } = ["22050", "44100", "48000"];
    public List<string> FrameSkipOptions { get; } = ["0", "1", "2", "3", "4", "5"];

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;
    public event Func<string> RequestFilePath;
    public event Func<string> RequestFolderPath;

    private void LoadSettings()
    {
        RaineFullscreen = _settings.RaineFullscreen;
        RaineFixAspectRatio = _settings.RaineFixAspectRatio;
        RaineVsync = _settings.RaineVsync;
        RaineResX = _settings.RaineResX;
        RaineResY = _settings.RaineResY;
        RaineSoundDriver = _settings.RaineSoundDriver;
        RaineSampleRate = _settings.RaineSampleRate;
        RaineShowSettingsBeforeLaunch = _settings.RaineShowSettingsBeforeLaunch;
        RaineShowFps = _settings.RaineShowFps;
        RaineFrameSkip = _settings.RaineFrameSkip;
        RaineNeoCdBios = _settings.RaineNeoCdBios ?? string.Empty;
        RaineMusicVolume = _settings.RaineMusicVolume;
        RaineSfxVolume = _settings.RaineSfxVolume;
        RaineMuteSfx = _settings.RaineMuteSfx;
        RaineMuteMusic = _settings.RaineMuteMusic;
        RaineRomDirectory = _settings.RaineRomDirectory ?? string.Empty;
    }

    private void SaveSettings()
    {
        _settings.RaineFullscreen = RaineFullscreen;
        _settings.RaineFixAspectRatio = RaineFixAspectRatio;
        _settings.RaineVsync = RaineVsync;
        _settings.RaineResX = RaineResX;
        _settings.RaineResY = RaineResY;
        _settings.RaineSoundDriver = RaineSoundDriver;
        _settings.RaineSampleRate = RaineSampleRate;
        _settings.RaineShowSettingsBeforeLaunch = RaineShowSettingsBeforeLaunch;
        _settings.RaineShowFps = RaineShowFps;
        _settings.RaineFrameSkip = RaineFrameSkip;
        _settings.RaineNeoCdBios = RaineNeoCdBios;
        _settings.RaineMusicVolume = RaineMusicVolume;
        _settings.RaineSfxVolume = RaineSfxVolume;
        _settings.RaineMuteSfx = RaineMuteSfx;
        _settings.RaineMuteMusic = RaineMuteMusic;
        _settings.RaineRomDirectory = RaineRomDirectory;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Raine", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.RaineExecutableNotFoundMessageBox();

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
            RaineConfigurationService.InjectSettings(path, _settings, _logErrors, _gameFilePath, _systemRomPath, _settings.RaineRomDirectory);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Raine configuration injection failed for path: {path}");
            return false;
        }
    }

    [RelayCommand]
    private void SelectNeoCdBios()
    {
        var result = RequestFilePath?.Invoke();
        if (!string.IsNullOrEmpty(result))
        {
            RaineNeoCdBios = result;
        }
    }

    [RelayCommand]
    private void SelectRaineRomDirectory()
    {
        var result = RequestFolderPath?.Invoke();
        if (!string.IsNullOrEmpty(result))
        {
            RaineRomDirectory = result;
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRaineConfigWindow));
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
                MessageBoxLibrary.RaineSettingsSavedAndInjectedMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRaineConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

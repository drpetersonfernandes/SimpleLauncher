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

public partial class InjectYumirConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
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

    public InjectYumirConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public List<string> VideoStandardOptions { get; } = ["PAL", "NTSC"];
    public List<string> ForcedAspectOptions { get; } = ["16:9", "4:3"];
    public List<string> ForcedAspectTags { get; } = ["1.7777777777777777", "1.3333333333333333"];

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        YumirFullscreen = _settings.YumirFullscreen;
        YumirForceAspectRatio = _settings.YumirForceAspectRatio;
        YumirReduceLatency = _settings.YumirReduceLatency;
        YumirMute = _settings.YumirMute;
        YumirVolume = _settings.YumirVolume;
        YumirAutoDetectRegion = _settings.YumirAutoDetectRegion;
        YumirVideoStandard = _settings.YumirVideoStandard;
        YumirPauseWhenUnfocused = _settings.YumirPauseWhenUnfocused;
        YumirForcedAspect = _settings.YumirForcedAspect;
        YumirShowSettingsBeforeLaunch = _settings.YumirShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.YumirFullscreen = YumirFullscreen;
        _settings.YumirForceAspectRatio = YumirForceAspectRatio;
        _settings.YumirReduceLatency = YumirReduceLatency;
        _settings.YumirMute = YumirMute;
        _settings.YumirVolume = YumirVolume;
        _settings.YumirAutoDetectRegion = YumirAutoDetectRegion;
        _settings.YumirVideoStandard = YumirVideoStandard;
        _settings.YumirPauseWhenUnfocused = YumirPauseWhenUnfocused;
        _settings.YumirForcedAspect = YumirForcedAspect;
        _settings.YumirShowSettingsBeforeLaunch = YumirShowSettingsBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
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

        MessageBoxLibrary.YumirEmulatorNotFoundMessageBox();

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
            YumirConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Yumir configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectYumirConfigWindow));
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
                MessageBoxLibrary.YumirConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectYumirConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

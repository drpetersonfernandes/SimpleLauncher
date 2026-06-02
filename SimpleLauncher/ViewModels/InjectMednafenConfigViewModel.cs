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

public partial class InjectMednafenConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    [ObservableProperty] private string _mednafenVideoDriver;
    [ObservableProperty] private string _mednafenStretch;
    [ObservableProperty] private string _mednafenShader;
    [ObservableProperty] private string _mednafenSpecial;
    [ObservableProperty] private bool _mednafenFullscreen;
    [ObservableProperty] private bool _mednafenVsync;
    [ObservableProperty] private bool _mednafenBilinear;
    [ObservableProperty] private int _mednafenScanlines;
    [ObservableProperty] private int _mednafenVolume;
    [ObservableProperty] private bool _mednafenCheats;
    [ObservableProperty] private bool _mednafenRewind;
    [ObservableProperty] private bool _mednafenShowSettingsBeforeLaunch;

    public InjectMednafenConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public List<string> VideoDriverOptions { get; } = ["opengl", "soft", "default"];
    public List<string> StretchOptions { get; } = ["0", "full", "aspect", "aspect_int"];
    public List<string> ShaderOptions { get; } = ["none", "ip", "ipsharper", "scale2x", "snes_ntsc", "goat"];

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        MednafenVideoDriver = _settings.MednafenVideoDriver;
        MednafenStretch = _settings.MednafenStretch;

        if (!string.IsNullOrEmpty(_settings.MednafenSpecial) && _settings.MednafenSpecial != "none")
        {
            MednafenShader = _settings.MednafenSpecial;
        }
        else
        {
            MednafenShader = _settings.MednafenShader;
        }

        MednafenSpecial = _settings.MednafenSpecial;
        MednafenFullscreen = _settings.MednafenFullscreen;
        MednafenVsync = _settings.MednafenVsync;
        MednafenBilinear = _settings.MednafenBilinear;
        MednafenScanlines = _settings.MednafenScanlines;
        MednafenVolume = _settings.MednafenVolume;
        MednafenCheats = _settings.MednafenCheats;
        MednafenRewind = _settings.MednafenRewind;
        MednafenShowSettingsBeforeLaunch = _settings.MednafenShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.MednafenVideoDriver = MednafenVideoDriver;
        _settings.MednafenStretch = MednafenStretch;

        if (MednafenShader is "scale2x" or "snes_ntsc")
        {
            _settings.MednafenSpecial = MednafenShader;
            _settings.MednafenShader = "none";
        }
        else
        {
            _settings.MednafenSpecial = "none";
            _settings.MednafenShader = MednafenShader;
        }

        _settings.MednafenFullscreen = MednafenFullscreen;
        _settings.MednafenVsync = MednafenVsync;
        _settings.MednafenBilinear = MednafenBilinear;
        _settings.MednafenScanlines = MednafenScanlines;
        _settings.MednafenVolume = MednafenVolume;
        _settings.MednafenCheats = MednafenCheats;
        _settings.MednafenRewind = MednafenRewind;
        _settings.MednafenShowSettingsBeforeLaunch = MednafenShowSettingsBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Mednafen", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.MednafenEmulatorNotFoundMessageBox();

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
            MednafenConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Mednafen configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectMednafenConfigWindow));
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
                MessageBoxLibrary.MednafenConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectMednafenConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

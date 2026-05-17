using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

public class InjectBlastemConfigViewModel : ViewModelBase
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    private bool _fullscreen;
    private bool _vsync;
    private bool _scanlines;
    private string _aspect;
    private string _scaling;
    private string _audioRate;
    private string _syncSource;
    private bool _showBeforeLaunch;

    public InjectBlastemConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        SaveCommand = new RelayCommand(_ => ExecuteSave());
        RunCommand = new RelayCommand(_ => ExecuteRun());

        LoadSettings();
    }

    public List<string> AspectOptions { get; } = ["4:3", "16:9", "stretch"];
    public List<string> ScalingOptions { get; } = ["linear", "nearest"];
    public List<string> AudioRateOptions { get; } = ["48000", "44100", "22050"];
    public List<string> SyncSourceOptions { get; } = ["audio", "video"];

    public bool Fullscreen
    {
        get => _fullscreen;
        set => SetProperty(ref _fullscreen, value);
    }

    public bool Vsync
    {
        get => _vsync;
        set => SetProperty(ref _vsync, value);
    }

    public bool Scanlines
    {
        get => _scanlines;
        set => SetProperty(ref _scanlines, value);
    }

    public string Aspect
    {
        get => _aspect;
        set => SetProperty(ref _aspect, value);
    }

    public string Scaling
    {
        get => _scaling;
        set => SetProperty(ref _scaling, value);
    }

    public string AudioRate
    {
        get => _audioRate;
        set => SetProperty(ref _audioRate, value);
    }

    public string SyncSource
    {
        get => _syncSource;
        set => SetProperty(ref _syncSource, value);
    }

    public bool ShowBeforeLaunch
    {
        get => _showBeforeLaunch;
        set => SetProperty(ref _showBeforeLaunch, value);
    }

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public ICommand SaveCommand { get; }
    public ICommand RunCommand { get; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        Fullscreen = _settings.BlastemFullscreen;
        Vsync = _settings.BlastemVsync;
        Scanlines = _settings.BlastemScanlines;
        Aspect = _settings.BlastemAspect;
        Scaling = _settings.BlastemScaling;
        AudioRate = _settings.BlastemAudioRate.ToString(CultureInfo.InvariantCulture);
        SyncSource = _settings.BlastemSyncSource;
        ShowBeforeLaunch = _settings.BlastemShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.BlastemFullscreen = Fullscreen;
        _settings.BlastemVsync = Vsync;
        _settings.BlastemScanlines = Scanlines;
        _settings.BlastemAspect = Aspect;
        _settings.BlastemScaling = Scaling;
        _settings.BlastemAudioRate = int.Parse(AudioRate, CultureInfo.InvariantCulture);
        _settings.BlastemSyncSource = SyncSource;
        _settings.BlastemShowSettingsBeforeLaunch = ShowBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Blastem");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.BlastemEmulatorNotFoundMessageBox();

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
            BlastemConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (FileNotFoundException ex)
        {
            var errorMsg = $"Configuration file not found for Blastem at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            var errorMsg = $"Permission denied accessing Blastem configuration at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return false;
        }
        catch (IOException ex)
        {
            var errorMsg = $"I/O error while accessing Blastem configuration at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return false;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Blastem configuration injection failed for path: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return false;
        }
    }

    private void ExecuteRun()
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectBlastemConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
            ShouldRun = true;
        }
    }

    private void ExecuteSave()
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                MessageBoxLibrary.BlastemConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectBlastemConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

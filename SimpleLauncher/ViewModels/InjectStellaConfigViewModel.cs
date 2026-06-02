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

public partial class InjectStellaConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _correctAspect;
    [ObservableProperty] private string _videoDriver;
    [ObservableProperty] private string _tvFilter;
    [ObservableProperty] private int _scanlines;
    [ObservableProperty] private bool _audioEnabled;
    [ObservableProperty] private int _audioVolume;
    [ObservableProperty] private bool _timeMachine;
    [ObservableProperty] private bool _confirmExit;
    [ObservableProperty] private bool _showBeforeLaunch;

    public InjectStellaConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public List<string> VideoDriverOptions { get; } = ["direct3d", "opengl", "software"];
    public List<string> TvFilterOptions { get; } = ["0", "1", "2", "3"];

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        Fullscreen = _settings.StellaFullscreen;
        Vsync = _settings.StellaVsync;
        CorrectAspect = _settings.StellaCorrectAspect;
        VideoDriver = _settings.StellaVideoDriver;
        TvFilter = _settings.StellaTvFilter.ToString(CultureInfo.InvariantCulture);
        Scanlines = _settings.StellaScanlines;
        AudioEnabled = _settings.StellaAudioEnabled;
        AudioVolume = _settings.StellaAudioVolume;
        TimeMachine = _settings.StellaTimeMachine;
        ConfirmExit = _settings.StellaConfirmExit;
        ShowBeforeLaunch = _settings.StellaShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.StellaFullscreen = Fullscreen;
        _settings.StellaVsync = Vsync;
        _settings.StellaCorrectAspect = CorrectAspect;
        _settings.StellaVideoDriver = VideoDriver;
        _settings.StellaTvFilter = int.Parse(TvFilter, CultureInfo.InvariantCulture);
        _settings.StellaScanlines = Scanlines;
        _settings.StellaAudioEnabled = AudioEnabled;
        _settings.StellaAudioVolume = AudioVolume;
        _settings.StellaTimeMachine = TimeMachine;
        _settings.StellaConfirmExit = ConfirmExit;
        _settings.StellaShowSettingsBeforeLaunch = ShowBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Stella", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.StellaEmulatorNotFoundMessageBox();

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
            StellaConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Stella configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectStellaConfigWindow));
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
                MessageBoxLibrary.StellaConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectStellaConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

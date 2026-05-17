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

public class InjectAzaharConfigViewModel : ViewModelBase
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    private string _graphicsApi;
    private string _resolution;
    private string _layout;
    private bool _fullscreen;
    private bool _vsync;
    private bool _asyncShader;
    private bool _isNew3Ds;
    private int _volume;
    private bool _showBeforeLaunch;
    private bool _audioStretching;

    public InjectAzaharConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        SaveCommand = new RelayCommand(_ => ExecuteSave());
        RunCommand = new RelayCommand(_ => ExecuteRun());

        LoadSettings();
    }

    public string GraphicsApi
    {
        get => _graphicsApi;
        set => SetProperty(ref _graphicsApi, value);
    }

    public string Resolution
    {
        get => _resolution;
        set => SetProperty(ref _resolution, value);
    }

    public string Layout
    {
        get => _layout;
        set => SetProperty(ref _layout, value);
    }

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

    public bool AsyncShader
    {
        get => _asyncShader;
        set => SetProperty(ref _asyncShader, value);
    }

    public bool IsNew3Ds
    {
        get => _isNew3Ds;
        set => SetProperty(ref _isNew3Ds, value);
    }

    public int Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    public bool ShowBeforeLaunch
    {
        get => _showBeforeLaunch;
        set => SetProperty(ref _showBeforeLaunch, value);
    }

    public bool AudioStretching
    {
        get => _audioStretching;
        set => SetProperty(ref _audioStretching, value);
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
        GraphicsApi = _settings.AzaharGraphicsApi.ToString(CultureInfo.InvariantCulture);
        Resolution = _settings.AzaharResolutionFactor.ToString(CultureInfo.InvariantCulture);
        Layout = _settings.AzaharLayoutOption.ToString(CultureInfo.InvariantCulture);
        Fullscreen = _settings.AzaharFullscreen;
        Vsync = _settings.AzaharUseVsync;
        AsyncShader = _settings.AzaharAsyncShaderCompilation;
        IsNew3Ds = _settings.AzaharIsNew3Ds;
        Volume = _settings.AzaharVolume;
        ShowBeforeLaunch = _settings.AzaharShowSettingsBeforeLaunch;
        AudioStretching = _settings.AzaharEnableAudioStretching;
    }

    private void SaveSettings()
    {
        _settings.AzaharGraphicsApi = int.Parse(GraphicsApi, CultureInfo.InvariantCulture);
        _settings.AzaharResolutionFactor = int.Parse(Resolution, CultureInfo.InvariantCulture);
        _settings.AzaharLayoutOption = int.Parse(Layout, CultureInfo.InvariantCulture);
        _settings.AzaharFullscreen = Fullscreen;
        _settings.AzaharUseVsync = Vsync;
        _settings.AzaharAsyncShaderCompilation = AsyncShader;
        _settings.AzaharIsNew3Ds = IsNew3Ds;
        _settings.AzaharVolume = Volume;
        _settings.AzaharShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.AzaharEnableAudioStretching = AudioStretching;
        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Azahar");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

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
            AzaharConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (AzaharPermissionException)
        {
            MessageBoxLibrary.AzaharConfigurationInjectionPermissionErrorMessageBox();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, "Azahar injection failed");
            MessageBoxLibrary.FailedToSaveAzaharConfigurationMessageBox();
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
        catch (AzaharPermissionException)
        {
            CloseRequested?.Invoke();
            ShouldRun = true;
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAzaharConfigWindow));
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
                MessageBoxLibrary.AzaharConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAzaharConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

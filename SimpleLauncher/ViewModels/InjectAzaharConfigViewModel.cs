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

public partial class InjectAzaharConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    [ObservableProperty] private string _graphicsApi;
    [ObservableProperty] private string _resolution;
    [ObservableProperty] private string _layout;
    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _asyncShader;
    [ObservableProperty] private bool _isNew3Ds;
    [ObservableProperty] private int _volume;
    [ObservableProperty] private bool _showBeforeLaunch;
    [ObservableProperty] private bool _audioStretching;

    public InjectAzaharConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

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

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Azahar", _logErrors);
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
            AzaharConfigurationService.InjectSettings(path, _settings, _logErrors);
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

    [RelayCommand]
    private void Save()
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

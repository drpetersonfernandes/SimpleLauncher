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

public class InjectCemuConfigViewModel : ViewModelBase
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    private bool _fullscreen;
    private string _graphicApi;
    private string _vsync;
    private bool _asyncCompile;
    private int _volume;
    private bool _discord;
    private string _language;
    private bool _showBeforeLaunch;

    public InjectCemuConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        SaveCommand = new RelayCommand(_ => ExecuteSave());
        RunCommand = new RelayCommand(_ => ExecuteRun());

        LoadSettings();
    }

    public bool Fullscreen
    {
        get => _fullscreen;
        set => SetProperty(ref _fullscreen, value);
    }

    public string GraphicApi
    {
        get => _graphicApi;
        set => SetProperty(ref _graphicApi, value);
    }

    public string Vsync
    {
        get => _vsync;
        set => SetProperty(ref _vsync, value);
    }

    public bool AsyncCompile
    {
        get => _asyncCompile;
        set => SetProperty(ref _asyncCompile, value);
    }

    public int Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    public bool Discord
    {
        get => _discord;
        set => SetProperty(ref _discord, value);
    }

    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
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
        Fullscreen = _settings.CemuFullscreen;
        GraphicApi = _settings.CemuGraphicApi.ToString(CultureInfo.InvariantCulture);
        Vsync = _settings.CemuVsync.ToString(CultureInfo.InvariantCulture);
        AsyncCompile = _settings.CemuAsyncCompile;
        Volume = _settings.CemuTvVolume;
        Discord = _settings.CemuDiscordPresence;
        Language = _settings.CemuConsoleLanguage.ToString(CultureInfo.InvariantCulture);
        ShowBeforeLaunch = _settings.CemuShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.CemuFullscreen = Fullscreen;
        _settings.CemuGraphicApi = int.Parse(GraphicApi, CultureInfo.InvariantCulture);
        _settings.CemuVsync = int.Parse(Vsync, CultureInfo.InvariantCulture);
        _settings.CemuAsyncCompile = AsyncCompile;
        _settings.CemuTvVolume = Volume;
        _settings.CemuDiscordPresence = Discord;
        _settings.CemuConsoleLanguage = int.Parse(Language, CultureInfo.InvariantCulture);
        _settings.CemuShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Cemu");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.CemuemulatornotfoundMessageBox();

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
            CemuConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Cemu injection failed: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectCemuConfigWindow));
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
                MessageBoxLibrary.CemuConfigurationSavedMessageBox();
                ShouldRun = false;
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectCemuConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

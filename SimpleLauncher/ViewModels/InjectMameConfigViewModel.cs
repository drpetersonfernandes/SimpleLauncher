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

public partial class InjectMameConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;
    private readonly string _systemRomPath;
    private readonly string[] _listOfSecondarySystemFolders;

    [ObservableProperty] private string _mameVideo;
    [ObservableProperty] private string _mameBgfxBackend;
    [ObservableProperty] private string _mameBgfxScreenChains;
    [ObservableProperty] private bool _mameFilter;
    [ObservableProperty] private bool _mameAutoframeskip;
    [ObservableProperty] private bool _mameCheat;
    [ObservableProperty] private bool _mameRewind;
    [ObservableProperty] private bool _mameNvramSave;
    [ObservableProperty] private bool _mameWindow;
    [ObservableProperty] private bool _mameMaximize;
    [ObservableProperty] private bool _mameKeepAspect;
    [ObservableProperty] private bool _mameSkipGameInfo;
    [ObservableProperty] private bool _mameAutosave;
    [ObservableProperty] private bool _mameConfirmQuit;
    [ObservableProperty] private bool _mameJoystick;
    [ObservableProperty] private bool _mameShowSettingsBeforeLaunch;

    public InjectMameConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode, string systemRomPath = null, string[] listOfSecondaryRomPaths = null)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _systemRomPath = systemRomPath;
        _listOfSecondarySystemFolders = listOfSecondaryRomPaths;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public List<string> VideoOptions { get; } = ["auto", "d3d", "opengl", "bgfx", "gdi"];
    public List<string> BgfxBackendOptions { get; } = ["auto", "d3d11", "vulkan", "opengl"];
    public List<string> BgfxChainsOptions { get; } = ["default", "crt-geom"];

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        MameVideo = _settings.MameVideo;
        MameBgfxBackend = _settings.MameBgfxBackend;
        MameBgfxScreenChains = _settings.MameBgfxScreenChains;
        MameFilter = _settings.MameFilter;
        MameAutoframeskip = _settings.MameAutoframeskip;
        MameCheat = _settings.MameCheat;
        MameRewind = _settings.MameRewind;
        MameNvramSave = _settings.MameNvramSave;
        MameWindow = _settings.MameWindow;
        MameMaximize = _settings.MameMaximize;
        MameKeepAspect = _settings.MameKeepAspect;
        MameSkipGameInfo = _settings.MameSkipGameInfo;
        MameAutosave = _settings.MameAutosave;
        MameConfirmQuit = _settings.MameConfirmQuit;
        MameJoystick = _settings.MameJoystick;
        MameShowSettingsBeforeLaunch = _settings.MameShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.MameVideo = MameVideo;
        _settings.MameBgfxBackend = MameBgfxBackend;
        _settings.MameBgfxScreenChains = MameBgfxScreenChains;
        _settings.MameFilter = MameFilter;
        _settings.MameAutoframeskip = MameAutoframeskip;
        _settings.MameCheat = MameCheat;
        _settings.MameRewind = MameRewind;
        _settings.MameNvramSave = MameNvramSave;
        _settings.MameWindow = MameWindow;
        _settings.MameMaximize = MameMaximize;
        _settings.MameKeepAspect = MameKeepAspect;
        _settings.MameSkipGameInfo = MameSkipGameInfo;
        _settings.MameAutosave = MameAutosave;
        _settings.MameConfirmQuit = MameConfirmQuit;
        _settings.MameJoystick = MameJoystick;
        _settings.MameShowSettingsBeforeLaunch = MameShowSettingsBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("MAME", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.MameEmulatorPathNotFoundMessageBox();

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
            MameConfigurationService.InjectSettings(path, _settings, _logErrors, _systemRomPath, _listOfSecondarySystemFolders);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"MAME configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectMameConfigWindow));
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
                MessageBoxLibrary.MamEconfigurationinjectedsuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectMameConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

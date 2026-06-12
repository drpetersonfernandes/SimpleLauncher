using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;
using MameConfigurationService = SimpleLauncher.Services.InjectEmulatorConfig.MameConfigurationService;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the MAME emulator configuration injection window.
/// </summary>
public partial class InjectMameConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;
    private string _systemRomPath;
    private string[] _listOfSecondarySystemFolders;

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

    public InjectMameConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path, launcher mode, and optional ROM paths.
    /// </summary>
    /// <param name="emulatorPath">The file path to the MAME emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    /// <param name="systemRomPath">Optional path to the system ROM directory.</param>
    /// <param name="listOfSecondaryRomPaths">Optional list of secondary ROM folder paths.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode, string systemRomPath = null, string[] listOfSecondaryRomPaths = null)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _systemRomPath = systemRomPath;
        _listOfSecondarySystemFolders = listOfSecondaryRomPaths;
        LoadSettings();
    }

    /// <summary>
    /// Available video output options for MAME.
    /// </summary>
    public List<string> VideoOptions { get; } = ["auto", "d3d", "opengl", "bgfx", "gdi"];

    /// <summary>
    /// Available BGFX backend options for MAME.
    /// </summary>
    public List<string> BgfxBackendOptions { get; } = ["auto", "d3d11", "vulkan", "opengl"];

    /// <summary>
    /// Available BGFX screen chain options for MAME.
    /// </summary>
    public List<string> BgfxChainsOptions { get; } = ["default", "crt-geom"];

    /// <summary>
    /// Gets whether the configuration is being injected from launcher mode.
    /// </summary>
    public bool IsLauncherMode { get; private set; }

    /// <summary>
    /// Gets whether the emulator should be launched after configuration injection.
    /// </summary>
    public bool ShouldRun { get; private set; }

    /// <summary>
    /// Raised when the window should be closed.
    /// </summary>
    public event Action CloseRequested;

    /// <summary>
    /// Requests the user to provide the emulator executable path.
    /// </summary>
    public event Func<string> RequestEmulatorPath;

    /// <summary>
    /// Gets the owner window for dialog display.
    /// </summary>
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        MameVideo = _settings.Mame.Video;
        MameBgfxBackend = _settings.Mame.BgfxBackend;
        MameBgfxScreenChains = _settings.Mame.BgfxScreenChains;
        MameFilter = _settings.Mame.Filter;
        MameAutoframeskip = _settings.Mame.Autoframeskip;
        MameCheat = _settings.Mame.Cheat;
        MameRewind = _settings.Mame.Rewind;
        MameNvramSave = _settings.Mame.NvramSave;
        MameWindow = _settings.Mame.Window;
        MameMaximize = _settings.Mame.Maximize;
        MameKeepAspect = _settings.Mame.KeepAspect;
        MameSkipGameInfo = _settings.Mame.SkipGameInfo;
        MameAutosave = _settings.Mame.Autosave;
        MameConfirmQuit = _settings.Mame.ConfirmQuit;
        MameJoystick = _settings.Mame.Joystick;
        MameShowSettingsBeforeLaunch = _settings.Mame.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Mame.Video = MameVideo;
        _settings.Mame.BgfxBackend = MameBgfxBackend;
        _settings.Mame.BgfxScreenChains = MameBgfxScreenChains;
        _settings.Mame.Filter = MameFilter;
        _settings.Mame.Autoframeskip = MameAutoframeskip;
        _settings.Mame.Cheat = MameCheat;
        _settings.Mame.Rewind = MameRewind;
        _settings.Mame.NvramSave = MameNvramSave;
        _settings.Mame.Window = MameWindow;
        _settings.Mame.Maximize = MameMaximize;
        _settings.Mame.KeepAspect = MameKeepAspect;
        _settings.Mame.SkipGameInfo = MameSkipGameInfo;
        _settings.Mame.Autosave = MameAutosave;
        _settings.Mame.ConfirmQuit = MameConfirmQuit;
        _settings.Mame.Joystick = MameJoystick;
        _settings.Mame.ShowSettingsBeforeLaunch = MameShowSettingsBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPathAsync()
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

        await _messageBox.MameEmulatorPathNotFoundMessageBox();

        var result = RequestEmulatorPath?.Invoke();
        if (string.IsNullOrEmpty(result)) return null;

        _emulatorPath = result;
        return _emulatorPath;
    }

    private async Task<bool> InjectConfigAsync()
    {
        var path = await EnsureEmulatorPathAsync();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            MameConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger, _systemRomPath, _listOfSecondarySystemFolders);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"MAME configuration injection failed for path: {path}");
            return false;
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfigAsync())
            {
                ShouldRun = true;
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBox();
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
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
            ShouldRun = true;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfigAsync())
            {
                await _messageBox.MameConfigurationInjectedSuccessfullyMessageBox();
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBox();
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
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

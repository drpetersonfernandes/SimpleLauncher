using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the RetroArch emulator configuration injection window.
/// </summary>
public partial class InjectRetroArchConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private string _videoDriver;
    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _threadedVideo;
    [ObservableProperty] private bool _bilinear;
    [ObservableProperty] private string _aspectRatioIndex;
    [ObservableProperty] private bool _scaleInteger;
    [ObservableProperty] private bool _shaderEnable;
    [ObservableProperty] private bool _hardSync;
    [ObservableProperty] private bool _audioEnable;
    [ObservableProperty] private bool _audioMute;
    [ObservableProperty] private bool _pauseNonActive;
    [ObservableProperty] private bool _saveOnExit;
    [ObservableProperty] private bool _autoSaveState;
    [ObservableProperty] private bool _autoLoadState;
    [ObservableProperty] private bool _rewind;
    [ObservableProperty] private bool _runAhead;
    [ObservableProperty] private string _menuDriver;
    [ObservableProperty] private bool _showAdvancedSettings;
    [ObservableProperty] private bool _cheevosEnable;
    [ObservableProperty] private bool _cheevosHardcore;
    [ObservableProperty] private bool _discordAllow;
    [ObservableProperty] private bool _showBeforeLaunch;

    public InjectRetroArchConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the RetroArch emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available video driver options for RetroArch.
    /// </summary>
    public List<string> VideoDriverOptions { get; } = ["gl", "vulkan", "d3d11", "d3d12", "d3d10", "sdl2"];

    /// <summary>
    /// Available aspect ratio display options for RetroArch.
    /// </summary>
    public List<string> AspectRatioIndexOptions { get; } = ["Core Provided", "4:3", "16:9", "16:10"];

    /// <summary>
    /// Tags corresponding to the aspect ratio options for RetroArch.
    /// </summary>
    public List<string> AspectRatioIndexTags { get; } = ["22", "0", "1", "2"];

    /// <summary>
    /// Available menu driver options for RetroArch.
    /// </summary>
    public List<string> MenuDriverOptions { get; } = ["ozone", "xmb", "rgui", "glui"];

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

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke();

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
        VideoDriver = _settings.RetroArch.VideoDriver;
        Fullscreen = _settings.RetroArch.Fullscreen;
        Vsync = _settings.RetroArch.Vsync;
        ThreadedVideo = _settings.RetroArch.ThreadedVideo;
        Bilinear = _settings.RetroArch.Bilinear;
        AspectRatioIndex = _settings.RetroArch.AspectRatioIndex;
        ScaleInteger = _settings.RetroArch.ScaleInteger;
        ShaderEnable = _settings.RetroArch.ShaderEnable;
        HardSync = _settings.RetroArch.HardSync;
        AudioEnable = _settings.RetroArch.AudioEnable;
        AudioMute = _settings.RetroArch.AudioMute;
        PauseNonActive = _settings.RetroArch.PauseNonActive;
        SaveOnExit = _settings.RetroArch.SaveOnExit;
        AutoSaveState = _settings.RetroArch.AutoSaveState;
        AutoLoadState = _settings.RetroArch.AutoLoadState;
        Rewind = _settings.RetroArch.Rewind;
        RunAhead = _settings.RetroArch.RunAhead;
        MenuDriver = _settings.RetroArch.MenuDriver;
        ShowAdvancedSettings = _settings.RetroArch.ShowAdvancedSettings;
        CheevosEnable = _settings.RetroArch.CheevosEnable;
        CheevosHardcore = _settings.RetroArch.CheevosHardcore;
        DiscordAllow = _settings.RetroArch.DiscordAllow;
        ShowBeforeLaunch = _settings.RetroArch.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.RetroArch.VideoDriver = VideoDriver;
        _settings.RetroArch.Fullscreen = Fullscreen;
        _settings.RetroArch.Vsync = Vsync;
        _settings.RetroArch.ThreadedVideo = ThreadedVideo;
        _settings.RetroArch.Bilinear = Bilinear;
        _settings.RetroArch.AspectRatioIndex = AspectRatioIndex;
        _settings.RetroArch.ScaleInteger = ScaleInteger;
        _settings.RetroArch.ShaderEnable = ShaderEnable;
        _settings.RetroArch.HardSync = HardSync;
        _settings.RetroArch.AudioEnable = AudioEnable;
        _settings.RetroArch.AudioMute = AudioMute;
        _settings.RetroArch.PauseNonActive = PauseNonActive;
        _settings.RetroArch.SaveOnExit = SaveOnExit;
        _settings.RetroArch.AutoSaveState = AutoSaveState;
        _settings.RetroArch.AutoLoadState = AutoLoadState;
        _settings.RetroArch.Rewind = Rewind;
        _settings.RetroArch.RunAhead = RunAhead;
        _settings.RetroArch.MenuDriver = MenuDriver;
        _settings.RetroArch.ShowAdvancedSettings = ShowAdvancedSettings;
        _settings.RetroArch.CheevosEnable = CheevosEnable;
        _settings.RetroArch.CheevosHardcore = CheevosHardcore;
        _settings.RetroArch.DiscordAllow = DiscordAllow;
        _settings.RetroArch.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("RetroArch", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.RetroArchemulatorpathnotfoundMessageBoxAsync();

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
            RetroArchConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"RetroArch configuration injection failed for path: {path}");
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
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRetroArchConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
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
                await _messageBox.RetroArchConfigurationInjectedSuccessfullyMessageBoxAsync();
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRetroArchConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

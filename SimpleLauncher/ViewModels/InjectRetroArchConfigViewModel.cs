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

/// <summary>
/// ViewModel for the RetroArch emulator configuration injection window.
/// </summary>
public partial class InjectRetroArchConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
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

    public InjectRetroArchConfigViewModel(SettingsManager settings)
    {
        _settings = settings;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
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
        VideoDriver = _settings.RetroArchVideoDriver;
        Fullscreen = _settings.RetroArchFullscreen;
        Vsync = _settings.RetroArchVsync;
        ThreadedVideo = _settings.RetroArchThreadedVideo;
        Bilinear = _settings.RetroArchBilinear;
        AspectRatioIndex = _settings.RetroArchAspectRatioIndex;
        ScaleInteger = _settings.RetroArchScaleInteger;
        ShaderEnable = _settings.RetroArchShaderEnable;
        HardSync = _settings.RetroArchHardSync;
        AudioEnable = _settings.RetroArchAudioEnable;
        AudioMute = _settings.RetroArchAudioMute;
        PauseNonActive = _settings.RetroArchPauseNonActive;
        SaveOnExit = _settings.RetroArchSaveOnExit;
        AutoSaveState = _settings.RetroArchAutoSaveState;
        AutoLoadState = _settings.RetroArchAutoLoadState;
        Rewind = _settings.RetroArchRewind;
        RunAhead = _settings.RetroArchRunAhead;
        MenuDriver = _settings.RetroArchMenuDriver;
        ShowAdvancedSettings = _settings.RetroArchShowAdvancedSettings;
        CheevosEnable = _settings.RetroArchCheevosEnable;
        CheevosHardcore = _settings.RetroArchCheevosHardcore;
        DiscordAllow = _settings.RetroArchDiscordAllow;
        ShowBeforeLaunch = _settings.RetroArchShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.RetroArchVideoDriver = VideoDriver;
        _settings.RetroArchFullscreen = Fullscreen;
        _settings.RetroArchVsync = Vsync;
        _settings.RetroArchThreadedVideo = ThreadedVideo;
        _settings.RetroArchBilinear = Bilinear;
        _settings.RetroArchAspectRatioIndex = AspectRatioIndex;
        _settings.RetroArchScaleInteger = ScaleInteger;
        _settings.RetroArchShaderEnable = ShaderEnable;
        _settings.RetroArchHardSync = HardSync;
        _settings.RetroArchAudioEnable = AudioEnable;
        _settings.RetroArchAudioMute = AudioMute;
        _settings.RetroArchPauseNonActive = PauseNonActive;
        _settings.RetroArchSaveOnExit = SaveOnExit;
        _settings.RetroArchAutoSaveState = AutoSaveState;
        _settings.RetroArchAutoLoadState = AutoLoadState;
        _settings.RetroArchRewind = Rewind;
        _settings.RetroArchRunAhead = RunAhead;
        _settings.RetroArchMenuDriver = MenuDriver;
        _settings.RetroArchShowAdvancedSettings = ShowAdvancedSettings;
        _settings.RetroArchCheevosEnable = CheevosEnable;
        _settings.RetroArchCheevosHardcore = CheevosHardcore;
        _settings.RetroArchDiscordAllow = DiscordAllow;
        _settings.RetroArchShowSettingsBeforeLaunch = ShowBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
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

        MessageBoxLibrary.RetroArchemulatorpathnotfoundMessageBox();

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
            RetroArchConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"RetroArch configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRetroArchConfigWindow));
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
                MessageBoxLibrary.RetroArchconfigurationinjectedsuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRetroArchConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

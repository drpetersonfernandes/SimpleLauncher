using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the DuckStation emulator configuration injection window.
/// </summary>
public partial class InjectDuckStationConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private bool _duckStationStartFullscreen;
    [ObservableProperty] private bool _duckStationPauseOnFocusLoss;
    [ObservableProperty] private bool _duckStationSaveStateOnExit;
    [ObservableProperty] private bool _duckStationRewindEnable;
    [ObservableProperty] private int _duckStationRunaheadFrameCount;
    [ObservableProperty] private string _duckStationRenderer;
    [ObservableProperty] private int _duckStationResolutionScale;
    [ObservableProperty] private string _duckStationTextureFilter;
    [ObservableProperty] private string _duckStationAspectRatio;
    [ObservableProperty] private bool _duckStationWidescreenHack;
    [ObservableProperty] private bool _duckStationPgxpEnable;
    [ObservableProperty] private bool _duckStationVsync;
    [ObservableProperty] private bool _duckStationOutputMuted;
    [ObservableProperty] private int _duckStationOutputVolume;
    [ObservableProperty] private bool _duckStationShowSettingsBeforeLaunch;

    public InjectDuckStationConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the DuckStation emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available renderer options for DuckStation.
    /// </summary>
    public List<string> RendererOptions { get; } = ["Automatic", "Vulkan", "Direct3D 11", "Direct3D 12", "OpenGL", "Software"];

    /// <summary>
    /// Available resolution scale options for DuckStation.
    /// </summary>
    public List<TagOption> ResolutionScaleOptions { get; } =
    [
        new("1", "1x (Native)"),
        new("2", "2x (720p)"),
        new("3", "3x (1080p)"),
        new("4", "4x (1440p)"),
        new("5", "5x (1620p)"),
        new("6", "6x (4K)"),
        new("7", "7x (5K)"),
        new("8", "8x (8K)")
    ];

    /// <summary>
    /// Available texture filter options for DuckStation.
    /// </summary>
    public List<string> TextureFilterOptions { get; } = ["Nearest", "Bilinear", "Bilinear(Bilinear)"];

    /// <summary>
    /// Available aspect ratio options for DuckStation.
    /// </summary>
    public List<string> AspectRatioOptions { get; } = ["Auto", "4:3", "16:9", "16:10", "Crop"];

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
        DuckStationStartFullscreen = _settings.DuckStation.StartFullscreen;
        DuckStationPauseOnFocusLoss = _settings.DuckStation.PauseOnFocusLoss;
        DuckStationSaveStateOnExit = _settings.DuckStation.SaveStateOnExit;
        DuckStationRewindEnable = _settings.DuckStation.RewindEnable;
        DuckStationRunaheadFrameCount = _settings.DuckStation.RunaheadFrameCount;
        DuckStationRenderer = _settings.DuckStation.Renderer;
        DuckStationResolutionScale = _settings.DuckStation.ResolutionScale;
        DuckStationTextureFilter = _settings.DuckStation.TextureFilter;
        DuckStationAspectRatio = _settings.DuckStation.AspectRatio;
        DuckStationWidescreenHack = _settings.DuckStation.WidescreenHack;
        DuckStationPgxpEnable = _settings.DuckStation.PgxpEnable;
        DuckStationVsync = _settings.DuckStation.Vsync;
        DuckStationOutputMuted = _settings.DuckStation.OutputMuted;
        DuckStationOutputVolume = _settings.DuckStation.OutputVolume;
        DuckStationShowSettingsBeforeLaunch = _settings.DuckStation.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.DuckStation.StartFullscreen = DuckStationStartFullscreen;
        _settings.DuckStation.PauseOnFocusLoss = DuckStationPauseOnFocusLoss;
        _settings.DuckStation.SaveStateOnExit = DuckStationSaveStateOnExit;
        _settings.DuckStation.RewindEnable = DuckStationRewindEnable;
        _settings.DuckStation.RunaheadFrameCount = DuckStationRunaheadFrameCount;
        _settings.DuckStation.Renderer = DuckStationRenderer;
        _settings.DuckStation.ResolutionScale = DuckStationResolutionScale;
        _settings.DuckStation.TextureFilter = DuckStationTextureFilter;
        _settings.DuckStation.AspectRatio = DuckStationAspectRatio;
        _settings.DuckStation.WidescreenHack = DuckStationWidescreenHack;
        _settings.DuckStation.PgxpEnable = DuckStationPgxpEnable;
        _settings.DuckStation.Vsync = DuckStationVsync;
        _settings.DuckStation.OutputMuted = DuckStationOutputMuted;
        _settings.DuckStation.OutputVolume = DuckStationOutputVolume;
        _settings.DuckStation.ShowSettingsBeforeLaunch = DuckStationShowSettingsBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("DuckStation", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.DuckStationEmulatorNotFoundMessageBoxAsync();

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
            DuckStationConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"DuckStation configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectDuckStationConfigWindow));
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
                await _messageBox.DuckStationConfigurationSavedSuccessfullyMessageBoxAsync();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectDuckStationConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

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
/// ViewModel for the DuckStation emulator configuration injection window.
/// </summary>
public partial class InjectDuckStationConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
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

    public InjectDuckStationConfigViewModel(SettingsManager settings)
    {
        _settings = settings;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
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
        DuckStationStartFullscreen = _settings.DuckStationStartFullscreen;
        DuckStationPauseOnFocusLoss = _settings.DuckStationPauseOnFocusLoss;
        DuckStationSaveStateOnExit = _settings.DuckStationSaveStateOnExit;
        DuckStationRewindEnable = _settings.DuckStationRewindEnable;
        DuckStationRunaheadFrameCount = _settings.DuckStationRunaheadFrameCount;
        DuckStationRenderer = _settings.DuckStationRenderer;
        DuckStationResolutionScale = _settings.DuckStationResolutionScale;
        DuckStationTextureFilter = _settings.DuckStationTextureFilter;
        DuckStationAspectRatio = _settings.DuckStationAspectRatio;
        DuckStationWidescreenHack = _settings.DuckStationWidescreenHack;
        DuckStationPgxpEnable = _settings.DuckStationPgxpEnable;
        DuckStationVsync = _settings.DuckStationVsync;
        DuckStationOutputMuted = _settings.DuckStationOutputMuted;
        DuckStationOutputVolume = _settings.DuckStationOutputVolume;
        DuckStationShowSettingsBeforeLaunch = _settings.DuckStationShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.DuckStationStartFullscreen = DuckStationStartFullscreen;
        _settings.DuckStationPauseOnFocusLoss = DuckStationPauseOnFocusLoss;
        _settings.DuckStationSaveStateOnExit = DuckStationSaveStateOnExit;
        _settings.DuckStationRewindEnable = DuckStationRewindEnable;
        _settings.DuckStationRunaheadFrameCount = DuckStationRunaheadFrameCount;
        _settings.DuckStationRenderer = DuckStationRenderer;
        _settings.DuckStationResolutionScale = DuckStationResolutionScale;
        _settings.DuckStationTextureFilter = DuckStationTextureFilter;
        _settings.DuckStationAspectRatio = DuckStationAspectRatio;
        _settings.DuckStationWidescreenHack = DuckStationWidescreenHack;
        _settings.DuckStationPgxpEnable = DuckStationPgxpEnable;
        _settings.DuckStationVsync = DuckStationVsync;
        _settings.DuckStationOutputMuted = DuckStationOutputMuted;
        _settings.DuckStationOutputVolume = DuckStationOutputVolume;
        _settings.DuckStationShowSettingsBeforeLaunch = DuckStationShowSettingsBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
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

        MessageBoxLibrary.DuckStationEmulatorNotFoundMessageBox();

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
            DuckStationConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"DuckStation configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectDuckStationConfigWindow));
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
                MessageBoxLibrary.DuckStationConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectDuckStationConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

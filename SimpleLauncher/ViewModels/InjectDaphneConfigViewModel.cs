using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Daphne emulator configuration injection window.
/// </summary>
public partial class InjectDaphneConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;

    [ObservableProperty] private bool _daphneFullscreen;
    [ObservableProperty] private bool _daphneBilinear;
    [ObservableProperty] private int _daphneResX;
    [ObservableProperty] private int _daphneResY;
    [ObservableProperty] private bool _daphneEnableSound;
    [ObservableProperty] private bool _daphneDisableCrosshairs;
    [ObservableProperty] private bool _daphneUseOverlays;
    [ObservableProperty] private bool _daphneShowSettingsBeforeLaunch;

    public InjectDaphneConfigViewModel(SettingsManager settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Initializes the ViewModel with the launcher mode.
    /// </summary>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(bool isLauncherMode)
    {
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

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

    private void LoadSettings()
    {
        DaphneFullscreen = _settings.DaphneFullscreen;
        DaphneBilinear = _settings.DaphneBilinear;
        DaphneResX = _settings.DaphneResX;
        DaphneResY = _settings.DaphneResY;
        DaphneEnableSound = _settings.DaphneEnableSound;
        DaphneDisableCrosshairs = _settings.DaphneDisableCrosshairs;
        DaphneUseOverlays = _settings.DaphneUseOverlays;
        DaphneShowSettingsBeforeLaunch = _settings.DaphneShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.DaphneFullscreen = DaphneFullscreen;
        _settings.DaphneBilinear = DaphneBilinear;
        _settings.DaphneResX = DaphneResX;
        _settings.DaphneResY = DaphneResY;
        _settings.DaphneEnableSound = DaphneEnableSound;
        _settings.DaphneDisableCrosshairs = DaphneDisableCrosshairs;
        _settings.DaphneUseOverlays = DaphneUseOverlays;
        _settings.DaphneShowSettingsBeforeLaunch = DaphneShowSettingsBeforeLaunch;

        _settings.Save();
    }

    [RelayCommand]
    private void Run()
    {
        SaveSettings();
        ShouldRun = true;
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Save()
    {
        SaveSettings();
        MessageBoxLibrary.DaphnesettingssavedsuccessfullyMessageBox();
        CloseRequested?.Invoke();
    }
}

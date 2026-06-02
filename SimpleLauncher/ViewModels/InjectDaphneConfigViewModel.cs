using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

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

    public InjectDaphneConfigViewModel(SettingsManager settings, bool isLauncherMode)
    {
        _settings = settings;
        IsLauncherMode = isLauncherMode;

        LoadSettings();
    }

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

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

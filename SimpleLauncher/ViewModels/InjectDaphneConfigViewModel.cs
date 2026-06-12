using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Daphne emulator configuration injection window.
/// </summary>
public partial class InjectDaphneConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly ILogErrors _logErrors;

    [ObservableProperty] private bool _daphneFullscreen;
    [ObservableProperty] private bool _daphneBilinear;
    [ObservableProperty] private int _daphneResX;
    [ObservableProperty] private int _daphneResY;
    [ObservableProperty] private bool _daphneEnableSound;
    [ObservableProperty] private bool _daphneDisableCrosshairs;
    [ObservableProperty] private bool _daphneUseOverlays;
    [ObservableProperty] private bool _daphneShowSettingsBeforeLaunch;

    public InjectDaphneConfigViewModel(SettingsManager settings, IMessageBoxLibraryService messageBox, ILogErrors logErrors)
    {
        _settings = settings;
        _messageBox = messageBox;
        _logErrors = logErrors;
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

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke();

    private void LoadSettings()
    {
        DaphneFullscreen = _settings.Daphne.Fullscreen;
        DaphneBilinear = _settings.Daphne.Bilinear;
        DaphneResX = _settings.Daphne.ResX;
        DaphneResY = _settings.Daphne.ResY;
        DaphneEnableSound = _settings.Daphne.EnableSound;
        DaphneDisableCrosshairs = _settings.Daphne.DisableCrosshairs;
        DaphneUseOverlays = _settings.Daphne.UseOverlays;
        DaphneShowSettingsBeforeLaunch = _settings.Daphne.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Daphne.Fullscreen = DaphneFullscreen;
        _settings.Daphne.Bilinear = DaphneBilinear;
        _settings.Daphne.ResX = DaphneResX;
        _settings.Daphne.ResY = DaphneResY;
        _settings.Daphne.EnableSound = DaphneEnableSound;
        _settings.Daphne.DisableCrosshairs = DaphneDisableCrosshairs;
        _settings.Daphne.UseOverlays = DaphneUseOverlays;
        _settings.Daphne.ShowSettingsBeforeLaunch = DaphneShowSettingsBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        try
        {
            SaveSettings();
            ShouldRun = true;
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            ShouldRun = false;
            _logErrors.LogAndForget(ex, "Error saving Daphne configuration.");
            await _messageBox.ErrorMessageBoxAsync();
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            SaveSettings();
            await _messageBox.DaphnesettingssavedsuccessfullyMessageBoxAsync();
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error saving Daphne configuration.");
            await _messageBox.DaphneConfigurationSaveFailedMessageBoxAsync();
        }
    }
}

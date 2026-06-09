using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class AvaloniaInjectDaphneConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;

    [ObservableProperty] private bool _daphneFullscreen;
    [ObservableProperty] private bool _daphneBilinear;
    [ObservableProperty] private int _daphneResX = 640;
    [ObservableProperty] private int _daphneResY = 480;
    [ObservableProperty] private bool _daphneEnableSound;
    [ObservableProperty] private bool _daphneDisableCrosshairs;
    [ObservableProperty] private bool _daphneUseOverlays;
    [ObservableProperty] private bool _daphneShowSettingsBeforeLaunch;

    public AvaloniaInjectDaphneConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    public void Initialize(bool isLauncherMode)
    {
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;

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
        _settings.SaveAsync();
    }

    [RelayCommand]
    private Task RunAsync()
    {
        SaveSettings();
        ShouldRun = true;
        CloseRequested?.Invoke();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        await _messageBox.SettingsSavedSuccessfullyMessageBox();
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}

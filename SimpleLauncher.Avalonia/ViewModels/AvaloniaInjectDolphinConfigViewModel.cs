using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectDolphinConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _gfxBackend = "Vulkan";
    [ObservableProperty] private bool _dspThread;
    [ObservableProperty] private bool _wiimoteContinuousScanning;
    [ObservableProperty] private bool _wiimoteEnableSpeaker;
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectDolphinConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    public void Initialize(string? emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    public List<string> GfxBackendOptions { get; } = ["Vulkan", "D3D12", "D3D11", "OpenGL", "Software Renderer"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        GfxBackend = _settings.Dolphin.GfxBackend;
        DspThread = _settings.Dolphin.DspThread;
        WiimoteContinuousScanning = _settings.Dolphin.WiimoteContinuousScanning;
        WiimoteEnableSpeaker = _settings.Dolphin.WiimoteEnableSpeaker;
        ShowBeforeLaunch = _settings.Dolphin.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Dolphin.GfxBackend = GfxBackend;
        _settings.Dolphin.DspThread = DspThread;
        _settings.Dolphin.WiimoteContinuousScanning = WiimoteContinuousScanning;
        _settings.Dolphin.WiimoteEnableSpeaker = WiimoteEnableSpeaker;
        _settings.Dolphin.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.SaveAsync();
    }

    private async Task<string?> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

        await _messageBox.DolphinEmulatorNotFoundMessageBox();
        var path = RequestEmulatorPath?.Invoke();
        if (!string.IsNullOrEmpty(path))
        {
            _emulatorPath = path;
        }

        return _emulatorPath;
    }

    private async Task InjectConfigAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_emulatorPath)) return;

            DolphinConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.FailedToInjectDolphinConfigurationMessageBox();
            _logErrors.LogAndForget(ex, "Error in method InjectConfigAsync");
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        var path = await EnsureEmulatorPathAsync();
        if (string.IsNullOrEmpty(path)) return;

        await InjectConfigAsync();
        ShouldRun = true;
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        var path = await EnsureEmulatorPathAsync();
        if (string.IsNullOrEmpty(path)) return;

        await InjectConfigAsync();
        await _messageBox.SettingsSavedSuccessfullyMessageBox();
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}

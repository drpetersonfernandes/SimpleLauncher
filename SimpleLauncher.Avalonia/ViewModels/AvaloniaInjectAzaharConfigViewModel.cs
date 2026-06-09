using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectAzaharConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _graphicsApi = "0";
    [ObservableProperty] private string _resolution = "1";
    [ObservableProperty] private string _layout = "0";
    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _asyncShader;
    [ObservableProperty] private bool _isNew3Ds;
    [ObservableProperty] private int _volume = 100;
    [ObservableProperty] private bool _showBeforeLaunch;
    [ObservableProperty] private bool _audioStretching;

    public AvaloniaInjectAzaharConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        GraphicsApi = _settings.Azahar.GraphicsApi.ToString();
        Resolution = _settings.Azahar.ResolutionFactor.ToString();
        Layout = _settings.Azahar.LayoutOption.ToString();
        Fullscreen = _settings.Azahar.Fullscreen;
        Vsync = _settings.Azahar.UseVsync;
        AsyncShader = _settings.Azahar.AsyncShaderCompilation;
        IsNew3Ds = _settings.Azahar.IsNew3Ds;
        Volume = _settings.Azahar.Volume;
        ShowBeforeLaunch = _settings.Azahar.ShowSettingsBeforeLaunch;
        AudioStretching = _settings.Azahar.EnableAudioStretching;
    }

    private void SaveSettings()
    {
        _settings.Azahar.GraphicsApi = int.Parse(GraphicsApi);
        _settings.Azahar.ResolutionFactor = int.Parse(Resolution);
        _settings.Azahar.LayoutOption = int.Parse(Layout);
        _settings.Azahar.Fullscreen = Fullscreen;
        _settings.Azahar.UseVsync = Vsync;
        _settings.Azahar.AsyncShaderCompilation = AsyncShader;
        _settings.Azahar.IsNew3Ds = IsNew3Ds;
        _settings.Azahar.Volume = Volume;
        _settings.Azahar.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.Azahar.EnableAudioStretching = AudioStretching;
        _settings.SaveAsync();
    }

    private async Task<string?> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

        await _messageBox.WarningMessageBox("Azahar emulator not found. Please locate the emulator executable.");
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

            AzaharConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox($"Error injecting Azahar configuration: {ex.Message}", "Injection Error");
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

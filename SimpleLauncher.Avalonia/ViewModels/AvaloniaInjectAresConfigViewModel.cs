using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectAresConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _videoDriver = "OpenGL 3.2";
    [ObservableProperty] private bool _exclusive;
    [ObservableProperty] private string _shader = "None";
    [ObservableProperty] private string _multiplier = "1";
    [ObservableProperty] private string _aspectCorrection = "Standard";
    [ObservableProperty] private bool _mute;
    [ObservableProperty] private double _volume = 1.0;
    [ObservableProperty] private bool _fastBoot;
    [ObservableProperty] private bool _rewind;
    [ObservableProperty] private bool _runAhead;
    [ObservableProperty] private bool _autoSaveMemory;
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectAresConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> VideoDriverOptions { get; } = ["OpenGL 3.2", "Vulkan", "Direct3D 11", "Direct3D 12"];
    public List<string> ShaderOptions { get; } = ["None", "Blur"];
    public List<string> MultiplierOptions { get; } = ["1", "2", "3", "4", "5"];
    public List<string> AspectCorrectionOptions { get; } = ["Standard", "Center", "Scale", "Stretch"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        VideoDriver = _settings.Ares.VideoDriver;
        Exclusive = _settings.Ares.Exclusive;
        Shader = _settings.Ares.Shader;
        Multiplier = _settings.Ares.Multiplier.ToString(CultureInfo.InvariantCulture);
        AspectCorrection = _settings.Ares.AspectCorrection;
        Mute = _settings.Ares.Mute;
        Volume = _settings.Ares.Volume;
        FastBoot = _settings.Ares.FastBoot;
        Rewind = _settings.Ares.Rewind;
        RunAhead = _settings.Ares.RunAhead;
        AutoSaveMemory = _settings.Ares.AutoSaveMemory;
        ShowBeforeLaunch = _settings.Ares.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Ares.VideoDriver = VideoDriver;
        _settings.Ares.Exclusive = Exclusive;
        _settings.Ares.Shader = Shader;
        _settings.Ares.Multiplier = int.Parse(Multiplier, CultureInfo.InvariantCulture);
        _settings.Ares.AspectCorrection = AspectCorrection;
        _settings.Ares.Mute = Mute;
        _settings.Ares.Volume = Volume;
        _settings.Ares.FastBoot = FastBoot;
        _settings.Ares.Rewind = Rewind;
        _settings.Ares.RunAhead = RunAhead;
        _settings.Ares.AutoSaveMemory = AutoSaveMemory;
        _settings.Ares.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.SaveAsync();
    }

    private async Task<string?> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

        await _messageBox.AresemulatornotfoundMessageBox();
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

            AresConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.FailedtoinjectAresconfigurationMessageBox();
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

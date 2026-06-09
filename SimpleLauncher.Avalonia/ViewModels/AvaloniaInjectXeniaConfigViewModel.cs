using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectXeniaConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _xeniaGpu = "d3d12";
    [ObservableProperty] private bool _xeniaVsync;
    [ObservableProperty] private bool _xeniaFullscreen;
    [ObservableProperty] private int _xeniaResScaleX = 1;
    [ObservableProperty] private int _xeniaResScaleY = 1;
    [ObservableProperty] private string _xeniaAa = "";
    [ObservableProperty] private string _xeniaScaling = "fsr";
    [ObservableProperty] private string _xeniaReadbackResolve = "none";
    [ObservableProperty] private bool _xeniaGammaSrgb;
    [ObservableProperty] private string _xeniaApu = "xaudio2";
    [ObservableProperty] private bool _xeniaMute;
    [ObservableProperty] private bool _xeniaMountCache;
    [ObservableProperty] private bool _xeniaVibration;
    [ObservableProperty] private bool _xeniaApplyPatches = true;
    [ObservableProperty] private string _xeniaHid = "xinput";
    [ObservableProperty] private int _xeniaUserLanguage = 1;
    [ObservableProperty] private bool _xeniaShowSettingsBeforeLaunch;

    public AvaloniaInjectXeniaConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> GpuOptions { get; } = ["d3d12", "vulkan", "null"];
    public List<string> ResScaleOptions { get; } = ["1", "2", "3"];
    public List<TagOption> AaOptions { get; } = [new("", "None"), new("fxaa", "FXAA"), new("fxaa_extreme", "FXAA Extreme")];
    public List<string> ScalingOptions { get; } = ["fsr", "cas", "bilinear"];
    public List<string> ReadbackOptions { get; } = ["none", "fast", "full"];
    public List<string> ApuOptions { get; } = ["xaudio2", "sdl", "nop", "any"];
    public List<string> HidOptions { get; } = ["xinput", "sdl", "winkey", "any"];

    public List<TagOption> LangOptions { get; } =
    [
        new("1", "English"),
        new("2", "Japanese"),
        new("3", "German"),
        new("4", "French"),
        new("5", "Spanish"),
        new("6", "Italian"),
        new("7", "Korean"),
        new("8", "Chinese"),
        new("9", "Portuguese")
    ];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        XeniaGpu = _settings.Xenia.Gpu;
        XeniaVsync = _settings.Xenia.Vsync;
        XeniaFullscreen = _settings.Xenia.Fullscreen;
        XeniaResScaleX = _settings.Xenia.ResScaleX;
        XeniaResScaleY = _settings.Xenia.ResScaleY;
        XeniaAa = _settings.Xenia.Aa;
        XeniaScaling = _settings.Xenia.Scaling;
        XeniaReadbackResolve = _settings.Xenia.ReadbackResolve;
        XeniaGammaSrgb = _settings.Xenia.GammaSrgb;
        XeniaApu = _settings.Xenia.Apu;
        XeniaMute = _settings.Xenia.Mute;
        XeniaMountCache = _settings.Xenia.MountCache;
        XeniaVibration = _settings.Xenia.Vibration;
        XeniaApplyPatches = _settings.Xenia.ApplyPatches;
        XeniaHid = _settings.Xenia.Hid;
        XeniaUserLanguage = _settings.Xenia.UserLanguage;
        XeniaShowSettingsBeforeLaunch = _settings.Xenia.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Xenia.Gpu = XeniaGpu;
        _settings.Xenia.Vsync = XeniaVsync;
        _settings.Xenia.Fullscreen = XeniaFullscreen;
        _settings.Xenia.ResScaleX = XeniaResScaleX;
        _settings.Xenia.ResScaleY = XeniaResScaleY;
        _settings.Xenia.Aa = XeniaAa;
        _settings.Xenia.Scaling = XeniaScaling;
        _settings.Xenia.ReadbackResolve = XeniaReadbackResolve;
        _settings.Xenia.GammaSrgb = XeniaGammaSrgb;
        _settings.Xenia.Apu = XeniaApu;
        _settings.Xenia.Mute = XeniaMute;
        _settings.Xenia.MountCache = XeniaMountCache;
        _settings.Xenia.Vibration = XeniaVibration;
        _settings.Xenia.ApplyPatches = XeniaApplyPatches;
        _settings.Xenia.Hid = XeniaHid;
        _settings.Xenia.UserLanguage = XeniaUserLanguage;
        _settings.Xenia.ShowSettingsBeforeLaunch = XeniaShowSettingsBeforeLaunch;
        _settings.SaveAsync();
    }

    private string? EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

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

            XeniaConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox(ex.Message, "Error");
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return;

        await InjectConfigAsync();
        ShouldRun = true;
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        var path = EnsureEmulatorPath();
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

public record TagOption(string Tag, string Display);

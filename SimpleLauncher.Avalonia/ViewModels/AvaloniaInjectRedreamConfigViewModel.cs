using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectRedreamConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _redreamCable = "vga";
    [ObservableProperty] private string _redreamBroadcast = "ntsc";
    [ObservableProperty] private bool _redreamVsync;
    [ObservableProperty] private bool _redreamFrameskip;
    [ObservableProperty] private string _redreamAspect = "4:3";
    [ObservableProperty] private int _redreamRes = 1;
    [ObservableProperty] private string _redreamRenderer = "hle_perstrip";
    [ObservableProperty] private string _redreamFullmode = "windowed";
    [ObservableProperty] private int _redreamWidth = 640;
    [ObservableProperty] private int _redreamHeight = 480;
    [ObservableProperty] private string _redreamLanguage = "english";
    [ObservableProperty] private string _redreamRegion = "usa";
    [ObservableProperty] private int _redreamVolume = 100;
    [ObservableProperty] private int _redreamLatency = 2;
    [ObservableProperty] private bool _redreamFramerate;
    [ObservableProperty] private bool _redreamShowSettingsBeforeLaunch;

    public AvaloniaInjectRedreamConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> CableOptions { get; } = ["vga", "composite", "rgb"];
    public List<string> BroadcastOptions { get; } = ["ntsc", "pal", "pal_m", "pal_n"];
    public List<string> AspectOptions { get; } = ["4:3", "16:9", "stretch"];
    public List<string> ResOptions { get; } = ["1", "2", "3", "4", "5", "6", "7", "8"];
    public List<string> RendererOptions { get; } = ["hle_perstrip", "hle_perpixel", "lle"];
    public List<string> FullmodeOptions { get; } = ["windowed", "exclusive fullscreen", "borderless fullscreen"];
    public List<string> WindowSizeOptions { get; } = ["640x480", "800x600", "1024x768", "1280x960", "1024x576", "1280x720", "1600x900", "1920x1080", "2560x1440", "3840x2160", "2560x1080", "3440x1440"];
    public List<string> LanguageOptions { get; } = ["english", "japanese", "german", "french", "spanish", "italian"];
    public List<string> RegionOptions { get; } = ["usa", "japan", "europe"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        RedreamCable = _settings.Redream.Cable;
        RedreamBroadcast = _settings.Redream.Broadcast;
        RedreamVsync = _settings.Redream.Vsync;
        RedreamFrameskip = _settings.Redream.Frameskip;
        RedreamAspect = _settings.Redream.Aspect;
        RedreamRes = _settings.Redream.Res;
        RedreamRenderer = _settings.Redream.Renderer;
        RedreamFullmode = _settings.Redream.Fullmode;
        RedreamWidth = _settings.Redream.Width;
        RedreamHeight = _settings.Redream.Height;
        RedreamLanguage = _settings.Redream.Language;
        RedreamRegion = _settings.Redream.Region;
        RedreamVolume = _settings.Redream.Volume;
        RedreamLatency = _settings.Redream.Latency;
        RedreamFramerate = _settings.Redream.Framerate;
        RedreamShowSettingsBeforeLaunch = _settings.Redream.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Redream.Cable = RedreamCable;
        _settings.Redream.Broadcast = RedreamBroadcast;
        _settings.Redream.Vsync = RedreamVsync;
        _settings.Redream.Frameskip = RedreamFrameskip;
        _settings.Redream.Aspect = RedreamAspect;
        _settings.Redream.Res = RedreamRes;
        _settings.Redream.Renderer = RedreamRenderer;
        _settings.Redream.Fullmode = RedreamFullmode;
        _settings.Redream.Width = RedreamWidth;
        _settings.Redream.Height = RedreamHeight;
        _settings.Redream.Language = RedreamLanguage;
        _settings.Redream.Region = RedreamRegion;
        _settings.Redream.Volume = RedreamVolume;
        _settings.Redream.Latency = RedreamLatency;
        _settings.Redream.Framerate = RedreamFramerate;
        _settings.Redream.ShowSettingsBeforeLaunch = RedreamShowSettingsBeforeLaunch;
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

            RedreamConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox("Emulator not found. Please locate the emulator executable.", "Error");
            _logErrors.LogAndForget(ex, "Error in method InjectConfigAsync");
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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectPcsx2ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private int _pcsx2Renderer = 14;
    [ObservableProperty] private int _pcsx2UpscaleMultiplier = 1;
    [ObservableProperty] private string _pcsx2AspectRatio = "4:3";
    [ObservableProperty] private bool _pcsx2Vsync;
    [ObservableProperty] private bool _pcsx2EnableWidescreenPatches;
    [ObservableProperty] private bool _pcsx2StartFullscreen;
    [ObservableProperty] private bool _pcsx2EnableCheats;
    [ObservableProperty] private int _pcsx2Volume = 100;
    [ObservableProperty] private bool _pcsx2AchievementsEnabled;
    [ObservableProperty] private bool _pcsx2AchievementsHardcore;
    [ObservableProperty] private bool _pcsx2ShowSettingsBeforeLaunch;

    public AvaloniaInjectPcsx2ConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> RendererOptions { get; } = ["14", "13", "12", "15", "11"];
    public List<string> RendererDisplayNames { get; } = ["Vulkan", "Direct3D 12", "Direct3D 11", "OpenGL", "Software"];
    public List<string> UpscaleOptions { get; } = ["1", "2", "3", "4", "5", "6", "8"];
    public List<string> UpscaleDisplayNames { get; } = ["1x (Native)", "2x", "3x", "4x", "5x", "6x", "8x"];
    public List<string> AspectOptions { get; } = ["4:3", "16:9", "Stretch"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        Pcsx2Renderer = _settings.Pcsx2.Renderer;
        Pcsx2UpscaleMultiplier = _settings.Pcsx2.UpscaleMultiplier;
        Pcsx2AspectRatio = _settings.Pcsx2.AspectRatio;
        Pcsx2Vsync = _settings.Pcsx2.Vsync;
        Pcsx2EnableWidescreenPatches = _settings.Pcsx2.EnableWidescreenPatches;
        Pcsx2StartFullscreen = _settings.Pcsx2.StartFullscreen;
        Pcsx2EnableCheats = _settings.Pcsx2.EnableCheats;
        Pcsx2Volume = _settings.Pcsx2.Volume;
        Pcsx2AchievementsEnabled = _settings.Pcsx2.AchievementsEnabled;
        Pcsx2AchievementsHardcore = _settings.Pcsx2.AchievementsHardcore;
        Pcsx2ShowSettingsBeforeLaunch = _settings.Pcsx2.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Pcsx2.Renderer = Pcsx2Renderer;
        _settings.Pcsx2.UpscaleMultiplier = Pcsx2UpscaleMultiplier;
        _settings.Pcsx2.AspectRatio = Pcsx2AspectRatio;
        _settings.Pcsx2.Vsync = Pcsx2Vsync;
        _settings.Pcsx2.EnableWidescreenPatches = Pcsx2EnableWidescreenPatches;
        _settings.Pcsx2.StartFullscreen = Pcsx2StartFullscreen;
        _settings.Pcsx2.EnableCheats = Pcsx2EnableCheats;
        _settings.Pcsx2.Volume = Pcsx2Volume;
        _settings.Pcsx2.AchievementsEnabled = Pcsx2AchievementsEnabled;
        _settings.Pcsx2.AchievementsHardcore = Pcsx2AchievementsHardcore;
        _settings.Pcsx2.ShowSettingsBeforeLaunch = Pcsx2ShowSettingsBeforeLaunch;
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

            Pcsx2ConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox($"Error injecting PCSX2 configuration: {ex.Message}", "Configuration Error");
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

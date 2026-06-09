using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectDuckStationConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private bool _startFullscreen;
    [ObservableProperty] private bool _pauseOnFocusLoss;
    [ObservableProperty] private bool _saveStateOnExit;
    [ObservableProperty] private bool _rewindEnable;
    [ObservableProperty] private int _runaheadFrameCount;
    [ObservableProperty] private string _renderer = "Automatic";
    [ObservableProperty] private int _resolutionScale = 1;
    [ObservableProperty] private string _textureFilter = "Nearest";
    [ObservableProperty] private string _aspectRatio = "Auto";
    [ObservableProperty] private bool _widescreenHack;
    [ObservableProperty] private bool _pgxpEnable;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _outputMuted;
    [ObservableProperty] private int _outputVolume = 100;
    [ObservableProperty] private bool _showSettingsBeforeLaunch;

    public AvaloniaInjectDuckStationConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> RendererOptions { get; } = ["Automatic", "Vulkan", "Direct3D 11", "Direct3D 12", "OpenGL", "Software"];

    public record TagOption(string Tag, string Display);

    public List<TagOption> ResolutionScaleOptions { get; } =
    [
        new("1", "1x (Native)"),
        new("2", "2x (720p)"),
        new("3", "3x (1080p)"),
        new("4", "4x (1440p)"),
        new("5", "5x (1620p)"),
        new("6", "6x (4K)"),
        new("7", "7x (5K)"),
        new("8", "8x (8K)")
    ];

    public List<string> TextureFilterOptions { get; } = ["Nearest", "Bilinear", "Bilinear(Bilinear)"];
    public List<string> AspectRatioOptions { get; } = ["Auto", "4:3", "16:9", "16:10", "Crop"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        StartFullscreen = _settings.DuckStation.StartFullscreen;
        PauseOnFocusLoss = _settings.DuckStation.PauseOnFocusLoss;
        SaveStateOnExit = _settings.DuckStation.SaveStateOnExit;
        RewindEnable = _settings.DuckStation.RewindEnable;
        RunaheadFrameCount = _settings.DuckStation.RunaheadFrameCount;
        Renderer = _settings.DuckStation.Renderer;
        ResolutionScale = _settings.DuckStation.ResolutionScale;
        TextureFilter = _settings.DuckStation.TextureFilter;
        AspectRatio = _settings.DuckStation.AspectRatio;
        WidescreenHack = _settings.DuckStation.WidescreenHack;
        PgxpEnable = _settings.DuckStation.PgxpEnable;
        Vsync = _settings.DuckStation.Vsync;
        OutputMuted = _settings.DuckStation.OutputMuted;
        OutputVolume = _settings.DuckStation.OutputVolume;
        ShowSettingsBeforeLaunch = _settings.DuckStation.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.DuckStation.StartFullscreen = StartFullscreen;
        _settings.DuckStation.PauseOnFocusLoss = PauseOnFocusLoss;
        _settings.DuckStation.SaveStateOnExit = SaveStateOnExit;
        _settings.DuckStation.RewindEnable = RewindEnable;
        _settings.DuckStation.RunaheadFrameCount = RunaheadFrameCount;
        _settings.DuckStation.Renderer = Renderer;
        _settings.DuckStation.ResolutionScale = ResolutionScale;
        _settings.DuckStation.TextureFilter = TextureFilter;
        _settings.DuckStation.AspectRatio = AspectRatio;
        _settings.DuckStation.WidescreenHack = WidescreenHack;
        _settings.DuckStation.PgxpEnable = PgxpEnable;
        _settings.DuckStation.Vsync = Vsync;
        _settings.DuckStation.OutputMuted = OutputMuted;
        _settings.DuckStation.OutputVolume = OutputVolume;
        _settings.DuckStation.ShowSettingsBeforeLaunch = ShowSettingsBeforeLaunch;
        _settings.SaveAsync();
    }

    private async Task<string?> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

        await _messageBox.DuckStationEmulatorNotFoundMessageBox();
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

            DuckStationConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.FailedToInjectDuckStationConfigurationMessageBox();
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

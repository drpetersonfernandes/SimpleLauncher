using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectMameConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _video = "auto";
    [ObservableProperty] private string _bgfxBackend = "auto";
    [ObservableProperty] private string _bgfxScreenChains = "default";
    [ObservableProperty] private bool _filter;
    [ObservableProperty] private bool _autoframeskip;
    [ObservableProperty] private bool _cheat;
    [ObservableProperty] private bool _rewind;
    [ObservableProperty] private bool _nvramSave;
    [ObservableProperty] private bool _window;
    [ObservableProperty] private bool _maximize;
    [ObservableProperty] private bool _keepAspect;
    [ObservableProperty] private bool _skipGameInfo;
    [ObservableProperty] private bool _autosave;
    [ObservableProperty] private bool _confirmQuit;
    [ObservableProperty] private bool _joystick;
    [ObservableProperty] private bool _showSettingsBeforeLaunch;

    public AvaloniaInjectMameConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> VideoOptions { get; } = ["auto", "d3d", "opengl", "bgfx", "gdi"];
    public List<string> BgfxBackendOptions { get; } = ["auto", "d3d11", "vulkan", "opengl"];
    public List<string> BgfxChainsOptions { get; } = ["default", "crt-geom"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        Video = _settings.Mame.Video;
        BgfxBackend = _settings.Mame.BgfxBackend;
        BgfxScreenChains = _settings.Mame.BgfxScreenChains;
        Filter = _settings.Mame.Filter;
        Autoframeskip = _settings.Mame.Autoframeskip;
        Cheat = _settings.Mame.Cheat;
        Rewind = _settings.Mame.Rewind;
        NvramSave = _settings.Mame.NvramSave;
        Window = _settings.Mame.Window;
        Maximize = _settings.Mame.Maximize;
        KeepAspect = _settings.Mame.KeepAspect;
        SkipGameInfo = _settings.Mame.SkipGameInfo;
        Autosave = _settings.Mame.Autosave;
        ConfirmQuit = _settings.Mame.ConfirmQuit;
        Joystick = _settings.Mame.Joystick;
        ShowSettingsBeforeLaunch = _settings.Mame.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Mame.Video = Video;
        _settings.Mame.BgfxBackend = BgfxBackend;
        _settings.Mame.BgfxScreenChains = BgfxScreenChains;
        _settings.Mame.Filter = Filter;
        _settings.Mame.Autoframeskip = Autoframeskip;
        _settings.Mame.Cheat = Cheat;
        _settings.Mame.Rewind = Rewind;
        _settings.Mame.NvramSave = NvramSave;
        _settings.Mame.Window = Window;
        _settings.Mame.Maximize = Maximize;
        _settings.Mame.KeepAspect = KeepAspect;
        _settings.Mame.SkipGameInfo = SkipGameInfo;
        _settings.Mame.Autosave = Autosave;
        _settings.Mame.ConfirmQuit = ConfirmQuit;
        _settings.Mame.Joystick = Joystick;
        _settings.Mame.ShowSettingsBeforeLaunch = ShowSettingsBeforeLaunch;
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

            MameConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox("Emulator not found. Please locate the emulator executable.", "Emulator Not Found");
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

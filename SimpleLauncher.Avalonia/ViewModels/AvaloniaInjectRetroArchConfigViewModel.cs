using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectRetroArchConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _videoDriver = "gl";
    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _threadedVideo;
    [ObservableProperty] private bool _bilinear;
    [ObservableProperty] private string _aspectRatioIndex = "22";
    [ObservableProperty] private bool _scaleInteger;
    [ObservableProperty] private bool _shaderEnable;
    [ObservableProperty] private bool _hardSync;
    [ObservableProperty] private bool _audioEnable = true;
    [ObservableProperty] private bool _audioMute;
    [ObservableProperty] private bool _pauseNonActive;
    [ObservableProperty] private bool _saveOnExit = true;
    [ObservableProperty] private bool _autoSaveState;
    [ObservableProperty] private bool _autoLoadState;
    [ObservableProperty] private bool _rewind;
    [ObservableProperty] private bool _runAhead;
    [ObservableProperty] private string _menuDriver = "ozone";
    [ObservableProperty] private bool _showAdvancedSettings;
    [ObservableProperty] private bool _cheevosEnable;
    [ObservableProperty] private bool _cheevosHardcore;
    [ObservableProperty] private bool _discordAllow;
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectRetroArchConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> VideoDriverOptions { get; } = ["gl", "vulkan", "d3d11", "d3d12", "d3d10", "sdl2"];
    public List<string> AspectRatioIndexDisplayOptions { get; } = ["Core Provided", "4:3", "16:9", "16:10"];
    public List<string> AspectRatioIndexTags { get; } = ["22", "0", "1", "2"];
    public List<string> MenuDriverOptions { get; } = ["ozone", "xmb", "rgui", "glui"];

    public string AspectRatioIndexDisplay
    {
        get
        {
            var index = AspectRatioIndexTags.IndexOf(AspectRatioIndex);
            return index >= 0 ? AspectRatioIndexDisplayOptions[index] : AspectRatioIndexDisplayOptions[0];
        }
        set
        {
            var index = AspectRatioIndexDisplayOptions.IndexOf(value);
            AspectRatioIndex = index >= 0 ? AspectRatioIndexTags[index] : AspectRatioIndexTags[0];
        }
    }

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        VideoDriver = _settings.RetroArch.VideoDriver;
        Fullscreen = _settings.RetroArch.Fullscreen;
        Vsync = _settings.RetroArch.Vsync;
        ThreadedVideo = _settings.RetroArch.ThreadedVideo;
        Bilinear = _settings.RetroArch.Bilinear;
        AspectRatioIndex = _settings.RetroArch.AspectRatioIndex;
        ScaleInteger = _settings.RetroArch.ScaleInteger;
        ShaderEnable = _settings.RetroArch.ShaderEnable;
        HardSync = _settings.RetroArch.HardSync;
        AudioEnable = _settings.RetroArch.AudioEnable;
        AudioMute = _settings.RetroArch.AudioMute;
        PauseNonActive = _settings.RetroArch.PauseNonActive;
        SaveOnExit = _settings.RetroArch.SaveOnExit;
        AutoSaveState = _settings.RetroArch.AutoSaveState;
        AutoLoadState = _settings.RetroArch.AutoLoadState;
        Rewind = _settings.RetroArch.Rewind;
        RunAhead = _settings.RetroArch.RunAhead;
        MenuDriver = _settings.RetroArch.MenuDriver;
        ShowAdvancedSettings = _settings.RetroArch.ShowAdvancedSettings;
        CheevosEnable = _settings.RetroArch.CheevosEnable;
        CheevosHardcore = _settings.RetroArch.CheevosHardcore;
        DiscordAllow = _settings.RetroArch.DiscordAllow;
        ShowBeforeLaunch = _settings.RetroArch.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.RetroArch.VideoDriver = VideoDriver;
        _settings.RetroArch.Fullscreen = Fullscreen;
        _settings.RetroArch.Vsync = Vsync;
        _settings.RetroArch.ThreadedVideo = ThreadedVideo;
        _settings.RetroArch.Bilinear = Bilinear;
        _settings.RetroArch.AspectRatioIndex = AspectRatioIndex;
        _settings.RetroArch.ScaleInteger = ScaleInteger;
        _settings.RetroArch.ShaderEnable = ShaderEnable;
        _settings.RetroArch.HardSync = HardSync;
        _settings.RetroArch.AudioEnable = AudioEnable;
        _settings.RetroArch.AudioMute = AudioMute;
        _settings.RetroArch.PauseNonActive = PauseNonActive;
        _settings.RetroArch.SaveOnExit = SaveOnExit;
        _settings.RetroArch.AutoSaveState = AutoSaveState;
        _settings.RetroArch.AutoLoadState = AutoLoadState;
        _settings.RetroArch.Rewind = Rewind;
        _settings.RetroArch.RunAhead = RunAhead;
        _settings.RetroArch.MenuDriver = MenuDriver;
        _settings.RetroArch.ShowAdvancedSettings = ShowAdvancedSettings;
        _settings.RetroArch.CheevosEnable = CheevosEnable;
        _settings.RetroArch.CheevosHardcore = CheevosHardcore;
        _settings.RetroArch.DiscordAllow = DiscordAllow;
        _settings.RetroArch.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
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

            RetroArchConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
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

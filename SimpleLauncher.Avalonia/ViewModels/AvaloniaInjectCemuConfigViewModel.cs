using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectCemuConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private string _graphicApi = "0";
    [ObservableProperty] private string _vsync = "0";
    [ObservableProperty] private bool _asyncCompile;
    [ObservableProperty] private int _volume = 100;
    [ObservableProperty] private bool _discord;
    [ObservableProperty] private string _language = "0";
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectCemuConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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
        Fullscreen = _settings.Cemu.Fullscreen;
        GraphicApi = _settings.Cemu.GraphicApi.ToString();
        Vsync = _settings.Cemu.Vsync.ToString();
        AsyncCompile = _settings.Cemu.AsyncCompile;
        Volume = _settings.Cemu.TvVolume;
        Discord = _settings.Cemu.DiscordPresence;
        Language = _settings.Cemu.ConsoleLanguage.ToString();
        ShowBeforeLaunch = _settings.Cemu.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Cemu.Fullscreen = Fullscreen;
        _settings.Cemu.GraphicApi = int.Parse(GraphicApi);
        _settings.Cemu.Vsync = int.Parse(Vsync);
        _settings.Cemu.AsyncCompile = AsyncCompile;
        _settings.Cemu.TvVolume = Volume;
        _settings.Cemu.DiscordPresence = Discord;
        _settings.Cemu.ConsoleLanguage = int.Parse(Language);
        _settings.Cemu.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.SaveAsync();
    }

    private async Task<string?> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

        await _messageBox.CemuemulatornotfoundMessageBox();
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

            CemuConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox($"Error injecting Cemu configuration: {ex.Message}", "Injection Error");
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

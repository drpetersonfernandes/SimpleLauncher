using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

public partial class InjectPcsx2ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    [ObservableProperty] private int _pcsx2Renderer;
    [ObservableProperty] private int _pcsx2UpscaleMultiplier;
    [ObservableProperty] private string _pcsx2AspectRatio;
    [ObservableProperty] private bool _pcsx2Vsync;
    [ObservableProperty] private bool _pcsx2EnableWidescreenPatches;
    [ObservableProperty] private bool _pcsx2StartFullscreen;
    [ObservableProperty] private bool _pcsx2EnableCheats;
    [ObservableProperty] private int _pcsx2Volume;
    [ObservableProperty] private bool _pcsx2AchievementsEnabled;
    [ObservableProperty] private bool _pcsx2AchievementsHardcore;
    [ObservableProperty] private bool _pcsx2ShowSettingsBeforeLaunch;

    public InjectPcsx2ConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public List<string> RendererOptions { get; } = ["14", "13", "12", "15", "11"];
    public List<string> RendererDisplayNames { get; } = ["Vulkan", "Direct3D 12", "Direct3D 11", "OpenGL", "Software"];
    public List<string> UpscaleOptions { get; } = ["1", "2", "3", "4", "5", "6", "8"];
    public List<string> UpscaleDisplayNames { get; } = ["1x (Native)", "2x", "3x", "4x", "5x", "6x", "8x"];
    public List<string> AspectOptions { get; } = ["4:3", "16:9", "Stretch"];

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        Pcsx2Renderer = _settings.Pcsx2Renderer;
        Pcsx2UpscaleMultiplier = _settings.Pcsx2UpscaleMultiplier;
        Pcsx2AspectRatio = _settings.Pcsx2AspectRatio;
        Pcsx2Vsync = _settings.Pcsx2Vsync;
        Pcsx2EnableWidescreenPatches = _settings.Pcsx2EnableWidescreenPatches;
        Pcsx2StartFullscreen = _settings.Pcsx2StartFullscreen;
        Pcsx2EnableCheats = _settings.Pcsx2EnableCheats;
        Pcsx2Volume = _settings.Pcsx2Volume;
        Pcsx2AchievementsEnabled = _settings.Pcsx2AchievementsEnabled;
        Pcsx2AchievementsHardcore = _settings.Pcsx2AchievementsHardcore;
        Pcsx2ShowSettingsBeforeLaunch = _settings.Pcsx2ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Pcsx2Renderer = Pcsx2Renderer;
        _settings.Pcsx2UpscaleMultiplier = Pcsx2UpscaleMultiplier;
        _settings.Pcsx2AspectRatio = Pcsx2AspectRatio;
        _settings.Pcsx2Vsync = Pcsx2Vsync;
        _settings.Pcsx2EnableWidescreenPatches = Pcsx2EnableWidescreenPatches;
        _settings.Pcsx2StartFullscreen = Pcsx2StartFullscreen;
        _settings.Pcsx2EnableCheats = Pcsx2EnableCheats;
        _settings.Pcsx2Volume = Pcsx2Volume;
        _settings.Pcsx2AchievementsEnabled = Pcsx2AchievementsEnabled;
        _settings.Pcsx2AchievementsHardcore = Pcsx2AchievementsHardcore;
        _settings.Pcsx2ShowSettingsBeforeLaunch = Pcsx2ShowSettingsBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("PCSX2", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        var result = RequestEmulatorPath?.Invoke();
        if (string.IsNullOrEmpty(result)) return null;

        _emulatorPath = result;
        return _emulatorPath;
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            Pcsx2ConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"PCSX2 configuration injection failed for path: {path}");
            return false;
        }
    }

    [RelayCommand]
    private void Run()
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                ShouldRun = true;
                CloseRequested?.Invoke();
            }
            else
            {
                MessageBoxLibrary.InjectionFailedGenericMessageBox();
                CloseRequested?.Invoke();
                ShouldRun = true;
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectPcsx2ConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
            ShouldRun = true;
        }
    }

    [RelayCommand]
    private void Save()
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                MessageBoxLibrary.Pcsx2SettingssavedMessageBox();
                CloseRequested?.Invoke();
            }
            else
            {
                MessageBoxLibrary.InjectionFailedGenericMessageBox();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectPcsx2ConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

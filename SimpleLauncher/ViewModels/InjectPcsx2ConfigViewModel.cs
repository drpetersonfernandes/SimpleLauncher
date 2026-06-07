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

/// <summary>
/// ViewModel for the PCSX2 emulator configuration injection window.
/// </summary>
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

    public InjectPcsx2ConfigViewModel(SettingsManager settings)
    {
        _settings = settings;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the PCSX2 emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available renderer ID options for PCSX2.
    /// </summary>
    public List<string> RendererOptions { get; } = ["14", "13", "12", "15", "11"];

    /// <summary>
    /// Display names corresponding to the renderer options for PCSX2.
    /// </summary>
    public List<string> RendererDisplayNames { get; } = ["Vulkan", "Direct3D 12", "Direct3D 11", "OpenGL", "Software"];

    /// <summary>
    /// Available upscale multiplier options for PCSX2.
    /// </summary>
    public List<string> UpscaleOptions { get; } = ["1", "2", "3", "4", "5", "6", "8"];

    /// <summary>
    /// Display names corresponding to the upscale multiplier options for PCSX2.
    /// </summary>
    public List<string> UpscaleDisplayNames { get; } = ["1x (Native)", "2x", "3x", "4x", "5x", "6x", "8x"];

    /// <summary>
    /// Available aspect ratio options for PCSX2.
    /// </summary>
    public List<string> AspectOptions { get; } = ["4:3", "16:9", "Stretch"];

    /// <summary>
    /// Gets whether the configuration is being injected from launcher mode.
    /// </summary>
    public bool IsLauncherMode { get; private set; }

    /// <summary>
    /// Gets whether the emulator should be launched after configuration injection.
    /// </summary>
    public bool ShouldRun { get; private set; }

    /// <summary>
    /// Raised when the window should be closed.
    /// </summary>
    public event Action CloseRequested;

    /// <summary>
    /// Requests the user to provide the emulator executable path.
    /// </summary>
    public event Func<string> RequestEmulatorPath;

    /// <summary>
    /// Gets the owner window for dialog display.
    /// </summary>
    public event Func<Window> GetOwnerWindow;

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
            _logErrors.LogAndForget(ex, $"PCSX2 configuration injection failed for path: {path}");
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

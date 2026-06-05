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
/// ViewModel for the Redream emulator configuration injection window.
/// </summary>
public partial class InjectRedreamConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    [ObservableProperty] private string _redreamCable;
    [ObservableProperty] private string _redreamBroadcast;
    [ObservableProperty] private bool _redreamVsync;
    [ObservableProperty] private bool _redreamFrameskip;
    [ObservableProperty] private string _redreamAspect;
    [ObservableProperty] private int _redreamRes;
    [ObservableProperty] private string _redreamRenderer;
    [ObservableProperty] private string _redreamFullmode;
    [ObservableProperty] private int _redreamWidth;
    [ObservableProperty] private int _redreamHeight;
    [ObservableProperty] private string _redreamLanguage;
    [ObservableProperty] private string _redreamRegion;
    [ObservableProperty] private int _redreamVolume;
    [ObservableProperty] private int _redreamLatency;
    [ObservableProperty] private bool _redreamFramerate;
    [ObservableProperty] private bool _redreamShowSettingsBeforeLaunch;

    public InjectRedreamConfigViewModel(SettingsManager settings)
    {
        _settings = settings;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Redream emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available video cable options for Redream.
    /// </summary>
    public List<string> CableOptions { get; } = ["vga", "composite", "rgb"];

    /// <summary>
    /// Available broadcast standard options for Redream.
    /// </summary>
    public List<string> BroadcastOptions { get; } = ["ntsc", "pal", "pal_m", "pal_n"];

    /// <summary>
    /// Available aspect ratio options for Redream.
    /// </summary>
    public List<string> AspectOptions { get; } = ["4:3", "16:9", "stretch"];

    /// <summary>
    /// Available internal resolution options for Redream.
    /// </summary>
    public List<string> ResOptions { get; } = ["1", "2", "3", "4", "5", "6", "7", "8"];

    /// <summary>
    /// Available renderer options for Redream.
    /// </summary>
    public List<string> RendererOptions { get; } = ["hle_perstrip", "hle_perpixel", "lle"];

    /// <summary>
    /// Available fullscreen mode options for Redream.
    /// </summary>
    public List<string> FullmodeOptions { get; } = ["windowed", "exclusive fullscreen", "borderless fullscreen"];

    /// <summary>
    /// Tags corresponding to the fullscreen mode options for Redream.
    /// </summary>
    public List<string> FullmodeTags { get; } = ["windowed", "exclusive fullscreen", "borderless fullscreen"];

    /// <summary>
    /// Available window size options for Redream.
    /// </summary>
    public List<string> WindowSizeOptions { get; } = ["640x480", "800x600", "1024x768", "1280x960", "1024x576", "1280x720", "1600x900", "1920x1080", "2560x1440", "3840x2160", "2560x1080", "3440x1440"];

    /// <summary>
    /// Available language options for Redream.
    /// </summary>
    public List<string> LanguageOptions { get; } = ["english", "japanese", "german", "french", "spanish", "italian"];

    /// <summary>
    /// Available region options for Redream.
    /// </summary>
    public List<string> RegionOptions { get; } = ["usa", "japan", "europe"];

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
        RedreamCable = _settings.RedreamCable;
        RedreamBroadcast = _settings.RedreamBroadcast;
        RedreamVsync = _settings.RedreamVsync;
        RedreamFrameskip = _settings.RedreamFrameskip;
        RedreamAspect = _settings.RedreamAspect;
        RedreamRes = _settings.RedreamRes;
        RedreamRenderer = _settings.RedreamRenderer;
        RedreamFullmode = _settings.RedreamFullmode;
        RedreamWidth = _settings.RedreamWidth;
        RedreamHeight = _settings.RedreamHeight;
        RedreamLanguage = _settings.RedreamLanguage;
        RedreamRegion = _settings.RedreamRegion;
        RedreamVolume = _settings.RedreamVolume;
        RedreamLatency = _settings.RedreamLatency;
        RedreamFramerate = _settings.RedreamFramerate;
        RedreamShowSettingsBeforeLaunch = _settings.RedreamShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.RedreamCable = RedreamCable;
        _settings.RedreamBroadcast = RedreamBroadcast;
        _settings.RedreamVsync = RedreamVsync;
        _settings.RedreamFrameskip = RedreamFrameskip;
        _settings.RedreamAspect = RedreamAspect;
        _settings.RedreamRes = RedreamRes;
        _settings.RedreamRenderer = RedreamRenderer;
        _settings.RedreamFullmode = RedreamFullmode;
        _settings.RedreamWidth = RedreamWidth;
        _settings.RedreamHeight = RedreamHeight;
        _settings.RedreamLanguage = RedreamLanguage;
        _settings.RedreamRegion = RedreamRegion;
        _settings.RedreamVolume = RedreamVolume;
        _settings.RedreamLatency = RedreamLatency;
        _settings.RedreamFramerate = RedreamFramerate;
        _settings.RedreamShowSettingsBeforeLaunch = RedreamShowSettingsBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Redream", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.ReDreamEmulatorPathNotFoundMessageBox();

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
            RedreamConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Redream configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRedreamConfigWindow));
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
                MessageBoxLibrary.ReDreamConfigurationInjectedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRedreamConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Xenia emulator configuration injection window.
/// </summary>
public partial class InjectXeniaConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private string _xeniaGpu;
    [ObservableProperty] private bool _xeniaVsync;
    [ObservableProperty] private bool _xeniaFullscreen;
    [ObservableProperty] private int _xeniaResScaleX;
    [ObservableProperty] private int _xeniaResScaleY;
    [ObservableProperty] private string _xeniaAa;
    [ObservableProperty] private string _xeniaScaling;
    [ObservableProperty] private string _xeniaReadbackResolve;
    [ObservableProperty] private bool _xeniaGammaSrgb;
    [ObservableProperty] private string _xeniaApu;
    [ObservableProperty] private bool _xeniaMute;
    [ObservableProperty] private bool _xeniaMountCache;
    [ObservableProperty] private bool _xeniaVibration;
    [ObservableProperty] private bool _xeniaApplyPatches;
    [ObservableProperty] private string _xeniaHid;
    [ObservableProperty] private int _xeniaUserLanguage;
    [ObservableProperty] private bool _xeniaShowSettingsBeforeLaunch;

    public InjectXeniaConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Xenia emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available GPU backend options for Xenia.
    /// </summary>
    public List<string> GpuOptions { get; } = ["d3d12", "vulkan", "null"];

    /// <summary>
    /// Available resolution scale options for Xenia.
    /// </summary>
    public List<string> ResScaleOptions { get; } = ["1", "2", "3"];

    /// <summary>
    /// Available anti-aliasing options for Xenia.
    /// </summary>
    public List<TagOption> AaOptions { get; } =
    [
        new("", "None"),
        new("fxaa", "FXAA"),
        new("fxaa_extreme", "FXAA Extreme")
    ];

    /// <summary>
    /// Available scaling options for Xenia.
    /// </summary>
    public List<string> ScalingOptions { get; } = ["fsr", "cas", "bilinear"];

    /// <summary>
    /// Available readback resolve options for Xenia.
    /// </summary>
    public List<string> ReadbackOptions { get; } = ["none", "fast", "full"];

    /// <summary>
    /// Available APU (audio processing unit) options for Xenia.
    /// </summary>
    public List<string> ApuOptions { get; } = ["xaudio2", "sdl", "nop", "any"];

    /// <summary>
    /// Available HID (human interface device) input options for Xenia.
    /// </summary>
    public List<string> HidOptions { get; } = ["xinput", "sdl", "winkey", "any"];

    /// <summary>
    /// Available language options for Xenia.
    /// </summary>
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

    private async Task<string> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Xenia", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.XeniaemulatorpathnotfoundMessageBox();

        var result = RequestEmulatorPath?.Invoke();
        if (string.IsNullOrEmpty(result)) return null;

        _emulatorPath = result;
        return _emulatorPath;
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPathAsync().GetAwaiter().GetResult();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            XeniaConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"Xenia configuration injection failed for path: {path}");
            return false;
        }
    }

    [RelayCommand]
    private async Task RunAsync()
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
                await _messageBox.InjectionFailedGenericMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectXeniaConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
            ShouldRun = true;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                await _messageBox.XeniaconfigurationinjectedsuccessfullyMessageBox();
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBox();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectXeniaConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

public record TagOption(string Tag, string Display);

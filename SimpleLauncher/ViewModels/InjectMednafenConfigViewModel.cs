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
/// ViewModel for the Mednafen emulator configuration injection window.
/// </summary>
public partial class InjectMednafenConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    [ObservableProperty] private string _mednafenVideoDriver;
    [ObservableProperty] private string _mednafenStretch;
    [ObservableProperty] private string _mednafenShader;
    [ObservableProperty] private string _mednafenSpecial;
    [ObservableProperty] private bool _mednafenFullscreen;
    [ObservableProperty] private bool _mednafenVsync;
    [ObservableProperty] private bool _mednafenBilinear;
    [ObservableProperty] private int _mednafenScanlines;
    [ObservableProperty] private int _mednafenVolume;
    [ObservableProperty] private bool _mednafenCheats;
    [ObservableProperty] private bool _mednafenRewind;
    [ObservableProperty] private bool _mednafenShowSettingsBeforeLaunch;

    public InjectMednafenConfigViewModel(SettingsManager settings)
    {
        _settings = settings;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Mednafen emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available video driver options for Mednafen.
    /// </summary>
    public List<string> VideoDriverOptions { get; } = ["opengl", "soft", "default"];

    /// <summary>
    /// Available stretch mode options for Mednafen.
    /// </summary>
    public List<string> StretchOptions { get; } = ["0", "full", "aspect", "aspect_int"];

    /// <summary>
    /// Available shader options for Mednafen.
    /// </summary>
    public List<string> ShaderOptions { get; } = ["none", "ip", "ipsharper", "scale2x", "snes_ntsc", "goat"];

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
        MednafenVideoDriver = _settings.Mednafen.VideoDriver;
        MednafenStretch = _settings.Mednafen.Stretch;

        if (!string.IsNullOrEmpty(_settings.Mednafen.Special) && _settings.Mednafen.Special != "none")
        {
            MednafenShader = _settings.Mednafen.Special;
        }
        else
        {
            MednafenShader = _settings.Mednafen.Shader;
        }

        MednafenSpecial = _settings.Mednafen.Special;
        MednafenFullscreen = _settings.Mednafen.Fullscreen;
        MednafenVsync = _settings.Mednafen.Vsync;
        MednafenBilinear = _settings.Mednafen.Bilinear;
        MednafenScanlines = _settings.Mednafen.Scanlines;
        MednafenVolume = _settings.Mednafen.Volume;
        MednafenCheats = _settings.Mednafen.Cheats;
        MednafenRewind = _settings.Mednafen.Rewind;
        MednafenShowSettingsBeforeLaunch = _settings.Mednafen.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Mednafen.VideoDriver = MednafenVideoDriver;
        _settings.Mednafen.Stretch = MednafenStretch;

        if (MednafenShader is "scale2x" or "snes_ntsc")
        {
            _settings.Mednafen.Special = MednafenShader;
            _settings.Mednafen.Shader = "none";
        }
        else
        {
            _settings.Mednafen.Special = "none";
            _settings.Mednafen.Shader = MednafenShader;
        }

        _settings.Mednafen.Fullscreen = MednafenFullscreen;
        _settings.Mednafen.Vsync = MednafenVsync;
        _settings.Mednafen.Bilinear = MednafenBilinear;
        _settings.Mednafen.Scanlines = MednafenScanlines;
        _settings.Mednafen.Volume = MednafenVolume;
        _settings.Mednafen.Cheats = MednafenCheats;
        _settings.Mednafen.Rewind = MednafenRewind;
        _settings.Mednafen.ShowSettingsBeforeLaunch = MednafenShowSettingsBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Mednafen", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.MednafenEmulatorNotFoundMessageBox();

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
            MednafenConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Mednafen configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectMednafenConfigWindow));
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
                MessageBoxLibrary.MednafenConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectMednafenConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Stella emulator configuration injection window.
/// </summary>
public partial class InjectStellaConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _correctAspect;
    [ObservableProperty] private string _videoDriver;
    [ObservableProperty] private string _tvFilter;
    [ObservableProperty] private int _scanlines;
    [ObservableProperty] private bool _audioEnabled;
    [ObservableProperty] private int _audioVolume;
    [ObservableProperty] private bool _timeMachine;
    [ObservableProperty] private bool _confirmExit;
    [ObservableProperty] private bool _showBeforeLaunch;

    public InjectStellaConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Stella emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available video driver options for Stella.
    /// </summary>
    public List<string> VideoDriverOptions { get; } = ["direct3d", "opengl", "software"];

    /// <summary>
    /// Available TV filter options for Stella.
    /// </summary>
    public List<string> TvFilterOptions { get; } = ["0", "1", "2", "3"];

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
        Fullscreen = _settings.Stella.Fullscreen;
        Vsync = _settings.Stella.Vsync;
        CorrectAspect = _settings.Stella.CorrectAspect;
        VideoDriver = _settings.Stella.VideoDriver;
        TvFilter = _settings.Stella.TvFilter.ToString(CultureInfo.InvariantCulture);
        Scanlines = _settings.Stella.Scanlines;
        AudioEnabled = _settings.Stella.AudioEnabled;
        AudioVolume = _settings.Stella.AudioVolume;
        TimeMachine = _settings.Stella.TimeMachine;
        ConfirmExit = _settings.Stella.ConfirmExit;
        ShowBeforeLaunch = _settings.Stella.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Stella.Fullscreen = Fullscreen;
        _settings.Stella.Vsync = Vsync;
        _settings.Stella.CorrectAspect = CorrectAspect;
        _settings.Stella.VideoDriver = VideoDriver;
        _settings.Stella.TvFilter = int.Parse(TvFilter, CultureInfo.InvariantCulture);
        _settings.Stella.Scanlines = Scanlines;
        _settings.Stella.AudioEnabled = AudioEnabled;
        _settings.Stella.AudioVolume = AudioVolume;
        _settings.Stella.TimeMachine = TimeMachine;
        _settings.Stella.ConfirmExit = ConfirmExit;
        _settings.Stella.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Stella", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.StellaEmulatorNotFoundMessageBox();

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
            StellaConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"Stella configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectStellaConfigWindow));
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
                await _messageBox.StellaConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectStellaConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

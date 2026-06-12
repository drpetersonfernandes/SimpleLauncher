using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Supermodel emulator configuration injection window.
/// </summary>
public partial class InjectSupermodelConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private bool _new3DEngine;
    [ObservableProperty] private bool _quadRendering;
    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _wideScreen;
    [ObservableProperty] private bool _stretch;
    [ObservableProperty] private int _resX;
    [ObservableProperty] private int _resY;
    [ObservableProperty] private int _musicVolume;
    [ObservableProperty] private int _soundVolume;
    [ObservableProperty] private bool _throttle;
    [ObservableProperty] private bool _multiThreaded;
    [ObservableProperty] private string _inputSystem;
    [ObservableProperty] private string _powerPcFrequency;
    [ObservableProperty] private bool _showBeforeLaunch;

    public InjectSupermodelConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Supermodel emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available input system options for Supermodel.
    /// </summary>
    public List<string> InputSystemOptions { get; } = ["xinput", "dinput", "rawinput"];

    /// <summary>
    /// Available PowerPC frequency options for Supermodel.
    /// </summary>
    public List<string> PpcFrequencyOptions { get; } = ["50", "60", "75", "100"];

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

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }

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
        New3DEngine = _settings.Supermodel.New3DEngine;
        QuadRendering = _settings.Supermodel.QuadRendering;
        Fullscreen = _settings.Supermodel.Fullscreen;
        Vsync = _settings.Supermodel.Vsync;
        WideScreen = _settings.Supermodel.WideScreen;
        Stretch = _settings.Supermodel.Stretch;
        ResX = _settings.Supermodel.ResX;
        ResY = _settings.Supermodel.ResY;
        MusicVolume = _settings.Supermodel.MusicVolume;
        SoundVolume = _settings.Supermodel.SoundVolume;
        Throttle = _settings.Supermodel.Throttle;
        MultiThreaded = _settings.Supermodel.MultiThreaded;
        InputSystem = _settings.Supermodel.InputSystem;
        PowerPcFrequency = _settings.Supermodel.PowerPcFrequency.ToString(CultureInfo.InvariantCulture);
        ShowBeforeLaunch = _settings.Supermodel.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Supermodel.New3DEngine = New3DEngine;
        _settings.Supermodel.QuadRendering = QuadRendering;
        _settings.Supermodel.Fullscreen = Fullscreen;
        _settings.Supermodel.Vsync = Vsync;
        _settings.Supermodel.WideScreen = WideScreen;
        _settings.Supermodel.Stretch = Stretch;
        _settings.Supermodel.ResX = ResX;
        _settings.Supermodel.ResY = ResY;
        _settings.Supermodel.MusicVolume = MusicVolume;
        _settings.Supermodel.SoundVolume = SoundVolume;
        _settings.Supermodel.Throttle = Throttle;
        _settings.Supermodel.MultiThreaded = MultiThreaded;
        _settings.Supermodel.InputSystem = InputSystem;
        if (int.TryParse(PowerPcFrequency, CultureInfo.InvariantCulture, out var powerPcFrequency))
        {
            _settings.Supermodel.PowerPcFrequency = powerPcFrequency;
        }

        _settings.Supermodel.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Supermodel", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.SupermodelEmulatorNotFoundMessageBoxAsync();

        var result = RequestEmulatorPath?.Invoke();
        if (string.IsNullOrEmpty(result)) return null;

        _emulatorPath = result;
        return _emulatorPath;
    }

    private async Task<bool> InjectConfigAsync()
    {
        var path = await EnsureEmulatorPathAsync();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            SupermodelConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"Supermodel configuration injection failed for path: {path}");
            return false;
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfigAsync())
            {
                ShouldRun = true;
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectSupermodelConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfigAsync())
            {
                await _messageBox.SupermodelConfigurationSavedSuccessfullyMessageBoxAsync();
                CloseRequested?.Invoke();
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectSupermodelConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

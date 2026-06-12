using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Sega Model 2 emulator configuration injection window.
/// </summary>
public partial class InjectSegaModel2ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private int _resX;
    [ObservableProperty] private int _resY;
    [ObservableProperty] private string _wideScreen;
    [ObservableProperty] private string _fsaa;
    [ObservableProperty] private bool _bilinear;
    [ObservableProperty] private bool _trilinear;
    [ObservableProperty] private bool _filterTilemaps;
    [ObservableProperty] private bool _drawCross;
    [ObservableProperty] private bool _xInput;
    [ObservableProperty] private bool _enableFf;
    [ObservableProperty] private bool _holdGears;
    [ObservableProperty] private bool _useRawInput;
    [ObservableProperty] private bool _showBeforeLaunch;

    public InjectSegaModel2ConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Sega Model 2 emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available widescreen mode options for Sega Model 2.
    /// </summary>
    public List<string> WideScreenOptions { get; } = ["0", "1", "2"];

    /// <summary>
    /// Available full-screen anti-aliasing options for Sega Model 2.
    /// </summary>
    public List<string> FsaaOptions { get; } = ["0", "2", "4", "8"];

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
        ResX = _settings.SegaModel2.ResX;
        ResY = _settings.SegaModel2.ResY;
        WideScreen = _settings.SegaModel2.WideScreen.ToString(CultureInfo.InvariantCulture);
        Fsaa = _settings.SegaModel2.Fsaa.ToString(CultureInfo.InvariantCulture);
        Bilinear = _settings.SegaModel2.Bilinear;
        Trilinear = _settings.SegaModel2.Trilinear;
        FilterTilemaps = _settings.SegaModel2.FilterTilemaps;
        DrawCross = _settings.SegaModel2.DrawCross;
        XInput = _settings.SegaModel2.XInput;
        EnableFf = _settings.SegaModel2.EnableFf;
        HoldGears = _settings.SegaModel2.HoldGears;
        UseRawInput = _settings.SegaModel2.UseRawInput;
        ShowBeforeLaunch = _settings.SegaModel2.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.SegaModel2.ResX = ResX;
        _settings.SegaModel2.ResY = ResY;
        if (int.TryParse(WideScreen, CultureInfo.InvariantCulture, out var wideScreen))
        {
            _settings.SegaModel2.WideScreen = wideScreen;
        }

        if (int.TryParse(Fsaa, CultureInfo.InvariantCulture, out var fsaa))
        {
            _settings.SegaModel2.Fsaa = fsaa;
        }

        _settings.SegaModel2.Bilinear = Bilinear;
        _settings.SegaModel2.Trilinear = Trilinear;
        _settings.SegaModel2.FilterTilemaps = FilterTilemaps;
        _settings.SegaModel2.DrawCross = DrawCross;
        _settings.SegaModel2.XInput = XInput;
        _settings.SegaModel2.EnableFf = EnableFf;
        _settings.SegaModel2.HoldGears = HoldGears;
        _settings.SegaModel2.UseRawInput = UseRawInput;
        _settings.SegaModel2.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("SEGA Model 2", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.SegaModel2EmulatorNotFoundMessageBoxAsync();

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
            SegaModel2ConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"SEGA Model 2 configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectSegaModel2ConfigWindow));
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
                await _messageBox.SegaModel2ConfigurationSavedSuccessfullyMessageBoxAsync();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectSegaModel2ConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

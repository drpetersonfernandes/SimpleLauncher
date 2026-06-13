using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Azahar emulator configuration injection window.
/// </summary>
public partial class InjectAzaharConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private string _graphicsApi;
    [ObservableProperty] private string _resolution;
    [ObservableProperty] private string _layout;
    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private bool _asyncShader;
    [ObservableProperty] private bool _isNew3Ds;
    [ObservableProperty] private int _volume;
    [ObservableProperty] private bool _showBeforeLaunch;
    [ObservableProperty] private bool _audioStretching;

    public InjectAzaharConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Azahar emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

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
        GraphicsApi = _settings.Azahar.GraphicsApi.ToString(CultureInfo.InvariantCulture);
        Resolution = _settings.Azahar.ResolutionFactor.ToString(CultureInfo.InvariantCulture);
        Layout = _settings.Azahar.LayoutOption.ToString(CultureInfo.InvariantCulture);
        Fullscreen = _settings.Azahar.Fullscreen;
        Vsync = _settings.Azahar.UseVsync;
        AsyncShader = _settings.Azahar.AsyncShaderCompilation;
        IsNew3Ds = _settings.Azahar.IsNew3Ds;
        Volume = _settings.Azahar.Volume;
        ShowBeforeLaunch = _settings.Azahar.ShowSettingsBeforeLaunch;
        AudioStretching = _settings.Azahar.EnableAudioStretching;
    }

    private void SaveSettings()
    {
        if (int.TryParse(GraphicsApi, CultureInfo.InvariantCulture, out var graphicsApi))
        {
            _settings.Azahar.GraphicsApi = graphicsApi;
        }

        if (int.TryParse(Resolution, CultureInfo.InvariantCulture, out var resolution))
        {
            _settings.Azahar.ResolutionFactor = resolution;
        }

        if (int.TryParse(Layout, CultureInfo.InvariantCulture, out var layout))
        {
            _settings.Azahar.LayoutOption = layout;
        }

        _settings.Azahar.Fullscreen = Fullscreen;
        _settings.Azahar.UseVsync = Vsync;
        _settings.Azahar.AsyncShaderCompilation = AsyncShader;
        _settings.Azahar.IsNew3Ds = IsNew3Ds;
        _settings.Azahar.Volume = Volume;
        _settings.Azahar.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.Azahar.EnableAudioStretching = AudioStretching;
        _ = _settings.SaveAsync();
    }

    private Task<string> EnsureEmulatorPathAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            {
                return Task.FromResult(_emulatorPath);
            }

            var resolved = EmulatorPathResolver.TryFindEmulatorPath("Azahar", _logErrors);
            if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
            {
                _emulatorPath = resolved;
                return Task.FromResult(_emulatorPath);
            }

            var result = RequestEmulatorPath?.Invoke();
            if (string.IsNullOrEmpty(result)) return Task.FromResult<string>(null);

            _emulatorPath = result;
            return Task.FromResult(_emulatorPath);
        }
        catch (Exception exception)
        {
            return Task.FromException<string>(exception);
        }
    }

    private async Task<bool> InjectConfigAsync()
    {
        var path = await EnsureEmulatorPathAsync();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            AzaharConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (AzaharPermissionException)
        {
            await _messageBox.AzaharConfigurationInjectionPermissionErrorMessageBoxAsync();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, "Azahar injection failed");
            await _messageBox.FailedToSaveAzaharConfigurationMessageBoxAsync();
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
        catch (AzaharPermissionException)
        {
            CloseRequested?.Invoke();
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke();
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAzaharConfigWindow));
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
                await _messageBox.AzaharConfigurationSavedSuccessfullyMessageBoxAsync();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAzaharConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

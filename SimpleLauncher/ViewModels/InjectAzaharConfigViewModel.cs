using System.Globalization;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Azahar emulator configuration injection window.
/// </summary>
public partial class InjectAzaharConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
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

    public InjectAzaharConfigViewModel(SettingsManager settings)
    {
        _settings = settings;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();
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
        _settings.Azahar.GraphicsApi = int.Parse(GraphicsApi, CultureInfo.InvariantCulture);
        _settings.Azahar.ResolutionFactor = int.Parse(Resolution, CultureInfo.InvariantCulture);
        _settings.Azahar.LayoutOption = int.Parse(Layout, CultureInfo.InvariantCulture);
        _settings.Azahar.Fullscreen = Fullscreen;
        _settings.Azahar.UseVsync = Vsync;
        _settings.Azahar.AsyncShaderCompilation = AsyncShader;
        _settings.Azahar.IsNew3Ds = IsNew3Ds;
        _settings.Azahar.Volume = Volume;
        _settings.Azahar.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.Azahar.EnableAudioStretching = AudioStretching;
        _settings.SaveAsync();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Azahar", _logErrors);
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

    private async Task<bool> InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            AzaharConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (AzaharPermissionException)
        {
            await _messageBox.AzaharConfigurationInjectionPermissionErrorMessageBox();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, "Azahar injection failed");
            await _messageBox.FailedToSaveAzaharConfigurationMessageBox();
            return false;
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfig())
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
        catch (AzaharPermissionException)
        {
            CloseRequested?.Invoke();
            ShouldRun = true;
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
            ShouldRun = true;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        try
        {
            if (await InjectConfig())
            {
                await _messageBox.AzaharConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAzaharConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

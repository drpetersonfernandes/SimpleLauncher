using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the Mesen emulator configuration injection window.
/// </summary>
public partial class InjectMesenConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    [ObservableProperty] private bool _fullscreen;
    [ObservableProperty] private bool _vsync;
    [ObservableProperty] private string _aspectRatio;
    [ObservableProperty] private bool _bilinear;
    [ObservableProperty] private string _videoFilter;
    [ObservableProperty] private bool _enableAudio;
    [ObservableProperty] private int _masterVolume;
    [ObservableProperty] private bool _rewind;
    [ObservableProperty] private int _runAhead;
    [ObservableProperty] private bool _pauseInBackground;
    [ObservableProperty] private bool _showBeforeLaunch;

    public InjectMesenConfigViewModel(SettingsManager settings)
    {
        _settings = settings;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Mesen emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available aspect ratio options for Mesen.
    /// </summary>
    public List<string> AspectRatioOptions { get; } = ["NoStretching", "4:3", "16:9", "Auto"];

    /// <summary>
    /// Available video filter options for Mesen.
    /// </summary>
    public List<string> VideoFilterOptions { get; } = ["None", "NTSC", "CRT", "LCD"];

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
        Fullscreen = _settings.Mesen.Fullscreen;
        Vsync = _settings.Mesen.Vsync;
        AspectRatio = _settings.Mesen.AspectRatio;
        Bilinear = _settings.Mesen.Bilinear;
        VideoFilter = _settings.Mesen.VideoFilter;
        EnableAudio = _settings.Mesen.EnableAudio;
        MasterVolume = _settings.Mesen.MasterVolume;
        Rewind = _settings.Mesen.Rewind;
        RunAhead = _settings.Mesen.RunAhead;
        PauseInBackground = _settings.Mesen.PauseInBackground;
        ShowBeforeLaunch = _settings.Mesen.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Mesen.Fullscreen = Fullscreen;
        _settings.Mesen.Vsync = Vsync;
        _settings.Mesen.AspectRatio = AspectRatio;
        _settings.Mesen.Bilinear = Bilinear;
        _settings.Mesen.VideoFilter = VideoFilter;
        _settings.Mesen.EnableAudio = EnableAudio;
        _settings.Mesen.MasterVolume = MasterVolume;
        _settings.Mesen.Rewind = Rewind;
        _settings.Mesen.RunAhead = RunAhead;
        _settings.Mesen.PauseInBackground = PauseInBackground;
        _settings.Mesen.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _settings.SaveAsync();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Mesen", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.MesenEmulatorNotFoundMessageBox();

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
            MesenConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"Mesen configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectMesenConfigWindow));
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
                MessageBoxLibrary.MesenConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectMesenConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

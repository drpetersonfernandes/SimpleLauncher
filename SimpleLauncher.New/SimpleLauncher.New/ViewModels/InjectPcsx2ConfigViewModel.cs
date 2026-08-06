using SimpleLauncher.New.Services.InjectEmulatorConfig;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.New.InjectConfigWindows;

namespace SimpleLauncher.New.ViewModels;

/// <summary>
/// ViewModel for the PCSX2 emulator configuration injection window.
/// </summary>
public partial class InjectPcsx2ConfigViewModel : ObservableObject
{
    private readonly SettingsManagerService _settings;
    private readonly ILogger _logger;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly EmulatorPathResolver _emulatorPathResolver;
    private string _emulatorPath = null!;
    [ObservableProperty] private int _pcsx2Renderer;
    [ObservableProperty] private int _pcsx2UpscaleMultiplier;
    [ObservableProperty] private string _pcsx2AspectRatio = null!;
    [ObservableProperty] private bool _pcsx2Vsync;
    [ObservableProperty] private bool _pcsx2EnableWidescreenPatches;
    [ObservableProperty] private bool _pcsx2StartFullscreen;
    [ObservableProperty] private bool _pcsx2EnableCheats;
    [ObservableProperty] private int _pcsx2Volume;
    [ObservableProperty] private bool _pcsx2AchievementsEnabled;
    [ObservableProperty] private bool _pcsx2AchievementsHardcore;
    [ObservableProperty] private bool _pcsx2ShowSettingsBeforeLaunch;

    /// <summary>Initializes a new instance of the <see cref="InjectPcsx2ConfigViewModel"/>.</summary>
    /// <param name="settings">The settings manager service.</param>
    /// <param name="messageBox">The message box service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="emulatorPathResolver">The service used to resolve the file path of an emulator executable.</param>
    public InjectPcsx2ConfigViewModel(SettingsManagerService settings, IMessageBoxLibraryService messageBox, ILogger logger, EmulatorPathResolver emulatorPathResolver)
    {
        _settings = settings;
        _logger = logger;
        _messageBox = messageBox;
        _emulatorPathResolver = emulatorPathResolver;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the PCSX2 emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string? emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath!;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available renderer ID options for PCSX2.
    /// </summary>
    public IList<string> RendererOptions { get; } = ["14", "13", "12", "15", "11"];

    /// <summary>
    /// Display names corresponding to the renderer options for PCSX2.
    /// </summary>
    public IList<string> RendererDisplayNames { get; } = ["Vulkan", "Direct3D 12", "Direct3D 11", "OpenGL", "Software"];

    /// <summary>
    /// Available upscale multiplier options for PCSX2.
    /// </summary>
    public IList<string> UpscaleOptions { get; } = ["1", "2", "3", "4", "5", "6", "8"];

    /// <summary>
    /// Display names corresponding to the upscale multiplier options for PCSX2.
    /// </summary>
    public IList<string> UpscaleDisplayNames { get; } = ["1x (Native)", "2x", "3x", "4x", "5x", "6x", "8x"];

    /// <summary>
    /// Available aspect ratio options for PCSX2.
    /// </summary>
    public IList<string> AspectOptions { get; } = ["4:3", "16:9", "Stretch"];

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
    public event EventHandler? CloseRequested;

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Requests the user to provide the emulator executable path.
    /// </summary>
    public Func<string?>? RequestEmulatorPath { get; set; }

    /// <summary>
    /// Gets the owner window for dialog display.
    /// </summary>
    public Func<Window>? GetOwnerWindow { get; set; }

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
        _ = _settings.SaveAsync();
    }

    private Task<string?> EnsureEmulatorPathAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            {
                return Task.FromResult<string?>(_emulatorPath);
            }

            var resolved = _emulatorPathResolver.TryFindEmulatorPath("PCSX2", _logger);
            if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
            {
                _emulatorPath = resolved;
                return Task.FromResult<string?>(_emulatorPath);
            }

            var result = RequestEmulatorPath?.Invoke();
            if (string.IsNullOrEmpty(result)) return Task.FromResult<string?>(null);

            _emulatorPath = result;
            return Task.FromResult<string?>(_emulatorPath);
        }
        catch (Exception exception)
        {
            return Task.FromException<string?>(exception);
        }
    }

    private async Task<bool> InjectConfigAsync()
    {
        var path = await EnsureEmulatorPathAsync();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            Pcsx2ConfigurationService.InjectSettings(path, _settings, _logger);
            return true;
        }
        catch (Pcsx2PermissionException)
        {
            await _messageBox.Pcsx2ConfigurationInjectionPermissionErrorMessageBoxAsync();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error(ex, $"PCSX2 configuration injection failed for path: {path}");
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
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Pcsx2PermissionException)
        {
            ShouldRun = true;
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectPcsx2ConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            await InjectionErrorHandler.HandleRunButtonFailure(_logger, ex, emulatorName, _emulatorPath, window, _messageBox);
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
                await _messageBox.Pcsx2SettingssavedMessageBoxAsync();
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Pcsx2PermissionException)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectPcsx2ConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            await InjectionErrorHandler.HandleSaveButtonFailure(_logger, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

using SimpleLauncher.New.Services.InjectEmulatorConfig;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.New.InjectConfigWindows;

namespace SimpleLauncher.New.ViewModels;

/// <summary>
/// ViewModel for the Ares emulator configuration injection window.
/// </summary>
public partial class InjectAresConfigViewModel : ObservableObject
{
    private readonly SettingsManagerService _settings;
    private readonly ILogger _logger;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly EmulatorPathResolver _emulatorPathResolver;
    private string _emulatorPath = "";

    [ObservableProperty] private string _videoDriver = "";
    [ObservableProperty] private bool _exclusive;
    [ObservableProperty] private string _shader = "";
    [ObservableProperty] private string _multiplier = "";
    [ObservableProperty] private string _aspectCorrection = "";
    [ObservableProperty] private bool _mute;
    [ObservableProperty] private double _volume;
    [ObservableProperty] private bool _fastBoot;
    [ObservableProperty] private bool _rewind;
    [ObservableProperty] private bool _runAhead;
    [ObservableProperty] private bool _autoSaveMemory;
    [ObservableProperty] private bool _showBeforeLaunch;

    /// <summary>Initializes a new instance of the <see cref="InjectAresConfigViewModel"/>.</summary>
    /// <param name="settings">The settings manager service.</param>
    /// <param name="messageBox">The message box service.</param>
    /// <param name="logger">The logger instance.</param>
    public InjectAresConfigViewModel(SettingsManagerService settings, IMessageBoxLibraryService messageBox, ILogger logger, EmulatorPathResolver emulatorPathResolver)
    {
        _settings = settings;
        _logger = logger;
        _messageBox = messageBox;
        _emulatorPathResolver = emulatorPathResolver;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the Ares emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string? emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath ?? "";
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available video driver options for Ares.
    /// </summary>
    public IList<string> VideoDriverOptions { get; } = ["OpenGL 3.2", "Vulkan", "Direct3D 11", "Direct3D 12"];

    /// <summary>
    /// Available shader options for Ares.
    /// </summary>
    public IList<string> ShaderOptions { get; } = ["None", "Blur"];

    /// <summary>
    /// Available resolution multiplier options for Ares.
    /// </summary>
    public IList<string> MultiplierOptions { get; } = ["1", "2", "3", "4", "5"];

    /// <summary>
    /// Available aspect correction options for Ares.
    /// </summary>
    public IList<string> AspectCorrectionOptions { get; } = ["Standard", "Center", "Scale", "Stretch"];

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
    public event EventHandler CloseRequested = null!;

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
        VideoDriver = _settings.Ares.VideoDriver;
        Exclusive = _settings.Ares.Exclusive;
        Shader = _settings.Ares.Shader;
        Multiplier = _settings.Ares.Multiplier.ToString(CultureInfo.InvariantCulture);
        AspectCorrection = _settings.Ares.AspectCorrection;
        Mute = _settings.Ares.Mute;
        Volume = _settings.Ares.Volume;
        FastBoot = _settings.Ares.FastBoot;
        Rewind = _settings.Ares.Rewind;
        RunAhead = _settings.Ares.RunAhead;
        AutoSaveMemory = _settings.Ares.AutoSaveMemory;
        ShowBeforeLaunch = _settings.Ares.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Ares.VideoDriver = VideoDriver;
        _settings.Ares.Exclusive = Exclusive;
        _settings.Ares.Shader = Shader;
        if (int.TryParse(Multiplier, CultureInfo.InvariantCulture, out var multiplier))
        {
            _settings.Ares.Multiplier = multiplier;
        }

        _settings.Ares.AspectCorrection = AspectCorrection;
        _settings.Ares.Mute = Mute;
        _settings.Ares.Volume = Volume;
        _settings.Ares.FastBoot = FastBoot;
        _settings.Ares.Rewind = Rewind;
        _settings.Ares.RunAhead = RunAhead;
        _settings.Ares.AutoSaveMemory = AutoSaveMemory;
        _settings.Ares.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    private async Task<string?> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = _emulatorPathResolver.TryFindEmulatorPath("Ares", _logger);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.AresemulatornotfoundMessageBoxAsync();

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
            AresConfigurationService.InjectSettings(path, _settings, _logger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logger.Error(ex, $"Ares configuration injection failed for path: {path}");
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
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAresConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            await InjectionErrorHandler.HandleRunButtonFailure(_logger, ex, emulatorName, _emulatorPath, window!, _messageBox);
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
                await _messageBox.AresConfigurationSavedSuccessfullyMessageBoxAsync();
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                await _messageBox.InjectionFailedGenericMessageBoxAsync();
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAresConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            await InjectionErrorHandler.HandleSaveButtonFailure(_logger, ex, emulatorName, _emulatorPath, window!, _messageBox);
        }
    }
}

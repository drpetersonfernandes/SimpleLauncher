using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the RPCS3 emulator configuration injection window.
/// </summary>
public partial class InjectRpcs3ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string _emulatorPath;

    [ObservableProperty] private string _rpcs3Renderer;
    [ObservableProperty] private string _rpcs3Resolution;
    [ObservableProperty] private string _rpcs3AspectRatio;
    [ObservableProperty] private bool _rpcs3Vsync;
    [ObservableProperty] private int _rpcs3ResolutionScale;
    [ObservableProperty] private int _rpcs3AnisotropicFilter;
    [ObservableProperty] private string _rpcs3PpuDecoder;
    [ObservableProperty] private string _rpcs3SpuDecoder;
    [ObservableProperty] private string _rpcs3AudioRenderer;
    [ObservableProperty] private bool _rpcs3AudioBuffering;
    [ObservableProperty] private bool _rpcs3StartFullscreen;
    [ObservableProperty] private bool _rpcs3ShowSettingsBeforeLaunch;

    public InjectRpcs3ConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    /// <summary>
    /// Initializes the ViewModel with the emulator path and launcher mode.
    /// </summary>
    /// <param name="emulatorPath">The file path to the RPCS3 emulator executable.</param>
    /// <param name="isLauncherMode">Whether the configuration is being injected from launcher mode.</param>
    public void Initialize(string emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    /// <summary>
    /// Available renderer options for RPCS3.
    /// </summary>
    public List<string> RendererOptions { get; } = ["Vulkan", "OpenGL", "Null"];

    /// <summary>
    /// Available resolution options for RPCS3.
    /// </summary>
    public List<string> ResolutionOptions { get; } = ["1280x720", "1920x1080", "2560x1440", "3840x2160"];

    /// <summary>
    /// Available aspect ratio options for RPCS3.
    /// </summary>
    public List<string> AspectRatioOptions { get; } = ["16:9", "4:3", "Auto"];

    /// <summary>
    /// Available resolution scale percentage options for RPCS3.
    /// </summary>
    public List<string> ResolutionScaleOptions { get; } = ["100", "150", "200", "300"];

    /// <summary>
    /// Available anisotropic filtering options for RPCS3.
    /// </summary>
    public List<string> AnisotropicFilterOptions { get; } = ["0", "2", "4", "8", "16"];

    /// <summary>
    /// Available PPU decoder options for RPCS3.
    /// </summary>
    public List<string> PpuDecoderOptions { get; } = ["Recompiler (LLVM)", "Interpreter (static)", "Interpreter (dynamic)"];

    /// <summary>
    /// Available SPU decoder options for RPCS3.
    /// </summary>
    public List<string> SpuDecoderOptions { get; } = ["Recompiler (LLVM)", "Recompiler (ASMJIT)", "Interpreter (static)", "Interpreter (dynamic)"];

    /// <summary>
    /// Available audio renderer options for RPCS3.
    /// </summary>
    public List<string> AudioRendererOptions { get; } = ["Cubeb", "XAudio2", "Null"];

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
        Rpcs3Renderer = _settings.Rpcs3.Renderer;
        Rpcs3Resolution = _settings.Rpcs3.Resolution;
        Rpcs3AspectRatio = _settings.Rpcs3.AspectRatio;
        Rpcs3Vsync = _settings.Rpcs3.Vsync;
        Rpcs3ResolutionScale = _settings.Rpcs3.ResolutionScale;
        Rpcs3AnisotropicFilter = _settings.Rpcs3.AnisotropicFilter;
        Rpcs3PpuDecoder = _settings.Rpcs3.PpuDecoder;
        Rpcs3SpuDecoder = _settings.Rpcs3.SpuDecoder;
        Rpcs3AudioRenderer = _settings.Rpcs3.AudioRenderer;
        Rpcs3AudioBuffering = _settings.Rpcs3.AudioBuffering;
        Rpcs3StartFullscreen = _settings.Rpcs3.StartFullscreen;
        Rpcs3ShowSettingsBeforeLaunch = _settings.Rpcs3.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Rpcs3.Renderer = Rpcs3Renderer;
        _settings.Rpcs3.Resolution = Rpcs3Resolution;
        _settings.Rpcs3.AspectRatio = Rpcs3AspectRatio;
        _settings.Rpcs3.Vsync = Rpcs3Vsync;
        _settings.Rpcs3.ResolutionScale = Rpcs3ResolutionScale;
        _settings.Rpcs3.AnisotropicFilter = Rpcs3AnisotropicFilter;
        _settings.Rpcs3.PpuDecoder = Rpcs3PpuDecoder;
        _settings.Rpcs3.SpuDecoder = Rpcs3SpuDecoder;
        _settings.Rpcs3.AudioRenderer = Rpcs3AudioRenderer;
        _settings.Rpcs3.AudioBuffering = Rpcs3AudioBuffering;
        _settings.Rpcs3.StartFullscreen = Rpcs3StartFullscreen;
        _settings.Rpcs3.ShowSettingsBeforeLaunch = Rpcs3ShowSettingsBeforeLaunch;
        _ = _settings.SaveAsync();
    }

    private async Task<string> EnsureEmulatorPathAsync()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("RPCS3", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        await _messageBox.Rpcs3EmulatorNotFoundPleaseLocateMessageBoxAsync();

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
            Rpcs3ConfigurationService.InjectSettings(path, _settings, _logErrors, _debugLogger);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogAndForget(ex, $"RPCS3 configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRpcs3ConfigWindow));
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
                await _messageBox.Rpcs3ConfigurationSavedSuccessfullyMessageBoxAsync();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRpcs3ConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window, _messageBox);
        }
    }
}

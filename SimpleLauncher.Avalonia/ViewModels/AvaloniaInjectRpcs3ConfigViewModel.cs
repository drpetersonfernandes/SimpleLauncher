using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectRpcs3ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private string _rpcs3Renderer = "Vulkan";
    [ObservableProperty] private string _rpcs3Resolution = "1920x1080";
    [ObservableProperty] private string _rpcs3AspectRatio = "16:9";
    [ObservableProperty] private bool _rpcs3Vsync;
    [ObservableProperty] private int _rpcs3ResolutionScale = 100;
    [ObservableProperty] private int _rpcs3AnisotropicFilter;
    [ObservableProperty] private string _rpcs3PpuDecoder = "Recompiler (LLVM)";
    [ObservableProperty] private string _rpcs3SpuDecoder = "Recompiler (LLVM)";
    [ObservableProperty] private string _rpcs3AudioRenderer = "Cubeb";
    [ObservableProperty] private bool _rpcs3AudioBuffering;
    [ObservableProperty] private bool _rpcs3StartFullscreen;
    [ObservableProperty] private bool _rpcs3ShowSettingsBeforeLaunch;

    public AvaloniaInjectRpcs3ConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _settings = settings;
        _logErrors = logErrors;
        _debugLogger = debugLogger;
        _messageBox = messageBox;
    }

    public void Initialize(string? emulatorPath, bool isLauncherMode)
    {
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        LoadSettings();
    }

    public List<string> RendererOptions { get; } = ["Vulkan", "OpenGL", "Null"];
    public List<string> ResolutionOptions { get; } = ["1280x720", "1920x1080", "2560x1440", "3840x2160"];
    public List<string> AspectRatioOptions { get; } = ["16:9", "4:3", "Auto"];
    public List<string> ResolutionScaleOptions { get; } = ["100", "150", "200", "300"];
    public List<string> AnisotropicFilterOptions { get; } = ["0", "2", "4", "8", "16"];
    public List<string> PpuDecoderOptions { get; } = ["Recompiler (LLVM)", "Interpreter (static)", "Interpreter (dynamic)"];
    public List<string> SpuDecoderOptions { get; } = ["Recompiler (LLVM)", "Recompiler (ASMJIT)", "Interpreter (static)", "Interpreter (dynamic)"];
    public List<string> AudioRendererOptions { get; } = ["Cubeb", "XAudio2", "Null"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

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
        _settings.SaveAsync();
    }

    private string? EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
            return _emulatorPath;

        var path = RequestEmulatorPath?.Invoke();
        if (!string.IsNullOrEmpty(path))
        {
            _emulatorPath = path;
        }

        return _emulatorPath;
    }

    private async Task InjectConfigAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(_emulatorPath)) return;

            Rpcs3ConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
        }
        catch (InvalidOperationException ex)
        {
            await _messageBox.CustomErrorMessageBox(ex.Message, "Error");
        }
    }

    [RelayCommand]
    private async Task RunAsync()
    {
        SaveSettings();
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return;

        await InjectConfigAsync();
        ShouldRun = true;
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveSettings();
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return;

        await InjectConfigAsync();
        await _messageBox.SettingsSavedSuccessfullyMessageBox();
        CloseRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}

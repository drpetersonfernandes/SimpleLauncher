using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

public partial class InjectRpcs3ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
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

    public InjectRpcs3ConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

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

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        Rpcs3Renderer = _settings.Rpcs3Renderer;
        Rpcs3Resolution = _settings.Rpcs3Resolution;
        Rpcs3AspectRatio = _settings.Rpcs3AspectRatio;
        Rpcs3Vsync = _settings.Rpcs3Vsync;
        Rpcs3ResolutionScale = _settings.Rpcs3ResolutionScale;
        Rpcs3AnisotropicFilter = _settings.Rpcs3AnisotropicFilter;
        Rpcs3PpuDecoder = _settings.Rpcs3PpuDecoder;
        Rpcs3SpuDecoder = _settings.Rpcs3SpuDecoder;
        Rpcs3AudioRenderer = _settings.Rpcs3AudioRenderer;
        Rpcs3AudioBuffering = _settings.Rpcs3AudioBuffering;
        Rpcs3StartFullscreen = _settings.Rpcs3StartFullscreen;
        Rpcs3ShowSettingsBeforeLaunch = _settings.Rpcs3ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.Rpcs3Renderer = Rpcs3Renderer;
        _settings.Rpcs3Resolution = Rpcs3Resolution;
        _settings.Rpcs3AspectRatio = Rpcs3AspectRatio;
        _settings.Rpcs3Vsync = Rpcs3Vsync;
        _settings.Rpcs3ResolutionScale = Rpcs3ResolutionScale;
        _settings.Rpcs3AnisotropicFilter = Rpcs3AnisotropicFilter;
        _settings.Rpcs3PpuDecoder = Rpcs3PpuDecoder;
        _settings.Rpcs3SpuDecoder = Rpcs3SpuDecoder;
        _settings.Rpcs3AudioRenderer = Rpcs3AudioRenderer;
        _settings.Rpcs3AudioBuffering = Rpcs3AudioBuffering;
        _settings.Rpcs3StartFullscreen = Rpcs3StartFullscreen;
        _settings.Rpcs3ShowSettingsBeforeLaunch = Rpcs3ShowSettingsBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
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

        MessageBoxLibrary.Rpcs3EmulatorNotFoundPleaseLocateMessageBox();

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
            Rpcs3ConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"RPCS3 configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRpcs3ConfigWindow));
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
                MessageBoxLibrary.Rpcs3ConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectRpcs3ConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

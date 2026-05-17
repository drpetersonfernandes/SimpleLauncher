using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher.ViewModels;

public class InjectAresConfigViewModel : ViewModelBase
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private string _emulatorPath;

    private string _videoDriver;
    private bool _exclusive;
    private string _shader;
    private string _multiplier;
    private string _aspectCorrection;
    private bool _mute;
    private double _volume;
    private bool _fastBoot;
    private bool _rewind;
    private bool _runAhead;
    private bool _autoSaveMemory;
    private bool _showBeforeLaunch;

    public InjectAresConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        SaveCommand = new RelayCommand(_ => ExecuteSave());
        RunCommand = new RelayCommand(_ => ExecuteRun());

        LoadSettings();
    }

    public List<string> VideoDriverOptions { get; } = ["OpenGL 3.2", "Vulkan", "Direct3D 11", "Direct3D 12"];
    public List<string> ShaderOptions { get; } = ["None", "Blur"];
    public List<string> MultiplierOptions { get; } = ["1", "2", "3", "4", "5"];
    public List<string> AspectCorrectionOptions { get; } = ["Standard", "Center", "Scale", "Stretch"];

    public string VideoDriver
    {
        get => _videoDriver;
        set => SetProperty(ref _videoDriver, value);
    }

    public bool Exclusive
    {
        get => _exclusive;
        set => SetProperty(ref _exclusive, value);
    }

    public string Shader
    {
        get => _shader;
        set => SetProperty(ref _shader, value);
    }

    public string Multiplier
    {
        get => _multiplier;
        set => SetProperty(ref _multiplier, value);
    }

    public string AspectCorrection
    {
        get => _aspectCorrection;
        set => SetProperty(ref _aspectCorrection, value);
    }

    public bool Mute
    {
        get => _mute;
        set => SetProperty(ref _mute, value);
    }

    public double Volume
    {
        get => _volume;
        set => SetProperty(ref _volume, value);
    }

    public bool FastBoot
    {
        get => _fastBoot;
        set => SetProperty(ref _fastBoot, value);
    }

    public bool Rewind
    {
        get => _rewind;
        set => SetProperty(ref _rewind, value);
    }

    public bool RunAhead
    {
        get => _runAhead;
        set => SetProperty(ref _runAhead, value);
    }

    public bool AutoSaveMemory
    {
        get => _autoSaveMemory;
        set => SetProperty(ref _autoSaveMemory, value);
    }

    public bool ShowBeforeLaunch
    {
        get => _showBeforeLaunch;
        set => SetProperty(ref _showBeforeLaunch, value);
    }

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public ICommand SaveCommand { get; }
    public ICommand RunCommand { get; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        VideoDriver = _settings.AresVideoDriver;
        Exclusive = _settings.AresExclusive;
        Shader = _settings.AresShader;
        Multiplier = _settings.AresMultiplier.ToString(CultureInfo.InvariantCulture);
        AspectCorrection = _settings.AresAspectCorrection;
        Mute = _settings.AresMute;
        Volume = _settings.AresVolume;
        FastBoot = _settings.AresFastBoot;
        Rewind = _settings.AresRewind;
        RunAhead = _settings.AresRunAhead;
        AutoSaveMemory = _settings.AresAutoSaveMemory;
        ShowBeforeLaunch = _settings.AresShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.AresVideoDriver = VideoDriver;
        _settings.AresExclusive = Exclusive;
        _settings.AresShader = Shader;
        _settings.AresMultiplier = int.Parse(Multiplier, CultureInfo.InvariantCulture);
        _settings.AresAspectCorrection = AspectCorrection;
        _settings.AresMute = Mute;
        _settings.AresVolume = Volume;
        _settings.AresFastBoot = FastBoot;
        _settings.AresRewind = Rewind;
        _settings.AresRunAhead = RunAhead;
        _settings.AresAutoSaveMemory = AutoSaveMemory;
        _settings.AresShowSettingsBeforeLaunch = ShowBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Ares");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.AresemulatornotfoundMessageBox();

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
            AresConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"Ares configuration injection failed for path: {path}");
            return false;
        }
    }

    private void ExecuteRun()
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAresConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
            ShouldRun = true;
        }
    }

    private void ExecuteSave()
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                MessageBoxLibrary.AresConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectAresConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

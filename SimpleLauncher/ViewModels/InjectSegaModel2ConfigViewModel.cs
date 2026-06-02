using System.Globalization;
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

public partial class InjectSegaModel2ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
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

    public InjectSegaModel2ConfigViewModel(SettingsManager settings, string emulatorPath, bool isLauncherMode)
    {
        _settings = settings;
        _emulatorPath = emulatorPath;
        IsLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        LoadSettings();
    }

    public List<string> WideScreenOptions { get; } = ["0", "1", "2"];
    public List<string> FsaaOptions { get; } = ["0", "2", "4", "8"];

    public bool IsLauncherMode { get; }

    public bool ShouldRun { get; private set; }

    public event Action CloseRequested;
    public event Func<string> RequestEmulatorPath;
    public event Func<Window> GetOwnerWindow;

    private void LoadSettings()
    {
        ResX = _settings.SegaModel2ResX;
        ResY = _settings.SegaModel2ResY;
        WideScreen = _settings.SegaModel2WideScreen.ToString(CultureInfo.InvariantCulture);
        Fsaa = _settings.SegaModel2Fsaa.ToString(CultureInfo.InvariantCulture);
        Bilinear = _settings.SegaModel2Bilinear;
        Trilinear = _settings.SegaModel2Trilinear;
        FilterTilemaps = _settings.SegaModel2FilterTilemaps;
        DrawCross = _settings.SegaModel2DrawCross;
        XInput = _settings.SegaModel2XInput;
        EnableFf = _settings.SegaModel2EnableFf;
        HoldGears = _settings.SegaModel2HoldGears;
        UseRawInput = _settings.SegaModel2UseRawInput;
        ShowBeforeLaunch = _settings.SegaModel2ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.SegaModel2ResX = ResX;
        _settings.SegaModel2ResY = ResY;
        _settings.SegaModel2WideScreen = int.Parse(WideScreen, CultureInfo.InvariantCulture);
        _settings.SegaModel2Fsaa = int.Parse(Fsaa, CultureInfo.InvariantCulture);
        _settings.SegaModel2Bilinear = Bilinear;
        _settings.SegaModel2Trilinear = Trilinear;
        _settings.SegaModel2FilterTilemaps = FilterTilemaps;
        _settings.SegaModel2DrawCross = DrawCross;
        _settings.SegaModel2XInput = XInput;
        _settings.SegaModel2EnableFf = EnableFf;
        _settings.SegaModel2HoldGears = HoldGears;
        _settings.SegaModel2UseRawInput = UseRawInput;
        _settings.SegaModel2ShowSettingsBeforeLaunch = ShowBeforeLaunch;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
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

        MessageBoxLibrary.SegaModel2EmulatorNotFoundMessageBox();

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
            SegaModel2ConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, $"SEGA Model 2 configuration injection failed for path: {path}");
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectSegaModel2ConfigWindow));
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
                MessageBoxLibrary.SegaModel2ConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, typeof(InjectSegaModel2ConfigWindow));
            var window = GetOwnerWindow?.Invoke();
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, window);
        }
    }
}

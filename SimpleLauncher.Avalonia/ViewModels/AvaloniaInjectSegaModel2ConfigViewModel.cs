using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaInjectSegaModel2ConfigViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IDebugLogger _debugLogger;
    private readonly IMessageBoxLibraryService _messageBox;
    private string? _emulatorPath;

    [ObservableProperty] private int _resX = 640;
    [ObservableProperty] private int _resY = 480;
    [ObservableProperty] private string _wideScreen = "0";
    [ObservableProperty] private string _fsaa = "0";
    [ObservableProperty] private bool _bilinear;
    [ObservableProperty] private bool _trilinear;
    [ObservableProperty] private bool _filterTilemaps;
    [ObservableProperty] private bool _drawCross;
    [ObservableProperty] private bool _xInput;
    [ObservableProperty] private bool _enableFf;
    [ObservableProperty] private bool _holdGears;
    [ObservableProperty] private bool _useRawInput;
    [ObservableProperty] private bool _showBeforeLaunch;

    public AvaloniaInjectSegaModel2ConfigViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
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

    public List<string> WideScreenOptions { get; } = ["0", "1", "2"];
    public List<string> FsaaOptions { get; } = ["0", "2", "4", "8"];

    public bool IsLauncherMode { get; private set; }
    public bool ShouldRun { get; private set; }

    public event Action? CloseRequested;
    public event Func<string?>? RequestEmulatorPath;

    private void LoadSettings()
    {
        ResX = _settings.SegaModel2.ResX;
        ResY = _settings.SegaModel2.ResY;
        WideScreen = _settings.SegaModel2.WideScreen.ToString(CultureInfo.InvariantCulture);
        Fsaa = _settings.SegaModel2.Fsaa.ToString(CultureInfo.InvariantCulture);
        Bilinear = _settings.SegaModel2.Bilinear;
        Trilinear = _settings.SegaModel2.Trilinear;
        FilterTilemaps = _settings.SegaModel2.FilterTilemaps;
        DrawCross = _settings.SegaModel2.DrawCross;
        XInput = _settings.SegaModel2.XInput;
        EnableFf = _settings.SegaModel2.EnableFf;
        HoldGears = _settings.SegaModel2.HoldGears;
        UseRawInput = _settings.SegaModel2.UseRawInput;
        ShowBeforeLaunch = _settings.SegaModel2.ShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.SegaModel2.ResX = ResX;
        _settings.SegaModel2.ResY = ResY;
        _settings.SegaModel2.WideScreen = int.Parse(WideScreen, CultureInfo.InvariantCulture);
        _settings.SegaModel2.Fsaa = int.Parse(Fsaa, CultureInfo.InvariantCulture);
        _settings.SegaModel2.Bilinear = Bilinear;
        _settings.SegaModel2.Trilinear = Trilinear;
        _settings.SegaModel2.FilterTilemaps = FilterTilemaps;
        _settings.SegaModel2.DrawCross = DrawCross;
        _settings.SegaModel2.XInput = XInput;
        _settings.SegaModel2.EnableFf = EnableFf;
        _settings.SegaModel2.HoldGears = HoldGears;
        _settings.SegaModel2.UseRawInput = UseRawInput;
        _settings.SegaModel2.ShowSettingsBeforeLaunch = ShowBeforeLaunch;
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

            SegaModel2ConfigurationService.InjectSettings(_emulatorPath, _settings, _logErrors, _debugLogger);
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

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectRedreamConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectRedreamConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _emulatorPath = emulatorPath;
        _isLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Video
        CmbCable.Text = _settings.RedreamCable;
        CmbBroadcast.Text = _settings.RedreamBroadcast;
        ChkVsync.IsChecked = _settings.RedreamVsync;
        ChkFrameskip.IsChecked = _settings.RedreamFrameskip;
        CmbAspect.Text = _settings.RedreamAspect;
        CmbRes.Text = _settings.RedreamRes.ToString(CultureInfo.InvariantCulture);
        CmbRenderer.Text = _settings.RedreamRenderer;
        CmbFullmode.Text = _settings.RedreamFullmode;

        // System
        CmbLanguage.Text = _settings.RedreamLanguage;
        CmbRegion.Text = _settings.RedreamRegion;

        ChkShowBeforeLaunch.IsChecked = _settings.RedreamShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        // Video
        _settings.RedreamCable = CmbCable.Text;
        _settings.RedreamBroadcast = CmbBroadcast.Text;
        _settings.RedreamVsync = ChkVsync.IsChecked ?? true;
        _settings.RedreamFrameskip = ChkFrameskip.IsChecked ?? true;
        _settings.RedreamAspect = CmbAspect.Text;
        if (int.TryParse(CmbRes.Text, out var res))
        {
            _settings.RedreamRes = res;
        }

        _settings.RedreamRenderer = CmbRenderer.Text;
        _settings.RedreamFullmode = CmbFullmode.Text;

        // System
        _settings.RedreamLanguage = CmbLanguage.Text;
        _settings.RedreamRegion = CmbRegion.Text;

        _settings.RedreamShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        MessageBoxLibrary.ReDreamEmulatorPathNotFound();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Redream Executable|redream.exe|All Executables|*.exe",
            Title = "Select Redream Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            RedreamConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Redream configuration injection failed for path: {path}");
            return false;
        }
    }

    private void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            ShouldRun = true;
            Close();
        }
        else
        {
            MessageBoxLibrary.FailedToInjectReDreamConfiguration();
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.ReDreamConfigurationInjectedSuccessfully();
            Close();
        }
        else
        {
            MessageBoxLibrary.FailedToInjectReDreamConfiguration();
        }
    }
}

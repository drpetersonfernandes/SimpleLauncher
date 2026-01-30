using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectMednafenConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectMednafenConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        CmbVideoDriver.Text = _settings.MednafenVideoDriver;
        ChkFullscreen.IsChecked = _settings.MednafenFullscreen;
        ChkVsync.IsChecked = _settings.MednafenVsync;
        CmbStretch.Text = _settings.MednafenStretch;
        ChkBilinear.IsChecked = _settings.MednafenBilinear;
        SldScanlines.Value = _settings.MednafenScanlines;
        CmbShader.Text = _settings.MednafenShader;
        SldVolume.Value = _settings.MednafenVolume;
        ChkCheats.IsChecked = _settings.MednafenCheats;
        ChkRewind.IsChecked = _settings.MednafenRewind;
        ChkShowBeforeLaunch.IsChecked = _settings.MednafenShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        MessageBoxLibrary.ShowCustomMessage("Mednafen emulator not found. Please locate mednafen.exe.", "Emulator Not Found");
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Mednafen Executable|mednafen.exe|All Executables|*.exe",
            Title = "Select Mednafen Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.MednafenVideoDriver = CmbVideoDriver.Text;
        _settings.MednafenFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.MednafenVsync = ChkVsync.IsChecked ?? true;
        _settings.MednafenStretch = CmbStretch.Text;
        _settings.MednafenBilinear = ChkBilinear.IsChecked ?? false;
        _settings.MednafenScanlines = (int)SldScanlines.Value;
        _settings.MednafenShader = CmbShader.Text;
        _settings.MednafenVolume = (int)SldVolume.Value;
        _settings.MednafenCheats = ChkCheats.IsChecked ?? true;
        _settings.MednafenRewind = ChkRewind.IsChecked ?? false;
        _settings.MednafenShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            MednafenConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Mednafen configuration injection failed for path: {path}");
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
            MessageBoxLibrary.ShowCustomMessage("Failed to inject Mednafen configuration. Please check file permissions and try again.", "Injection Failed");
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.ShowCustomMessage("Mednafen configuration saved successfully.", "Success");
        }
        else
        {
            MessageBoxLibrary.ShowCustomMessage("Failed to save Mednafen configuration. Please check file permissions.", "Save Failed");
        }

        Close();
    }
}

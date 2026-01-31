using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectDolphinConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectDolphinConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        CmbGfxBackend.Text = _settings.DolphinGfxBackend;
        ChkDspThread.IsChecked = _settings.DolphinDspThread;
        ChkWiimoteContinuousScanning.IsChecked = _settings.DolphinWiimoteContinuousScanning;
        ChkWiimoteEnableSpeaker.IsChecked = _settings.DolphinWiimoteEnableSpeaker;
        ChkShowBeforeLaunch.IsChecked = _settings.DolphinShowSettingsBeforeLaunch;

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

        MessageBoxLibrary.ShowCustomMessage("Dolphin emulator not found. Please locate Dolphin.exe.", "Emulator Not Found");
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Dolphin Executable|Dolphin.exe|All Executables|*.exe",
            Title = "Select Dolphin Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.DolphinGfxBackend = CmbGfxBackend.Text;
        _settings.DolphinDspThread = ChkDspThread.IsChecked ?? true;
        _settings.DolphinWiimoteContinuousScanning = ChkWiimoteContinuousScanning.IsChecked ?? true;
        _settings.DolphinWiimoteEnableSpeaker = ChkWiimoteEnableSpeaker.IsChecked ?? true;
        _settings.DolphinShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            DolphinConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Dolphin configuration injection failed for path: {path}");
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
            MessageBoxLibrary.ShowCustomMessage("Failed to inject Dolphin configuration. Please check file permissions and try again.", "Injection Failed");
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.ShowCustomMessage("Dolphin configuration saved successfully.", "Success");
        }
        else
        {
            MessageBoxLibrary.ShowCustomMessage("Failed to save Dolphin configuration. Please check file permissions.", "Save Failed");
        }

        Close();
    }
}

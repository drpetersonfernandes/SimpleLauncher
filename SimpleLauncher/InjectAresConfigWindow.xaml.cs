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

public partial class InjectAresConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectAresConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        CmbVideoDriver.Text = _settings.AresVideoDriver;
        ChkExclusive.IsChecked = _settings.AresExclusive;
        CmbShader.Text = _settings.AresShader;
        CmbMultiplier.Text = _settings.AresMultiplier.ToString(CultureInfo.InvariantCulture);
        CmbAspectCorrection.Text = _settings.AresAspectCorrection;
        ChkMute.IsChecked = _settings.AresMute;
        SldVolume.Value = _settings.AresVolume;
        ChkFastBoot.IsChecked = _settings.AresFastBoot;
        ChkRewind.IsChecked = _settings.AresRewind;
        ChkRunAhead.IsChecked = _settings.AresRunAhead;
        ChkAutoSaveMemory.IsChecked = _settings.AresAutoSaveMemory;
        ChkShowBeforeLaunch.IsChecked = _settings.AresShowSettingsBeforeLaunch;

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

        MessageBoxLibrary.ShowCustomMessage("Ares emulator not found. Please locate ares.exe.", "Emulator Not Found");
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Ares Executable|ares.exe|All Executables|*.exe",
            Title = "Select Ares Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.AresVideoDriver = CmbVideoDriver.Text;
        _settings.AresExclusive = ChkExclusive.IsChecked ?? false;
        _settings.AresShader = CmbShader.Text;
        _settings.AresMultiplier = int.Parse(CmbMultiplier.Text);
        _settings.AresAspectCorrection = CmbAspectCorrection.Text;
        _settings.AresMute = ChkMute.IsChecked ?? false;
        _settings.AresVolume = SldVolume.Value;
        _settings.AresFastBoot = ChkFastBoot.IsChecked ?? false;
        _settings.AresRewind = ChkRewind.IsChecked ?? false;
        _settings.AresRunAhead = ChkRunAhead.IsChecked ?? false;
        _settings.AresAutoSaveMemory = ChkAutoSaveMemory.IsChecked ?? true;
        _settings.AresShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            AresConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Ares configuration injection failed for path: {path}");
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
            MessageBoxLibrary.ShowCustomMessage("Failed to inject Ares configuration. Please check file permissions and try again.", "Injection Failed");
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.ShowCustomMessage("Ares configuration saved successfully.", "Success");
        }
        else
        {
            MessageBoxLibrary.ShowCustomMessage("Failed to save Ares configuration. Please check file permissions.", "Save Failed");
        }

        Close();
    }
}

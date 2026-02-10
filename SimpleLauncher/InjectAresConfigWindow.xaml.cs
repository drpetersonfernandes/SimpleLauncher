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

        MessageBoxLibrary.Aresemulatornotfound();

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Ares Executable|ares.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectAresEmulator") ?? "Select Ares Emulator"
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
        _settings.AresMultiplier = int.Parse(CmbMultiplier.Text, CultureInfo.InvariantCulture);
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
            MessageBoxLibrary.FailedtoinjectAresconfiguration();
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (!InjectConfig())
        {
            MessageBoxLibrary.FailedToSaveAresConfiguration();
            return; // Don't close on failure
        }

        MessageBoxLibrary.AresConfigurationSavedSuccessfully();
        // Only close on success, and only if not in launcher mode (otherwise Run button handles it)
        // Actually, in non-launcher mode, we should close on success
        Close();
    }
}
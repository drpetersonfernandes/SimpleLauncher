using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectYumirConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private readonly string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectYumirConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        ChkFullscreen.IsChecked = _settings.YumirFullscreen;
        ChkForceAspect.IsChecked = _settings.YumirForceAspectRatio;
        ChkReduceLatency.IsChecked = _settings.YumirReduceLatency;
        ChkMute.IsChecked = _settings.YumirMute;
        SldVolume.Value = _settings.YumirVolume;
        ChkAutoRegion.IsChecked = _settings.YumirAutoDetectRegion;
        CmbVideoStandard.Text = _settings.YumirVideoStandard;
        ChkPauseUnfocused.IsChecked = _settings.YumirPauseWhenUnfocused;
        ChkShowBeforeLaunch.IsChecked = _settings.YumirShowSettingsBeforeLaunch;

        foreach (ComboBoxItem item in CmbForcedAspect.Items)
        {
            if (item.Tag.ToString() == _settings.YumirForcedAspect.ToString(CultureInfo.InvariantCulture))
            {
                CmbForcedAspect.SelectedItem = item;
                break;
            }
        }

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        _settings.YumirFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.YumirForceAspectRatio = ChkForceAspect.IsChecked ?? false;
        _settings.YumirReduceLatency = ChkReduceLatency.IsChecked ?? true;
        _settings.YumirMute = ChkMute.IsChecked ?? false;
        _settings.YumirVolume = SldVolume.Value;
        _settings.YumirAutoDetectRegion = ChkAutoRegion.IsChecked ?? true;
        _settings.YumirVideoStandard = CmbVideoStandard.Text;
        _settings.YumirPauseWhenUnfocused = ChkPauseUnfocused.IsChecked ?? false;
        _settings.YumirShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        if (CmbForcedAspect.SelectedItem is ComboBoxItem selected)
        {
            _settings.YumirForcedAspect = double.Parse(selected.Tag.ToString() ?? "1.7777777777777777", CultureInfo.InvariantCulture);
        }

        _settings.Save();
    }

    private bool InjectConfig()
    {
        if (string.IsNullOrEmpty(_emulatorPath) || !File.Exists(_emulatorPath)) return false;

        try
        {
            YumirConfigurationService.InjectSettings(_emulatorPath, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, "Yumir injection failed.");
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
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (!InjectConfig())
        {
            MessageBoxLibrary.FailedToInjectYumirConfiguration();
            return;
        }

        MessageBoxLibrary.YumirConfigurationSavedSuccessfully();
        Close();
    }
}

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

public partial class InjectAzaharConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectAzaharConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        SelectComboByTag(CmbGraphicsApi, _settings.AzaharGraphicsApi.ToString(CultureInfo.InvariantCulture));
        SelectComboByTag(CmbResolution, _settings.AzaharResolutionFactor.ToString(CultureInfo.InvariantCulture));
        SelectComboByTag(CmbLayout, _settings.AzaharLayoutOption.ToString(CultureInfo.InvariantCulture));

        ChkFullscreen.IsChecked = _settings.AzaharFullscreen;
        ChkVsync.IsChecked = _settings.AzaharUseVsync;
        ChkAsyncShader.IsChecked = _settings.AzaharAsyncShaderCompilation;
        ChkNew3Ds.IsChecked = _settings.AzaharIsNew3ds;
        SldVolume.Value = _settings.AzaharVolume;
        ChkShowBeforeLaunch.IsChecked = _settings.AzaharShowSettingsBeforeLaunch;
        ChkAudioStretching.IsChecked = _settings.AzaharEnableAudioStretching;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        _settings.AzaharGraphicsApi = int.Parse(GetSelectedTag(CmbGraphicsApi), CultureInfo.InvariantCulture);
        _settings.AzaharResolutionFactor = int.Parse(GetSelectedTag(CmbResolution), CultureInfo.InvariantCulture);
        _settings.AzaharLayoutOption = int.Parse(GetSelectedTag(CmbLayout), CultureInfo.InvariantCulture);
        _settings.AzaharFullscreen = ChkFullscreen.IsChecked ?? true;
        _settings.AzaharUseVsync = ChkVsync.IsChecked ?? true;
        _settings.AzaharAsyncShaderCompilation = ChkAsyncShader.IsChecked ?? true;
        _settings.AzaharIsNew3ds = ChkNew3Ds.IsChecked ?? true;
        _settings.AzaharVolume = (int)SldVolume.Value;
        _settings.AzaharShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;
        _settings.AzaharEnableAudioStretching = ChkAudioStretching.IsChecked ?? true;
        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Azahar Executable|azahar.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectAzaharEmulator") ?? "Select Azahar Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return false;

        try
        {
            AzaharConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, "Azahar injection failed");
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
        if (InjectConfig())
        {
            MessageBoxLibrary.SettingsSaved();
            Close();
        }
    }

    private static void SelectComboByTag(ComboBox cmb, string tag)
    {
        foreach (ComboBoxItem item in cmb.Items)
            if (item.Tag?.ToString() == tag)
            {
                cmb.SelectedItem = item;
                return;
            }
    }

    private static string GetSelectedTag(ComboBox cmb)
    {
        return (cmb.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "0";
    }
}
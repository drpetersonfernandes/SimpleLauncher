using System.IO;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.InjectEmulatorConfig;
using Microsoft.Extensions.DependencyInjection;
using System;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectRetroArchConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;


    public InjectRetroArchConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        CmbVideoDriver.Text = _settings.RetroArchVideoDriver;
        ChkFullscreen.IsChecked = _settings.RetroArchFullscreen;
        ChkVsync.IsChecked = _settings.RetroArchVsync;
        ChkThreadedVideo.IsChecked = _settings.RetroArchThreadedVideo;
        ChkBilinear.IsChecked = _settings.RetroArchBilinear;
        SelectComboByTag(CmbAspectRatio, _settings.RetroArchAspectRatioIndex);
        ChkScaleInteger.IsChecked = _settings.RetroArchScaleInteger;
        ChkShaderEnable.IsChecked = _settings.RetroArchShaderEnable;
        ChkHardSync.IsChecked = _settings.RetroArchHardSync;

        ChkAudioEnable.IsChecked = _settings.RetroArchAudioEnable;
        ChkAudioMute.IsChecked = _settings.RetroArchAudioMute;

        ChkPauseNonActive.IsChecked = _settings.RetroArchPauseNonActive;
        ChkSaveOnExit.IsChecked = _settings.RetroArchSaveOnExit;
        ChkAutoSaveState.IsChecked = _settings.RetroArchAutoSaveState;
        ChkAutoLoadState.IsChecked = _settings.RetroArchAutoLoadState;
        ChkRewind.IsChecked = _settings.RetroArchRewind;
        ChkRunAhead.IsChecked = _settings.RetroArchRunAhead;

        CmbMenuDriver.Text = _settings.RetroArchMenuDriver;
        ChkAdvancedSettings.IsChecked = _settings.RetroArchShowAdvancedSettings;

        ChkCheevosEnable.IsChecked = _settings.RetroArchCheevosEnable;
        ChkCheevosHardcore.IsChecked = _settings.RetroArchCheevosHardcore;
        ChkDiscordAllow.IsChecked = _settings.RetroArchDiscordAllow;

        ChkShowBeforeLaunch.IsChecked = _settings.RetroArchShowSettingsBeforeLaunch;

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

        MessageBoxLibrary.RetroArchemulatorpathnotfound();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "RetroArch Executable|retroarch.exe|All Executables|*.exe",
            Title = "Select RetroArch Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.RetroArchVideoDriver = CmbVideoDriver.Text;
        _settings.RetroArchFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.RetroArchVsync = ChkVsync.IsChecked ?? true;
        _settings.RetroArchThreadedVideo = ChkThreadedVideo.IsChecked ?? false;
        _settings.RetroArchBilinear = ChkBilinear.IsChecked ?? false;
        _settings.RetroArchAspectRatioIndex = GetSelectedTag(CmbAspectRatio);
        _settings.RetroArchScaleInteger = ChkScaleInteger.IsChecked ?? false;
        _settings.RetroArchShaderEnable = ChkShaderEnable.IsChecked ?? true;
        _settings.RetroArchHardSync = ChkHardSync.IsChecked ?? false;

        _settings.RetroArchAudioEnable = ChkAudioEnable.IsChecked ?? true;
        _settings.RetroArchAudioMute = ChkAudioMute.IsChecked ?? false;

        _settings.RetroArchPauseNonActive = ChkPauseNonActive.IsChecked ?? true;
        _settings.RetroArchSaveOnExit = ChkSaveOnExit.IsChecked ?? true;
        _settings.RetroArchAutoSaveState = ChkAutoSaveState.IsChecked ?? false;
        _settings.RetroArchAutoLoadState = ChkAutoLoadState.IsChecked ?? false;
        _settings.RetroArchRewind = ChkRewind.IsChecked ?? false;
        _settings.RetroArchRunAhead = ChkRunAhead.IsChecked ?? false;

        _settings.RetroArchMenuDriver = CmbMenuDriver.Text;
        _settings.RetroArchShowAdvancedSettings = ChkAdvancedSettings.IsChecked ?? true;

        _settings.RetroArchCheevosEnable = ChkCheevosEnable.IsChecked ?? false;
        _settings.RetroArchCheevosHardcore = ChkCheevosHardcore.IsChecked ?? false;
        _settings.RetroArchDiscordAllow = ChkDiscordAllow.IsChecked ?? false;

        _settings.RetroArchShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            RetroArchConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"RetroArch configuration injection failed for path: {path}");
            return false;
        }
    }

    private void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        var success = InjectConfig();
        if (success)
        {
            ShouldRun = true;
            Close();
        }
        else
        {
            MessageBoxLibrary.FailedtoinjectRetroArchconfiguration();
            // Keep window open so user can retry or cancel
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        var success = InjectConfig();
        if (success)
        {
            MessageBoxLibrary.RetroArchconfigurationinjectedsuccessfully();
        }
        else
        {
            MessageBoxLibrary.FailedtoinjectRetroArchconfiguration2();
        }

        Close();
    }

    private static void SelectComboByTag(ComboBox cmb, string tagValue)
    {
        foreach (ComboBoxItem item in cmb.Items)
        {
            if (item.Tag?.ToString() == tagValue)
            {
                cmb.SelectedItem = item;
                return;
            }
        }

        if (cmb.Items.Count > 0)
        {
            cmb.SelectedIndex = 0;
        }
    }

    private static string GetSelectedTag(ComboBox cmb)
    {
        return (cmb.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "";
    }
}
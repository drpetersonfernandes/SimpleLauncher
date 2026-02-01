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

public partial class InjectPcsx2ConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectPcsx2ConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        SelectComboByTag(CmbRenderer, _settings.Pcsx2Renderer.ToString(CultureInfo.InvariantCulture));
        SelectComboByTag(CmbUpscale, _settings.Pcsx2UpscaleMultiplier.ToString(CultureInfo.InvariantCulture));
        CmbAspect.Text = _settings.Pcsx2AspectRatio;
        ChkVsync.IsChecked = _settings.Pcsx2Vsync;
        ChkWidescreenPatches.IsChecked = _settings.Pcsx2EnableWidescreenPatches;
        ChkFullscreen.IsChecked = _settings.Pcsx2StartFullscreen;
        ChkCheats.IsChecked = _settings.Pcsx2EnableCheats;
        SldVolume.Value = _settings.Pcsx2Volume;
        ChkAchEnabled.IsChecked = _settings.Pcsx2AchievementsEnabled;
        ChkAchHardcore.IsChecked = _settings.Pcsx2AchievementsHardcore;
        ChkShowBeforeLaunch.IsChecked = _settings.Pcsx2ShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        _settings.Pcsx2Renderer = int.Parse(GetSelectedTag(CmbRenderer), CultureInfo.InvariantCulture);
        _settings.Pcsx2UpscaleMultiplier = int.Parse(GetSelectedTag(CmbUpscale), CultureInfo.InvariantCulture);
        _settings.Pcsx2AspectRatio = CmbAspect.Text;
        _settings.Pcsx2Vsync = ChkVsync.IsChecked ?? false;
        _settings.Pcsx2EnableWidescreenPatches = ChkWidescreenPatches.IsChecked == true;
        _settings.Pcsx2StartFullscreen = ChkFullscreen.IsChecked == true;
        _settings.Pcsx2EnableCheats = ChkCheats.IsChecked == true;
        _settings.Pcsx2Volume = (int)SldVolume.Value;
        _settings.Pcsx2AchievementsEnabled = ChkAchEnabled.IsChecked == true;
        _settings.Pcsx2AchievementsHardcore = ChkAchHardcore.IsChecked == true;
        _settings.Pcsx2ShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked == true;
        _settings.Save();
    }

    private bool InjectConfig()
    {
        if (string.IsNullOrEmpty(_emulatorPath) || !File.Exists(_emulatorPath))
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "PCSX2 Executable|pcsx2*.exe" };
            if (dialog.ShowDialog() == true)
            {
                _emulatorPath = dialog.FileName;
            }
            else return false;
        }

        try
        {
            Pcsx2ConfigurationService.InjectSettings(_emulatorPath, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, "PCSX2 injection failed.");
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
        if (InjectConfig()) MessageBoxLibrary.ShowCustomMessage("PCSX2 settings saved.", "Success");
        Close();
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

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

public partial class InjectRaineConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath; // Removed readonly to allow selection
    private readonly ILogErrors _logErrors;

    public InjectRaineConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _emulatorPath = emulatorPath;
        _isLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }

        LoadSettings();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        // Use a localized message if possible, or generic
        MessageBoxLibrary.RaineExecutableNotFound();

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Raine Executable|raine*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectExeTitle") ?? "Select Raine Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void LoadSettings()
    {
        ChkFullscreen.IsChecked = _settings.RaineFullscreen;
        ChkFixAspect.IsChecked = _settings.RaineFixAspectRatio;
        ChkVsync.IsChecked = _settings.RaineVsync;
        NumResX.Value = _settings.RaineResX;
        NumResY.Value = _settings.RaineResY;
        CmbAudioDriver.Text = _settings.RaineSoundDriver;
        CmbSampleRate.Text = _settings.RaineSampleRate.ToString(CultureInfo.InvariantCulture);
        ChkShowBeforeLaunch.IsChecked = _settings.RaineShowSettingsBeforeLaunch;
    }

    private void SaveSettings()
    {
        _settings.RaineFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.RaineFixAspectRatio = ChkFixAspect.IsChecked ?? true;
        _settings.RaineVsync = ChkVsync.IsChecked ?? true;
        _settings.RaineResX = (int)(NumResX.Value ?? 640);
        _settings.RaineResY = (int)(NumResY.Value ?? 480);
        _settings.RaineSoundDriver = (CmbAudioDriver.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "directsound";

        if (CmbSampleRate.SelectedItem is ComboBoxItem selectedRate &&
            int.TryParse(selectedRate.Content.ToString(), out var rate))
        {
            _settings.RaineSampleRate = rate;
        }

        _settings.RaineShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;
        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return false;

        try
        {
            RaineConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"Raine configuration injection failed for path: {path}");
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
            if (!_isLauncherMode)
                MessageBoxLibrary.RaineSettingsSavedAndInjected();
        }

        Close();
    }
}
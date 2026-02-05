using System.Globalization;
using System.IO;
using System.Windows;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectRaineConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly string _emulatorPath;
    public bool ShouldRun { get; private set; }

    public InjectRaineConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _emulatorPath = emulatorPath;
        BtnRun.Visibility = isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        LoadSettings();
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
        _settings.RaineSoundDriver = CmbAudioDriver.Text;
        _settings.RaineSampleRate = int.Parse(CmbSampleRate.Text);
        _settings.RaineShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;
        _settings.Save();
    }

    private void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        RaineConfigurationService.InjectSettings(_emulatorPath, _settings);
        ShouldRun = true;
        Close();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (File.Exists(_emulatorPath)) RaineConfigurationService.InjectSettings(_emulatorPath, _settings);
        Close();
    }
}
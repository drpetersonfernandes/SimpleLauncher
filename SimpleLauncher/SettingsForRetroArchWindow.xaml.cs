using System.Windows;
using SimpleLauncher.Managers;

namespace SimpleLauncher;

public partial class SettingsForRetroArchWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }

    public SettingsForRetroArchWindow(SettingsManager settings, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _isLauncherMode = isLauncherMode;
        LoadSettings();
    }

    private void LoadSettings()
    {
        CmbVideoDriver.Text = _settings.RetroArchVideoDriver;
        ChkFullscreen.IsChecked = _settings.RetroArchFullscreen;
        ChkVsync.IsChecked = _settings.RetroArchVsync;
        ChkThreadedVideo.IsChecked = _settings.RetroArchThreadedVideo;
        ChkBilinear.IsChecked = _settings.RetroArchBilinear;

        ChkAudioEnable.IsChecked = _settings.RetroArchAudioEnable;
        ChkAudioMute.IsChecked = _settings.RetroArchAudioMute;

        ChkPauseNonActive.IsChecked = _settings.RetroArchPauseNonActive;
        ChkSaveOnExit.IsChecked = _settings.RetroArchSaveOnExit;
        ChkAutoSaveState.IsChecked = _settings.RetroArchAutoSaveState;
        ChkAutoLoadState.IsChecked = _settings.RetroArchAutoLoadState;
        ChkRewind.IsChecked = _settings.RetroArchRewind;

        CmbMenuDriver.Text = _settings.RetroArchMenuDriver;

        ChkCheevosEnable.IsChecked = _settings.RetroArchCheevosEnable;
        ChkCheevosHardcore.IsChecked = _settings.RetroArchCheevosHardcore;

        ChkShowBeforeLaunch.IsChecked = _settings.RetroArchShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        _settings.RetroArchVideoDriver = CmbVideoDriver.Text;
        _settings.RetroArchFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.RetroArchVsync = ChkVsync.IsChecked ?? true;
        _settings.RetroArchThreadedVideo = ChkThreadedVideo.IsChecked ?? false;
        _settings.RetroArchBilinear = ChkBilinear.IsChecked ?? false;

        _settings.RetroArchAudioEnable = ChkAudioEnable.IsChecked ?? true;
        _settings.RetroArchAudioMute = ChkAudioMute.IsChecked ?? false;

        _settings.RetroArchPauseNonActive = ChkPauseNonActive.IsChecked ?? true;
        _settings.RetroArchSaveOnExit = ChkSaveOnExit.IsChecked ?? true;
        _settings.RetroArchAutoSaveState = ChkAutoSaveState.IsChecked ?? false;
        _settings.RetroArchAutoLoadState = ChkAutoLoadState.IsChecked ?? false;
        _settings.RetroArchRewind = ChkRewind.IsChecked ?? false;

        _settings.RetroArchMenuDriver = CmbMenuDriver.Text;

        _settings.RetroArchCheevosEnable = ChkCheevosEnable.IsChecked ?? false;
        _settings.RetroArchCheevosHardcore = ChkCheevosHardcore.IsChecked ?? false;

        _settings.RetroArchShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        ShouldRun = true;
        Close();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        Close();
    }
}

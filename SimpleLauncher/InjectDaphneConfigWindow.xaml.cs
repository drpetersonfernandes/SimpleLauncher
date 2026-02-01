using System.Windows;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectDaphneConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }

    public InjectDaphneConfigWindow(SettingsManager settings, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _isLauncherMode = isLauncherMode;
        // emulatorPath is not used here as we don't write to a file, but kept for constructor consistency.
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Video
        ChkFullscreen.IsChecked = _settings.DaphneFullscreen;
        ChkBilinear.IsChecked = _settings.DaphneBilinear;
        NumResX.Value = _settings.DaphneResX;
        NumResY.Value = _settings.DaphneResY;

        // Audio
        ChkEnableSound.IsChecked = _settings.DaphneEnableSound;

        // Gameplay
        ChkDisableCrosshairs.IsChecked = _settings.DaphneDisableCrosshairs;
        ChkUseOverlays.IsChecked = _settings.DaphneUseOverlays;

        ChkShowBeforeLaunch.IsChecked = _settings.DaphneShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        // Video
        _settings.DaphneFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.DaphneBilinear = ChkBilinear.IsChecked ?? true;
        _settings.DaphneResX = (int)(NumResX.Value ?? 640);
        _settings.DaphneResY = (int)(NumResY.Value ?? 480);

        // Audio
        _settings.DaphneEnableSound = ChkEnableSound.IsChecked ?? true;

        // Gameplay
        _settings.DaphneDisableCrosshairs = ChkDisableCrosshairs.IsChecked ?? false;
        _settings.DaphneUseOverlays = ChkUseOverlays.IsChecked ?? true;

        _settings.DaphneShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

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
        MessageBoxLibrary.Daphnesettingssavedsuccessfully();
        Close();
    }
}

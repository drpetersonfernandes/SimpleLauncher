using System.Windows;
using SimpleLauncher.Managers;

namespace SimpleLauncher;

public partial class InjectMameConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }

    public InjectMameConfigWindow(SettingsManager settings, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _isLauncherMode = isLauncherMode;
        LoadSettings();
    }

    private void LoadSettings()
    {
        CmbVideo.Text = _settings.MameVideo;
        CmbBgfxBackend.Text = _settings.MameBgfxBackend;
        CmbBgfxChains.Text = _settings.MameBgfxScreenChains;
        ChkFilter.IsChecked = _settings.MameFilter;
        ChkAutoframeskip.IsChecked = _settings.MameAutoframeskip;
        ChkCheat.IsChecked = _settings.MameCheat;
        ChkRewind.IsChecked = _settings.MameRewind;
        ChkNvramSave.IsChecked = _settings.MameNvramSave;
        ChkWindow.IsChecked = _settings.MameWindow;
        ChkMaximize.IsChecked = _settings.MameMaximize;
        ChkKeepAspect.IsChecked = _settings.MameKeepAspect;
        ChkSkipGameInfo.IsChecked = _settings.MameSkipGameInfo;
        ChkAutosave.IsChecked = _settings.MameAutosave;
        ChkConfirmQuit.IsChecked = _settings.MameConfirmQuit;
        ChkJoystick.IsChecked = _settings.MameJoystick;
        ChkShowBeforeLaunch.IsChecked = _settings.MameShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;

        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        _settings.MameVideo = CmbVideo.Text;
        _settings.MameBgfxBackend = CmbBgfxBackend.Text;
        _settings.MameBgfxScreenChains = CmbBgfxChains.Text;
        _settings.MameFilter = ChkFilter.IsChecked ?? true;
        _settings.MameAutoframeskip = ChkAutoframeskip.IsChecked ?? false;
        _settings.MameCheat = ChkCheat.IsChecked ?? false;
        _settings.MameRewind = ChkRewind.IsChecked ?? false;
        _settings.MameNvramSave = ChkNvramSave.IsChecked ?? true;
        _settings.MameWindow = ChkWindow.IsChecked ?? false;
        _settings.MameMaximize = ChkMaximize.IsChecked ?? true;
        _settings.MameKeepAspect = ChkKeepAspect.IsChecked ?? true;
        _settings.MameSkipGameInfo = ChkSkipGameInfo.IsChecked ?? true;
        _settings.MameAutosave = ChkAutosave.IsChecked ?? false;
        _settings.MameConfirmQuit = ChkConfirmQuit.IsChecked ?? false;
        _settings.MameJoystick = ChkJoystick.IsChecked ?? true;
        _settings.MameShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

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
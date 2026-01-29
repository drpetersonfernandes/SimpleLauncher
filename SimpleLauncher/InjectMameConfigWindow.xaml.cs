using System;
using System.IO;
using System.Windows;
using SimpleLauncher.Services.InjectEmulatorConfig;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectMameConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly string _systemRomPath;
    private readonly ILogErrors _logErrors;


    public InjectMameConfigWindow(SettingsManager settings, string emulatorPath = null, string systemRomPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _emulatorPath = emulatorPath;
        _systemRomPath = systemRomPath;
        _isLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
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

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        MessageBoxLibrary.MamEemulatorpathnotfound();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "MAME Executable|mame*.exe|All Executables|*.exe",
            Title = "Select MAME Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
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

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            MameConfigurationService.InjectSettings(path, _settings, _systemRomPath);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"MAME configuration injection failed for path: {path}");
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
            MessageBoxLibrary.FailedtoinjectMamEconfiguration();
            // Keep window open so user can retry or cancel
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        var success = InjectConfig();
        if (success)
        {
            MessageBoxLibrary.MamEconfigurationinjectedsuccessfully();
        }
        else
        {
            MessageBoxLibrary.FailedtoinjectMamEconfiguration2();
        }

        Close();
    }
}
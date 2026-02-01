using System;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectSupermodelConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectSupermodelConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        // Video
        ChkNew3DEngine.IsChecked = _settings.SupermodelNew3DEngine;
        ChkQuadRendering.IsChecked = _settings.SupermodelQuadRendering;
        ChkFullscreen.IsChecked = _settings.SupermodelFullscreen;
        ChkVsync.IsChecked = _settings.SupermodelVsync;
        ChkWideScreen.IsChecked = _settings.SupermodelWideScreen;
        ChkStretch.IsChecked = _settings.SupermodelStretch;
        NumResX.Value = _settings.SupermodelResX;
        NumResY.Value = _settings.SupermodelResY;

        // Audio
        SldMusic.Value = _settings.SupermodelMusicVolume;
        SldSound.Value = _settings.SupermodelSoundVolume;

        // System
        ChkThrottle.IsChecked = _settings.SupermodelThrottle;
        ChkMultiThreaded.IsChecked = _settings.SupermodelMultiThreaded;
        CmbInputSystem.Text = _settings.SupermodelInputSystem;
        CmbPpcFrequency.Text = _settings.SupermodelPowerPcFrequency.ToString(CultureInfo.InvariantCulture);

        ChkShowBeforeLaunch.IsChecked = _settings.SupermodelShowSettingsBeforeLaunch;

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

        MessageBoxLibrary.SupermodelEmulatorNotFound();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Supermodel Executable|Supermodel.exe|All Executables|*.exe",
            Title = "Select Supermodel Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        // Video
        _settings.SupermodelNew3DEngine = ChkNew3DEngine.IsChecked ?? true;
        _settings.SupermodelQuadRendering = ChkQuadRendering.IsChecked ?? false;
        _settings.SupermodelFullscreen = ChkFullscreen.IsChecked ?? true;
        _settings.SupermodelVsync = ChkVsync.IsChecked ?? true;
        _settings.SupermodelWideScreen = ChkWideScreen.IsChecked ?? true;
        _settings.SupermodelStretch = ChkStretch.IsChecked ?? false;
        _settings.SupermodelResX = (int)(NumResX.Value ?? 1920);
        _settings.SupermodelResY = (int)(NumResY.Value ?? 1080);

        // Audio
        _settings.SupermodelMusicVolume = (int)SldMusic.Value;
        _settings.SupermodelSoundVolume = (int)SldSound.Value;

        // System
        _settings.SupermodelThrottle = ChkThrottle.IsChecked ?? true;
        _settings.SupermodelMultiThreaded = ChkMultiThreaded.IsChecked ?? true;
        _settings.SupermodelInputSystem = CmbInputSystem.Text;
        _settings.SupermodelPowerPcFrequency = int.Parse(CmbPpcFrequency.Text, CultureInfo.InvariantCulture);

        _settings.SupermodelShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            SupermodelConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Supermodel configuration injection failed for path: {path}");
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
        else
        {
            MessageBoxLibrary.FailedToInjectSupermodelConfiguration();
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.SupermodelConfigurationSavedSuccessfully();
        }
        else
        {
            MessageBoxLibrary.FailedToSaveSupermodelConfiguration();
        }

        Close();
    }
}

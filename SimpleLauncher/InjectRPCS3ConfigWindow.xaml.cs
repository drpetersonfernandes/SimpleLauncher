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

public partial class InjectRpcs3ConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectRpcs3ConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        CmbRenderer.Text = _settings.Rpcs3Renderer;
        CmbResolution.Text = _settings.Rpcs3Resolution;
        CmbAspectRatio.Text = _settings.Rpcs3AspectRatio;
        ChkVsync.IsChecked = _settings.Rpcs3Vsync;
        CmbResolutionScale.Text = _settings.Rpcs3ResolutionScale.ToString(CultureInfo.InvariantCulture);
        CmbAnisotropicFilter.Text = _settings.Rpcs3AnisotropicFilter.ToString(CultureInfo.InvariantCulture);

        // Core
        CmbPpuDecoder.Text = _settings.Rpcs3PpuDecoder;
        CmbSpuDecoder.Text = _settings.Rpcs3SpuDecoder;

        // Audio
        CmbAudioRenderer.Text = _settings.Rpcs3AudioRenderer;
        ChkAudioBuffering.IsChecked = _settings.Rpcs3AudioBuffering;

        // Misc
        ChkStartFullscreen.IsChecked = _settings.Rpcs3StartFullscreen;
        ChkShowBeforeLaunch.IsChecked = _settings.Rpcs3ShowSettingsBeforeLaunch;

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

        MessageBoxLibrary.Rpcs3EmulatorNotFoundPleaseLocate();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "RPCS3 Executable|rpcs3.exe|All Executables|*.exe",
            Title = "Select RPCS3 Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        // Video
        _settings.Rpcs3Renderer = CmbRenderer.Text;
        _settings.Rpcs3Resolution = CmbResolution.Text;
        _settings.Rpcs3AspectRatio = CmbAspectRatio.Text;
        _settings.Rpcs3Vsync = ChkVsync.IsChecked ?? false;
        _settings.Rpcs3ResolutionScale = int.Parse(CmbResolutionScale.Text, CultureInfo.InvariantCulture);
        _settings.Rpcs3AnisotropicFilter = int.Parse(CmbAnisotropicFilter.Text, CultureInfo.InvariantCulture);

        // Core
        _settings.Rpcs3PpuDecoder = CmbPpuDecoder.Text;
        _settings.Rpcs3SpuDecoder = CmbSpuDecoder.Text;

        // Audio
        _settings.Rpcs3AudioRenderer = CmbAudioRenderer.Text;
        _settings.Rpcs3AudioBuffering = ChkAudioBuffering.IsChecked ?? true;

        // Misc
        _settings.Rpcs3StartFullscreen = ChkStartFullscreen.IsChecked ?? false;
        _settings.Rpcs3ShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            Rpcs3ConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"RPCS3 configuration injection failed for path: {path}");
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
            MessageBoxLibrary.FailedToInjectRpcs3Configuration();
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.Rpcs3ConfigurationSavedSuccessfully();
            Close();
        }
        else
        {
            MessageBoxLibrary.FailedToSaveRpcs3Configuration();
        }
    }
}
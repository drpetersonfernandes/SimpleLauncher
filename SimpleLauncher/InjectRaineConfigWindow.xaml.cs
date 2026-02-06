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
    private string _emulatorPath;
    private readonly string _gameFilePath;
    private readonly string _systemRomPath;
    private readonly ILogErrors _logErrors;

    public InjectRaineConfigWindow(SettingsManager settings, string emulatorPath = null, string gameFilePath = null, string systemRomPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _emulatorPath = emulatorPath;
        _gameFilePath = gameFilePath;
        _systemRomPath = systemRomPath;
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
        ChkShowFps.IsChecked = _settings.RaineShowFps;
        CmbFrameSkip.SelectedIndex = Math.Clamp(_settings.RaineFrameSkip, 0, 5);
        TxtNeoCdBios.Text = _settings.RaineNeoCdBios ?? string.Empty;
        NumMusicVolume.Value = _settings.RaineMusicVolume;
        NumSfxVolume.Value = _settings.RaineSfxVolume;
        ChkMuteSfx.IsChecked = _settings.RaineMuteSfx;
        ChkMuteMusic.IsChecked = _settings.RaineMuteMusic;
        TxtRaineRomDirectory.Text = _settings.RaineRomDirectory ?? string.Empty;
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
        _settings.RaineShowFps = ChkShowFps.IsChecked ?? false;
        _settings.RaineFrameSkip = CmbFrameSkip.SelectedIndex;
        _settings.RaineNeoCdBios = TxtNeoCdBios.Text;
        _settings.RaineMusicVolume = (int)(NumMusicVolume.Value ?? 60);
        _settings.RaineSfxVolume = (int)(NumSfxVolume.Value ?? 60);
        _settings.RaineMuteSfx = ChkMuteSfx.IsChecked ?? false;
        _settings.RaineMuteMusic = ChkMuteMusic.IsChecked ?? false;
        _settings.RaineRomDirectory = TxtRaineRomDirectory.Text;
        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return false;

        try
        {
            // Pass the stored paths to the service
            RaineConfigurationService.InjectSettings(path, _settings, _gameFilePath, _systemRomPath, _settings.RaineRomDirectory);
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

    private void BtnSelectNeoCdBios_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "NeoGeo CD BIOS (neocd.bin)|neocd.bin|All Files (*.*)|*.*",
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectNeoCdBios") ?? "Select NeoGeo CD BIOS File"
        };

        if (!string.IsNullOrEmpty(TxtNeoCdBios.Text) && File.Exists(TxtNeoCdBios.Text))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(TxtNeoCdBios.Text);
            dialog.FileName = Path.GetFileName(TxtNeoCdBios.Text);
        }
        else if (!string.IsNullOrEmpty(_emulatorPath))
        {
            // Try to suggest a path relative to the emulator
            var emuDir = Path.GetDirectoryName(_emulatorPath);
            if (emuDir != null)
            {
                var biosPath = Path.Combine(emuDir, "bios", "neocd.bin"); // Common bios location
                if (File.Exists(biosPath))
                {
                    dialog.InitialDirectory = Path.GetDirectoryName(biosPath);
                    dialog.FileName = Path.GetFileName(biosPath);
                }
                else if (Directory.Exists(emuDir))
                {
                    dialog.InitialDirectory = emuDir;
                }
            }
        }

        if (dialog.ShowDialog() == true)
        {
            TxtNeoCdBios.Text = dialog.FileName;
        }
    }

    private void BtnSelectRaineRomDirectory_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectRomDirectory") ?? "Select Raine ROM Directory"
        };

        if (!string.IsNullOrEmpty(TxtRaineRomDirectory.Text) && Directory.Exists(TxtRaineRomDirectory.Text))
        {
            dialog.InitialDirectory = TxtRaineRomDirectory.Text;
        }
        else if (!string.IsNullOrEmpty(_systemRomPath) && Directory.Exists(_systemRomPath))
        {
            dialog.InitialDirectory = _systemRomPath;
        }
        else if (!string.IsNullOrEmpty(_emulatorPath))
        {
            var emuDir = Path.GetDirectoryName(_emulatorPath);
            if (emuDir != null && Directory.Exists(Path.Combine(emuDir, "roms")))
            {
                dialog.InitialDirectory = Path.Combine(emuDir, "roms");
            }
            else if (Directory.Exists(emuDir))
            {
                dialog.InitialDirectory = emuDir;
            }
        }

        if (dialog.ShowDialog() == true)
        {
            TxtRaineRomDirectory.Text = dialog.FolderName;
        }
    }
}
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectSegaModel2ConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectSegaModel2ConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        // Renderer
        NumResX.Value = _settings.SegaModel2ResX;
        NumResY.Value = _settings.SegaModel2ResY;
        SelectComboByTag(CmbWidescreen, _settings.SegaModel2WideScreen.ToString(CultureInfo.InvariantCulture));
        SelectComboByTag(CmbFsaa, _settings.SegaModel2Fsaa.ToString(CultureInfo.InvariantCulture));
        ChkBilinear.IsChecked = _settings.SegaModel2Bilinear;
        ChkTrilinear.IsChecked = _settings.SegaModel2Trilinear;
        ChkFilterTilemaps.IsChecked = _settings.SegaModel2FilterTilemaps;
        ChkDrawCross.IsChecked = _settings.SegaModel2DrawCross;

        // Input
        ChkXInput.IsChecked = _settings.SegaModel2XInput;
        ChkEnableFf.IsChecked = _settings.SegaModel2EnableFf;
        ChkHoldGears.IsChecked = _settings.SegaModel2HoldGears;
        ChkUseRawInput.IsChecked = _settings.SegaModel2UseRawInput;

        ChkShowBeforeLaunch.IsChecked = _settings.SegaModel2ShowSettingsBeforeLaunch;

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

        MessageBoxLibrary.SegaModel2EmulatorNotFound();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "SEGA Model 2 Executable|emulator.exe|All Executables|*.exe",
            Title = "Select SEGA Model 2 Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        // Renderer
        _settings.SegaModel2ResX = (int)(NumResX.Value ?? 640);
        _settings.SegaModel2ResY = (int)(NumResY.Value ?? 480);
        _settings.SegaModel2WideScreen = int.Parse(GetSelectedTag(CmbWidescreen), CultureInfo.InvariantCulture);
        _settings.SegaModel2Fsaa = int.Parse(GetSelectedTag(CmbFsaa), CultureInfo.InvariantCulture);
        _settings.SegaModel2Bilinear = ChkBilinear.IsChecked ?? true;
        _settings.SegaModel2Trilinear = ChkTrilinear.IsChecked ?? false;
        _settings.SegaModel2FilterTilemaps = ChkFilterTilemaps.IsChecked ?? false;
        _settings.SegaModel2DrawCross = ChkDrawCross.IsChecked ?? true;

        // Input
        _settings.SegaModel2XInput = ChkXInput.IsChecked ?? false;
        _settings.SegaModel2EnableFf = ChkEnableFf.IsChecked ?? false;
        _settings.SegaModel2HoldGears = ChkHoldGears.IsChecked ?? false;
        _settings.SegaModel2UseRawInput = ChkUseRawInput.IsChecked ?? false;

        _settings.SegaModel2ShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            SegaModel2ConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"SEGA Model 2 configuration injection failed for path: {path}");
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
            MessageBoxLibrary.FailedToInjectSegaModel2Configuration();
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.SegaModel2ConfigurationSavedSuccessfully();
        }
        else
        {
            MessageBoxLibrary.FailedToSaveSegaModel2Configuration();
        }

        Close();
    }

    private static void SelectComboByTag(ComboBox cmb, string tagValue)
    {
        foreach (var item in cmb.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag?.ToString() == tagValue)
            {
                cmb.SelectedItem = item;
                return;
            }
        }

        if (cmb.Items.Count > 0)
        {
            cmb.SelectedIndex = 0;
        }
    }

    private static string GetSelectedTag(ComboBox cmb)
    {
        return (cmb.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "0";
    }
}

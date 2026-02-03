using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls; // Added for ComboBox and ComboBoxItem
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectDuckStationConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectDuckStationConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        // General
        ChkFullscreen.IsChecked = _settings.DuckStationStartFullscreen;
        ChkPauseOnFocusLoss.IsChecked = _settings.DuckStationPauseOnFocusLoss;
        ChkSaveStateOnExit.IsChecked = _settings.DuckStationSaveStateOnExit;
        ChkRewindEnable.IsChecked = _settings.DuckStationRewindEnable;
        SldRunahead.Value = _settings.DuckStationRunaheadFrameCount;

        // Video/GPU - Select by matching content to ensure SelectedItem is set
        SelectComboBoxItemByContent(CmbRenderer, _settings.DuckStationRenderer);
        SelectComboBoxItemByContent(CmbResolutionScale, _settings.DuckStationResolutionScale.ToString(CultureInfo.InvariantCulture));
        SelectComboBoxItemByContent(CmbTextureFilter, _settings.DuckStationTextureFilter);
        SelectComboBoxItemByContent(CmbAspectRatio, _settings.DuckStationAspectRatio);
        ChkWidescreenHack.IsChecked = _settings.DuckStationWidescreenHack;
        ChkPgxpEnable.IsChecked = _settings.DuckStationPgxpEnable;
        ChkVsync.IsChecked = _settings.DuckStationVsync;

        // Audio
        ChkMute.IsChecked = _settings.DuckStationOutputMuted;
        SldVolume.Value = _settings.DuckStationOutputVolume;

        ChkShowBeforeLaunch.IsChecked = _settings.DuckStationShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }

        return;

        static void SelectComboBoxItemByContent(ComboBox comboBox, string content)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content?.ToString() == content)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            // Fallback: set text (may result in null SelectedItem)
            comboBox.Text = content;
        }
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        MessageBoxLibrary.DuckStationEmulatorNotFound();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "DuckStation Executable|duckstation*.exe|All Executables|*.exe",
            Title = "Select DuckStation Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        // General
        _settings.DuckStationStartFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.DuckStationPauseOnFocusLoss = ChkPauseOnFocusLoss.IsChecked ?? true;
        _settings.DuckStationSaveStateOnExit = ChkSaveStateOnExit.IsChecked ?? true;
        _settings.DuckStationRewindEnable = ChkRewindEnable.IsChecked ?? false;
        _settings.DuckStationRunaheadFrameCount = (int)SldRunahead.Value;

        // Video/GPU
        _settings.DuckStationRenderer = CmbRenderer.SelectedItem is ComboBoxItem rendererItem
            ? rendererItem.Content?.ToString() ?? "Automatic"
            : "Automatic";

        // Use Tag property instead of Text to handle descriptive content like "2x (720p)"
        var selectedItem = CmbResolutionScale.SelectedItem as ComboBoxItem;
        if (selectedItem?.Tag != null && int.TryParse(selectedItem.Tag.ToString(), out var scale))
        {
            _settings.DuckStationResolutionScale = scale;
        }
        else
        {
            _settings.DuckStationResolutionScale = 2; // Default fallback
        }

        _settings.DuckStationTextureFilter = CmbTextureFilter.SelectedItem is ComboBoxItem filterItem
            ? filterItem.Content?.ToString() ?? "Nearest"
            : "Nearest";

        _settings.DuckStationAspectRatio = CmbAspectRatio.SelectedItem is ComboBoxItem aspectItem
            ? aspectItem.Content?.ToString() ?? "16:9"
            : "16:9";

        _settings.DuckStationWidescreenHack = ChkWidescreenHack.IsChecked ?? false;
        _settings.DuckStationPgxpEnable = ChkPgxpEnable.IsChecked ?? false;
        _settings.DuckStationVsync = ChkVsync.IsChecked ?? false;

        // Audio
        _settings.DuckStationOutputMuted = ChkMute.IsChecked ?? false;
        _settings.DuckStationOutputVolume = (int)SldVolume.Value;

        _settings.DuckStationShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            DuckStationConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"DuckStation configuration injection failed for path: {path}");
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
            MessageBoxLibrary.FailedToInjectDuckStationConfiguration();
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.DuckStationConfigurationSavedSuccessfully();
        }
        else
        {
            MessageBoxLibrary.FailedToSaveDuckStationConfiguration();
        }

        Close();
    }
}
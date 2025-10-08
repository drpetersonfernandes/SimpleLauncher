using System;
using System.Windows;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class SetGamepadDeadZoneWindow
{
    private readonly SettingsManager _settingsManager;

    public SetGamepadDeadZoneWindow(SettingsManager settings)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _settingsManager = settings ?? throw new ArgumentNullException(nameof(settings));
        LoadDeadZones();
    }

    private void LoadDeadZones()
    {
        // Load the dead zone values from settings and update the sliders.
        DeadZoneXSlider.Value = _settingsManager.DeadZoneX;
        DeadZoneYSlider.Value = _settingsManager.DeadZoneY;
    }

    private void SaveDeadZoneButton_Click(object sender, RoutedEventArgs e)
    {
        // Use the slider values; they are already validated by the slider's range.
        _settingsManager.DeadZoneX = (float)DeadZoneXSlider.Value;
        _settingsManager.DeadZoneY = (float)DeadZoneYSlider.Value;
        _settingsManager.Save();

        MessageBoxLibrary.DeadZonesSavedMessageBox();
    }

    private void RevertDeadZoneButton_Click(object sender, RoutedEventArgs e)
    {
        // Revert settings to the defaults.
        _settingsManager.DeadZoneX = SettingsManager.DefaultDeadZoneX;
        _settingsManager.DeadZoneY = SettingsManager.DefaultDeadZoneY;

        // Update the sliders to show the default values.
        DeadZoneXSlider.Value = SettingsManager.DefaultDeadZoneX;
        DeadZoneYSlider.Value = SettingsManager.DefaultDeadZoneY;

        _settingsManager.Save();
    }
}
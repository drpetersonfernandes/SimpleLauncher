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
        // Define default values.
        const float defaultDeadZoneX = 0.05f;
        const float defaultDeadZoneY = 0.02f;

        // Revert settings to the defaults.
        _settingsManager.DeadZoneX = defaultDeadZoneX;
        _settingsManager.DeadZoneY = defaultDeadZoneY;

        // Update the sliders to show the default values.
        DeadZoneXSlider.Value = defaultDeadZoneX;
        DeadZoneYSlider.Value = defaultDeadZoneY;

        _settingsManager.Save();
    }
}
using System;
using System.Windows;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.Utils;

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

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingGamepadDeadZoneSettings") ?? "Saving gamepad dead zone settings...", Application.Current.MainWindow as MainWindow);
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

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("RevertingGamepadDeadZoneSettings") ?? "Reverting gamepad dead zone settings...", Application.Current.MainWindow as MainWindow);
    }
}
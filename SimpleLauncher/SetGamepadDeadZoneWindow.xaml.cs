using System;
using System.Diagnostics;
using System.Windows;

namespace SimpleLauncher;

public partial class SetGamepadDeadZoneWindow
{
    private readonly SettingsConfig _settingsConfig;

    public SetGamepadDeadZoneWindow(SettingsConfig settings)
    {
        InitializeComponent();

        _settingsConfig = settings;
        LoadDeadZones();

        Closing += SetGamePadDeadZone_Closing;
    }

    private void LoadDeadZones()
    {
        // Load the dead zone values from settings and update the sliders.
        DeadZoneXSlider.Value = _settingsConfig.DeadZoneX;
        DeadZoneYSlider.Value = _settingsConfig.DeadZoneY;
    }

    private void SaveDeadZoneButton_Click(object sender, RoutedEventArgs e)
    {
        // Use the slider values; they are already validated by the slider's range.
        _settingsConfig.DeadZoneX = (float)DeadZoneXSlider.Value;
        _settingsConfig.DeadZoneY = (float)DeadZoneYSlider.Value;
        _settingsConfig.Save();

        MessageBoxLibrary.DeadZonesSavedMessageBox();
    }

    private void RevertDeadZoneButton_Click(object sender, RoutedEventArgs e)
    {
        // Define default values.
        const float defaultDeadZoneX = 0.05f;
        const float defaultDeadZoneY = 0.02f;

        // Revert settings to the defaults.
        _settingsConfig.DeadZoneX = defaultDeadZoneX;
        _settingsConfig.DeadZoneY = defaultDeadZoneY;

        // Update the sliders to show the default values.
        DeadZoneXSlider.Value = defaultDeadZoneX;
        DeadZoneYSlider.Value = defaultDeadZoneY;

        _settingsConfig.Save();
    }

    private static void SetGamePadDeadZone_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule != null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = processModule.FileName,
                UseShellExecute = true
            };

            Process.Start(startInfo);

            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}
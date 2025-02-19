using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace SimpleLauncher;

public partial class SetGamepadDeadZone
{
    private readonly SettingsConfig _settingsConfig;
    public SetGamepadDeadZone(SettingsConfig settings)
    {
        InitializeComponent();
        
        _settingsConfig = settings;
        LoadDeadZones();
        
        Closing += EditLinks_Closing;
    }
    
    private void LoadDeadZones()
    {
        DeadZoneXTextBox.Text = _settingsConfig.DeadZoneX.ToString(CultureInfo.InvariantCulture);
        DeadZoneYTextBox.Text = _settingsConfig.DeadZoneY.ToString(CultureInfo.InvariantCulture);
    }

    private void SaveDeadZoneButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsConfig.DeadZoneX = Convert.ToSingle(DeadZoneXTextBox.Text);
        _settingsConfig.DeadZoneY = Convert.ToSingle(DeadZoneYTextBox.Text);
        
        _settingsConfig.Save();
        
        // Notify user
        MessageBoxLibrary.LinksSavedMessageBox();
    }
    
    private void RevertDeadZoneButton_Click(object sender, RoutedEventArgs e)
    {
        _settingsConfig.DeadZoneX = 0.05f;
        _settingsConfig.DeadZoneY = 0.02f;

        DeadZoneXTextBox.Text = "0.05";
        DeadZoneYTextBox.Text = "0.02";

        _settingsConfig.Save();

        // Notify user
        MessageBoxLibrary.LinksRevertedMessageBox();
    }

    private static void EditLinks_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule == null) return;
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
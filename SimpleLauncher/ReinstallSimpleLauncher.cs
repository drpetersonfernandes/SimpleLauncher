using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SimpleLauncher;

public static class ReinstallSimpleLauncher
{
    public static void StartUpdaterAndShutdown()
    {
        string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");

        if (File.Exists(updaterPath))
        {
            Process.Start(updaterPath);

            // Shutdown SimpleLauncher instance
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
        else
        {
            MessageBox.Show("'Updater.exe' not found.\n\n" +
                            "Please reinstall 'Simple Launcher' manually to fix the problem.\n\n" +
                            "The application will now shut down.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Shutdown the application and exit
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
}
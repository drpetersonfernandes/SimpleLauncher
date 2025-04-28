using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SimpleLauncher.Services;

public static class ReinstallSimpleLauncher
{
    public static void StartUpdaterAndShutdown()
    {
        var updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");

        if (File.Exists(updaterPath))
        {
            Process.Start(updaterPath);

            // Shutdown SimpleLauncher instance
            Application.Current.Shutdown();
            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        }
        else
        {
            // Notify user
            MessageBoxLibrary.UpdaterNotFoundMessageBox();
        }
    }
}
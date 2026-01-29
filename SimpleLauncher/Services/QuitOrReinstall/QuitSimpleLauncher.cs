using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.QuitOrReinstall;

public static class QuitSimpleLauncher
{
    public static void RestartApplication()
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule == null) return;

        var startInfo = new ProcessStartInfo
        {
            FileName = processModule.FileName,
            // Add the "--restarting" argument to signal the new instance
            Arguments = "--restarting",
            UseShellExecute = true
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to start new process during application restart.");

            // Notify user
            MessageBoxLibrary.FailedToRestartMessageBox();

            // Don't shut down the current instance if the new one couldn't start
            return;
        }


        // Shutdown the current instance
        Application.Current.Shutdown();
    }

    public static void SimpleQuitApplication()
    {
        Application.Current.Shutdown();
    }

    // A robust way to launch the updater and exit immediately.
    public static void ShutdownForUpdate(string updaterPath)
    {
        try
        {
            var startInfo = new ProcessStartInfo(updaterPath)
            {
                Arguments = Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                UseShellExecute = true
            };
            Process.Start(startInfo);

            // Use Dispatcher to ensure shutdown happens on the UI thread,
            // then forcefully exit.
            Application.Current.Dispatcher.Invoke(static () =>
            {
                Application.Current.Shutdown();
                Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
            });
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to start updater and shut down.");

            // // If the updater fails to launch, we should inform the user and NOT shut down.
            // MessageBoxLibrary.UpdaterLaunchFailedMessageBox();
        }
    }
}

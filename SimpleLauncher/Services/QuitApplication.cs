using System;
using System.Diagnostics;
using System.Windows;

namespace SimpleLauncher.Services;

public static class QuitApplication
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
            _ = LogErrors.LogErrorAsync(ex, "Failed to start new process during application restart.");

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

    public static void ForcefullyQuitApplication()
    {
        foreach (Window window in Application.Current.Windows)
        {
            window.Close(); // Close each window
        }

        GC.Collect(); // Force garbage collection
        GC.WaitForPendingFinalizers(); // Wait for finalizers to complete

        Application.Current.Shutdown(); // Shutdown the application
    }
}
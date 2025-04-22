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
        // Environment.Exit(0); // Shutdown() is usually sufficient for WPF applications
    }

    public static void SimpleQuitApplication()
    {
        Application.Current.Shutdown();
        // Environment.Exit(0); // Shutdown() is usually sufficient for WPF applications
    }

    public static void ForcefullyQuitApplication()
    {
        // Note: Forcefully killing the process might prevent proper cleanup (like mutex release)
        // Consider if this is truly necessary or if SimpleQuitApplication is sufficient.
        // If you use this, the mutex might remain orphaned until the system cleans it up.
        // For a clean restart, the RestartApplication method is preferred.

        foreach (Window window in Application.Current.Windows)
        {
            window.Close(); // Close each window
        }

        // Give some time for windows to close and resources to be released
        // Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle); // Requires System.Windows.Threading

        GC.Collect(); // Force garbage collection
        GC.WaitForPendingFinalizers(); // Wait for finalizers to complete

        Application.Current.Shutdown(); // Shutdown the application

        // Only kill if absolutely necessary and Shutdown didn't work
        try
        {
            Process.GetCurrentProcess().Kill(); // Forcefully kill the process
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to forcefully kill process.");
        }
    }
}
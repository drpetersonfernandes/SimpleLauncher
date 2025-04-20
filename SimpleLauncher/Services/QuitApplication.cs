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
            UseShellExecute = true
        };

        Process.Start(startInfo);

        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    public static void SimpleQuitApplication()
    {
        Application.Current.Shutdown();
        Environment.Exit(0);
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
        Process.GetCurrentProcess().Kill(); // Forcefully kill the process
    }
}
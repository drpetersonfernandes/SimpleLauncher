using System.ComponentModel;
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
            Arguments = "--restarting",
            UseShellExecute = true,
            WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
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
        catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access Denied
        {
            // Log the initial access denied error
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>()
                .LogErrorAsync(ex, "Access denied when starting Updater.exe. Attempting to restart with elevation.");

            try
            {
                // Retry with elevated privileges (UAC prompt)
                var elevatedStartInfo = new ProcessStartInfo(updaterPath)
                {
                    Arguments = Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                    UseShellExecute = true,
                    Verb = "runas" // Request administrator privileges
                };
                Process.Start(elevatedStartInfo);

                // If elevation succeeded, shutdown
                Application.Current.Dispatcher.Invoke(static () =>
                {
                    Application.Current.Shutdown();
                    Process.GetCurrentProcess().Kill();
                    Environment.Exit(0);
                });
            }
            catch (Exception elevationEx)
            {
                // Log the elevation attempt failure
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>()
                    .LogErrorAsync(elevationEx, "Failed to start Updater.exe even with elevation.");

                // Notify user that update failed
                MessageBoxLibrary.UpdaterLaunchFailedMessageBox();
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to start updater and shut down.");

            // Notify user that update failed
            MessageBoxLibrary.UpdaterLaunchFailedMessageBox();
        }
    }
}

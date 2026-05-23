using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CheckForUpdates;
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

    // Downloads a fresh Updater.exe from GitHub first, falling back to the local copy if offline.
    // Then launches the updater and forcefully exits the current process.
    public static async Task ShutdownForUpdateAsync(string updaterPath)
    {
        var appDirectory = Path.GetDirectoryName(updaterPath) ?? AppDomain.CurrentDomain.BaseDirectory;

        // 1. Try to download a fresh Updater.exe from GitHub (overwrites any existing copy)
        var downloaded = false;
        try
        {
            var updateChecker = App.ServiceProvider.GetRequiredService<UpdateChecker>();
            var (updaterZipUrl, _) = await updateChecker.GetLatestUpdaterInfoAsync();

            if (!string.IsNullOrEmpty(updaterZipUrl))
            {
                using var memoryStream = new MemoryStream();
                await updateChecker.DownloadUpdateFileToMemoryAsync(updaterZipUrl, memoryStream);
                UpdateChecker.ExtractAllFromZip(memoryStream, appDirectory, null);
                if (File.Exists(updaterPath))
                {
                    downloaded = true;
                }
            }
        }
        catch
        {
            // GitHub unreachable or download failed — will fall back to local copy
        }

        // 2. If neither downloaded nor local file exists, notify and return
        if (!downloaded && !File.Exists(updaterPath))
        {
            MessageBoxLibrary.UpdaterLaunchFailedMessageBox();
            return;
        }

        // 3. Launch Updater.exe and shut down
        try
        {
            var startInfo = new ProcessStartInfo(updaterPath)
            {
                Arguments = Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                UseShellExecute = true,
                WorkingDirectory = appDirectory
            };
            Process.Start(startInfo);

            Application.Current.Dispatcher.Invoke(static () =>
            {
                Application.Current.Shutdown();
                Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
            });
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access Denied
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>()
                .LogErrorAsync(ex, "Access denied when starting Updater.exe.");

            MessageBoxLibrary.UpdaterLaunchFailedMessageBox();
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to start updater and shut down.");

            MessageBoxLibrary.UpdaterLaunchFailedMessageBox();
        }
    }
}

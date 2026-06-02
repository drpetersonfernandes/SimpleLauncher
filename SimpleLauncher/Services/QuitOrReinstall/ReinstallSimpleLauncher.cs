using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.QuitOrReinstall;

public static class ReinstallSimpleLauncher
{
    public static async void StartUpdaterAndShutdown()
    {
        try
        {
            var logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
            var updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");

            if (File.Exists(updaterPath))
            {
                try
                {
                    var startInfo = new ProcessStartInfo(updaterPath)
                    {
                        Arguments = Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                        UseShellExecute = true,
                        WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                    };
                    Process.Start(startInfo);

                    ShutdownApplication();
                }
                catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access Denied
                {
                    // Log the access denied error
                    logErrors.LogAndForget(ex, "Access denied when starting Updater.exe.");

                    // Notify user that update failed
                    MessageBoxLibrary.UpdaterLaunchFailedMessageBox();
                }
            }
            else
            {
                try
                {
                    var updateChecker = App.ServiceProvider.GetRequiredService<CheckForUpdates.UpdateChecker>();

                    // 1. Get the URL from GitHub
                    var (updaterZipUrl, _) = await updateChecker.GetLatestUpdaterInfoAsync();

                    if (string.IsNullOrEmpty(updaterZipUrl))
                    {
                        MessageBoxLibrary.CouldNotFindUpdaterOnGitHubMessageBox();
                        return;
                    }

                    // 2. Download the updater file into memory
                    using var memoryStream = new MemoryStream();
                    await updateChecker.DownloadUpdateFileToMemoryAsync(updaterZipUrl, memoryStream);

                    // 3. Extract the contents to the application directory
                    var extractionSuccess = CheckForUpdates.UpdateChecker.ExtractAllFromZip(memoryStream, AppDomain.CurrentDomain.BaseDirectory, null, App.ServiceProvider.GetRequiredService<ILogErrors>());

                    if (!extractionSuccess)
                    {
                        // Notify user
                        MessageBoxLibrary.InstallUpdateManuallyMessageBox();

                        return;
                    }

                    // 4. Verify Updater.exe now exists and launches it
                    if (File.Exists(updaterPath))
                    {
                        try
                        {
                            var startInfo = new ProcessStartInfo(updaterPath)
                            {
                                Arguments = Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                                UseShellExecute = true,
                                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                            };
                            Process.Start(startInfo);

                            ShutdownApplication();
                        }
                        catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access Denied
                        {
                            // Log the access denied error
                            logErrors.LogAndForget(ex, "Access denied when starting Updater.exe after download.");

                            // Notify user that update failed
                            MessageBoxLibrary.UpdaterLaunchFailedMessageBox();
                        }
                    }
                    else
                    {
                        // Notify user
                        MessageBoxLibrary.InstallUpdateManuallyMessageBox();
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    logErrors.LogAndForget(ex, "Failed to download and reinstall the updater.");

                    // Notify user
                    MessageBoxLibrary.InstallUpdateManuallyMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            var logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

            // Notify developer
            logErrors.LogAndForget(ex, "Failed to reinstall SimpleLauncher.");

            // Notify user
            MessageBoxLibrary.InstallUpdateManuallyMessageBox();
        }
    }

    private static void ShutdownApplication()
    {
        // Use Dispatcher to ensure shutdown happens on the UI thread.
        Application.Current.Dispatcher.Invoke(static () =>
        {
            Application.Current.Shutdown();
            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        });
    }
}
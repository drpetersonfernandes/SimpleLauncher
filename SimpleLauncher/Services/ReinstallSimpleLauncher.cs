using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;

namespace SimpleLauncher.Services;

public static class ReinstallSimpleLauncher
{
    public static async void StartUpdaterAndShutdown()
    {
        try
        {
            var updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Updater.exe");

            if (File.Exists(updaterPath))
            {
                var startInfo = new ProcessStartInfo(updaterPath)
                {
                    Arguments = Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                    UseShellExecute = true
                };
                Process.Start(startInfo);

                ShutdownApplication();
            }
            else
            {
                // Updater.exe is missing, attempt to download it.
                MessageBoxLibrary.UpdaterIsMissingAttemptingDownload();

                try
                {
                    // 1. Get the URL for updater.zip from GitHub
                    var (updaterZipUrl, _) = await UpdateChecker.GetLatestUpdaterInfoAsync();

                    if (string.IsNullOrEmpty(updaterZipUrl))
                    {
                        MessageBoxLibrary.CouldNotFindUpdaterOnGitHub();
                        return;
                    }

                    // 2. Download the updater.zip file into memory
                    using var memoryStream = new MemoryStream();
                    await UpdateChecker.DownloadUpdateFileToMemoryAsync(updaterZipUrl, memoryStream);

                    // 3. Extract the contents to the application directory
                    var extractionSuccess = UpdateChecker.ExtractAllFromZip(memoryStream, AppDomain.CurrentDomain.BaseDirectory, null);

                    if (!extractionSuccess)
                    {
                        // Notify user
                        MessageBoxLibrary.InstallUpdateManuallyMessageBox();

                        return;
                    }

                    // 4. Verify Updater.exe now exists and launches it
                    if (File.Exists(updaterPath))
                    {
                        var startInfo = new ProcessStartInfo(updaterPath)
                        {
                            Arguments = Environment.ProcessId.ToString(CultureInfo.InvariantCulture),
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);

                        ShutdownApplication();
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
                    _ = LogErrors.LogErrorAsync(ex, "Failed to download and reinstall the updater.");

                    // Notify user
                    MessageBoxLibrary.InstallUpdateManuallyMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Failed to reinstall SimpleLauncher.");

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

using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.QuitOrReinstall;

public class ReinstallSimpleLauncher
{
    private readonly ILogErrors _logErrors;
    private readonly IApplicationLifetime _applicationLifetime;
    private readonly IDispatcherService _dispatcherService;
    private readonly IServiceProvider _serviceProvider;

    public ReinstallSimpleLauncher(ILogErrors logErrors, IApplicationLifetime applicationLifetime, IDispatcherService dispatcherService, IServiceProvider serviceProvider)
    {
        _logErrors = logErrors;
        _applicationLifetime = applicationLifetime;
        _dispatcherService = dispatcherService;
        _serviceProvider = serviceProvider;
    }

    public async void StartUpdaterAndShutdown()
    {
        try
        {
            var messageBoxLibrary = _serviceProvider.GetRequiredService<IMessageBoxLibraryService>();
            try
            {
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
                        _logErrors.LogAndForget(ex, "Access denied when starting Updater.exe.");

                        // Notify user that update failed
                        await messageBoxLibrary.UpdaterLaunchFailedMessageBox();
                    }
                }
                else
                {
                    try
                    {
                        var updateChecker = _serviceProvider.GetRequiredService<CheckForUpdates.UpdateChecker>();

                        // 1. Get the URL from GitHub
                        var (updaterZipUrl, _) = await updateChecker.GetLatestUpdaterInfoAsync();

                        if (string.IsNullOrEmpty(updaterZipUrl))
                        {
                            await messageBoxLibrary.CouldNotFindUpdaterOnGitHubMessageBox();
                            return;
                        }

                        // 2. Download the updater file into memory
                        using var memoryStream = new MemoryStream();
                        await updateChecker.DownloadUpdateFileToMemoryAsync(updaterZipUrl, memoryStream);

                        // 3. Extract the contents to the application directory
                        var extractionSuccess = CheckForUpdates.UpdateChecker.ExtractAllFromZip(memoryStream, AppDomain.CurrentDomain.BaseDirectory, null, _logErrors);

                        if (!extractionSuccess)
                        {
                            // Notify user
                            await messageBoxLibrary.InstallUpdateManuallyMessageBox();

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
                                _logErrors.LogAndForget(ex, "Access denied when starting Updater.exe after download.");

                                // Notify user that update failed
                                await messageBoxLibrary.UpdaterLaunchFailedMessageBox();
                            }
                        }
                        else
                        {
                            // Notify user
                            await messageBoxLibrary.InstallUpdateManuallyMessageBox();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        _logErrors.LogAndForget(ex, "Failed to download and reinstall the updater.");

                        // Notify user
                        await messageBoxLibrary.InstallUpdateManuallyMessageBox();
                    }
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                _logErrors.LogAndForget(ex, "Failed to reinstall SimpleLauncher.");

                // Notify user
                await messageBoxLibrary.InstallUpdateManuallyMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method StartUpdaterAndShutdown.");
        }
    }

    private void ShutdownApplication()
    {
        // Use Dispatcher to ensure shutdown happens on the UI thread.
        _dispatcherService.Invoke(() =>
        {
            _applicationLifetime.Shutdown();
            Process.GetCurrentProcess().Kill();
            Environment.Exit(0);
        });
    }
}

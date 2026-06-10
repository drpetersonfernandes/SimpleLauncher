using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CheckForUpdates;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.QuitOrReinstall;

public class QuitSimpleLauncher
{
    private readonly ILogErrors _logErrors;
    private readonly IApplicationLifetime _applicationLifetime;
    private readonly IDispatcherService _dispatcherService;
    private readonly Lazy<UpdateChecker> _updateChecker;

    public QuitSimpleLauncher(ILogErrors logErrors, IApplicationLifetime applicationLifetime, IDispatcherService dispatcherService, Lazy<UpdateChecker> updateChecker)
    {
        _logErrors = logErrors;
        _applicationLifetime = applicationLifetime;
        _dispatcherService = dispatcherService;
        _updateChecker = updateChecker;
    }

    public async Task RestartApplication(IMessageBoxLibraryService messageBox)
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
            _logErrors.LogAndForget(ex, "Failed to start new process during application restart.");

            // Notify user
            await messageBox.FailedToRestartMessageBox();

            // Don't shut down the current instance if the new one couldn't start
            return;
        }


        // Shutdown the current instance
        _applicationLifetime.Shutdown();
    }

    public void SimpleQuitApplication()
    {
        _applicationLifetime.Shutdown();
    }

    // Downloads a fresh Updater.exe from GitHub first, falling back to the local copy if offline.
    // Then launches the updater and forcefully exits the current process.
    public async Task ShutdownForUpdateAsync(string updaterPath, IMessageBoxLibraryService messageBox)
    {
        var appDirectory = Path.GetDirectoryName(updaterPath) ?? AppDomain.CurrentDomain.BaseDirectory;

        // 1. Try to download a fresh Updater.exe from GitHub (overwrites any existing copy)
        var downloaded = false;
        try
        {
            var updateChecker = _updateChecker.Value;
            var (updaterZipUrl, _) = await updateChecker.GetLatestUpdaterInfoAsync();

            if (!string.IsNullOrEmpty(updaterZipUrl))
            {
                using var memoryStream = new MemoryStream();
                await updateChecker.DownloadUpdateFileToMemoryAsync(updaterZipUrl, memoryStream);
                UpdateChecker.ExtractAllFromZip(memoryStream, appDirectory, null, _logErrors);
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
            await messageBox.UpdaterLaunchFailedMessageBox();
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

            _dispatcherService.Invoke(() =>
            {
                _applicationLifetime.Shutdown();
                Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
            });
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access Denied
        {
            _logErrors.LogAndForget(ex, "Access denied when starting Updater.exe.");

            await messageBox.UpdaterLaunchFailedMessageBox();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Failed to start updater and shut down.");

            await messageBox.UpdaterLaunchFailedMessageBox();
        }
    }
}

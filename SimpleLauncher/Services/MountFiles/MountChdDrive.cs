using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.MountFiles;

/// <summary>
/// Represents a temporarily mounted CHD drive.
/// Disposing this object will unmount the drive by terminating the CHDMounter process.
/// </summary>
public class MountChdDrive : IAsyncDisposable
{
    private readonly Process _mountProcess;
    private readonly int _mountProcessId;

    public string MountedPath { get; }
    public string MountedDriveLetter { get; }
    public bool IsMounted { get; }

    /// <summary>
    /// Constructor for a successful mount.
    /// </summary>
    public MountChdDrive(Process mountProcess, string mountedPath, string mountedDriveLetter)
    {
        _mountProcess = mountProcess;
        _mountProcessId = mountProcess?.Id ?? -1;
        MountedPath = mountedPath;
        MountedDriveLetter = mountedDriveLetter;
        IsMounted = !string.IsNullOrEmpty(mountedPath) && _mountProcess != null;
    }

    /// <summary>
    /// Constructor for a failed mount.
    /// </summary>
    public MountChdDrive()
    {
        IsMounted = false;
    }

    public async ValueTask DisposeAsync()
    {
        if (!IsMounted || _mountProcess == null)
        {
            return;
        }

        var processExitedBeforeKill = false;
        try
        {
            try
            {
                _mountProcess.Kill(true);
                DebugLogger.Log($"[MountChdDrive.DisposeAsync] Kill signal sent to CHDMounter (ID: {_mountProcessId}).");
            }
            catch (InvalidOperationException)
            {
                processExitedBeforeKill = true;
                DebugLogger.Log($"[MountChdDrive.DisposeAsync] CHDMounter (ID: {_mountProcessId}) had already exited before Kill could complete.");
            }
            catch (ArgumentException)
            {
                processExitedBeforeKill = true;
                DebugLogger.Log($"[MountChdDrive.DisposeAsync] CHDMounter (ID: {_mountProcessId}) had already exited before explicit unmount was needed.");
            }

            if (!processExitedBeforeKill)
            {
                DebugLogger.Log($"[MountChdDrive.DisposeAsync] Waiting for CHDMounter (ID: {_mountProcessId}) to exit (up to 10s).");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    await _mountProcess.WaitForExitAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    DebugLogger.Log($"[MountChdDrive.DisposeAsync] Timeout (10s) waiting for CHDMounter (ID: {_mountProcessId}) to exit after Kill.");
                }

                if (_mountProcess.HasExited)
                {
                    DebugLogger.Log($"[MountChdDrive.DisposeAsync] CHDMounter (ID: {_mountProcessId}) terminated. Exit code: {_mountProcess.ExitCode}.");
                }
                else
                {
                    DebugLogger.Log($"[MountChdDrive.DisposeAsync] CHDMounter (ID: {_mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                }
            }
        }
        catch (Exception termEx)
        {
            DebugLogger.Log($"[MountChdDrive.DisposeAsync] Exception while terminating CHDMounter (ID: {_mountProcessId}): {termEx}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(termEx,
                $"Failed to terminate CHDMounter (ID: {_mountProcessId}) for unmounting.");
        }
        finally
        {
            _mountProcess.Dispose();
        }

        if (!string.IsNullOrEmpty(MountedDriveLetter))
        {
            var driveRoot = $"{MountedDriveLetter}:\\";
            await Task.Delay(1000);
            if (Directory.Exists(driveRoot))
            {
                DebugLogger.Log($"[MountChdDrive.DisposeAsync] WARNING: Drive {driveRoot} still exists after attempting to unmount.");
            }
            else
            {
                DebugLogger.Log($"[MountChdDrive.DisposeAsync] Drive {driveRoot} successfully unmounted.");
            }
        }

        GC.SuppressFinalize(this);
    }
}

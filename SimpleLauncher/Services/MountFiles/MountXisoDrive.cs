using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.MountFiles;

/// <inheritdoc />
/// <summary>
/// Represents a temporarily mounted XISO drive.
/// Disposing this object will unmount the drive by terminating the mounting process.
/// </summary>
public class MountXisoDrive : IAsyncDisposable
{
    private readonly Process _mountProcess;
    private readonly int _mountProcessId;

    public string MountedPath { get; }
    public bool IsMounted { get; }

    /// <summary>
    /// Constructor for a successful mount.
    /// </summary>
    public MountXisoDrive(Process mountProcess, string mountedPath)
    {
        _mountProcess = mountProcess;
        _mountProcessId = mountProcess?.Id ?? -1;
        MountedPath = mountedPath;
        IsMounted = !string.IsNullOrEmpty(mountedPath) && _mountProcess != null;
    }

    /// <summary>
    /// Constructor for a failed mount.
    /// </summary>
    public MountXisoDrive()
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
            // Attempt to terminate the process without checking HasExited first.
            // This avoids a race condition where the process exits between the check
            // and the Kill() call. We handle the "already exited" case via catch blocks.
            try
            {
                _mountProcess.Kill(true);
                DebugLogger.Log($"[MountXisoDrive.DisposeAsync] Kill signal sent to mounting tool (ID: {_mountProcessId}).");
            }
            catch (InvalidOperationException)
            {
                // Thrown when the process has already exited before Kill() was invoked
                processExitedBeforeKill = true;
                DebugLogger.Log($"[MountXisoDrive.DisposeAsync] Mounting tool (ID: {_mountProcessId}) had already exited before Kill could complete (race condition handled).");
            }
            catch (ArgumentException)
            {
                // Thrown when the process is not associated with a valid handle (already exited/disposed)
                processExitedBeforeKill = true;
                DebugLogger.Log(
                    $"[MountXisoDrive.DisposeAsync] Mounting tool (ID: {_mountProcessId}) had already exited before explicit unmount was needed.");
            }

            // Only wait for exit if we actually sent a kill signal.
            // If the process was already gone, we skip the wait logic.
            if (!processExitedBeforeKill)
            {
                DebugLogger.Log(
                    $"[MountXisoDrive.DisposeAsync] Waiting for mounting tool (ID: {_mountProcessId}) to exit (up to 10s).");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                try
                {
                    await _mountProcess.WaitForExitAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    DebugLogger.Log(
                        $"[MountXisoDrive.DisposeAsync] Timeout (10s) waiting for mounting tool (ID: {_mountProcessId}) to exit after Kill.");
                }

                if (_mountProcess.HasExited)
                {
                    DebugLogger.Log(
                        $"[MountXisoDrive.DisposeAsync] Mounting tool (ID: {_mountProcessId}) terminated. Exit code: {_mountProcess.ExitCode}.");
                }
                else
                {
                    DebugLogger.Log(
                        $"[MountXisoDrive.DisposeAsync] xbox-iso-vfs.exe (ID: {_mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                }
            }
        }
        catch (Exception termEx)
        {
            DebugLogger.Log(
                $"[MountXisoDrive.DisposeAsync] Exception while terminating mounting tool (ID: {_mountProcessId}): {termEx}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(termEx,
                $"Failed to terminate mounting tool (ID: {_mountProcessId}) for unmounting.");
        }
        finally
        {
            _mountProcess.Dispose();
        }

        if (!string.IsNullOrEmpty(MountedPath))
        {
            var driveRoot = Path.GetPathRoot(MountedPath);
            await Task.Delay(1000); // Give OS a moment to release the drive
            if (Directory.Exists(driveRoot))
            {
                DebugLogger.Log(
                    $"[MountXisoDrive.DisposeAsync] WARNING: {driveRoot} drive still exists after attempting to unmount.");
            }
            else
            {
                DebugLogger.Log($"[MountXisoDrive.DisposeAsync] {driveRoot} drive successfully unmounted.");
            }
        }

        GC.SuppressFinalize(this);
    }
}
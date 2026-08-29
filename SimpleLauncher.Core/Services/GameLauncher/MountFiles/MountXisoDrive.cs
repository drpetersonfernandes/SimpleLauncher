using System.Diagnostics;

namespace SimpleLauncher.Core.Services.GameLauncher.MountFiles;

/// <inheritdoc />
/// <summary>
///     Represents a temporarily mounted XISO drive.
///     Disposing this object will unmount the drive by terminating the mounting process.
/// </summary>
public class MountXisoDrive : IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly Process? _mountProcess;
    private readonly int _mountProcessId;

    /// <summary>
    ///     Constructor for a successful mount.
    /// </summary>
    public MountXisoDrive(Process mountProcess, string mountedPath, ILogger logErrors, ILogger logger)
    {
        _mountProcess = mountProcess;
        _mountProcessId = mountProcess?.Id ?? -1;
        _logger = logErrors;
        _logger = logger;
        MountedPath = mountedPath;
        IsMounted = !string.IsNullOrEmpty(mountedPath) && _mountProcess != null;
    }

    /// <summary>
    ///     Constructor for a failed mount.
    /// </summary>
    public MountXisoDrive(ILogger logErrors, ILogger logger)
    {
        _logger = logErrors;
        _logger = logger;
        IsMounted = false;
    }

    /// <summary>
    ///     Gets the path where the XISO was mounted.
    /// </summary>
    public string MountedPath { get; } = "";

    /// <summary>
    ///     Gets a value indicating whether the XISO was successfully mounted.
    /// </summary>
    public bool IsMounted { get; }

    /// <summary>
    ///     Unmounts the XISO drive by terminating the mounting process and waiting for it to exit.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!IsMounted || _mountProcess == null) return;

        var processExitedBeforeKill = false;
        try
        {
            // Attempt to terminate the process without checking HasExited first.
            // This avoids a race condition where the process exits between the check
            // and the Kill() call. We handle the "already exited" case via catch blocks.
            try
            {
                _mountProcess.Kill(true);
                _logger.Debug(
                    $"[MountXisoDrive.DisposeAsync] Kill signal sent to mounting tool (ID: {_mountProcessId}).");
            }
            catch (InvalidOperationException)
            {
                // Thrown when the process has already exited before Kill() was invoked
                processExitedBeforeKill = true;
                _logger.Debug(
                    $"[MountXisoDrive.DisposeAsync] Mounting tool (ID: {_mountProcessId}) had already exited before Kill could complete (race condition handled).");
            }
            catch (ArgumentException)
            {
                // Thrown when the process is not associated with a valid handle (already exited/disposed)
                processExitedBeforeKill = true;
                _logger.Debug(
                    $"[MountXisoDrive.DisposeAsync] Mounting tool (ID: {_mountProcessId}) had already exited before explicit unmount was needed.");
            }

            // Only wait for exit if we actually sent a kill signal.
            // If the process was already gone, we skip the wait logic.
            if (!processExitedBeforeKill)
            {
                _logger.Debug(
                    $"[MountXisoDrive.DisposeAsync] Waiting for mounting tool (ID: {_mountProcessId}) to exit (up to 20s).");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                try
                {
                    await _mountProcess.WaitForExitAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    _logger.Debug(
                        $"[MountXisoDrive.DisposeAsync] Timeout (10s) waiting for mounting tool (ID: {_mountProcessId}) to exit after Kill.");
                }

                if (_mountProcess.HasExited)
                    _logger.Debug(
                        $"[MountXisoDrive.DisposeAsync] Mounting tool (ID: {_mountProcessId}) terminated. Exit code: {_mountProcess.ExitCode}.");
                else
                    _logger.Debug(
                        $"[MountXisoDrive.DisposeAsync] xbox-iso-vfs.exe (ID: {_mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
            }
        }
        catch (Exception termEx)
        {
            _logger.Debug(
                $"[MountXisoDrive.DisposeAsync] Exception while terminating mounting tool (ID: {_mountProcessId}): {termEx}");
            _logger.Error(termEx,
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
                _logger.Debug(
                    $"[MountXisoDrive.DisposeAsync] WARNING: {driveRoot} drive still exists after attempting to unmount.");
            else
                _logger.Debug($"[MountXisoDrive.DisposeAsync] {driveRoot} drive successfully unmounted.");
        }

        GC.SuppressFinalize(this);
    }
}
using System.Diagnostics;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

using Interfaces;

/// <summary>
/// Represents a temporarily mounted CHD drive.
/// Disposing this object will unmount the drive by terminating the CHDMounter process.
/// </summary>
public class MountChdDrive : IAsyncDisposable
{
    private readonly Process _mountProcess;
    private readonly int _mountProcessId;
    private readonly ILogErrors _logErrors;
    private readonly ILogger _logger;

    public string MountedPath { get; }
    public string MountedDriveLetter { get; }
    public bool IsMounted { get; }

    /// <summary>
    /// Constructor for a successful mount.
    /// </summary>
    public MountChdDrive(Process mountProcess, string mountedPath, string mountedDriveLetter, ILogErrors logErrors, ILogger logger)
    {
        _mountProcess = mountProcess;
        _mountProcessId = mountProcess?.Id ?? -1;
        _logErrors = logErrors;
        _logger = logger;
        MountedPath = mountedPath;
        MountedDriveLetter = mountedDriveLetter;
        IsMounted = !string.IsNullOrEmpty(mountedPath) && _mountProcess != null;
    }

    /// <summary>
    /// Constructor for a failed mount.
    /// </summary>
    public MountChdDrive(ILogErrors logErrors, ILogger logger)
    {
        _logErrors = logErrors;
        _logger = logger;
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
                _logger.Debug($"[MountChdDrive.DisposeAsync] Kill signal sent to CHDMounter (ID: {_mountProcessId}).");
            }
            catch (InvalidOperationException)
            {
                processExitedBeforeKill = true;
                _logger.Debug($"[MountChdDrive.DisposeAsync] CHDMounter (ID: {_mountProcessId}) had already exited before Kill could complete.");
            }
            catch (ArgumentException)
            {
                processExitedBeforeKill = true;
                _logger.Debug($"[MountChdDrive.DisposeAsync] CHDMounter (ID: {_mountProcessId}) had already exited before explicit unmount was needed.");
            }

            if (!processExitedBeforeKill)
            {
                _logger.Debug($"[MountChdDrive.DisposeAsync] Waiting for CHDMounter (ID: {_mountProcessId}) to exit (up to 20s).");

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                try
                {
                    await _mountProcess.WaitForExitAsync(cts.Token);
                }
                catch (TaskCanceledException)
                {
                    _logger.Debug($"[MountChdDrive.DisposeAsync] Timeout (10s) waiting for CHDMounter (ID: {_mountProcessId}) to exit after Kill.");
                }

                if (_mountProcess.HasExited)
                {
                    _logger.Debug($"[MountChdDrive.DisposeAsync] CHDMounter (ID: {_mountProcessId}) terminated. Exit code: {_mountProcess.ExitCode}.");
                }
                else
                {
                    _logger.Debug($"[MountChdDrive.DisposeAsync] CHDMounter (ID: {_mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                }
            }
        }
        catch (Exception termEx)
        {
            _logger.Debug($"[MountChdDrive.DisposeAsync] Exception while terminating CHDMounter (ID: {_mountProcessId}): {termEx}");
            _logErrors.LogAndForget(termEx, $"Failed to terminate CHDMounter (ID: {_mountProcessId}) for unmounting.");
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
                _logger.Debug($"[MountChdDrive.DisposeAsync] WARNING: Drive {driveRoot} still exists after attempting to unmount.");
            }
            else
            {
                _logger.Debug($"[MountChdDrive.DisposeAsync] Drive {driveRoot} successfully unmounted.");
            }
        }

        GC.SuppressFinalize(this);
    }
}

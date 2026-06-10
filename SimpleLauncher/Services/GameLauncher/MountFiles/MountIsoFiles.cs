using System.Diagnostics;
using System.Text;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public class MountIsoFiles : IMountIsoFiles
{
    private readonly IDebugLogger _debugLogger;

    public MountIsoFiles(IDebugLogger debugLogger)
    {
        _debugLogger = debugLogger;
    }

    public async Task MountIsoFileAsync(
        string resolvedIsoFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        ISystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        IWindowContext windowContext,
        string logPath,
        ILauncherService gameLauncher,
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox)
    {
        _debugLogger.Log($"[MountIsoFiles] Starting to mount ISO using PowerShell: {resolvedIsoFilePath}");
        _debugLogger.Log($"[MountIsoFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        string mountPath = null;

        if (resolvedIsoFilePath == null)
        {
            // Notify developer
            var contextMessage = $"Resolved ISO path is null. ISO: {resolvedIsoFilePath}";
            logErrors.LogAndForget(null, contextMessage);

            // Notify user
            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();

            return;
        }

        try
        {
            // 1. Mount ISO and get drive letter
            var mountedDriveLetter = await ExecutePowerShellMountCommandAsync(resolvedIsoFilePath, logErrors, messageBox);

            if (string.IsNullOrEmpty(mountedDriveLetter))
            {
                // Error already logged by ExecutePowerShellMountCommandAsync
                // User already notified by ExecutePowerShellMountCommandAsync or will be here
                await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
                return;
            }

            mountPath = $"{mountedDriveLetter}:\\";
            _debugLogger.Log($"[MountIsoFiles] ISO reportedly mounted to drive: {mountedDriveLetter}. Mount path: {mountPath}");

            // Poll for the drive to become available with a timeout
            if (!await WaitForDirectoryToExistAsync(mountPath, 10000, 200, logErrors))
            {
                var errorMessage = $"Mount path {mountPath} does not exist after mounting ISO {resolvedIsoFilePath}. PowerShell might have failed silently or the drive is not accessible.";
                _debugLogger.Log($"[MountIsoFiles] Error: {errorMessage}");

                // Notify developer
                logErrors.LogAndForget(null, errorMessage);

                // Notify user
                await messageBox.ThereWasAnErrorMountingTheFileMessageBox();

                // The finally block will attempt to dismount.
                return;
            }

            _debugLogger.Log($"[MountIsoFiles] Mount path {mountPath} confirmed to exist.");

            // 2. Find EBOOT.BIN in the mounted ISO
            _debugLogger.Log($"[MountIsoFiles] Searching for EBOOT.BIN in {mountPath}...");

            // Find EBOOT.BIN
            var ebootBinPath = FindEbootBin.FindEbootBinRecursive(mountPath, logErrors, _debugLogger);

            if (string.IsNullOrEmpty(ebootBinPath))
            {
                var errorMessage = $"EBOOT.BIN not found in mounted ISO at {mountPath}. Original ISO: {resolvedIsoFilePath}";
                _debugLogger.Log($"[MountIsoFiles] Error: {errorMessage}");

                // Notify developer
                logErrors.LogAndForget(new FileNotFoundException(errorMessage), errorMessage);

                // Notify user
                await messageBox.ThereWasAnErrorMountingTheFileMessageBox();

                return;
            }

            _debugLogger.Log($"[MountIsoFiles] EBOOT.BIN found at: {ebootBinPath}. Proceeding to launch.");

            // 3. Launch the game/emulator with EBOOT.BIN
            // Pass the original ISO file path for display in notifications
            await gameLauncher.LaunchRegularEmulatorAsync(ebootBinPath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, windowContext, null, resolvedIsoFilePath);

            _debugLogger.Log($"[MountIsoFiles] Emulator for {ebootBinPath} has exited.");
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[MountIsoFiles] Exception during ISO mount/launch process for {resolvedIsoFilePath}: {ex}");
            var contextMessage = $"Error during ISO mount/launch process for {resolvedIsoFilePath}.\nException: {ex.Message}";

            // Notify developer
            logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
        }
        finally
        {
            _debugLogger.Log($"[MountIsoFiles] Entering finally block for ISO: {resolvedIsoFilePath}.");
            if (!string.IsNullOrEmpty(resolvedIsoFilePath))
            {
                _debugLogger.Log($"[MountIsoFiles] Attempting to dismount ISO: {resolvedIsoFilePath}");
                await ExecutePowerShellDismountCommandAsync(resolvedIsoFilePath, logErrors, messageBox);

                if (!string.IsNullOrEmpty(mountPath))
                {
                    await Task.Delay(1000);
                    if (Directory.Exists(mountPath))
                    {
                        _debugLogger.Log($"[MountIsoFiles] WARNING: Mount path {mountPath} still exists after dismount attempt for ISO: {resolvedIsoFilePath}. Manual dismount might be needed.");
                    }
                    else
                    {
                        _debugLogger.Log($"[MountIsoFiles] Mount path {mountPath} successfully unmounted or no longer detected for ISO: {resolvedIsoFilePath}.");
                    }
                }
            }
        }
    }

    public async Task<bool> WaitForDirectoryToExistAsync(string directoryPath, int maxWaitTimeMs, int pollIntervalMs, ILogErrors logErrors)
    {
        _debugLogger.Log($"[MountIsoFiles] Waiting for directory to exist: {directoryPath} (max wait: {maxWaitTimeMs}ms, poll interval: {pollIntervalMs}ms)");

        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < maxWaitTimeMs)
        {
            if (Directory.Exists(directoryPath))
            {
                _debugLogger.Log($"[MountIsoFiles] Directory confirmed to exist after {stopwatch.ElapsedMilliseconds}ms: {directoryPath}");
                return true;
            }

            await Task.Delay(pollIntervalMs);
        }

        _debugLogger.Log($"[MountIsoFiles] Timeout waiting for directory to exist after {stopwatch.ElapsedMilliseconds}ms: {directoryPath}");
        return false;
    }

    public async Task<string> ExecutePowerShellMountCommandAsync(string isoPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        var escapedIsoPath = isoPath.Replace("'", "''"); // Escape single quotes for PowerShell
        var command = $"$isoPath = '{escapedIsoPath}'; " +
                      "$diskImage = Mount-DiskImage -ImagePath $isoPath -PassThru -ErrorAction Stop; " +
                      "$driveLetter = ($diskImage | Get-Volume | Where-Object { $_.DriveLetter -ne $null -and $_.DriveType -eq 'CD-ROM' } | Select-Object -First 1).DriveLetter; " +
                      "if (-not $driveLetter) { throw 'Failed to get drive letter for mounted ISO. Ensure the ISO is valid and contains a recognized file system.' } " +
                      "Write-Output $driveLetter";

        _debugLogger.Log($"[MountIsoFiles] Executing PowerShell Mount Command: {command}");

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -Command \"& {{ {command} }}\"",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = new Process();
        process.StartInfo = psi;
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data != null) outputBuilder.AppendLine(args.Data);
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data != null) errorBuilder.AppendLine(args.Data);
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30-second timeout

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(cts.Token);

            var errors = errorBuilder.ToString().Trim();
            if (process.ExitCode != 0 || !string.IsNullOrEmpty(errors))
            {
                // Check for execution policy restrictions
                if (IsExecutionPolicyRestricted(errors))
                {
                    await messageBox.UnabletomountIsOfileMessageBox();

                    return null;
                }

                // Notify developer
                var errorMessage = $"PowerShell command to mount ISO failed. Exit Code: {process.ExitCode}.\nPath: {isoPath}\nErrors: {errors}\nOutput: {outputBuilder}";
                _debugLogger.Log($"[MountIsoFiles] Error: {errorMessage}");
                logErrors.LogAndForget(null, errorMessage);

                return null;
            }

            var driveLetter = outputBuilder.ToString().Trim();
            if (driveLetter.Length == 1 && char.IsLetter(driveLetter[0]))
            {
                _debugLogger.Log($"[MountIsoFiles] Successfully mounted ISO {isoPath} and retrieved drive letter: {driveLetter}");
                return driveLetter.ToUpperInvariant();
            }

            // Notify developer
            var failureMessage = $"Failed to parse drive letter from PowerShell output for ISO {isoPath}. Output: '{driveLetter}'\nErrors: {errors}";
            _debugLogger.Log($"[MountIsoFiles] Error: {failureMessage}");
            logErrors.LogAndForget(null, failureMessage);

            return null;
        }
        catch (OperationCanceledException) // Catches TaskCanceledException from WaitForExitAsync with timeout
        {
            // Check if the error output contains execution policy restrictions
            var errorOutput = errorBuilder.ToString().Trim();
            if (IsExecutionPolicyRestricted(errorOutput))
            {
                await messageBox.UnabletomountIsOfileMessageBox();
            }

            // Notify developer
            var timeoutMessage = $"PowerShell mount command timed out (30s) for ISO {isoPath}.";
            _debugLogger.Log($"[MountIsoFiles] Timeout: {timeoutMessage}");
            logErrors.LogAndForget(null, timeoutMessage);

            if (process.HasExited) return null;

            try
            {
                process.Kill(true);
            }
            catch
            {
                /* Ignore errors killing timed-out process */
            }

            return null;
        }
        catch (Exception ex)
        {
            // Check if the exception message indicates execution policy restrictions
            if (IsExecutionPolicyRestricted(ex.Message))
            {
                await messageBox.UnabletomountIsOfileMessageBox();
            }

            // Notify developer
            var errorMessage = $"Exception while executing PowerShell mount command for ISO {isoPath}: {ex.Message}\nOutput: {outputBuilder}\nError: {errorBuilder}";
            _debugLogger.Log($"[MountIsoFiles] Exception: {errorMessage}");
            logErrors.LogAndForget(ex, errorMessage);

            return null;
        }
    }

    public async Task ExecutePowerShellDismountCommandAsync(string isoPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        var escapedIsoPath = isoPath.Replace("'", "''");
        var command = $"Dismount-DiskImage -ImagePath '{escapedIsoPath}' -ErrorAction SilentlyContinue";

        _debugLogger.Log($"[MountIsoFiles] Executing PowerShell Dismount Command: {command}");

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -Command \"& {{ {command} }}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();
        process.StartInfo = psi;
        var errorBuilder = new StringBuilder();
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data != null) errorBuilder.AppendLine(args.Data);
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30-second timeout

        try
        {
            process.Start();
            process.BeginErrorReadLine(); // Capture errors
            // Standard output is usually not relevant for dismount with SilentlyContinue
            await process.WaitForExitAsync(cts.Token);

            var errors = errorBuilder.ToString().Trim();
            if (process.ExitCode != 0 || !string.IsNullOrEmpty(errors))
            {
                // Check for execution policy restrictions
                if (IsExecutionPolicyRestricted(errors))
                {
                    await messageBox.UnabletoDismountIsOfileMessageBox();
                }

                var warningMessage = $"PowerShell dismount command for ISO {isoPath} finished with Exit Code: {process.ExitCode} or reported errors (ErrorAction SilentlyContinue was used).\nErrors: {errors}";
                _debugLogger.Log($"[MountIsoFiles] Info: {warningMessage}"); // Log as Info/Warning
            }
            else
            {
                _debugLogger.Log($"[MountIsoFiles] PowerShell dismount command executed successfully for ISO: {isoPath}.");
            }
        }
        catch (OperationCanceledException)
        {
            // Check if the error output contains execution policy restrictions
            var errorOutput = errorBuilder.ToString().Trim();
            if (IsExecutionPolicyRestricted(errorOutput))
            {
                await messageBox.UnabletoDismountIsOfileMessageBox();
            }

            // Notify developer
            var timeoutMessage = $"PowerShell dismount command timed out (30s) for ISO {isoPath}.";
            _debugLogger.Log($"[MountIsoFiles] Timeout: {timeoutMessage}");
            logErrors.LogAndForget(null, timeoutMessage); // Log timeout as an error

            if (!process.HasExited)
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
        catch (Exception ex)
        {
            // Check if the exception message indicates execution policy restrictions
            if (IsExecutionPolicyRestricted(ex.Message))
            {
                await messageBox.UnabletoDismountIsOfileMessageBox();
            }

            // Notify developer
            var errorMessage = $"Exception while executing PowerShell dismount command for ISO {isoPath}: {ex.Message}";
            _debugLogger.Log($"[MountIsoFiles] Exception: {errorMessage}");
            logErrors.LogAndForget(ex, errorMessage);
        }
    }

    /// <summary>
    /// Detects if PowerShell error output indicates execution policy restrictions
    /// </summary>
    private static bool IsExecutionPolicyRestricted(string errorOutput)
    {
        if (string.IsNullOrWhiteSpace(errorOutput)) return false;

        var lowerError = errorOutput.ToLowerInvariant();
        return lowerError.Contains("execution of scripts is disabled") ||
               (lowerError.Contains("execution policy") &&
                (lowerError.Contains("prevents execution") ||
                 lowerError.Contains("restricted") ||
                 lowerError.Contains("cannot be loaded"))) ||
               (lowerError.Contains("is not digitally signed") && lowerError.Contains("execution policy"));
    }
}

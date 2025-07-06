using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleLauncher.Managers; // For SystemManager, DebugLogger, LogErrors, MessageBoxLibrary, MainWindow

namespace SimpleLauncher.Services;

public static class MountIsoFiles
{
    public static async Task MountIsoFile(
        string resolvedIsoFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath)
    {
        DebugLogger.Log($"[MountIsoFiles] Starting to mount ISO using PowerShell: {resolvedIsoFilePath}");
        DebugLogger.Log($"[MountIsoFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        string mountPath = null;

        try
        {
            // 1. Mount ISO and get drive letter
            var mountedDriveLetter = await ExecutePowerShellMountCommandAsync(resolvedIsoFilePath, logPath);

            if (string.IsNullOrEmpty(mountedDriveLetter))
            {
                // Error already logged by ExecutePowerShellMountCommandAsync
                // User already notified by ExecutePowerShellMountCommandAsync or will be here
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
                return;
            }

            mountPath = $"{mountedDriveLetter}:\\";
            DebugLogger.Log($"[MountIsoFiles] ISO reportedly mounted to drive: {mountedDriveLetter}. Mount path: {mountPath}");

            // Brief delay for the system to recognize the drive fully
            await Task.Delay(2000); // 2 seconds, adjust if necessary

            if (!Directory.Exists(mountPath))
            {
                var errorMessage = $"Mount path {mountPath} does not exist after mounting ISO {resolvedIsoFilePath}. PowerShell might have failed silently or the drive is not accessible.";
                DebugLogger.Log($"[MountIsoFiles] Error: {errorMessage}");

                // Notify developer
                _ = LogErrors.LogErrorAsync(null, errorMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

                // The finally block will attempt to dismount.
                return;
            }

            DebugLogger.Log($"[MountIsoFiles] Mount path {mountPath} confirmed to exist.");

            // 2. Find EBOOT.BIN in the mounted ISO
            DebugLogger.Log($"[MountIsoFiles] Searching for EBOOT.BIN in {mountPath}...");

            // Find EBOOT.BIN
            var ebootBinPath = FindEbootBin.FindEbootBinRecursive(mountPath);

            if (string.IsNullOrEmpty(ebootBinPath))
            {
                var errorMessage = $"EBOOT.BIN not found in mounted ISO at {mountPath}. Original ISO: {resolvedIsoFilePath}";
                DebugLogger.Log($"[MountIsoFiles] Error: {errorMessage}");

                // Notify developer
                _ = LogErrors.LogErrorAsync(new FileNotFoundException(errorMessage), errorMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath); // Or a more specific message like "Required game files not found in ISO"

                return;
            }

            DebugLogger.Log($"[MountIsoFiles] EBOOT.BIN found at: {ebootBinPath}. Proceeding to launch.");

            // 3. Launch the game/emulator with EBOOT.BIN
            await GameLauncher.LaunchRegularEmulator(ebootBinPath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow);
            DebugLogger.Log($"[MountIsoFiles] Emulator for {ebootBinPath} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountIsoFiles] Exception during ISO mount/launch process for {resolvedIsoFilePath}: {ex}");
            var contextMessage = $"Error during ISO mount/launch process for {resolvedIsoFilePath}.\nException: {ex.Message}";

            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
        }
        finally
        {
            DebugLogger.Log($"[MountIsoFiles] Entering finally block for ISO: {resolvedIsoFilePath}.");
            if (!string.IsNullOrEmpty(resolvedIsoFilePath))
            {
                DebugLogger.Log($"[MountIsoFiles] Attempting to dismount ISO: {resolvedIsoFilePath}");
                await ExecutePowerShellDismountCommandAsync(resolvedIsoFilePath, logPath);

                if (!string.IsNullOrEmpty(mountPath))
                {
                    await Task.Delay(1000);
                    if (Directory.Exists(mountPath))
                    {
                        DebugLogger.Log($"[MountIsoFiles] WARNING: Mount path {mountPath} still exists after dismount attempt for ISO: {resolvedIsoFilePath}. Manual dismount might be needed.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountIsoFiles] Mount path {mountPath} successfully unmounted or no longer detected for ISO: {resolvedIsoFilePath}.");
                    }
                }
            }
        }
    }

    private static async Task<string> ExecutePowerShellMountCommandAsync(string isoPath, string logPath)
    {
        var escapedIsoPath = isoPath.Replace("'", "''"); // Escape single quotes for PowerShell
        var command = $"$isoPath = '{escapedIsoPath}'; " +
                      "$diskImage = Mount-DiskImage -ImagePath $isoPath -PassThru -ErrorAction Stop; " +
                      "$driveLetter = ($diskImage | Get-Volume | Where-Object { $_.DriveLetter -ne $null -and $_.DriveType -eq 'CD-ROM' } | Select-Object -First 1).DriveLetter; " +
                      "if (-not $driveLetter) { throw 'Failed to get drive letter for mounted ISO. Ensure the ISO is valid and contains a recognized file system.' } " +
                      "Write-Output $driveLetter";

        DebugLogger.Log($"[MountIsoFiles] Executing PowerShell Mount Command: {command}");

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"& {{ {command} }}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
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
                // Notify developer
                var errorMessage = $"PowerShell command to mount ISO failed. Exit Code: {process.ExitCode}.\nPath: {isoPath}\nErrors: {errors}\nOutput: {outputBuilder}";
                DebugLogger.Log($"[MountIsoFiles] Error: {errorMessage}");
                _ = LogErrors.LogErrorAsync(null, errorMessage);

                return null;
            }

            var driveLetter = outputBuilder.ToString().Trim();
            if (driveLetter.Length == 1 && char.IsLetter(driveLetter[0]))
            {
                DebugLogger.Log($"[MountIsoFiles] Successfully mounted ISO {isoPath} and retrieved drive letter: {driveLetter}");
                return driveLetter.ToUpperInvariant();
            }

            // Notify developer
            var failureMessage = $"Failed to parse drive letter from PowerShell output for ISO {isoPath}. Output: '{driveLetter}'\nErrors: {errors}";
            DebugLogger.Log($"[MountIsoFiles] Error: {failureMessage}");
            _ = LogErrors.LogErrorAsync(null, failureMessage);

            return null;
        }
        catch (OperationCanceledException) // Catches TaskCanceledException from WaitForExitAsync with timeout
        {
            // Notify developer
            var timeoutMessage = $"PowerShell mount command timed out (30s) for ISO {isoPath}.";
            DebugLogger.Log($"[MountIsoFiles] Timeout: {timeoutMessage}");
            _ = LogErrors.LogErrorAsync(null, timeoutMessage);

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
            // Notify developer
            var errorMessage = $"Exception while executing PowerShell mount command for ISO {isoPath}: {ex.Message}\nOutput: {outputBuilder}\nError: {errorBuilder}";
            DebugLogger.Log($"[MountIsoFiles] Exception: {errorMessage}");
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            return null;
        }
    }

    private static async Task ExecutePowerShellDismountCommandAsync(string isoPath, string logPath)
    {
        var escapedIsoPath = isoPath.Replace("'", "''");
        var command = $"Dismount-DiskImage -ImagePath '{escapedIsoPath}' -ErrorAction SilentlyContinue";

        DebugLogger.Log($"[MountIsoFiles] Executing PowerShell Dismount Command: {command}");

        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"& {{ {command} }}\"",
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
                var warningMessage = $"PowerShell dismount command for ISO {isoPath} finished with Exit Code: {process.ExitCode} or reported errors (ErrorAction SilentlyContinue was used).\nErrors: {errors}";
                DebugLogger.Log($"[MountIsoFiles] Info: {warningMessage}"); // Log as Info/Warning
            }
            else
            {
                DebugLogger.Log($"[MountIsoFiles] PowerShell dismount command executed successfully for ISO: {isoPath}.");
            }
        }
        catch (OperationCanceledException)
        {
            // Notify developer
            var timeoutMessage = $"PowerShell dismount command timed out (30s) for ISO {isoPath}.";
            DebugLogger.Log($"[MountIsoFiles] Timeout: {timeoutMessage}");
            _ = LogErrors.LogErrorAsync(null, timeoutMessage); // Log timeout as an error

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
            // Notify developer
            var errorMessage = $"Exception while executing PowerShell dismount command for ISO {isoPath}: {ex.Message}";
            DebugLogger.Log($"[MountIsoFiles] Exception: {errorMessage}");
            _ = LogErrors.LogErrorAsync(ex, errorMessage);
        }
    }
}
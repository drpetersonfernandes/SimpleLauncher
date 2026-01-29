using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services;

public static class MountIsoFiles
{
    public static async Task MountIsoFileAsync(
        string resolvedIsoFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        GamePadController gamePadController,
        string logPath,
        GameLauncher gameLauncher)
    {
        DebugLogger.Log($"[MountIsoFiles] Starting to mount ISO using PowerShell: {resolvedIsoFilePath}");
        DebugLogger.Log($"[MountIsoFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        string mountPath = null;

        if (resolvedIsoFilePath == null)
        {
            // Notify developer
            var contextMessage = $"Resolved ISO path is null. ISO: {resolvedIsoFilePath}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

            return;
        }

        try
        {
            // 1. Mount ISO and get drive letter
            var mountedDriveLetter = await ExecutePowerShellMountCommandAsync(resolvedIsoFilePath);

            if (string.IsNullOrEmpty(mountedDriveLetter))
            {
                // Error already logged by ExecutePowerShellMountCommandAsync
                // User already notified by ExecutePowerShellMountCommandAsync or will be here
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
                return;
            }

            mountPath = $"{mountedDriveLetter}:\\";
            DebugLogger.Log($"[MountIsoFiles] ISO reportedly mounted to drive: {mountedDriveLetter}. Mount path: {mountPath}");

            // Poll for the drive to become available with a timeout
            if (!await WaitForDirectoryToExistAsync(mountPath, 10000, 200))
            {
                var errorMessage = $"Mount path {mountPath} does not exist after mounting ISO {resolvedIsoFilePath}. PowerShell might have failed silently or the drive is not accessible.";
                DebugLogger.Log($"[MountIsoFiles] Error: {errorMessage}");

                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);

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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new FileNotFoundException(errorMessage), errorMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath); // Or a more specific message like "Required game files not found in ISO"

                return;
            }

            DebugLogger.Log($"[MountIsoFiles] EBOOT.BIN found at: {ebootBinPath}. Proceeding to launch.");

            // 3. Launch the game/emulator with EBOOT.BIN
            await gameLauncher.LaunchRegularEmulatorAsync(ebootBinPath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow, gameLauncher);
            DebugLogger.Log($"[MountIsoFiles] Emulator for {ebootBinPath} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountIsoFiles] Exception during ISO mount/launch process for {resolvedIsoFilePath}: {ex}");
            var contextMessage = $"Error during ISO mount/launch process for {resolvedIsoFilePath}.\nException: {ex.Message}";

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
        }
        finally
        {
            DebugLogger.Log($"[MountIsoFiles] Entering finally block for ISO: {resolvedIsoFilePath}.");
            if (!string.IsNullOrEmpty(resolvedIsoFilePath))
            {
                DebugLogger.Log($"[MountIsoFiles] Attempting to dismount ISO: {resolvedIsoFilePath}");
                await ExecutePowerShellDismountCommandAsync(resolvedIsoFilePath);

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

    private static async Task<bool> WaitForDirectoryToExistAsync(string directoryPath, int maxWaitTimeMs, int pollIntervalMs)
    {
        DebugLogger.Log($"[MountIsoFiles] Waiting for directory to exist: {directoryPath} (max wait: {maxWaitTimeMs}ms, poll interval: {pollIntervalMs}ms)");

        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.ElapsedMilliseconds < maxWaitTimeMs)
        {
            if (Directory.Exists(directoryPath))
            {
                DebugLogger.Log($"[MountIsoFiles] Directory confirmed to exist after {stopwatch.ElapsedMilliseconds}ms: {directoryPath}");
                return true;
            }

            await Task.Delay(pollIntervalMs);
        }

        DebugLogger.Log($"[MountIsoFiles] Timeout waiting for directory to exist after {stopwatch.ElapsedMilliseconds}ms: {directoryPath}");
        return false;
    }

    private static async Task<string> ExecutePowerShellMountCommandAsync(string isoPath)
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
                // Check for execution policy restrictions
                if (IsExecutionPolicyRestricted(errors))
                {
                    MessageBoxLibrary.UnabletomountIsOfile();

                    return null;
                }

                // Notify developer
                var errorMessage = $"PowerShell command to mount ISO failed. Exit Code: {process.ExitCode}.\nPath: {isoPath}\nErrors: {errors}\nOutput: {outputBuilder}";
                DebugLogger.Log($"[MountIsoFiles] Error: {errorMessage}");
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, failureMessage);

            return null;
        }
        catch (OperationCanceledException) // Catches TaskCanceledException from WaitForExitAsync with timeout
        {
            // Check if the error output contains execution policy restrictions
            var errorOutput = errorBuilder.ToString().Trim();
            if (IsExecutionPolicyRestricted(errorOutput))
            {
                MessageBoxLibrary.UnabletomountIsOfile();
            }

            // Notify developer
            var timeoutMessage = $"PowerShell mount command timed out (30s) for ISO {isoPath}.";
            DebugLogger.Log($"[MountIsoFiles] Timeout: {timeoutMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, timeoutMessage);

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
                MessageBoxLibrary.UnabletomountIsOfile();
            }

            // Notify developer
            var errorMessage = $"Exception while executing PowerShell mount command for ISO {isoPath}: {ex.Message}\nOutput: {outputBuilder}\nError: {errorBuilder}";
            DebugLogger.Log($"[MountIsoFiles] Exception: {errorMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, errorMessage);

            return null;
        }
    }

    private static async Task ExecutePowerShellDismountCommandAsync(string isoPath)
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
                // Check for execution policy restrictions
                if (IsExecutionPolicyRestricted(errors))
                {
                    MessageBoxLibrary.UnabletoDismountIsOfile();
                }

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
            // Check if the error output contains execution policy restrictions
            var errorOutput = errorBuilder.ToString().Trim();
            if (IsExecutionPolicyRestricted(errorOutput))
            {
                MessageBoxLibrary.UnabletoDismountIsOfile();
            }

            // Notify developer
            var timeoutMessage = $"PowerShell dismount command timed out (30s) for ISO {isoPath}.";
            DebugLogger.Log($"[MountIsoFiles] Timeout: {timeoutMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, timeoutMessage); // Log timeout as an error

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
                MessageBoxLibrary.UnabletoDismountIsOfile();
            }

            // Notify developer
            var errorMessage = $"Exception while executing PowerShell dismount command for ISO {isoPath}: {ex.Message}";
            DebugLogger.Log($"[MountIsoFiles] Exception: {errorMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, errorMessage);
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
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class MountXisoFiles
{
    public static async Task MountXisoFile(string resolvedIsoFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath)
    {
        DebugLogger.Log($"[MountXisoFile] Starting to mount ISO: {resolvedIsoFilePath}");
        DebugLogger.Log($"[MountXisoFile] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        const string xboxIsoVfsExe = @"tools\xbox-iso-vfs\xbox-iso-vfs.exe";
        var resolvedXboxIsoVfsPath = PathHelper.ResolveRelativeToAppDirectory(xboxIsoVfsExe);

        DebugLogger.Log($"[MountXisoFile] Path to {xboxIsoVfsExe}: {resolvedXboxIsoVfsPath}");

        if (string.IsNullOrWhiteSpace(resolvedXboxIsoVfsPath) || !File.Exists(resolvedXboxIsoVfsPath))
        {
            // Notify developer
            const string errorMessage =
                "xbox-iso-vfs.exe not found in tools\\xbox-iso-vfs directory. Cannot mount ISO.";
            DebugLogger.Log($"[MountXisoFile] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

            return;
        }

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedXboxIsoVfsPath,
            Arguments = $"/l \"{resolvedIsoFilePath}\" w",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal,
            WorkingDirectory = Path.GetDirectoryName(resolvedXboxIsoVfsPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log($"[MountXisoFile] ProcessStartInfo for {xboxIsoVfsExe}:");
        DebugLogger.Log($"[MountXisoFile] FileName: {psiMount.FileName}");
        DebugLogger.Log($"[MountXisoFile] Arguments: {psiMount.Arguments}");
        DebugLogger.Log($"[MountXisoFile] WorkingDirectory: {psiMount.WorkingDirectory}");

        using var mountProcess = new Process();
        mountProcess.StartInfo = psiMount;
        mountProcess.EnableRaisingEvents = true;
        var mountProcessId = -1; // To store process ID for reliable logging

        try
        {
            DebugLogger.Log($"[MountXisoFile] Starting {xboxIsoVfsExe} process...");
            var processStarted = mountProcess.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException($"Failed to start the {xboxIsoVfsExe} process.");
            }

            mountProcessId = mountProcess.Id; // Store ID after process started
            DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} process started (ID: {mountProcessId}).");

            // Replace fixed delay with a robust polling mechanism
            const string defaultXbePath = "W:\\default.xbe";
            var mountSuccessful = await WaitForDriveMount(defaultXbePath, mountProcess, xboxIsoVfsExe, mountProcessId);

            if (!mountSuccessful)
            {
                var exitCodeInfo = mountProcess.HasExited
                    ? $"Exit Code: {mountProcess.ExitCode}"
                    : "Process still running";
                DebugLogger.Log(
                    $"[MountXisoFile] Mount check failed. {exitCodeInfo}. Check the console window of {xboxIsoVfsExe} for details.");

                if (!mountProcess.HasExited)
                {
                    DebugLogger.Log($"[MountXisoFile] Terminating unsuccessful {xboxIsoVfsExe} process (ID: {mountProcessId}).");
                    try
                    {
                        mountProcess.Kill(true);
                        // Wait for exit with CancellationToken for timeout (5 seconds)
                        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                        {
                            await mountProcess.WaitForExitAsync(cts.Token);
                        }

                        DebugLogger.Log(
                            $"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) exited after Kill (in mount failure path).");
                    }
                    catch (TaskCanceledException) // Timeout occurred
                    {
                        DebugLogger.Log(
                            $"[MountXisoFile] Timeout (5s) waiting for {xboxIsoVfsExe} (ID: {mountProcessId}) to exit after Kill (in mount failure path).");
                    }
                    catch (InvalidOperationException killEx) when (killEx.Message.Contains("process has already exited",
                                                                       StringComparison.OrdinalIgnoreCase))
                    {
                        DebugLogger.Log(
                            $"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) already exited before WaitForExitAsync after kill (in mount failure path).");
                    }
                    catch (Exception killEx)
                    {
                        DebugLogger.Log(
                            $"[MountXisoFile] Exception while killing/waiting for {xboxIsoVfsExe} (ID: {mountProcessId}) after failed mount: {killEx.Message}");
                    }
                }

                // Notify developer
                var contextMessage = $"Failed to mount the ISO file {resolvedIsoFilePath} or {defaultXbePath} not found after attempting to mount. " +
                                     $"Output should be visible in the console window. {exitCodeInfo}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

                return;
            }

            DebugLogger.Log($"[MountXisoFile] ISO mounted successfully. Proceeding to launch {defaultXbePath} with {selectedEmulatorName}.");

            // Launch default.xbe
            await GameLauncher.LaunchRegularEmulator(defaultXbePath, selectedEmulatorName, selectedSystemManager,
                selectedEmulatorManager, rawEmulatorParameters, mainWindow);

            DebugLogger.Log($"[MountXisoFile] Emulator for {defaultXbePath} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountXisoFile] Exception during mounting or launching: {ex}");

            // Notify developer
            var exitCodeInfo = mountProcess.HasExited ? $"Exit Code: {mountProcess.ExitCode}" : "Process state unknown";
            var contextMessage = $"Error during ISO mount/launch process for {resolvedIsoFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"Console output should be visible in the tool's window. {exitCodeInfo}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
        }
        finally
        {
            if (!mountProcess.HasExited)
            {
                DebugLogger.Log(
                    $"[MountXisoFile] Attempting to unmount by terminating {xboxIsoVfsExe} (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    DebugLogger.Log($"[MountXisoFile] Kill signal sent to {xboxIsoVfsExe} (ID: {mountProcessId}). Waiting for process to exit (up to 10s).");

                    // Wait for exit with CancellationToken for timeout (10 seconds)
                    using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                    {
                        try
                        {
                            await mountProcess.WaitForExitAsync(cts.Token);
                        }
                        catch (TaskCanceledException) // Timeout occurred
                        {
                            DebugLogger.Log($"[MountXisoFile] Timeout (10s) waiting for {xboxIsoVfsExe} (ID: {mountProcessId}) to exit after Kill.");
                        }
                    }

                    if (mountProcess.HasExited)
                    {
                        DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode}.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (InvalidOperationException ioEx)
                {
                    if (ioEx.Message.Contains("process has already exited", StringComparison.OrdinalIgnoreCase) ||
                        ioEx.Message.Contains("No process is associated", StringComparison.OrdinalIgnoreCase) ||
                        ioEx.Message.Contains("Process has not been started", StringComparison.OrdinalIgnoreCase))
                    {
                        DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) had already exited or was not running when explicit kill/wait in finally was attempted: {ioEx.Message}. Output was not redirected.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountXisoFile] Unexpected InvalidOperationException while terminating {xboxIsoVfsExe} (ID: {mountProcessId}): {ioEx}");

                        // Notify developer
                        _ = LogErrors.LogErrorAsync(ioEx, $"Unexpected InvalidOperationException during {xboxIsoVfsExe} termination.");
                    }
                }
                catch (Exception termEx)
                {
                    DebugLogger.Log($"[MountXisoFile] Exception while terminating {xboxIsoVfsExe} (ID: {mountProcessId}): {termEx}");

                    // Notify developer
                    _ = LogErrors.LogErrorAsync(termEx, $"Failed to terminate {xboxIsoVfsExe} (ID: {mountProcessId}) for unmounting.");
                }
            }
            else
            {
                DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) had already exited before finally block's kill check. Exit code likely {(mountProcess.HasExited ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}. Output was not redirected.");
            }

            await Task.Delay(1000);
            if (Directory.Exists("W:\\"))
            {
                DebugLogger.Log("[MountXisoFile] WARNING: W: drive still exists after attempting to unmount. Manual unmount might be needed or xbox-iso-vfs.exe did not unmount on Kill().");
            }
            else
            {
                DebugLogger.Log("[MountXisoFile] W: drive successfully unmounted (or was not detected after unmount attempt).");
            }
        }
    }

    /// <summary>
    /// Waits for polling to mount the drive for the existence of default.xbe.
    /// Also monitors the mounting process to detect early exits.
    /// </summary>
    /// <param name="defaultXbePath">Path to check for mount success</param>
    /// <param name="mountProcess">The mounting process to monitor</param>
    /// <param name="toolName">Name of the mounting tool for logging</param>
    /// <param name="processId">Process ID for logging</param>
    /// <returns>True if the mount was successful, false otherwise</returns>
    private static async Task<bool> WaitForDriveMount(string defaultXbePath, Process mountProcess, string toolName,
        int processId)
    {
        const int maxRetries = 240; // 2 minutes with 500 ms intervals
        const int pollIntervalMs = 500;
        var retryCount = 0;

        DebugLogger.Log(
            $"[MountXisoFile] Polling for '{defaultXbePath}' to appear (max {maxRetries * pollIntervalMs / 1000}s)...");

        while (retryCount < maxRetries)
        {
            // Check if the target file exists (mount successful)
            if (File.Exists(defaultXbePath))
            {
                DebugLogger.Log($"[MountXisoFile] Found '{defaultXbePath}' after {retryCount * pollIntervalMs / 1000.0:F1} seconds. Mount successful!");
                return true;
            }

            // Check if the drive exists even if default.xbe doesn't
            if (Directory.Exists("W:\\"))
            {
                DebugLogger.Log($"[MountXisoFile] W: drive exists after {retryCount * pollIntervalMs / 1000.0:F1} seconds, but '{defaultXbePath}' not found. Continuing to poll...");
            }

            // Check if the mount process exited prematurely
            if (mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountXisoFile] Mount process {toolName} (ID: {processId}) exited prematurely during polling. Exit Code: {mountProcess.ExitCode}.");
                return false;
            }

            retryCount++;
            await Task.Delay(pollIntervalMs);
        }

        DebugLogger.Log($"[MountXisoFile] Timed out waiting for '{defaultXbePath}' after {maxRetries * pollIntervalMs / 1000} seconds.");
        return false;
    }
}
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class MountXisoFiles2
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

        const string xboxIsoVfsExe = @"tools\SimpleXisoDrive\SimpleXisoDrive.exe";
        var resolvedXboxIsoVfsPath = PathHelper.ResolveRelativeToAppDirectory(xboxIsoVfsExe);

        DebugLogger.Log($"[MountXisoFile] Path to {xboxIsoVfsExe}: {resolvedXboxIsoVfsPath}");

        if (string.IsNullOrWhiteSpace(resolvedXboxIsoVfsPath) || !File.Exists(resolvedXboxIsoVfsPath))
        {
            // Notify developer
            const string errorMessage = "SimpleXisoDrive.exe not found in tools directory. Cannot mount ISO.";
            DebugLogger.Log($"[MountXisoFile] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

            return;
        }

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedXboxIsoVfsPath,
            Arguments = $"\"{resolvedIsoFilePath}\" w",
            UseShellExecute = false,
            // Do not redirect output; let it show in the tool's own console window.
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false, // Ensure the console window is created and visible.
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

            // Output is no longer redirected, so BeginOutputReadLine/BeginErrorReadLine are not needed.

            // Replace the fixed delay with a polling mechanism to wait for the mount to complete.
            const string defaultXbePath = "W:\\default.xbe";
            var mountSuccessful = false;
            var timeout = TimeSpan.FromMinutes(2); // Generous 2-minute timeout for large ISOs
            var pollInterval = TimeSpan.FromMilliseconds(500);
            var stopwatch = Stopwatch.StartNew();

            DebugLogger.Log($"[MountXisoFile] Polling for '{defaultXbePath}' to appear (timeout: {timeout.TotalSeconds}s)...");

            while (stopwatch.Elapsed < timeout)
            {
                if (File.Exists(defaultXbePath))
                {
                    mountSuccessful = true;
                    DebugLogger.Log($"[MountXisoFile] Found '{defaultXbePath}' after {stopwatch.Elapsed.TotalSeconds:F1} seconds.");
                    break;
                }

                // Also check if the mount process exited prematurely
                if (mountProcess.HasExited)
                {
                    DebugLogger.Log($"[MountXisoFile] Mount process {xboxIsoVfsExe} (ID: {mountProcessId}) exited prematurely during polling. Exit Code: {mountProcess.ExitCode}.");
                    break; // Exit loop, mountSuccessful will remain false
                }

                await Task.Delay(pollInterval);
            }

            stopwatch.Stop();

            DebugLogger.Log($"[MountXisoFile] Checking for mounted file: {defaultXbePath}. Exists: {mountSuccessful}");

            if (!mountSuccessful)
            {
                if (stopwatch.Elapsed >= timeout)
                {
                    DebugLogger.Log($"[MountXisoFile] Timed out waiting for '{defaultXbePath}'. The mounting tool may be stuck or still processing a large file.");
                }

                var exitCodeInfoOnFailure = mountProcess.HasExited ? $"The process exited with code {mountProcess.ExitCode}." : "The process was still running.";
                DebugLogger.Log($"[MountXisoFile] Mount check failed. {exitCodeInfoOnFailure} Check the console window of {xboxIsoVfsExe} for details.");

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

                        DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) exited after Kill (in mount failure path).");
                    }
                    catch (TaskCanceledException) // Timeout occurred
                    {
                        DebugLogger.Log($"[MountXisoFile] Timeout (5s) waiting for {xboxIsoVfsExe} (ID: {mountProcessId}) to exit after Kill (in mount failure path).");
                    }
                    catch (InvalidOperationException killEx) when (killEx.Message.Contains("process has already exited", StringComparison.OrdinalIgnoreCase))
                    {
                        DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) already exited before WaitForExitAsync after kill (in mount failure path).");
                    }
                    catch (Exception killEx)
                    {
                        DebugLogger.Log($"[MountXisoFile] Exception while killing/waiting for {xboxIsoVfsExe} (ID: {mountProcessId}) after failed mount: {killEx.Message}");
                    }
                }

                // Notify developer
                var contextMessage = $"Failed to mount the ISO file {resolvedIsoFilePath} or {defaultXbePath} not found after attempting to mount. " +
                                     $"Output was not redirected and should have been visible in the tool's console window. {exitCodeInfoOnFailure}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

                return;
            }

            DebugLogger.Log($"[MountXisoFile] ISO mounted successfully. Proceeding to launch {defaultXbePath} with {selectedEmulatorName}.");

            // Launch default.xbe
            await GameLauncher.LaunchRegularEmulator(defaultXbePath, selectedSystemName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow);

            DebugLogger.Log($"[MountXisoFile] Emulator for {defaultXbePath} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountXisoFile] Exception during mounting or launching: {ex}");

            // Notify developer
            var exitCodeInfoInCatch = mountProcess.HasExited ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during ISO mount/launch process for {resolvedIsoFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"The tool's output was not redirected. {exitCodeInfoInCatch}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
        }
        finally
        {
            if (!mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountXisoFile] Attempting to unmount by terminating {xboxIsoVfsExe} (ID: {mountProcessId}).");
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
                DebugLogger.Log("[MountXisoFile] WARNING: W: drive still exists after attempting to unmount. Manual unmount might be needed or SimpleXisoDrive.exe did not unmount on Kill().");
            }
            else
            {
                DebugLogger.Log("[MountXisoFile] W: drive successfully unmounted (or was not detected after unmount attempt).");
            }
        }
    }
}

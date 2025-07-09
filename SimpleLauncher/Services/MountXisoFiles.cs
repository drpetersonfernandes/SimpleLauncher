using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
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

        const string xboxIsoVfsExe = "SimpleXisoDrive.exe";
        var resolvedXboxIsoVfsPath = PathHelper.ResolveRelativeToAppDirectory(xboxIsoVfsExe);

        DebugLogger.Log($"[MountXisoFile] Path to {xboxIsoVfsExe}: {resolvedXboxIsoVfsPath}");

        if (string.IsNullOrWhiteSpace(resolvedXboxIsoVfsPath) || !File.Exists(resolvedXboxIsoVfsPath))
        {
            // Notify developer
            const string errorMessage = "SimpleXisoDrive.exe not found in application directory. Cannot mount ISO.";
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
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
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

        var mountOutput = new StringBuilder();
        var mountError = new StringBuilder();

        mountProcess.OutputDataReceived += (_, args) =>
        {
            if (args.Data == null) return;

            mountOutput.AppendLine(args.Data);
            DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} STDOUT: {args.Data}");
        };
        mountProcess.ErrorDataReceived += (_, args) =>
        {
            if (args.Data == null) return;

            mountError.AppendLine(args.Data);
            DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} STDERR: {args.Data}");
        };

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

            mountProcess.BeginOutputReadLine();
            mountProcess.BeginErrorReadLine();

            DebugLogger.Log("[MountXisoFile] Waiting a few seconds for ISO to mount...");
            await Task.Delay(3000);

            const string defaultXbePath = "W:\\default.xbe";
            var mountSuccessful = File.Exists(defaultXbePath);

            DebugLogger.Log($"[MountXisoFile] Checking for mounted file: {defaultXbePath}. Exists: {mountSuccessful}");

            if (!mountSuccessful)
            {
                DebugLogger.Log($"[MountXisoFile] Mount check failed. {xboxIsoVfsExe} Output:\n{mountOutput}");
                DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} Error:\n{mountError}");

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
                var contextMessage = $"Failed to mount the ISO file {resolvedIsoFilePath} or {defaultXbePath} not found after attempting to mount.\n" +
                                     $"{xboxIsoVfsExe} Output: {mountOutput}\n" +
                                     $"{xboxIsoVfsExe} Error: {mountError}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

                return;
            }

            DebugLogger.Log($"[MountXisoFile] ISO mounted successfully. Proceeding to launch {defaultXbePath} with {selectedEmulatorName}.");

            // Launch default.xbe
            await GameLauncher.LaunchRegularEmulator(defaultXbePath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow);

            DebugLogger.Log($"[MountXisoFile] Emulator for {defaultXbePath} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountXisoFile] Exception during mounting or launching: {ex}");

            // Notify developer
            var contextMessage = $"Error during ISO mount/launch process for {resolvedIsoFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"{xboxIsoVfsExe} Output: {mountOutput}\n" +
                                 $"{xboxIsoVfsExe} Error: {mountError}";
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
                        DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) had already exited or was not running when explicit kill/wait in finally was attempted: {ioEx.Message}. Output:\n{mountOutput}\nError:\n{mountError}");
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
                DebugLogger.Log($"[MountXisoFile] {xboxIsoVfsExe} (ID: {mountProcessId}) had already exited before finally block's kill check. Exit code likely {(mountProcess.HasExited ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}. Output:\n{mountOutput}\nError:\n{mountError}");
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
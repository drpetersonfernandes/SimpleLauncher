using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class GameLauncher
{
    private static readonly string LogPath = GetLogPath.Path();
    private static SystemManager.Emulator _selectedEmulatorManager;
    private static string _selectedEmulatorParameters;

    private const int MemoryAccessViolation = -1073741819;
    private const int DepViolation = -1073740791;

    public static async Task HandleButtonClick(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager selectedSystemManager, SettingsManager settings, MainWindow mainWindow)
    {
        var resolvedFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath);

        if (string.IsNullOrWhiteSpace(resolvedFilePath) || !File.Exists(resolvedFilePath))
        {
            // Notify developer
            const string contextMessage = "Invalid resolvedFilePath or file does not exist.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.FilePathIsInvalid(LogPath);

            return;
        }

        if (string.IsNullOrWhiteSpace(selectedEmulatorName))
        {
            // Notify developer
            const string contextMessage = "selectedEmulatorName is null or empty.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        if (selectedSystemName == null)
        {
            // Notify developer
            const string contextMessage = "selectedSystemName is null.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        if (selectedSystemManager == null)
        {
            // Notify developer
            const string contextMessage = "selectedSystemManager is null";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        _selectedEmulatorManager = selectedSystemManager.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (_selectedEmulatorManager == null)
        {
            // Notify developer
            const string contextMessage = "_selectedEmulatorManager is null.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        if (string.IsNullOrWhiteSpace(_selectedEmulatorManager.EmulatorName))
        {
            // Notify developer
            const string contextMessage = "_selectedEmulatorManager.EmulatorName is null.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        _selectedEmulatorParameters = _selectedEmulatorManager.EmulatorParameters;

        var wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;
        if (wasGamePadControllerRunning)
        {
            GamePadController.Instance2.Stop();
        }

        var startTime = DateTime.Now;

        try
        {
            // Specific handling for Cxbx-Reloaded
            if (selectedEmulatorName.Equals("Cxbx-Reloaded", StringComparison.OrdinalIgnoreCase) &&
                Path.GetExtension(resolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"Cxbx-Reloaded call detected. Attempting to mount and launch: {resolvedFilePath}");
                await MountXisoFile(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow);
            }
            else
            {
                var fileExtension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();
                switch (fileExtension)
                {
                    case ".BAT":
                        await LaunchBatchFile(resolvedFilePath, mainWindow);
                        break;
                    case ".LNK":
                        await LaunchShortcutFile(resolvedFilePath, mainWindow);
                        break;
                    case ".EXE":
                        await LaunchExecutable(resolvedFilePath, mainWindow);
                        break;
                    default:
                        await LaunchRegularEmulator(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Unhandled error in GameLauncher's main launch block.\n" +
                                 $"FilePath: {resolvedFilePath}\n" +
                                 $"SelectedSystem: {selectedSystemName}\n" +
                                 $"SelectedEmulator: {selectedEmulatorName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        }
        finally
        {
            if (wasGamePadControllerRunning)
            {
                GamePadController.Instance2.Start();
            }

            var endTime = DateTime.Now;
            var playTime = endTime - startTime;

            var fileName = Path.GetFileName(resolvedFilePath);

            settings.UpdateSystemPlayTime(selectedSystemName, playTime);
            settings.Save();
            var playTimeFormatted = playTime.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture);
            DebugLogger.Log($"PlayTime saved: {playTimeFormatted}");

            var playTime2 = (string)Application.Current.TryFindResource("Playtime") ?? "Playtime";
            TrayIconManager.ShowTrayMessage($"{playTime2}: {playTimeFormatted}");

            UpdateStatusBar.UpdateContent("", mainWindow);

            try
            {
                var playHistoryManager = PlayHistoryManager.LoadPlayHistory();
                playHistoryManager.AddOrUpdatePlayHistoryItem(fileName, selectedSystemName, playTime);
                mainWindow.RefreshGameListAfterPlay(fileName, selectedSystemName);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error updating play history";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }

            var systemPlayTime = settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystemName);
            if (systemPlayTime != null)
            {
                mainWindow.PlayTime = systemPlayTime.PlayTime;
                DebugLogger.Log($"System PlayTime updated: {systemPlayTime.PlayTime}");
            }

            if (selectedEmulatorName is not null)
            {
                // Update stats
                _ = Stats.CallApiAsync(selectedEmulatorName);
            }
        }
    }

    private static async Task MountXisoFile(string resolvedIsoFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow)
    {
        DebugLogger.Log($"[MountXisoFile] Starting to mount ISO: {resolvedIsoFilePath}");
        DebugLogger.Log($"[MountXisoFile] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        const string xboxIsoVfsExe = "xbox-iso-vfs.exe";
        var resolvedXboxIsoVfsPath = PathHelper.ResolveRelativeToAppDirectory(xboxIsoVfsExe);

        DebugLogger.Log($"[MountXisoFile] Path to {xboxIsoVfsExe}: {resolvedXboxIsoVfsPath}");

        if (string.IsNullOrWhiteSpace(resolvedXboxIsoVfsPath) || !File.Exists(resolvedXboxIsoVfsPath))
        {
            // Notify developer
            const string errorMessage = "xbox-iso-vfs.exe not found in application directory. Cannot mount ISO.";
            DebugLogger.Log($"[MountXisoFile] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheXisoFile(LogPath);

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
                MessageBoxLibrary.ThereWasAnErrorMountingTheXisoFile(LogPath);

                return;
            }

            DebugLogger.Log($"[MountXisoFile] ISO mounted successfully. Proceeding to launch {defaultXbePath} with {selectedEmulatorName}.");

            // Launch default.xbe
            await LaunchRegularEmulator(defaultXbePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow);

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
            MessageBoxLibrary.ThereWasAnErrorMountingTheXisoFile(LogPath);
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
                DebugLogger.Log("[MountXisoFile] WARNING: W: drive still exists after attempting to unmount. Manual unmount might be needed or xbox-iso-vfs.exe did not unmount on Kill().");
            }
            else
            {
                DebugLogger.Log("[MountXisoFile] W: drive successfully unmounted (or was not detected after unmount attempt).");
            }
        }
    }

    private static async Task LaunchBatchFile(string resolvedFilePath, MainWindow mainWindow)
    {
        var psi = new ProcessStartInfo
        {
            FileName = resolvedFilePath,
            UseShellExecute = false, // UseShellExecute=false is required for redirecting output/error
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true // Hide the console window
        };

        // Set the working directory to the directory of the batch file
        try
        {
            var workingDirectory = Path.GetDirectoryName(resolvedFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Could not get workingDirectory for batch file: '{resolvedFilePath}'. Using default.");

            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        DebugLogger.Log("LaunchBatchFile:\n\n");
        DebugLogger.Log($"Batch File: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        TrayIconManager.ShowTrayMessage($"{psi.FileName} launched");
        UpdateStatusBar.UpdateContent($"{psi.FileName} launched", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;

        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                error.AppendLine(args.Data);
            }
        };

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the batch process.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                // Notify developer
                var contextMessage = $"There was an issue running the batch process. User was not notified.\n" +
                                     $"Batch file: {psi.FileName}\n" +
                                     $"Exit code {process.ExitCode}\n" +
                                     $"Output: {output}\n" +
                                     $"Error: {error}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Exception running the batch process. User was not notified.\n" +
                                 $"Batch file: {psi.FileName}\n" +
                                 // Safely get ExitCode
                                 $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"Output: {output}\n" +
                                 $"Error: {error}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchShortcutFile(string resolvedFilePath, MainWindow mainWindow)
    {
        var psi = new ProcessStartInfo
        {
            FileName = resolvedFilePath,
            UseShellExecute = true // UseShellExecute=true is typical for launching .lnk
        };

        // Working directory is often ignored for .lnk files when UseShellExecute is true,
        // but setting it doesn't hurt.
        try
        {
            var workingDirectory = Path.GetDirectoryName(resolvedFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Could not get workingDirectory for shortcut file: '{resolvedFilePath}'. Using default.");

            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        DebugLogger.Log("LaunchShortcutFile:\n\n");
        DebugLogger.Log($"Shortcut File: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        TrayIconManager.ShowTrayMessage($"{psi.FileName} launched");
        UpdateStatusBar.UpdateContent($"{psi.FileName} launched", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the shortcut process.");
            }

            // Note: With UseShellExecute = true, process.WaitForExit() might not wait for the launched application,
            // but rather for the shell process that opened it (like explorer.exe). This is a limitation.
            // I will keep the wait for consistency. It might return immediately.
            await process.WaitForExitAsync();

            // ExitCode might not be reliable with UseShellExecute = true
            if (process.ExitCode != 0)
            {
                 // Log the exit code, but don't necessarily treat it as a critical error
                 // since it might just be the shell's exit code.
                 // Notify developer
                 var contextMessage = $"Shortcut process exited with non-zero code (may be shell's code).\n" +
                                      $"Shortcut file: {psi.FileName}\n" +
                                      $"Exit code {process.ExitCode}";
                 _ = LogErrors.LogErrorAsync(null, contextMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Exception launching the shortcut file.\n" +
                                 $"Shortcut file: {psi.FileName}\n" +
                                 $"Exception: {ex.Message}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchExecutable(string resolvedFilePath, MainWindow mainWindow)
    {
        var psi = new ProcessStartInfo
        {
            FileName = resolvedFilePath,
            UseShellExecute = true // UseShellExecute=true is typical for launching .exe directly
        };

        // Set the working directory to the directory of the executable
        try
        {
            var workingDirectory = Path.GetDirectoryName(resolvedFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Could not get workingDirectory for executable file: '{resolvedFilePath}'. Using default.");

            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        DebugLogger.Log("LaunchExecutable:\n\n");
        DebugLogger.Log($"Executable File: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        TrayIconManager.ShowTrayMessage($"{psi.FileName} launched");
        UpdateStatusBar.UpdateContent($"{psi.FileName} launched", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the executable process.");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                // Notify developer
                var contextMessage = $"Executable process exited with non-zero code.\n" +
                                     $"Executable file: {psi.FileName}\n" +
                                     $"Exit code {process.ExitCode}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Exception launching the executable file.\n" +
                                 $"Executable file: {psi.FileName}\n" +
                                 // Safely get ExitCode
                                 $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}\n" +
                                 $"Exception: {ex.Message}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchRegularEmulator(string resolvedFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters, MainWindow mainWindow) // This is the raw parameter string from config
    {
        // Check if the file to launch is the mounted XBE path, which should not be extracted again.
        var isMountedXbe = resolvedFilePath.Equals("W:\\default.xbe", StringComparison.OrdinalIgnoreCase);

        if (selectedSystemManager.ExtractFileBeforeLaunch == true && !isMountedXbe)
        {
            resolvedFilePath = await ExtractFilesBeforeLaunch(resolvedFilePath, selectedSystemManager);
        }

        if (string.IsNullOrEmpty(resolvedFilePath))
        {
            // Notify developer
            const string contextMessage = "resolvedFilePath is null or empty after extraction attempt (or for mounted XBE).";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        // For mounted XBE, ensure it still exists before proceeding (it should, if MountXisoFile worked)
        if (isMountedXbe && !File.Exists(resolvedFilePath))
        {
            var contextMessage = $"Mounted file {resolvedFilePath} not found when trying to launch with emulator.";
            DebugLogger.Log($"[LaunchRegularEmulator] Error: {contextMessage}");

            // Notify developer
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        // Resolve the Emulator Path (executable)
        var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorManager.EmulatorLocation);
        if (string.IsNullOrEmpty(resolvedEmulatorExePath) || !File.Exists(resolvedEmulatorExePath))
        {
            // Notify developer
            var contextMessage = $"Emulator executable path is null, empty, or does not exist after resolving: '{selectedEmulatorManager.EmulatorLocation}' -> '{resolvedEmulatorExePath}'";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        // Determine the emulator's directory, which is the base for %EMULATORFOLDER%
        var resolvedEmulatorFolderPath = Path.GetDirectoryName(resolvedEmulatorExePath);
        if (string.IsNullOrEmpty(resolvedEmulatorFolderPath) || !Directory.Exists(resolvedEmulatorFolderPath)) // Should exist if exe exists
        {
             // Notify developer
             var contextMessage = $"Could not determine emulator folder path from executable path: '{resolvedEmulatorExePath}'";
             _ = LogErrors.LogErrorAsync(null, contextMessage);

             // Notify user
             MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
             return;
        }

        // Resolve System Folder Path, which is the base for %SYSTEMFOLDER%
        var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.SystemFolder);
        // Note: SystemFolder might not be strictly required to exist for all emulators/parameters,
        // but if %SYSTEMFOLDER% is used in parameters, this path needs to be valid.

        // Resolve Emulator Parameters using the ParameterValidator.ResolveParameterString
        var resolvedParameters = ParameterValidator.ResolveParameterString(
            rawEmulatorParameters, // The raw parameter string from config
            resolvedSystemFolderPath, // The fully resolved system folder path
            resolvedEmulatorFolderPath // The fully resolved emulator directory path
        );

        var arguments = $"{resolvedParameters} \"{resolvedFilePath}\"";

        string workingDirectory;
        try
        {
            // Set the working directory to the directory of the emulator executable
            workingDirectory = resolvedEmulatorFolderPath;
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Could not get workingDirectory for emulator: '{resolvedEmulatorFolderPath}'";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            workingDirectory = AppDomain.CurrentDomain.BaseDirectory; // fallback
        }

        var psi = new ProcessStartInfo
        {
            FileName = resolvedEmulatorExePath, // Use the resolved executable path
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        DebugLogger.Log($"LaunchRegularEmulator:\n\n" +
                        $"Program Location: {resolvedEmulatorExePath}\n" +
                        $"Arguments: {arguments}\n" +
                        $"Working Directory: {psi.WorkingDirectory}\n" +
                        $"File to launch: {resolvedFilePath}");

        var fileName = Path.GetFileNameWithoutExtension(resolvedFilePath);
        var launchedwith = (string)Application.Current.TryFindResource("launchedwith") ?? "launched with";
        TrayIconManager.ShowTrayMessage($"{fileName} {launchedwith} {selectedEmulatorName}");
        UpdateStatusBar.UpdateContent($"{fileName} {launchedwith} {selectedEmulatorName}", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;
        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                error.AppendLine(args.Data);
            }
        };

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.");
            }

            await Task.Delay(100);

            if (!process.HasExited)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
            }

            if (process.HasExited)
            {
                // if (await CheckForMemoryAccessViolation(process, psi, output, error, selectedEmulatorManager)) return;
                if (await CheckForDepViolation(process, psi, output, error, selectedEmulatorManager)) return;

                await CheckForExitCodeWithErrorAny(process, psi, output, error, selectedEmulatorManager);
            }
        }
        catch (InvalidOperationException ex)
        {
            // Notify developer
            const string contextMessage = "Invalid Operation Exception while launching emulator.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.InvalidOperationExceptionMessageBox(LogPath);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorDetail = $"Exit code: {(process.HasExited ? process.ExitCode : -1)}\n" +
                              $"Emulator: {psi.FileName}\n" +
                              $"Emulator output: {output}\n" +
                              $"Emulator error: {error}\n" +
                              $"Calling parameters: {psi.Arguments}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"The emulator could not open the game with the provided parameters. {userNotified}\n\n{errorDetail}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
            }
        }
    }

    private static async Task<string> ExtractFilesBeforeLaunch(string resolvedFilePath, SystemManager systemManager)
    {
        var fileExtension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();
        DebugLogger.Log($"[ExtractFilesBeforeLaunch] Attempting to extract: {resolvedFilePath}, Extension: {fileExtension}");

        switch (fileExtension)
        {
            case ".ZIP":
            {
                var extractCompressedFile = new ExtractCompressedFile();
                var pathToExtractionDirectory = await extractCompressedFile.ExtractWithNativeLibraryToTempAsync(resolvedFilePath);

                var extractedFileToLaunch = await ValidateAndFindGameFile(pathToExtractionDirectory, systemManager);
                if (!string.IsNullOrEmpty(extractedFileToLaunch))
                    return extractedFileToLaunch;

                break;
            }
            case ".7Z" or ".RAR":
            {
                var extractCompressedFile = new ExtractCompressedFile();
                var pathToExtractionDirectory = await extractCompressedFile.ExtractWith7ZToTempAsync(resolvedFilePath);

                var extractedFileToLaunch = await ValidateAndFindGameFile(pathToExtractionDirectory, systemManager);
                if (!string.IsNullOrEmpty(extractedFileToLaunch))
                    return extractedFileToLaunch;

                break;
            }
            default:
            {
                // Notify developer
                var contextMessage = $"Can not extract file: {resolvedFilePath}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);
                DebugLogger.Log($"[ExtractFilesBeforeLaunch] Error: {contextMessage}");

                // Notify user
                MessageBoxLibrary.CannotExtractThisFileMessageBox(resolvedFilePath);

                break;
            }
        }

        DebugLogger.Log($"[ExtractFilesBeforeLaunch] No suitable file found after extraction attempt for: {resolvedFilePath}");
        return null; // Explicitly return null if no file found or extraction failed

        static Task<string> ValidateAndFindGameFile(string tempExtractLocation, SystemManager sysManager)
        {
            DebugLogger.Log($"[ValidateAndFindGameFile] Validating extracted path: {tempExtractLocation}");
            if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
            {
                // Notify developer
                var contextMessage = $"Extracted path is invalid: {tempExtractLocation}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);
                DebugLogger.Log($"[ValidateAndFindGameFile] Error: {contextMessage}");

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return Task.FromResult<string>(null);
            }

            if (sysManager.FileFormatsToLaunch == null || sysManager.FileFormatsToLaunch.Count == 0)
            {
                // Notify developer
                const string contextMessage = "FileFormatsToLaunch is null or empty.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);
                DebugLogger.Log($"[ValidateAndFindGameFile] Error: {contextMessage}");

                // Notify user
                MessageBoxLibrary.NullFileExtensionMessageBox();

                return Task.FromResult<string>(null);
            }

            DebugLogger.Log($"[ValidateAndFindGameFile] Searching for formats: {string.Join(", ", sysManager.FileFormatsToLaunch)} in {tempExtractLocation}");
            foreach (var formatToLaunch in sysManager.FileFormatsToLaunch)
            {
                try
                {
                    // Ensure formatToLaunch is just the extension like ".cue", not "*.cue"
                    var searchPattern = $"*{formatToLaunch}";
                    if (!formatToLaunch.StartsWith('.'))
                    {
                        searchPattern = $"*.{formatToLaunch}"; // Normalize if needed
                    }

                    var files = Directory.GetFiles(tempExtractLocation, searchPattern, SearchOption.AllDirectories);
                    if (files.Length <= 0) continue;

                    DebugLogger.Log($"[ValidateAndFindGameFile] Found file to launch: {files[0]}");
                    return Task.FromResult(files[0]);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, $"Error searching for file format '{formatToLaunch}' in '{tempExtractLocation}'.");
                    DebugLogger.Log($"[ValidateAndFindGameFile] Exception searching for {formatToLaunch}: {ex.Message}");
                }
            }

            // Notify developer
            const string notFoundContext = "Could not find a file with any of the extensions defined in 'FileFormatsToLaunch' after extraction.";
            _ = LogErrors.LogErrorAsync(new FileNotFoundException(notFoundContext), notFoundContext);
            DebugLogger.Log($"[ValidateAndFindGameFile] Error: {notFoundContext}");

            // Notify user
            MessageBoxLibrary.CouldNotFindAFileMessageBox();

            return Task.FromResult<string>(null);
        }
    }

    private static Task CheckForExitCodeWithErrorAny(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorManager)
    {
        // Ignore MemoryAccessViolation
        if (!process.HasExited || process.ExitCode == 0 || process.ExitCode == MemoryAccessViolation)
        {
            return Task.CompletedTask;
        }

        // Check if the output contains "File open/read error" and ignore it,
        // This is a common RetroArch error that should be ignored
        if (output.ToString().Contains("File open/read error", StringComparison.OrdinalIgnoreCase))
        {
            DebugLogger.Log($"[CheckForExitCodeWithErrorAny] Ignored exit code {process.ExitCode} due to 'File open/read error' in output.");
            return Task.CompletedTask;
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify developer
            var contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                                 $"User was notified.\n\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n" +
                                 $"Calling parameters: {psi.Arguments}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
        }
        else
        {
            // Notify developer
            var contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                                 $"User was not notified.\n\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n" +
                                 $"Calling parameters: {psi.Arguments}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        }

        return Task.CompletedTask;
    }

    private static Task<bool> CheckForMemoryAccessViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorManager)
    {
        if (!process.HasExited || process.ExitCode != MemoryAccessViolation) // Ensure process has exited
        {
            return Task.FromResult(false);
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify developer
            var contextMessage = $"There was an memory access violation error running the emulator.\n" +
                                 $"User was notified.\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n" +
                                 $"Calling parameters: {psi.Arguments}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
        }
        else
        {
            // Notify developer
            var contextMessage = $"There was an memory access violation error running the emulator.\n" +
                                 $"User was not notified.\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n" +
                                 $"Calling parameters: {psi.Arguments}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify user
            MessageBoxLibrary.CheckForMemoryAccessViolation(LogPath);
        }

        return Task.FromResult(true);
    }

    private static Task<bool> CheckForDepViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorManager)
    {
        if (!process.HasExited || process.ExitCode != DepViolation) // Ensure process has exited
        {
            return Task.FromResult(false);
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify developer
            var contextMessage = $"Data Execution Prevention (DEP) violation error occurred while running the emulator.\n" +
                                 $"User was notified.\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n" +
                                 $"Calling parameters: {psi.Arguments}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
        }
        else
        {
            // Notify developer
            var contextMessage = $"Data Execution Prevention (DEP) violation error occurred while running the emulator.\n" +
                                 $"User was not notified.\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n" +
                                 $"Calling parameters: {psi.Arguments}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify user
            MessageBoxLibrary.DepViolationMessageBox(LogPath);
        }

        return Task.FromResult(true);
    }
}
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class GameLauncher
{
    private static readonly string LogPath = GetLogPath.Path();
    private static SystemManager.Emulator _selectedEmulatorManager;
    private static string _selectedEmulatorParameters;

    private const int MemoryAccessViolation = -1073741819;
    private const int DepViolation = -1073740791;

    // Load Config to Mount Zip Drive
    public static void Initialize(IConfiguration configuration)
    {
        MountZipFiles.Configure(configuration);
    }

    public static async Task HandleButtonClick(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager selectedSystemManager, SettingsManager settings, MainWindow mainWindow)
    {
        var resolvedFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath);

        if (string.IsNullOrWhiteSpace(resolvedFilePath) || !File.Exists(resolvedFilePath))
        {
            // Notify developer
            var contextMessage = $"Invalid resolvedFilePath or file does not exist.\n\n" +
                                 $"Original filePath: {filePath}\n" +
                                 $"Resolved filePath: {resolvedFilePath}";
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
        mainWindow.IsLoadingGames = true;

        try
        {
            // Specific handling for Cxbx-Reloaded
            if (selectedEmulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase) &&
                Path.GetExtension(resolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"Cxbx-Reloaded call detected. Attempting to mount and launch: {resolvedFilePath}");
                await MountXisoFiles.MountXisoFile(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, LogPath);
            }
            // Specific handling for ScummVM games with ZIP files
            else if ((selectedSystemName.Contains("ScummVM", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("Scumm-VM", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("Scumm", StringComparison.OrdinalIgnoreCase))
                     && Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"ScummVM game with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                await MountZipFiles.MountZipFileAndLoadWithScummVm(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, LogPath);
            }
            // Specific handling for RPCS3 with ZIP files
            else if (selectedEmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) &&
                     Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"RPCS3 with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                await MountZipFiles.MountZipFileAndLoadEbootBin(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, LogPath);
            }
            // Specific handling for RPCS3 with ISO files
            else if (selectedEmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) &&
                     Path.GetExtension(resolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"RPCS3 with ISO call detected. Attempting to mount ISO and launch: {resolvedFilePath}");
                await MountIsoFiles.MountIsoFile(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, LogPath);
            }
            // Specific handling for XBLA games with ZIP files
            else if ((selectedSystemName.Contains("xbla", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("xbox live", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("live arcade", StringComparison.OrdinalIgnoreCase) || resolvedFilePath.Contains("xbla", StringComparison.OrdinalIgnoreCase))
                     && Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                DebugLogger.Log($"XBLA game with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                await MountZipFiles.MountZipFileAndSearchForFileToLoad(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, LogPath);
            }
            else
            {
                var fileExtension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();
                switch (fileExtension)
                {
                    case ".BAT":
                        await LaunchBatchFile(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                        break;
                    case ".LNK":
                        await LaunchShortcutFile(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                        break;
                    case ".EXE":
                        await LaunchExecutable(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                        break;
                    default:
                        await LaunchRegularEmulator(resolvedFilePath, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow);
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
            mainWindow.IsLoadingGames = false;
            if (wasGamePadControllerRunning)
            {
                GamePadController.Instance2.Start();
            }

            var endTime = DateTime.Now;
            var playTime = endTime - startTime;

            // Always use the original file path for history, not the resolved/extracted path.
            // The 'filePath' parameter passed into HandleButtonClick is the original path from the game list.
            var fileNameForHistory = Path.GetFileName(filePath);

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
                playHistoryManager.AddOrUpdatePlayHistoryItem(fileNameForHistory, selectedSystemName, playTime);
                mainWindow.RefreshGameListAfterPlay(fileNameForHistory, selectedSystemName);
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

    private static async Task LaunchBatchFile(string resolvedFilePath, SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
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
                var errorDetail = $"There was an issue running the batch process.\n" +
                                  $"Batch file: {psi.FileName}\n" +
                                  $"Exit code {process.ExitCode}\n" +
                                  $"Output: {output}\n" +
                                  $"Error: {error}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorDetail = $"Exception running the batch process.\n" +
                              $"Batch file: {psi.FileName}\n" +
                              $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}\n" +
                              $"Exception: {ex.Message}\n" +
                              $"Output: {output}\n" +
                              $"Error: {error}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            }
        }
    }

    private static async Task LaunchShortcutFile(string resolvedFilePath, SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
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
                 var errorDetail = $"Shortcut process exited with non-zero code (may be shell's code).\n" +
                                   $"Shortcut file: {psi.FileName}\n" +
                                   $"Exit code {process.ExitCode}";
                 var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                 var contextMessage = $"{errorDetail}\n{userNotified}";
                 _ = LogErrors.LogErrorAsync(null, contextMessage);

                 if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                 {
                     // Notify user
                     MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
                 }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorDetail = $"Exception launching the shortcut file.\n" +
                              $"Shortcut file: {psi.FileName}\n" +
                              $"Exception: {ex.Message}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            }
        }
    }

    private static async Task LaunchExecutable(string resolvedFilePath, SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
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
                var errorDetail = $"Executable process exited with non-zero code.\n" +
                                  $"Executable file: {psi.FileName}\n" +
                                  $"Exit code {process.ExitCode}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorDetail = $"Exception launching the executable file.\n" +
                              $"Executable file: {psi.FileName}\n" +
                              $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}\n" +
                              $"Exception: {ex.Message}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            }
        }
    }

    public static async Task LaunchRegularEmulator(string resolvedFilePath,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow) // This is the raw parameter string from config
    {
        // Check if the file to launch a mounted XBE path, which should not be extracted
        var isMountedXbe = resolvedFilePath.Equals("W:\\default.xbe", StringComparison.OrdinalIgnoreCase);

        // Check if the file to launch a mounted ZIP file, which will not be extracted
        var isMountedZip = resolvedFilePath.StartsWith(MountZipFiles.ConfiguredMountDriveRoot, StringComparison.OrdinalIgnoreCase);

        if (selectedSystemManager.ExtractFileBeforeLaunch == true && !isMountedXbe && !isMountedZip)
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

        // For mounted files, ensure it still exists before proceeding
        if ((isMountedXbe || isMountedZip) && !File.Exists(resolvedFilePath))
        {
            // Notify developer
            var contextMessage = $"Mounted file {resolvedFilePath} not found when trying to launch with emulator.";
            DebugLogger.Log($"[LaunchRegularEmulator] Error: {contextMessage}");
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
        var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.PrimarySystemFolder);
        // Note: SystemFolder might not be strictly required to exist for all emulators/parameters,
        // but if %SYSTEMFOLDER% is used in parameters, this path needs to be valid.

        // Resolve Emulator Parameters using the ParameterValidator.ResolveParameterString
        var resolvedParameters = ParameterValidator.ResolveParameterString(
            rawEmulatorParameters, // The raw parameter string from config
            resolvedSystemFolderPath, // The fully resolved system folder path
            resolvedEmulatorFolderPath // The fully resolved emulator directory path
        );

        string arguments;

        // Handling Mattel Aquarius Games
        if ((selectedSystemManager.SystemName.Contains("aquarius", StringComparison.OrdinalIgnoreCase) || selectedSystemManager.SystemName.Contains("mattel", StringComparison.OrdinalIgnoreCase)))
        {
            // Provide only the filename without extension and without the path
            var resolvedFileName = Path.GetFileNameWithoutExtension(resolvedFilePath);
            DebugLogger.Log($"Mattel Aquarius game call detected. Attempting to launch: {resolvedFileName}");

            arguments = $"{resolvedParameters} \"{resolvedFileName}\"";
        }
        else // General call - Provide full filepath
        {
            arguments = $"{resolvedParameters} \"{resolvedFilePath}\"";
        }

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
                await CheckForMemoryAccessViolation(process, psi, output, error, selectedEmulatorManager);
                await CheckForDepViolation(process, psi, output, error, selectedEmulatorManager);
                await CheckForExitCodeWithErrorAny(process, psi, output, error, selectedEmulatorManager);
            }
        }
        catch (InvalidOperationException ex)
        {
            // Notify developer
            const string contextMessage = "InvalidOperationException while launching emulator.";
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
            case ".ZIP" or ".7Z" or ".RAR":
            {
                var extractCompressedFile = new ExtractCompressedFile();
                var pathToExtractionDirectory = await extractCompressedFile.ExtractWithSevenZipSharpToTempAsync(resolvedFilePath);

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
        // Ignore MemoryAccessViolation and DepViolation
        if (!process.HasExited || process.ExitCode == 0 || process.ExitCode == MemoryAccessViolation || process.ExitCode == DepViolation)
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

    private static Task CheckForMemoryAccessViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorManager)
    {
        if (process.HasExited && process.ExitCode != MemoryAccessViolation)
        {
            return Task.CompletedTask;
        }

        // Notify developer
        var contextMessage = $"There was a memory access violation error running the emulator.\n" +
                             $"User was not notified.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n" +
                             $"Calling parameters: {psi.Arguments}";
        _ = LogErrors.LogErrorAsync(null, contextMessage);

        return Task.CompletedTask;
    }

    private static Task CheckForDepViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorManager)
    {
        if (process.HasExited && process.ExitCode != DepViolation) return Task.CompletedTask;

        // Notify developer
        var contextMessage = $"Data Execution Prevention (DEP) violation error occurred while running the emulator.\n" +
                             $"User was not notified.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n" +
                             $"Calling parameters: {psi.Arguments}";
        _ = LogErrors.LogErrorAsync(null, contextMessage);

        return Task.CompletedTask;
    }
}

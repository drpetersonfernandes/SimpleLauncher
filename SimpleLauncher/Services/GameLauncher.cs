using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public class GameLauncher
{
    private readonly string _logPath = GetLogPath.Path();
    private SystemManager.Emulator _selectedEmulatorManager;
    private string _selectedEmulatorParameters;
    private const int MemoryAccessViolation = -1073741819;
    private const int DepViolation = -1073740791;
    private readonly IExtractionService _extractionService;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Stats _stats;

    public GameLauncher(IExtractionService extractionService, PlaySoundEffects playSoundEffects, Stats stats)
    {
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));
    }

    public async Task HandleButtonClickAsync(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager selectedSystemManager, SettingsManager settings, MainWindow mainWindow, GamePadController gamePadController)
    {
        try
        {
            var resolvedFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath);

            if (string.IsNullOrWhiteSpace(resolvedFilePath) || (!File.Exists(resolvedFilePath) && !Directory.Exists(resolvedFilePath)))
            {
                // Notify developer
                var contextMessage = $"Invalid resolvedFilePath or file/directory does not exist.\n\n" +
                                     $"Original filePath: {filePath}\n" +
                                     $"Resolved filePath: {resolvedFilePath}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.FilePathIsInvalid(_logPath);

                return;
            }

            if (string.IsNullOrWhiteSpace(selectedEmulatorName))
            {
                // Notify developer
                const string contextMessage = "[HandleButtonClickAsync] selectedEmulatorName is null or empty.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);

                return;
            }

            if (selectedSystemName == null)
            {
                // Notify developer
                const string contextMessage = "selectedSystemName is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);

                return;
            }

            if (selectedSystemManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedSystemManager is null";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);

                return;
            }

            _selectedEmulatorManager = selectedSystemManager.Emulators.FirstOrDefault(e => e.EmulatorName.Equals(selectedEmulatorName, StringComparison.OrdinalIgnoreCase));
            if (_selectedEmulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "_selectedEmulatorManager is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);

                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedEmulatorManager.EmulatorName))
            {
                // Notify developer
                const string contextMessage = "_selectedEmulatorManager.EmulatorName is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);

                return;
            }

            _selectedEmulatorParameters = _selectedEmulatorManager.EmulatorParameters;

            var wasGamePadControllerRunning = gamePadController.IsRunning;
            if (wasGamePadControllerRunning)
            {
                gamePadController.Stop();
            }

            var startTime = DateTime.Now;
            mainWindow.IsLoadingGames = true;

            try
            {
                // Check for GroupByFolder compatibility before proceeding with any launch logic
                if (selectedSystemManager.GroupByFolder)
                {
                    var emulatorName = _selectedEmulatorManager.EmulatorName ?? string.Empty;
                    var emulatorLocation = _selectedEmulatorManager.EmulatorLocation ?? string.Empty;

                    var isMame = emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
                                 emulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                                 emulatorLocation.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase);

                    if (!isMame)
                    {
                        MessageBoxLibrary.GroupByFolderOnlyForMameMessageBox();
                        return; // Abort launch. The 'finally' block will handle cleanup.
                    }
                }

                // Specific handling for Cxbx-Reloaded
                if (selectedEmulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetExtension(resolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the system architecture is ARM64
                    if (IsArm64System())
                    {
                        // Notify the user that XISO mounting is not available on ARM64
                        MessageBoxLibrary.XisoMountNotSupportedOnArm64();
                        DebugLogger.Log("XISO mounting is not supported on ARM64 systems.");
                        mainWindow.IsLoadingGames = false;
                        return;
                    }

                    DebugLogger.Log($"Cxbx-Reloaded call detected. Attempting to mount and launch: {resolvedFilePath}");
                    await using var mountedDrive = await MountXisoFiles.MountAsync(resolvedFilePath, _logPath);
                    if (mountedDrive.IsMounted)
                    {
                        DebugLogger.Log($"ISO mounted successfully. Proceeding to launch {mountedDrive.MountedPath} with {selectedEmulatorName}.");
                        // Launch default.xbe
                        await LaunchRegularEmulatorAsync(mountedDrive.MountedPath, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, this);
                        DebugLogger.Log($"Emulator for {mountedDrive.MountedPath} has exited. Unmounting will occur automatically.");
                    }
                    else
                    {
                        DebugLogger.Log("ISO mounting failed. The user has been notified. Aborting launch.");
                        // User is already notified by MountAsync on failure.
                    }
                }
                // Specific handling for ScummVM games with ZIP files
                else if ((selectedSystemName.Contains("ScummVM", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("Scumm-VM", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("Scumm", StringComparison.OrdinalIgnoreCase))
                         && Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLogger.Log($"ScummVM game with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                    await MountZipFiles.MountZipFileAndLoadWithScummVmAsync(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, _logPath, this);
                }
                // Specific handling for RPCS3 with ZIP files
                else if (selectedEmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) && Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLogger.Log($"RPCS3 with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                    await MountZipFiles.MountZipFileAndLoadEbootBinAsync(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, gamePadController, _logPath, this);
                }
                // Specific handling for RPCS3 with ISO files
                else if (selectedEmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) &&
                         Path.GetExtension(resolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLogger.Log($"RPCS3 with ISO call detected. Attempting to mount ISO and launch: {resolvedFilePath}");
                    await MountIsoFiles.MountIsoFileAsync(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, gamePadController, _logPath, this);
                }
                // Specific handling for XBLA games with ZIP files
                else if ((selectedSystemName.Contains("xbla", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("xbox live", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("live arcade", StringComparison.OrdinalIgnoreCase) || resolvedFilePath.Contains("xbla", StringComparison.OrdinalIgnoreCase))
                         && Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLogger.Log($"XBLA game with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                    await MountZipFiles.MountZipFileAndSearchForFileToLoadAsync(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, gamePadController, _logPath, this);
                }
                else
                {
                    var fileExtension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();
                    switch (fileExtension)
                    {
                        case ".BAT":
                            await RunBatchFileAsync(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                            break;
                        case ".LNK":
                        case ".URL":
                            await LaunchShortcutFileAsync(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                            break;
                        case ".EXE":
                            await LaunchExecutableAsync(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                            break;
                        default:
                            await LaunchRegularEmulatorAsync(resolvedFilePath, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, this);
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);
            }
            finally
            {
                mainWindow.IsLoadingGames = false;
                if (wasGamePadControllerRunning)
                {
                    gamePadController.Start();
                }

                var endTime = DateTime.Now;
                var playTime = endTime - startTime;

                if (playTime.TotalSeconds > 5)
                {
                    UpdateStatsAndPlayCountAsync(playTime);
                }
            }
        }
        catch (Exception e)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(e, "Unhandled error in GameLauncher's main launch block.");
        }

        return;

        void UpdateStatsAndPlayCountAsync(TimeSpan playTime)
        {
            // Always use the original file path for history, not the resolved/extracted path.
            // The 'filePath' parameter passed into HandleButtonClickAsync is the original path from the game list.
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
                var playHistoryManager = mainWindow.PlayHistoryManager;
                playHistoryManager.AddOrUpdatePlayHistoryItem(fileNameForHistory, selectedSystemName, playTime);
                mainWindow.RefreshGameListAfterPlay(fileNameForHistory, selectedSystemName);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error updating play history";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
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
                _ = _stats.CallApiAsync(selectedEmulatorName);
            }
        }
    }

    private bool IsArm64System()
    {
        try
        {
            return System.Runtime.InteropServices.RuntimeInformation.OSArchitecture == System.Runtime.InteropServices.Architecture.Arm64;
        }
        catch (Exception ex)
        {
            // Log the exception but don't fail - assume not ARM64
            DebugLogger.Log($"Error detecting system architecture: {ex.Message}");
            return false;
        }
    }

    private async Task RunBatchFileAsync(string resolvedFilePath, SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        var psi = new ProcessStartInfo
        {
            FileName = resolvedFilePath,
            UseShellExecute = false, // UseShellExecute=false is required for redirecting output/error
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true, // Hide the console window
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for batch file: '{resolvedFilePath}'. Using default.");

            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        DebugLogger.Log("RunBatchFileAsync:\n\n");
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
            }
        }
    }

    private Task LaunchShortcutFileAsync(string resolvedFilePath, SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        // Common UI updates
        TrayIconManager.ShowTrayMessage($"{Path.GetFileName(resolvedFilePath)} launched");
        UpdateStatusBar.UpdateContent($"{Path.GetFileName(resolvedFilePath)} launched", mainWindow);

        try
        {
            var extension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();

            if (extension == ".URL")
            {
                // For .URL files (especially shell:AppsFolder), use the simplest ProcessStartInfo configuration.
                // This is equivalent to using the Windows Run dialog and avoids all WorkingDirectory issues
                // by letting the shell handle the protocol entirely.
                DebugLogger.Log("LaunchShortcutFileAsync (.URL):\n\n");
                DebugLogger.Log($"Shortcut File: {resolvedFilePath}");
                DebugLogger.Log("Using shell-only execution to handle protocol.\n");

                Process.Start(new ProcessStartInfo(resolvedFilePath) { UseShellExecute = true });
            }
            else // Assumes .LNK or other shell-executable files that might need a working directory
            {
                var psi = new ProcessStartInfo
                {
                    FileName = resolvedFilePath,
                    UseShellExecute = true
                };

                // Only set working directory for .lnk files, as they often require it.
                if (extension == ".LNK")
                {
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
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for shortcut file: '{resolvedFilePath}'. Using default.");
                        psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    }
                }

                DebugLogger.Log("LaunchShortcutFileAsync (.LNK):\n\n");
                DebugLogger.Log($"Shortcut File: {psi.FileName}");
                DebugLogger.Log($"Working Directory: {(psi.WorkingDirectory)}\n");

                using var process = new Process();
                process.StartInfo = psi;
                process.Start();
            }
        }
        catch (Exception ex)
        {
            // Centralized error handling for both cases
            var errorDetail = $"Exception launching the shortcut file.\n" +
                              $"Shortcut file: {resolvedFilePath}\n" +
                              $"Exception: {ex.Message}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
            }
        }

        return Task.CompletedTask;
    }

    private async Task LaunchExecutableAsync(string resolvedFilePath, SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        var psi = new ProcessStartInfo
        {
            FileName = resolvedFilePath,
            UseShellExecute = false,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for executable file: '{resolvedFilePath}'. Using default.");

            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        DebugLogger.Log("LaunchExecutableAsync:\n\n");
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
            }
        }
    }

    public async Task LaunchRegularEmulatorAsync(
        string resolvedFilePath,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        GameLauncher gameLauncher)
    {
        var isDirectory = Directory.Exists(resolvedFilePath);

        if (string.IsNullOrEmpty(selectedEmulatorName))
        {
            // Notify developer
            const string contextMessage = "[LaunchRegularEmulatorAsync] selectedEmulatorName is null or empty.";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            return;
        }

        // A simple and effective way to identify a mounted XBE path from our tool
        // is by its characteristic filename. This avoids hardcoding drive letters.
        var isMountedXbe = Path.GetFileName(resolvedFilePath).Equals("default.xbe", StringComparison.OrdinalIgnoreCase);

        // Check if the file to launch is a mounted ZIP file, which will not be extracted
        var isMountedZip = resolvedFilePath.StartsWith(MountZipFiles.ConfiguredMountDriveRoot, StringComparison.OrdinalIgnoreCase);

        // Declare tempExtractionPath here to be accessible in the finally block
        string tempExtractionPath = null;

        if (selectedSystemManager.ExtractFileBeforeLaunch == true && !isDirectory && !isMountedXbe && !isMountedZip)
        {
            if (selectedSystemManager.FileFormatsToLaunch == null || selectedSystemManager.FileFormatsToLaunch.Count == 0)
            {
                // Notify developer
                const string contextMessage = "FileFormatsToLaunch is null or empty, but ExtractFileBeforeLaunch is true for game launching. Cannot determine which file to launch after extraction.";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                // Notify user specifically for launching context
                MessageBoxLibrary.NullFileExtensionMessageBox(); // This is the correct place for this warning.
                return; // Abort launch due to incomplete configuration for launching
            }

            // Use the extraction service from the DI container
            var (extractedGameFilePath, extractedTempDirPath) = await _extractionService.ExtractToTempAndGetLaunchFileAsync(resolvedFilePath, selectedSystemManager.FileFormatsToLaunch);

            if (!string.IsNullOrEmpty(extractedGameFilePath))
            {
                resolvedFilePath = extractedGameFilePath;
            }

            // Always store the temp directory path for cleanup, even if no game file was found within it
            tempExtractionPath = extractedTempDirPath;
        }

        if (string.IsNullOrEmpty(resolvedFilePath))
        {
            // Notify developer
            const string contextMessage = "resolvedFilePath is null or empty after extraction attempt (or for mounted files).";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            // The finally block will handle cleanup of tempExtractionPath if it was set.
            return;
        }

        // For mounted files, ensure it still exists before proceeding
        if ((isMountedXbe || isMountedZip) && !File.Exists(resolvedFilePath))
        {
            // Notify developer
            var contextMessage = $"Mounted file {resolvedFilePath} not found when trying to launch with emulator.";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            return;
        }

        // Resolve the Emulator Path (executable)
        var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorManager.EmulatorLocation);
        if (string.IsNullOrEmpty(resolvedEmulatorExePath) || !File.Exists(resolvedEmulatorExePath))
        {
            // Notify developer
            var contextMessage = $"Emulator executable path is null, empty, or does not exist after resolving: '{selectedEmulatorManager.EmulatorLocation}' -> '{resolvedEmulatorExePath}'";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            return;
        }

        // Determine the emulator's directory, which is the base for %EMULATORFOLDER%
        var resolvedEmulatorFolderPath = Path.GetDirectoryName(resolvedEmulatorExePath);
        if (string.IsNullOrEmpty(resolvedEmulatorFolderPath) || !Directory.Exists(resolvedEmulatorFolderPath)) // Should exist if exe exists
        {
            // Notify developer
            var contextMessage = $"Could not determine emulator folder path from executable path: '{resolvedEmulatorExePath}'";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            return;
        }

        // Resolve System Folder Path, which is the base for %SYSTEMFOLDER%
        var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.PrimarySystemFolder);
        // Note: SystemFolder might not be strictly required to exist for all emulators/parameters,
        // but if %SYSTEMFOLDER% is used in parameters, this path needs to be valid.

        // Resolve Emulator Parameters using the PathHelper.ResolveParameterString
        var resolvedParameters = PathHelper.ResolveParameterString(
            rawEmulatorParameters, // The raw parameter string from config
            resolvedSystemFolderPath, // The fully resolved system folder path
            resolvedEmulatorFolderPath // The fully resolved emulator directory path
        );

        string arguments;

        // Handling MAME Related Games
        // Will load the filename without the extension
        if ((selectedEmulatorName.Equals("MAME", StringComparison.OrdinalIgnoreCase) ||
             selectedEmulatorManager.EmulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase)))
        {
            string mameRomName;
            if (isDirectory)
            {
                mameRomName = Path.GetFileName(resolvedFilePath);
            }
            else
            {
                mameRomName = Path.GetFileNameWithoutExtension(resolvedFilePath);
            }

            DebugLogger.Log($"MAME call detected. Attempting to launch: {mameRomName}");

            arguments = $"{resolvedParameters} \"{mameRomName}\"";
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            workingDirectory = AppDomain.CurrentDomain.BaseDirectory; // fallback
        }

        var psi = new ProcessStartInfo
        {
            FileName = resolvedEmulatorExePath,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        DebugLogger.Log($"LaunchRegularEmulatorAsync:\n\n" +
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

            if (!process.HasExited)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
            }

            if (process.HasExited)
            {
                if (DoNotCheckErrorsOnSpecificEmulators(selectedEmulatorName, resolvedEmulatorExePath, process, psi, output, error)) return;

                await CheckForMemoryAccessViolationAsync(process, psi, output, error, selectedEmulatorManager);
                await CheckForDepViolationAsync(process, psi, output, error, selectedEmulatorManager);
                await CheckForExitCodeWithErrorAnyAsync(process, psi, output, error, selectedEmulatorManager);
            }
        }
        catch (InvalidOperationException ex)
        {
            // Notify developer
            const string contextMessage = "InvalidOperationException while launching emulator.";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                await MessageBoxLibrary.InvalidOperationExceptionMessageBox(_logPath);
                await MessageBoxLibrary.DoYouWantToReceiveSupportFromTheDeveloper(ex, contextMessage, gameLauncher, _playSoundEffects);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            // Safely check if the process ever started before trying to access its properties.
            // A simple way is to check if an ID was ever assigned.
            string exitCodeInfo;
            try
            {
                // This check is safe even if the process didn't start.
                _ = process.Id;
                exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode : "N/A (Still Running or Failed to get code)")}";
            }
            catch (InvalidOperationException)
            {
                exitCodeInfo = "Exit code: N/A (Process failed to start)";
            }

            var errorDetail = $"{exitCodeInfo}\n" +
                              $"Emulator: {psi.FileName}\n" +
                              $"Calling parameters: {psi.Arguments}\n" +
                              $"Emulator output: {output}\n" +
                              $"Emulator error: {error}\n";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"The emulator could not open the game with the provided parameters. {userNotified}\n\n{errorDetail}";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                await MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);
                await MessageBoxLibrary.DoYouWantToReceiveSupportFromTheDeveloper(ex, contextMessage, gameLauncher, _playSoundEffects);
            }
        }
        finally
        {
            // Only attempt to delete if a temporary extraction path was actually set
            if (!string.IsNullOrEmpty(tempExtractionPath) && Directory.Exists(tempExtractionPath))
            {
                try
                {
                    DebugLogger.Log($"[LaunchRegularEmulatorAsync] Attempting to delete temporary extraction directory: {tempExtractionPath}");
                    Directory.Delete(tempExtractionPath, true); // Use Directory.Delete with recursive=true
                    DebugLogger.Log($"[LaunchRegularEmulatorAsync] Successfully deleted temporary extraction directory: {tempExtractionPath}");
                }
                catch (Exception ex)
                {
                    // Log the error but don't prevent other finally block actions
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to delete temporary extraction directory: {tempExtractionPath}");
                    DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error deleting temporary extraction directory {tempExtractionPath}: {ex.Message}");
                }
            }
        }
    }

    private async Task CheckForExitCodeWithErrorAnyAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorManager)
    {
        string contextMessage;

        // Ignore MemoryAccessViolation and DepViolation
        if (!process.HasExited || process.ExitCode == 0 || process.ExitCode == MemoryAccessViolation || process.ExitCode == DepViolation)
        {
            return;
        }

        // Check if the output contains "File open/read error" and ignore it,
        // Common RetroArch error that should be ignored
        if (output.ToString().Contains("File open/read error", StringComparison.OrdinalIgnoreCase))
        {
            DebugLogger.Log($"[CheckForExitCodeWithErrorAnyAsync] Ignored exit code {process.ExitCode} due to 'File open/read error' in output.");
            return;
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify developer
            contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                             $"User was notified.\n\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
        }
        else
        {
            // Notify developer
            contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                             $"User was not notified.\n\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify user
            await MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);
            await MessageBoxLibrary.DoYouWantToReceiveSupportFromTheDeveloper(null, contextMessage, this, _playSoundEffects);
        }
    }

    private static Task CheckForMemoryAccessViolationAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorManager)
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
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

        return Task.CompletedTask;
    }

    private Task CheckForDepViolationAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorManager)
    {
        if (process.HasExited && process.ExitCode != DepViolation) return Task.CompletedTask;

        // Notify developer
        var contextMessage = $"Data Execution Prevention (DEP) violation error occurred while running the emulator.\n" +
                             $"User was not notified.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

        return Task.CompletedTask;
    }

    private static bool DoNotCheckErrorsOnSpecificEmulators(string selectedEmulatorName, string resolvedEmulatorExePath, Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (selectedEmulatorName.Contains("Kega Fusion", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("KegaFusion", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Kega", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Fusion", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Project64", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Project 64", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Emulicious", StringComparison.OrdinalIgnoreCase) ||
            resolvedEmulatorExePath.Contains("Project64.exe", StringComparison.OrdinalIgnoreCase))
        {
            // Notify developer
            var contextMessage = $"User just ran {selectedEmulatorName}.\n" +
                                 $"'Simple Launcher' do not track error codes for this emulator.\n\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Calling parameters: {psi.Arguments}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

            return true;
        }

        return false;
    }
}
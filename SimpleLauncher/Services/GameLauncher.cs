using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

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

        // // Resolve paths within the parameter string using ParameterValidator
        // // It will just validate the paths, not actually resolve them.
        // var isMameSystem = selectedSystemManager.SystemIsMame;
        // var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.SystemFolder);
        // var (parametersValid, invalidPaths) = ParameterValidator.ValidateParameterPaths(_selectedEmulatorParameters, resolvedSystemFolder, isMameSystem);
        //
        // if (!parametersValid && invalidPaths != null && invalidPaths.Count > 0)
        // {
        //     // Notify user
        //     var proceedAnyway = MessageBoxLibrary.AskUserToProceedWithInvalidPath(invalidPaths);
        //     if (proceedAnyway == MessageBoxResult.No)
        //     {
        //         return;
        //     }
        // }

        var wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;
        if (wasGamePadControllerRunning)
        {
            GamePadController.Instance2.Stop();
        }

        var startTime = DateTime.Now;

        try
        {
            var fileExtension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();
            switch (fileExtension)
            {
                case ".BAT":
                    await LaunchBatchFile(resolvedFilePath);
                    break;
                case ".LNK":
                    await LaunchShortcutFile(resolvedFilePath);
                    break;
                case ".EXE":
                    await LaunchExecutable(resolvedFilePath);
                    break;
                default:
                    await LaunchRegularEmulator(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Generic error in the GameLauncher class.\n" +
                                 $"FilePath: {resolvedFilePath}\n" +
                                 $"SelectedSystem: {selectedSystemName}\n" +
                                 $"SelectedEmulator: {selectedEmulatorName}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            //
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
            DebugLogger.Log($"PlayTime saved: {playTime}");

            var fileName2 = Path.GetFileNameWithoutExtension(resolvedFilePath);
            var youPlayed = (string)Application.Current.TryFindResource("Youplayed") ?? "You played";
            var for2 = (string)Application.Current.TryFindResource("for") ?? "for";
            var playTimeFormatted = playTime.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture);
            TrayIconManager.ShowTrayMessage($"{youPlayed} {fileName2} {for2} {playTimeFormatted}");

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

    private static async Task LaunchBatchFile(string resolvedFilePath)
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

        DebugLogger.Log($"Launching Batch: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}");

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
                                 $"Exit code {process.ExitCode}\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"Output: {output}\n" +
                                 $"Error: {error}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchShortcutFile(string resolvedFilePath)
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

        DebugLogger.Log($"Launching Shortcut: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}");

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

    private static async Task LaunchExecutable(string resolvedFilePath)
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

        DebugLogger.Log($"Launching Executable: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}");

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
                                 $"Exit code {process.ExitCode}" +
                                 $"Exception: {ex.Message}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchRegularEmulator(
        string resolvedFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemConfig,
        SystemManager.Emulator selectedEmulatorConfig,
        string selectedEmulatorParameters) // This is the raw parameter string
    {
        if (selectedSystemConfig.ExtractFileBeforeLaunch == true)
        {
            resolvedFilePath = await ExtractFilesBeforeLaunch(resolvedFilePath, selectedSystemConfig);
        }

        if (string.IsNullOrEmpty(resolvedFilePath))
        {
            // Notify developer
            const string contextMessage = "resolvedFilePath is null or empty after extraction attempt.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        // Resolve the Emulator Path
        var resolvedEmulatorPath = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorConfig.EmulatorLocation);
        if (string.IsNullOrEmpty(resolvedEmulatorPath) || !File.Exists(resolvedEmulatorPath))
        {
            // Notify developer
            var contextMessage = $"Emulator resolvedEmulatorPath is null, empty, or does not exist after resolving: '{selectedEmulatorConfig.EmulatorLocation}' -> '{resolvedEmulatorPath}'";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        // Resolve Emulator Parameters
        var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(selectedSystemConfig.SystemFolder);
        var resolvedEmulatorParameters = ParameterValidator.ResolveParameterString(selectedEmulatorParameters, resolvedSystemFolder);
        var resolvedEmulatorParameters2 = PathHelper.ResolveOtherParameterString(resolvedEmulatorParameters, resolvedSystemFolder, resolvedEmulatorPath);

        var arguments = $"{resolvedEmulatorParameters2} \"{resolvedFilePath}\"";

        string workingDirectory;
        try
        {
            // Set the working directory to the directory of the emulator executable
            workingDirectory = Path.GetDirectoryName(resolvedEmulatorPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Could not get workingDirectory for programLocation: '{resolvedEmulatorPath}'";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            workingDirectory = AppDomain.CurrentDomain.BaseDirectory; // fallback
        }

        var psi = new ProcessStartInfo
        {
            FileName = resolvedEmulatorPath,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? AppDomain.CurrentDomain.BaseDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        DebugLogger.Log($"LaunchRegularEmulator:\n\n" +
                        $"Program Location: {resolvedEmulatorPath}\n" +
                        $"Arguments: {arguments}\n" +
                        $"PSI Working Directory: {psi.WorkingDirectory}\n");

        var fileName = Path.GetFileNameWithoutExtension(resolvedFilePath);
        var launchedwith = (string)Application.Current.TryFindResource("launchedwith") ?? "launched with";
        TrayIconManager.ShowTrayMessage($"{fileName} {launchedwith} {selectedEmulatorName}");

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
                if (await CheckForMemoryAccessViolation(process, psi, output, error, selectedEmulatorConfig)) return;
                if (await CheckForDepViolation(process, psi, output, error, selectedEmulatorConfig)) return;

                await CheckForExitCodeWithErrorAny(process, psi, output, error, selectedEmulatorConfig);
            }
        }
        catch (InvalidOperationException ex)
        {
            // Notify developer
            const string contextMessage = "Invalid Operation Exception while launching emulator.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorConfig.ReceiveANotificationOnEmulatorError)
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
            var userNotified = selectedEmulatorConfig.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"The emulator could not open the game with the provided parameters. {userNotified}\n\n{errorDetail}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorConfig.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
            }
        }
    }

    private static async Task<string> ExtractFilesBeforeLaunch(string resolvedFilePath, SystemManager systemManager)
    {
        var fileExtension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();

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

                // Notify user
                MessageBoxLibrary.CannotExtractThisFileMessageBox(resolvedFilePath);

                break;
            }
        }

        return null;

        static Task<string> ValidateAndFindGameFile(string tempExtractLocation, SystemManager sysConfig)
        {
            if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
            {
                // Notify developer
                var contextMessage = $"Extracted path is invalid: {tempExtractLocation}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return Task.FromResult<string>(null);
            }

            if (sysConfig.FileFormatsToLaunch == null || sysConfig.FileFormatsToLaunch.Count == 0)
            {
                // Notify developer
                const string contextMessage = "FileFormatsToLaunch is null or empty.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.NullFileExtensionMessageBox();

                return Task.FromResult<string>(null);
            }

            foreach (var formatToLaunch in sysConfig.FileFormatsToLaunch)
            {
                try
                {
                    var files = Directory.GetFiles(tempExtractLocation, $"*{formatToLaunch}", SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        return Task.FromResult(files[0]); // Return the first found file (which is already an absolute path)
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, $"Error searching for file format '{formatToLaunch}' in '{tempExtractLocation}'.");
                }
            }

            // Notify developer
            const string notFoundContext = "Could not find a file with the extension defined in 'Extension to Launch After Extraction'.";
            var exNotFound = new Exception(notFoundContext);
            _ = LogErrors.LogErrorAsync(exNotFound, notFoundContext);

            // Notify user
            MessageBoxLibrary.CouldNotFindAFileMessageBox();

            return Task.FromResult<string>(null);
        }
    }

    private static Task CheckForExitCodeWithErrorAny(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorConfig)
    {
        if (!process.HasExited || process.ExitCode == 0) return Task.CompletedTask; // Ensure process has exited

        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
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

        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        }

        return Task.CompletedTask;
    }

    private static Task<bool> CheckForMemoryAccessViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorConfig)
    {
        if (!process.HasExited || process.ExitCode != MemoryAccessViolation) // Ensure process has exited
        {
            return Task.FromResult(false);
        }

        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
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

        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify user
            MessageBoxLibrary.CheckForMemoryAccessViolation(LogPath);
        }

        return Task.FromResult(true);
    }

    private static Task<bool> CheckForDepViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorConfig)
    {
        if (!process.HasExited || process.ExitCode != DepViolation) // Ensure process has exited
        {
            return Task.FromResult(false);
        }

        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
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

        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
        {
            // Notify user
            MessageBoxLibrary.DepViolationMessageBox(LogPath);
        }

        return Task.FromResult(true);
    }
}
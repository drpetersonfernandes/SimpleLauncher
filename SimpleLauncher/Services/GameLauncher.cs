using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        var absoluteFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath);

        if (string.IsNullOrWhiteSpace(absoluteFilePath) || !File.Exists(absoluteFilePath))
        {
            // Notify developer
            const string contextMessage = "Invalid absoluteFilePath.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        if (string.IsNullOrWhiteSpace(selectedEmulatorName) || string.IsNullOrEmpty(selectedEmulatorName))
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
            const string contextMessage = "Invalid system.";
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

        // Check if this is a MAME system
        var isMameSystem = selectedSystemManager.SystemIsMame;
        var systemFolder = selectedSystemManager.SystemFolder;

        // Validate parameters but collect results rather than returning immediately
        var (parametersValid, invalidPaths) = ParameterValidator.ValidateEmulatorParameters(_selectedEmulatorParameters, systemFolder, isMameSystem);

        // If validation failed, ask the user if they want to proceed
        if (!parametersValid && invalidPaths != null && invalidPaths.Count > 0)
        {
            var proceedAnyway = MessageBoxLibrary.AskUserToProceedWithInvalidPath(invalidPaths); // Pass the full list for the message

            if (!proceedAnyway)
            {
                return; // User chose not to proceed
            }
            // If we're here, the user wants to proceed despite validation warnings
        }

        // Stop the GamePadController if it is running
        // To prevent interference with third party programs, like emulators or games
        var wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;
        if (wasGamePadControllerRunning)
        {
            GamePadController.Instance2.Stop();
        }

        // Start tracking the time when the game is launched
        // To track system playtime
        var startTime = DateTime.Now;

        try
        {
            var fileExtension = Path.GetExtension(absoluteFilePath).ToUpperInvariant();
            switch (fileExtension)
            {
                case ".BAT":
                    await LaunchBatchFile(absoluteFilePath);
                    break;
                case ".LNK":
                    await LaunchShortcutFile(absoluteFilePath);
                    break;
                case ".EXE":
                    await LaunchExecutable(absoluteFilePath);
                    break;
                default:
                    await LaunchRegularEmulator(absoluteFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Generic error in the GameLauncher class.\n" +
                                 $"FilePath: {absoluteFilePath}\n" +
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

            var endTime = DateTime.Now; // Capture the time when the game exits
            var playTime = endTime - startTime; // Calculate the playtime

            // Get file name
            var fileName = Path.GetFileName(absoluteFilePath);

            // Update system playtime
            settings.UpdateSystemPlayTime(selectedSystemName, playTime); // Update the system playtime in settings
            settings.Save(); // Save the updated settings

            // Update play history
            try
            {
                // Load and update play history
                var playHistoryManager = PlayHistoryManager.LoadPlayHistory();
                playHistoryManager.AddOrUpdatePlayHistoryItem(fileName, selectedSystemName, playTime);

                // Refresh the game list to update playtime in ListView mode
                mainWindow.RefreshGameListAfterPlay(fileName, selectedSystemName);
            }
            catch (Exception ex)
            {
                // Notify the developer
                const string contextMessage = "Error updating play history";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }

            // Update the PlayTime property in the MainWindow to refresh the UI
            var systemPlayTime = settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystemName);
            if (systemPlayTime != null)
            {
                mainWindow.PlayTime = systemPlayTime.PlayTime; // Update PlayTime in MainWindow
            }

            // Send Emulator Usage Stats
            if (selectedEmulatorName is not null)
            {
                _ = Stats.CallApiAsync(selectedEmulatorName);
            }
        }
    }

    private static async Task LaunchBatchFile(string absoluteFilePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = absoluteFilePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Debug.WriteLine(absoluteFilePath);
        Debug.WriteLine(psi.WorkingDirectory);

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
            var contextMessage = $"There was an issue running the batch process. User was not notified.\n" +
                                 $"Batch file: {psi.FileName}\n" +
                                 $"Exit code {process.ExitCode}\n" +
                                 $"Output: {output}\n" +
                                 $"Error: {error}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchShortcutFile(string absoluteFilePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = absoluteFilePath,
            UseShellExecute = true
        };

        Debug.WriteLine(absoluteFilePath);
        Debug.WriteLine(psi.WorkingDirectory);

        using var process = new Process();
        process.StartInfo = psi;

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                // Notify developer
                var contextMessage = $"Error launching the shortcut file. User was not notified.\n" +
                                     $"Shortcut file: {psi.FileName}\n" +
                                     $"Exit code {process.ExitCode}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Error launching the shortcut file. User was not notified.\n" +
                                 $"Shortcut file: {psi.FileName}\n" +
                                 $"Exit code {process.ExitCode}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchExecutable(string absoluteFilePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = absoluteFilePath,
            UseShellExecute = true
        };

        Debug.WriteLine(absoluteFilePath);
        Debug.WriteLine(psi.WorkingDirectory);

        using var process = new Process();
        process.StartInfo = psi;

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                // Notify developer
                var contextMessage = $"Error launching the executable file. User was not notified.\n" +
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
            var contextMessage = $"Error launching the executable file. User was not notified.\n" +
                                 $"Executable file: {psi.FileName}\n" +
                                 $"Exit code {process.ExitCode}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchRegularEmulator(
        string absoluteFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemConfig,
        SystemManager.Emulator selectedEmulatorConfig,
        string selectedEmulatorParameters)
    {
        if (selectedSystemConfig.ExtractFileBeforeLaunch == true)
        {
            absoluteFilePath = await ExtractFilesBeforeLaunch(absoluteFilePath, selectedSystemConfig);
        }

        if (string.IsNullOrEmpty(absoluteFilePath))
        {
            // Notify developer
            const string contextMessage = "absoluteFilePath is null or empty after extraction attempt.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        // Ensure programLocation is absolute and exists
        var programLocation = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorConfig.EmulatorLocation);
        if (string.IsNullOrEmpty(programLocation) || !File.Exists(programLocation))
        {
            // Notify developer
            var contextMessage = $"Emulator programLocation is null, empty, or does not exist: '{programLocation}'";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        // Use the selectedEmulatorParameters directly
        var arguments = $"{selectedEmulatorParameters} \"{absoluteFilePath}\"";

        string workingDirectory = null;
        try
        {
            workingDirectory = Path.GetDirectoryName(programLocation);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Could not get workingDirectory for programLocation: '{programLocation}'";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
            // Proceed with null workingDirectory, which defaults to app base or process default
        }

        var psi = new ProcessStartInfo
        {
            FileName = programLocation,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? AppDomain.CurrentDomain.BaseDirectory, // Fallback
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Debug.WriteLine(programLocation);
        Debug.WriteLine(arguments);
        Debug.WriteLine(workingDirectory);
        Debug.WriteLine(psi.WorkingDirectory);

        DebugLogger.Log($"Program Location: {programLocation}");
        DebugLogger.Log($"Arguments: {arguments}");
        DebugLogger.Log($"Working Directory: {workingDirectory}");
        DebugLogger.Log($"PSI Working Directory: {psi.WorkingDirectory}");

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

            // Add a small delay to ensure the process is properly initialized
            await Task.Delay(100);

            // Only setup output redirection if the process is still running
            if (!process.HasExited)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
            }

            // Check process state before accessing properties
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

            // Notify user
            if (selectedEmulatorConfig.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.InvalidOperationExceptionMessageBox(LogPath);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorDetail = $"Exit code: {process.ExitCode}\n" +
                              $"Emulator: {psi.FileName}\n" +
                              $"Emulator output: {output}\n" +
                              $"Emulator error: {error}\n" +
                              $"Calling parameters: {psi.Arguments}";
            var userNotified = selectedEmulatorConfig.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"The emulator could not open the game with the provided parameters. {userNotified}\n\n{errorDetail}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            if (selectedEmulatorConfig.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
            }
        }
    }

    private static async Task<string> ExtractFilesBeforeLaunch(string absoluteFilePath, SystemManager systemManager)
    {
        var fileExtension = Path.GetExtension(absoluteFilePath).ToUpperInvariant();

        switch (fileExtension)
        {
            case ".ZIP":
            {
                var extractCompressedFile = new ExtractCompressedFile();
                var pathToExtractionDirectory = await extractCompressedFile.ExtractGameToTempAsync2(absoluteFilePath);

                var extractedFileToLaunch = await ValidateAndFindGameFile(pathToExtractionDirectory, systemManager);
                if (!string.IsNullOrEmpty(extractedFileToLaunch))
                    return extractedFileToLaunch;

                break;
            }
            case ".7Z" or ".RAR":
            {
                var extractCompressedFile = new ExtractCompressedFile();
                var pathToExtractionDirectory = await extractCompressedFile.ExtractGameToTempAsync(absoluteFilePath);

                var extractedFileToLaunch = await ValidateAndFindGameFile(pathToExtractionDirectory, systemManager);
                if (!string.IsNullOrEmpty(extractedFileToLaunch))
                    return extractedFileToLaunch;

                break;
            }
            default:
            {
                // Notify developer
                var contextMessage = $"Can not extract file: {absoluteFilePath}";
                _ = LogErrors.LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.CannotExtractThisFileMessageBox(absoluteFilePath);

                break;
            }
        }

        return null;

        Task<string> ValidateAndFindGameFile(string tempExtractLocation, SystemManager sysConfig)
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

            // Search for any file with specified extensions recursively
            foreach (var formatToLaunch in sysConfig.FileFormatsToLaunch)
            {
                var files = Directory.GetFiles(tempExtractLocation, $"*{formatToLaunch}", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    return Task.FromResult(files[0]); // Return the first found file
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
        if (process.ExitCode == 0) return Task.CompletedTask;

        // Notify developer
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

        // Notify the user only if he wants
        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
        {
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        }

        return Task.CompletedTask;
    }

    private static Task<bool> CheckForMemoryAccessViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorConfig)
    {
        if (process.ExitCode != MemoryAccessViolation)
        {
            return Task.FromResult(false);
        }

        // Notify developer
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

        // Notify the user only if he wants
        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
        {
            MessageBoxLibrary.CheckForMemoryAccessViolation(LogPath);
        }

        return Task.FromResult(true);
    }

    private static Task<bool> CheckForDepViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorConfig)
    {
        if (process.ExitCode != DepViolation)
        {
            return Task.FromResult(false);
        }

        // Notify developer
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

        // Notify the user only if he wants
        if (emulatorConfig.ReceiveANotificationOnEmulatorError == true)
        {
            MessageBoxLibrary.DepViolationMessageBox(LogPath);
        }

        return Task.FromResult(true);
    }

    private static Task<bool> CheckForTempExtractLocation(string tempExtractLocation)
    {
        if (!string.IsNullOrEmpty(tempExtractLocation) && Directory.Exists(tempExtractLocation)) return Task.FromResult(false);

        // Notify developer
        const string contextMessage = "Extraction failed.";
        _ = LogErrors.LogErrorAsync(null, contextMessage);

        // Notify user
        MessageBoxLibrary.ExtractionFailedMessageBox();

        return Task.FromResult(true);
    }
}
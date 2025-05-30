using System;
using System.Diagnostics;
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
    // filePath is already resolved by the caller (MainWindow, FavoritesWindow, GlobalSearchWindow)
    var absoluteFilePath = filePath;

    if (string.IsNullOrWhiteSpace(absoluteFilePath) || !File.Exists(absoluteFilePath))
    {
        const string contextMessage = "Invalid absoluteFilePath or file does not exist.";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        return;
    }

    if (string.IsNullOrWhiteSpace(selectedEmulatorName))
    {
        const string contextMessage = "selectedEmulatorName is null or empty.";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        return;
    }

    if (selectedSystemName == null)
    {
        const string contextMessage = "Invalid system.";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        return;
    }

    if (selectedSystemManager == null)
    {
        const string contextMessage = "selectedSystemManager is null";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        return;
    }

    _selectedEmulatorManager = selectedSystemManager.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
    if (_selectedEmulatorManager == null)
    {
        const string contextMessage = "_selectedEmulatorManager is null.";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        return;
    }

    if (string.IsNullOrWhiteSpace(_selectedEmulatorManager.EmulatorName))
    {
        const string contextMessage = "_selectedEmulatorManager.EmulatorName is null.";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        return;
    }

    _selectedEmulatorParameters = _selectedEmulatorManager.EmulatorParameters; // Get the raw parameter string

    var isMameSystem = selectedSystemManager.SystemIsMame;
    var systemFolder = selectedSystemManager.SystemFolder; // Use the raw system folder string for parameter resolution context

    // Validate parameters using the raw parameter string and raw system folder string
    var (parametersValid, invalidPaths) = ParameterValidator.ValidateParameterPaths(_selectedEmulatorParameters, systemFolder, isMameSystem);

    if (!parametersValid && invalidPaths != null && invalidPaths.Count > 0)
    {
        var proceedAnyway = MessageBoxLibrary.AskUserToProceedWithInvalidPath(invalidPaths);
        if (proceedAnyway == MessageBoxResult.No)
        {
            return;
        }
    }

    var wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;
    if (wasGamePadControllerRunning)
    {
        GamePadController.Instance2.Stop();
    }

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
                // LaunchRegularEmulator needs the raw parameter string and raw system folder string
                await LaunchRegularEmulator(absoluteFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters);
                break;
        }
    }
    catch (Exception ex)
    {
        var contextMessage = $"Generic error in the GameLauncher class.\n" +
                             $"FilePath: {absoluteFilePath}\n" +
                             $"SelectedSystem: {selectedSystemName}\n" +
                             $"SelectedEmulator: {selectedEmulatorName}";
        _ = LogErrors.LogErrorAsync(ex, contextMessage);
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

        var fileName = Path.GetFileName(absoluteFilePath);

        settings.UpdateSystemPlayTime(selectedSystemName, playTime);
        settings.Save();

        try
        {
            var playHistoryManager = PlayHistoryManager.LoadPlayHistory();
            playHistoryManager.AddOrUpdatePlayHistoryItem(fileName, selectedSystemName, playTime);
            mainWindow.RefreshGameListAfterPlay(fileName, selectedSystemName);
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error updating play history";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }

        var systemPlayTime = settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystemName);
        if (systemPlayTime != null)
        {
            mainWindow.PlayTime = systemPlayTime.PlayTime;
        }

        if (selectedEmulatorName is not null)
        {
            _ = Stats.CallApiAsync(selectedEmulatorName);
        }
    }
}
    
    private static void LaunchExternalTool(string toolPath, string arguments = null, string workingDirectory = null)
{
    if (string.IsNullOrEmpty(toolPath))
    {
        const string contextMessage = "Tool path cannot be null or empty.";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.SelectedToolNotFoundMessageBox();
        return;
    }

    if (!File.Exists(toolPath))
    {
        var contextMessage = $"External tool not found: {toolPath}";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.SelectedToolNotFoundMessageBox();
        return;
    }

    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = toolPath,
            Arguments = arguments ?? string.Empty,
            UseShellExecute = true
        };

        // Set working directory if provided, resolving it relative to the app directory
        if (!string.IsNullOrEmpty(workingDirectory))
        {
            var resolvedWorkingDirectory = PathHelper.ResolveRelativeToAppDirectory(workingDirectory);

            if (!string.IsNullOrEmpty(resolvedWorkingDirectory) && Directory.Exists(resolvedWorkingDirectory)) // Check if resolution was successful and directory exists
            {
                psi.WorkingDirectory = resolvedWorkingDirectory;
            }
            else
            {
                var warningMessage = $"Specified working directory not found or resolution failed: '{workingDirectory}' -> '{resolvedWorkingDirectory}'. Launching with default working directory.";
                _ = LogErrors.LogErrorAsync(new DirectoryNotFoundException(warningMessage), warningMessage);
                // Let psi.WorkingDirectory default to the executable's directory or AppDomain.CurrentDomain.BaseDirectory
            }
        }

        Process.Start(psi);
    }
    catch (Exception ex)
    {
        var contextMessage = $"An error occurred while launching external tool: {toolPath}.\n" +
                             $"Arguments: {arguments ?? "None"}\n" +
                             $"Working Directory: {workingDirectory ?? "Default"}";
        _ = LogErrors.LogErrorAsync(ex, contextMessage);
        MessageBoxLibrary.ErrorLaunchingToolMessageBox(LogPath);
    }
}


    private static async Task LaunchBatchFile(string absoluteFilePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = absoluteFilePath,
            UseShellExecute = false, // UseShellExecute=false is required for redirecting output/error
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true // Hide the console window
        };

        // Set working directory to the directory of the batch file
        try
        {
            var workingDirectory = Path.GetDirectoryName(absoluteFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Could not get workingDirectory for batch file: '{absoluteFilePath}'. Using default.");
            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        Debug.WriteLine($"Launching Batch: {psi.FileName}");
        Debug.WriteLine($"Working Directory: {psi.WorkingDirectory}");

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

    private static async Task LaunchShortcutFile(string absoluteFilePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = absoluteFilePath,
            UseShellExecute = true // UseShellExecute=true is typical for launching .lnk
        };

        // Working directory is often ignored for .lnk files when UseShellExecute is true,
        // but setting it doesn't hurt.
        try
        {
            var workingDirectory = Path.GetDirectoryName(absoluteFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Could not get workingDirectory for shortcut file: '{absoluteFilePath}'. Using default.");
            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        Debug.WriteLine($"Launching Shortcut: {psi.FileName}");
        Debug.WriteLine($"Working Directory: {psi.WorkingDirectory}");

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

    private static async Task LaunchExecutable(string absoluteFilePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = absoluteFilePath,
            UseShellExecute = true // UseShellExecute=true is typical for launching .exe directly
        };

        // Set working directory to the directory of the executable
        try
        {
            var workingDirectory = Path.GetDirectoryName(absoluteFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Could not get workingDirectory for executable file: '{absoluteFilePath}'. Using default.");
            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        Debug.WriteLine($"Launching Executable: {psi.FileName}");
        Debug.WriteLine($"Working Directory: {psi.WorkingDirectory}");

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
    string absoluteFilePath, // This is already resolved
    string selectedSystemName,
    string selectedEmulatorName,
    SystemManager selectedSystemConfig,
    SystemManager.Emulator selectedEmulatorConfig,
    string selectedEmulatorParameters) // This is the raw parameter string
{
    if (selectedSystemConfig.ExtractFileBeforeLaunch == true)
    {
        absoluteFilePath = await ExtractFilesBeforeLaunch(absoluteFilePath, selectedSystemConfig);
    }

    if (string.IsNullOrEmpty(absoluteFilePath))
    {
        const string contextMessage = "absoluteFilePath is null or empty after extraction attempt.";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        return;
    }

    // Resolve programLocation using PathHelper
    var programLocation = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorConfig.EmulatorLocation);
    if (string.IsNullOrEmpty(programLocation) || !File.Exists(programLocation))
    {
        var contextMessage = $"Emulator programLocation is null, empty, or does not exist after resolving: '{selectedEmulatorConfig.EmulatorLocation}' -> '{programLocation}'";
        _ = LogErrors.LogErrorAsync(null, contextMessage);
        MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
        return;
    }

    // Resolve paths within the parameter string using ParameterValidator
    // Pass the raw system folder string for parameter resolution context
    var resolvedArguments = ParameterValidator.ResolveParameterString(selectedEmulatorParameters, selectedSystemConfig.SystemFolder);

    var arguments = $"{resolvedArguments} \"{absoluteFilePath}\"";

    string workingDirectory = null;
    try
    {
        // Set working directory to the directory of the emulator executable
        // Use PathHelper.ResolveRelativeToAppDirectory for consistency if working directory was configured
        // If not configured, Path.GetDirectoryName(programLocation) is fine
        workingDirectory = Path.GetDirectoryName(programLocation);
    }
    catch (Exception ex)
    {
        var contextMessage = $"Could not get workingDirectory for programLocation: '{programLocation}'";
        _ = LogErrors.LogErrorAsync(ex, contextMessage);
    }

    var psi = new ProcessStartInfo
    {
        FileName = programLocation,
        Arguments = arguments,
        WorkingDirectory = workingDirectory ?? AppDomain.CurrentDomain.BaseDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    Debug.WriteLine($"Launching Emulator: {psi.FileName}");
    Debug.WriteLine($"Arguments: {psi.Arguments}");
    Debug.WriteLine($"Working Directory: {psi.WorkingDirectory}");

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
        const string contextMessage = "Invalid Operation Exception while launching emulator.";
        _ = LogErrors.LogErrorAsync(ex, contextMessage);
        if (selectedEmulatorConfig.ReceiveANotificationOnEmulatorError)
        {
            MessageBoxLibrary.InvalidOperationExceptionMessageBox(LogPath);
        }
    }
    catch (Exception ex)
    {
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
            // ExtractGameToTempAsync2 uses PathHelper internally
            var pathToExtractionDirectory = await extractCompressedFile.ExtractGameToTempAsync2(absoluteFilePath);

            // ValidateAndFindGameFile needs the resolved temp path
            var extractedFileToLaunch = await ValidateAndFindGameFile(pathToExtractionDirectory, systemManager);
            if (!string.IsNullOrEmpty(extractedFileToLaunch))
                return extractedFileToLaunch;

            break;
        }
        case ".7Z" or ".RAR":
        {
            var extractCompressedFile = new ExtractCompressedFile();
            // ExtractGameToTempAsync uses PathHelper internally
            var pathToExtractionDirectory = await extractCompressedFile.ExtractGameToTempAsync(absoluteFilePath);

            // ValidateAndFindGameFile needs the resolved temp path
            var extractedFileToLaunch = await ValidateAndFindGameFile(pathToExtractionDirectory, systemManager);
            if (!string.IsNullOrEmpty(extractedFileToLaunch))
                return extractedFileToLaunch;

            break;
        }
        default:
        {
            var contextMessage = $"Can not extract file: {absoluteFilePath}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.CannotExtractThisFileMessageBox(absoluteFilePath);
            break;
        }
    }

    return null;

    // ValidateAndFindGameFile: This method needs the resolved temp path
    static Task<string> ValidateAndFindGameFile(string tempExtractLocation, SystemManager sysConfig)
    {
        if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
        {
            var contextMessage = $"Extracted path is invalid: {tempExtractLocation}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.ExtractionFailedMessageBox();
            return Task.FromResult<string>(null);
        }

        if (sysConfig.FileFormatsToLaunch == null || sysConfig.FileFormatsToLaunch.Count == 0)
        {
            const string contextMessage = "FileFormatsToLaunch is null or empty.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);
            MessageBoxLibrary.NullFileExtensionMessageBox();
            return Task.FromResult<string>(null);
        }

        foreach (var formatToLaunch in sysConfig.FileFormatsToLaunch)
        {
            try
            {
                // Search within the resolved temp location
                var files = Directory.GetFiles(tempExtractLocation, $"*{formatToLaunch}", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    return Task.FromResult(files[0]); // Return the first found file (which is already an absolute path)
                }
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, $"Error searching for file format '{formatToLaunch}' in '{tempExtractLocation}'.");
            }
        }

        const string notFoundContext = "Could not find a file with the extension defined in 'Extension to Launch After Extraction'.";
        var exNotFound = new Exception(notFoundContext);
        _ = LogErrors.LogErrorAsync(exNotFound, notFoundContext);
        MessageBoxLibrary.CouldNotFindAFileMessageBox();
        return Task.FromResult<string>(null);
    }
}

    private static Task CheckForExitCodeWithErrorAny(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.Emulator emulatorConfig)
    {
        if (!process.HasExited || process.ExitCode == 0) return Task.CompletedTask; // Ensure process has exited

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
        if (!process.HasExited || process.ExitCode != MemoryAccessViolation) // Ensure process has exited
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
        if (!process.HasExited || process.ExitCode != DepViolation) // Ensure process has exited
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
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SimpleLauncher;

public static class GameLauncher
{
    private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");

    public static async Task HandleButtonClick(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs, SettingsManager settings, MainWindow mainWindow)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            // Notify developer
            const string contextMessage = "Invalid filePath.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        if (emulatorComboBox.SelectedItem == null)
        {
            // Notify developer
            const string contextMessage = "Invalid emulator.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        if (systemComboBox.SelectedItem == null)
        {
            // Notify developer
            const string contextMessage = "Invalid system.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        var selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        var selectedSystem = systemComboBox.SelectedItem?.ToString() ?? string.Empty;
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (systemConfig == null)
        {
            // Notify developer
            const string contextMessage = "systemConfig is null.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (emulatorConfig == null)
        {
            // Notify developer
            const string contextMessage = "emulatorConfig is null.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

            return;
        }

        var programLocation = emulatorConfig.EmulatorLocation;
        var parameters = emulatorConfig.EmulatorParameters;

        // Check if this is a MAME system
        var isMameSystem = systemConfig.SystemIsMame;
        var systemFolder = systemConfig.SystemFolder;

        // Validate program location and parameters but collect results rather than returning immediately
        var (programLocationValid, invalidProgramLocation) = ParameterValidator.ValidateProgramLocation(programLocation);
        var (parametersValid, invalidPaths) = ParameterValidator.ValidateEmulatorParameters(parameters, systemFolder, isMameSystem);

        // If either validation failed, ask the user if they want to proceed
        if (!programLocationValid || !parametersValid)
        {
            var proceedAnyway = MessageBoxLibrary.AskUserToProceedWithInvalidPath(
                programLocationValid ? null : invalidProgramLocation,
                invalidPaths);

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
            var fileExtension = Path.GetExtension(filePath).ToUpperInvariant();
            switch (fileExtension)
            {
                case ".BAT":
                    await LaunchBatchFile(filePath);
                    break;
                case ".LNK":
                    await LaunchShortcutFile(filePath);
                    break;
                case ".EXE":
                    await LaunchExecutable(filePath);
                    break;
                default:
                    if (selectedSystem.Contains("XBLA", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await LaunchXblaGame(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    }
                    else if (selectedSystem.Contains("aquarius", StringComparison.InvariantCultureIgnoreCase) && selectedEmulatorName != null && selectedEmulatorName.Contains("mame", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await LaunchMattelAquariusGame(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    }
                    else
                    {
                        await LaunchRegularEmulator(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    }

                    break;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Generic error in the GameLauncher class.\n" +
                                 $"FilePath: {filePath}\n" +
                                 $"SelectedSystem: {systemComboBox.SelectedItem}\n" +
                                 $"SelectedEmulator: {emulatorComboBox.SelectedItem}";
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
            var fileName = Path.GetFileName(filePath);

            // Update system playtime
            settings.UpdateSystemPlayTime(selectedSystem, playTime); // Update the system playtime in settings
            settings.Save(); // Save the updated settings

            // Update play history
            try
            {
                // Load and update play history
                var playHistoryManager = PlayHistoryManager.LoadPlayHistory();
                playHistoryManager.AddOrUpdatePlayHistoryItem(fileName, selectedSystem, playTime);

                // Refresh the game list to update playtime in ListView mode
                mainWindow.RefreshGameListAfterPlay(fileName, selectedSystem);
            }
            catch (Exception ex)
            {
                // Notify the developer
                const string contextMessage = "Error updating play history";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }

            // Update the PlayTime property in the MainWindow to refresh the UI
            var systemPlayTime = settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystem);
            if (systemPlayTime != null)
            {
                mainWindow.PlayTime = systemPlayTime.PlayTime; // Update PlayTime in MainWindow
            }

            // Send Emulator Usage Stats
            if (emulatorComboBox.SelectedItem is not null)
            {
                _ = Stats.CallApiAsync(emulatorComboBox.SelectedItem.ToString());
            }
        }
    }

    private static async Task LaunchBatchFile(string filePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

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
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
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
        }
    }

    private static async Task LaunchShortcutFile(string filePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        };

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
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Error launching the shortcut file. User was not notified.\n" +
                                 $"Shortcut file: {psi.FileName}\n" +
                                 $"Exit code {process.ExitCode}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private static async Task LaunchExecutable(string filePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        };

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
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Error launching the executable file. User was not notified.\n" +
                                 $"Executable file: {psi.FileName}\n" +
                                 $"Exit code {process.ExitCode}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private static async Task LaunchRegularEmulator(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        var filePathToLaunch = filePath;
        var selectedEmulator = emulatorComboBox?.SelectedItem.ToString();
        var selectedSystem = systemComboBox?.SelectedItem.ToString();

        if (string.IsNullOrEmpty(selectedEmulator) || string.IsNullOrEmpty(selectedSystem))
        {
            // Notify developer
            const string contextMessage = "selectedEmulator or selectedSystem is null";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        var selectedSystemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (selectedSystemConfig == null)
        {
            // Notify developer
            const string contextMessage = "selectedSystemConfig is null";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        if (selectedSystemConfig.ExtractFileBeforeLaunch)
        {
            filePathToLaunch = await ExtractFilesBeforeLaunch(filePath, selectedSystemConfig, filePathToLaunch);
        }

        if (string.IsNullOrEmpty(filePathToLaunch) || !File.Exists(filePathToLaunch))
        {
            // Notify developer
            var contextMessage = $"Invalid filePathToLaunch: {filePathToLaunch}";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        var selectedEmulatorConfig = selectedSystemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulator);

        if (selectedEmulatorConfig == null)
        {
            // Notify developer
            const string contextMessage = "selectedEmulatorConfig is null";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        // Construct the PSI
        var programLocation = selectedEmulatorConfig.EmulatorLocation;
        var parameters = selectedEmulatorConfig.EmulatorParameters;
        var arguments = $"{parameters} \"{filePathToLaunch}\"";

        var psi = new ProcessStartInfo
        {
            FileName = programLocation,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

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
            const string contextMessage = "Invalid Operation Exception";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user only if he wants
            if (selectedEmulatorConfig.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.InvalidOperationExceptionMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n" +
                                 $"Calling parameters: {psi.Arguments}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user only if he wants
            if (selectedEmulatorConfig.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
            }
        }
    }

    private static async Task LaunchXblaGame(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        var selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        var selectedSystem = systemComboBox.SelectedItem.ToString();
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (systemConfig == null) return;

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        string gamePathToLaunch = null;

        // Force extraction of the compressed file even if the config is wrongly set to false
        systemConfig.ExtractFileBeforeLaunch = true;

        // Check if extraction is needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            var fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

            // Accept ZIP, 7Z and RAR files
            if (fileExtension is ".ZIP" or ".7Z" or ".RAR")
            {
                // Create Instance of ExtractCompressedFile
                var extractCompressedFile = new ExtractCompressedFile();
                var tempExtractLocation = await extractCompressedFile.ExtractGameToTempAsync(filePath);

                if (await CheckForTempExtractLocation(tempExtractLocation)) return;

                gamePathToLaunch = await FindXblaGamePath(tempExtractLocation); // Search within the extracted folder
            }
            else
            {
                MessageBoxLibrary.CannotExtractThisFileMessageBox(filePath);

                return;
            }
        }

        if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
        {
            // Notify developer
            var contextMessage = $"Invalid gamePathToLaunch: {gamePathToLaunch}";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return;
        }

        // Construct the PSI
        if (emulatorConfig == null) return;

        {
            var programLocation = emulatorConfig.EmulatorLocation;
            var parameters = emulatorConfig.EmulatorParameters;
            var arguments = $"{parameters} \"{gamePathToLaunch}\"";

            var psi = new ProcessStartInfo
            {
                FileName = programLocation,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

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

                if (!process.HasExited)
                {
                    throw new InvalidOperationException("Process has not exited as expected.");
                }

                if (await CheckForMemoryAccessViolation(process, psi, output, error, emulatorConfig)) return;
                if (await CheckForDepViolation(process, psi, output, error, emulatorConfig)) return;

                await CheckForExitCodeWithErrorAny(process, psi, output, error, emulatorConfig);
            }
            catch (InvalidOperationException ex)
            {
                // Notify developer
                const string contextMessage = "Invalid Operation Exception";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user only if he wants
                if (emulatorConfig.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.InvalidOperationExceptionMessageBox();
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                                     $"Exit code: {process.ExitCode}\n" +
                                     $"Emulator: {psi.FileName}\n" +
                                     $"Emulator output: {output}\n" +
                                     $"Emulator error: {error}\n" +
                                     $"Calling parameters: {psi.Arguments}";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user only if he wants
                if (emulatorConfig.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.EmulatorCouldNotOpenXboxXblaSimpleMessageBox(LogPath);
                }
            }
        }

        return;

        static Task<string> FindXblaGamePath(string rootFolderPath)
        {
            var directories = Directory.GetDirectories(rootFolderPath, "000D0000", SearchOption.AllDirectories);
            foreach (var directory in directories)
            {
                var files = Directory.GetFiles(directory);
                if (files.Length > 0)
                {
                    return Task.FromResult(files[0]); // Return the first file found
                }
            }

            return Task.FromResult(string.Empty);
        }
    }

    private static async Task LaunchMattelAquariusGame(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        var selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        var selectedSystem = systemComboBox.SelectedItem.ToString();
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (systemConfig != null)
        {
            var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

            var gamePathToLaunch = filePath;

            // Extract File if Needed
            if (systemConfig.ExtractFileBeforeLaunch)
            {
                gamePathToLaunch = await ExtractFilesBeforeLaunch(filePath, systemConfig, gamePathToLaunch);
            }

            if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
            {
                // Notify developer
                var contextMessage = $"Invalid gamePathToLaunch: {gamePathToLaunch}";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

                return;
            }

            // Construct the PSI
            if (emulatorConfig != null)
            {
                var programLocation = emulatorConfig.EmulatorLocation;
                var parameters = emulatorConfig.EmulatorParameters;
                var workingDirectory = Path.GetDirectoryName(programLocation);
                var gameFilenameWithoutExtension = Path.GetFileNameWithoutExtension(gamePathToLaunch);
                var arguments = $"{parameters} {gameFilenameWithoutExtension}";

                // Check workingDirectory
                if (await CheckForWorkingDirectory(workingDirectory)) return;

                if (workingDirectory != null)
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = programLocation,
                        Arguments = arguments,
                        WorkingDirectory = workingDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

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

                        if (!process.HasExited)
                        {
                            throw new InvalidOperationException("Process has not exited as expected.");
                        }

                        if (await CheckForMemoryAccessViolation(process, psi, output, error, emulatorConfig)) return;
                        if (await CheckForDepViolation(process, psi, output, error, emulatorConfig)) return;

                        await CheckForExitCodeWithErrorAny(process, psi, output, error, emulatorConfig);
                    }

                    catch (InvalidOperationException ex)
                    {
                        // Notify developer
                        const string contextMessage = "Invalid Operation Exception.";
                        _ = LogErrors.LogErrorAsync(ex, contextMessage);

                        // Notify user only if he wants
                        if (emulatorConfig.ReceiveANotificationOnEmulatorError)
                        {
                            MessageBoxLibrary.InvalidOperationExceptionMessageBox();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        var contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                                             $"Exit code: {process.ExitCode}\n" +
                                             $"Emulator: {psi.FileName}\n" +
                                             $"Emulator output: {output}\n" +
                                             $"Emulator error: {error}\n" +
                                             $"Calling parameters: {psi.Arguments}";
                        _ = LogErrors.LogErrorAsync(ex, contextMessage);

                        // Notify user only if he wants
                        if (emulatorConfig.ReceiveANotificationOnEmulatorError)
                        {
                            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
                        }
                    }
                }
            }
        }
    }

    private static async Task<string> ExtractFilesBeforeLaunch(string filePath, SystemConfig systemConfig, string gamePathToLaunch)
    {
        var fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

        switch (fileExtension)
        {
            case ".ZIP":
            {
                // Use a native .net library to extract
                // Only accept zip
                // Create Instance of ExtractCompressedFile
                var extractCompressedFile = new ExtractCompressedFile();
                var tempExtractLocation = await extractCompressedFile.ExtractGameToTempAsync2(filePath);

                var extractFilesBeforeLaunch = await ValidateAndFindGameFile(tempExtractLocation);
                if (extractFilesBeforeLaunch != null) return extractFilesBeforeLaunch;

                break;
            }
            case ".7Z" or ".RAR":
            {
                // Use 7z to extract
                // Can extract zip, 7z, rar
                // Create Instance of ExtractCompressedFile
                var extractCompressedFile = new ExtractCompressedFile();
                var tempExtractLocation = await extractCompressedFile.ExtractGameToTempAsync(filePath);

                var extractFilesBeforeLaunch = await ValidateAndFindGameFile(tempExtractLocation);
                if (extractFilesBeforeLaunch != null) return extractFilesBeforeLaunch;

                break;
            }
            default:
            {
                // Notify developer
                var contextMessage = $"Can not extract file: {filePath}";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CannotExtractThisFileMessageBox(filePath);

                return gamePathToLaunch;
            }
        }

        return gamePathToLaunch;

        Task<string> ValidateAndFindGameFile(string tempExtractLocation)
        {
            if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
            {
                // Notify developer
                var contextMessage = $"gameFile path is invalid: {tempExtractLocation}";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();

                return Task.FromResult(gamePathToLaunch);
            }

            if (systemConfig.FileFormatsToLaunch == null)
            {
                // Notify developer
                const string contextMessage = "FileFormatsToLaunch is null.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.NullFileExtensionMessageBox();

                return Task.FromResult(gamePathToLaunch);
            }

            // Iterate through the formats to launch and find the first file with the specified extension
            var fileFound = false;
            foreach (var files in systemConfig.FileFormatsToLaunch.Select(formatToLaunch => Directory.GetFiles(tempExtractLocation, $"*{formatToLaunch}")).Where(static files => files.Length > 0))
            {
                gamePathToLaunch = files[0];
                fileFound = true;
                break;
            }

            if (string.IsNullOrEmpty(gamePathToLaunch))
            {
                // Notify developer
                var contextMessage = $"gamePath is null or empty: {gamePathToLaunch}";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.CouldNotFindAFileMessageBox();

                return Task.FromResult(gamePathToLaunch);
            }

            if (fileFound) return Task.FromResult<string>(null);

            // Notify developer
            const string contextMessage2 = "Could not find a file with the extension defined in 'Extension to Launch After Extraction'.";
            var ex2 = new Exception(contextMessage2);
            _ = LogErrors.LogErrorAsync(ex2, contextMessage2);

            // Notify user
            MessageBoxLibrary.CouldNotFindAFileMessageBox();

            return Task.FromResult(gamePathToLaunch);
        }
    }

    private static Task CheckForExitCodeWithErrorAny(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemConfig.Emulator emulatorConfig)
    {
        if (process.ExitCode == 0 && !ContainsEmulatorError(output, error)) return Task.CompletedTask;

        // Notify developer
        var contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n" +
                             $"Calling parameters: {psi.Arguments}";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user only if he wants
        if (emulatorConfig?.ReceiveANotificationOnEmulatorError == true)
        {
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        }

        return Task.CompletedTask;
    }

    private static Task<bool> CheckForMemoryAccessViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemConfig.Emulator emulatorConfig)
    {
        if (process.ExitCode != -1073741819) return Task.FromResult(false);

        // Notify developer
        var contextMessage = $"There was an memory access violation error running the emulator. User was not notified.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n" +
                             $"Calling parameters: {psi.Arguments}";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user only if he wants
        if (emulatorConfig?.ReceiveANotificationOnEmulatorError == true)
        {
            MessageBoxLibrary.CheckForMemoryAccessViolation();
        }

        return Task.FromResult(true);
    }

    private static Task<bool> CheckForDepViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemConfig.Emulator emulatorConfig)
    {
        if (process.ExitCode != -1073740791) return Task.FromResult(false);

        // Notify developer
        var contextMessage = $"Data Execution Prevention (DEP) violation error occurred while running the emulator.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n" +
                             $"Calling parameters: {psi.Arguments}";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user only if he wants
        if (emulatorConfig?.ReceiveANotificationOnEmulatorError == true)
        {
            MessageBoxLibrary.DepViolationMessageBox();
        }

        return Task.FromResult(true);
    }

    private static Task<bool> CheckForTempExtractLocation(string tempExtractLocation)
    {
        if (!string.IsNullOrEmpty(tempExtractLocation) && Directory.Exists(tempExtractLocation)) return Task.FromResult(false);

        // Notify developer
        const string contextMessage = "Extraction failed.";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.ExtractionFailedMessageBox();

        return Task.FromResult(true);
    }

    private static Task<bool> CheckForWorkingDirectory(string workingDirectory)
    {
        if (!string.IsNullOrEmpty(workingDirectory)) return Task.FromResult(false);

        // Notify developer
        var contextMessage = $"workingDirectory is null or empty: {workingDirectory}";
        var ex = new Exception(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);

        // Notify user
        MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

        return Task.FromResult(true);
    }

    private static bool ContainsEmulatorError(StringBuilder output, StringBuilder error)
    {
        if (output == null && error == null) return false;

        // Common RetroArch error messages
        var errorMessages = new[]
        {
            "File open/read error"
        };

        var outputStr = output?.ToString() ?? string.Empty;
        var errorStr = error?.ToString() ?? string.Empty;

        return errorMessages.Any(errorMsg =>
            outputStr.Contains(errorMsg, StringComparison.OrdinalIgnoreCase) ||
            errorStr.Contains(errorMsg, StringComparison.OrdinalIgnoreCase));
    }
}
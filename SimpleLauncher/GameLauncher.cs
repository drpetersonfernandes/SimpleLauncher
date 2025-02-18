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
    
    public static async Task HandleButtonClick(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs, SettingsConfig settings, MainWindow mainWindow)
    {
        if (CheckFilepath()) return;
        
        if (CheckSystemComboBox()) return;

        if (CheckEmulatorComboBox()) return;
        
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
                    var selectedSystem = systemComboBox.SelectedItem?.ToString() ?? string.Empty;
                    if (selectedSystem.ToUpperInvariant().Contains("XBLA"))
                    {
                        await LaunchXblaGame(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    }
                    // ReSharper disable once PossibleNullReferenceException
                    else if (selectedSystem.ToLowerInvariant().Contains("aquarius") && emulatorComboBox.SelectedItem.ToString().ToLowerInvariant().Contains("mame"))
                    {
                        await LaunchMattelAquariusGame(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    }
                    // ReSharper disable once PossibleNullReferenceException
                    else if (emulatorComboBox.SelectedItem.ToString().ToLowerInvariant().Contains("fusion"))
                    {
                        await LaunchRegularEmulatorWithoutWarnings(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    }
                    // ReSharper disable once PossibleNullReferenceException
                    else if (emulatorComboBox.SelectedItem.ToString().ToLowerInvariant().Contains("mastergear"))
                    {
                        await LaunchRegularEmulatorWithoutWarnings(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    }
                    // ReSharper disable twice PossibleNullReferenceException
                    else if (emulatorComboBox.SelectedItem.ToString().ToLowerInvariant().Contains("project64") || emulatorComboBox.SelectedItem.ToString().ToLowerInvariant().Contains("project 64"))
                    {
                        await LaunchRegularEmulatorWithoutWarnings(filePath, emulatorComboBox, systemComboBox, systemConfigs);
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
            await LogErrors.LogErrorAsync(ex,
                $"Generic error in the GameLauncher class.\n\n" +
                $"Exception type: {ex.GetType().Name}\n" +
                $"Exception details: {ex.Message}\n" +
                $"FilePath: {filePath}\n" +
                $"SelectedSystem: {systemComboBox.SelectedItem}\n" +
                $"SelectedEmulator: {emulatorComboBox.SelectedItem}");

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
            
            // Get System Name
            var selectedSystem = systemComboBox.SelectedItem?.ToString() ?? string.Empty;
            
            settings.UpdateSystemPlayTime(selectedSystem, playTime); // Update the system playtime in settings
            settings.Save(); // Save the updated settings
            
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

        bool CheckFilepath()
        {
            if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath)) return false;
            
            // Notify developer
            var errorMessage = "Invalid filePath.";
            Exception ex = new();
            LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return true;

        }

        bool CheckSystemComboBox()
        {
            if (systemComboBox.SelectedItem != null) return false;
            
            // Notify developer
            var errorMessage = "Invalid system.";
            Exception ex = new();
            LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return true;
        }

        bool CheckEmulatorComboBox()
        {
            if (emulatorComboBox.SelectedItem != null) return false;
            
            // Notify developer
            var errorMessage = "Invalid emulator.";
            Exception ex = new();
            LogErrors.LogErrorAsync(ex, errorMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);

            return true;
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

        process.OutputDataReceived += (_, args) => {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) => {
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
                string errorMessage = $"There was an issue running the batch process. User was not notified.\n\n" +
                                      $"Batch file: {psi.FileName}\n" +
                                      $"Exit code {process.ExitCode}\n" +
                                      $"Output: {output}\n" +
                                      $"Error: {error}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                
                // Notify user
                // Ignore
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"There was an issue running the batch process. User was not notified.\n\n" +
                               $"Batch file: {psi.FileName}\n" +
                               $"Exit code {process.ExitCode}\n" +
                               $"Output: {output}\n" +
                               $"Error: {error}\n" +
                               $"Exception type: {ex.GetType().Name}\n" +
                               $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            // Notify user
            // Ignore
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
                var errorMessage = $"Error launching the shortcut file. User was not notified.\n\n" +
                                   $"Shortcut file: {psi.FileName}\n" +
                                   $"Exit code {process.ExitCode}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);

                // Notify user
                // Ignore
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorDetails = $"Error launching the shortcut file. User was not notified.\n\n" +
                               $"Shortcut file: {psi.FileName}\n" +
                               $"Exit code {process.ExitCode}\n" +
                               $"Exception type: {ex.GetType().Name}\n" +
                               $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorDetails);

            // Notify user
            // Ignore
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
                string errorMessage = $"Error launching the executable file. User was not notified.\n\n" +
                                      $"Executable file: {psi.FileName}\n" +
                                      $"Exit code {process.ExitCode}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);

                // Notify user
                // Ignore
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorDetails = $"Error launching the executable file. User was not notified.\n\n" +
                               $"Executable file: {psi.FileName}\n" +
                               $"Exit code {process.ExitCode}\n" +
                               $"Exception type: {ex.GetType().Name}\n" +
                               $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorDetails);

            // Notify user
            // Ignore
        }
    }

    private static async Task LaunchRegularEmulator(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        var selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        var selectedSystem = systemComboBox.SelectedItem.ToString();
        
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (await CheckSystemConfig(systemConfig)) return;

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (await CheckEmulatorConfig(emulatorConfig)) return;

        var gamePathToLaunch = filePath;

        // Extract File if Needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            gamePathToLaunch = await ExtractFilesBeforeLaunch(filePath, systemConfig, gamePathToLaunch);
        }
        
        // Check gamePath
        if (await CheckGamePathToLaunch(gamePathToLaunch)) return;

        // Construct the PSI
        var programLocation = emulatorConfig.EmulatorLocation;
        var parameters = emulatorConfig.EmulatorParameters;
        var arguments = $"{parameters} \"{gamePathToLaunch}\"";

        // Check programLocation before call it
        if (await CheckProgramLocation(programLocation)) return;

        var psi = new ProcessStartInfo
        {
            FileName = programLocation,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new Process { StartInfo = psi };
        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, args) => {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) => {
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

            if (await CheckForMemoryAccessViolation(process, psi, output, error)) return;
            
            await CheckForExitCodeWithErrorAny(process, psi, output, error);
        }
        
        catch (InvalidOperationException ex)
        {
            // Notify developer
            const string formattedException = "Invalid Operation Exception";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            MessageBoxLibrary.InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                     $"Exit code: {process.ExitCode}\n" +
                                     $"Emulator: {psi.FileName}\n" +
                                     $"Emulator output: {output}\n" +
                                     $"Emulator error: {error}\n" +
                                     $"Calling parameters: {psi.Arguments}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        }
    }

    private static async Task LaunchRegularEmulatorWithoutWarnings(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        var selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        var selectedSystem = systemComboBox.SelectedItem.ToString();

        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (await CheckSystemConfig(systemConfig)) return;

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (await CheckEmulatorConfig(emulatorConfig)) return;

        var gamePathToLaunch = filePath;

        // Extract File if Needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            gamePathToLaunch = await ExtractFilesBeforeLaunch(filePath, systemConfig, gamePathToLaunch);
        }
        
        // Check gamePath
        if (await CheckGamePathToLaunch(gamePathToLaunch)) return;

        // Construct the PSI
        var programLocation = emulatorConfig.EmulatorLocation;
        var parameters = emulatorConfig.EmulatorParameters;
        var arguments = $"{parameters} \"{gamePathToLaunch}\"";
        
        // Check programLocation before call it
        if (await CheckProgramLocation(programLocation)) return;

        var psi = new ProcessStartInfo
        {
            FileName = programLocation,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new Process { StartInfo = psi };
        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, args) => {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) => {
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
                throw new InvalidOperationException("The process has not exited as expected.");
            }
            
            if (await CheckForMemoryAccessViolation(process, psi, output, error)) return;
            
            if (await CheckForExitCodeWithError1WithoutUserNotification(process, psi, output, error)) return;

            await CheckForExitCodeWithErrorAnyWithoutUserNotification(process, psi, output, error);
        }
        
        catch (InvalidOperationException ex)
        {
            // Notify developer
            const string formattedException = "Invalid Operation Exception";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            // Ignore
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"The emulator could not open the game with the provided parameters. User was not notified.\n\n" +
                                     $"Exit code: {process.ExitCode}\n" +
                                     $"Emulator: {psi.FileName}\n" +
                                     $"Emulator output: {output}\n" +
                                     $"Emulator error: {error}\n" +
                                     $"Calling parameters: {psi.Arguments}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            // Ignore
        }
    }

    private static async Task LaunchXblaGame(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        var selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        var selectedSystem = systemComboBox.SelectedItem.ToString();
    
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (await CheckSystemConfig(systemConfig)) return;
    
        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (await CheckEmulatorConfig(emulatorConfig)) return;

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
        
        if (await CheckGamePathToLaunch(gamePathToLaunch)) return;
        
        // Construct the PSI
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
            
            if (await CheckForMemoryAccessViolation(process, psi, output, error)) return;

            await CheckForExitCodeWithErrorAny(process, psi, output, error);
        }
        catch (InvalidOperationException ex)
        {
            // Notify developer
            var formattedException = "Invalid Operation Exception";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                     $"Exit code: {process.ExitCode}\n" +
                                     $"Emulator: {psi.FileName}\n" +
                                     $"Emulator output: {output}\n" +
                                     $"Emulator error: {error}\n" +
                                     $"Calling parameters: {psi.Arguments}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            // Notify user
            MessageBoxLibrary.EmulatorCouldNotOpenXboxXblaSimpleMessageBox(LogPath);
        }
        
        Task<string> FindXblaGamePath(string rootFolderPath)
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
        if (await CheckSystemConfig(systemConfig)) return;

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (await CheckEmulatorConfig(emulatorConfig)) return;

        var gamePathToLaunch = filePath;

        // Extract File if Needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            gamePathToLaunch = await ExtractFilesBeforeLaunch(filePath, systemConfig, gamePathToLaunch);
        }
        
        if (await CheckGamePathToLaunch(gamePathToLaunch)) return;

        // Construct the PSI
        var programLocation = emulatorConfig.EmulatorLocation;
        var parameters = emulatorConfig.EmulatorParameters;
        var workingDirectory = Path.GetDirectoryName(programLocation);
        var gameFilenameWithoutExtension = Path.GetFileNameWithoutExtension(gamePathToLaunch);
        var arguments = $"{parameters} {gameFilenameWithoutExtension}";

        if (await CheckProgramLocation(programLocation)) return;

        // Check workingDirectory
        if (await CheckForWorkingDirectory(workingDirectory)) return;
        Debug.Assert(workingDirectory != null, nameof(workingDirectory) + " != null");
        
        var psi = new ProcessStartInfo
        {
            FileName = programLocation,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = new Process { StartInfo = psi };
        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, args) => {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) => {
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

            if (await CheckForMemoryAccessViolation(process, psi, output, error)) return;
            
            await CheckForExitCodeWithErrorAny(process, psi, output, error);
        }
        
        catch (InvalidOperationException ex)
        {
            // Notify developer
            var formattedException = $"Invalid Operation Exception.";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            MessageBoxLibrary.InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                     $"Exit code: {process.ExitCode}\n" +
                                     $"Emulator: {psi.FileName}\n" +
                                     $"Emulator output: {output}\n" +
                                     $"Emulator error: {error}\n" +
                                     $"Calling parameters: {psi.Arguments}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        }
    }

    private static async Task<string> ExtractFilesBeforeLaunch(string filePath, SystemConfig systemConfig, string gamePathToLaunch)
    {
        var fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

        if (fileExtension == ".ZIP")
        {
            // Use a native .net library to extract
            // Only accept zip
            // Create Instance of ExtractCompressedFile
            var extractCompressedFile = new ExtractCompressedFile();
            var tempExtractLocation = await extractCompressedFile.ExtractGameToTempAsync2(filePath);

            var extractFilesBeforeLaunch = await ValidateAndFindGameFile(tempExtractLocation);
            if (extractFilesBeforeLaunch != null) return extractFilesBeforeLaunch;
        }
        else if (fileExtension is ".7Z" or ".RAR")
        {
            // Use 7z to extract
            // Can extract zip, 7z, rar
            // Create Instance of ExtractCompressedFile
            var extractCompressedFile = new ExtractCompressedFile();
            var tempExtractLocation = await extractCompressedFile.ExtractGameToTempAsync(filePath);
                
            var extractFilesBeforeLaunch = await ValidateAndFindGameFile(tempExtractLocation);
            if (extractFilesBeforeLaunch != null) return extractFilesBeforeLaunch;
        }else
        {
            // Notify developer
            var formattedException = $"Can not extract file: {filePath}";
            Exception ex = new(formattedException);
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            MessageBoxLibrary.CannotExtractThisFileMessageBox(filePath);
            
            return gamePathToLaunch;
        }
        return gamePathToLaunch;

        async Task<string> ValidateAndFindGameFile(string tempExtractLocation)
        {
            if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
            {
                // Notify developer
                var formattedException = $"gameFile path is invalid: {tempExtractLocation}";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                // Notify user
                MessageBoxLibrary.ExtractionFailedMessageBox();
                
                return gamePathToLaunch;
            }
                
            if (systemConfig.FileFormatsToLaunch == null)
            {
                // Notify developer
                var formattedException = "FileFormatsToLaunch is null.";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                // Notify user
                MessageBoxLibrary.NullFileExtensionMessageBox();

                return gamePathToLaunch;
            }
                
            // Iterate through the formats to launch and find the first file with the specified extension
            var fileFound = false;
            foreach (var files in systemConfig.FileFormatsToLaunch.Select(formatToLaunch => Directory.GetFiles(tempExtractLocation, $"*{formatToLaunch}")).Where(files => files.Length > 0))
            {
                gamePathToLaunch = files[0];
                fileFound = true;
                break;
            }
                
            if (string.IsNullOrEmpty(gamePathToLaunch))
            {
                // Notify developer
                var formattedException = $"gamePath is null or empty: {gamePathToLaunch}";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                // Notify user
                MessageBoxLibrary.CouldNotFindAFileMessageBox();

                return gamePathToLaunch;
            }

            if (fileFound) return null;
            
            // Notify developer
            var errorMessage = "Could not find a file with the extension defined in 'Extension to Launch After Extraction'.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            MessageBoxLibrary.CouldNotFindAFileMessageBox();

            return gamePathToLaunch;
        }
    }
    
    private static async Task CheckForExitCodeWithErrorAny(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (process.ExitCode != 0)
        {
            // Notify developer
            var errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                               $"Exit code: {process.ExitCode}\n" +
                               $"Emulator: {psi.FileName}\n" +
                               $"Emulator output: {output}\n" +
                               $"Emulator error: {error}\n" +
                               $"Calling parameters: {psi.Arguments}";
            Exception ex = new(errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.CouldNotLaunchGameMessageBox(LogPath);
        }
    }

    private static async Task<bool> CheckForMemoryAccessViolation(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (process.ExitCode != -1073741819) return false;
        
        // Notify developer
        string errorMessage = $"There was an access violation error running the emulator. User was not notified.\n\n" +
                              $"Exit code: {process.ExitCode}\n" +
                              $"Emulator: {psi.FileName}\n" +
                              $"Emulator output: {output}\n" +
                              $"Emulator error: {error}\n" +
                              $"Calling parameters: {psi.Arguments}";
        Exception ex = new(errorMessage);
        await LogErrors.LogErrorAsync(ex, errorMessage);

        // Notify user
        // Ignore

        return true;

    }

    private static async Task<bool> CheckProgramLocation(string programLocation)
    {
        if (!string.IsNullOrWhiteSpace(programLocation) && File.Exists(programLocation)) return false;
        
        // Notify developer
        var errorMessage = $"Invalid programLocation: {programLocation}";
        Exception exception = new(errorMessage);
        await LogErrors.LogErrorAsync(exception, errorMessage);

        // Notify user
        MessageBoxLibrary.InvalidProgramLocationMessageBox(programLocation);
            
        return true;

    }

    private static async Task<bool> CheckGamePathToLaunch(string gamePathToLaunch)
    {
        if (!string.IsNullOrEmpty(gamePathToLaunch) && File.Exists(gamePathToLaunch)) return false;
       
        // Notify developer
        var errorMessage = $"Invalid GamePath: {gamePathToLaunch}";
        Exception exception = new(errorMessage);
        await LogErrors.LogErrorAsync(exception, errorMessage);
            
        // Notify user
        MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            
        return true;
    }

    private static async Task<bool> CheckEmulatorConfig(SystemConfig.Emulator emulatorConfig)
    {
        if (emulatorConfig != null) return false;
        
        // Notify developer
        string errorMessage = $"emulatorConfig is null.";
        Exception exception = new(errorMessage);
        await LogErrors.LogErrorAsync(exception, errorMessage);

        // Notify user
        MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);

        return true;

    }

    private static async Task<bool> CheckSystemConfig(SystemConfig systemConfig)
    {
        if (systemConfig != null) return false;
        
        // Notify developer
        var errorMessage = "systemConfig is null.";
        Exception exception = new(errorMessage);
        await LogErrors.LogErrorAsync(exception, errorMessage);

        // Notify user
        MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            
        return true;
    }
    
    private static async Task CheckForExitCodeWithErrorAnyWithoutUserNotification(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (process.ExitCode != 0)
        {
            // Notify developer
            var errorMessage = $"Emulator error. User was not notified.\n\n" +
                               $"Exit code: {process.ExitCode}\n" +
                               $"Emulator: {psi.FileName}\n" +
                               $"Emulator output: {output}\n" +
                               $"Emulator error: {error}\n" +
                               $"Calling parameters: {psi.Arguments}";
            Exception ex = new(errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            // Ignore
        }
    }

    private static async Task<bool> CheckForExitCodeWithError1WithoutUserNotification(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (process.ExitCode != 1) return false;
        
        // Notify developer
        var errorMessage = $"Generic error in the emulator. User was not notified.\n\n" +
                           $"Exit code: {process.ExitCode}\n" +
                           $"Emulator: {psi.FileName}\n" +
                           $"Emulator output: {output}\n" +
                           $"Emulator error: {error}\n" +
                           $"Calling parameters: {psi.Arguments}";
        Exception ex = new(errorMessage);
        await LogErrors.LogErrorAsync(ex, errorMessage);

        // Notify user
        // Ignore
                
        return true;
    }
    
    private static async Task<bool> CheckForTempExtractLocation(string tempExtractLocation)
    {
        if (!string.IsNullOrEmpty(tempExtractLocation) && Directory.Exists(tempExtractLocation)) return false;
        
        // Notify developer
        const string errorMessage = "Extraction failed.";
        Exception exception = new(errorMessage);
        await LogErrors.LogErrorAsync(exception, errorMessage);

        // Notify user
        MessageBoxLibrary.ExtractionFailedMessageBox();

        return true;
    }
    
    private static async Task<bool> CheckForWorkingDirectory(string workingDirectory)
    {
        if (!string.IsNullOrEmpty(workingDirectory)) return false;
        
        // Notify developer
        string errorMessage = $"workingDirectory is null or empty: {workingDirectory}";
        Exception exception = new(errorMessage);
        await LogErrors.LogErrorAsync(exception, errorMessage);

        // Notify user
        MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(LogPath);
            
        return true;
    }
}
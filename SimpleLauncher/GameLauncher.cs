using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher;

public static class GameLauncher
{
    static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");
    
    public static async Task HandleButtonClick(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs, SettingsConfig settings, MainWindow mainWindow)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            // Notify developer
            string errorMessage = "Invalid filePath.";
            Exception ex = new();
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            CouldNotLaunchGameMessageBox();

            return;
        }
        
        if (systemComboBox.SelectedItem == null)
        {
            // Notify developer
            string errorMessage = "Invalid system.";
            Exception ex = new();
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            CouldNotLaunchGameMessageBox();

            return;
        }

        if (emulatorComboBox.SelectedItem == null)
        {
            // Notify developer
            string errorMessage = "Invalid emulator.";
            Exception ex = new();
            Task logTask = LogErrors.LogErrorAsync(ex, errorMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            // Notify user
            CouldNotLaunchGameMessageBox();

            return;
        }
        
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Copy the file name to the clipboard
        Clipboard.SetText(fileName);
        
        // Stop the GamePadController if it is running
        // To prevent interference with third party programs, like emulators or games
        bool wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;
        if (wasGamePadControllerRunning)
        {
            GamePadController.Instance2.Stop();
        }
        
        // Start tracking the time when the game is launched
        // To track system playtime
        DateTime startTime = DateTime.Now;
        
        try
        {
            string fileExtension = Path.GetExtension(filePath).ToUpperInvariant();
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
                    string selectedSystem = systemComboBox.SelectedItem?.ToString() ?? string.Empty;
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
            CouldNotLaunchGameMessageBox();
        }
        finally
        {
            if (wasGamePadControllerRunning)
            {
                GamePadController.Instance2.Start();
            }
            
            DateTime endTime = DateTime.Now; // Capture the time when the game exits 
            TimeSpan playTime = endTime - startTime; // Calculate the playtime
            
            // Get System Name
            string selectedSystem = systemComboBox.SelectedItem?.ToString() ?? string.Empty;
            
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
                _ = Stats.CallApiAsync(emulatorComboBox.SelectedItem.ToString().ToLowerInvariant());
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
            bool processStarted = process.Start();
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
            string errorMessage = $"There was an issue running the batch process. User was not notified.\n\n" +
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
            bool processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.");
            }
            await process.WaitForExitAsync();
           
            if (process.ExitCode != 0)
            {
                // Notify developer
                string errorMessage = $"Error launching the shortcut file. User was not notified.\n\n" +
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
            string errorDetails = $"Error launching the shortcut file. User was not notified.\n\n" +
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
            bool processStarted = process.Start();
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
            string errorDetails = $"Error launching the executable file. User was not notified.\n\n" +
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
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();
        
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (systemConfig == null)
        {
            // Notify developer
            string errorMessage = "systemConfig is null.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (emulatorConfig == null)
        {
            // Notify developer
            string errorMessage = $"emulatorConfig is null.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();

            return;
        }

        string gamePathToLaunch = filePath;

        // Extract File if Needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            gamePathToLaunch = await ExtractFilesBeforeLaunch(filePath, systemConfig, gamePathToLaunch);
        }
        
        // Check gamePath
        if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
        {
            // Notify developer
            string errorMessage = $"Invalid GamePath: {gamePathToLaunch}";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

        // Construct the PSI
        string programLocation = emulatorConfig.EmulatorLocation;
        string parameters = emulatorConfig.EmulatorParameters;
        string arguments = $"{parameters} \"{gamePathToLaunch}\"";

        // Check programLocation before call it
        if (string.IsNullOrWhiteSpace(programLocation) || !File.Exists(programLocation))
        {
            // Notify developer
            string errorMessage = $"Invalid programLocation: {programLocation}";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            InvalidProgramLocationMessageBox();
            
            return;
        }

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
            bool processStarted = process.Start();

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

            // Memory Access Violation error
            if (process.ExitCode == -1073741819)
            {
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

                return;
            }
            
            if (process.ExitCode != 0)
            {
                // Notify developer
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                CouldNotLaunchGameMessageBox();
            }
        }
        
        catch (InvalidOperationException ex)
        {
            // Notify developer
            string formattedException = "Invalid Operation Exception";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                        $"Exit code: {process.ExitCode}\n" +
                                        $"Emulator: {psi.FileName}\n" +
                                        $"Emulator output: {output}\n" +
                                        $"Emulator error: {error}\n" +
                                        $"Calling parameters: {psi.Arguments}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            CouldNotLaunchGameMessageBox();
        }
    }

    private static async Task LaunchRegularEmulatorWithoutWarnings(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();

        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (systemConfig == null)
        {
            // Notify developer
            string errorMessage = $"systemConfig is null.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (emulatorConfig == null)
        {
            // Notify developer
            string errorMessage = $"emulatorConfig is null.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

        string gamePathToLaunch = filePath;

        // Extract File if Needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            gamePathToLaunch = await ExtractFilesBeforeLaunch(filePath, systemConfig, gamePathToLaunch);
        }
        
        // Check gamePath
        if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
        {
            // Notify developer
            string errorMessage = $"Invalid GamePath: {gamePathToLaunch}";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

        // Construct the PSI
        string programLocation = emulatorConfig.EmulatorLocation;
        string parameters = emulatorConfig.EmulatorParameters;
        string arguments = $"{parameters} \"{gamePathToLaunch}\"";
        
        // Check programLocation before call it
        if (string.IsNullOrWhiteSpace(programLocation) || !File.Exists(programLocation))
        {
            // Notify developer
            string errorMessage = $"Invalid programLocation: {programLocation}";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

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
            bool processStarted = process.Start();
            
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
            
            if (process.ExitCode == -1073741819)
            {
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
                
                return;
            }
            
            if (process.ExitCode == 1)
            {
                // Notify developer
                string errorMessage = $"Generic error in the emulator. User was not notified.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                // Ignore
                
                return;
            }

            if (process.ExitCode != 0)
            {
                // Notify developer
                string errorMessage = $"Emulator error. User was not notified.\n\n" +
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
        
        catch (InvalidOperationException ex)
        {
            // Notify developer
            string formattedException = $"Invalid Operation Exception";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            // Ignore
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"The emulator could not open the game with the provided parameters. User was not notified.\n\n" +
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
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();
    
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (systemConfig == null)
        {
            // Notify developer
            string errorMessage = "systemConfig is null.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();

            return;
        }
    
        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        if (emulatorConfig == null)
        {
            // Notify developer
            string errorMessage = $"emulatorConfig is null.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

        string gamePathToLaunch = null;

        // Force extraction of the compressed file even if the config is wrongly set to false
        systemConfig.ExtractFileBeforeLaunch = true;

        // Check if extraction is needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            string fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

            // Accept ZIP, 7Z and RAR files
            if (fileExtension == ".ZIP" || fileExtension == ".7Z" || fileExtension == ".RAR")
            {
                // Create Instance of ExtractCompressedFile
                ExtractCompressedFile extractCompressedFile = new ExtractCompressedFile();
                string tempExtractLocation = await extractCompressedFile.ExtractGameToTempAsync(filePath);
                
                if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
                {
                    // Notify developer
                    string errorMessage = $"Extraction failed.";
                    Exception exception = new(errorMessage);
                    await LogErrors.LogErrorAsync(exception, errorMessage);

                    // Notify user
                    ExtractionFailedMessageBox();

                    return;
                }
                
                gamePathToLaunch = await FindXblaGamePath(tempExtractLocation); // Search within the extracted folder
            }
            else
            {
                CannotExtractThisFileMessageBox(filePath);

                return;
            }
        }
        
        if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
        {
            // Notify developer
            string errorMessage = $"gamePath is null or empty: {gamePathToLaunch}";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();

            return;
        }
        
        // Construct the PSI
        string programLocation = emulatorConfig.EmulatorLocation;
        string parameters = emulatorConfig.EmulatorParameters;
        string arguments = $"{parameters} \"{gamePathToLaunch}\"";

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
            bool processStarted = process.Start();

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
            
            // Memory Access Violation error
            if (process.ExitCode == -1073741819)
            {
                // Notify developer
                string errorMessage = $"There was an memory access violation error running this emulator with this ROM. User was not notified.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                // Ignore
                
                return;
            }

            // Other errors
            if (process.ExitCode != 0)
            {
                // Notify developer
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                EmulatorCouldNotOpenXboxXblaSimpleMessageBox();
            }
            
        }
        catch (InvalidOperationException ex)
        {
            // Notify developer
            string formattedException = "Invalid Operation Exception";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                        $"Exit code: {process.ExitCode}\n" +
                                        $"Emulator: {psi.FileName}\n" +
                                        $"Emulator output: {output}\n" +
                                        $"Emulator error: {error}\n" +
                                        $"Calling parameters: {psi.Arguments}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            // Notify user
            EmulatorCouldNotOpenXboxXblaSimpleMessageBox();
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
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (systemConfig == null)
        {
            // Notify developer
            string errorMessage = "systemConfig is null.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (emulatorConfig == null)
        {
            // Notify developer
            string errorMessage = "emulatorConfig is null.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

        string gamePathToLaunch = filePath;

        // Extract File if Needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            gamePathToLaunch = await ExtractFilesBeforeLaunch(filePath, systemConfig, gamePathToLaunch);
        }
        
        // Check gamePath
        if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
        {
            // Notify developer
            string errorMessage = $"gamePath is null or empty: '{gamePathToLaunch}'.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();

            return;
        }

        // Construct the PSI
        string programLocation = emulatorConfig.EmulatorLocation;
        string parameters = emulatorConfig.EmulatorParameters;
        string workingDirectory = Path.GetDirectoryName(programLocation);
        string gameFilenameWithoutExtension = Path.GetFileNameWithoutExtension(gamePathToLaunch);
        string arguments = $"{parameters} {gameFilenameWithoutExtension}";

        // Check programLocation
        if (string.IsNullOrWhiteSpace(programLocation) || !File.Exists(programLocation))
        {
            // Notify developer
            string errorMessage = $"programLocation is null or empty: {programLocation}";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            InvalidProgramLocationMessageBox();
            
            return;
        }

        // Check workingDirectory
        if (string.IsNullOrEmpty(workingDirectory))
        {
            // Notify developer
            string errorMessage = $"workingDirectory is null or empty: {workingDirectory}";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            // Notify user
            ThereWasAnErrorLaunchingThisGameMessageBox();
            
            return;
        }

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
            bool processStarted = process.Start();
            
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

            // Memory Access Violation error
            if (process.ExitCode == -1073741819)
            {
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
                
                return;
            }
            
            if (process.ExitCode != 0)
            {
                // Notify developer
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                CouldNotLaunchGameMessageBox();
            }
        }
        
        catch (InvalidOperationException ex)
        {
            // Notify developer
            string formattedException = $"Invalid Operation Exception.";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                        $"Exit code: {process.ExitCode}\n" +
                                        $"Emulator: {psi.FileName}\n" +
                                        $"Emulator output: {output}\n" +
                                        $"Emulator error: {error}\n" +
                                        $"Calling parameters: {psi.Arguments}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            CouldNotLaunchGameMessageBox();
        }
    }
    
    private static async Task<string> ExtractFilesBeforeLaunch(string filePath, SystemConfig systemConfig, string gamePathToLaunch)
    {
        string fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

        if (fileExtension == ".ZIP")
        {
            // Use a native .net library to extract
            // Only accept zip
            // Create Instance of ExtractCompressedFile
            ExtractCompressedFile extractCompressedFile = new ExtractCompressedFile();
            string tempExtractLocation = await extractCompressedFile.ExtractGameToTempAsync2(filePath);

            var extractFilesBeforeLaunch = await ValidateAndFindGameFile(tempExtractLocation);
            if (extractFilesBeforeLaunch != null) return extractFilesBeforeLaunch;
        }
        else if (fileExtension == ".7Z" || fileExtension == ".RAR")
        {
            // Use 7z to extract
            // Can extract zip, 7z, rar
            // Create Instance of ExtractCompressedFile
            ExtractCompressedFile extractCompressedFile = new ExtractCompressedFile();
            string tempExtractLocation = await extractCompressedFile.ExtractGameToTempAsync(filePath);
                
            var extractFilesBeforeLaunch = await ValidateAndFindGameFile(tempExtractLocation);
            if (extractFilesBeforeLaunch != null) return extractFilesBeforeLaunch;
        }else
        {
            // Notify developer
            string formattedException = $"Can not extract file: {filePath}";
            Exception ex = new(formattedException);
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            // Notify user
            CannotExtractThisFileMessageBox(filePath);
            
            return gamePathToLaunch;
        }
        return gamePathToLaunch;

        async Task<string> ValidateAndFindGameFile(string tempExtractLocation)
        {
            if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
            {
                // Notify developer
                string formattedException = $"gameFile path is invalid: {tempExtractLocation}";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                // Notify user
                ExtractionFailedMessageBox();
                
                return gamePathToLaunch;
            }
                
            if (systemConfig.FileFormatsToLaunch == null)
            {
                // Notify developer
                string formattedException = "FileFormatsToLaunch is null.";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                // Notify user
                NullFileExtensionMessageBox();

                return gamePathToLaunch;
            }
                
            // Iterate through the formats to launch and find the first file with the specified extension
            bool fileFound = false;
            foreach (string formatToLaunch in systemConfig.FileFormatsToLaunch)
            {
                string[] files = Directory.GetFiles(tempExtractLocation, $"*{formatToLaunch}");

                if (files.Length > 0)
                {
                    gamePathToLaunch = files[0];
                    fileFound = true;
                    break;
                }
            }
                
            if (string.IsNullOrEmpty(gamePathToLaunch))
            {
                // Notify developer
                string formattedException = $"gamePath is null or empty: {gamePathToLaunch}";
                Exception ex = new(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                // Notify user
                CouldNotFindAFileMessageBox();

                return gamePathToLaunch;
            }

            if (!fileFound)
            {
                // Notify developer
                string errorMessage = "Could not find a file with the extension defined in 'Extension to Launch After Extraction'.";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);

                // Notify user
                CouldNotFindAFileMessageBox();

                return gamePathToLaunch;
            }
            return null;
        }
    }
    
    private static void CouldNotLaunchGameMessageBox()
    {
        string simpleLaunchercouldnotlaunch2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
        string ifyouaretryingtorunMamEensurethatyourRom2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunMAMEensurethatyourROM") ?? "If you are trying to run MAME, ensure that your ROM collection is compatible with the MAME version you are using.";
        string ifyouaretryingtorunRetroarchensurethattheBios2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarchensurethattheBIOS") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core are installed.";
        string alsomakesureyouarecallingtheemulator2 = (string)Application.Current.TryFindResource("Alsomakesureyouarecallingtheemulator") ?? "Also, make sure you are calling the emulator with the correct parameter.";
        string doyouwanttoopenthefile2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{simpleLaunchercouldnotlaunch2}\n\n" +
            $"{ifyouaretryingtorunMamEensurethatyourRom2}\n\n" +
            $"{ifyouaretryingtorunRetroarchensurethattheBios2}\n\n" +
            $"{alsomakesureyouarecallingtheemulator2}\n\n" +
            $"{doyouwanttoopenthefile2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                string thefileerroruserlogwas2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwas2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private static void InvalidOperationExceptionMessageBox()
    {
        string failedtostarttheemulator2 = (string)Application.Current.TryFindResource("Failedtostarttheemulator") ?? "Failed to start the emulator or it has not exited as expected.";
        string thistypeoferrorhappenswhenSimpleLauncher2 = (string)Application.Current.TryFindResource("ThistypeoferrorhappenswhenSimpleLauncher") ?? "This type of error happens when 'Simple Launcher' does not have the privileges to launch an external program, such as an emulator.";
        string grantSimpleLauncheradministrativeaccess2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrativeaccess") ?? "Grant 'Simple Launcher' administrative access.";
        string alsochecktheintegrityoftheemulator2 = (string)Application.Current.TryFindResource("Alsochecktheintegrityoftheemulator") ?? "Also, check the integrity of the emulator and its dependencies.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtostarttheemulator2}\n\n" +
                        $"{thistypeoferrorhappenswhenSimpleLauncher2}\n\n" +
                        $"{grantSimpleLauncheradministrativeaccess2}\n\n" +
                        $"{alsochecktheintegrityoftheemulator2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    private static void ThereWasAnErrorLaunchingThisGameMessageBox()
    {
        string therewasanerrorlaunchingthisgame2 = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingthisgame") ?? "There was an error launching this game.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string doyouwanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{therewasanerrorlaunchingthisgame2}\n\n" +
                                     $"{theerrorwasreportedtothedeveloper2}\n\n" +
                                     $"{doyouwanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                string thefileerroruserlog2 = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
   
    private static void CannotExtractThisFileMessageBox(string filePath)
    {
        string theselectedfile2 = (string)Application.Current.TryFindResource("Theselectedfile") ?? "The selected file";
        string cannotbeextracted2 = (string)Application.Current.TryFindResource("cannotbeextracted") ?? "can not be extracted.";
        string toextractafileitneedstobe2 = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ?? "To extract a file, it needs to be a 7z, zip, or rar file.";
        string pleasegotoEditSystem2 = (string)Application.Current.TryFindResource("PleasegotoEditSystem") ?? "Please go to Edit System - Expert Mode and edit this system.";
        string invalidFile2 = (string)Application.Current.TryFindResource("InvalidFile") ?? "Invalid File";
        MessageBox.Show($"{theselectedfile2} '{filePath}' {cannotbeextracted2}\n\n" +
                        $"{toextractafileitneedstobe2}\n\n" +
                        $"{pleasegotoEditSystem2}", 
            invalidFile2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    private static void InvalidProgramLocationMessageBox()
    {
        string invalidemulatorexecutablepath2 = (string)Application.Current.TryFindResource("Invalidemulatorexecutablepath") ?? "Invalid emulator executable path. Please check the configuration.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show(invalidemulatorexecutablepath2, error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    private static void ExtractionFailedMessageBox()
    {
        string extractionfailedCouldnotfindthetemporary2 = (string)Application.Current.TryFindResource("ExtractionfailedCouldnotfindthetemporary") ?? "Extraction failed. Could not find the temporary extract folder.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show(extractionfailedCouldnotfindthetemporary2, 
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    private static void EmulatorCouldNotOpenXboxXblaSimpleMessageBox()
    {
        string theemulatorcouldnotopenthegame2 = (string)Application.Current.TryFindResource("Theemulatorcouldnotopenthegame") ?? "The emulator could not open the game with the provided parameters.";
        string doyouwanttoopenthefileerror2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{theemulatorcouldnotopenthegame2}\n\n" +
            $"{doyouwanttoopenthefileerror2}", error2, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                string thefileerroruser2 = (string)Application.Current.TryFindResource("Thefileerroruser") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruser2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private static void NullFileExtensionMessageBox()
    {
        string thereisnoExtension2 = (string)Application.Current.TryFindResource("ThereisnoExtension") ?? "There is no 'Extension to Launch After Extraction' set in the system configuration.";
        string pleaseeditthissystemto2 = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{thereisnoExtension2}\n\n" +
                        $"{pleaseeditthissystemto2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    private static void CouldNotFindAFileMessageBox()
    {
        string couldnotfindafilewiththeextensiondefined2 = (string)Application.Current.TryFindResource("Couldnotfindafilewiththeextensiondefined") ?? "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
        string pleaseeditthissystemtofix2 = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{couldnotfindafilewiththeextensiondefined2}\n\n" +
                        $"{pleaseeditthissystemtofix2}", error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
}
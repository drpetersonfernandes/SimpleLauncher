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
            MessageBox.Show("Invalid filePath.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            string contextMessage = "Invalid filePath.\n\n" +
                                    "Method: HandleButtonClick";
            Exception ex = new();
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            return;
        }
        
        if (systemComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a system before launching the game.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            
            string contextMessage = "Invalid system.\n\n" +
                                    "Method: HandleButtonClick";
            Exception ex = new();
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            return;
        }

        if (emulatorComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select an emulator before launching the game.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            
            string contextMessage = "Invalid emulator.\n\n" +
                                    "Method: HandleButtonClick";
            Exception ex = new();
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            return;
        }
        
        // Get the file name from the filePath
        string fileName = Path.GetFileNameWithoutExtension(filePath);

        // Copy the file name to the clipboard
        Clipboard.SetText(fileName);
        
        // Stop the GamePadController if it is running
        bool wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;
        if (wasGamePadControllerRunning)
        {
            GamePadController.Instance2.Stop();
        }
        
        // Start tracking the time when the game is launched
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
                    else
                    {
                        await LaunchRegularEmulator(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            await LogErrors.LogErrorAsync(ex,
                $"Generic error in the GameLauncher class.\n\n" +
                $"Exception type: {ex.GetType().Name}\n" +
                $"Exception details: {ex.Message}\n" +
                $"FilePath: {filePath}\n" +
                $"SelectedSystem: {systemComboBox.SelectedItem}\n" +
                $"SelectedEmulator: {emulatorComboBox.SelectedItem}");

            CouldNotLaunchGameMessageBox();
        }
        finally
        {
            if (wasGamePadControllerRunning)
            {
                GamePadController.Instance2.Start();
            }
            
            // Capture the time when the game exits
            DateTime endTime = DateTime.Now; 
            // Calculate the playtime
            TimeSpan playTime = endTime - startTime; 
            // Get System Name
            string selectedSystem = systemComboBox.SelectedItem?.ToString() ?? string.Empty;
            // Update the system playtime in settings
            settings.UpdateSystemPlayTime(selectedSystem, playTime);
            // Save the updated settings
            settings.Save(); 
            // Update the PlayTime property in the MainWindow to refresh the UI
            var systemPlayTime = settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystem);
            if (systemPlayTime != null)
            {
                mainWindow.PlayTime = systemPlayTime.PlayTime; // Update PlayTime property in MainWindow
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
                string errorMessage = $"There was an issue running the batch process. User was not notified.\n\n" +
                                      $"Batch file: {psi.FileName}\n" +
                                      $"Exit code {process.ExitCode}\n" +
                                      $"Output: {output}\n" +
                                      $"Error: {error}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"There was an issue running the batch process. User was not notified.\n\n" +
                                  $"Batch file: {psi.FileName}\n" +
                                  $"Exit code {process.ExitCode}\n" +
                                  $"Output: {output}\n" +
                                  $"Error: {error}\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
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
                string errorMessage = $"Error launching the shortcut file.\n\n" +
                                      $"Shortcut file: {psi.FileName}\n" +
                                      $"Exit code {process.ExitCode}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                CouldNotLaunchShortcut();
            }
        }
        catch (Exception ex)
        {
            string errorDetails = $"Error launching the shortcut file.\n\n" +
                                  $"Shortcut file: {psi.FileName}\n" +
                                  $"Exit code {process.ExitCode}\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorDetails);
                        
            CouldNotLaunchShortcut();
        }

        void CouldNotLaunchShortcut()
        {
            var result = MessageBox.Show("There was an error launching the shortcut file.\n\n" +
                                         "Try to run the shortcut file outside 'Simple Launcher' to see if it is working properly.\n\n" +
                                         "Do you want to open the file 'error_user.log' to debug the error?",
                "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

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
                    MessageBox.Show("The file 'error_user.log' was not found!",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                string errorMessage = $"Error launching the executable file.\n\n" +
                                      $"Executable file: {psi.FileName}\n" +
                                      $"Exit code {process.ExitCode}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                CouldNotLaunchExe();
            }
        }
        catch (Exception ex)
        {
            string errorDetails = $"Error launching the executable file.\n\n" +
                                  $"Executable file: {psi.FileName}\n" +
                                  $"Exit code {process.ExitCode}\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorDetails);
                        
            CouldNotLaunchExe();
        }

        void CouldNotLaunchExe()
        {
            var result = MessageBox.Show("There was an error launching the executable file.\n\n" +
                                         "Try to run the executable file outside 'Simple Launcher' to see if it is working properly.\n\n" +
                                         "Do you want to open the file 'error_user.log' to debug the error?",
                "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

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
                    MessageBox.Show("The file 'error_user.log' was not found!",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private static async Task LaunchRegularEmulator(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();
        
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (systemConfig == null)
        {
            string errorMessage = $"systemConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchRegularEmulator";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            CouldNotLaunchEmulatorMessageBox();
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchRegularEmulator";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            CouldNotLaunchEmulatorMessageBox();
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
            string errorMessage = $"Invalid game path: {gamePathToLaunch}. Cannot launch the game.";
            Exception ex = new ArgumentNullException(nameof(gamePathToLaunch), errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Construct the PSI
        string programLocation = emulatorConfig.EmulatorLocation;
        string parameters = emulatorConfig.EmulatorParameters;
        string arguments = $"{parameters} \"{gamePathToLaunch}\"";

        // Check programLocation before call it
        if (string.IsNullOrWhiteSpace(programLocation) || !File.Exists(programLocation))
        {
            MessageBox.Show("Invalid emulator executable path. Please check the configuration.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                throw new InvalidOperationException("Failed to start the process.\n" +
                                                    "Method: LaunchRegularEmulator");
            }
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            
            if (!process.HasExited)
            {
                throw new InvalidOperationException("The process has not exited as expected.\n" +
                                                    "Method: LaunchRegularEmulator");
            }

            // Memory Access Violation error
            if (process.ExitCode == -1073741819)
            {
                string errorMessage = $"There was an access violation error running the emulator.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                // MemoryAccessViolationErrorMessageBox();
                return;
            }
            
            if (process.ExitCode != 0)
            {
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                CouldNotLaunchGameMessageBox();
            }
        }
        
        catch (InvalidOperationException ex)
        {
            string formattedException = $"InvalidOperationException in the method LaunchRegularEmulator";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            string formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                        $"Exit code: {process.ExitCode}\n" +
                                        $"Emulator: {psi.FileName}\n" +
                                        $"Emulator output: {output}\n" +
                                        $"Emulator error: {error}\n" +
                                        $"Calling parameters: {psi.Arguments}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            EmulatorCouldNotOpenWithProvidedParametersMessageBox();
        }
    }

    private static async Task LaunchRegularEmulatorWithoutWarnings(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();

        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (systemConfig == null)
        {
            string errorMessage = $"systemConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchRegularEmulatorWithoutWarnings";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            CouldNotLaunchEmulatorMessageBox();
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchRegularEmulatorWithoutWarnings";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            CouldNotLaunchEmulatorMessageBox();
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
            string errorMessage = $"Invalid game path: {gamePathToLaunch}. Cannot launch the game.";
            Exception ex = new ArgumentNullException(nameof(gamePathToLaunch), errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Construct the PSI
        string programLocation = emulatorConfig.EmulatorLocation;
        string parameters = emulatorConfig.EmulatorParameters;
        string arguments = $"{parameters} \"{gamePathToLaunch}\"";
        
        // Check programLocation before call it
        if (string.IsNullOrWhiteSpace(programLocation) || !File.Exists(programLocation))
        {
            MessageBox.Show("Invalid emulator executable path. Please check the configuration.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                throw new InvalidOperationException("Failed to start the process.\n" +
                                                    "Method: LaunchRegularEmulatorWithoutWarnings");
            }
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            
            if (!process.HasExited)
            {
                throw new InvalidOperationException("The process has not exited as expected.\n" +
                                                    "Method: LaunchRegularEmulatorWithoutWarnings");
            }
            
            if (process.ExitCode == -1073741819)
            {
                string errorMessage = $"There was an access violation error running the emulator.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                //Do not notify user
                
                return;
            }
            
            if (process.ExitCode == 1)
            {
                string errorMessage = $"Generic error in the emulator. User was not notified.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                //Do not notify user
                
                return;
            }

            if (process.ExitCode != 0)
            {
                string errorMessage = $"Emulator error. User was not notified.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                //Do not notify user
            }
        }
        
        catch (InvalidOperationException ex)
        {
            string formattedException = $"InvalidOperationException in the method LaunchRegularEmulatorWithoutWarnings";
            await LogErrors.LogErrorAsync(ex, formattedException);

            InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            string formattedException = $"The emulator could not open the game with the provided parameters. User was not notified.\n\n" +
                                        $"Exit code: {process.ExitCode}\n" +
                                        $"Emulator: {psi.FileName}\n" +
                                        $"Emulator output: {output}\n" +
                                        $"Emulator error: {error}\n" +
                                        $"Calling parameters: {psi.Arguments}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            //Do not notify user
        }
    }

    private static async Task LaunchXblaGame(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();
    
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (systemConfig == null)
        {
            string errorMessage = "systemConfig not found for the selected system.\n\n" +
                                  "Method: LaunchXblaGame";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            CouldNotLaunchEmulatorMessageBox();
            return;
        }
    
        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchXblaGame";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            CouldNotLaunchEmulatorMessageBox();
            return;
        }

        string gamePathToLaunch;

        // Check if extraction is needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            string fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

            // Accept ZIP, 7Z and RAR files
            if (fileExtension == ".ZIP" || fileExtension == ".7Z" || fileExtension == ".RAR")
            {
                string tempExtractLocation = await ExtractCompressedFile.Instance2.ExtractArchiveToTempAsync(filePath);
                
                if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
                {
                    MessageBox.Show("Extraction failed. Could not find the temporary extract folder.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                gamePathToLaunch = await FindXblaGamePath(tempExtractLocation); // Search within the extracted folder
            }
            else
            {
                MessageBox.Show($"The selected file '{filePath}' cannot be extracted.\n\n" +
                                $"To extract a file, it needs to be a 7z, zip, or rar file.\n\n" +
                                $"Please go to Edit System - Expert Mode, and edit this system.", 
                    "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        else
        {
            MessageBox.Show("To launch Xbox 360 XBLA games the compressed file need to be extracted first.\n\n" +
                            "Please go to Expert Mode and change the 'Extract File Before Launch' to true.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        
        if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
        {
            string errorMessage = $"Invalid game path: {gamePathToLaunch}. Cannot launch game.";
            Exception ex = new ArgumentNullException(nameof(gamePathToLaunch), errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            MessageBox.Show("Could not find a game file in the 000D0000 folder inside the temporary folder.\n\n" +
                            "Please check the compressed file to see if you can find the game file inside it.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                throw new InvalidOperationException("Failed to start the process.\n" +
                                                    "Method: LaunchXblaGame");
            }
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (!process.HasExited)
            {
                throw new InvalidOperationException("The process has not exited as expected.\n" +
                                                    "Method: LaunchXblaGame");
            }
            
            // Memory Access Violation error
            if (process.ExitCode == -1073741819)
            {
                string errorMessage = $"There was an memory access violation error running this emulator with this ROM.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                // MemoryAccessViolationErrorMessageBox();
                
                return;
            }

            // Other errors
            if (process.ExitCode != 0)
            {
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                EmulatorCouldNotOpenWithProvidedParametersSimpleMessageBox();
            }
            
        }
        catch (InvalidOperationException ex)
        {
            string formattedException = $"InvalidOperationException in the method LaunchXblaGame";
            await LogErrors.LogErrorAsync(ex, formattedException);

            InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            string formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                        $"Exit code: {process.ExitCode}\n" +
                                        $"Emulator: {psi.FileName}\n" +
                                        $"Emulator output: {output}\n" +
                                        $"Emulator error: {error}\n" +
                                        $"Calling parameters: {psi.Arguments}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            EmulatorCouldNotOpenWithProvidedParametersSimpleMessageBox();
        }
    }
    
    private static async Task LaunchMattelAquariusGame(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);
        if (systemConfig == null)
        {
            string errorMessage = $"systemConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchMattelAquariusGame";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            CouldNotLaunchEmulatorMessageBox();
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);
        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchMattelAquariusGame";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            CouldNotLaunchEmulatorMessageBox();
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
            string errorMessage = $"Invalid game path: {gamePathToLaunch}. Cannot launch the game.";
            Exception ex = new ArgumentNullException(nameof(gamePathToLaunch), errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Construct the PSI
        string programLocation = emulatorConfig.EmulatorLocation;
        string parameters = emulatorConfig.EmulatorParameters;
        string workingDirectory = Path.GetDirectoryName(programLocation);
        string gameFilenameWithoutExtension = Path.GetFileNameWithoutExtension(gamePathToLaunch);
        string arguments = $"{parameters} {gameFilenameWithoutExtension}";

        // Check programLocation before call it
        if (string.IsNullOrWhiteSpace(programLocation) || !File.Exists(programLocation))
        {
            MessageBox.Show("Invalid emulator executable path. Please check the configuration.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Check workingDirectory before call it
        if (string.IsNullOrEmpty(workingDirectory))
        {
            MessageBox.Show("Invalid working directory. Please check the file.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                throw new InvalidOperationException("Failed to start the process.\n" +
                                                    "Method: LaunchMattelAquariusGame");
            }
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            
            if (!process.HasExited)
            {
                throw new InvalidOperationException("The process has not exited as expected.\n" +
                                                    "Method: LaunchMattelAquariusGame");
            }

            // Memory Access Violation error
            if (process.ExitCode == -1073741819)
            {
                string errorMessage = $"There was an access violation error running the emulator.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                // MemoryAccessViolationErrorMessageBox();
                
                return;
            }
            
            // Other errors
            if (process.ExitCode != 0)
            {
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                CouldNotLaunchGameMessageBox();
            }
        }
        
        catch (InvalidOperationException ex)
        {
            string formattedException = $"InvalidOperationException in the method LaunchMattelAquariusGame";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            InvalidOperationExceptionMessageBox();
        }
        catch (Exception ex)
        {
            string formattedException = $"The emulator could not open the game with the provided parameters.\n\n" +
                                        $"Exit code: {process.ExitCode}\n" +
                                        $"Emulator: {psi.FileName}\n" +
                                        $"Emulator output: {output}\n" +
                                        $"Emulator error: {error}\n" +
                                        $"Calling parameters: {psi.Arguments}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            EmulatorCouldNotOpenWithProvidedParametersMessageBox();
        }
    }
    
    private static async Task<string> ExtractFilesBeforeLaunch(string filePath, SystemConfig systemConfig, string gamePathToLaunch)
    {
        string fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

        if (fileExtension == ".ZIP")
        {
            // Use a native .net library to extract
            string tempExtractLocation = await ExtractCompressedFile.Instance2.ExtractArchiveToTempAsync2(filePath);

            var extractFilesBeforeLaunch = await ValidateAndFindGameFile(tempExtractLocation);
            if (extractFilesBeforeLaunch != null) return extractFilesBeforeLaunch;
        }
        else if (fileExtension == ".7Z" || fileExtension == ".RAR")
        {
            // Use 7z to extract
            string tempExtractLocation = await ExtractCompressedFile.Instance2.ExtractArchiveToTempAsync(filePath);
                
            var extractFilesBeforeLaunch = await ValidateAndFindGameFile(tempExtractLocation);
            if (extractFilesBeforeLaunch != null) return extractFilesBeforeLaunch;
        }else
        {
            MessageBox.Show($"The selected file '{filePath}' cannot be extracted.\n\n" +
                            $"To extract a file, it needs to be a 7z, zip, or rar file.\n\n" +
                            $"Please go to Expert Mode and fix this system.",
                "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
            return gamePathToLaunch;
        }
        return gamePathToLaunch;

        async Task<string> ValidateAndFindGameFile(string tempExtractLocation)
        {
            if (string.IsNullOrEmpty(tempExtractLocation) || !Directory.Exists(tempExtractLocation))
            {
                MessageBox.Show("Extraction failed. Could not find the temporary extract folder.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return gamePathToLaunch;
            }
                
            if (systemConfig.FileFormatsToLaunch == null)
            {
                MessageBox.Show("There is no 'Extension to Launch After Extraction' set in the system configuration.\n\n" +
                                "Please go to Expert Mode and fix this system.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"No valid game file found with the specified extension: {string.Join(", ", systemConfig.FileFormatsToLaunch)}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return gamePathToLaunch;
            }

            if (!fileFound)
            {
                string errorMessage = "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);

                MessageBox.Show("Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.\n\n" +
                                "Please go to Expert Mode and fix this system.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return gamePathToLaunch;
            }
            return null;
        }
    }
    
    private static Task<string> FindXblaGamePath(string rootFolderPath)
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
    
    private static void CouldNotLaunchGameMessageBox()
    {
        string theapplicationcouldnotlaunchtheselectedgame2 = (string)Application.Current.TryFindResource("Theapplicationcouldnotlaunchtheselectedgame") ?? "The application could not launch the selected game.";
        string ifyouaretryingtorunMame2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunMAME") ?? "If you are trying to run MAME, ensure that your ROM collection is compatible with the latest version of MAME.";
        string ifyouaretryingtorunRetroarch2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarch") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core you are using are installed.";
        string alsoverifythattheemulator2 = (string)Application.Current.TryFindResource("Alsoverifythattheemulator") ?? "Also, verify that the emulator you are using is properly configured. Check if it requires BIOS or system files to work properly.";
        string doyouwanttoopenthefile2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{theapplicationcouldnotlaunchtheselectedgame2}\n\n" +
            $"{ifyouaretryingtorunMame2}\n\n" +
            $"{ifyouaretryingtorunRetroarch2}\n\n" +
            $"{alsoverifythattheemulator2}\n\n" +
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
                MessageBox.Show("The file 'error_user.log' was not found!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private static void EmulatorCouldNotOpenWithProvidedParametersMessageBox()
    {
        string theemulatorcouldnotopenthegamewiththeprovidedparameters2 = (string)Application.Current.TryFindResource("Theemulatorcouldnotopenthegamewiththeprovidedparameters") ?? "The emulator could not open the game with the provided parameters.";
        string ifyouaretryingtorunMame2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunMAME") ?? "If you are trying to run MAME, ensure that your ROM collection is compatible with the latest version of MAME.";
        string ifyouaretryingtorunRetroarch2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarch") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core you are using are installed.";
        string alsoverifythattheemulator2 = (string)Application.Current.TryFindResource("Alsoverifythattheemulator") ?? "Also, verify that the emulator you are using is properly configured. Check if it requires BIOS or system files to work properly.";
        string doyouwanttoopenthefile2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{theemulatorcouldnotopenthegamewiththeprovidedparameters2}\n\n" +
            $"{ifyouaretryingtorunMame2}\n\n" +
            $"{ifyouaretryingtorunRetroarch2}\n\n" +
            $"{alsoverifythattheemulator2}\n\n" +
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
                MessageBox.Show("The file 'error_user.log' was not found!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private static void InvalidOperationExceptionMessageBox()
    {
        string failedtostarttheemulator2 = (string)Application.Current.TryFindResource("Failedtostarttheemulator") ?? "Failed to start the emulator or it has not exited as expected.";
        string thistypeoferrorhappenswhen2 = (string)Application.Current.TryFindResource("Thistypeoferrorhappenswhen") ?? "This type of error happens when";
        string doesnothavetheprivilegestolaunch2 = (string)Application.Current.TryFindResource("doesnothavetheprivilegestolaunch") ?? "does not have the privileges to launch an external program, such as the emulator.";
        string youneedtogivemoreprivilegesto2 = (string)Application.Current.TryFindResource("Youneedtogivemoreprivilegesto") ?? "You need to give more privileges to";
        string toperformitstask2 = (string)Application.Current.TryFindResource("toperformitstask") ?? "to perform its task.";
        string pleaseconfigureittorun2 = (string)Application.Current.TryFindResource("Pleaseconfigureittorun") ?? "Please configure it to run with administrative privileges.";
        string anotherpossiblecausefortheerror2 = (string)Application.Current.TryFindResource("Anotherpossiblecausefortheerror") ?? "Another possible cause for the error is related to the integrity of the emulator.";
        string pleasereinstalltheemulator2 = (string)Application.Current.TryFindResource("Pleasereinstalltheemulator") ?? "Please reinstall the emulator to ensure it is working.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtostarttheemulator2}\n\n" +
                        $"{thistypeoferrorhappenswhen2} 'Simple Launcher' {doesnothavetheprivilegestolaunch2}\n" +
                        $"{youneedtogivemoreprivilegesto2} 'Simple Launcher' {toperformitstask2}\n" +
                        $"{pleaseconfigureittorun2}\n\n" +
                        $"{anotherpossiblecausefortheerror2}\n" +
                        $"{pleasereinstalltheemulator2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    private static void CouldNotLaunchEmulatorMessageBox()
    {
        var result = MessageBox.Show("There was an error launching this game.\n\n" +
                                     "The error was reported to the developer that will try to fix the issue.\n\n" +
                                     "Do you want to open the file 'error_user.log' to debug the error?",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
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
                MessageBox.Show("The file 'error_user.log' was not found!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    // private static void MemoryAccessViolationErrorMessageBox()
    // {
    //     string therewasanmemoryaccessviolation2 = (string)Application.Current.TryFindResource("Therewasanmemoryaccessviolation") ?? "There was an memory access violation error running this emulator with this ROM.";
    //     string thistypeoferrorusuallyoccurs2 = (string)Application.Current.TryFindResource("Thistypeoferrorusuallyoccurs") ?? "This type of error usually occurs when the emulator attempts to access memory it doesn't have permission to read or write.";
    //     string thiscanhappeniftheresabug2 = (string)Application.Current.TryFindResource("Thiscanhappeniftheresabug") ?? "This can happen if there’s a bug in the emulator code, meaning the emulator is not fully compatible with that ROM.";
    //     string anotherpossibilityistheRom2 = (string)Application.Current.TryFindResource("AnotherpossibilityistheROM") ?? "Another possibility is the ROM or any dependency files (such as DLLs) are corrupted.";
    //     string doyouwanttoopenfile2 = (string)Application.Current.TryFindResource("Doyouwanttoopenfile") ?? "Do you want to open file 'error_user.log' to debug the error?";
    //     string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
    //     var result = MessageBox.Show(
    //         $"{therewasanmemoryaccessviolation2}\n\n" +
    //         $"{thistypeoferrorusuallyoccurs2}\n\n" +
    //         $"{thiscanhappeniftheresabug2}\n\n" +
    //         $"{anotherpossibilityistheRom2}\n\n" +
    //         $"{doyouwanttoopenfile2}",
    //         error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
    //
    //     if (result == MessageBoxResult.Yes)
    //     {
    //         try
    //         {
    //             Process.Start(new ProcessStartInfo
    //             {
    //                 FileName = LogPath,
    //                 UseShellExecute = true
    //             });
    //         }
    //         catch (Exception)
    //         {
    //             MessageBox.Show("The file 'error_user.log' was not found!",
    //                 "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //         }
    //     }
    // }
    
    private static void EmulatorCouldNotOpenWithProvidedParametersSimpleMessageBox()
    {
        var result = MessageBox.Show(
            "The emulator could not open the game with the provided parameters.\n\n" +
            "Do you want to open the file 'error_user.log' to debug the error?",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

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
                MessageBox.Show("The file 'error_user.log' was not found!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
}
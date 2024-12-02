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
        if (string.IsNullOrWhiteSpace(filePath))
        {
            MessageBox.Show("The filePath cannot be null or empty.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            string contextMessage = "The filePath is null or empty in the method HandleButtonClick.";
            Exception ex = new();
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            return;
        }
        
        if (systemComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select a system from the system combo box.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            
            string contextMessage = "The system from the system combo box is null or empty in the method HandleButtonClick.";
            Exception ex = new();
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            return;
        }

        if (emulatorComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select an emulator from the emulator combo box.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            
            string contextMessage = "The emulator from the emulator combo box is null or empty in the method HandleButtonClick.";
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
                    if (selectedSystem.ToLowerInvariant().Contains("aquarius") && emulatorComboBox.SelectedItem.ToString().ToLowerInvariant().Contains("mame"))
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
            await LogErrors.LogErrorAsync(ex,
                $"Generic error in the GameLauncher class.\n\n" +
                $"Exception type: {ex.GetType().Name}\n" +
                $"Exception details: {ex.Message}\n" +
                $"FilePath: {filePath}\n" +
                $"SelectedSystem: {systemComboBox.SelectedItem}\n" +
                $"SelectedEmulator: {emulatorComboBox.SelectedItem}");

            var result = MessageBox.Show(
                "The application could not launch the selected game.\n\n" +
                "If you are trying to run MAME, ensure that your ROM collection is compatible with the latest version of MAME.\n\n" +
                "If you are trying to run Retroarch, ensure that the BIOS or required files for the core you are using are installed.\n\n" +
                "Also, verify that the emulator you are using is properly configured. Check if it requires BIOS or system files to work properly.\n\n" +
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
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
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath), @"Batch file path is null or empty.");
        }

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

            if (process.ExitCode != 0 || error.Length > 0)
            {
                string errorMessage = $"There was an issue running the batch process.\n\n" +
                                      $"Batch file: {psi.FileName}\n" +
                                      $"Exit code {process.ExitCode}\n" +
                                      $"Output: {output}\n" +
                                      $"Error: {error}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                var result = MessageBox.Show("There was an issue running the batch process.\n\n" +
                                "Try to run the batch file outside 'Simple Launcher' to see if it is working properly.\n\n" +
                                "Maybe the batch file has errors.\n\n" +
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
                        MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"There was an issue running the batch process.\n\n" +
                                  $"Batch file: {psi.FileName}\n" +
                                  $"Exit code {process.ExitCode}\n" +
                                  $"Output: {output}\n" +
                                  $"Error: {error}\n" +
                                  $"Exception type: {ex.GetType().Name}\n" +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                        
            var result = MessageBox.Show("There was an issue running the batch process.\n\n" +
                            "Try to run the batch file outside 'Simple Launcher' to see if it is working.\n\n" +
                            "Maybe the batch file has errors.\n\n" +
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
        }
    }

    private static async Task LaunchShortcutFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath), @"Shortcut filePath is null or empty.");
        }
        
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
                        MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private static async Task LaunchExecutable(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentNullException(nameof(filePath), @"Executable filePath is null or empty.");
        }

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
                        MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
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
                        
            var result = MessageBox.Show("There was an error launching the executable file.\n\n" +
                            "Try to launch the executable file outside 'Simple Launcher' to see if it is working.\n\n" +
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private static async Task LaunchRegularEmulator(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        if (emulatorComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select an emulator first.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        
        string selectedSystem = systemComboBox.SelectedItem.ToString();
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (systemConfig == null)
        {
            string errorMessage = $"systemConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchRegularEmulator";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchRegularEmulator";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            return;
        }

        string gamePathToLaunch = filePath;

        // Extract File if Needed
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

                if (systemConfig.FileFormatsToLaunch == null)
                {
                    MessageBox.Show("There is no 'Extension to Launch After Extraction' set in the system configuration.\n\n" +
                                    "Please go to Expert Mode and fix this system.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
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
                    return;
                }

                if (!fileFound)
                {
                    string errorMessage = "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
                    Exception exception = new(errorMessage);
                    await LogErrors.LogErrorAsync(exception, errorMessage);

                    MessageBox.Show("Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.\n\n" +
                                    "Please go to Expert Mode and fix this system.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show($"The selected file '{filePath}' cannot be extracted.\n\n" +
                                $"To extract a file, it needs to be a 7z, zip, or rar file.\n\n" +
                                $"Please go to Expert Mode and fix this system.",
                    "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        
        if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
        {
            string errorMessage = $"Invalid game path: {gamePathToLaunch}. Cannot launch the game.";
            Exception ex = new ArgumentNullException(nameof(gamePathToLaunch), errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            MessageBox.Show("Invalid game file path. Please check the file.",
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
            // Attempt to start the process
            bool processStarted = process.Start();
            
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.\n" +
                                                    "Method: LaunchRegularEmulator");
            }
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            // Wait for the process to exit
            await process.WaitForExitAsync();
            
            // Verify if the process has exited before accessing ExitCode
            if (!process.HasExited)
            {
                throw new InvalidOperationException("The process has not exited as expected.\n" +
                                                    "Method: LaunchRegularEmulator");
            }

            if (process.ExitCode != 0 && process.ExitCode != -1073741819)
            {
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                var result = MessageBox.Show(
                    "The application could not launch the selected game.\n\n" +
                    "If you are trying to run MAME, ensure that your ROM collection is compatible with the latest version of MAME.\n\n" +
                    "If you are trying to run Retroarch, ensure that the BIOS or required files for the core you are using are installed.\n\n" +
                    "Also, verify that the emulator you are using is properly configured. Check if it requires BIOS or system files to work properly.\n\n" +
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
                        MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                
                return;
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
                
                var result = MessageBox.Show(
                    "There was an memory access violation error running this emulator with this ROM.\n\n" +
                    "This type of error usually occurs when the emulator attempts to access memory it doesn't have permission to read or write.\n" +
                    "This can happen if there’s a bug in the emulator code, meaning the emulator is not fully compatible with that ROM.\n" +
                    "Another possibility is the ROM or any dependency files (such as DLLs) are corrupted.\n\n" +
                    "Do you want to open file 'error_user.log' to debug the error?",
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
                        MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        
        catch (InvalidOperationException ex)
        {
            string formattedException = $"InvalidOperationException in the method LaunchRegularEmulator";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            MessageBox.Show("Failed to start the emulator or it has not exited as expected.\n\n" +
                            "This type of error happens when 'Simple Launcher' does not have the privileges to launch an external program, such as the emulator.\n" +
                            "You need to give more privileges to 'Simple Launcher' to perform its task.\n" +
                            "Please configure it to run with administrative privileges.\n\n" +
                            "Another possible cause for the error is related to the integrity of the emulator.\n" +
                            "Please reinstall the emulator to ensure it is working.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                
            var result = MessageBox.Show(
                "The emulator could not open the game with the provided parameters.\n\n" +
                "If you are trying to run MAME, be sure that your ROM collection is compatible with the latest version of MAME.\n\n" +
                "If you are trying to run Retroarch, ensure to install bios or required files for the core you are using.\n\n" +
                "Also, verify that the emulator you are using is properly configured. Check if it requires BIOS or system files to work properly.\n\n" +
                "Would you like to open the file 'error_user.log' to debug the error?",
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
    
    private static async Task LaunchXblaGame(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        if (emulatorComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select an emulator first.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();
    
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (systemConfig == null)
        {
            string errorMessage = "systemConfig not found for the selected system.\n\n" +
                                  "Method: LaunchXblaGame";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            return;
        }
    
        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchXblaGame";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
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
            // Attempt to start the process
            bool processStarted = process.Start();

            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.\n" +
                                                    "Method: LaunchXblaGame");
            }
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            // Wait for the process to exit
            await process.WaitForExitAsync();

            // Verify if the process has exited before accessing ExitCode
            if (!process.HasExited)
            {
                throw new InvalidOperationException("The process has not exited as expected.\n" +
                                                    "Method: LaunchXblaGame");
            }
            
            if (process.ExitCode != 0 && process.ExitCode != -1073741819)
            {
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                var result = MessageBox.Show(
                    "The emulator could not open the game with the provided parameters.\n\n" +
                    "Do you wan to open file 'error_user.log' to debug the error?",
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
                        MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                
                return;
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
                
                var result = MessageBox.Show(
                    "There was an memory access violation error running this emulator with this ROM.\n" +
                    "This type of error usually occurs when the emulator attempts to access memory it doesn't have permission to read or write.\n" +
                    "This can happen if there’s a bug in the emulator code, meaning the emulator is not fully compatible with that ROM.\n" +
                    "Another possibility is the ROM or any dependency files (such as DLLs) are corrupted.\n\n" +
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
                        MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            string formattedException = $"InvalidOperationException in the method LaunchXblaGame";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            MessageBox.Show("Failed to start the emulator or it has not exited as expected.\n\n" +
                            "This type of error happens when 'Simple Launcher' does not have the privileges to launch an external program, such as the emulator.\n" +
                            "You need to give more privileges to 'Simple Launcher' to perform its task.\n" +
                            "Please configure it to run with administrative privileges.\n\n" +
                            "Another possible cause for the error is related to the integrity of the emulator.\n" +
                            "Please reinstall the emulator to ensure it is working.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
    
    private static async Task LaunchMattelAquariusGame(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        if (emulatorComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select an emulator first.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        
        string selectedSystem = systemComboBox.SelectedItem.ToString();
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (systemConfig == null)
        {
            string errorMessage = $"systemConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchMattelAquariusGame";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\n" +
                                  $"Method: LaunchMattelAquariusGame";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
            return;
        }

        string gamePathToLaunch = filePath;

        // Extract File if Needed
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

                if (systemConfig.FileFormatsToLaunch == null)
                {
                    MessageBox.Show("There is no 'Extension to Launch After Extraction' set in the system configuration.\n\n" +
                                    "Please go to Expert Mode and fix this system.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
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
                    return;
                }

                if (!fileFound)
                {
                    string errorMessage = "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
                    Exception exception = new(errorMessage);
                    await LogErrors.LogErrorAsync(exception, errorMessage);

                    MessageBox.Show("Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.\n\n" +
                                    "Please go to Expert Mode and fix this system.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show($"The selected file '{filePath}' cannot be extracted.\n\n" +
                                $"To extract a file, it needs to be a 7z, zip, or rar file.\n\n" +
                                $"Please go to Expert Mode and fix this system.",
                    "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        
        if (string.IsNullOrEmpty(gamePathToLaunch) || !File.Exists(gamePathToLaunch))
        {
            string errorMessage = $"Invalid game path: {gamePathToLaunch}. Cannot launch the game.";
            Exception ex = new ArgumentNullException(nameof(gamePathToLaunch), errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            
            MessageBox.Show("Invalid game file path. Please check the file.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Construct the PSI
        string programLocation = emulatorConfig.EmulatorLocation;
        string parameters = emulatorConfig.EmulatorParameters;
        string gameFilenameWithoutExtension = Path.GetFileNameWithoutExtension(gamePathToLaunch);
        string arguments = $"{parameters} {gameFilenameWithoutExtension}";

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
            // Attempt to start the process
            bool processStarted = process.Start();
            
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.\n" +
                                                    "Method: LaunchMattelAquariusGame");
            }
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            // Wait for the process to exit
            await process.WaitForExitAsync();
            
            // Verify if the process has exited before accessing ExitCode
            if (!process.HasExited)
            {
                throw new InvalidOperationException("The process has not exited as expected.\n" +
                                                    "Method: LaunchMattelAquariusGame");
            }

            if (process.ExitCode != 0 && process.ExitCode != -1073741819)
            {
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n" +
                                      $"Calling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                var result = MessageBox.Show(
                    "The application could not launch the selected game.\n\n" +
                    "If you are trying to run MAME, ensure that your ROM collection is compatible with the latest version of MAME.\n\n" +
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
                        MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                
                return;
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
                
                var result = MessageBox.Show(
                    "There was an memory access violation error running MAME with this ROM.\n\n" +
                    "This type of error usually occurs when the emulator attempts to access memory it doesn't have permission to read or write.\n" +
                    "This can happen if there’s a bug in the emulator code, meaning the emulator is not fully compatible with that ROM.\n" +
                    "Another possibility is the ROM or any dependency files (such as DLLs) are corrupted.\n\n" +
                    "Do you want to open file 'error_user.log' to debug the error?",
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
        
        catch (InvalidOperationException ex)
        {
            string formattedException = $"InvalidOperationException in the method LaunchMattelAquariusGame";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            MessageBox.Show("Failed to start the MAME emulator or it has not exited as expected.\n\n" +
                            "This type of error happens when 'Simple Launcher' does not have the privileges to launch an external program, such as the emulator.\n" +
                            "You need to give more privileges to 'Simple Launcher' to perform its task.\n" +
                            "Please configure it to run with administrative privileges.\n\n" +
                            "Another possible cause for the error is related to the integrity of the emulator.\n" +
                            "Please reinstall the emulator to ensure it is working.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            string formattedException = $"The MAME emulator could not open the game with the provided parameters.\n\n" +
                                        $"Exit code: {process.ExitCode}\n" +
                                        $"Emulator: {psi.FileName}\n" +
                                        $"Emulator output: {output}\n" +
                                        $"Emulator error: {error}\n" +
                                        $"Calling parameters: {psi.Arguments}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            var result = MessageBox.Show(
                "The MAME emulator could not open the game with the provided parameters.\n\n" +
                "If you are trying to run MAME, be sure that your ROM collection is compatible with the latest version of MAME.\n\n" +
                "Would you like to open the file 'error_user.log' to debug the error?",
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
                    MessageBox.Show("The file 'error_user.log' was not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
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
    public static async Task HandleButtonClick(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs, SettingsConfig settings, MainWindow mainWindow)
    {
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
                $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}");
            
            MessageBox.Show("The application could not launch the selected game.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.\n\n" +
                            "If you want to debug the error yourself you can see the file 'error_user.log' inside 'Simple Launcher' folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (process.ExitCode != 0 || error.Length > 0)
            {
                string errorMessage = $"There was an issue running the batch process.\n\n" +
                                      $"Batch file: {psi.FileName}\nExit code {process.ExitCode}\nOutput: {output}\nError: {error}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                MessageBox.Show("There was an issue running the batch process.\n\n" +
                                "Try to run the batch file outside 'Simple Launcher' to see if it is working properly.\n\n" +
                                "Maybe the batch file has errors.\n\n" +
                                "If you want to debug the error you can see the file 'error_user.log' inside 'Simple Launcher' folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"There was an issue running the batch process.\n\n" +
                                  $"Batch file: {psi.FileName}\nExit code {process.ExitCode}\nOutput: {output}\nError: {error}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                        
            MessageBox.Show("There was an issue running the batch process.\n\n" +
                            "Try to run the batch file outside 'Simple Launcher' to see if it is working.\n\n" +
                            "Maybe the batch file has errors.\n\n" +
                            "If you want to debug the error you can see the file 'error_user.log' inside 'Simple Launcher' folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                      $"Shortcut file: {psi.FileName}\nExit code {process.ExitCode}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                MessageBox.Show("There was an error launching the shortcut file.\n\n" +
                                "Try to run the shortcut file outside 'Simple Launcher' to see if it is working properly.\n\n" +
                                "If you want to debug the error you can see the file 'error_user.log' inside 'Simple Launcher' folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            string errorDetails = $"Error launching the shortcut file.\n\n" +
                                  $"Shortcut file: {psi.FileName}\nExit code {process.ExitCode}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorDetails);
                        
            MessageBox.Show("There was an error launching the shortcut file.\n\n" +
                            "Try to run the shortcut file outside 'Simple Launcher' to see if it is working properly.\n\n" +
                            "If you want to debug the error you can see the file 'error_user.log' inside 'Simple Launcher' folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                      $"Executable file: {psi.FileName}\nExit code {process.ExitCode}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                MessageBox.Show("There was an error launching the executable file.\n\n" +
                                "Try to run the executable file outside 'Simple Launcher' to see if it is working properly.\n\n" +
                                "If you want to debug the error you can see the file 'error_user.log' inside 'Simple Launcher' folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            string errorDetails = $"Error launching the executable file.\n\n" +
                                  $"Executable file: {psi.FileName}\nExit code {process.ExitCode}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorDetails);
                        
            MessageBox.Show("There was an error launching the executable file.\n\n" +
                            "Try to launch the executable file outside 'Simple Launcher' to see if it is working.\n\n" +
                            "If you want to debug the error you can see the file 'error_user.log' inside 'Simple Launcher' folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static async Task LaunchRegularEmulator(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        if (emulatorComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select an emulator first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();

        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (systemConfig == null)
        {
            string errorMessage = $"systemConfig not found for the selected system.\n\nError generated inside GameLauncher class.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            MessageBox.Show("There was an error launching this game.\n\nThe error was reported to the developer that will try to fix the issue.\n\nIf you want to debug the error you can see the file 'error_user.log' in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\nError generated inside GameLauncher class.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            MessageBox.Show("There was an error launching this game.\n\nThe error was reported to the developer that will try to fix the issue.\n\nIf you want to debug the error you can see the file 'error_user.log' in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (!fileFound)
                {
                    string errorMessage = @"Could not find a file with the extension defined in 'Format to Launch After Extraction' inside the extracted folder.";
                    Exception exception = new(errorMessage);
                    await LogErrors.LogErrorAsync(exception, errorMessage);

                    MessageBox.Show("Could not find a file with the extension defined in 'Format to Launch After Extraction' inside the extracted folder.\n\n" +
                                    "Please go to Edit System - Expert Mode and fix the settings for this system.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show($"The selected file '{filePath}' cannot be extracted.\n\n" +
                                $"To extract a file, it needs to be a 7z, zip, or rar file.\n\n" +
                                $"Please go to Edit System - Expert Mode, and edit this system.", 
                    "Invalid File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(gamePathToLaunch))
            {
                return;
            }
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
            bool processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.");
            }
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0 && process.ExitCode != -1073741819)
            {
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\nExit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                var result = MessageBox.Show(
                    "The emulator could not open the game with the provided parameters.\n\n" +
                    "If you are trying to run MAME, be sure that your ROM collection is compatible with the latest version of MAME.\n\n" +
                    "If you are trying to run Retroarch, ensure to install bios or required files for the core you are using.\n\n" +
                    "If you want to debug the error, you can see the 'error_user.log' file inside the 'Simple Launcher' folder.\n\n" +
                    "Would you like to be redirected to the 'Simple Launcher' Wiki, where you will find a list of parameters for each emulator?",
                    "Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters",
                        UseShellExecute = true
                    });
                }
            }
        
            // Memory Access Violation error
            if (process.ExitCode == -1073741819)
            {
                string errorMessage = $"There was an access violation error running the emulator.\n\nExit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                var result = MessageBox.Show(
                    "There was an memory access violation error running this emulator with this ROM.\n" +
                    "This type of error usually occurs when the emulator attempts to access memory it doesn't have permission to read or write.\n" +
                    "This can happen if there’s a bug in the emulator code, meaning the emulator is not fully compatible with that ROM.\n" +
                    "Another possibility is the ROM or any dependency files (such as DLLs) are corrupted.\n\n" +
                    "If you want to debug the error, you can see the file 'error_user.log' inside 'Simple Launcher' folder.\n\n" +
                    "Please visit our Wiki on GitHub. There, you will find a list of recommended emulators and parameters.\n\n" +
                    "Would you like to be redirected to the 'Simple Launcher' Wiki?",
                    "Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters",
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"The emulator could not open the game with the provided parameters.\n\nExit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            var result = MessageBox.Show(
                "The emulator could not open the game with the provided parameters.\n\n" +
                "If you are trying to run MAME, be sure that your ROM collection is compatible with the latest version of MAME.\n\n" +
                "If you are trying to run Retroarch, ensure to install bios or required files for the core you are using.\n\n" +
                "If you want to debug the error, you can see the 'error_user.log' file inside the 'Simple Launcher' folder.\n\n" +
                "Would you like to be redirected to the 'Simple Launcher' Wiki, where you will find a list of parameters for each emulator?",
                "Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters",
                    UseShellExecute = true
                });
            }
        }
    }
    
    private static async Task LaunchXblaGame(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        if (emulatorComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select an emulator first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
        string selectedSystem = systemComboBox.SelectedItem.ToString();
    
        var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

        if (systemConfig == null)
        {
            string errorMessage = "systemConfig not found for the selected system.\n\nError generated inside GameLauncher class.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);

            MessageBox.Show("There was an error launching this game.\n\nThe error was reported to the developer that will try to fix the issue.\n\nIf you want to debug the error you can see the file 'error_user.log' in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    
        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\nError generated inside GameLauncher class.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            MessageBox.Show("There was an error launching this game.\n\nThe error was reported to the developer that will try to fix the issue.\n\nIf you want to debug the error you can see the file 'error_user.log' in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                
                if (!Directory.Exists(tempExtractLocation))
                {
                    MessageBox.Show("Extraction failed. Could not find the extracted files inside the temp folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show("To launch Xbox 360 XBLA games the compressed file need to be extracted first.\n\nPlease go to Edit System - Expert Mode and change the 'Extract File Before Launch' to true.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (string.IsNullOrEmpty(gamePathToLaunch))
        {
            MessageBox.Show("Could not find a game file in the 000D0000 folder inside the 'temp' folder.\n\nPlease check the compressed file to see if you can find the game file inside it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (process.ExitCode != 0 && process.ExitCode != -1073741819)
            {
                string errorMessage = $"The emulator could not open the game with the provided parameters.\n\n" +
                                      $"Exit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);

                var result = MessageBox.Show(
                    "The emulator could not open the game with the provided parameters.\n\n" +
                    "If you want to debug the error, you can see the 'error_user.log' file inside the 'Simple Launcher' folder.\n\n" +
                    "Would you like to be redirected to the 'Simple Launcher' Wiki, where you will find a list of parameters for each emulator?",
                    "Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters",
                        UseShellExecute = true
                    });
                }
            }
            
            // Memory Access Violation error
            if (process.ExitCode == -1073741819)
            {
                string errorMessage = $"There was an memory access violation error running this emulator with this ROM.\n\nExit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}";
                Exception ex = new(errorMessage);
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                var result = MessageBox.Show(
                    "There was an memory access violation error running this emulator with this ROM.\n" +
                    "This type of error usually occurs when the emulator attempts to access memory it doesn't have permission to read or write.\n" +
                    "This can happen if there’s a bug in the emulator code, meaning the emulator is not fully compatible with that ROM.\n" +
                    "Another possibility is the ROM or any dependency files (such as DLLs) are corrupted.\n\n" +
                    "If you want to debug the error, you can see the file 'error_user.log' inside 'Simple Launcher' folder.\n\n" +
                    "Please visit our Wiki on GitHub. There, you will find a list of recommended emulators and parameters.\n\n" +
                    "Would you like to be redirected to the 'Simple Launcher' Wiki?",
                    "Error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters",
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"The emulator could not open the game with the provided parameters.\n\nExit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            var result = MessageBox.Show(
                "The emulator could not open the game with the provided parameters.\n\n" +
                "If you want to debug the error, you can see the 'error_user.log' file inside the 'Simple Launcher' folder.\n\n" +
                "Would you like to be redirected to the 'Simple Launcher' Wiki, where you will find a list of parameters for each emulator?",
                "Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters",
                    UseShellExecute = true
                });
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
}
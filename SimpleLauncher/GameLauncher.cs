﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher
{
    public static class GameLauncher
    {
        public static async Task HandleButtonClick(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
        {
            ProcessStartInfo psi = null;

            try
            {
                string fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

                // Check if the file is a .bat file
                if (fileExtension == ".BAT")
                {
                    bool wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;

                    // If the GamePadController is running, stop it before executing the .BAT
                    if (wasGamePadControllerRunning)
                    {
                        GamePadController.Instance2.Stop();
                    }

                    psi = new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    Process process = new() { StartInfo = psi };
                    process.Start();

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    // If the GamePadController was running, restart it after the .BAT execution
                    if (wasGamePadControllerRunning)
                    {
                        GamePadController.Instance2.Start();
                    }

                    if (process.ExitCode != 0)
                    {
                        string errorMessage = $"Error launching the batch file.\n\nExit code {process.ExitCode}\n\nOutput: {output}\n\nError: {error}\n\nBAT file: {psi.FileName}";
                        Exception exception = new(errorMessage);
                        await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                        MessageBox.Show("There was an error launching the bat file.\n\nTry to run the bat file outside Simple Launcher to see if it is working properly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }

                // Check if the file is a .lnk (shortcut) file
                if (fileExtension == ".LNK")
                {
                    bool wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;

                    // If the GamePadController is running, stop it before launching the .LNK
                    if (wasGamePadControllerRunning)
                    {
                        GamePadController.Instance2.Stop();
                    }

                    psi = new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };

                    try
                    {
                        Process process = Process.Start(psi); // Start the process without redirecting output/error
                        if (process != null) await process.WaitForExitAsync(); // Wait for the process to exit

                        // If the GamePadController was running, restart it after the .LNK exits
                        if (wasGamePadControllerRunning)
                        {
                            GamePadController.Instance2.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorDetails = $"Error launching the lnk file.\n\nShortcut: {psi.FileName}\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                        await LogErrors.LogErrorAsync(ex, errorDetails);
                        
                        MessageBox.Show("The was an error launching the lnk file.\n\nTry to run the lnk file outside Simple Launcher to see if it is working properly.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }

                // Check if the file is a .exe (executable) file
                if (fileExtension == ".EXE")
                {
                    bool wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;

                    // If the GamePadController is running, stop it before launching the .EXE
                    if (wasGamePadControllerRunning)
                    {
                        GamePadController.Instance2.Stop();
                    }

                    psi = new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };

                    try
                    {
                        Process process = Process.Start(psi); // Start the process without redirecting output/error
                        if (process != null)
                            await process
                                .WaitForExitAsync(); // Use WaitForExitAsync to asynchronously wait for the process to exit

                        // If the GamePadController was running, restart it after the .EXE exits
                        if (wasGamePadControllerRunning)
                        {
                            GamePadController.Instance2.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorDetails = $"There was an error launching the exe file.\n\nProgram: {psi.FileName}\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                        await LogErrors.LogErrorAsync(ex, errorDetails);
                        
                        MessageBox.Show("There was an error launching the exe file.\n\nTry to launch the exe file outside Simple Launcher to see if it is working.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }

                // Regular call of the method
                if (emulatorComboBox.SelectedItem != null)
                {
                    bool wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;

                    // If the GamePadController is running, stop it before proceeding
                    if (wasGamePadControllerRunning)
                    {
                        GamePadController.Instance2.Stop();
                    }
                    
                    string selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();
                    string selectedSystem = systemComboBox.SelectedItem.ToString();

                    var systemConfig = systemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

                    if (systemConfig == null)
                    {
                        MessageBox.Show("Please select a valid system.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

                    if (emulatorConfig == null)
                    {
                        MessageBox.Show("Selected emulator configuration not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string gamePathToLaunch = filePath;  // Default to the original path

                    // Determine if extraction is needed based on system configuration
                    if (systemConfig.ExtractFileBeforeLaunch)
                    {
                        
                        if (fileExtension == ".ZIP" || fileExtension == ".7Z")
                        {
                            var pleaseWaitExtraction = new PleaseWaitExtraction();
                            pleaseWaitExtraction.Show();
                            
                            // Extract the archive to a temporary location
                            string tempExtractLocation = ExtractCompressedFile.Instance2.ExtractArchiveToTemp(filePath);
                            
                            var delayTask = Task.Delay(1500);
                            await delayTask;

                            if (string.IsNullOrEmpty(tempExtractLocation))
                            {
                                pleaseWaitExtraction.Close();
                                return;
                            }
                            pleaseWaitExtraction.Close();

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
                                string errorMessage = "Could not find a file with the extension defined in 'Format to Launch After Extraction'.";
                                Exception exception = new(errorMessage);
                                await LogErrors.LogErrorAsync(exception, errorMessage);
                                
                                MessageBox.Show("Could not find a file with the extension defined in 'Format to Launch After Extraction'.\n\nPlease edit system to fix it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    string programLocation = emulatorConfig.EmulatorLocation;
                    string parameters = emulatorConfig.EmulatorParameters;
                    string arguments = $"{parameters} \"{gamePathToLaunch}\"";
                    
                    // Create ProcessStartInfo
                    psi = new ProcessStartInfo
                    {
                        FileName = programLocation,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    // Initialize the process
                    Process process = new() { StartInfo = psi };
                    
                    // Create variables to store the output and error
                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    // Event handlers to capture standard output and error
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
                    
                    // Start the process and begin asynchronously reading output and error streams
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for the process to exit
                    await process.WaitForExitAsync();
                    
                    // Ensure to close output/error streams
                    await process.WaitForExitAsync();

                    // Generic error code
                    if ((process.ExitCode != 0 || error.Length > 0) && process.ExitCode != -1073741819)
                    {
                        string errorMessage = $"The emulator could not open this game.\n\nExit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}";
                        Exception ex = new(errorMessage);
                        await LogErrors.LogErrorAsync(ex, errorMessage);

                        MessageBox.Show($"The emulator could not open this game with the provided parameters.\n\nPlease visit Simple Launcher Wiki on GitHub. There, you will find a list of parameters for each emulator.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                    // Access Violation error
                    if (process.ExitCode == -1073741819)
                    {
                        string errorMessage = $"The emulator could not open this game.\n\nExit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}";
                        Exception ex = new(errorMessage);
                        await LogErrors.LogErrorAsync(ex, errorMessage);
                    }

                    // If the GamePadController was running, restart it after the psi exits
                    if (wasGamePadControllerRunning)
                    {
                        GamePadController.Instance2.Start();
                    }
                }
                else
                {
                    MessageBox.Show("Please select an emulator first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                string formattedException = $"The emulator could not open this game.\n\nEmulator: {psi.FileName}\nParameters: {psi.Arguments}\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                
                MessageBox.Show($"The emulator could not open this game with the provided parameters.\n\nPlease visit Simple Launcher Wiki on GitHub. There, you will find a list of parameters for each emulator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
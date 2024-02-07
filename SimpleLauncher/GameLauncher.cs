using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher
{
    public class GameLauncher
    {
        public static async Task HandleButtonClick(string filePath, ComboBox EmulatorComboBox, ComboBox SystemComboBox, List<SystemConfig> SystemConfigs)
        {
            ProcessStartInfo psi = null;

            try
            {
                string fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

                // Check if the file is a .bat file
                if (fileExtension == ".BAT")
                {
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

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show("Error executing the batch file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        string errorMessage = $"Error launching batch file: Exit code {process.ExitCode}\n";
                        errorMessage += $"Process Start Info:\nFileName: {psi.FileName}\n";
                        await LogErrors.LogErrorAsync(new Exception(errorMessage));
                    }

                    return;
                }

                // Check if the file is a .lnk (shortcut) file
                if (fileExtension == ".LNK")
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };

                    try
                    {
                        Process process = Process.Start(psi); // Start the process without redirecting output/error
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception
                        MessageBox.Show($"Error executing the shortcut: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        // Log the error with more details
                        await LogErrors.LogErrorAsync(new Exception(ex.Message));
                    }
                    return;
                }

                // Check if the file is a .exe (executable) file
                if (fileExtension == ".EXE")
                {
                    psi = new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    };

                    try
                    {
                        Process process = Process.Start(psi); // Start the process without redirecting output/error
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error executing the executable: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        await LogErrors.LogErrorAsync(new Exception(ex.Message));
                    }
                    return;
                }

                // Regular call of the method
                if (EmulatorComboBox.SelectedItem != null)
                {
                    string selectedEmulatorName = EmulatorComboBox.SelectedItem.ToString();
                    string selectedSystem = SystemComboBox.SelectedItem.ToString();

                    var systemConfig = SystemConfigs.FirstOrDefault(config => config.SystemName == selectedSystem);

                    if (systemConfig == null)
                    {
                        MessageBox.Show("Please select a valid system", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

                    if (emulatorConfig == null)
                    {
                        MessageBox.Show("Selected emulator configuration not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string gamePathToLaunch = filePath;  // Default to the original path

                    // Determine if extraction is needed based on system configuration
                    if (systemConfig.ExtractFileBeforeLaunch)
                    {
                        if (fileExtension == ".ZIP" || fileExtension == ".7Z")
                        {
                            // Extract the archive to a temporary location
                            string tempExtractLocation = ExtractCompressedFile.Instance.ExtractArchiveToTemp(filePath);

                            if (string.IsNullOrEmpty(tempExtractLocation))
                            {
                                MessageBox.Show("Failed to extract the archive.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                            if (!fileFound)
                            {
                                MessageBox.Show("Couldn't find a file with the specified extensions after extraction.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }

                    string programLocation = emulatorConfig.EmulatorLocation;
                    string parameters = emulatorConfig.EmulatorParameters;
                    string filename = Path.GetFileName(gamePathToLaunch);
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

                    // Launch the external program
                    Process process = new() { StartInfo = psi };
                    process.Start();

                    // Read the output streams
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    // Wait for the process to exit
                    process.WaitForExit();

                    if (process.ExitCode != 0 && process.ExitCode != -1073741819)
                    {
                        MessageBox.Show("The emulator could not open this file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        string errorMessage = $"Error launching external program: Exit code {process.ExitCode}\n";
                        errorMessage += $"Process Start Info:\nFileName: {psi.FileName}\nArguments: {psi.Arguments}\n";
                        await LogErrors.LogErrorAsync(new Exception(errorMessage));
                    }
                }
                else
                {
                    MessageBox.Show("Please select an emulator", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                string errorDetails = $"Exception Details:\n{ex}\n";
                if (psi != null)
                {
                    errorDetails += $"Process Start Info:\nFileName: {psi.FileName}\nArguments: {psi.Arguments}\n";
                }
                await LogErrors.LogErrorAsync(ex, errorDetails);
            }
        }

    }
}

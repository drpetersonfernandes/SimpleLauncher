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
    public static async Task HandleButtonClick(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
    {
        bool wasGamePadControllerRunning = GamePadController.Instance2.IsRunning;
        if (wasGamePadControllerRunning)
        {
            GamePadController.Instance2.Stop();
        }
        
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
                    await LaunchUsingEmulator(filePath, emulatorComboBox, systemComboBox, systemConfigs);
                    break;
            }
        }
        catch (Exception ex)
        {
            await LogErrors.LogErrorAsync(ex,
                $"Generic error in the GameLauncher class.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}");
        }
        finally
        {
            if (wasGamePadControllerRunning)
            {
                GamePadController.Instance2.Start();
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
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0 || error.Length > 0)
            {
                string errorMessage = $"Error launching the bat file.\n\nBAT file: {psi.FileName}\nExit code {process.ExitCode}\nOutput: {output}\nError: {error}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                MessageBox.Show("There was an error launching the bat file.\n\nTry to run the bat file outside Simple Launcher to see if it is working properly.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Error launching the batch file.\n\nBAT file: {psi.FileName}\nExit code {process.ExitCode}\nOutput: {output}\nError: {error}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
                        
            MessageBox.Show("There was an error launching the bat file.\n\nTry to run the bat file outside Simple Launcher to see if it is working properly.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static async Task LaunchShortcutFile(string filePath)
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
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
           
            if (process.ExitCode != 0 || error.Length > 0)
            {
                string errorMessage = $"Error launching the lnk file.\n\nLNK file: {psi.FileName}\nExit code {process.ExitCode}\nOutput: {output}\nError: {error}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                MessageBox.Show("There was an error launching the lnk file.\n\nTry to run the lnk file outside Simple Launcher to see if it is working properly.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            string errorDetails = $"Error launching the lnk file.\n\nLNK file: {psi.FileName}\nExit code {process.ExitCode}\nOutput: {output}\nError: {error}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorDetails);
                        
            MessageBox.Show("The was an error launching the lnk file.\n\nTry to run the lnk file outside Simple Launcher to see if it is working properly.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static async Task LaunchExecutable(string filePath)
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
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
           
            if (process.ExitCode != 0 || error.Length > 0)
            {
                string errorMessage = $"Error launching the exe file.\n\nEXE file: {psi.FileName}\nExit code {process.ExitCode}\nOutput: {output}\nError: {error}";
                Exception exception = new(errorMessage);
                await LogErrors.LogErrorAsync(exception, errorMessage);
                        
                MessageBox.Show("There was an error launching the exe file.\n\nTry to run the exe file outside Simple Launcher to see if it is working properly.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            string errorDetails = $"Error launching the exe file.\n\nEXE file: {psi.FileName}\nExit code {process.ExitCode}\nOutput: {output}\nError: {error}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorDetails);
                        
            MessageBox.Show("There was an error launching the exe file.\n\nTry to launch the exe file outside Simple Launcher to see if it is working.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static async Task LaunchUsingEmulator(string filePath, ComboBox emulatorComboBox, ComboBox systemComboBox, List<SystemConfig> systemConfigs)
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
            
            MessageBox.Show("There was an error launching this game.\n\nThe error was reported to the developer that will try to fix the issue.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var emulatorConfig = systemConfig.Emulators.FirstOrDefault(e => e.EmulatorName == selectedEmulatorName);

        if (emulatorConfig == null)
        {
            string errorMessage = $"emulatorConfig not found for the selected system.\n\nError generated inside GameLauncher class.";
            Exception exception = new(errorMessage);
            await LogErrors.LogErrorAsync(exception, errorMessage);
            
            MessageBox.Show("There was an error launching this game.\n\nThe error was reported to the developer that will try to fix the issue.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string gamePathToLaunch = filePath;

        // Extract File if Needed
        if (systemConfig.ExtractFileBeforeLaunch)
        {
            string tempExtractLocation = await ExtractCompressedFile.Instance2.ExtractArchiveToTempAsync(filePath);
            string fileExtension = Path.GetExtension(filePath).ToUpperInvariant();

            // Only Accept ZIP or 7Z
            if (fileExtension == ".ZIP" || fileExtension == ".7Z")
            {
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
                    string errorMessage = @"Could not find a file with the extension defined in 'Format to Launch After Extraction'.";
                    Exception exception = new(errorMessage);
                    await LogErrors.LogErrorAsync(exception, errorMessage);
                                
                    MessageBox.Show("Could not find a file with the extension defined in 'Format to Launch After Extraction'.\n\nPlease edit the system to fix it.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            
            if (string.IsNullOrEmpty(gamePathToLaunch)) return;
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
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

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
        }
        catch (Exception ex)
        {
            string formattedException = $"The emulator could not open this game.\n\nExit code: {process.ExitCode}\nEmulator: {psi.FileName}\nEmulator output: {output}\nEmulator error: {error}\nCalling parameters: {psi.Arguments}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
                
            MessageBox.Show($"The emulator could not open this game with the provided parameters.\n\nPlease visit Simple Launcher Wiki on GitHub. There, you will find a list of parameters for each emulator.\n\nIf you want to debug the error you can see the file error_user.log in the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
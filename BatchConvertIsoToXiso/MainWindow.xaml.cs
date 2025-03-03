using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BatchConvertIsoToXiso;

public partial class MainWindow
{
    private CancellationTokenSource _cts;

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        LogMessage("Welcome to the Batch Convert ISO to XISO.");
        LogMessage("");
        LogMessage("This program will convert ISO files to Xbox XISO format using extract-xiso.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the input folder containing ISO files to convert");
        LogMessage("2. Select the output folder where converted XISO files will be saved");
        LogMessage("3. Choose whether to delete original files after conversion");
        LogMessage("4. Click 'Start Conversion' to begin the process");
        LogMessage("");

        // Verify extract-xiso.exe exists
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var extractXisoPath = Path.Combine(appDirectory, "extract-xiso.exe");

        if (File.Exists(extractXisoPath))
        {
            LogMessage("extract-xiso.exe found in the application directory.");
        }
        else
        {
            LogMessage("WARNING: extract-xiso.exe not found in the application directory!");
            LogMessage("Please ensure extract-xiso.exe is in the same folder as this application.");
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        _cts.Cancel();
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    private void LogMessage(string message)
    {
        var timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

        Application.Current.Dispatcher.Invoke(() =>
        {
            LogViewer.AppendText($"{timestampedMessage}{Environment.NewLine}");
            LogViewer.ScrollToEnd();
        });
    }

    private void BrowseInputButton_Click(object sender, RoutedEventArgs e)
    {
        var inputFolder = SelectFolder("Select the folder containing ISO files to convert");
        if (!string.IsNullOrEmpty(inputFolder))
        {
            InputFolderTextBox.Text = inputFolder;
            LogMessage($"Input folder selected: {inputFolder}");
        }
    }

    private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var outputFolder = SelectFolder("Select the output folder where converted XISO files will be saved");
        if (!string.IsNullOrEmpty(outputFolder))
        {
            OutputFolderTextBox.Text = outputFolder;
            LogMessage($"Output folder selected: {outputFolder}");
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var extractXisoPath = Path.Combine(appDirectory, "extract-xiso.exe");

        if (!File.Exists(extractXisoPath))
        {
            LogMessage("Error: extract-xiso.exe not found in the application folder.");
            ShowError("extract-xiso.exe is missing from the application folder. Please ensure it's in the same directory as this application.");
            return;
        }

        var inputFolder = InputFolderTextBox.Text;
        var outputFolder = OutputFolderTextBox.Text;
        var deleteFiles = DeleteFilesCheckBox.IsChecked ?? false;

        if (string.IsNullOrEmpty(inputFolder))
        {
            LogMessage("Error: No input folder selected.");
            ShowError("Please select the input folder containing ISO files to convert.");
            return;
        }

        if (string.IsNullOrEmpty(outputFolder))
        {
            LogMessage("Error: No output folder selected.");
            ShowError("Please select the output folder where converted XISO files will be saved.");
            return;
        }

        // Reset cancellation token if it was previously used
        if (_cts.IsCancellationRequested)
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        // Disable input controls during conversion
        SetControlsState(false);

        LogMessage("Starting batch conversion process...");
        LogMessage($"Using extract-xiso.exe: {extractXisoPath}");
        LogMessage($"Input folder: {inputFolder}");
        LogMessage($"Output folder: {outputFolder}");
        LogMessage($"Delete original files: {deleteFiles}");

        try
        {
            await PerformBatchConversionAsync(extractXisoPath, inputFolder, outputFolder, deleteFiles);
        }
        catch (OperationCanceledException)
        {
            LogMessage("Operation was canceled by user.");
        }
        catch (Exception ex)
        {
            LogMessage($"Error: {ex.Message}");
        }
        finally
        {
            // Re-enable input controls
            SetControlsState(true);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cts.Cancel();
        LogMessage("Cancellation requested. Waiting for current operation to complete...");
    }

    private void SetControlsState(bool enabled)
    {
        InputFolderTextBox.IsEnabled = enabled;
        OutputFolderTextBox.IsEnabled = enabled;
        BrowseInputButton.IsEnabled = enabled;
        BrowseOutputButton.IsEnabled = enabled;
        DeleteFilesCheckBox.IsEnabled = enabled;
        StartButton.IsEnabled = enabled;

        // Show/hide progress controls
        ProgressBar.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
        CancelButton.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
    }

    private static string? SelectFolder(string description)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = description
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private async Task PerformBatchConversionAsync(string extractXisoPath, string inputFolder, string outputFolder, bool deleteFiles)
    {
        try
        {
            LogMessage("Scanning input folder for ISO files...");

            // Get all ISO files in the input folder
            var isoFiles = Directory.GetFiles(inputFolder, "*.iso", SearchOption.TopDirectoryOnly);

            LogMessage($"Found {isoFiles.Length} ISO files to convert.");

            if (isoFiles.Length == 0)
            {
                LogMessage("No ISO files found in the input folder.");
                return;
            }

            ProgressBar.Maximum = isoFiles.Length;
            ProgressBar.Value = 0;

            var successCount = 0;
            var failureCount = 0;

            for (var i = 0; i < isoFiles.Length; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    LogMessage("Operation canceled by user.");
                    break;
                }

                var inputFile = isoFiles[i];
                var fileName = Path.GetFileName(inputFile);

                LogMessage($"[{i + 1}/{isoFiles.Length}] Converting: {fileName}");

                var success = await ConvertFileAsync(extractXisoPath, inputFile, outputFolder, deleteFiles);

                if (success)
                {
                    LogMessage($"Conversion successful: {fileName}");
                    successCount++;
                }
                else
                {
                    LogMessage($"Conversion failed: {fileName}");
                    failureCount++;
                }

                ProgressBar.Value = i + 1;
            }

            LogMessage("");
            LogMessage("Batch conversion completed.");
            LogMessage($"Successfully converted: {successCount} files");
            if (failureCount > 0)
            {
                LogMessage($"Failed to convert: {failureCount} files");
            }

            ShowMessageBox($"Batch conversion completed.\n\n" +
                           $"Successfully converted: {successCount} files\n" +
                           $"Failed to convert: {failureCount} files",
                "Conversion Complete", MessageBoxButton.OK,
                failureCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"Error during batch conversion: {ex.Message}");
            ShowError($"Error during batch conversion: {ex.Message}");
        }
    }

    private async Task<bool> ConvertFileAsync(string extractXisoPath, string inputFile, string outputFolder, bool deleteFiles)
    {
        try
        {
            // Step 1: Run the conversion tool
            var success = await RunConversionToolAsync(extractXisoPath, inputFile);
            if (!success)
            {
                return false;
            }

            // Step 2: Find and move the converted file
            var fileName = Path.GetFileName(inputFile);
            var convertedFilePath = await FindConvertedFileAsync(fileName);

            if (string.IsNullOrEmpty(convertedFilePath))
            {
                LogMessage($"Could not find converted file for: {fileName}");
                return false;
            }

            // Step 3: Move the file to the output folder
            var destinationPath = Path.Combine(outputFolder, fileName);
            LogMessage($"Moving converted file to output folder: {destinationPath}");

            await Task.Run(() =>
            {
                Directory.CreateDirectory(outputFolder);
                File.Move(convertedFilePath, destinationPath, true);
            }, _cts.Token);

            // Step 4: Handle the original file
            var renamedFilePath = inputFile + ".old";
            if (File.Exists(renamedFilePath))
            {
                if (deleteFiles)
                {
                    LogMessage($"Deleting original file: {fileName}");
                    await Task.Run(() => File.Delete(renamedFilePath), _cts.Token);
                }
                else
                {
                    LogMessage($"Restoring original file: {fileName}");
                    await Task.Run(() => File.Move(renamedFilePath, inputFile, true), _cts.Token);
                }
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogMessage($"Error processing file: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> RunConversionToolAsync(string extractXisoPath, string inputFile)
    {
        try
        {
            LogMessage($"Running extract-xiso on file: {inputFile}");

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = extractXisoPath,
                Arguments = $"-r \"{inputFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
            };

            // Setup output handling
            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    LogMessage($"extract-xiso: {args.Data}");
                }
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    LogMessage($"extract-xiso error: {args.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Register for cancellation
            var cancellationRegistration = _cts.Token.Register(() =>
            {
                // ReSharper disable once AccessToDisposedClosure
                if (!process.HasExited)
                {
                    try
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        process.Kill();
                    }
                    catch
                    {
                        // Ignore errors if the process already exited
                    }
                }
            });

            // Wait for the process to complete
            await Task.Run(() => process.WaitForExit(), _cts.Token);

            // Clean up registration
            await cancellationRegistration.DisposeAsync();

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            LogMessage($"Error during conversion: {ex.Message}");
            return false;
        }
    }

    private async Task<string> FindConvertedFileAsync(string fileName)
    {
        return await Task.Run(() =>
        {
            // Check potential locations
            var locations = new[]
            {
                Path.Combine(Environment.CurrentDirectory, fileName),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName)
            };

            foreach (var location in locations)
            {
                LogMessage($"Checking for converted file at: {location}");
                if (File.Exists(location))
                {
                    LogMessage($"Found converted file at: {location}");
                    return location;
                }
            }

            return string.Empty;
        }, _cts.Token);
    }

    private void ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        Dispatcher.Invoke(() =>
            MessageBox.Show(message, title, buttons, icon));
    }

    private void ShowError(string message)
    {
        ShowMessageBox(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
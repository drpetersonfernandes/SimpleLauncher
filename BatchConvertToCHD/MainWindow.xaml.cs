using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BatchConvertToCHD;

public partial class MainWindow
{
    private CancellationTokenSource _cts;

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        LogMessage("Welcome to the Batch Convert to CHD.");
        LogMessage("");
        LogMessage("This program will convert all [CUE], [ISO], and [IMG] files in the input folder to [CHD] files in the output folder.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the input folder containing files to convert");
        LogMessage("2. Select the output folder where CHD files will be saved");
        LogMessage("3. Choose whether to delete original files after conversion");
        LogMessage("4. Click 'Start Conversion' to begin the process");
        LogMessage("");

        // Verify chdman.exe exists
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var chdmanPath = Path.Combine(appDirectory, "chdman.exe");

        if (File.Exists(chdmanPath))
        {
            LogMessage("chdman.exe found in the application directory.");
        }
        else
        {
            LogMessage("WARNING: chdman.exe not found in the application directory!");
            LogMessage("Please ensure chdman.exe is in the same folder as this application.");
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
        var timestampedMessage = $"[{DateTime.Now}] {message}";

        Application.Current.Dispatcher.Invoke(() =>
        {
            LogViewer.AppendText($"{timestampedMessage}{Environment.NewLine}");
            LogViewer.ScrollToEnd();
        });
    }

    private void BrowseInputButton_Click(object sender, RoutedEventArgs e)
    {
        var inputFolder = SelectFolder("Select the folder containing files to convert");
        if (!string.IsNullOrEmpty(inputFolder))
        {
            InputFolderTextBox.Text = inputFolder;
            LogMessage($"Input folder selected: {inputFolder}");
        }
    }

    private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var outputFolder = SelectFolder("Select the output folder where CHD files will be saved");
        if (!string.IsNullOrEmpty(outputFolder))
        {
            OutputFolderTextBox.Text = outputFolder;
            LogMessage($"Output folder selected: {outputFolder}");
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var chdmanPath = Path.Combine(appDirectory, "chdman.exe");

        if (!File.Exists(chdmanPath))
        {
            LogMessage("Error: chdman.exe not found in the application folder.");
            ShowError("chdman.exe is missing from the application folder. Please ensure it's in the same directory as this application.");
            return;
        }

        var inputFolder = InputFolderTextBox.Text;
        var outputFolder = OutputFolderTextBox.Text;
        var deleteFiles = DeleteFilesCheckBox.IsChecked ?? false;

        if (string.IsNullOrEmpty(inputFolder))
        {
            LogMessage("Error: No input folder selected.");
            ShowError("Please select the input folder containing files to convert.");
            return;
        }

        if (string.IsNullOrEmpty(outputFolder))
        {
            LogMessage("Error: No output folder selected.");
            ShowError("Please select the output folder where CHD files will be saved.");
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
        LogMessage($"Using chdman.exe: {chdmanPath}");
        LogMessage($"Input folder: {inputFolder}");
        LogMessage($"Output folder: {outputFolder}");
        LogMessage($"Delete original files: {deleteFiles}");

        try
        {
            await PerformBatchConversionAsync(chdmanPath, inputFolder, outputFolder, deleteFiles);
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

    private async Task PerformBatchConversionAsync(string chdmanPath, string inputFolder, string outputFolder, bool deleteFiles)
    {
        try
        {
            LogMessage("Preparing for batch conversion...");

            // Restrict to .cue, .iso, and .img files only
            var supportedExtensions = new[] { ".cue", ".iso", ".img" };
            var files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToArray();

            LogMessage($"Found {files.Length} files to convert.");

            if (files.Length == 0)
            {
                LogMessage("No [CUE], [ISO], or [IMG] files found in the input folder.");
                return;
            }

            ProgressBar.Maximum = files.Length;
            ProgressBar.Value = 0;

            var successCount = 0;
            var failureCount = 0;

            for (var i = 0; i < files.Length; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    LogMessage("Operation canceled by user.");
                    break;
                }

                var inputFile = files[i];
                var fileName = Path.GetFileName(inputFile);
                var outputFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(inputFile) + ".chd");

                // Overwrite the output file if it exists
                if (File.Exists(outputFile))
                {
                    LogMessage($"Output file already exists, deleting: {outputFile}");
                    File.Delete(outputFile);
                }

                LogMessage($"[{i + 1}/{files.Length}] Converting: {fileName}");
                var success = await ConvertToChdAsync(chdmanPath, inputFile, outputFile);

                if (success)
                {
                    LogMessage($"Conversion successful: {fileName}");
                    successCount++;

                    if (deleteFiles)
                    {
                        try
                        {
                            // Delete the original input file
                            File.Delete(inputFile);
                            LogMessage($"Deleted original file: {fileName}");

                            // Check for .bin file (common with .cue files)
                            if (Path.GetExtension(inputFile).ToLower() == ".cue")
                            {
                                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
                                var binFileName = $"{fileNameWithoutExtension}.bin";
                                var binFileNameWithPath = Path.Combine(Path.GetDirectoryName(inputFile) ?? string.Empty, binFileName);

                                if (File.Exists(binFileNameWithPath))
                                {
                                    File.Delete(binFileNameWithPath);
                                    LogMessage($"Deleted associated .bin file: {binFileName}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Failed to delete original file: {fileName} - {ex.Message}");
                        }
                    }
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

    private Task<bool> ConvertToChdAsync(string chdmanPath, string inputFile, string outputFile)
    {
        return Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = chdmanPath,
                        Arguments = $"createcd -i \"{inputFile}\" -o \"{outputFile}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                // Read output to prevent the process from blocking
                process.StandardOutput.ReadToEnd();
                process.StandardError.ReadToEnd();

                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                LogMessage($"Error converting file: {ex.Message}");
                return false;
            }
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BatchConvertTo7z;

public partial class MainWindow
{
    private CancellationTokenSource _cts;

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        LogMessage("Welcome to the Batch Convert to 7z.");
        LogMessage("");
        LogMessage("This program will compress all files in the input folder to .7z format in the output folder.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the input folder containing files to compress");
        LogMessage("2. Select the output folder where 7z files will be saved");
        LogMessage("3. Choose whether to delete original files after compression");
        LogMessage("4. Click 'Start Compression' to begin the process");
        LogMessage("");

        // Verify 7z.exe exists
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var sevenZipPath = Path.Combine(appDirectory, "7z.exe");

        if (File.Exists(sevenZipPath))
        {
            LogMessage("7z.exe found in the application directory.");
        }
        else
        {
            LogMessage("WARNING: 7z.exe not found in the application directory!");
            LogMessage("Please ensure 7z.exe is in the same folder as this application.");
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
        var inputFolder = SelectFolder("Select the folder containing files to compress");
        if (!string.IsNullOrEmpty(inputFolder))
        {
            InputFolderTextBox.Text = inputFolder;
            LogMessage($"Input folder selected: {inputFolder}");
        }
    }

    private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var outputFolder = SelectFolder("Select the output folder where 7z files will be saved");
        if (!string.IsNullOrEmpty(outputFolder))
        {
            OutputFolderTextBox.Text = outputFolder;
            LogMessage($"Output folder selected: {outputFolder}");
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var sevenZipPath = Path.Combine(appDirectory, "7z.exe");

        if (!File.Exists(sevenZipPath))
        {
            LogMessage("Error: 7z.exe not found in the application folder.");
            ShowError("7z.exe is missing from the application folder. Please ensure it's in the same directory as this application.");
            return;
        }

        var inputFolder = InputFolderTextBox.Text;
        var outputFolder = OutputFolderTextBox.Text;
        var deleteFiles = DeleteFilesCheckBox.IsChecked ?? false;

        if (string.IsNullOrEmpty(inputFolder))
        {
            LogMessage("Error: No input folder selected.");
            ShowError("Please select the input folder containing files to compress.");
            return;
        }

        if (string.IsNullOrEmpty(outputFolder))
        {
            LogMessage("Error: No output folder selected.");
            ShowError("Please select the output folder where 7z files will be saved.");
            return;
        }

        // Reset cancellation token if it was previously used
        if (_cts.IsCancellationRequested)
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        // Disable input controls during compression
        SetControlsState(false);

        LogMessage("Starting batch compression process...");
        LogMessage($"Using 7z.exe: {sevenZipPath}");
        LogMessage($"Input folder: {inputFolder}");
        LogMessage($"Output folder: {outputFolder}");
        LogMessage($"Delete original files: {deleteFiles}");

        try
        {
            await PerformBatchCompressionAsync(sevenZipPath, inputFolder, outputFolder, deleteFiles);
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

    private async Task PerformBatchCompressionAsync(string sevenZipPath, string inputFolder, string outputFolder, bool deleteFiles)
    {
        try
        {
            LogMessage("Preparing for batch compression...");
            var files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly).ToArray();
            LogMessage($"Found {files.Length} files to compress.");

            if (files.Length == 0)
            {
                LogMessage("No files found in the input folder.");
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
                var fileName = Path.GetFileNameWithoutExtension(inputFile);
                var outputFile = Path.Combine(outputFolder, fileName + ".7z");

                if (File.Exists(outputFile))
                {
                    LogMessage($"Output file already exists, deleting: {outputFile}");
                    File.Delete(outputFile);
                }

                LogMessage($"[{i + 1}/{files.Length}] Compressing: {fileName}");
                var success = await CompressFileAsync(sevenZipPath, inputFile, outputFile);

                if (success)
                {
                    LogMessage($"Compression successful: {fileName}");
                    successCount++;

                    if (deleteFiles)
                    {
                        try
                        {
                            File.Delete(inputFile);
                            LogMessage($"Deleted original file: {fileName}");
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Failed to delete original file: {fileName} - {ex.Message}");
                        }
                    }
                }
                else
                {
                    LogMessage($"Compression failed: {fileName}");
                    failureCount++;
                }

                ProgressBar.Value = i + 1;
            }

            LogMessage("");
            LogMessage("Batch compression completed.");
            LogMessage($"Successfully compressed: {successCount} files");
            if (failureCount > 0)
            {
                LogMessage($"Failed to compress: {failureCount} files");
            }

            ShowMessageBox($"Batch compression completed.\n\n" +
                           $"Successfully compressed: {successCount} files\n" +
                           $"Failed to compress: {failureCount} files",
                "Compression Complete", MessageBoxButton.OK,
                failureCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"Error during batch compression: {ex.Message}");
            ShowError($"Error during batch compression: {ex.Message}");
        }
    }

    private Task<bool> CompressFileAsync(string sevenZipPath, string inputFile, string outputFile)
    {
        return Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = sevenZipPath,
                        Arguments = $"a \"{outputFile}\" \"{inputFile}\"",
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
                LogMessage($"Error compressing file: {ex.Message}");
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
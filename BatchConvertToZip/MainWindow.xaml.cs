using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace BatchConvertToZip;

public partial class MainWindow
{
    private CancellationTokenSource _cts;
    private readonly BugReportService _bugReportService;

    // Bug Report API configuration
    private const string BugReportApiUrl = "http://localhost:5116/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "BatchConvertToZip";

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        LogMessage("Welcome to the Batch Convert to Zip.");
        LogMessage("");
        LogMessage("This program will compress all files in the input folder to .zip format in the output folder.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the input folder containing files to compress");
        LogMessage("2. Select the output folder where zip files will be saved");
        LogMessage("3. Choose whether to delete original files after compression");
        LogMessage("4. Click 'Start Compression' to begin the process");
        LogMessage("");
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
        var outputFolder = SelectFolder("Select the output folder where zip files will be saved");
        if (!string.IsNullOrEmpty(outputFolder))
        {
            OutputFolderTextBox.Text = outputFolder;
            LogMessage($"Output folder selected: {outputFolder}");
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        var inputFolder = InputFolderTextBox.Text;
        var outputFolder = OutputFolderTextBox.Text;
        var deleteFiles = DeleteFilesCheckBox.IsChecked ?? false;

        if (string.IsNullOrEmpty(inputFolder))
        {
            LogMessage("Error: No input folder selected.");
            ShowError("Please select the input folder containing files to compress.");
            return;
        }

        if (!Directory.Exists(inputFolder))
        {
            LogMessage($"Error: Input folder does not exist: {inputFolder}");
            ShowError("The selected input folder does not exist.");
            await ReportBugAsync($"Input folder does not exist: {inputFolder}");
            return;
        }

        if (string.IsNullOrEmpty(outputFolder))
        {
            LogMessage("Error: No output folder selected.");
            ShowError("Please select the output folder where zip files will be saved.");
            return;
        }

        try
        {
            // Ensure output directory exists
            if (!Directory.Exists(outputFolder))
            {
                LogMessage($"Creating output directory: {outputFolder}");
                Directory.CreateDirectory(outputFolder);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error creating output directory: {ex.Message}");
            ShowError($"Failed to create output directory: {ex.Message}");
            await ReportBugAsync("Failed to create output directory", ex);
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
        LogMessage($"Input folder: {inputFolder}");
        LogMessage($"Output folder: {outputFolder}");
        LogMessage($"Delete original files: {deleteFiles}");

        try
        {
            await PerformBatchCompressionAsync(inputFolder, outputFolder, deleteFiles);
        }
        catch (OperationCanceledException)
        {
            LogMessage("Operation was canceled by user.");
        }
        catch (Exception ex)
        {
            LogMessage($"Error: {ex.Message}");

            // Report the exception to our bug reporting service
            await ReportBugAsync("Error during batch compression process", ex);
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
        var dialog = new OpenFolderDialog
        {
            Title = description
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private async Task PerformBatchCompressionAsync(string inputFolder, string outputFolder, bool deleteFiles)
    {
        try
        {
            LogMessage("Preparing for batch compression...");

            // Get all files in the input folder
            string[] files;
            try
            {
                files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly).ToArray();
            }
            catch (Exception ex)
            {
                LogMessage($"Error accessing input folder: {ex.Message}");
                await ReportBugAsync("Error accessing input folder for file listing", ex);
                return;
            }

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
                var outputFile = Path.Combine(outputFolder, fileName + ".zip");

                if (File.Exists(outputFile))
                {
                    LogMessage($"Output file already exists, deleting: {outputFile}");
                    try
                    {
                        File.Delete(outputFile);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Failed to delete existing output file: {ex.Message}");
                        await ReportBugAsync($"Failed to delete existing output file: {outputFile}", ex);
                        failureCount++;
                        continue;
                    }
                }

                LogMessage($"[{i + 1}/{files.Length}] Compressing: {fileName}");
                var success = await CompressFileToZipAsync(inputFile, outputFile);

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
                            await ReportBugAsync($"Failed to delete original file after compression: {fileName}", ex);
                        }
                    }
                }
                else
                {
                    LogMessage($"Compression failed: {fileName}");
                    failureCount++;

                    // Report compression failure
                    await ReportBugAsync($"Failed to compress file: {fileName}");
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

            // Report the exception
            await ReportBugAsync("Error during batch compression operation", ex);
        }
    }

    private async Task<bool> CompressFileToZipAsync(string inputFile, string outputFile)
    {
        try
        {
            await Task.Run(() =>
            {
                using var zipArchive = ZipFile.Open(outputFile, ZipArchiveMode.Create);
                zipArchive.CreateEntryFromFile(inputFile, Path.GetFileName(inputFile), CompressionLevel.Optimal);
            }, _cts.Token);

            return true;
        }
        catch (Exception ex)
        {
            LogMessage($"Error compressing file: {ex.Message}");

            // Report this specific file compression error
            await ReportBugAsync($"Error compressing file: {Path.GetFileName(inputFile)}", ex);
            return false;
        }
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

    /// <summary>
    /// Silently reports bugs/errors to the API
    /// </summary>
    private async Task ReportBugAsync(string message, Exception? exception = null)
    {
        try
        {
            var fullReport = new StringBuilder();

            // Add system information
            fullReport.AppendLine("=== Bug Report ===");
            fullReport.AppendLine($"Application: {ApplicationName}");
            fullReport.AppendLine($"Version: {GetType().Assembly.GetName().Version}");
            fullReport.AppendLine($"OS: {Environment.OSVersion}");
            fullReport.AppendLine($".NET Version: {Environment.Version}");
            fullReport.AppendLine($"Date/Time: {DateTime.Now}");
            fullReport.AppendLine();

            // Add a message
            fullReport.AppendLine("=== Error Message ===");
            fullReport.AppendLine(message);
            fullReport.AppendLine();

            // Add exception details if available
            if (exception != null)
            {
                fullReport.AppendLine("=== Exception Details ===");
                fullReport.AppendLine($"Type: {exception.GetType().FullName}");
                fullReport.AppendLine($"Message: {exception.Message}");
                fullReport.AppendLine($"Source: {exception.Source}");
                fullReport.AppendLine("Stack Trace:");
                fullReport.AppendLine(exception.StackTrace);

                // Add inner exception if available
                if (exception.InnerException != null)
                {
                    fullReport.AppendLine("Inner Exception:");
                    fullReport.AppendLine($"Type: {exception.InnerException.GetType().FullName}");
                    fullReport.AppendLine($"Message: {exception.InnerException.Message}");
                    fullReport.AppendLine($"Stack Trace:");
                    fullReport.AppendLine(exception.InnerException.StackTrace);
                }
            }

            // Add log contents if available
            if (LogViewer != null)
            {
                var logContent = string.Empty;

                // Safely get log content from UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    logContent = LogViewer.Text;
                });

                if (!string.IsNullOrEmpty(logContent))
                {
                    fullReport.AppendLine();
                    fullReport.AppendLine("=== Application Log ===");
                    fullReport.Append(logContent);
                }
            }

            // Silently send the report
            await _bugReportService.SendBugReportAsync(fullReport.ToString());
        }
        catch
        {
            // Silently fail if error reporting itself fails
        }
    }
}
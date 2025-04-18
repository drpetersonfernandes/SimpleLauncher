using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace BatchConvertToCompressedFile;

public partial class MainWindow : IDisposable
{
    private CancellationTokenSource _cts;
    private readonly BugReportService _bugReportService;
    private readonly string _sevenZipPath; // Path to the appropriate 7z executable

    // Bug Report API configuration
    private const string BugReportApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "BatchConvertToCompressedFile";

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        LogMessage("Welcome to the Batch Convert to Compressed File.");
        LogMessage("");
        LogMessage("This program will compress all files in the input folder to .7z or .zip format in the output folder.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the input folder containing files to compress");
        LogMessage("2. Select the output folder where the compressed files will be saved");
        LogMessage("3. Choose the compression format (.7z or .zip)");
        LogMessage("4. Choose whether to delete original files after compression");
        LogMessage("5. Click 'Start Compression' to begin the process");
        LogMessage("");

        // Determine and verify the appropriate 7z executable
        _sevenZipPath = GetAppropriateSevenZipPath();

        if (File.Exists(_sevenZipPath))
        {
            LogMessage($"Using {Path.GetFileName(_sevenZipPath)} for {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")} system.");
        }
        else
        {
            LogMessage($"WARNING: {Path.GetFileName(_sevenZipPath)} not found in the application directory!");
            LogMessage("Please ensure the required 7z executables are in the same folder as this application.");

            // Report this as a potential issue
            Task.Run(async () => await ReportBugAsync($"{Path.GetFileName(_sevenZipPath)} not found in the application directory. This will prevent the application from functioning correctly."));
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
        if (string.IsNullOrEmpty(inputFolder)) return;

        InputFolderTextBox.Text = inputFolder;
        LogMessage($"Input folder selected: {inputFolder}");
    }

    private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var outputFolder = SelectFolder("Select the output folder where compressed files will be saved");
        if (string.IsNullOrEmpty(outputFolder)) return;

        OutputFolderTextBox.Text = outputFolder;
        LogMessage($"Output folder selected: {outputFolder}");
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!File.Exists(_sevenZipPath))
            {
                LogMessage($"Error: {Path.GetFileName(_sevenZipPath)} not found in the application folder.");
                ShowError($"{Path.GetFileName(_sevenZipPath)} is missing from the application folder. Please ensure it's in the same directory as this application.");

                // Report this issue
                await ReportBugAsync($"{Path.GetFileName(_sevenZipPath)} not found when trying to start compression",
                    new FileNotFoundException($"The required {Path.GetFileName(_sevenZipPath)} file was not found.", _sevenZipPath));
                return;
            }

            var inputFolder = InputFolderTextBox.Text;
            var outputFolder = OutputFolderTextBox.Text;
            var deleteFiles = DeleteFilesCheckBox.IsChecked ?? false;
            var use7ZFormat = SevenZipRadioButton.IsChecked ?? true;
            var compressionFormat = use7ZFormat ? "7z" : "zip";

            if (string.IsNullOrEmpty(inputFolder))
            {
                LogMessage("Error: No input folder selected.");
                ShowError("Please select the input folder containing files to compress.");
                return;
            }

            if (string.IsNullOrEmpty(outputFolder))
            {
                LogMessage("Error: No output folder selected.");
                ShowError("Please select the output folder where compressed files will be saved.");
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
            LogMessage($"Using {Path.GetFileName(_sevenZipPath)}: {_sevenZipPath}");
            LogMessage($"Input folder: {inputFolder}");
            LogMessage($"Output folder: {outputFolder}");
            LogMessage($"Compression format: {compressionFormat}");
            LogMessage($"Delete original files: {deleteFiles}");

            try
            {
                await PerformBatchCompressionAsync(_sevenZipPath, inputFolder, outputFolder, compressionFormat, deleteFiles);
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
        catch (Exception ex)
        {
            await ReportBugAsync("Error during batch compression process", ex);
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
        SevenZipRadioButton.IsEnabled = enabled;
        ZipRadioButton.IsEnabled = enabled;

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

    private async Task PerformBatchCompressionAsync(
        string sevenZipPath,
        string inputFolder,
        string outputFolder,
        string compressionFormat,
        bool deleteFiles)
    {
        try
        {
            LogMessage("Preparing for batch compression...");
            var files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly);
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
                var outputFile = Path.Combine(outputFolder, fileName + "." + compressionFormat);

                if (File.Exists(outputFile))
                {
                    LogMessage($"Output file already exists, deleting: {outputFile}");
                    File.Delete(outputFile);
                }

                LogMessage($"[{i + 1}/{files.Length}] Compressing: {fileName} to {compressionFormat} format");
                var success = await CompressFileAsync(sevenZipPath, inputFile, outputFile, compressionFormat);

                if (success)
                {
                    LogMessage($"Compression successful: {fileName}.{compressionFormat}");
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

                            // Report the file deletion error
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

    private async Task<bool> CompressFileAsync(
        string sevenZipPath,
        string inputFile,
        string outputFile,
        string compressionFormat)
    {
        try
        {
            // Build the arguments for 7z.exe - same command 'a' (add) works for both formats
            // The archive type is determined by the extension of the output file
            var arguments = $"a \"{outputFile}\" \"{inputFile}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            // Read output to prevent the process from blocking
            await process.StandardOutput.ReadToEndAsync();
            await process.StandardError.ReadToEndAsync();

            await Task.Run(() => process.WaitForExit(), _cts.Token);

            return process.ExitCode == 0;
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
            fullReport.AppendLine(CultureInfo.InvariantCulture, $"Version: {GetType().Assembly.GetName().Version}");
            fullReport.AppendLine(CultureInfo.InvariantCulture, $"OS: {Environment.OSVersion}");
            fullReport.AppendLine(CultureInfo.InvariantCulture, $".NET Version: {Environment.Version}");
            fullReport.AppendLine(CultureInfo.InvariantCulture, $"Date/Time: {DateTime.Now}");
            fullReport.AppendLine();

            // Add a message
            fullReport.AppendLine("=== Error Message ===");
            fullReport.AppendLine(message);
            fullReport.AppendLine();

            // Add exception details if available
            if (exception != null)
            {
                fullReport.AppendLine("=== Exception Details ===");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.GetType().FullName}");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.Message}");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Source: {exception.Source}");
                fullReport.AppendLine("Stack Trace:");
                fullReport.AppendLine(exception.StackTrace);

                // Add inner exception if available
                if (exception.InnerException != null)
                {
                    fullReport.AppendLine("Inner Exception:");
                    fullReport.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.InnerException.GetType().FullName}");
                    fullReport.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.InnerException.Message}");
                    fullReport.AppendLine("Stack Trace:");
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

    /// <summary>
    /// Determines the appropriate 7z executable path based on the system architecture
    /// </summary>
    /// <returns>The path to the appropriate 7z executable</returns>
    private string GetAppropriateSevenZipPath()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Check if we're running on a 64-bit operating system
        if (Environment.Is64BitOperatingSystem)
        {
            return Path.Combine(appDirectory, "7z.exe");
        }
        else
        {
            return Path.Combine(appDirectory, "7z_x86.exe");
        }
    }

    public void Dispose()
    {
        // Dispose of the cancellation token source
        _cts?.Dispose();
        // _cts = null;

        // Dispose of the bug report service if it implements IDisposable
        if (_bugReportService is IDisposable bugReportService)
        {
            bugReportService.Dispose();
        }

        // Suppress finalization for better performance
        GC.SuppressFinalize(this);
    }
}
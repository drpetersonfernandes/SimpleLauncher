using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace BatchConvertToCHD;

public partial class MainWindow
{
    private CancellationTokenSource _cts;
    private readonly BugReportService _bugReportService;

    // Bug Report API configuration
    private const string BugReportApiUrl = "http://localhost:5116/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "BatchConvertToCHD";

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

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

            // Report this as a potential issue
            Task.Run(async () => await ReportBugAsync("chdman.exe not found in the application directory. This will prevent the application from functioning correctly."));
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

            // Report this issue
            await ReportBugAsync("chdman.exe not found when trying to start conversion",
                new FileNotFoundException("The required chdman.exe file was not found.", chdmanPath));
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
        catch (OperationCanceledException)
        {
            LogMessage("Operation was canceled by user.");
        }
        catch (Exception ex)
        {
            LogMessage($"Error: {ex.Message}");

            // Report the exception to our bug reporting service
            await ReportBugAsync("Error during batch conversion process", ex);
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
                            // For .cue files, get all referenced files first
                            var filesToDelete = new List<string>();

                            if (Path.GetExtension(inputFile).ToLower() == ".cue")
                            {
                                // Get all files referenced in the .cue file
                                var referencedFiles = GetReferencedFilesFromCue(inputFile);

                                if (referencedFiles.Any())
                                {
                                    LogMessage($"Found {referencedFiles.Count} referenced file(s) in CUE file.");
                                    filesToDelete.AddRange(referencedFiles);
                                }
                            }

                            // Always add the original file to be deleted
                            filesToDelete.Add(inputFile);

                            // Delete all files
                            foreach (var fileToDelete in filesToDelete)
                            {
                                if (File.Exists(fileToDelete))
                                {
                                    File.Delete(fileToDelete);
                                    LogMessage($"Deleted file: {Path.GetFileName(fileToDelete)}");
                                }
                                else
                                {
                                    LogMessage($"Warning: Referenced file not found: {Path.GetFileName(fileToDelete)}");

                                    // Report missing referenced file
                                    await ReportBugAsync($"Referenced file not found during deletion: {Path.GetFileName(fileToDelete)}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Failed to delete original file(s): {ex.Message}");

                            // Report file deletion error
                            await ReportBugAsync($"Failed to delete original file(s) after conversion: {fileName}", ex);
                        }
                    }
                }
                else
                {
                    LogMessage($"Conversion failed: {fileName}");
                    failureCount++;

                    // Report conversion failure
                    await ReportBugAsync($"Failed to convert file: {fileName}");
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

            // Report the exception
            await ReportBugAsync("Error during batch conversion operation", ex);
        }
    }

    private List<string> GetReferencedFilesFromCue(string cuePath)
    {
        var referencedFiles = new List<string>();
        var cueDir = Path.GetDirectoryName(cuePath) ?? string.Empty;

        try
        {
            LogMessage($"Parsing CUE file to find referenced files: {Path.GetFileName(cuePath)}");
            var lines = File.ReadAllLines(cuePath);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("FILE ", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract filename from the FILE line
                    // Format: FILE "filename.ext" FILETYPE
                    var parts = trimmedLine.Split('"');
                    if (parts.Length >= 2)
                    {
                        var fileName = parts[1];
                        var filePath = Path.Combine(cueDir, fileName);

                        LogMessage($"Found referenced file in CUE: {fileName}");
                        referencedFiles.Add(filePath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error parsing CUE file: {ex.Message}");

            // Report CUE parsing error
            Task.Run(async () => await ReportBugAsync($"Error parsing CUE file: {Path.GetFileName(cuePath)}", ex));
        }

        return referencedFiles;
    }

    private async Task<bool> ConvertToChdAsync(string chdmanPath, string inputFile, string outputFile)
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
            await process.StandardOutput.ReadToEndAsync();
            await process.StandardError.ReadToEndAsync();

            await Task.Run(() => process.WaitForExit(), _cts.Token);

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            LogMessage($"Error converting file: {ex.Message}");

            // Report this specific file conversion error
            await ReportBugAsync($"Error converting file: {Path.GetFileName(inputFile)}", ex);
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
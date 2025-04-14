using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace BatchConvertIsoToXiso;

public partial class MainWindow : IDisposable
{
    private CancellationTokenSource _cts;
    private readonly BugReportService _bugReportService;

    // Bug Report API configuration
    private const string BugReportApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "BatchConvertIsoToXiso";

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

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

            // Report this as a potential issue
            Task.Run(async () => await ReportBugAsync("extract-xiso.exe not found in the application directory. This will prevent the application from functioning correctly."));
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
        if (string.IsNullOrEmpty(inputFolder)) return;

        InputFolderTextBox.Text = inputFolder;
        LogMessage($"Input folder selected: {inputFolder}");
    }

    private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var outputFolder = SelectFolder("Select the output folder where converted XISO files will be saved");
        if (string.IsNullOrEmpty(outputFolder)) return;

        OutputFolderTextBox.Text = outputFolder;
        LogMessage($"Output folder selected: {outputFolder}");
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var extractXisoPath = Path.Combine(appDirectory, "extract-xiso.exe");

            if (!File.Exists(extractXisoPath))
            {
                LogMessage("Error: extract-xiso.exe not found in the application folder.");
                ShowError("extract-xiso.exe is missing from the application folder. Please ensure it's in the same directory as this application.");

                // Report this issue
                await ReportBugAsync("extract-xiso.exe not found when trying to start conversion",
                    new FileNotFoundException("The required extract-xiso.exe file was not found.", extractXisoPath));

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

                // Report the exception to our bug reporting service
                await ReportBugAsync("Error during batch conversion process", ex);
            }
            finally
            {
                // Re-enable input controls
                SetControlsState(true);
            }
        }
        catch (Exception ex)
        {
            await ReportBugAsync("Error during batch conversion process", ex);
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
            if (!File.Exists(renamedFilePath)) return true;

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

            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            LogMessage($"Error processing file: {ex.Message}");

            // Report this specific file processing error
            await ReportBugAsync($"Error processing file: {Path.GetFileName(inputFile)}", ex);
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
                if (process.HasExited) return;

                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    process.Kill();
                }
                catch
                {
                    // Ignore errors if the process already exited
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

            // Report this error
            await ReportBugAsync($"Error running extract-xiso on file: {Path.GetFileName(inputFile)}", ex);
            return false;
        }
    }

    private Task<string> FindConvertedFileAsync(string fileName)
    {
        return Task.Run(() =>
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
                if (!File.Exists(location)) continue;

                LogMessage($"Found converted file at: {location}");
                return location;
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

    public void Dispose()
    {
        // Cancel any ongoing operations
        if (true)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null!;
        }

        // Dispose the bug report service
        _bugReportService?.Dispose();

        // Suppress finalization
        GC.SuppressFinalize(this);
    }
}
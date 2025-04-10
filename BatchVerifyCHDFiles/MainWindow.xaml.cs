using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace BatchVerifyCHDFiles;

public partial class MainWindow : IDisposable
{
    private CancellationTokenSource _cts;
    private readonly BugReportService _bugReportService;
    private int _totalFiles;
    private int _verifiedOkCount;
    private int _failedCount;
    private readonly Stopwatch _processingTimer = new();

    // Bug Report API configuration
    private const string BugReportApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "BatchVerifyCHDFiles";
    private static readonly char[] Separator = new[] { '\r', '\n' };

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        LogMessage("Welcome to Batch Verify CHD Files.");
        LogMessage("");
        LogMessage("This program will verify the integrity of all CHD files in the selected folder.");
        LogMessage("It will check each file's structure and validate its data against internal checksums.");
        LogMessage("");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the folder containing CHD files to verify");
        LogMessage("2. Choose whether to include subfolders in the search");
        LogMessage("3. Click 'Start Verification' to begin the process");
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
        var timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

        Application.Current.Dispatcher.Invoke((Action)(() =>
        {
            LogViewer.AppendText($"{timestampedMessage}{Environment.NewLine}");
            LogViewer.ScrollToEnd();
        }));
    }

    private void DisplayFileInfo(string info)
    {
        Application.Current.Dispatcher.Invoke((Action)(() =>
        {
            FileInfoViewer.Text = info;
        }));
    }

    private void BrowseInputButton_Click(object sender, RoutedEventArgs e)
    {
        var inputFolder = SelectFolder("Select the folder containing CHD files to verify");
        if (!string.IsNullOrEmpty(inputFolder))
        {
            InputFolderTextBox.Text = inputFolder;
            LogMessage($"Input folder selected: {inputFolder}");
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
            await ReportBugAsync("chdman.exe not found when trying to start verification",
                new FileNotFoundException("The required chdman.exe file was not found.", chdmanPath));
            return;
        }

        var inputFolder = InputFolderTextBox.Text;
        var includeSubfolders = IncludeSubfoldersCheckBox.IsChecked ?? false;

        if (string.IsNullOrEmpty(inputFolder))
        {
            LogMessage("Error: No input folder selected.");
            ShowError("Please select the input folder containing CHD files to verify.");
            return;
        }

        // Reset cancellation token if it was previously used
        if (_cts.IsCancellationRequested)
        {
            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        // Reset counters
        _totalFiles = 0;
        _verifiedOkCount = 0;
        _failedCount = 0;
        UpdateCounters();

        // Clear file info
        DisplayFileInfo("");

        // Disable input controls during verification
        SetControlsState(false);

        LogMessage("Starting batch verification process...");
        LogMessage($"Using chdman.exe: {chdmanPath}");
        LogMessage($"Input folder: {inputFolder}");
        LogMessage($"Include subfolders: {includeSubfolders}");

        // Start timer
        _processingTimer.Restart();

        try
        {
            await PerformBatchVerificationAsync(chdmanPath, inputFolder, includeSubfolders);
        }
        catch (OperationCanceledException)
        {
            LogMessage("Operation was canceled by user.");
        }
        catch (Exception ex)
        {
            LogMessage($"Error: {ex.Message}");

            // Report the exception to our bug reporting service
            await ReportBugAsync("Error during batch verification process", ex);
        }
        finally
        {
            // Stop timer
            _processingTimer.Stop();
            UpdateProcessingTime();

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
        BrowseInputButton.IsEnabled = enabled;
        IncludeSubfoldersCheckBox.IsEnabled = enabled;
        StartButton.IsEnabled = enabled;

        // Show/hide progress controls
        ProgressBar.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
        BatchProgressText.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
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

    private async Task PerformBatchVerificationAsync(string chdmanPath, string inputFolder, bool includeSubfolders)
    {
        try
        {
            LogMessage("Searching for CHD files...");

            // Find all CHD files in the input folder
            var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(inputFolder, "*.chd", searchOption);

            _totalFiles = files.Length;
            UpdateCounters();

            LogMessage($"Found {files.Length} CHD files to verify.");

            if (files.Length == 0)
            {
                LogMessage("No CHD files found in the specified folder.");
                return;
            }

            // Setup progress tracking
            ProgressBar.Maximum = files.Length;
            ProgressBar.Value = 0;

            for (var i = 0; i < files.Length; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    LogMessage("Operation canceled by user.");
                    break;
                }

                var chdFile = files[i];
                var fileName = Path.GetFileName(chdFile);

                // Show progress information
                UpdateProgressStatus(i + 1, files.Length, fileName);
                LogMessage($"[{i + 1}/{files.Length}] Verifying: {fileName}");

                // Get the file info first
                var fileInfo = await GetChdInfoAsync(chdmanPath, chdFile);
                DisplayFileInfo(fileInfo);

                // Verify the file integrity
                var isValid = await VerifyChdAsync(chdmanPath, chdFile);

                if (isValid)
                {
                    LogMessage($"✓ Verification successful: {fileName}");
                    _verifiedOkCount++;
                }
                else
                {
                    LogMessage($"✗ Verification failed: {fileName}");
                    _failedCount++;
                }

                UpdateCounters();
                UpdateProcessingTime();

                // Update progress
                ProgressBar.Value = i + 1;
            }

            LogMessage("");
            LogMessage("Batch verification completed.");
            LogMessage($"Total files: {_totalFiles}");
            LogMessage($"Successfully verified: {_verifiedOkCount} files");
            if (_failedCount > 0)
            {
                LogMessage($"Failed verification: {_failedCount} files");
            }

            ShowMessageBox($"Batch verification completed.\n\n" +
                           $"Total files: {_totalFiles}\n" +
                           $"Successfully verified: {_verifiedOkCount} files\n" +
                           $"Failed verification: {_failedCount} files",
                "Verification Complete", MessageBoxButton.OK,
                _failedCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            LogMessage($"Error during batch verification: {ex.Message}");
            ShowError($"Error during batch verification: {ex.Message}");
            await ReportBugAsync("Error during batch verification operation", ex);
        }
    }

    private void UpdateProgressStatus(int current, int total, string currentFile)
    {
        // Calculate percentage for the overall batch
        var percentage = (double)current / total * 100;

        // Update UI elements on the UI thread
        Application.Current.Dispatcher.Invoke((Action)(() =>
        {
            // Update progress bar values
            ProgressBar.Value = current;
            ProgressBar.Maximum = total;

            // Update batch progress text
            BatchProgressText.Text = $"Overall Progress: {current} of {total} files ({percentage:F1}%)";
        }));
    }

    private async Task<bool> VerifyChdAsync(string chdmanPath, string chdFile)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = chdmanPath,
                    Arguments = $"verify -i \"{chdFile}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            // Set up output handling
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    outputBuilder.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    // Check if this is a progress update rather than an actual error
                    if (args.Data.Contains("Verifying,") && args.Data.Contains("% complete"))
                    {
                        // This is a progress update, not an error
                        // Log it with a better prefix
                        LogMessage($"{args.Data}");
                    }
                    else
                    {
                        // This is an actual error
                        errorBuilder.AppendLine(args.Data);
                        LogMessage($"[ERROR] {args.Data}");
                    }
                }
            };

            // Start the process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit(), _cts.Token);

            // CHD verification is successful if the process exits with code 0
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            LogMessage($"Error verifying file: {ex.Message}");
            await ReportBugAsync($"Error verifying file: {Path.GetFileName(chdFile)}", ex);
            return false;
        }
    }

    private async Task<string> GetChdInfoAsync(string chdmanPath, string chdFile)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = chdmanPath,
                    Arguments = $"info -i \"{chdFile}\" -v",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            // Set up output handling
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    outputBuilder.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    errorBuilder.AppendLine(args.Data);
                }
            };

            // Start the process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit(), _cts.Token);

            // Format the CHD info for display
            var formattedInfo = FormatChdInfo(chdFile, outputBuilder.ToString());
            return formattedInfo;
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting file info: {ex.Message}");
            await ReportBugAsync($"Error getting file info: {Path.GetFileName(chdFile)}", ex);
            return $"Error getting file info: {ex.Message}";
        }
    }

    private static string FormatChdInfo(string chdFile, string rawInfo)
    {
        var sb = new StringBuilder();

        // Add file path
        sb.AppendLine(CultureInfo.InvariantCulture, $"File: {chdFile}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Size: {FormatFileSize(new FileInfo(chdFile).Length)}");
        sb.AppendLine(new string('-', 40));

        // Process the raw info from chdman
        var lines = rawInfo.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        // Skip the first line which is usually the chdman version
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];

            // Skip horizontal separator lines
            if (line.Contains("----------"))
                continue;

            // Add the info line
            sb.AppendLine(line);
        }

        return sb.ToString();
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        var counter = 0;
        decimal number = bytes;

        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }

        return $"{number:n2} {suffixes[counter]}";
    }

    private void UpdateCounters()
    {
        Application.Current.Dispatcher.Invoke((Action)(() =>
        {
            TotalFilesValue.Text = _totalFiles.ToString(CultureInfo.InvariantCulture);
            VerifiedOkValue.Text = _verifiedOkCount.ToString(CultureInfo.InvariantCulture);
            FailedValue.Text = _failedCount.ToString(CultureInfo.InvariantCulture);
        }));
    }

    private void UpdateProcessingTime()
    {
        var elapsed = _processingTimer.Elapsed;
        Application.Current.Dispatcher.Invoke((Action)(() =>
        {
            ProcessingTimeValue.Text = $"{elapsed:hh\\:mm\\:ss}";
        }));
    }

    private void ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        Dispatcher.Invoke((Action)(() =>
            MessageBox.Show(message, title, buttons, icon)));
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

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this // Set the owner to center the About window relative to MainWindow
            };
            aboutWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            _ = ReportBugAsync("Error opening About window", ex);
        }
    }

    public void Dispose()
    {
        // Dispose the cancellation token source
        _cts?.Dispose();
        _cts = null;

        // Dispose the bug report service
        _bugReportService?.Dispose();

        // Stop the processing timer if it's running
        if (_processingTimer.IsRunning)
        {
            _processingTimer.Stop();
        }

        // Unregister any event handlers if needed
        // (none appear to be explicitly registered in the provided code)
    }
}
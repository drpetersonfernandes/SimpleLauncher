using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace BatchVerifyCompressedFiles;

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
    private const string ApplicationName = "BatchVerifyCompressedFiles";
    private static readonly char[] Separator = new[] { '\r', '\n' };

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        LogMessage("Welcome to Batch Verify Compressed Files.");
        LogMessage("");
        LogMessage("This program will verify the integrity of compressed files in the selected folder.");
        LogMessage("It will check each file's structure and validate its data.");
        LogMessage("");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the folder containing compressed files to verify");
        LogMessage("2. Choose which file types to verify (ZIP, 7Z, RAR)");
        LogMessage("3. Choose whether to include subfolders in the search");
        LogMessage("4. Click 'Start Verification' to begin the process");
        LogMessage("");

        // Determine the appropriate 7-Zip executable based on system architecture
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var sevenZipPath = GetSevenZipExecutablePath(appDirectory);

        if (File.Exists(sevenZipPath))
        {
            LogMessage($"{Path.GetFileName(sevenZipPath)} found in the application directory.");
            LogMessage($"Running in {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")} mode.");
        }
        else
        {
            LogMessage($"WARNING: {Path.GetFileName(sevenZipPath)} not found in the application directory!");
            LogMessage("Please ensure the appropriate 7-Zip executable is in the same folder as this application.");

            // Report this as a potential issue
            Task.Run(async () => await ReportBugAsync($"{Path.GetFileName(sevenZipPath)} not found in the application directory. This will prevent the application from functioning correctly."));
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
        var inputFolder = SelectFolder("Select the folder containing compressed files to verify");
        if (!string.IsNullOrEmpty(inputFolder))
        {
            InputFolderTextBox.Text = inputFolder;
            LogMessage($"Input folder selected: {inputFolder}");
        }
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var sevenZipPath = GetSevenZipExecutablePath(appDirectory);
        var executableName = Path.GetFileName(sevenZipPath);

        if (!File.Exists(sevenZipPath))
        {
            LogMessage($"Error: {executableName} not found in the application folder.");
            ShowError($"{executableName} is missing from the application folder. Please ensure it's in the same directory as this application.");

            // Report this issue
            await ReportBugAsync($"{executableName} not found when trying to start verification",
                new FileNotFoundException($"The required {executableName} file was not found.", sevenZipPath));
            return;
        }

        var inputFolder = InputFolderTextBox.Text;
        var includeSubfolders = IncludeSubfoldersCheckBox.IsChecked ?? false;

        // Check which file types to verify
        var verifyZip = ZipFilesCheckBox.IsChecked ?? false;
        var verifySevenZip = SevenZipFilesCheckBox.IsChecked ?? false;
        var verifyRar = RarFilesCheckBox.IsChecked ?? false;

        if (!verifyZip && !verifySevenZip && !verifyRar)
        {
            LogMessage("Error: No file types selected for verification.");
            ShowError("Please select at least one file type (ZIP, 7Z, or RAR) to verify.");
            return;
        }

        if (string.IsNullOrEmpty(inputFolder))
        {
            LogMessage("Error: No input folder selected.");
            ShowError("Please select the input folder containing compressed files to verify.");
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
        LogMessage($"Using {Path.GetFileName(sevenZipPath)}: {sevenZipPath}");
        LogMessage($"Input folder: {inputFolder}");
        LogMessage($"Include subfolders: {includeSubfolders}");
        LogMessage($"File types: " +
                   (verifyZip ? "ZIP " : "") +
                   (verifySevenZip ? "7Z " : "") +
                   (verifyRar ? "RAR" : ""));

        // Start timer
        _processingTimer.Restart();

        try
        {
            await PerformBatchVerificationAsync(sevenZipPath, inputFolder, includeSubfolders,
                verifyZip, verifySevenZip, verifyRar);
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
        ZipFilesCheckBox.IsEnabled = enabled;
        SevenZipFilesCheckBox.IsEnabled = enabled;
        RarFilesCheckBox.IsEnabled = enabled;
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

    private async Task PerformBatchVerificationAsync(string sevenZipPath, string inputFolder, bool includeSubfolders,
        bool verifyZip, bool verifySevenZip, bool verifyRar)
    {
        try
        {
            LogMessage("Searching for compressed files...");

            // Create a search pattern based on selected file types
            var fileExtensions = new List<string>();
            if (verifyZip) fileExtensions.Add("*.zip");
            if (verifySevenZip) fileExtensions.Add("*.7z");
            if (verifyRar) fileExtensions.Add("*.rar");

            // Find all matching files in the input folder
            var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var allFiles = new List<string>();

            foreach (var extension in fileExtensions)
            {
                allFiles.AddRange(Directory.GetFiles(inputFolder, extension, searchOption));
            }

            // Sort the files for a predictable order
            var files = allFiles.OrderBy(f => f).ToArray();

            _totalFiles = files.Length;
            UpdateCounters();

            LogMessage($"Found {files.Length} compressed files to verify.");

            if (files.Length == 0)
            {
                LogMessage("No matching compressed files found in the specified folder.");
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

                var compressedFile = files[i];
                var fileName = Path.GetFileName(compressedFile);
                var fileType = Path.GetExtension(compressedFile).TrimStart('.').ToUpperInvariant();

                // Show progress information
                UpdateProgressStatus(i + 1, files.Length, fileName);
                LogMessage($"[{i + 1}/{files.Length}] Verifying {fileType} archive: {fileName}");

                // Get the file info first
                var fileInfo = await GetArchiveInfoAsync(sevenZipPath, compressedFile);
                DisplayFileInfo(fileInfo);

                // Verify the archive integrity
                var isValid = await VerifyArchiveAsync(sevenZipPath, compressedFile);

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

    private async Task<bool> VerifyArchiveAsync(string sevenZipPath, string archiveFile)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    // Use the "t" (test) command to verify archive integrity
                    Arguments = $"t \"{archiveFile}\" -r",
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
                    // Log progress or status information
                    if (args.Data.Contains('%') || args.Data.Contains("Testing") ||
                        args.Data.Contains("Everything is Ok") || args.Data.Contains("Error"))
                    {
                        LogMessage($"  {args.Data.Trim()}");
                    }

                    outputBuilder.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    errorBuilder.AppendLine(args.Data);
                    LogMessage($"[ERROR] {args.Data}");
                }
            };

            // Start the process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit(), _cts.Token);

            // Archive verification is successful if the process exits with code 0
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            LogMessage($"Error verifying archive: {ex.Message}");
            await ReportBugAsync($"Error verifying archive: {Path.GetFileName(archiveFile)}", ex);
            return false;
        }
    }

    private async Task<string> GetArchiveInfoAsync(string sevenZipPath, string archiveFile)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    // Use the "l" (list) command to get archive info
                    Arguments = $"l \"{archiveFile}\" -slt",
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

            // Format the archive info for display
            var formattedInfo = FormatArchiveInfo(archiveFile, outputBuilder.ToString());
            return formattedInfo;
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting archive info: {ex.Message}");
            await ReportBugAsync($"Error getting archive info: {Path.GetFileName(archiveFile)}", ex);
            return $"Error getting archive info: {ex.Message}";
        }
    }

    private static string FormatArchiveInfo(string archiveFile, string rawInfo)
    {
        var sb = new StringBuilder();

        // Add file path and size
        sb.AppendLine(CultureInfo.InvariantCulture, $"File: {archiveFile}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Size: {FormatFileSize(new FileInfo(archiveFile).Length)}");
        sb.AppendLine(new string('-', 40));

        // Extract and format archive information from 7z output
        // First, add archive properties
        var lines = rawInfo.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        var inArchiveSection = false;
        var inFileSection = false;
        var fileCount = 0;
        long uncompressedSize = 0;

        foreach (var line in lines)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Track when we enter the archive section
            if (line.Trim() == "----------")
            {
                inArchiveSection = true;
                continue;
            }

            // When we find the archive properties, add them
            if (inArchiveSection && !inFileSection)
            {
                if (line.StartsWith("Type =", StringComparison.Ordinal) ||
                    line.StartsWith("Method =", StringComparison.Ordinal) ||
                    line.StartsWith("Solid =", StringComparison.Ordinal) ||
                    line.StartsWith("Blocks =", StringComparison.Ordinal) ||
                    line.StartsWith("Physical Size =", StringComparison.Ordinal) ||
                    line.StartsWith("Headers Size =", StringComparison.Ordinal))
                {
                    sb.AppendLine(line);
                }


                // Check if we're entering the file section
                if (line.StartsWith("Path =", StringComparison.Ordinal))
                {
                    inFileSection = true;
                    fileCount++;
                }
            }
            else if (inFileSection)
            {
                // Count files and accumulated size
                if (line.StartsWith("Path =", StringComparison.Ordinal))
                {
                    fileCount++;
                }
                else if (line.StartsWith("Size =", StringComparison.Ordinal))
                {
                    if (long.TryParse(line.Substring("Size =".Length).Trim(), out var size))
                    {
                        uncompressedSize += size;
                    }
                }
            }
        }

        // Add summary information
        sb.AppendLine(new string('-', 40));
        sb.AppendLine(CultureInfo.InvariantCulture, $"Total files: {fileCount}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Uncompressed size: {FormatFileSize(uncompressedSize)}");

        // Calculate compression ratio if possible
        var compressedSize = new FileInfo(archiveFile).Length;
        if (uncompressedSize > 0 && compressedSize > 0)
        {
            var ratio = (double)compressedSize / uncompressedSize;
            sb.AppendLine(CultureInfo.InvariantCulture, $"Compression ratio: {ratio:P1}");
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

    /// <summary>
    /// Gets the appropriate 7-Zip executable path based on the system architecture
    /// </summary>
    /// <param name="appDirectory">The application directory</param>
    /// <returns>The path to the appropriate 7-Zip executable</returns>
    private string GetSevenZipExecutablePath(string appDirectory)
    {
        // For 64-bit systems, use 7z.exe
        // For 32-bit systems, use 7z_x86.exe
        var executableName = Environment.Is64BitOperatingSystem ? "7z.exe" : "7z_x86.exe";

        // Get the full path
        return Path.Combine(appDirectory, executableName);
    }

    public void Dispose()
    {
        // Dispose the cancellation token source
        _cts?.Dispose();
        _cts = null;

        // Dispose the bug report service
        _bugReportService?.Dispose();

        // Dispose the stopwatch if needed
        _processingTimer?.Stop();

        // Suppress finalization
        GC.SuppressFinalize(this);
    }
}
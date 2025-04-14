using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace BatchConvertToCHD;

public partial class MainWindow : IDisposable
{
    private CancellationTokenSource _cts;
    private readonly BugReportService _bugReportService;

    // Bug Report API configuration
    private const string BugReportApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "BatchConvertToCHD";
    private static readonly char[] Separator = [' ', '\t'];

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        LogMessage("Welcome to the Batch Convert to CHD.");
        LogMessage("");
        LogMessage("This program will convert the following formats to CHD:");
        LogMessage("- CUE+BIN files (CD images)");
        LogMessage("- ISO files (CD images)");
        LogMessage("- GDI files (GD-ROM images)");
        LogMessage("- TOC files (CD images)");
        LogMessage("- IMG files (Hard disk images)");
        LogMessage("- RAW files (Raw data)");
        LogMessage("- ZIP files (containing any of the above formats)");
        LogMessage("");
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

        Application.Current.Dispatcher.Invoke((Action)(() =>
        {
            LogViewer.AppendText($"{timestampedMessage}{Environment.NewLine}");
            LogViewer.ScrollToEnd();
        }));
    }

    private void BrowseInputButton_Click(object sender, RoutedEventArgs e)
    {
        var inputFolder = SelectFolder("Select the folder containing files to convert");
        if (string.IsNullOrEmpty(inputFolder)) return;

        InputFolderTextBox.Text = inputFolder;
        LogMessage($"Input folder selected: {inputFolder}");
    }

    private void BrowseOutputButton_Click(object sender, RoutedEventArgs e)
    {
        var outputFolder = SelectFolder("Select the output folder where CHD files will be saved");
        if (string.IsNullOrEmpty(outputFolder)) return;

        OutputFolderTextBox.Text = outputFolder;
        LogMessage($"Output folder selected: {outputFolder}");
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
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

            // Clear any previous progress display
            ClearProgressDisplay();

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
        ProgressText.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;

        // When re-enabling the controls, make sure to clear any progress display
        if (enabled)
        {
            ClearProgressDisplay();
        }
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

            // Restrict to supported file types
            var supportedExtensions = new[] { ".cue", ".iso", ".img", ".gdi", ".toc", ".raw", ".zip" };
            var files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                .ToArray();

            LogMessage($"Found {files.Length} files to convert.");

            if (files.Length == 0)
            {
                LogMessage("No supported files found in the input folder.");
                return;
            }

            // Setup progress tracking
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

                // Show progress information
                UpdateProgressStatus(i + 1, files.Length, fileName);
                LogMessage($"[{i + 1}/{files.Length}] Processing: {fileName}");

                // Process the file
                var success = await ProcessFileAsync(chdmanPath, inputFile, outputFolder, deleteFiles);

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

                // Update progress
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
            await ReportBugAsync("Error during batch conversion operation", ex);
        }
    }

    private void UpdateProgressStatus(int current, int total, string currentFile)
    {
        // Calculate percentage
        var percentage = (double)current / total * 100;

        // Update UI elements on the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Update progress bar
            ProgressBar.Value = current;
            ProgressBar.Maximum = total;
            ProgressBar.Visibility = Visibility.Visible;

            // Update the progress text
            ProgressText.Text = $"Processing file {current} of {total}: {Path.GetFileName(currentFile)} ({percentage:F1}%)";
            ProgressText.Visibility = Visibility.Visible;
        });
    }

    private void ClearProgressDisplay()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Hide the progress bar
            ProgressBar.Visibility = Visibility.Collapsed;

            // Clear and hide the progress text
            ProgressText.Text = string.Empty;
            ProgressText.Visibility = Visibility.Collapsed;
        });
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
                if (!trimmedLine.StartsWith("FILE ", StringComparison.OrdinalIgnoreCase)) continue;
                // Extract filename from the FILE line
                // Format: FILE "filename.ext" FILETYPE
                var parts = trimmedLine.Split('"');
                if (parts.Length < 2) continue;

                var fileName = parts[1];
                var filePath = Path.Combine(cueDir, fileName);

                LogMessage($"Found referenced file in CUE: {fileName}");
                referencedFiles.Add(filePath);
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
            // Determine which command to use based on file extension
            var command = "createcd"; // Default for CD-ROM formats
            var extension = Path.GetExtension(inputFile).ToLowerInvariant();

            switch (extension)
            {
                case ".img":
                    // For .img files, we could be dealing with a hard disk or raw image
                    // You might need a more sophisticated way to determine this
                    command = "createhd";
                    break;
                case ".raw":
                    command = "createraw";
                    break;
            }

            LogMessage($"Using CHDMAN command: {command}");

            // Create the process
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = chdmanPath,
                    Arguments = $"{command} -i \"{inputFile}\" -o \"{outputFile}\" -f",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            // Set up output handling for detailed progress
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (_, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;

                outputBuilder.AppendLine(args.Data);

                // Check for progress information
                if (args.Data.Contains("Compressing") && args.Data.Contains('%'))
                {
                    // Extract percentage and update UI
                    UpdateConversionProgress(args.Data);
                }
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (string.IsNullOrEmpty(args.Data)) return;

                errorBuilder.AppendLine(args.Data);

                // Check if this is a progress update or completion message rather than an actual error
                if ((args.Data.Contains("Compressing") && args.Data.Contains('%')) ||
                    args.Data.Contains("Compression complete"))
                {
                    // This is a progress update or completion message, not an error
                    if (args.Data.Contains("Compression complete"))
                    {
                        LogMessage($"Compression completed with {args.Data.Substring(args.Data.IndexOf("ratio =", StringComparison.Ordinal) + 8)}");
                    }
                    else
                    {
                        UpdateConversionProgress(args.Data);
                    }
                }
                else
                {
                    // This is likely an actual error
                    LogMessage($"[ERROR] {args.Data}");
                }
            };

            // Start the process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.Run(() => process.WaitForExit(), _cts.Token);

            // Log detailed output for debugging
            LogMessage($"CHDMAN output: {outputBuilder}");

            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            LogMessage($"Error converting file: {ex.Message}");
            await ReportBugAsync($"Error converting file: {Path.GetFileName(inputFile)}", ex);
            return false;
        }
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

    private void UpdateConversionProgress(string progressLine)
    {
        try
        {
            // Extract percentage from a line like "Compressing, 45.6% complete... (ratio=40.5%)"
            var match = MyRegex().Match(progressLine);
            if (!match.Success) return;

            // Get the percentage string, handling both decimal point and comma formats
            var percentageStr = match.Groups[1].Value;

            // Replace comma with a period to ensure proper parsing regardless of culture
            percentageStr = percentageStr.Replace(',', '.');

            if (!double.TryParse(percentageStr, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var percentage)) return;

            // Ensure percentage is within the expected range (0-100)
            // If it's consistently 10x too high, divide by 10
            if (percentage > 100)
            {
                percentage = percentage / 10;
            }

            // Get the ratio if available
            var ratio = "unknown";
            var ratioMatch = MyRegex1().Match(progressLine);
            if (ratioMatch.Success)
            {
                ratio = ratioMatch.Groups[1].Value.Replace(',', '.') + "%";
            }

            // Just log the progress without adding file-specific progress bars
            LogMessage($"Converting: {percentage:F1}% complete (compression ratio: {ratio})");
        }
        catch (Exception ex)
        {
            // Just log and continue - don't let progress updates crash the app
            LogMessage($"Error updating progress: {ex.Message}");
        }
    }

    private async Task<bool> ProcessFileAsync(string chdmanPath, string inputFile, string outputFolder, bool deleteOriginal)
    {
        try
        {
            var fileToProcess = inputFile;
            var isZipFile = false;
            var tempDir = string.Empty;

            // Check if the file is a ZIP
            if (Path.GetExtension(inputFile).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                LogMessage($"Processing ZIP file: {Path.GetFileName(inputFile)}");
                var extractResult = await ExtractZipFileAsync(inputFile);

                if (extractResult.Success)
                {
                    fileToProcess = extractResult.FilePath;
                    tempDir = extractResult.TempDir;
                    isZipFile = true;
                    LogMessage($"Using extracted file: {Path.GetFileName(fileToProcess)}");
                }
                else
                {
                    LogMessage($"Error extracting ZIP: {extractResult.ErrorMessage}");
                    return false;
                }
            }

            try
            {
                // Determine the output file path
                var outputFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fileToProcess) + ".chd");

                // Perform the conversion
                var success = await ConvertToChdAsync(chdmanPath, fileToProcess, outputFile);

                // Handle cleanup
                if (!success || !deleteOriginal) return success;

                if (isZipFile)
                {
                    // For ZIP files, delete the original ZIP file
                    try
                    {
                        File.Delete(inputFile);
                        LogMessage($"Deleted original ZIP file: {Path.GetFileName(inputFile)}");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Failed to delete ZIP file: {ex.Message}");
                    }
                }
                else
                {
                    // For regular files, delete according to existing logic
                    await DeleteOriginalFilesAsync(fileToProcess);
                }

                return success;
            }
            finally
            {
                // Always clean up temp directory if we extracted a ZIP
                if (isZipFile && !string.IsNullOrEmpty(tempDir) && Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                        LogMessage("Temporary extraction directory cleaned up");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Failed to clean up temp directory: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error processing file {Path.GetFileName(inputFile)}: {ex.Message}");
            await ReportBugAsync($"Error processing file: {Path.GetFileName(inputFile)}", ex);
            return false;
        }
    }

    private Task<(bool Success, string FilePath, string TempDir, string ErrorMessage)> ExtractZipFileAsync(string zipPath)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            LogMessage($"Extracting ZIP file to temporary directory: {tempDir}");
            Directory.CreateDirectory(tempDir);

            // Extract the ZIP file
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    var entryPath = Path.Combine(tempDir, entry.FullName);

                    // Create directory if needed
                    var dirPath = Path.GetDirectoryName(entryPath);
                    if (!string.IsNullOrEmpty(dirPath) && !Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    // Skip directories
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;

                    // Extract the file
                    entry.ExtractToFile(entryPath, true);
                }
            }

            // Find the first supported file in the extracted directory
            var supportedExtensions = new[] { ".cue", ".iso", ".img", ".gdi", ".toc", ".raw" };
            var supportedFile = Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories)
                .FirstOrDefault(f => supportedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()));

            if (supportedFile != null)
            {
                return Task.FromResult((true, supportedFile, tempDir, string.Empty));
            }

            return Task.FromResult((false, string.Empty, tempDir, "No supported files found in ZIP archive"));
        }
        catch (Exception ex)
        {
            return Task.FromResult((false, string.Empty, tempDir, $"Error extracting ZIP: {ex.Message}"));
        }
    }

    private async Task DeleteOriginalFilesAsync(string inputFile)
    {
        try
        {
            // For .cue files, get all referenced files first
            var filesToDelete = new List<string>();

            if (Path.GetExtension(inputFile).Equals(".cue", StringComparison.OrdinalIgnoreCase))
            {
                // Get all files referenced in the .cue file
                var referencedFiles = GetReferencedFilesFromCue(inputFile);
                filesToDelete.AddRange(referencedFiles);
            }
            // Handle GDI files similarly to CUE files
            else if (Path.GetExtension(inputFile).Equals(".gdi", StringComparison.OrdinalIgnoreCase))
            {
                var referencedFiles = GetReferencedFilesFromGdi(inputFile);
                filesToDelete.AddRange(referencedFiles);
            }

            // Always add the original file to be deleted
            filesToDelete.Add(inputFile);

            // Delete all files
            foreach (var fileToDelete in filesToDelete)
            {
                if (!File.Exists(fileToDelete)) continue;

                File.Delete(fileToDelete);
                LogMessage($"Deleted file: {Path.GetFileName(fileToDelete)}");
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Failed to delete original file(s): {ex.Message}");
            await ReportBugAsync($"Failed to delete original file(s): {Path.GetFileName(inputFile)}", ex);
        }
    }

    // Add this method to parse GDI files (similar to how you parse CUE files)
    private List<string> GetReferencedFilesFromGdi(string gdiPath)
    {
        var referencedFiles = new List<string>();
        var gdiDir = Path.GetDirectoryName(gdiPath) ?? string.Empty;

        try
        {
            LogMessage($"Parsing GDI file to find referenced files: {Path.GetFileName(gdiPath)}");
            var lines = File.ReadAllLines(gdiPath);

            // Skip the first line (track count)
            for (var i = 1; i < lines.Length; i++)
            {
                var trimmedLine = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine))
                    continue;

                // GDI format: Track LBA Type SectorSize FileName
                var parts = trimmedLine.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5) continue;

                var fileName = parts[4].Trim('"');
                var filePath = Path.Combine(gdiDir, fileName);

                LogMessage($"Found referenced file in GDI: {fileName}");
                referencedFiles.Add(filePath);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error parsing GDI file: {ex.Message}");
            Task.Run(async () => await ReportBugAsync($"Error parsing GDI file: {Path.GetFileName(gdiPath)}", ex));
        }

        return referencedFiles;
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        // This will close the application
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
            LogMessage($"Error opening About window: {ex.Message}");

            // Report the exception to our bug reporting service
            Task.Run(async () => await ReportBugAsync("Error opening About window", ex));
        }
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"(\d+[\.,]\d+)%")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();

    [System.Text.RegularExpressions.GeneratedRegex(@"ratio=(\d+[\.,]\d+)%")]
    private static partial System.Text.RegularExpressions.Regex MyRegex1();

    public void Dispose()
    {
        // Dispose the cancellation token source
        _cts?.Cancel();
        _cts?.Dispose();
        // _cts = null;

        // Dispose the bug report service
        _bugReportService?.Dispose();

        // Suppress finalization since we've explicitly disposed resources
        GC.SuppressFinalize(this);
    }
}
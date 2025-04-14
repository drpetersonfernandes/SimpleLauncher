using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace CreateBatchFilesForSegaModel3Games;

public partial class MainWindow : IDisposable
{
    private readonly BugReportService _bugReportService;

    // Bug Report API configuration
    private const string BugReportApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "CreateBatchFilesForSegaModel3Games";

    public MainWindow()
    {
        InitializeComponent();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        LogMessage("Welcome to the Batch File Creator for Sega Model 3 Games.");
        LogMessage("");
        LogMessage("This program creates batch files to launch your Sega Model 3 games.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the Supermodel emulator executable file (Supermodel.exe)");
        LogMessage("2. Select the folder containing your Sega Model 3 ROM zip files");
        LogMessage("3. Click 'Create Batch Files' to generate the batch files");
        LogMessage("");
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    private void LogMessage(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        });
    }

    private void BrowseSupermodelButton_Click(object sender, RoutedEventArgs e)
    {
        var supermodelExePath = SelectFile();
        if (string.IsNullOrEmpty(supermodelExePath)) return;

        SupermodelPathTextBox.Text = supermodelExePath;
        LogMessage($"Supermodel executable selected: {supermodelExePath}");

        if (supermodelExePath.EndsWith("Supermodel.exe", StringComparison.OrdinalIgnoreCase)) return;

        LogMessage("Warning: The selected file does not appear to be Supermodel.exe.");
        _ = ReportBugAsync("User selected a file that doesn't appear to be Supermodel.exe: " + supermodelExePath);
    }

    private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var romFolder = SelectFolder();
        if (string.IsNullOrEmpty(romFolder)) return;

        RomFolderTextBox.Text = romFolder;
        LogMessage($"ROM folder selected: {romFolder}");
    }

    private async void CreateBatchFilesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var supermodelExePath = SupermodelPathTextBox.Text;
            var romFolder = RomFolderTextBox.Text;

            if (string.IsNullOrEmpty(supermodelExePath))
            {
                LogMessage("Error: No Supermodel executable selected.");
                ShowError("Please select the Supermodel executable file (Supermodel.exe).");
                return;
            }

            if (!File.Exists(supermodelExePath))
            {
                LogMessage($"Error: Supermodel executable not found at path: {supermodelExePath}");
                ShowError("The selected Supermodel executable file does not exist.");
                await ReportBugAsync("Supermodel executable not found", new FileNotFoundException("The Supermodel executable was not found", supermodelExePath));
                return;
            }

            if (string.IsNullOrEmpty(romFolder))
            {
                LogMessage("Error: No ROM folder selected.");
                ShowError("Please select the folder containing your Sega Model 3 ROM zip files.");
                return;
            }

            if (!Directory.Exists(romFolder))
            {
                LogMessage($"Error: ROM folder not found at path: {romFolder}");
                ShowError("The selected ROM folder does not exist.");
                await ReportBugAsync("ROM folder not found", new DirectoryNotFoundException($"ROM folder not found: {romFolder}"));
                return;
            }

            try
            {
                CreateBatchFilesForModel3Games(romFolder, supermodelExePath);
            }
            catch (Exception ex)
            {
                LogMessage($"Error creating batch files: {ex.Message}");
                ShowError($"An error occurred while creating batch files: {ex.Message}");
                await ReportBugAsync("Error creating batch files", ex);
            }
        }
        catch (Exception ex)
        {
            await ReportBugAsync("Error creating batch files", ex);
        }
    }

    private static string? SelectFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Please select the folder where your Sega Model 3 ROM zip files are located."
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private string? SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Please select the Supermodel executable file (Supermodel.exe)",
            Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void CreateBatchFilesForModel3Games(string romFolder, string supermodelExePath)
    {
        try
        {
            var romFiles = Directory.GetFiles(romFolder, "*.zip");
            var filesCreated = 0;

            LogMessage("");
            LogMessage("Starting batch file creation process...");
            LogMessage("");

            if (romFiles.Length == 0)
            {
                LogMessage("No ROM zip files found. No batch files were created.");
                ShowError("No ROM zip files found. No batch files were created.");
                _ = ReportBugAsync("No ROM zip files found in selected folder",
                    new FileNotFoundException("No *.zip files found in ROM folder", romFolder));
                return;
            }

            foreach (var romFilePath in romFiles)
            {
                try
                {
                    var romFileName = Path.GetFileNameWithoutExtension(romFilePath);
                    var batchFilePath = Path.Combine(romFolder, romFileName + ".bat");

                    using (StreamWriter sw = new(batchFilePath))
                    {
                        sw.WriteLine($"cd /d \"{Path.GetDirectoryName(supermodelExePath)}\"");
                        sw.WriteLine($"\"{supermodelExePath}\" \"{romFilePath}\" -fullscreen -show-fps");

                        LogMessage($"Batch file created: {batchFilePath}");
                    }

                    filesCreated++;
                }
                catch (Exception ex)
                {
                    LogMessage($"Error creating batch file for {romFilePath}: {ex.Message}");
                    _ = ReportBugAsync($"Error creating batch file for {Path.GetFileName(romFilePath)}", ex);
                }
            }

            if (filesCreated > 0)
            {
                LogMessage("");
                LogMessage($"{filesCreated} batch files have been successfully created.");
                LogMessage("They are located in the same folder as your ROM zip files.");

                ShowMessageBox($"{filesCreated} batch files have been successfully created.\n\n" +
                               "They are located in the same folder as your ROM zip files.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                LogMessage("Failed to create any batch files.");
                ShowError("Failed to create any batch files.");
                _ = ReportBugAsync("Failed to create any batch files despite finding zip files",
                    new Exception($"Found {romFiles.Length} zip files but created 0 batch files"));
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error accessing ROM folder: {ex.Message}");
            _ = ReportBugAsync("Error accessing ROM folder during batch file creation", ex);
            throw; // Rethrow to be caught by the outer try-catch
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
            if (LogTextBox != null)
            {
                var logContent = string.Empty;

                // Safely get log content from UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    logContent = LogTextBox.Text;
                });

                if (!string.IsNullOrEmpty(logContent))
                {
                    fullReport.AppendLine();
                    fullReport.AppendLine("=== Application Log ===");
                    fullReport.Append(logContent);
                }
            }

            // Add Supermodel and ROM folder paths if available
            if (SupermodelPathTextBox != null && RomFolderTextBox != null)
            {
                var supermodelPath = string.Empty;
                var romFolderPath = string.Empty;

                await Dispatcher.InvokeAsync(() =>
                {
                    supermodelPath = SupermodelPathTextBox.Text;
                    romFolderPath = RomFolderTextBox.Text;
                });

                fullReport.AppendLine();
                fullReport.AppendLine("=== Configuration ===");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Supermodel Path: {supermodelPath}");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"ROM Folder: {romFolderPath}");
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
        // Dispose the bug report service
        _bugReportService?.Dispose();

        // Suppress finalization since we're explicitly disposing
        GC.SuppressFinalize(this);
    }
}
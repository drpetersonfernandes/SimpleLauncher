using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace CreateBatchFilesForXbox360XBLAGames;

public partial class MainWindow : IDisposable
{
    private readonly BugReportService _bugReportService;

    // Bug Report API configuration
    private const string BugReportApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "CreateBatchFilesForXbox360XBLAGames";

    public MainWindow()
    {
        InitializeComponent();

        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        LogMessage("Welcome to the Batch File Creator for Xbox 360 XBLA Games.");
        LogMessage("");
        LogMessage("This program creates batch files to launch your Xbox 360 XBLA games.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the Xenia executable file (xenia.exe)");
        LogMessage("2. Select the root folder containing your Xbox 360 XBLA game folders");
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

    private async void BrowseXeniaButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var xeniaExePath = SelectFile();
            if (string.IsNullOrEmpty(xeniaExePath)) return;

            XeniaPathTextBox.Text = xeniaExePath;
            LogMessage($"Xenia executable selected: {xeniaExePath}");

            // Validate the Xenia executable
            if (!File.Exists(xeniaExePath))
            {
                LogMessage("Warning: The selected Xenia executable file does not exist.");
                await ReportBugAsync("Selected Xenia executable does not exist: " + xeniaExePath);
            }
            else if (!Path.GetFileName(xeniaExePath).Contains("xenia", StringComparison.CurrentCultureIgnoreCase))
            {
                LogMessage("Warning: The selected file does not appear to be a Xenia executable.");
                await ReportBugAsync("Selected file may not be Xenia executable: " + xeniaExePath);
            }
        }
        catch (Exception ex)
        {
            await ReportBugAsync("Error in method BrowseXeniaButton_Click", ex);
        }
    }

    private async void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var rootFolder = SelectFolder();
            if (string.IsNullOrEmpty(rootFolder)) return;

            GameFolderTextBox.Text = rootFolder;
            LogMessage($"Game folder selected: {rootFolder}");

            // Validate the game folder
            if (!Directory.Exists(rootFolder))
            {
                LogMessage("Warning: The selected game folder does not exist.");
                await ReportBugAsync("Selected game folder does not exist: " + rootFolder);
            }
            else
            {
                // Check if the folder has any subdirectories
                var subDirectories = Directory.GetDirectories(rootFolder);
                if (subDirectories.Length != 0) return;

                LogMessage("Warning: The selected game folder has no subdirectories.");
                await ReportBugAsync("Selected game folder has no subdirectories: " + rootFolder);
            }
        }
        catch (Exception ex)
        {
            await ReportBugAsync("Error in method BrowseFolderButton_Click", ex);
        }
    }

    private async void CreateBatchFilesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var xeniaExePath = XeniaPathTextBox.Text;
            var rootFolder = GameFolderTextBox.Text;

            if (string.IsNullOrEmpty(xeniaExePath))
            {
                LogMessage("Error: No Xenia executable selected.");
                ShowError("Please select the Xenia executable file (xenia.exe).");
                return;
            }

            if (!File.Exists(xeniaExePath))
            {
                LogMessage($"Error: Xenia executable not found at path: {xeniaExePath}");
                ShowError("The selected Xenia executable file does not exist.");
                await ReportBugAsync("Xenia executable not found", new FileNotFoundException("The Xenia executable was not found", xeniaExePath));
                return;
            }

            if (string.IsNullOrEmpty(rootFolder))
            {
                LogMessage("Error: No game folder selected.");
                ShowError("Please select the root folder containing your Xbox 360 XBLA game folders.");
                return;
            }

            if (!Directory.Exists(rootFolder))
            {
                LogMessage($"Error: Game folder not found at path: {rootFolder}");
                ShowError("The selected game folder does not exist.");
                await ReportBugAsync("Game folder not found", new DirectoryNotFoundException($"Game folder not found: {rootFolder}"));
                return;
            }

            try
            {
                await CreateBatchFilesForXboxXblaGames(rootFolder, xeniaExePath);
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
        var fbd = new OpenFolderDialog
        {
            Title = "Please select the root folder where your Xbox 360 XBLA game folders are located."
        };

        return fbd.ShowDialog() == true ? fbd.FolderName : null;
    }

    private string? SelectFile()
    {
        var ofd = new OpenFileDialog
        {
            Title = "Please select the Xenia executable file (xenia.exe)",
            Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        return ofd.ShowDialog() == true ? ofd.FileName : null;
    }

    private async Task CreateBatchFilesForXboxXblaGames(string rootFolder, string xeniaExePath)
    {
        try
        {
            var gameDirectories = Directory.GetDirectories(rootFolder);
            var filesCreated = 0;
            var directoriesProcessed = 0;
            var directoriesSkipped = 0;

            LogMessage("");
            LogMessage("Starting batch file creation process...");
            LogMessage("");

            foreach (var gameDirectory in gameDirectories)
            {
                directoriesProcessed++;
                try
                {
                    var gameFolderName = Path.GetFileName(gameDirectory);
                    var batchFilePath = Path.Combine(rootFolder, gameFolderName + ".bat");

                    var gameFilePath = await FindGameFile(gameDirectory);

                    if (string.IsNullOrEmpty(gameFilePath))
                    {
                        LogMessage($"No game file found in {gameFolderName}. Skipping...");
                        directoriesSkipped++;
                        await ReportBugAsync($"No game file found in directory: {gameFolderName}",
                            new FileNotFoundException("No game file found in XBLA directory structure", gameDirectory));
                        continue;
                    }

                    // Check if the batch file exists and whether we can write to it
                    try
                    {
                        await using (StreamWriter sw = new(batchFilePath))
                        {
                            await sw.WriteLineAsync($"\"{xeniaExePath}\" \"{gameFilePath}\"");
                            LogMessage($"Batch file created: {batchFilePath}");
                        }

                        filesCreated++;
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error creating batch file for {gameFolderName}: {ex.Message}");
                        await ReportBugAsync($"Error creating batch file for {gameFolderName}", ex);
                        directoriesSkipped++;
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error processing directory {Path.GetFileName(gameDirectory)}: {ex.Message}");
                    await ReportBugAsync($"Error processing directory: {Path.GetFileName(gameDirectory)}", ex);
                    directoriesSkipped++;
                }
            }

            LogMessage("");
            LogMessage($"Processed {directoriesProcessed} directories.");
            LogMessage($"Skipped {directoriesSkipped} directories.");

            if (filesCreated > 0)
            {
                LogMessage($"{filesCreated} batch files have been successfully created.");
                LogMessage("They are located in the root folder of your Xbox 360 XBLA games.");

                ShowMessageBox($"{filesCreated} batch files have been successfully created.\n\n" +
                               "They are located in the root folder of your Xbox 360 XBLA games.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                const string errorMessage = "No valid game folders found. No batch files were created.";
                LogMessage(errorMessage);
                ShowError(errorMessage);

                var ex = new Exception($"Processed {directoriesProcessed} directories but created 0 batch files");
                await ReportBugAsync(errorMessage, ex);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error scanning game folders: {ex.Message}");
            await ReportBugAsync("Error scanning game folders", ex);
            throw;
        }
    }

    private async Task<string?> FindGameFile(string gameDirectory)
    {
        try
        {
            var directories = Directory.GetDirectories(gameDirectory, "000D0000", SearchOption.AllDirectories);

            if (directories.Length > 0)
            {
                var files = Directory.GetFiles(directories[0]);

                if (files.Length > 0)
                {
                    return files[0];
                }
                else
                {
                    await ReportBugAsync($"No files found in 000D0000 directory for game: {Path.GetFileName(gameDirectory)}");
                }
            }
            else
            {
                // If we couldn't find the 000D0000 directory, let's try to report the directory structure
                var directoryStructure = new StringBuilder();
                directoryStructure.AppendLine(CultureInfo.InvariantCulture, $"Directory structure for {Path.GetFileName(gameDirectory)}:");

                try
                {
                    // Get top-level subdirectories
                    var topLevelDirs = Directory.GetDirectories(gameDirectory);
                    foreach (var dir in topLevelDirs)
                    {
                        directoryStructure.AppendLine(CultureInfo.InvariantCulture, $"- {Path.GetFileName(dir)}");

                        // Get second-level subdirectories (limited to keep the report reasonable)
                        try
                        {
                            var secondLevelDirs = Directory.GetDirectories(dir);
                            foreach (var subDir in secondLevelDirs.Take(5)) // Only report up to 5 subdirectories
                            {
                                directoryStructure.AppendLine(CultureInfo.InvariantCulture, $"  - {Path.GetFileName(subDir)}");
                            }

                            if (secondLevelDirs.Length > 5)
                            {
                                directoryStructure.AppendLine(CultureInfo.InvariantCulture, $"  - ... and {secondLevelDirs.Length - 5} more directories");
                            }
                        }
                        catch
                        {
                            directoryStructure.AppendLine("  - Unable to access subdirectories");
                        }
                    }
                }
                catch (Exception ex)
                {
                    directoryStructure.AppendLine(CultureInfo.InvariantCulture, $"Error accessing directory structure: {ex.Message}");
                }

                await ReportBugAsync($"No 000D0000 directory found for game: {Path.GetFileName(gameDirectory)}",
                    new DirectoryNotFoundException(directoryStructure.ToString()));
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error finding game file in {Path.GetFileName(gameDirectory)}: {ex.Message}");
            await ReportBugAsync($"Error finding game file in {Path.GetFileName(gameDirectory)}", ex);
        }

        return null;
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

            // Add Xenia and game folder paths if available
            if (XeniaPathTextBox != null && GameFolderTextBox != null)
            {
                var xeniaPath = string.Empty;
                var gameFolderPath = string.Empty;

                await Dispatcher.InvokeAsync(() =>
                {
                    xeniaPath = XeniaPathTextBox.Text;
                    gameFolderPath = GameFolderTextBox.Text;
                });

                fullReport.AppendLine();
                fullReport.AppendLine("=== Configuration ===");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Xenia Path: {xeniaPath}");
                fullReport.AppendLine(CultureInfo.InvariantCulture, $"Game Folder Path: {gameFolderPath}");
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

        // Suppress finalization since there's no need for it
        GC.SuppressFinalize(this);
    }
}
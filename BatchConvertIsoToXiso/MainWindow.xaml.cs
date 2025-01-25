using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace BatchConvertIsoToXiso;

public partial class MainWindow
{
    private readonly CancellationTokenSource _cts;

    public MainWindow()
    {
        InitializeComponent();
        _cts = new CancellationTokenSource();

        WelcomeMessage();
    }

    private void WelcomeMessage()
    {
        AppendLog("Welcome to the Batch Convert ISO to XISO program.");
        AppendLog("Convert ISO files to Xbox XISO format using extract-xiso.");
        AppendLog("Click 'Start Conversion' to begin.");
    }

    private async Task PerformBatchConversionAsync(string extractXisoPath, string inputFolder, string outputFolder, bool deleteFiles)
    {
        try
        {
            AppendLog("Scanning input folder for ISO files...");
            var isoFiles = Directory.GetFiles(inputFolder, "*.iso", SearchOption.TopDirectoryOnly);

            if (!isoFiles.Any())
            {
                AppendLog("No ISO files found in the selected folder.");
                return;
            }

            AppendLog($"Found {isoFiles.Length} ISO file(s). Starting conversion...");

            ProgressBar.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            ProgressBar.Maximum = isoFiles.Length;

            for (int i = 0; i < isoFiles.Length; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    AppendLog("Conversion canceled.");
                    break;
                }

                var isoFile = isoFiles[i];
                AppendLog($"[{i + 1}/{isoFiles.Length}] Converting: {isoFile}");

                bool success = await ConvertToXisoAsync(extractXisoPath, isoFile);

                if (success)
                {
                    string convertedFileName = Path.GetFileName(isoFile); // Retains the original name with .iso extension
                    string convertedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, convertedFileName);
                    string destinationPath = Path.Combine(outputFolder, convertedFileName);

                    // Move the converted file to the output folder asynchronously
                    if (File.Exists(convertedFilePath))
                    {
                        try
                        {
                            await Task.Run(() => File.Move(convertedFilePath, destinationPath, overwrite: true));
                            AppendLog($"Moved converted file to: {destinationPath}");
                        }
                        catch (Exception ex)
                        {
                            AppendLog($"Error moving file {convertedFilePath} to {destinationPath}: {ex.Message}");
                        }
                    }
                    else
                    {
                        AppendLog($"Expected converted file not found: {convertedFilePath}");
                    }

                    // Handle the .old file based on deleteFiles parameter asynchronously
                    string renamedFilePath = isoFile + ".old";
                    if (File.Exists(renamedFilePath))
                    {
                        if (deleteFiles)
                        {
                            try
                            {
                                await Task.Run(() => File.Delete(renamedFilePath));
                                AppendLog($"Deleted renamed file: {renamedFilePath}");
                            }
                            catch (Exception ex)
                            {
                                AppendLog($"Error deleting file {renamedFilePath}: {ex.Message}");
                            }
                        }
                        else
                        {
                            try
                            {
                                await Task.Run(() => File.Move(renamedFilePath, isoFile, overwrite: true));
                                AppendLog($"Renamed file back to original: {isoFile}");
                            }
                            catch (Exception ex)
                            {
                                AppendLog($"Error renaming file {renamedFilePath} back to {isoFile}: {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    AppendLog($"Conversion failed: {isoFile}");
                }

                ProgressBar.Value = i + 1;
            }

            AppendLog("Batch conversion completed.");
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
        }
        finally
        {
            ProgressBar.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
        }
    }

    private Task<bool> ConvertToXisoAsync(string extractXisoPath, string isoFile)
    {
        return Task.Run(() =>
        {
            try
            {
                AppendLog($"Starting conversion for file: {isoFile}");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = extractXisoPath,
                        Arguments = $"-r \"{isoFile}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                AppendLog($"Process output: {output}");
                AppendLog(process.ExitCode != 0 ? $"Process error: {error}" : "Conversion successful.");
                
                return process.ExitCode == 0;
                
            }
            catch (Exception ex)
            {
                AppendLog($"Error during conversion: {ex.Message}");
                return false;
            }
        });
    }

    private void AppendLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logMessage = $"[{timestamp}] {message}";

        // Log to Console
        Console.WriteLine(logMessage);

        // Log to WPF UI
        Dispatcher.Invoke(() =>
        {
            LogViewer.AppendText($"{logMessage}{Environment.NewLine}");
            LogViewer.ScrollToEnd();
        });
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        AppendLog("Cancel button clicked. Cancelling the operation...");
        _cts.Cancel();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AppendLog("Start button clicked. Initializing conversion process...");

            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string extractXisoPath = Path.Combine(appDirectory, "extract-xiso.exe");
            if (!File.Exists(extractXisoPath))
            {
                AppendLog("Error: extract-xiso.exe not found in the application folder.");
                MessageBox.Show("extract-xiso.exe is missing!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AppendLog($"Using extract-xiso.exe from: {extractXisoPath}");

            var inputFolder = SelectFolder("Select the folder containing ISO files to convert");
            if (string.IsNullOrEmpty(inputFolder))
            {
                AppendLog("Input folder selection canceled.");
                return;
            }

            AppendLog($"Input folder selected: {inputFolder}");

            var outputFolder = SelectFolder("Select the output folder for converted XISO files");
            if (string.IsNullOrEmpty(outputFolder))
            {
                AppendLog("Output folder selection canceled.");
                return;
            }

            AppendLog($"Output folder selected: {outputFolder}");

            var deleteFiles = MessageBox.Show("Delete original ISO files after successful conversion?",
                "Delete Files", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

            AppendLog($"Delete original files option: {deleteFiles}");

            AppendLog("Starting batch conversion...");
            await PerformBatchConversionAsync(extractXisoPath, inputFolder, outputFolder, deleteFiles);
        }
        catch (Exception ex)
        {
            AppendLog($"Unexpected error: {ex.Message}");
        }
    }

    private string SelectFolder(string description)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = description;

        AppendLog($"Opening folder dialog: {description}");
        var result = dialog.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK)
        {
            AppendLog($"Folder selected: {dialog.SelectedPath}");
            return dialog.SelectedPath;
        }

        AppendLog("Folder selection canceled.");
        return string.Empty;
    }
}
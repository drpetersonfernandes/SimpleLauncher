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
        AppendLog("This program will convert all [ISO] files in the input folder to Xbox XISO files in the output folder using extract-xiso.");
        AppendLog("Click on the Start Conversion button below to begin the conversion process.");
    }

    private async Task PerformBatchConversionAsync(string extractXisoPath, string inputFolder, string outputFolder, bool deleteFiles)
    {
        try
        {
            AppendLog("Preparing for batch conversion...");

            // Restrict to .iso files only
            var files = Directory.GetFiles(inputFolder, "*.iso", SearchOption.TopDirectoryOnly).ToArray();

            AppendLog($"Found {files.Length} ISO files to convert.");

            if (files.Length == 0)
            {
                AppendLog("No ISO files found in the input folder.");
                return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            ProgressBar.Maximum = files.Length;

            // Create converted folder in the output directory
            var convertedFolder = Path.Combine(outputFolder, "converted");
            Directory.CreateDirectory(convertedFolder);

            for (int i = 0; i < files.Length; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    AppendLog("Operation canceled by user.");
                    break;
                }

                var inputFile = files[i];
                var outputFile = Path.Combine(outputFolder, Path.GetFileName(inputFile));

                AppendLog($"[{i + 1}/{files.Length}] Converting: {inputFile}");
                bool success = await ConvertToXisoAsync(extractXisoPath, inputFile);

                if (success)
                {
                    AppendLog($"Conversion successful: {inputFile}");

                    if (deleteFiles)
                    {
                        File.Delete(inputFile);
                        AppendLog($"Deleted original file: {inputFile}");
                    }

                    // Move the converted file to the converted folder
                    var movedFile = Path.Combine(convertedFolder, Path.GetFileName(outputFile));
                    File.Move(outputFile, movedFile);
                    AppendLog($"Moved converted file to: {movedFile}");
                }
                else
                {
                    AppendLog($"Conversion failed: {inputFile}");
                }

                ProgressBar.Value = i + 1;
            }

            ProgressBar.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            AppendLog("Batch conversion completed.");
        }
        catch (Exception ex)
        {
            AppendLog($"Error during batch conversion: {ex.Message}");
        }
    }

    private Task<bool> ConvertToXisoAsync(string extractXisoPath, string inputFile)
    {
        return Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = extractXisoPath,
                        Arguments = $"-r \"{inputFile}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                AppendLog($"Error: {ex.Message}");
                return false;
            }
        });
    }

    private void AppendLog(string message)
    {
        string timestampedMessage = $"[{DateTime.Now}] {message}";

        // Log to console
        Console.WriteLine(timestampedMessage);

        // Log to WPF UI
        Dispatcher.Invoke(() =>
        {
            LogViewer.AppendText($"{timestampedMessage}{Environment.NewLine}");
            LogViewer.ScrollToEnd();
        });
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cts.Cancel();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AppendLog("Starting batch conversion process...");

            // Step 1: Use extract-xiso.exe from the application folder
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string extractXisoPath = Path.Combine(appDirectory, "extract-xiso.exe");
            if (!File.Exists(extractXisoPath))
            {
                AppendLog("extract-xiso.exe not found in the application folder.");
                MessageBox.Show("extract-xiso.exe is missing from the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            AppendLog($"Using extract-xiso.exe: {extractXisoPath}");

            // Step 2: Prompt for input folder
            var inputFolderDialog = new FolderBrowserDialog
            {
                Description = "Select the folder containing ISO files to convert"
            };
            if (inputFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                AppendLog("Input folder selection canceled.");
                return;
            }
            string inputFolder = inputFolderDialog.SelectedPath;
            AppendLog($"Input folder selected: {inputFolder}");

            // Step 3: Prompt for output folder
            var outputFolderDialog = new FolderBrowserDialog
            {
                Description = "Select the output folder"
            };
            if (outputFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                AppendLog("Output folder selection canceled.");
                return;
            }
            string outputFolder = outputFolderDialog.SelectedPath;
            AppendLog($"Output folder selected: {outputFolder}");

            // Step 4: Ask if successfully converted files should be deleted
            var result = MessageBox.Show("Do you want to delete successfully converted files after conversion?",
                "Delete Files", MessageBoxButton.YesNo, MessageBoxImage.Question);
            bool deleteFiles = result == MessageBoxResult.Yes;
            AppendLog($"Delete files option: {deleteFiles}");

            // Step 5: Start batch conversion
            AppendLog("Starting batch conversion...");
            await PerformBatchConversionAsync(extractXisoPath, inputFolder, outputFolder, deleteFiles);
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
        }
    }
}

using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace BatchConvertToZip;

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
        AppendLog("Welcome to the Batch Convert to Zip.");
        AppendLog("This program will compress all files in the input folder to .zip format in the output folder.");
        AppendLog("Click on the Start Compression button below to begin.");
    }

    private async Task PerformBatchCompressionAsync(string inputFolder, string outputFolder, bool deleteFiles)
    {
        try
        {
            AppendLog("Preparing for batch compression...");
            var files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly).ToArray();
            AppendLog($"Found {files.Length} files to compress.");

            if (files.Length == 0)
            {
                AppendLog("No files found in the input folder.");
                return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            ProgressBar.Maximum = files.Length;

            for (var i = 0; i < files.Length; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    AppendLog("Operation canceled by user.");
                    break;
                }

                var inputFile = files[i];
                var outputFile = Path.Combine(outputFolder, Path.GetFileName(inputFile) + ".zip");

                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                AppendLog($"[{i + 1}/{files.Length}] Compressing: {inputFile}");
                var success = await CompressFileToZipAsync(inputFile, outputFile);

                if (success)
                {
                    AppendLog($"Compression successful: {inputFile}");
                    if (deleteFiles)
                    {
                        File.Delete(inputFile);
                        AppendLog($"Deleted original file: {inputFile}");
                    }
                }
                else
                {
                    AppendLog($"Compression failed: {inputFile}");
                }

                ProgressBar.Value = i + 1;
            }

            ProgressBar.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Collapsed;
            AppendLog("Batch compression completed.");
        }
        catch (Exception ex)
        {
            AppendLog($"Error during batch compression: {ex.Message}");
        }
    }

    private Task<bool> CompressFileToZipAsync(string inputFile, string outputFile)
    {
        return Task.Run(() =>
        {
            try
            {
                using var zipArchive = ZipFile.Open(outputFile, ZipArchiveMode.Create);
                zipArchive.CreateEntryFromFile(inputFile, Path.GetFileName(inputFile), CompressionLevel.Optimal);
                return true;
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
        var timestampedMessage = $"[{DateTime.Now}] {message}";

        Console.WriteLine(timestampedMessage);

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
            AppendLog("Starting batch compression process...");

            var inputFolderDialog = new FolderBrowserDialog
            {
                Description = "Select the folder containing files to compress"
            };
            if (inputFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                AppendLog("Input folder selection canceled.");
                return;
            }
            var inputFolder = inputFolderDialog.SelectedPath;
            AppendLog($"Input folder selected: {inputFolder}");

            var outputFolderDialog = new FolderBrowserDialog
            {
                Description = "Select the output folder"
            };
            if (outputFolderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                AppendLog("Output folder selection canceled.");
                return;
            }
            var outputFolder = outputFolderDialog.SelectedPath;
            AppendLog($"Output folder selected: {outputFolder}");

            var result = MessageBox.Show("Do you want to delete successfully compressed files after compression?",
                "Delete Files", MessageBoxButton.YesNo, MessageBoxImage.Question);
            var deleteFiles = result == MessageBoxResult.Yes;
            AppendLog($"Delete files option: {deleteFiles}");

            await PerformBatchCompressionAsync(inputFolder, outputFolder, deleteFiles);
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
        }
    }
}
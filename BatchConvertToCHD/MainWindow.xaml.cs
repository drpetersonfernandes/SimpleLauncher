using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace BatchConvertToCHD;

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
        AppendLog("Welcome to the Batch Convert to CHD.");
        AppendLog("This program will convert all [CUE], [ISO], and [IMG] files in the input folder to [CHD] files in the output folder.");
        AppendLog("Click on the Start Conversion button bellow to begin the conversion process.");
    }

    private async Task PerformBatchConversionAsync(string chdmanPath, string inputFolder, string outputFolder, bool deleteFiles)
    {
        try
        {
            AppendLog("Preparing for batch conversion...");
    
            // Restrict to .cue, .iso, and .img files only
            var supportedExtensions = new[] { ".cue", ".iso", ".img" };
            var files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly)
                .Where(file => supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToArray();
    
            AppendLog($"Found {files.Length} files to convert.");

            if (files.Length == 0)
            {
                AppendLog("No [CUE], [ISO], or [IMG] files found in the input folder.");
                return;
            }

            ProgressBar.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Visible;
            ProgressBar.Maximum = files.Length;

            for (int i = 0; i < files.Length; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    AppendLog("Operation canceled by user.");
                    break;
                }

                var inputFile = files[i];
                var outputFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(inputFile) + ".chd");

                // Overwrite the output file if it exists
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                AppendLog($"[{i + 1}/{files.Length}] Converting: {inputFile}");
                bool success = await ConvertToChdAsync(chdmanPath, inputFile, outputFile);

                if (success)
                {
                    AppendLog($"Conversion successful: {inputFile}");

                    // Extracting the filename without the extension
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
                    string binFileName = $"{fileNameWithoutExtension}.bin";
                    string binFileNameWithPath = Path.Combine(inputFolder, binFileName);
       
                    if (deleteFiles)
                    {
                        // Delete the original input file
                        File.Delete(inputFile);
                        AppendLog($"Deleted original file: {inputFile}");

                        // Delete the `.bin` file
                        if (File.Exists(binFileNameWithPath))
                        {
                            File.Delete(binFileNameWithPath);
                            AppendLog($"Deleted .bin file: {binFileNameWithPath}");
                        }
                        else
                        {
                            AppendLog($"Cannot delete .bin file. File not found: {binFileNameWithPath}");
                        }
                    }
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

    private Task<bool> ConvertToChdAsync(string chdmanPath, string inputFile, string outputFile)
    {
        return Task.Run(() =>
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
            
            // Step 1: Use chdman.exe from the application folder
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string chdmanPath = Path.Combine(appDirectory, "chdman.exe");
            if (!File.Exists(chdmanPath))
            {
                AppendLog("chdman.exe not found in the application folder.");
                MessageBox.Show("chdman.exe is missing from the application folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            AppendLog($"Using chdman.exe: {chdmanPath}");

            // Step 2: Prompt for input folder
            var inputFolderDialog = new FolderBrowserDialog
            {
                Description = "Select the folder containing files to convert"
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
            await PerformBatchConversionAsync(chdmanPath, inputFolder, outputFolder, deleteFiles);
        }
        catch (Exception ex)
        {
            AppendLog($"Error: {ex.Message}");
        }
    }

}
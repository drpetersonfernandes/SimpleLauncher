using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace BatchConvertIsoToXiso
{
    /// <summary>
    /// ViewModel for the MainWindow, implementing INotifyPropertyChanged for binding.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Properties

        private bool _isBusy;

        private bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _statusText = "Ready";

        public string StatusText
        {
            get => _statusText;
            private set => SetProperty(ref _statusText, value);
        }

        #endregion

        #region Commands

        public ICommand StartConversionCommand { get; }
        public ICommand CancelConversionCommand { get; }

        #endregion

        #region Fields

        private readonly FlowDocument _logDocument;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion

        #region Constructor

        public MainViewModel(FlowDocument logDocument)
        {
            _logDocument = logDocument;
            StartConversionCommand = new AsyncRelayCommand(StartConversionAsync, () => !IsBusy);
            CancelConversionCommand = new RelayCommand(CancelConversion, () => IsBusy);
            _cancellationTokenSource = new CancellationTokenSource();

            // Initial welcome message
            AppendLog("Welcome to the Batch Convert ISO to XISO program.");
            AppendLog("Convert ISO files to Xbox XISO format using extract-xiso.");
            AppendLog("Click 'Start Conversion' to begin.");
        }

        #endregion

        #region Logger Methods

        private void ClearLog()
        {
            Application.Current.Dispatcher.Invoke((Action)(() => _logDocument.Blocks.Clear()));
        }

        private void AppendLog(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logMessage = $"[{timestamp}] {message}";

            // Log to Console
            Console.WriteLine(logMessage);

            // Log to UI
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                var paragraph = new Paragraph();
                paragraph.Inlines.Add(new Run(logMessage));
                _logDocument.Blocks.Add(paragraph);

                // Force scroll to end
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.LogViewerControl.ScrollToEnd();
                }
            }));
        }

        private void LogColoredMessage(string message, Brush color)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logMessage = $"[{timestamp}] {message}";

            // Log to Console
            Console.WriteLine(logMessage);

            // Log to UI
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                var paragraph = new Paragraph();
                var run = new Run(logMessage) { Foreground = color };
                paragraph.Inlines.Add(run);
                _logDocument.Blocks.Add(paragraph);

                // Force scroll to end
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.LogViewerControl.ScrollToEnd();
                }
            }));
        }

        #endregion

        #region Conversion Methods

        private async Task StartConversionAsync()
        {
            try
            {
                // Reset UI state
                ClearLog();
                IsBusy = true;
                StatusText = "Initializing...";

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }

                AppendLog("Starting conversion process...");

                // Check for extract-xiso.exe
                var extractXisoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extract-xiso.exe");
                if (!File.Exists(extractXisoPath))
                {
                    AppendLog("Error: extract-xiso.exe not found in the application folder.");
                    LogColoredMessage("ERROR: extract-xiso.exe is missing! Please ensure it's in the application folder.", Brushes.Red);
                    MessageBox.Show("extract-xiso.exe is missing!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AppendLog($"Using extract-xiso.exe from: {extractXisoPath}");

                // Select input folder
                StatusText = "Selecting input folder...";
                var inputFolder = await ShowFolderDialogAsync("Select the folder containing ISO files to convert");
                if (string.IsNullOrEmpty(inputFolder))
                {
                    AppendLog("Input folder selection canceled.");
                    return;
                }

                AppendLog($"Input folder selected: {inputFolder}");

                // Select output folder
                StatusText = "Selecting output folder...";
                var outputFolder = await ShowFolderDialogAsync("Select the output folder for converted XISO files");
                if (string.IsNullOrEmpty(outputFolder))
                {
                    AppendLog("Output folder selection canceled.");
                    return;
                }

                AppendLog($"Output folder selected: {outputFolder}");

                // Confirm deletion option
                StatusText = "Confirming options...";
                var deleteFiles = await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageBox.Show("Delete original ISO files after successful conversion?",
                        "Delete Files", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);

                AppendLog($"Delete original files option: {deleteFiles}");

                // Start conversion
                StatusText = "Starting...";
                AppendLog("Starting batch conversion...");

                await PerformBatchConversionAsync(extractXisoPath, inputFolder, outputFolder, deleteFiles);
            }
            catch (OperationCanceledException)
            {
                AppendLog("Operation was canceled.");
                LogColoredMessage("Conversion process was canceled by user.", Brushes.Orange);
            }
            catch (Exception ex)
            {
                AppendLog($"Unexpected error: {ex.Message}");
                LogColoredMessage($"ERROR: {ex.Message}", Brushes.Red);
            }
            finally
            {
                IsBusy = false;
                StatusText = "Ready";
            }
        }

        private void CancelConversion()
        {
            AppendLog("Cancel button clicked. Cancelling the operation...");
            StatusText = "Cancelling...";
            _cancellationTokenSource.Cancel();
        }

        private async Task<string> ShowFolderDialogAsync(string description)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                using var dialog = new FolderBrowserDialog();
                dialog.Description = description;
                dialog.UseDescriptionForTitle = true;
                dialog.ShowNewFolderButton = true;

                AppendLog($"Opening folder dialog: {description}");

                return dialog.ShowDialog() == DialogResult.OK
                    ? dialog.SelectedPath
                    : string.Empty;
            });
        }

        private async Task PerformBatchConversionAsync(
            string extractXisoPath,
            string inputFolder,
            string outputFolder,
            bool deleteFiles)
        {
            AppendLog("Scanning input folder for ISO files...");
            StatusText = "Scanning files...";

            var isoFiles = await Task.Run(() =>
                    Directory.GetFiles(inputFolder, "*.iso", SearchOption.TopDirectoryOnly),
                _cancellationTokenSource.Token);

            if (isoFiles.Length == 0)
            {
                AppendLog("No ISO files found in the selected folder.");
                return;
            }

            AppendLog($"Found {isoFiles.Length} ISO file(s). Starting conversion...");

            // Track progress
            var progress = new Progress<string>(AppendLog);
            var totalFiles = isoFiles.Length;
            var convertedFiles = 0;

            for (var i = 0; i < isoFiles.Length; i++)
            {
                _cancellationTokenSource.Token.ThrowIfCancellationRequested();

                var isoFile = isoFiles[i];
                var fileName = Path.GetFileName(isoFile);
                StatusText = $"Converting {fileName} ({i + 1}/{totalFiles})";

                AppendLog($"[{i + 1}/{totalFiles}] Converting: {isoFile}");

                var success = await ConvertFileAsync(extractXisoPath, isoFile, outputFolder, deleteFiles, progress);

                if (success)
                    convertedFiles++;
            }

            // Final report
            StatusText = _cancellationTokenSource.IsCancellationRequested ? "Canceled" : "Completed";

            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                AppendLog("Batch conversion completed.");
                LogColoredMessage($"Conversion completed! Successfully processed {convertedFiles} of {totalFiles} files.",
                    convertedFiles == totalFiles ? Brushes.Green : Brushes.Orange);
            }
        }

        private async Task<bool> ConvertFileAsync(
            string extractXisoPath,
            string isoFile,
            string outputFolder,
            bool deleteFiles,
            IProgress<string> progress)
        {
            try
            {
                // Step 1: Convert the file
                var conversionResult = await RunConversionToolAsync(extractXisoPath, isoFile, progress);
                if (!conversionResult)
                {
                    AppendLog($"Conversion failed: {isoFile}");
                    LogColoredMessage($"ERROR: Failed to convert {isoFile}", Brushes.Red);
                    return false;
                }

                // Step 2: Find the converted file
                var convertedFilePath = await FindConvertedFileAsync(Path.GetFileName(isoFile), progress);
                if (string.IsNullOrEmpty(convertedFilePath))
                {
                    AppendLog($"Could not find converted file for: {Path.GetFileName(isoFile)}");
                    return false;
                }

                // Step 3: Move to output folder
                var fileName = Path.GetFileName(isoFile);
                var destinationPath = Path.Combine(outputFolder, fileName);

                StatusText = $"Moving {fileName} to output folder";
                progress.Report($"Moving converted file to output folder...");

                await Task.Run(() =>
                {
                    Directory.CreateDirectory(outputFolder);
                    File.Move(convertedFilePath, destinationPath, true);
                }, _cancellationTokenSource.Token);

                progress.Report($"Moved converted file from {convertedFilePath} to: {destinationPath}");

                // Step 4: Handle the original file
                var renamedFilePath = isoFile + ".old";
                if (File.Exists(renamedFilePath))
                {
                    if (deleteFiles)
                    {
                        StatusText = $"Cleaning up {fileName}";
                        progress.Report("Deleting original file...");
                        await Task.Run(() => File.Delete(renamedFilePath), _cancellationTokenSource.Token);
                        progress.Report($"Deleted renamed file: {renamedFilePath}");
                    }
                    else
                    {
                        StatusText = $"Restoring {fileName}";
                        progress.Report("Restoring original file...");
                        await Task.Run(() => File.Move(renamedFilePath, isoFile, true), _cancellationTokenSource.Token);
                        progress.Report($"Renamed file back to original: {isoFile}");
                    }
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                progress.Report($"Error processing file: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RunConversionToolAsync(string extractXisoPath, string isoFile, IProgress<string> progress)
        {
            try
            {
                progress.Report($"Starting conversion for file: {isoFile}");
                progress.Report($"Current working directory: {Environment.CurrentDirectory}");

                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = extractXisoPath,
                    Arguments = $"-r \"{isoFile}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                // Setup event handlers for real-time output
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        outputBuilder.AppendLine(args.Data);
                        progress.Report($"extract-xiso: {args.Data}");
                    }
                };

                process.ErrorDataReceived += (_, args) =>
                {
                    if (!string.IsNullOrEmpty(args.Data))
                    {
                        errorBuilder.AppendLine(args.Data);
                        progress.Report($"extract-xiso error: {args.Data}");
                    }
                };

                var token = _cancellationTokenSource.Token;
                await using (token.Register(() =>
                             {
                                 // ReSharper disable once AccessToDisposedClosure
                                 if (!process.HasExited)
                                 {
                                     try
                                     {
                                         // ReSharper disable once AccessToDisposedClosure
                                         process.Kill();
                                     }
                                     catch
                                     {
                                         // ignored
                                     }
                                 }
                             }))
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for the process to complete with a timeout
                    if (!await Task.Run(() => process.WaitForExit(60000), token))
                    {
                        progress.Report("Process timeout: Conversion took too long");
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                            // ignored
                        }

                        return false;
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                progress.Report($"Error during conversion: {ex.Message}");
                return false;
            }
        }

        private async Task<string> FindConvertedFileAsync(string fileName, IProgress<string> progress)
        {
            return await Task.Run(() =>
            {
                // Check potential locations
                var locations = new[]
                {
                    Path.Combine(Environment.CurrentDirectory, fileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName)
                };

                foreach (var location in locations)
                {
                    progress.Report($"Checking for converted file at: {location}");
                    if (File.Exists(location))
                    {
                        progress.Report($"Found converted file at: {location}");
                        return location;
                    }
                }

                // If we couldn't find it, check one more time with the directory info
                var directoryName = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(directoryName))
                {
                    var filePath = Path.Combine(directoryName, Path.GetFileName(fileName));
                    progress.Report($"Checking for converted file at: {filePath}");
                    if (File.Exists(filePath))
                    {
                        progress.Report($"Found converted file at: {filePath}");
                        return filePath;
                    }
                }

                return string.Empty;
            }, _cancellationTokenSource.Token);
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return;
            storage = value;
            OnPropertyChanged(propertyName);
        }

        #endregion
    }

    /// <summary>
    /// Implementation of ICommand that supports async/await pattern.
    /// </summary>
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool>? _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;

            try
            {
                _isExecuting = true;
                CommandManager.InvalidateRequerySuggested();
                await _execute();
            }
            finally
            {
                _isExecuting = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    /// <summary>
    /// Simple relay command implementation for non-async commands.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            if (CanExecute(parameter))
                _execute();
        }
    }
}
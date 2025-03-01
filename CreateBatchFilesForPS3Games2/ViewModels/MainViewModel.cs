using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using CreateBatchFilesForPS3Games2.Interfaces;
using CreateBatchFilesForPS3Games2.Models;

namespace CreateBatchFilesForPS3Games2.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IBatchFileService _batchFileService;
        private readonly ILogger _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        private string _gameFolderPath = string.Empty;
        private string _rpcs3Path = string.Empty;
        private bool _includeInstalledGames = true;
        private bool _overwriteExistingFiles = true;
        private string _logContent = string.Empty;
        private string _statusMessage = "Ready";
        private int _progressValue;
        private bool _isProcessing;

        public string GameFolderPath
        {
            get => _gameFolderPath;
            set => SetProperty(ref _gameFolderPath, value);
        }

        public string Rpcs3Path
        {
            get => _rpcs3Path;
            set => SetProperty(ref _rpcs3Path, value);
        }

        public bool IncludeInstalledGames
        {
            get => _includeInstalledGames;
            set => SetProperty(ref _includeInstalledGames, value);
        }

        public bool OverwriteExistingFiles
        {
            get => _overwriteExistingFiles;
            set => SetProperty(ref _overwriteExistingFiles, value);
        }

        public string LogContent
        {
            get => _logContent;
            set => SetProperty(ref _logContent, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public int ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (SetProperty(ref _isProcessing, value))
                {
                    OnPropertyChanged(nameof(IsNotProcessing));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool IsNotProcessing => !IsProcessing;

        // Commands
        public ICommand BrowseGameFolderCommand { get; }
        public ICommand BrowseRpcs3PathCommand { get; }
        public ICommand CreateBatchFilesCommand { get; }
        public ICommand CancelCommand { get; }

        public MainViewModel(IBatchFileService batchFileService, ILogger logger)
        {
            _batchFileService = batchFileService;
            _logger = logger;

            // Initialize commands
            BrowseGameFolderCommand = new RelayCommand(_ => BrowseGameFolder());
            BrowseRpcs3PathCommand = new RelayCommand(_ => BrowseRpcs3Path());
            CreateBatchFilesCommand = new RelayCommand(_ => CreateBatchFiles(), _ => CanCreateBatchFiles());
            CancelCommand = new RelayCommand(_ => CancelOperation(), _ => IsProcessing);

            // Set up logger
            _logger.OnLogMessageReceived += (_, message) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LogContent += message + Environment.NewLine;
                });
            };

            // Initial log message
            LogMessage("Welcome to PS3 Batch Creator. Please select your game folder and RPCS3 executable.");
        }

        private void LogMessage(string message)
        {
            LogContent += message + Environment.NewLine;
        }

        private void BrowseGameFolder()
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select the root folder where your PS3 game folders are located";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                GameFolderPath = dialog.SelectedPath;
                LogMessage($"Selected game folder: {GameFolderPath}");
            }
        }

        private void BrowseRpcs3Path()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select RPCS3 executable",
                Filter = "RPCS3 Executable (rpcs3.exe)|rpcs3.exe|All files (*.*)|*.*",
                FilterIndex = 1
            };

            if (dialog.ShowDialog() == true)
            {
                Rpcs3Path = dialog.FileName;
                LogMessage($"Selected RPCS3 path: {Rpcs3Path}");
            }
        }

        private bool CanCreateBatchFiles()
        {
            return !string.IsNullOrWhiteSpace(GameFolderPath) &&
                   !string.IsNullOrWhiteSpace(Rpcs3Path) &&
                   !IsProcessing;
        }

        private async void CreateBatchFiles()
        {
            if (!CanCreateBatchFiles())
                return;

            try
            {
                IsProcessing = true;
                ProgressValue = 0;
                StatusMessage = "Starting batch file creation...";

                _cancellationTokenSource = new CancellationTokenSource();

                var options = new BatchCreationOptions
                {
                    GameFolderPath = GameFolderPath,
                    Rpcs3Path = Rpcs3Path,
                    IncludeInstalledGames = IncludeInstalledGames,
                    OverwriteExisting = OverwriteExistingFiles
                };

                var progress = new Progress<BatchCreationProgress>(p =>
                {
                    ProgressValue = p.PercentComplete;
                    StatusMessage = p.StatusMessage;
                });

                var created = await _batchFileService.CreateBatchFilesAsync(
                    options, progress, _cancellationTokenSource.Token);

                ProgressValue = 100;
                StatusMessage = $"Created {created} batch files successfully";

                MessageBox.Show(
                    $"Successfully created {created} batch files.",
                    "Operation Complete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operation canceled";
                LogMessage("Operation was canceled by user");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error occurred";
                LogMessage($"Error creating batch files: {ex.Message}");
                MessageBox.Show(
                    $"An error occurred: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void CancelOperation()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Canceling operation...";
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class DownloadImagePackWindow : IDisposable
{
    private EasyModeManager _manager;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _httpClient;
    private const int HttpTimeoutSeconds = 60;
    private bool _isDownloadCompleted;
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
    private bool _disposed;

    public DownloadImagePackWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Initialize HttpClient with a timeout
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds)
        };

        // Load Config
        _manager = EasyModeManager.Load();
        PopulateSystemDropdown();

        Closed += CloseWindowRoutine;
    }

    private void PopulateSystemDropdown()
    {
        if (_manager?.Systems == null) return;

        // Filter systems that have a valid ExtrasDownloadLink
        var systemsWithImagePacks = _manager.Systems
            .Where(system => !string.IsNullOrEmpty(system.Emulators.Emulator.ExtrasDownloadLink))
            .Select(system => system.SystemName)
            .OrderBy(name => name) // Order by system name
            .ToList();

        SystemNameDropdown.ItemsSource = systemsWithImagePacks;
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null) return;

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem != null)
        {
            DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadLink);

            // Automatically populate the extraction path with default if available
            if (!string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath))
            {
                ExtractionFolderTextBox.Text = selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;
            }
        }
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        // Enable the Stop Button
        StopDownloadButton.IsEnabled = true;

        // Input validation
        if (!ValidateInputs(out var selectedSystem)) return;

        try
        {
            // Reset the flag at the start of the download
            _isDownloadCompleted = false;

            var extrasDownloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;

            // Determine the extraction folder
            var extractionFolder = !string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text)
                ? ExtractionFolderTextBox.Text
                : selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;

            var downloadFilePath = Path.Combine(_tempFolder, Path.GetFileName(extrasDownloadUrl) ??
                                                             throw new InvalidOperationException("'Simple Launcher' could not get extrasDownloadUrl"));

            // Create temp directory if it doesn't exist
            Directory.CreateDirectory(_tempFolder);

            // Check available disk space
            if (!CheckAvailableDiskSpace(_tempFolder))
            {
                MessageBoxLibrary.InsufficientDiskSpaceMessageBox();
                return;
            }

            try
            {
                // Update UI elements
                DownloadProgressBar.Visibility = Visibility.Visible;
                DownloadProgressBar.Value = 0;
                StopDownloadButton.IsEnabled = true;
                DownloadExtrasButton.IsEnabled = false;

                var preparingtodownloadfrom2 = (string)Application.Current.TryFindResource("Preparingtodownloadfrom") ?? "Preparing to download from";
                UpdateStatus($"{preparingtodownloadfrom2} {extrasDownloadUrl}...");

                // Initialize cancellation token source
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();

                // Create a progress reporter
                var progress = new Progress<DownloadProgressInfo>(OnDownloadProgressChanged);

                // Start download
                await DownloadWithProgressAsync(extrasDownloadUrl, downloadFilePath, progress, _cancellationTokenSource.Token);

                // Only proceed with extraction if the download completed successfully
                if (_isDownloadCompleted)
                {
                    var downloadcompleteStartingextractionto2 = (string)Application.Current.TryFindResource("DownloadcompleteStartingextractionto") ?? "Download complete. Starting extraction to";
                    UpdateStatus($"{downloadcompleteStartingextractionto2} {extractionFolder}...");

                    // Show the PleaseWaitExtraction window
                    var pleaseWaitWindow = new PleaseWaitExtractionWindow();
                    pleaseWaitWindow.Show();

                    var extractionSuccess = await ExtractCompressedFile.ExtractDownloadFilesAsync(downloadFilePath, extractionFolder);

                    // Close the PleaseWaitExtraction window
                    pleaseWaitWindow.Close();

                    if (extractionSuccess)
                    {
                        // Notify user
                        MessageBoxLibrary.DownloadExtractionSuccessfullyMessageBox();

                        var imagepackdownloadedandextractedsuccessfully2 = (string)Application.Current.TryFindResource("Imagepackdownloadedandextractedsuccessfully") ?? "Image pack downloaded and extracted successfully.";
                        UpdateStatus(imagepackdownloadedandextractedsuccessfully2);

                        TryToDeleteDownloadedFile(downloadFilePath);

                        // Mark as downloaded and disable button
                        DownloadExtrasButton.IsEnabled = false;
                    }
                    else // Extraction fail
                    {
                        // Notify developer
                        var contextMessage = $"Image Pack extraction failed.\n" +
                                             $"File: {extrasDownloadUrl}";
                        var ex = new Exception(contextMessage);
                        _ = LogErrors.LogErrorAsync(ex, contextMessage);

                        // Notify user
                        MessageBoxLibrary.ExtractionFailedMessageBox();

                        var extractionfailedSeeerrormessagefordetails2 = (string)Application.Current.TryFindResource("ExtractionfailedSeeerrormessagefordetails") ?? "Extraction failed. See error message for details.";
                        UpdateStatus(extractionfailedSeeerrormessagefordetails2);
                    }
                }
                else
                {
                    var downloadwasnotcompletedsuccessfully2 = (string)Application.Current.TryFindResource("Downloadwasnotcompletedsuccessfully") ?? "Download was not completed successfully.";
                    UpdateStatus(downloadwasnotcompletedsuccessfully2);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"Error downloading the Image Pack.\n" +
                                     $"File: {extrasDownloadUrl}";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ImagePackDownloadErrorOfferRedirectMessageBox(selectedSystem);

                var downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror2") ?? "Download error";
                UpdateStatus($"{downloaderror2}: {ex.Message}");
            }
            finally
            {
                StopDownloadButton.IsEnabled = false;
                DownloadExtrasButton.IsEnabled = true;
                TryToDeleteDownloadedFile(downloadFilePath);

                // Dispose cancellation token source
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error downloading the Image Pack.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ImagePackDownloadExtractionFailedMessageBox();

            var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            UpdateStatus($"{error2}: {ex.Message}");
            DownloadExtrasButton.IsEnabled = true;
        }
    }

    private bool ValidateInputs(out EasyModeSystemConfig selectedSystem)
    {
        selectedSystem = null;

        // Check if a system is selected
        if (SystemNameDropdown.SelectedItem == null)
        {
            MessageBoxLibrary.SystemNameIsNullMessageBox();
            return false;
        }

        // Get the selected system
        selectedSystem = _manager.Systems.FirstOrDefault(
            system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());

        if (selectedSystem == null)
        {
            MessageBoxLibrary.SelectedSystemIsNullMessageBox();
            return false;
        }

        // Validate download URL
        var downloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;
        if (string.IsNullOrEmpty(downloadUrl))
        {
            MessageBoxLibrary.DownloadUrlIsNullMessageBox();
            return false;
        }

        // Validate extraction folder
        if (string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text) &&
            string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath))
        {
            MessageBoxLibrary.ExtractionFolderIsNullMessageBox();
            return false;
        }

        var extractionFolder = !string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text)
            ? ExtractionFolderTextBox.Text
            : selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;

        // Verify the extraction folder exists or can be created
        try
        {
            if (!Directory.Exists(extractionFolder))
            {
                Directory.CreateDirectory(extractionFolder);
            }
        }
        catch (Exception ex)
        {
            MessageBoxLibrary.ExtractionFolderCannotBeCreatedMessageBox(ex);
            return false;
        }

        return true;
    }

    private static bool CheckAvailableDiskSpace(string folderPath)
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(folderPath) ?? throw new InvalidOperationException("Could not get the HDD info"));
            // Require at least 1GB free space (adjust as needed)
            const long requiredSpace = 1024 * 1024 * 1024;
            return driveInfo.AvailableFreeSpace > requiredSpace;
        }
        catch
        {
            // If we can't check disk space, assume it's enough
            return true;
        }
    }

    private static void TryToDeleteDownloadedFile(string file)
    {
        if (!File.Exists(file)) return;
        try
        {
            File.Delete(file);
        }
        catch (Exception)
        {
            // ignore
        }
    }

    // Handle progress updates on UI thread
    private void OnDownloadProgressChanged(DownloadProgressInfo progressInfo)
    {
        DownloadProgressBar.Value = progressInfo.ProgressPercentage;
        UpdateStatus(progressInfo.StatusMessage);
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        var counter = 0;
        double size = bytes;
        while (size > 1024 && counter < suffixes.Length - 1)
        {
            size /= 1024;
            counter++;
        }

        return $"{size:F2} {suffixes[counter]}";
    }

    private void UpdateStatus(string message)
    {
        // Update the status text block
        StatusTextBlock.Text = message;
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource == null) return;

        // Cancel the ongoing download
        _cancellationTokenSource.Cancel();

        // Disable the stop button once the download is canceled
        StopDownloadButton.IsEnabled = false;

        // Reset completion flag and progress
        _isDownloadCompleted = false;
        DownloadProgressBar.Value = 0;
        var downloadcanceled2 = (string)Application.Current.TryFindResource("Downloadcanceled") ?? "Download canceled";
        UpdateStatus(downloadcanceled2);

        // Enable download button again
        DownloadExtrasButton.IsEnabled = true;
    }

    private void CloseWindowRoutine(object sender, EventArgs e)
    {
        // Empty EasyMode Config
        _manager = null;
        Dispose();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void ChooseExtractionFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var selectafoldertoextracttheImagePack2 = (string)Application.Current.TryFindResource("SelectafoldertoextracttheImagePack") ?? "Select a folder to extract the Image Pack";

        // Create a new StorageFolder picker
        var openFolderDialog = new OpenFolderDialog
        {
            Title = selectafoldertoextracttheImagePack2
        };

        // Show the dialog and handle the result
        if (openFolderDialog.ShowDialog() == true)
        {
            ExtractionFolderTextBox.Text = openFolderDialog.FolderName;
        }
    }

    // IDisposable implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _cancellationTokenSource?.Dispose();
            _httpClient?.Dispose();
        }

        _disposed = true;
    }

    ~DownloadImagePackWindow()
    {
        Dispose(false);
    }
}
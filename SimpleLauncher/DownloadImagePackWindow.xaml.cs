using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class DownloadImagePackWindow : IDisposable
{
    private EasyModeConfig _config;
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
        _config = EasyModeConfig.Load();
        PopulateSystemDropdown();

        Closed += CloseWindowRoutine;
    }

    private void PopulateSystemDropdown()
    {
        if (_config?.Systems == null) return;

        // Filter systems that have a valid ExtrasDownloadLink
        var systemsWithImagePacks = _config.Systems
            .Where(system => !string.IsNullOrEmpty(system.Emulators.Emulator.ExtrasDownloadLink))
            .Select(system => system.SystemName)
            .OrderBy(name => name) // Order by system name
            .ToList();

        SystemNameDropdown.ItemsSource = systemsWithImagePacks;
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null) return;

        var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
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

                    var extractionSuccess = await ExtractCompressedFile.ExtractDownloadFilesAsync2(downloadFilePath, extractionFolder);

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
                        var formattedException = $"Image Pack extraction failed.\n\n" +
                                                 $"File: {extrasDownloadUrl}";
                        var ex = new Exception(formattedException);
                        await LogErrors.LogErrorAsync(ex, formattedException);

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
                var formattedException = $"Error downloading the Image Pack.\n\n" +
                                         $"File: {extrasDownloadUrl}\n" +
                                         $"Exception type: {ex.GetType().Name}\n" +
                                         $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);

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
            var formattedException = $"Error downloading the Image Pack.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

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
        selectedSystem = _config.Systems.FirstOrDefault(
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

    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath,
        IProgress<DownloadProgressInfo> progress, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(downloadUrl,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var unknownsize2 = (string)Application.Current.TryFindResource("unknownsize") ?? "unknown size";
            var totalBytes = response.Content.Headers.ContentLength;
            var totalSizeFormatted = totalBytes.HasValue
                ? FormatFileSize(totalBytes.Value)
                : unknownsize2;

            var startingdownload2 = (string)Application.Current.TryFindResource("Startingdownload2") ?? "Starting download";
            progress.Report(new DownloadProgressInfo
            {
                BytesReceived = 0,
                TotalBytesToReceive = totalBytes,
                ProgressPercentage = 0,
                StatusMessage = $"{startingdownload2}: {Path.GetFileName(downloadUrl)} ({totalSizeFormatted})"
            });

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create,
                FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;
            var lastProgressUpdate = DateTime.Now;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                // Limit progress updates to reduce UI thread congestion (update every ~100ms)
                var now = DateTime.Now;
                if ((now - lastProgressUpdate).TotalMilliseconds >= 100)
                {
                    var progressPercentage = totalBytes.HasValue
                        ? (double)totalBytesRead / totalBytes.Value * 100
                        : 0;

                    var sizeStatus = totalBytes.HasValue
                        ? $"{FormatFileSize(totalBytesRead)} of {FormatFileSize(totalBytes.Value)}"
                        : $"{FormatFileSize(totalBytesRead)} of {totalSizeFormatted}";

                    var downloading2 = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
                    progress.Report(new DownloadProgressInfo
                    {
                        BytesReceived = totalBytesRead,
                        TotalBytesToReceive = totalBytes,
                        ProgressPercentage = progressPercentage,
                        StatusMessage = $"{downloading2}: {sizeStatus} ({progressPercentage:F1}%)"
                    });

                    lastProgressUpdate = now;
                }
            }

            // Check if the file was fully downloaded
            if (totalBytes.HasValue && totalBytesRead == totalBytes.Value)
            {
                _isDownloadCompleted = true;
                var downloadcomplete2 = (string)Application.Current.TryFindResource("Downloadcomplete2") ?? "Download complete";
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = 100,
                    StatusMessage = $"{downloadcomplete2}: {FormatFileSize(totalBytesRead)}"
                });
            }
            else if (totalBytes.HasValue)
            {
                _isDownloadCompleted = false;
                var downloadincomplete2 = (string)Application.Current.TryFindResource("Downloadincomplete") ?? "Download incomplete";
                var expected2 = (string)Application.Current.TryFindResource("Expected") ?? "Expected";
                var butreceived2 = (string)Application.Current.TryFindResource("butreceived") ?? "but received";
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = 0,
                    StatusMessage = $"{downloadincomplete2}: {expected2} {FormatFileSize(totalBytes.Value)} {butreceived2} {FormatFileSize(totalBytesRead)}"
                });
                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
            }
            else
            {
                // If the server didn't provide a content length, we assume the download is complete
                _isDownloadCompleted = true;
                var downloadcomplete2 = (string)Application.Current.TryFindResource("Downloadcomplete2") ?? "Download complete";
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytesRead, // Use received as total
                    ProgressPercentage = 100,
                    StatusMessage = $"{downloadcomplete2}: {FormatFileSize(totalBytesRead)}"
                });
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            // Notify developer
            var formattedException = $"The requested file was not available on the server.\n\n" +
                                     $"URL: {downloadUrl}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.DownloadErrorMessageBox();
            var errorFilenotfoundontheserver2 = (string)Application.Current.TryFindResource("ErrorFilenotfoundontheserver") ?? "Error: File not found on the server";
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = errorFilenotfoundontheserver2
            });
        }
        catch (HttpRequestException ex)
        {
            // Notify developer
            var formattedException = $"Network error during file download.\n\n" +
                                     $"URL: {downloadUrl}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.DownloadErrorMessageBox();
            var networkerror2 = (string)Application.Current.TryFindResource("Networkerror") ?? "Network error";
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"{networkerror2}: {ex.Message}"
            });
        }
        catch (IOException ex)
        {
            // Notify developer
            var formattedException = $"File read/write error after file download.\n\n" +
                                     $"URL: {downloadUrl}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.IoExceptionMessageBox(_tempFolder);
            var fileerror2 = (string)Application.Current.TryFindResource("Fileerror") ?? "File error";
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"{fileerror2}: {ex.Message}"
            });
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Notify developer
                var formattedException = $"Download was canceled by the user. User was not notified.\n\n" +
                                         $"URL: {downloadUrl}\n" +
                                         $"Exception type: {ex.GetType().Name}\n" +
                                         $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);

                var downloadcanceledbyuser2 = (string)Application.Current.TryFindResource("Downloadcanceledbyuser2") ?? "Download canceled by user";
                progress.Report(new DownloadProgressInfo
                {
                    ProgressPercentage = 0,
                    StatusMessage = downloadcanceledbyuser2
                });
            }
            else
            {
                // Notify developer
                var formattedException = $"Download timed out or was canceled unexpectedly.\n\n" +
                                         $"URL: {downloadUrl}\n" +
                                         $"Exception type: {ex.GetType().Name}\n" +
                                         $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);

                // Notify user
                MessageBoxLibrary.DownloadErrorMessageBox();
                var downloadtimedoutorwascanceledunexpectedly2 = (string)Application.Current.TryFindResource("Downloadtimedoutorwascanceledunexpectedly") ?? "Download timed out or was canceled unexpectedly";
                progress.Report(new DownloadProgressInfo
                {
                    ProgressPercentage = 0,
                    StatusMessage = downloadtimedoutorwascanceledunexpectedly2
                });
            }

            TryToDeleteDownloadedFile(destinationPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"Generic download error.\n\n" +
                                     $"URL: {downloadUrl}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.DownloadErrorMessageBox();
            var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"{error2}: {ex.Message}"
            });
        }
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
        _config = null;
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
        using var dialog = new System.Windows.Forms.FolderBrowserDialog();
        dialog.Description = selectafoldertoextracttheImagePack2;
        dialog.UseDescriptionForTitle = true;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ExtractionFolderTextBox.Text = dialog.SelectedPath;
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

    // Progress information class
    private class DownloadProgressInfo
    {
        public long BytesReceived { get; set; }
        public long? TotalBytesToReceive { get; set; }
        public double ProgressPercentage { get; set; }
        public string StatusMessage { get; set; }
    }
}
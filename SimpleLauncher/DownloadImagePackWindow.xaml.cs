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
    private bool _isDownloadCompleted;
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
    private bool _disposed;
    private const int HttpTimeoutSeconds = 60;
    
    // Status text element (add this to your XAML)
    // <TextBlock x:Name="StatusTextBlock" Grid.Row="7" Margin="10,5,10,5" TextWrapping="Wrap" />

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

        Closed += EditSystemEasyModeAddSystem_Closed;
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
        // Input validation
        if (!ValidateInputs(out var selectedSystem)) return;
        
        try
        {
            // Reset the flag at the start of the download
            _isDownloadCompleted = false;
            
            string extrasDownloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;

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
                UpdateStatus($"Preparing to download from {extrasDownloadUrl}...");

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
                    UpdateStatus($"Download complete. Starting extraction to {extractionFolder}...");
                    
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
                        UpdateStatus("Image pack downloaded and extracted successfully.");

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
                        UpdateStatus("Extraction failed. See error message for details.");
                    }
                }
                else
                {
                    UpdateStatus("Download was not completed successfully.");
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
                UpdateStatus($"Download error: {ex.Message}");
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
            UpdateStatus($"Error: {ex.Message}");
            DownloadExtrasButton.IsEnabled = true;
        }
    }

    private bool ValidateInputs(out EasyModeSystemConfig selectedSystem)
{
    selectedSystem = null;
    
    // Check if a system is selected
    if (SystemNameDropdown.SelectedItem == null)
    {
        MessageBox.Show("Please select a system from the dropdown.",
            "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    // Get the selected system
    selectedSystem = _config.Systems.FirstOrDefault(
        system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
    
    if (selectedSystem == null)
    {
        MessageBox.Show("Could not find the selected system in the configuration.",
            "System Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
    }

    // Validate download URL
    var downloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;
    if (string.IsNullOrEmpty(downloadUrl))
    {
        MessageBox.Show("The selected system does not have a valid download link.",
            "Invalid Download Link", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    // Validate extraction folder
    if (string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text) && 
        string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath))
    {
        MessageBox.Show("Please select an extraction folder.",
            "Extraction Folder Required", MessageBoxButton.OK, MessageBoxImage.Warning);
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
        MessageBox.Show($"Cannot create or access the extraction folder: {ex.Message}",
            "Invalid Extraction Folder", MessageBoxButton.OK, MessageBoxImage.Error);
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

    // Progress information class
    private class DownloadProgressInfo
    {
        public long BytesReceived { get; set; }
        public long? TotalBytesToReceive { get; set; }
        public double ProgressPercentage { get; set; }
        public string StatusMessage { get; set; }
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

            var totalBytes = response.Content.Headers.ContentLength;
            string totalSizeFormatted = totalBytes.HasValue 
                ? FormatFileSize(totalBytes.Value) 
                : "unknown size";
            
            progress.Report(new DownloadProgressInfo
            {
                BytesReceived = 0,
                TotalBytesToReceive = totalBytes,
                ProgressPercentage = 0,
                StatusMessage = $"Starting download: {Path.GetFileName(downloadUrl)} ({totalSizeFormatted})"
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
                    double progressPercentage = totalBytes.HasValue 
                        ? (double)totalBytesRead / totalBytes.Value * 100 
                        : 0;
                    
                    string sizeStatus = totalBytes.HasValue
                        ? $"{FormatFileSize(totalBytesRead)} of {FormatFileSize(totalBytes.Value)}"
                        : $"{FormatFileSize(totalBytesRead)} of {totalSizeFormatted}";
                    
                    progress.Report(new DownloadProgressInfo
                    {
                        BytesReceived = totalBytesRead,
                        TotalBytesToReceive = totalBytes,
                        ProgressPercentage = progressPercentage,
                        StatusMessage = $"Downloading: {sizeStatus} ({progressPercentage:F1}%)"
                    });
                    
                    lastProgressUpdate = now;
                }
            }

            // Check if the file was fully downloaded
            if (totalBytes.HasValue && totalBytesRead == totalBytes.Value)
            {
                _isDownloadCompleted = true;
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = 100,
                    StatusMessage = $"Download complete: {FormatFileSize(totalBytesRead)}"
                });
            }
            else if (totalBytes.HasValue)
            {
                _isDownloadCompleted = false;
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytes,
                    ProgressPercentage = 0,
                    StatusMessage = $"Download incomplete: Expected {FormatFileSize(totalBytes.Value)} but received {FormatFileSize(totalBytesRead)}"
                });
                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
            }
            else
            {
                // If the server didn't provide a content length, we assume the download is complete
                _isDownloadCompleted = true;
                progress.Report(new DownloadProgressInfo
                {
                    BytesReceived = totalBytesRead,
                    TotalBytesToReceive = totalBytesRead, // Use received as total
                    ProgressPercentage = 100,
                    StatusMessage = $"Download complete: {FormatFileSize(totalBytesRead)}"
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
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = "Error: File not found on the server"
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
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"Network error: {ex.Message}"
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
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"File error: {ex.Message}"
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
                
                progress.Report(new DownloadProgressInfo
                {
                    ProgressPercentage = 0,
                    StatusMessage = "Download canceled by user"
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
                progress.Report(new DownloadProgressInfo
                {
                    ProgressPercentage = 0,
                    StatusMessage = "Download timed out or was canceled unexpectedly"
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
            progress.Report(new DownloadProgressInfo
            {
                ProgressPercentage = 0,
                StatusMessage = $"Error: {ex.Message}"
            });
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
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
        UpdateStatus("Download canceled");

        // Enable download button again
        DownloadExtrasButton.IsEnabled = true;
    }

    private void EditSystemEasyModeAddSystem_Closed(object sender, EventArgs e)
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
}
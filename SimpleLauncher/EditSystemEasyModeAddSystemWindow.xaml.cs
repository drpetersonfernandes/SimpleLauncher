using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Xml.Linq;
using Application = System.Windows.Application;
using System.Security.Authentication;

namespace SimpleLauncher;

public partial class EditSystemEasyModeAddSystemWindow
{
    private EasyModeConfig _config;
    private bool _isEmulatorDownloaded;
    private bool _isCoreDownloaded;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _httpClient;
    private const int HttpTimeoutSeconds = 60;
    private bool _isDownloadCompleted;
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
    private bool _isUserCancellation;

    // Download component types
    private enum DownloadType
    {
        Emulator,
        Core,
        ImagePack
    }

    // Download status tracking
    private string _downloadStatus = string.Empty;

    private string DownloadStatus
    {
        get => _downloadStatus;
        set
        {
            _downloadStatus = value;
            DownloadStatusTextBlock.Text = value;
        }
    }

    public EditSystemEasyModeAddSystemWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Load Config
        _config = EasyModeConfig.Load();
        PopulateSystemDropdown();

        // Initialize HttpClient with custom handler to configure TLS
        var handler = new HttpClientHandler();

        // Configure TLS 1.2 and 1.3 if available
        handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

        // Initialize HttpClient with a timeout
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds)
        };

        // Initialize temp folder
        Directory.CreateDirectory(_tempFolder);

        Closed += CloseWindowRoutine;
    }

    private void PopulateSystemDropdown()
    {
        if (_config?.Systems == null) return;
        var sortedSystemNames = _config.Systems
            .Where(system => !string.IsNullOrEmpty(system.Emulators?.Emulator?.EmulatorDownloadLink)) // only if EmulatorDownloadLink is not null
            .Select(system => system.SystemName)
            .OrderBy(name => name) // order by name
            .ToList();

        SystemNameDropdown.ItemsSource = sortedSystemNames;
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null) return;

        var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem != null)
        {
            DownloadEmulatorButton.IsEnabled = true;
            DownloadCoreButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.CoreDownloadLink);
            DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadLink);

            // Reset download status
            _isEmulatorDownloaded = false;
            _isCoreDownloaded = !DownloadCoreButton.IsEnabled;

            UpdateAddSystemButtonState();
        }
    }

    private async void DownloadEmulatorButton_Click(object sender, RoutedEventArgs e)
    {
        DownloadEmulatorButton.IsEnabled = false;
        _isEmulatorDownloaded = false;
        UpdateAddSystemButtonState();

        var success = await DownloadAndExtractAsync(DownloadType.Emulator);

        if (success)
        {
            _isEmulatorDownloaded = true;
        }
        else
        {
            // Re-enable the button unless it was a successful download
            DownloadEmulatorButton.IsEnabled = true;
        }

        UpdateAddSystemButtonState();
    }

    private async void DownloadCoreButton_Click(object sender, RoutedEventArgs e)
    {
        DownloadCoreButton.IsEnabled = false;
        _isCoreDownloaded = false;
        UpdateAddSystemButtonState();

        var success = await DownloadAndExtractAsync(DownloadType.Core);

        if (success)
        {
            _isCoreDownloaded = true;
        }
        else
        {
            // Re-enable the button unless it was a successful download
            DownloadCoreButton.IsEnabled = true;
        }

        UpdateAddSystemButtonState();
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        DownloadExtrasButton.IsEnabled = false;

        var success = await DownloadAndExtractAsync(DownloadType.ImagePack);

        // Re-enable the button if not successful and not user-canceled
        if (!success)
        {
            DownloadExtrasButton.IsEnabled = true;
        }
    }

    private async Task<bool> TryDownloadWithRetryAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        var currentRetry = 0;

        while (currentRetry < maxRetries)
        {
            try
            {
                await DownloadWithProgressAsync(downloadUrl, destinationPath, cancellationToken);
                return true; // Success
            }
            catch (IOException ex) when (ex.InnerException is Win32Exception winEx &&
                                         unchecked((uint)winEx.NativeErrorCode == 0x80090330))
            {
                currentRetry++;
                if (currentRetry >= maxRetries)
                    throw; // Re-throw if max retries reached

                // Log retry attempt
                var retryAttempt = (string)Application.Current.TryFindResource("RetryingDownload") ?? $"SSL connection error, retrying ({currentRetry}/{maxRetries})...";
                DownloadStatus = retryAttempt;

                // Wait before retrying
                await Task.Delay(2000, cancellationToken);
            }
        }

        return false;
    }

    private async Task<bool> DownloadAndExtractAsync(DownloadType downloadType)
    {
        _isDownloadCompleted = false;
        _isUserCancellation = false; // Reset the cancellation flag
        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return false;

        string downloadUrl;
        string destinationPath;
        string componentName;
        string downloadFilePath;

        // Configure based on the download type
        switch (downloadType)
        {
            case DownloadType.Emulator:
                downloadUrl = selectedSystem.Emulators.Emulator.EmulatorDownloadLink;
                destinationPath = selectedSystem.Emulators.Emulator.EmulatorDownloadExtractPath;
                componentName = "Emulator";
                break;
            case DownloadType.Core:
                downloadUrl = selectedSystem.Emulators.Emulator.CoreDownloadLink;
                destinationPath = selectedSystem.Emulators.Emulator.CoreDownloadExtractPath;
                componentName = "Core";
                break;
            case DownloadType.ImagePack:
                downloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;
                destinationPath = selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;
                componentName = "Image Pack";
                break;
            default:
                return false;
        }

        // Ensure valid URL
        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor2 = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            DownloadStatus = $"{errorNodownloadUrLfor2} {componentName}";
            return false;
        }

        // Setup download file path
        try
        {
            var fileName = Path.GetFileName(downloadUrl);
            downloadFilePath = Path.Combine(_tempFolder, fileName);
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorcreatingdownloadpathfor2 = (string)Application.Current.TryFindResource("Errorcreatingdownloadpathfor") ?? "Error creating download path for";
            await LogErrors.LogErrorAsync(ex, $"{errorcreatingdownloadpathfor2} {componentName}");

            // Notify user
            MessageBoxLibrary.DownloadExtractionFailedMessageBox();

            return false;
        }

        try
        {
            // Reset status
            var preparingtodownload2 = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            DownloadStatus = $"{preparingtodownload2} {componentName}...";

            // Display progress bar
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;

            // Initialize cancellation token source
            _cancellationTokenSource = new CancellationTokenSource();

            // Start download with retry logic
            var downloading2 = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            DownloadStatus = $"{downloading2} {componentName}...";

            // Implement retry logic
            const int maxRetries = 3;
            var currentRetry = 0;
            const int baseDelay = 1000; // 1 second initial delay
            var downloadSuccess = false;

            while (currentRetry < maxRetries && !_isUserCancellation)
            {
                try
                {
                    await DownloadWithProgressAsync(downloadUrl, downloadFilePath, _cancellationTokenSource.Token);
                    downloadSuccess = _isDownloadCompleted;
                    if (downloadSuccess) break; // Exit retry loop on success

                    currentRetry++;
                    if (currentRetry < maxRetries && !_isUserCancellation)
                    {
                        // Calculate delay with exponential backoff
                        var delay = baseDelay * (int)Math.Pow(2, currentRetry - 1);
                        var retrying = (string)Application.Current.TryFindResource("RetryingDownload") ??
                                       $"Download incomplete, retrying ({currentRetry}/{maxRetries})...";
                        DownloadStatus = retrying;
                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                }
                catch (TaskCanceledException)
                {
                    if (_isUserCancellation) break; // Exit if user canceled

                    currentRetry++;
                    if (currentRetry < maxRetries)
                    {
                        // Calculate delay with exponential backoff
                        var delay = baseDelay * (int)Math.Pow(2, currentRetry - 1);
                        var retrying = (string)Application.Current.TryFindResource("RetryingDownload") ??
                                       $"Connection timeout, retrying ({currentRetry}/{maxRetries})...";
                        DownloadStatus = retrying;
                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Notify developer
                    await LogErrors.LogErrorAsync(ex, $"HTTP error during download attempt {currentRetry + 1}: {ex.Message}");

                    currentRetry++;
                    if (currentRetry < maxRetries && !_isUserCancellation)
                    {
                        // Calculate delay with exponential backoff
                        var delay = baseDelay * (int)Math.Pow(2, currentRetry - 1);
                        var retrying = (string)Application.Current.TryFindResource("RetryingDownload") ??
                                       $"Connection error, retrying ({currentRetry}/{maxRetries})...";
                        DownloadStatus = retrying;
                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    await LogErrors.LogErrorAsync(ex, $"Unexpected error during download attempt {currentRetry + 1}: {ex.Message}");

                    currentRetry++;
                    if (currentRetry < maxRetries && !_isUserCancellation)
                    {
                        // Calculate delay with exponential backoff
                        var delay = baseDelay * (int)Math.Pow(2, currentRetry - 1);
                        var retrying = (string)Application.Current.TryFindResource("RetryingDownload") ??
                                       $"Error occurred, retrying ({currentRetry}/{maxRetries})...";
                        DownloadStatus = retrying;
                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                }
            }

            // Only proceed with extraction if download was successful
            if (downloadSuccess)
            {
                // Special handling for emulator download that might need extension change
                if (downloadType == DownloadType.Emulator)
                {
                    downloadFilePath = ChangeFileExtensionFunction(selectedSystem, downloadFilePath);
                }

                // Show the extraction window
                var extracting2 = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                DownloadStatus = $"{extracting2} {componentName}...";
                var pleaseWaitWindow = new PleaseWaitExtractionWindow();
                pleaseWaitWindow.Show();

                var extractionSuccess = await ExtractCompressedFile.ExtractDownloadFilesAsync(downloadFilePath, destinationPath);

                // Close extraction window
                pleaseWaitWindow.Close();

                if (extractionSuccess)
                {
                    // Notify user
                    var hasbeensuccessfullydownloadedandinstalled2 = (string)Application.Current.TryFindResource("hasbeensuccessfullydownloadedandinstalled") ?? "has been successfully downloaded and installed.";
                    DownloadStatus = $"{componentName} {hasbeensuccessfullydownloadedandinstalled2}";
                    MessageBoxLibrary.DownloadAndExtrationWereSuccessfulMessageBox();

                    // Clean up the downloaded file only if extraction is successful
                    TryDeleteFile(downloadFilePath);

                    return true;
                }
                else
                {
                    // Log extraction failure
                    var errorFailedtoextract2 = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    DownloadStatus = $"{errorFailedtoextract2} {componentName}.";

                    // Notify developer
                    var formattedException = $"{componentName} extraction failed.\n\n" +
                                             $"File: {downloadFilePath}";
                    var ex = new Exception(formattedException);
                    await LogErrors.LogErrorAsync(ex, formattedException);

                    // Notify user
                    MessageBoxLibrary.ExtractionFailedMessageBox();
                }
            }
            else if (!_isUserCancellation) // Only show error if not user-canceled
            {
                // Log download failure after all retries
                var errorFailedtodownload2 = (string)Application.Current.TryFindResource("ErrorFailedtodownload") ?? "Error: Failed to download";
                DownloadStatus = $"{errorFailedtodownload2} {componentName} after {maxRetries} attempts.";

                // Notify developer
                var formattedException = $"{componentName} download failed after {maxRetries} attempts.";
                var ex = new Exception(formattedException);
                await LogErrors.LogErrorAsync(ex, formattedException);

                // Notify user
                MessageBoxLibrary.DownloadExtractionFailedMessageBox();
            }
        }
        catch (TaskCanceledException)
        {
            if (_isUserCancellation)
            {
                var downloadof2 = (string)Application.Current.TryFindResource("Downloadof") ?? "Download of";
                var wascanceled2 = (string)Application.Current.TryFindResource("wascanceled") ?? "was canceled.";
                DownloadStatus = $"{downloadof2} {componentName} {wascanceled2}";
                // Don't show an error message for user cancellation
            }
            else
            {
                var errorDownloadtimedout2 = (string)Application.Current.TryFindResource("ErrorDownloadtimedout") ?? "Error: Download timed out.";
                DownloadStatus = errorDownloadtimedout2;

                // Notify developer
                await LogErrors.LogErrorAsync(new Exception("Download timed out"), "Download timed out");

                // Notify user
                MessageBoxLibrary.DownloadExtractionFailedMessageBox();
            }

            TryDeleteFile(downloadFilePath);
        }
        catch (Exception ex)
        {
            var errorduring2 = (string)Application.Current.TryFindResource("Errorduring") ?? "Error during";
            var downloadprocess2 = (string)Application.Current.TryFindResource("downloadprocess") ?? "download process.";
            DownloadStatus = $"{errorduring2} {componentName} {downloadprocess2}";

            // Notify developer
            var formattedException = $"Error downloading {componentName}.\n\n" +
                                     $"File: {downloadFilePath}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            // Show the appropriate error message based on the download type
            switch (downloadType)
            {
                case DownloadType.Emulator:
                    await MessageBoxLibrary.EmulatorDownloadErrorMessageBox(selectedSystem, ex);
                    break;
                case DownloadType.Core:
                    await MessageBoxLibrary.CoreDownloadErrorMessageBox(selectedSystem, ex);
                    break;
                case DownloadType.ImagePack:
                    await MessageBoxLibrary.ImagePackDownloadErrorMessageBox(selectedSystem, ex);
                    break;
                default:
                    MessageBoxLibrary.DownloadExtractionFailedMessageBox();
                    break;
            }
        }
        finally
        {
            StopDownloadButton.IsEnabled = false;
            TryDeleteFile(downloadFilePath);
        }

        return false;
    }

    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        HttpClient localClient = null;

        try
        {
            // Use the main HttpClient first
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            var totalMb = totalBytes.HasValue ? Math.Round((double)totalBytes.Value / (1024 * 1024), 2) : 0;
            var startingdownload2 = (string)Application.Current.TryFindResource("Startingdownload") ?? "Starting download:";
            var total2 = (string)Application.Current.TryFindResource("total") ?? "total";
            var sizeunknown2 = (string)Application.Current.TryFindResource("sizeunknown") ?? "size unknown";
            DownloadStatus = $"{startingdownload2} {(totalMb > 0 ? $"{totalMb} MB {total2}" : sizeunknown2)}";

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true);
            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            // For throttling UI updates
            var lastUpdateTime = DateTime.Now;
            const int updateIntervalMs = 100; // Update UI every 100 ms
            var lastProgressReported = 0.0;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                // Only update UI if enough time has passed since the last update
                // or if progress has changed significantly
                var now = DateTime.Now;
                var progress = totalBytes.HasValue ? (double)totalBytesRead / totalBytes.Value * 100 : 0;
                var significantChange = Math.Abs(progress - lastProgressReported) >= 1.0; // 1% change threshold

                if ((now - lastUpdateTime).TotalMilliseconds >= updateIntervalMs || significantChange)
                {
                    lastUpdateTime = now;
                    lastProgressReported = progress;

                    if (totalBytes.HasValue)
                    {
                        DownloadProgressBar.Value = progress;
                        var downloadedMb = Math.Round((double)totalBytesRead / (1024 * 1024), 2);
                        var downloaded2 = (string)Application.Current.TryFindResource("Downloaded") ?? "Downloaded";
                        DownloadStatus = $"{downloaded2} {downloadedMb} MB of {totalMb} MB ({progress:F1}%)";
                    }
                    else
                    {
                        var downloadedMb = Math.Round((double)totalBytesRead / (1024 * 1024), 2);
                        var downloaded2 = (string)Application.Current.TryFindResource("Downloaded") ?? "Downloaded";
                        var totalsizeunknown2 = (string)Application.Current.TryFindResource("totalsizeunknown") ?? "total size unknown";
                        DownloadStatus = $"{downloaded2} {downloadedMb} MB ({totalsizeunknown2})";
                    }
                }
            }

            // Check if the file was fully downloaded
            if (totalBytes.HasValue && totalBytesRead == totalBytes.Value)
            {
                _isDownloadCompleted = true;
                var downloadcomplete2 = (string)Application.Current.TryFindResource("Downloadcomplete") ?? "Download complete:";
                DownloadStatus = $"{downloadcomplete2} {Math.Round((double)totalBytesRead / (1024 * 1024), 2)} MB";
            }
            else if (totalBytes.HasValue)
            {
                _isDownloadCompleted = false;
                var downloadincompleteDownloadedbytesdonotmatchexpectedfilesize2 = (string)Application.Current.TryFindResource("DownloadincompleteDownloadedbytesdonotmatchexpectedfilesize") ?? "Download incomplete. Downloaded bytes do not match expected file size.";
                DownloadStatus = downloadincompleteDownloadedbytesdonotmatchexpectedfilesize2;
                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
            }
            else
            {
                // If we don't know the total size, assume it's complete
                _isDownloadCompleted = true;
                var downloadcomplete2 = (string)Application.Current.TryFindResource("Downloadcomplete") ?? "Download complete:";
                DownloadStatus = $"{downloadcomplete2} {Math.Round((double)totalBytesRead / (1024 * 1024), 2)} MB";
            }
        }
        catch (HttpRequestException ex) when (ex.InnerException is AuthenticationException ||
                                              (ex.InnerException is IOException ioEx &&
                                               ioEx.Message.Contains("decryption operation failed")))
        {
            // Handle SSL/TLS errors with fallback method
            var errorSslConnection = (string)Application.Current.TryFindResource("ErrorSSLConnection") ?? "SSL/TLS connection issue. Trying alternate connection method...";
            DownloadStatus = errorSslConnection;

            // Notify developer
            await LogErrors.LogErrorAsync(ex, $"SSL/TLS error: {ex.Message}. Inner exception: {ex.InnerException?.Message}");

            try
            {
                // Create a fallback handler with relaxed SSL validation
                var fallbackHandler = new HttpClientHandler
                {
                    // Temporarily bypass certificate validation for this specific download
                    ServerCertificateCustomValidationCallback = (sender, certificate, chain, errors) => true,
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                };

                // Create a new client for this fallback attempt
                localClient = new HttpClient(fallbackHandler)
                {
                    Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds)
                };

                // Try download with a fallback client
                var fallbackResponse = await localClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                fallbackResponse.EnsureSuccessStatusCode();

                var totalBytes = fallbackResponse.Content.Headers.ContentLength;
                var totalMb = totalBytes.HasValue ? Math.Round((double)totalBytes.Value / (1024 * 1024), 2) : 0;
                var fallbackdownload = (string)Application.Current.TryFindResource("FallbackDownload") ?? "Using fallback connection:";
                DownloadStatus = $"{fallbackdownload} {(totalMb > 0 ? $"{totalMb} MB total" : "size unknown")}";

                await using var contentStream = await fallbackResponse.Content.ReadAsStreamAsync(cancellationToken);
                await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true);
                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;

                // For throttling UI updates
                var lastUpdateTime = DateTime.Now;
                const int updateIntervalMs = 100; // Update UI every 100 ms

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalBytesRead += bytesRead;

                    // Only update UI if enough time has passed since the last update
                    var now = DateTime.Now;
                    if ((now - lastUpdateTime).TotalMilliseconds >= updateIntervalMs)
                    {
                        lastUpdateTime = now;

                        if (totalBytes.HasValue)
                        {
                            var progress = (double)totalBytesRead / totalBytes.Value * 100;
                            DownloadProgressBar.Value = progress;
                            var downloadedMb = Math.Round((double)totalBytesRead / (1024 * 1024), 2);
                            var downloaded2 = (string)Application.Current.TryFindResource("Downloaded") ?? "Downloaded";
                            DownloadStatus = $"{downloaded2} {downloadedMb} MB of {totalMb} MB ({progress:F1}%) [fallback]";
                        }
                        else
                        {
                            var downloadedMb = Math.Round((double)totalBytesRead / (1024 * 1024), 2);
                            var downloaded2 = (string)Application.Current.TryFindResource("Downloaded") ?? "Downloaded";
                            var totalsizeunknown2 = (string)Application.Current.TryFindResource("totalsizeunknown") ?? "total size unknown";
                            DownloadStatus = $"{downloaded2} {downloadedMb} MB ({totalsizeunknown2}) [fallback]";
                        }
                    }
                }

                // Check if the file was fully downloaded
                if (totalBytes.HasValue && totalBytesRead == totalBytes.Value)
                {
                    _isDownloadCompleted = true;
                    var downloadcomplete2 = (string)Application.Current.TryFindResource("Downloadcomplete") ?? "Download complete:";
                    DownloadStatus = $"{downloadcomplete2} {Math.Round((double)totalBytesRead / (1024 * 1024), 2)} MB [fallback]";
                }
                else if (totalBytes.HasValue)
                {
                    _isDownloadCompleted = false;
                    var downloadincompleteDownloadedbytesdonotmatchexpectedfilesize2 = (string)Application.Current.TryFindResource("DownloadincompleteDownloadedbytesdonotmatchexpectedfilesize") ?? "Download incomplete. Downloaded bytes do not match expected file size.";
                    DownloadStatus = downloadincompleteDownloadedbytesdonotmatchexpectedfilesize2;
                    throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
                }
                else
                {
                    // If we don't know the total size, assume it's complete
                    _isDownloadCompleted = true;
                    var downloadcomplete2 = (string)Application.Current.TryFindResource("Downloadcomplete") ?? "Download complete:";
                    DownloadStatus = $"{downloadcomplete2} {Math.Round((double)totalBytesRead / (1024 * 1024), 2)} MB [fallback]";
                }
            }
            catch (Exception fallbackEx)
            {
                // Notify developer
                // If fallback also fails, log and rethrow
                await LogErrors.LogErrorAsync(fallbackEx, "Fallback download method also failed: " + fallbackEx.Message);
                throw;
            }
            finally
            {
                // Dispose of the local client if we created one
                localClient?.Dispose();
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            var errorTherequestedfilewasnotfoundontheserver2 = (string)Application.Current.TryFindResource("ErrorTherequestedfilewasnotfoundontheserver") ?? "Error: The requested file was not found on the server.";
            DownloadStatus = errorTherequestedfilewasnotfoundontheserver2;

            // Notify developer
            var formattedException = $"The requested file was not available on the server.\n\n" +
                                     $"URL: {downloadUrl}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Rethrow to let caller handle it
            throw;
        }
        catch (HttpRequestException ex)
        {
            var errorNetworkerrorduringfiledownload2 = (string)Application.Current.TryFindResource("ErrorNetworkerrorduringfiledownload") ?? "Error: Network error during file download.";
            DownloadStatus = errorNetworkerrorduringfiledownload2;

            // Notify developer
            var formattedException = $"Network error during file download.\n\n" +
                                     $"URL: {downloadUrl}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Rethrow to let caller handle it
            throw;
        }
        catch (IOException ex)
        {
            var errorFilereadwriteerrorduringdownload2 = (string)Application.Current.TryFindResource("ErrorFilereadwriteerrorduringdownload") ?? "Error: File read/write error during download.";
            DownloadStatus = errorFilereadwriteerrorduringdownload2;

            // Notify developer
            var formattedException = $"File read/write error during file download.\n\n" +
                                     $"URL: {downloadUrl}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Rethrow to let caller handle it
            throw;
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested && _isUserCancellation)
            {
                var downloadcanceledbyuser2 = (string)Application.Current.TryFindResource("Downloadcanceledbyuser") ?? "Download canceled by user.";
                DownloadStatus = downloadcanceledbyuser2;

                // Notify the developer without showing the user message
                var formattedException = $"Download was canceled by the user.\n\n" +
                                         $"URL: {downloadUrl}\n" +
                                         $"Exception type: {ex.GetType().Name}\n" +
                                         $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
            }
            else
            {
                var errorDownloadtimedoutorwascanceledunexpectedly2 = (string)Application.Current.TryFindResource("ErrorDownloadtimedoutorwascanceledunexpectedly") ?? "Error: Download timed out or was canceled unexpectedly.";
                DownloadStatus = errorDownloadtimedoutorwascanceledunexpectedly2;

                // Notify developer
                var formattedException = $"Download timed out or was canceled unexpectedly.\n\n" +
                                         $"URL: {downloadUrl}\n" +
                                         $"Exception type: {ex.GetType().Name}\n" +
                                         $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
            }

            // Rethrow to let caller handle it
            throw;
        }
        catch (Exception ex)
        {
            var errorUnexpectederrorduringdownload2 = (string)Application.Current.TryFindResource("ErrorUnexpectederrorduringdownload") ?? "Error: Unexpected error during download.";
            DownloadStatus = errorUnexpectederrorduringdownload2;

            // Notify developer
            var formattedException = $"Generic download error.\n\n" +
                                     $"URL: {downloadUrl}\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Rethrow to let caller handle it
            throw;
        }
        finally
        {
            // Dispose of the local client if we created one
            if (localClient != null)
            {
                localClient.Dispose();
            }
        }
    }

    private EasyModeSystemConfig GetSelectedSystem()
    {
        return SystemNameDropdown.SelectedItem != null
            ? _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString())
            : null;
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            // Set flag to indicate this is a user-initiated cancellation
            _isUserCancellation = true;

            var cancelingdownload2 = (string)Application.Current.TryFindResource("Cancelingdownload") ?? "Canceling download...";
            DownloadStatus = cancelingdownload2;
            _cancellationTokenSource.Cancel(); // Cancel the ongoing download
            StopDownloadButton.IsEnabled = false; // Disable the stop button once the download is canceled

            // Reset completion flag and progress
            _isDownloadCompleted = false;
            DownloadProgressBar.Value = 0;

            // Reinitialize the cancellation token source for the next download
            _cancellationTokenSource = new CancellationTokenSource();

            // The relevant download button will be re-enabled in the finally block
            // of the download method when it catches the TaskCanceledException
        }
    }

    private void AddSystemButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return;

        // Determine the system folder to use
        var systemFolder = SystemFolderTextBox.Text;
        if (string.IsNullOrEmpty(systemFolder))
        {
            systemFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", selectedSystem.SystemName);
            SystemFolderTextBox.Text = systemFolder;
        }

        // Remove the leading dot from the SystemImageFolder for the message
        var systemImageFolderForMessage = selectedSystem.SystemImageFolder.TrimStart('.').TrimStart('\\', '/');

        // Combine with the base directory for the message
        var fullImageFolderPathForMessage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, systemImageFolderForMessage);

        // Path to the system.xml file
        var systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

        try
        {
            var addingsystemtoconfiguration2 = (string)Application.Current.TryFindResource("Addingsystemtoconfiguration") ?? "Adding system to configuration...";
            DownloadStatus = addingsystemtoconfiguration2;

            // Load existing system configurations
            var xmlDoc = XDocument.Load(systemXmlPath);
            var systemConfigs = xmlDoc.Descendants("SystemConfig").ToList();

            // Check if the system already exists
            var existingSystem = systemConfigs.FirstOrDefault(config => config.Element("SystemName")?.Value == selectedSystem.SystemName);
            if (existingSystem != null)
            {
                // Ask user if they want to overwrite the existing system
                if (MessageBoxLibrary.OverwriteSystemMessageBox(selectedSystem)) return;

                // Overwrite existing system
                existingSystem.SetElementValue("SystemName", selectedSystem.SystemName);
                existingSystem.SetElementValue("SystemFolder", systemFolder);
                existingSystem.SetElementValue("SystemImageFolder", selectedSystem.SystemImageFolder);
                existingSystem.SetElementValue("SystemIsMAME", selectedSystem.SystemIsMame.ToString());
                existingSystem.Element("FileFormatsToSearch")?.Remove();
                existingSystem.Add(new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(format => new XElement("FormatToSearch", format))));
                existingSystem.SetElementValue("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString());
                existingSystem.Element("FileFormatsToLaunch")?.Remove();
                existingSystem.Add(new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(format => new XElement("FormatToLaunch", format))));
                existingSystem.Element("Emulators")?.Remove();
                existingSystem.Add(new XElement("Emulators",
                    new XElement("Emulator",
                        new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                        new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                        new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                    )
                ));
            }
            else
            {
                // Create a new XElement for the selected system
                var newSystemElement = new XElement("SystemConfig",
                    new XElement("SystemName", selectedSystem.SystemName),
                    new XElement("SystemFolder", systemFolder),
                    new XElement("SystemImageFolder", selectedSystem.SystemImageFolder),
                    new XElement("SystemIsMAME", selectedSystem.SystemIsMame.ToString()),
                    new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(format => new XElement("FormatToSearch", format))),
                    new XElement("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString()),
                    new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(format => new XElement("FormatToLaunch", format))),
                    new XElement("Emulators",
                        new XElement("Emulator",
                            new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                            new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                            new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                        )
                    )
                );

                // Add the new system to the XML document
                xmlDoc.Root?.Add(newSystemElement);
            }

            // Sort the systems alphabetically by SystemName
            xmlDoc.Root?.ReplaceNodes(xmlDoc.Root.Elements("SystemConfig")
                .OrderBy(systemElement => systemElement.Element("SystemName")?.Value));

            // Save the updated XML document
            xmlDoc.Save(systemXmlPath);

            var creatingsystemfolders2 = (string)Application.Current.TryFindResource("Creatingsystemfolders") ?? "Creating system folders...";
            DownloadStatus = creatingsystemfolders2;

            // Create the necessary folders for the system
            CreateSystemFolders(selectedSystem.SystemName, systemFolder, fullImageFolderPathForMessage);

            var systemhasbeensuccessfullyadded2 = (string)Application.Current.TryFindResource("Systemhasbeensuccessfullyadded") ?? "System has been successfully added!";
            DownloadStatus = systemhasbeensuccessfullyadded2;

            // Notify user
            MessageBoxLibrary.SystemAddedMessageBox(systemFolder, fullImageFolderPathForMessage, selectedSystem);

            // Disable Add System Button
            AddSystemButton.IsEnabled = false;
        }
        catch (Exception ex)
        {
            var errorFailedtoaddsystem2 = (string)Application.Current.TryFindResource("ErrorFailedtoaddsystem") ?? "Error: Failed to add system.";
            DownloadStatus = errorFailedtoaddsystem2;

            // Notify developer
            var formattedException = $"Error adding system.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.AddSystemFailedMessageBox();
        }
    }

    private void UpdateAddSystemButtonState()
    {
        AddSystemButton.IsEnabled = _isEmulatorDownloaded && _isCoreDownloaded;
    }

    private static void CreateSystemFolders(string systemName, string systemFolder, string systemImageFolder)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Paths for the primary system folder and image folder
        var systemFolderPath = Path.GetFullPath(systemFolder);
        var imagesFolderPath = Path.Combine(baseDirectory, systemImageFolder);

        // List of additional folders to create
        string[] additionalFolders = ["roms", "images", "title_snapshots", "gameplay_snapshots", "videos", "manuals", "walkthrough", "cabinets", "carts", "flyers", "pcbs"];

        try
        {
            // Create the primary system folder if it doesn't exist
            if (!Directory.Exists(systemFolderPath))
            {
                Directory.CreateDirectory(systemFolderPath);
            }

            // Create the primary image folder if it doesn't exist
            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            // Create each additional folder
            foreach (var folder in additionalFolders)
            {
                var folderPath = Path.Combine(baseDirectory, folder, systemName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"The application failed to create the necessary folders for the newly added system.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.FolderCreationFailedMessageBox();

            throw;
        }
    }

    private void CloseWindowRoutine(object sender, EventArgs e)
    {
        _config = null;

        // Prepare the process start info
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule == null) return;

        var startInfo = new ProcessStartInfo
        {
            FileName = processModule.FileName,
            UseShellExecute = true
        };

        // Start the new application instance
        Process.Start(startInfo);

        // Shutdown the current application instance
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var chooseaFolderwithRoMsorIsOs2 = (string)Application.Current.TryFindResource("ChooseaFolderwithROMsorISOs") ?? "Choose a folder with 'ROMs' or 'ISOs' for this system";
        using var dialog = new FolderBrowserDialog();
        dialog.Description = chooseaFolderwithRoMsorIsOs2;
        dialog.ShowNewFolderButton = true;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SystemFolderTextBox.Text = dialog.SelectedPath;
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private static void TryDeleteFile(string file)
    {
        if (string.IsNullOrEmpty(file) || !File.Exists(file)) return;
        try
        {
            File.Delete(file);
        }
        catch (Exception)
        {
            // ignore
        }
    }

    private static string ChangeFileExtensionFunction(EasyModeSystemConfig selectedSystem, string downloadFilePath)
    {
        if (!selectedSystem.Emulators.Emulator.EmulatorDownloadRename) return downloadFilePath;

        var newFilePath = Path.ChangeExtension(downloadFilePath, ".7z");
        if (File.Exists(downloadFilePath) && !File.Exists(newFilePath))
        {
            try
            {
                File.Move(downloadFilePath, newFilePath);
                return newFilePath;
            }
            catch (Exception)
            {
                // ignore
            }
        }

        return downloadFilePath;
    }
}
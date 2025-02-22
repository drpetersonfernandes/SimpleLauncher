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

public partial class DownloadImagePack
{
    private readonly ExtractCompressedFile _extractCompressedFile = new();
    private EasyModeConfig _config;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _httpClient = new();
    private bool _isDownloadCompleted;
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

    public DownloadImagePack()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

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
        }
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset the flag at the start of the download
            _isDownloadCompleted = false;

            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem == null) return;
            var extrasDownloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;

            // Determine the extraction folder
            var extractionFolder = !string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text)
                ? ExtractionFolderTextBox.Text
                : selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;

            var downloadFilePath = Path.Combine(_tempFolder, Path.GetFileName(extrasDownloadUrl) ?? throw new InvalidOperationException("'Simple Launcher' could not get extrasDownloadUrl"));
            Directory.CreateDirectory(_tempFolder);

            try
            {
                // Display progress bar
                DownloadProgressBar.Visibility = Visibility.Visible;
                DownloadProgressBar.Value = 0;
                StopDownloadButton.IsEnabled = true;

                // Initialize cancellation token source
                _cancellationTokenSource = new CancellationTokenSource();

                await DownloadWithProgressAsync(extrasDownloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                // Only proceed with extraction if the download completed successfully
                if (_isDownloadCompleted)
                {
                    // Show the PleaseWaitExtraction window
                    var pleaseWaitWindow = new PleaseWaitExtraction();
                    pleaseWaitWindow.Show();

                    var extractionSuccess = await _extractCompressedFile.ExtractDownloadFilesAsync2(downloadFilePath, extractionFolder);

                    // Close the PleaseWaitExtraction window
                    pleaseWaitWindow.Close();

                    if (extractionSuccess)
                    {
                        // Notify user
                        MessageBoxLibrary.DownloadExtractionSuccessfullyMessageBox();

                        DeleteDownloadedFile(downloadFilePath);

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
                    }
                }
                else // Download fail
                {
                    // Notify developer
                    var formattedException = $"Image Pack download failed.\n\n" +
                                             $"File: {extrasDownloadUrl}";
                    var ex = new Exception(formattedException);
                    await LogErrors.LogErrorAsync(ex, formattedException);

                    // Notify user
                    MessageBoxLibrary.ImagePackDownloadExtractionFailedMessageBox();
                }
            }
            catch (TaskCanceledException)
            {
                DeleteDownloadedFile(downloadFilePath);

                // Notify user
                MessageBoxLibrary.DownloadCanceledMessageBox();
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
            }
            finally
            {
                StopDownloadButton.IsEnabled = false;
                DeleteDownloadedFile(downloadFilePath);
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
        }
    }

    private static void DeleteDownloadedFile(string file)
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

    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 8192, true);
            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalBytesRead += bytesRead;

                if (totalBytes.HasValue)
                {
                    DownloadProgressBar.Value = (double)totalBytesRead / totalBytes.Value * 100;
                }
            }

            // Check if the file was fully downloaded
            if (totalBytesRead == totalBytes)
            {
                _isDownloadCompleted = true;
            }
            else
            {
                _isDownloadCompleted = false;
                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
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
            }

            DeleteDownloadedFile(destinationPath);
        }
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

        // Reinitialize the cancellation token source for the next download
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void EditSystemEasyModeAddSystem_Closed(object sender, EventArgs e)
    {
        // Empty EasyMode Config
        _config = null;
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
}
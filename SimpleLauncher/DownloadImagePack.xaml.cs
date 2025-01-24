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
using MessageBox = System.Windows.MessageBox;

namespace SimpleLauncher;

public partial class DownloadImagePack
{
    private EasyModeConfig _config;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _httpClient = new();
    private bool _isDownloadCompleted;
    private readonly string _tempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        
    public DownloadImagePack()
    {
        InitializeComponent();
          
        App.ApplyThemeToWindow(this);
        _config = EasyModeConfig.Load();
        PopulateSystemDropdown();
            
        // Subscribe to the Closed event
        Closed += EditSystemEasyModeAddSystem_Closed;

        string someantivirusprogramsmaylock2 = (string)Application.Current.TryFindResource("Someantivirusprogramsmaylock") ?? "Some antivirus programs may lock or prevent the extraction of newly downloaded files, causing access issues during installation.";
        string ifyouencountererrors2 = (string)Application.Current.TryFindResource("Ifyouencountererrors") ?? "If you encounter errors, try temporarily disabling real-time protection and run 'Simple Launcher' with administrative privileges.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{someantivirusprogramsmaylock2}\n\n" +
                        $"{ifyouencountererrors2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void PopulateSystemDropdown()
    {
        if (_config?.Systems != null)
        {
            // Filter systems that have a valid ExtrasDownloadLink
            var systemsWithImagePacks = _config.Systems
                .Where(system => !string.IsNullOrEmpty(system.Emulators.Emulator.ExtrasDownloadLink))
                .Select(system => system.SystemName)
                .OrderBy(name => name)
                .ToList();

            SystemNameDropdown.ItemsSource = systemsWithImagePacks;
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem != null)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadLink);
            }
        }
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset the flag at the start of the download
            _isDownloadCompleted = false;

            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string extrasDownloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;
                
                // Determine the extraction folder
                string extractionFolder = !string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text)
                    ? ExtractionFolderTextBox.Text
                    : selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;
                
                string downloadFilePath = Path.Combine(_tempFolder, Path.GetFileName(extrasDownloadUrl) ?? throw new InvalidOperationException("'Simple Launcher' could not get extrasDownloadUrl"));
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
                        PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
                        pleaseWaitWindow.Show();

                        bool extractionSuccess = await ExtractCompressedFile.Instance2.ExtractDownloadFilesAsync2(downloadFilePath, extractionFolder);
                        
                        // Close the PleaseWaitExtraction window
                        pleaseWaitWindow.Close();

                        if (extractionSuccess)
                        {
                            string imagepackfor2 = (string)Application.Current.TryFindResource("Imagepackfor") ?? "Image pack for";
                            string downloadedandextracted2 = (string)Application.Current.TryFindResource("downloadedandextracted") ?? "downloaded and extracted successfully.";
                            string downloadComplete2 = (string)Application.Current.TryFindResource("DownloadComplete") ?? "Download Complete";
                            MessageBox.Show($"{imagepackfor2} {selectedSystem.SystemName} {downloadedandextracted2}",
                                downloadComplete2, MessageBoxButton.OK, MessageBoxImage.Information);
                                
                            DeleteDownloadedFile(downloadFilePath);
               
                            // Mark as downloaded and disable button
                            DownloadExtrasButton.IsEnabled = false;
                        }
                        else // Extraction fail
                        {
                            // Notify developer
                            string formattedException = $"Image Pack extraction failed.\n\n" +
                                                        $"File: {extrasDownloadUrl}";
                            Exception ex = new Exception(formattedException);
                            await LogErrors.LogErrorAsync(ex, formattedException);
                            
                            string imagePackextractionfailed2 = (string)Application.Current.TryFindResource("ImagePackextractionfailed") ?? "Image Pack extraction failed!";
                            string grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
                            string ensuretheSimpleLauncher2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
                            string temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirussoftware") ?? "Temporarily disable your antivirus software and try again.";
                            string extractionFailed2 = (string)Application.Current.TryFindResource("ExtractionFailed") ?? "Extraction Failed";
                            MessageBox.Show($"{imagePackextractionfailed2}\n\n" +
                                            $"{grantSimpleLauncheradministrative2}\n\n" +
                                            $"{ensuretheSimpleLauncher2}\n\n" +
                                            $"{temporarilydisableyourantivirussoftware2}",
                                extractionFailed2, MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else // Download fail
                    {
                        // Notify developer
                        string formattedException = $"Image Pack download failed.\n\n" +
                                                    $"File: {extrasDownloadUrl}";
                        Exception ex = new Exception(formattedException);
                        await LogErrors.LogErrorAsync(ex, formattedException);

                        string imagePackdownloadfailed2 = (string)Application.Current.TryFindResource("ImagePackdownloadfailed") ?? "Image Pack download failed!";
                        string grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
                        string ensuretheSimpleLauncher2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
                        string temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirussoftware") ?? "Temporarily disable your antivirus software and try again.";
                        string downloadFailed2 = (string)Application.Current.TryFindResource("DownloadFailed") ?? "Download Failed";
                        MessageBox.Show($"{imagePackdownloadfailed2}\n\n" +
                                        $"{grantSimpleLauncheradministrative2}\n\n" +
                                        $"{ensuretheSimpleLauncher2}\n\n" +
                                        $"{temporarilydisableyourantivirussoftware2}",
                            downloadFailed2, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (TaskCanceledException)
                {
                    DeleteDownloadedFile(downloadFilePath);
                    
                    string imagePackdownloadwascanceled2 = (string)Application.Current.TryFindResource("ImagePackdownloadwascanceled") ?? "Image Pack download was canceled.";
                    string downloadCanceled2 = (string)Application.Current.TryFindResource("DownloadCanceled") ?? "Download Canceled";
                    MessageBox.Show(imagePackdownloadwascanceled2,
                        downloadCanceled2, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    string formattedException = $"Error downloading the Image Pack.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\n" +
                                                $"Exception details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, formattedException);

                    string errordownloadingtheImagePack2 = (string)Application.Current.TryFindResource("ErrordownloadingtheImagePack") ?? "Error downloading the Image Pack.";
                    string wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
                    string downloadError2 = (string)Application.Current.TryFindResource("DownloadError") ?? "Download Error";
                    MessageBoxResult result = MessageBox.Show($"{errordownloadingtheImagePack2}\n\n" +
                                                              $"{wouldyouliketoberedirected2}",
                        downloadError2, MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                            UseShellExecute = true
                        });
                    }
                }
                finally
                {
                    StopDownloadButton.IsEnabled = false;
                    DeleteDownloadedFile(downloadFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"Error downloading the Image Pack.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
            
            MessageBox.Show("Image Pack download or extraction failed!\n\n" +
                            "Grant 'Simple Launcher' administrative access and try again.\n\n" +
                            "Ensure the 'Simple Launcher' folder is a writable directory.\n\n" +
                            "Temporarily disable your antivirus software and try again.",
                "Download or Extraction Failed", MessageBoxButton.OK, MessageBoxImage.Information);  
        }

        void DeleteDownloadedFile(string downloadFilePath)
        {
            if (File.Exists(downloadFilePath))
            {
                try
                {
                    File.Delete(downloadFilePath);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
    }

    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;

                if (totalBytes.HasValue)
                {
                    DownloadProgressBar.Value = (double)totalBytesRead / totalBytes.Value * 100;
                }
            }

            // Check if the file was fully downloaded
            if (totalBytes.HasValue && totalBytesRead == totalBytes.Value)
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
            string formattedException = $"The requested file was not available on the server.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show("The requested file is not available on the server.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (HttpRequestException ex)
        {
            string formattedException = $"Network error during file download.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show("There was a network error either with your internet access or the server.\n\n" +
                            "Please try again later.",
                "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (IOException ex)
        {
            string formattedException = $"File read/write error after file download.\n\n" +
                                        $"URL: {downloadUrl}\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            IoExceptionMessageBox();
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                DeleteDownloadedFile();

                string formattedException = $"Download was canceled by the user. User was not notified.\n\n" +
                                            $"URL: {downloadUrl}\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
            }
            else
            {
                DeleteDownloadedFile();
                    
                string formattedException = $"Download timed out or was canceled unexpectedly.\n\n" +
                                            $"URL: {downloadUrl}\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                    
                MessageBox.Show("Download timed out or was canceled unexpectedly.\n\n" +
                                "You can try again later.",
                    "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void DeleteDownloadedFile()
        {
            // If user canceled, delete the partially downloaded file
            if (File.Exists(destinationPath))
            {
                try
                {
                    File.Delete(destinationPath);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }

        void IoExceptionMessageBox()
        {
            var result = MessageBox.Show("A file read/write error occurred after the file was downloaded.\n\n" +
                                         "This error may occur if an antivirus program is locking or scanning the newly downloaded files, causing access issues. Try temporarily disabling real-time protection.\n\n" +
                                         "Additionally, grant 'Simple Launcher' administrative access to enable file writing.\n\n" +
                                         "Make sure the 'Simple Launcher' folder is located in a writable directory.\n\n" +
                                         "Would you like to open the 'temp' folder to view the downloaded file?",
                "Read/Write Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _tempFolder,
                        UseShellExecute = true
                    });
                }
                catch (Exception)
                {
                    MessageBox.Show("'Simple Launcher' was unable to open the 'temp' folder due to access issues.\n\n" +
                                    $"{_tempFolder}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel(); // Cancel the ongoing download
            StopDownloadButton.IsEnabled = false; // Disable the stop button once the download is canceled

            // Reset completion flag and progress
            _isDownloadCompleted = false; 
            DownloadProgressBar.Value = 0;
                
            // Reinitialize the cancellation token source for the next download
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    private void EditSystemEasyModeAddSystem_Closed(object sender, EventArgs e)
    {
        _config = null;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void ChooseExtractionFolderButton_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog();
        dialog.Description = @"Select a folder to extract the Image Pack";
        dialog.UseDescriptionForTitle = true;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            ExtractionFolderTextBox.Text = dialog.SelectedPath;
        }
    }
}